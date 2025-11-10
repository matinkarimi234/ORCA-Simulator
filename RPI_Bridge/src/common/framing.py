# 64-byte RAW framing helpers
RAW_SIZE = 64
RAW_HEADER = 0xFB
BATCH_SIZE   = 8
BATCH_COUNT  = 128

def build_raw64(payload62: bytes) -> bytes:
    if payload62 is None: payload62 = b""
    if len(payload62) > 62: payload62 = payload62[:62]
    frame = bytearray(RAW_SIZE)
    frame[0] = RAW_HEADER
    frame[1:1+len(payload62)] = payload62
    s = 0
    for i in range(RAW_SIZE - 1):
        s = (s + frame[i]) & 0xFF
    frame[RAW_SIZE - 1] = s
    return bytes(frame)

def verify_raw64(frame: bytes) -> bool:
    if len(frame) != RAW_SIZE: return False
    if frame[0] != RAW_HEADER: return False
    s = 0
    for i in range(RAW_SIZE - 1):
        s = (s + frame[i]) & 0xFF
    return s == frame[RAW_SIZE - 1]
