#!/usr/bin/env python3
import math
import matplotlib.pyplot as plt

HEADER      = 0xFB
BATCH_SIZE  = 8
BATCH_COUNT = 128

BATCH_MS           = 25    # 25 ms per batch
ACTIVE_BATCHES     = 40    # first 40 batches have data
FRAME_STEP_BATCHES = 10    # 250 ms shift between frames

# ---- signal + period ----
FREQ_SINE     = 0.05       # 1/20 Hz
FREQ_COSINE   = 0.10       # 1/10 Hz
AMPLITUDE     = 10000
OFFSET        = 0
PHASE_RAD     = 0.0

PERIOD_SECONDS  = 20.0
PERIOD_SAMPLES  = int(round(PERIOD_SECONDS * 1000.0 / BATCH_MS))  # = 800
TOTAL_SAMPLES   = PERIOD_SAMPLES                                  # indices 0..799

OUT_SINE_COSINE = "sine_cosine_overlap_loop_800_pretty.txt"
OUT_SINE = "sine_overlap_loop_800_pretty.txt"


def put_le_i32(buf, idx, v):
    v32 = v & 0xFFFFFFFF
    buf[idx + 0] = (v32 >> 0) & 0xFF
    buf[idx + 1] = (v32 >> 8) & 0xFF
    buf[idx + 2] = (v32 >> 16) & 0xFF
    buf[idx + 3] = (v32 >> 24) & 0xFF


def put_le_i16(buf, idx, v):
    v16 = v & 0xFFFF
    buf[idx + 0] = (v16 >> 0) & 0xFF
    buf[idx + 1] = (v16 >> 8) & 0xFF


def get_le_i32(buf, idx):
    v = (buf[idx + 0]
         | (buf[idx + 1] << 8)
         | (buf[idx + 2] << 16)
         | (buf[idx + 3] << 24))
    if v & 0x80000000:
        v -= 0x100000000
    return v


def checksum7(b):
    s = 0
    for k in range(7):
        s = (s + b[k]) & 0xFF
    return s


def batch_bytes_as_hex_line(batch_bytes):
    return " ".join(f"{x:02x}" for x in batch_bytes)


# -------- build global signal (0..799) --------

def build_global_sine_cosine():
    vals = []
    for k in range(TOTAL_SAMPLES):
        t = k * (BATCH_MS / 1000.0)
        v = (
            OFFSET + AMPLITUDE * math.sin(2.0 * math.pi * FREQ_SINE   * t + PHASE_RAD) +
            OFFSET + AMPLITUDE * math.cos(2.0 * math.pi * FREQ_COSINE * t + PHASE_RAD)
        )
        vals.append(int(round(v)))
    return vals


def build_sine():
    vals = []
    for k in range(TOTAL_SAMPLES):
        t = k * (BATCH_MS / 1000.0)
        v = (
            OFFSET + AMPLITUDE * math.sin(2.0 * math.pi * FREQ_SINE   * t + PHASE_RAD)
        )
        vals.append(int(round(v)))
    return vals

# -------- framing --------

def build_frame_batches(start_batch_index: int, values):
    total_samples = len(values)
    batches = [bytearray(BATCH_SIZE) for _ in range(BATCH_COUNT)]

    for i in range(BATCH_COUNT):
        b = batches[i]
        b[0] = HEADER

        g = start_batch_index + i  # global index

        if i < ACTIVE_BATCHES and g < total_samples:
            put_le_i32(b, 1, values[g])
            put_le_i16(b, 5, g)
        # else: [1..6] stay 0

        b[7] = checksum7(b)

    return batches


def write_pretty_file(path: str, values):
    total_samples = len(values)
    frame_idx = 0
    with open(path, "w", encoding="utf-8") as f:
        while True:
            start_batch = frame_idx * FRAME_STEP_BATCHES
            if start_batch >= total_samples:
                break
            batches = build_frame_batches(start_batch, values)
            f.write(f"FRAME {frame_idx:04d}\n")
            for i, b in enumerate(batches):
                f.write(f"  {i:03d}: {batch_bytes_as_hex_line(b)}\n")
            frame_idx += 1


# -------- decode back from bytes to double-check --------

def decode_signal_from_bytes(values):
    total_samples = len(values)
    samples = {}

    frame_idx = 0
    while True:
        start_batch = frame_idx * FRAME_STEP_BATCHES
        if start_batch >= total_samples:
            break

        batches = build_frame_batches(start_batch, values)

        for i in range(ACTIVE_BATCHES):
            g = start_batch + i
            if g >= total_samples:
                continue
            if g not in samples:
                b = batches[i]
                samples[g] = get_le_i32(b, 1)

        frame_idx += 1

    xs, ys = [], []
    for k in range(total_samples):
        if k in samples:
            xs.append(k * (BATCH_MS / 1000.0))
            ys.append(samples[k])
    return xs, ys


def plot_and_print_loop_info(values):
    t, y = decode_signal_from_bytes(values)

    print(f"Decoded samples: {len(y)}")
    print(f"value[0]   = {y[0]}")
    print(f"value[1]   = {y[1]}")
    print(f"value[799] = {y[799]}")

    d_normal = y[1]   - y[0]
    d_wrap   = y[0]   - y[799]
    print(f"Δ normal (1-0)   = {d_normal}")
    print(f"Δ wrap   (0-799) = {d_wrap}")

    plt.figure()
    plt.plot(t, y)
    plt.xlabel("time (s)")
    plt.ylabel("decoded int32 value")
    plt.grid(True)
    plt.title("Looped sine+cosine (800-sample period)")
    plt.tight_layout()
    plt.show()


def main():
    print("Generating 800-sample loop-safe sine+cosine trajectory")

    global_mix = build_global_sine_cosine()

    sine  = build_sine()

    print(f"TOTAL_SAMPLES = {len(global_mix)}")
    print(f"mix[0]        = {global_mix[0]}")
    print(f"mix[1]        = {global_mix[1]}")
    print(f"mix[799]      = {global_mix[799]}")
    print(f"mix[800] (math check, not in file) would equal mix[0].")

    write_pretty_file(OUT_SINE, sine)
    print(f"-> {OUT_SINE}")

    write_pretty_file(OUT_SINE_COSINE, global_mix)
    print(f"-> {OUT_SINE_COSINE}")

    plot_and_print_loop_info(global_mix)
    print("Done.")


if __name__ == "__main__":
    main()
