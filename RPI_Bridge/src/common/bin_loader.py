# rpi/src/common/bin_loader.py
from typing import List
from common.framing import BATCH_COUNT , BATCH_SIZE

FRAME_SIZE = BATCH_COUNT * BATCH_SIZE

def load_frames_from_bin(path: str) -> List[bytes]:
    data = open(path, "rb").read()
    if len(data) % FRAME_SIZE != 0:
        raise ValueError(f"BIN size {len(data)} not multiple of {FRAME_SIZE}")
    return [bytes(data[i:i+FRAME_SIZE]) for i in range(0, len(data), FRAME_SIZE)]
