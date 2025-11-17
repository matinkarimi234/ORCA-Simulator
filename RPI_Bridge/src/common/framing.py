RAW_SIZE    = 64
RAW_HEADER  = 0xFB
BATCH_SIZE  = 8
BATCH_COUNT = 128
RAW_COUNT = RAW_SIZE // BATCH_SIZE
BATCH_DATA  = BATCH_SIZE - 2         # 6 data bytes per batch


def build_raw64(payload: bytes) -> bytes:
    """
    Build a 64-byte frame made of 8-byte batches.
    Each 8-byte batch: [HEADER, D0, D1, D2, D3, D4, D5, CHK]
    CHK = sum(bytes[0..6]) & 0xFF
    Max payload = 48 bytes (8 * 6). Shorter payloads are zero-padded.
    """
    if payload is None:
        payload = b""

    # Truncate to max payload capacity (48 bytes)
    if len(payload) > RAW_COUNT * BATCH_DATA:
        payload = payload[:RAW_COUNT * BATCH_DATA]

    frame = bytearray(RAW_SIZE)
    p_idx = 0  # index into payload

    for batch in range(RAW_COUNT):
        base = batch * BATCH_SIZE

        # Header
        frame[base + 0] = RAW_HEADER

        # Data bytes (up to 6 per batch)
        for k in range(BATCH_DATA):
            if p_idx < len(payload):
                frame[base + 1 + k] = payload[p_idx]
                p_idx += 1
            else:
                frame[base + 1 + k] = 0  # pad with zeros

        # Per-batch checksum over bytes[0..6] of this batch
        s = 0
        for k in range(7):  # header + 6 data bytes
            s = (s + frame[base + k]) & 0xFF
        frame[base + 7] = s

    return bytes(frame)


def verify_raw64(frame: bytes) -> bool:
    """
    Verify a 64-byte frame built by build_raw64.
    Checks:
      - length = 64
      - per-batch header == RAW_HEADER
      - per-batch checksum matches sum(bytes[0..6]) & 0xFF
    """
    if len(frame) != RAW_SIZE:
        return False

    # Check each 8-byte batch
    for batch in range(RAW_COUNT):
        base = batch * BATCH_SIZE

        # Header
        if frame[base + 0] != RAW_HEADER:
            return False

        # Checksum
        s = 0
        for k in range(7):
            s = (s + frame[base + k]) & 0xFF
        if frame[base + 7] != s:
            return False

    return True


