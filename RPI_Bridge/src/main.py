#!/usr/bin/env python3
import time, os, yaml, zlib
from common.logging_setup import setup_logging
from common.framing import RAW_SIZE, build_raw64, verify_raw64
from lan.buffer_client import BufferClient
from lan.file_server import FileServer
from uart.uart_worker import UartWorker

HEADER_1024  = 0xFB
BATCH_SIZE   = 8
BATCH_COUNT  = 128
FRAME_1024   = BATCH_SIZE * BATCH_COUNT  # 1024

def verify_1024(frame: bytes) -> bool:
    if len(frame) != FRAME_1024: return False
    for i in range(BATCH_COUNT):
        base = i * BATCH_SIZE
        if frame[base] != HEADER_1024: return False
        s = 0
        for k in range(BATCH_SIZE - 1):
            s = (s + frame[base + k]) & 0xFF
        if s != frame[base + (BATCH_SIZE - 1)]: return False
    return True

def main():
    setup_logging("INFO")
    cfg = yaml.safe_load(open("../config/app.yaml"))
    # UART
    uart = UartWorker(
        port=cfg["serial"]["port"],
        baud=cfg["serial"]["baud"],
        rtscts=cfg["serial"]["rtscts"],
        rx_qmax=cfg["timing"]["rx_queue_max"],
        tx_qmax=cfg["timing"]["tx_queue_max"],
        tx_gap_ms=cfg["timing"]["uart_tx_gap_ms"],
    )
    uart.start()

    # 64B Buffer client (RPi -> PC)
    buf = BufferClient(cfg["network"]["buffer_server_ip"], cfg["network"]["buffer_server_port"])
    buf.start()

    # File server (PC -> RPi)
    files = FileServer(cfg["network"]["file_server_ip"], cfg["network"]["file_server_port"],
                       cfg["paths"]["incoming_dir"])
    files.start()

    last_uart_crc = None
    last_buf_crc = None
    last_file_seen = None

    print("[MAIN] Running: PC buffer server @5001, RPi file server @5002, UART active.")
    try:
        while True:
            # UART RX -> (optionally) update 64B status to PC
            rx = uart.get_rx_nowait()
            if rx and verify_1024(rx):
                c = zlib.crc32(rx)
                if c != last_uart_crc:
                    last_uart_crc = c
                    # Example: push tiny status to PC (first 8 bytes of frame)
                    payload = rx[:8]
                    buf.Tx_Frame[:] = build_raw64(payload)
                    buf.send_once()

            # If PC sends 64B command, you can translate to UART frame(s)
            frame64 = bytes(buf.Rx_Frame)
            if verify_raw64(frame64):
                c2 = zlib.crc32(frame64)
                if c2 != last_buf_crc:
                    last_buf_crc = c2
                    # demo: expand 64B into one 1024B echo frame (toy)
                    out = bytearray([HEADER_1024,0,0,0,0,0,0,0] * BATCH_COUNT)
                    # copy a few bytes into payload region if needed
                    uart.put_tx(bytes(out))

            # New file arrived -> stream to UART (paced)
            if files.last_file_path and files.last_file_path != last_file_seen:
                p = files.last_file_path
                last_file_seen = p
                print(f"[MAIN] New file: {os.path.basename(p)} ({files.last_file_size} B)")
                with open(p, "rb") as f:
                    while True:
                        chunk = f.read(1024)
                        if not chunk: break
                        if len(chunk) < 1024:
                            chunk = chunk + b"\x00" * (1024 - len(chunk))
                        # stamp header/checksum per 8-byte batch
                        ba = bytearray(chunk)
                        for i in range(BATCH_COUNT):
                            base = i * BATCH_SIZE
                            ba[base] = HEADER_1024
                            s = 0
                            for k in range(BATCH_SIZE - 1):
                                s = (s + ba[base + k]) & 0xFF
                            ba[base + (BATCH_SIZE - 1)] = s & 0xFF
                        uart.put_tx(bytes(ba))
                # (optional) clear files.last_file_path if you want one-shot handling
            time.sleep(0.01)
    except KeyboardInterrupt:
        print("\n[MAIN] Stopping...")
    finally:
        files.stop(); buf.stop(); uart.stop()

if __name__ == "__main__":
    main()
