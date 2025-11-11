#!/usr/bin/env python3
"""
Generate pretty-formatted overlapping moving-window files
for sine and cosine (also supports ramp mode).

Output format:
FRAME 0000
  000: fb xx xx xx xx 00 00 cc
  ...
  127: fb 00 00 00 00 00 00 fb

Specs
-----
- 1024 bytes per frame (128 batches × 8 bytes)
- Batch layout: [0]=0xFB, [1..4]=LE int32 signal (only for first 40 batches),
  [5]=0x00, [6]=0x00, [7]=checksum(sum of [0..6] mod 256)
- Active batches per frame: 40 (1.000 s at 25 ms per batch)
- Overlap: next frame starts 10 batches later (250 ms)
- Duration: 5 minutes (300 s)
- Signals:
    - ramp: val = (global_batch_index * ramp_step)
    - sine: val = round(offset + amplitude * sin(2π f t + phase))
    - cosine: val = round(offset + amplitude * cos(2π f t + phase))
- All signals are written as little-endian 32-bit integers.
"""

import math

HEADER = 0xFB
BATCH_SIZE = 8
BATCH_COUNT = 128
FRAME_SIZE = BATCH_SIZE * BATCH_COUNT

BATCH_MS = 25                    # 25 ms per batch
ACTIVE_BATCHES = 40              # first 40 batches are filled with the signal
FRAME_STEP_BATCHES = 10          # 250 ms shift between frames

TOTAL_SECONDS = 300              # 5 minutes total
TOTAL_BATCHES = (TOTAL_SECONDS * 1000) // BATCH_MS  # 12000
TOTAL_FRAMES = (TOTAL_BATCHES - ACTIVE_BATCHES) // FRAME_STEP_BATCHES + 1  # 1200

# ---------- knobs ----------
PER_FRAME_RESTART = False  # False = global continuity; True = restart per frame
# Ramp settings (if you ever use ramp mode here)
RAMP_STEP = 100            # 0, 100, 200, ... (per batch)
# Sine/Cosine settings
FREQ_HZ = 0.05              # 0.05 Hz signal
AMPLITUDE = 10000          # peak amplitude in integer units
OFFSET = 0                 # DC offset in integer units
PHASE_RAD = 0.0            # phase in radians
# Output file names
OUT_SINE   = "sine_overlap_5min_pretty.txt"
OUT_COSINE = "cosine_overlap_5min_pretty.txt"
# ---------------------------

def put_le_i32(buf, idx, v):
    """Write signed int32 little-endian (two's complement) into buf[idx:idx+4]."""
    v32 = v & 0xFFFFFFFF
    buf[idx + 0] = (v32 >> 0) & 0xFF
    buf[idx + 1] = (v32 >> 8) & 0xFF
    buf[idx + 2] = (v32 >> 16) & 0xFF
    buf[idx + 3] = (v32 >> 24) & 0xFF

def put_le_i16(buf, idx, v):

    v16 = v & 0xFFFF
    buf[idx + 0] = (v16 >> 0) & 0xFF
    buf[idx + 1] = (v16 >> 8) & 0xFF

def checksum7(b):
    s = 0
    for k in range(7):
        s = (s + b[k]) & 0xFF
    return s

def batch_bytes_as_hex_line(batch_bytes):
    return " ".join(f"{x:02x}" for x in batch_bytes)

def signal_ramp(global_batch_index, i_in_frame):
    if PER_FRAME_RESTART:
        base = 0
    else:
        base = global_batch_index
    return base * RAMP_STEP + i_in_frame * RAMP_STEP

def signal_sine(global_batch_index, i_in_frame):
    # time per batch
    t = (global_batch_index + i_in_frame) * (BATCH_MS / 1000.0) if not PER_FRAME_RESTART else i_in_frame * (BATCH_MS / 1000.0)
    val = OFFSET + AMPLITUDE * math.sin(2.0 * math.pi * FREQ_HZ * t + PHASE_RAD)
    return int(round(val))

def signal_cosine(global_batch_index, i_in_frame):
    t = (global_batch_index + i_in_frame) * (BATCH_MS / 1000.0) if not PER_FRAME_RESTART else i_in_frame * (BATCH_MS / 1000.0)
    val = OFFSET + AMPLITUDE * math.cos(2.0 * math.pi * FREQ_HZ * t + PHASE_RAD)
    return int(round(val))

def build_frame_batches(start_batch_index, value_fn):
    """
    Returns list of 128 bytearrays (each 8B) for the frame starting at start_batch_index.
    value_fn(global_batch_index, i_in_frame)->int
    """
    batches = [bytearray(BATCH_SIZE) for _ in range(BATCH_COUNT)]
    for i in range(BATCH_COUNT):
        b = batches[i]
        b[0] = HEADER

        if i < ACTIVE_BATCHES:
            # compute int32 value and write little-endian
            val = value_fn(start_batch_index, i)
            put_le_i32(b, 1, val)
            put_le_i16(b, 5, i + start_batch_index)
        # else last 88 are empty with zeros in [1..6]

        b[7] = checksum7(b)

    return batches

def write_pretty_file(path, value_fn):
    with open(path, "w", encoding="utf-8") as f:
        for frame_idx in range(TOTAL_FRAMES):
            start_batch = frame_idx * FRAME_STEP_BATCHES
            batches = build_frame_batches(start_batch, value_fn)
            f.write(f"FRAME {frame_idx:04d}\n")
            for i, b in enumerate(batches):
                f.write(f"  {i:03d}: {batch_bytes_as_hex_line(b)}\n")

def main():
    print(f"Generating 5-minute overlapping files (PER_FRAME_RESTART={PER_FRAME_RESTART})")
    print(f"  sine  : freq={FREQ_HZ} Hz, amp={AMPLITUDE}, offset={OFFSET}, phase={PHASE_RAD} rad")
    write_pretty_file(OUT_SINE,   signal_sine)
    print(f"  -> {OUT_SINE}")
    print(f"  cosine: freq={FREQ_HZ} Hz, amp={AMPLITUDE}, offset={OFFSET}, phase={PHASE_RAD} rad")
    write_pretty_file(OUT_COSINE, signal_cosine)
    print(f"  -> {OUT_COSINE}")
    print("Done.")

if __name__ == "__main__":
    main()
