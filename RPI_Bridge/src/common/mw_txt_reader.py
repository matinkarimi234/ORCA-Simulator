# rpi/src/common/mw_txt_reader.py
from typing import  List
from common.framing import BATCH_COUNT, BATCH_SIZE

FRAME_SIZE   = BATCH_SIZE * BATCH_COUNT

def _parse_batch_line(line: str) -> bytes:
    # "  012: fb 00 00 00 00 00 00 fb"
    _, hex_part = line.split(":", 1)
    hs = hex_part.strip().split()
    if len(hs) != BATCH_SIZE:
        raise ValueError(f"Expected {BATCH_SIZE} hex bytes, got {len(hs)}")
    return bytes(int(x, 16) for x in hs)

def load_frames_from_txt(txt_path: str) -> List[bytes]:
    frames: List[bytes] = []
    cur = bytearray()
    with open(txt_path, "r", encoding="utf-8", errors="ignore") as f:
        for raw in f:
            line = raw.strip("\n")
            if not line:
                continue
            if line.startswith("FRAME "):
                if cur:
                    if len(cur) != FRAME_SIZE:
                        raise ValueError(f"Incomplete frame({len(cur)})")
                    frames.append(bytes(cur))
                    cur.clear()
                continue
            if ":" in line:
                b = _parse_batch_line(line)
                cur += b
        if cur:
            if len(cur) != FRAME_SIZE:
                raise ValueError(f"Incomplete frame({len(cur)}) at EOF")
            frames.append(bytes(cur))
    return frames