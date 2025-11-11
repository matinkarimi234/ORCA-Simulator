# rpi/src/main.py
#!/usr/bin/env python3
import os, time, yaml, zlib
from common.logging_setup import setup_logging
from common.framing import BATCH_SIZE, BATCH_COUNT   # 8, 128
from lan.buffer_client import BufferClient
from lan.file_server import FileServer
from uart.uart_worker import UartWorker

from workers.frame_on_uart_rx import FrameOnUartRxFeeder
from workers.file_ingest import FileIngestWorker
from workers.uart_to_net_forwarder import UartToNetForwarder
from common.mw_txt_reader import load_frames_from_txt
from common.bin_loader import load_frames_from_bin

HEADER_1024  = 0xFB
FRAME_1024   = BATCH_SIZE * BATCH_COUNT  # 1024 bytes

def verify_1024(frame: bytes) -> bool:
    if len(frame) != FRAME_1024:
        return False
    for i in range(BATCH_COUNT):
        base = i * BATCH_SIZE
        if frame[base] != HEADER_1024:
            return False
        s = 0
        for k in range(BATCH_SIZE - 1):
            s = (s + frame[base + k]) & 0xFF
        if s != frame[base + (BATCH_SIZE - 1)]:
            return False
    return True

def main():
    setup_logging("INFO")
    cfg = yaml.safe_load(open("../config/app.yaml"))

    # --- UART worker (single source of truth for RX/TX) ---
    uart = UartWorker(
        port=cfg["serial"]["port"],
        baud=cfg["serial"]["baud"],
        rtscts=cfg["serial"]["rtscts"],
        rx_qmax=cfg["timing"]["rx_queue_max"],
        tx_qmax=cfg["timing"]["tx_queue_max"],
        tx_gap_ms=cfg["timing"]["uart_tx_gap_ms"],
    )
    uart.start()

    # --- TCP client to PC (fixed 1024B frames) ---
    net = BufferClient(
        server_ip=cfg["network"]["buffer_server_ip"],
        server_port=cfg["network"]["buffer_server_port"],
        rx_verify_fn=verify_1024
    )
    net.start()

    # --- File server for ingesting motion scripts from PC ---
    files = FileServer(
        cfg["network"]["file_server_ip"],
        cfg["network"]["file_server_port"],
        cfg["paths"]["incoming_dir"]
    )
    files.start()

    # --- Preload default motion frames into feeder ---
    default_txt = cfg["paths"].get("moving_window_txt",
                                   "/home/pi/incoming/ramp_overlap_5min.txt")
    try:
        init_frames = load_frames_from_txt(default_txt) if os.path.exists(default_txt) else []
    except Exception:
        init_frames = []

    feeder = FrameOnUartRxFeeder(
        uart=uart,
        frames=init_frames,
        verify_fn=verify_1024,
        loop_frames=True,
        poll_sleep_s=0.001,
        rx_qmax=cfg["timing"]["rx_queue_max"]
    )
    feeder.start()

    # --- Hot-swap frames when new file arrives ---
    ingest = FileIngestWorker(
        files_obj=files,
        on_frames=feeder.reset_with_frames,
        txt_loader=load_frames_from_txt,
        bin_loader=load_frames_from_bin,
        poll_s=0.2
    )
    ingest.start()

    # --- Forward uC->PC without touching UART thread ---
    forwarder = UartToNetForwarder(
        feeder=feeder,
        net_client=net,
        verify_fn=verify_1024,
        poll_sleep_s=0.005
    )
    forwarder.start()

    print("[MAIN] Running (uC is timing master; feeder replies with next 1024B per RX; PC mirror enabled).")
    try:
        while True:
            time.sleep(0.1)
    except KeyboardInterrupt:
        print("\n[MAIN] Stopping...")
    finally:
        forwarder.stop()
        ingest.stop()
        feeder.stop()
        files.stop()
        net.stop()
        uart.stop()

if __name__ == "__main__":
    main()
