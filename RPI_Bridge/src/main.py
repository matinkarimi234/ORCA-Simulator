#!/usr/bin/env python3
import os, time, yaml, zlib
from common.logging_setup import setup_logging
from common.framing import BATCH_SIZE, BATCH_COUNT   # 8, 128
from lan.buffer_client import BufferClient
from lan.file_server import FileServer
from uart.uart_worker import UartWorker

HEADER_1024  = 0xFB
FRAME_1024   = BATCH_SIZE * BATCH_COUNT  # 1024 bytes

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

    # --- UART worker (handles serial in its own thread) ---
    uart = UartWorker(
        port=cfg["serial"]["port"],
        baud=cfg["serial"]["baud"],
        rtscts=cfg["serial"]["rtscts"],
        rx_qmax=cfg["timing"]["rx_queue_max"],
        tx_qmax=cfg["timing"]["tx_queue_max"],
        tx_gap_ms=cfg["timing"]["uart_tx_gap_ms"],
    )
    uart.start()

    # --- 1024B TCP client to C# BufferServer ---
    net = BufferClient(
        server_ip=cfg["network"]["buffer_server_ip"],
        server_port=cfg["network"]["buffer_server_port"],
        frame_size=FRAME_1024,
        verify_fn=verify_1024
    )
    net.start()

    # --- File server stays as-is (if you need it) ---
    files = FileServer(
        cfg["network"]["file_server_ip"],
        cfg["network"]["file_server_port"],
        cfg["paths"]["incoming_dir"]
    )
    files.start()

    last_uart_crc = None
    last_pc_crc   = None
    last_file_seen = None

    print("[MAIN] Running: PC 1024B buffer server @%d, RPi file server @%d, UART active."
          % (cfg["network"]["buffer_server_port"], cfg["network"]["file_server_port"]))

    try:
        while True:
            # 1) UART -> PC (forward full 1024B frames AS-IS)
            frm = uart.get_rx_nowait()
            if frm and verify_1024(frm):
                c = zlib.crc32(frm)
                if c != last_uart_crc:
                    last_uart_crc = c
                    net.send_frame(frm)

            # 2) (optional) PC -> UART (if you want to inject frames from PC)
            pc = bytes(net.last_frame)
            if verify_1024(pc):
                c2 = zlib.crc32(pc)
                if c2 != last_pc_crc:
                    last_pc_crc = c2
                    # Uncomment if you want PC to drive the uC:
                    # uart.put_tx(pc)

            # 3) (optional) file->UART handling left as-is or deferred
            if files.last_file_path and files.last_file_path != last_file_seen:
                last_file_seen = files.last_file_path
                print(f"[MAIN] New file: {os.path.basename(last_file_seen)} ({files.last_file_size} B)")
                # stream to UART in a separate thread if needed

            time.sleep(0.01)
    except KeyboardInterrupt:
        print("\n[MAIN] Stopping...")
    finally:
        files.stop(); net.stop(); uart.stop()

if __name__ == "__main__":
    main()
