# rpi/src/workers/frame_on_uart_rx.py
import threading
import time
import queue
import logging
from typing import List, Optional, Callable

import RPi.GPIO as GPIO

# Use your framing constants so keep-alive matches verify_1024
from common.framing import BATCH_SIZE, BATCH_COUNT  # 8, 128

tx_pin = 20
rx_pin = 21
GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)
GPIO.setup(tx_pin, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(rx_pin, GPIO.OUT, initial=GPIO.LOW)

KEEPALIVE_HEADER = 0xFB  # must match your 1024B verify

log = logging.getLogger("Rx")


def build_keepalive_frame(counter: int) -> bytes:
    """
    Build a valid 1024B frame (BATCH_COUNT * BATCH_SIZE) with header+checksum per 8B batch.
    Payload is a simple deterministic pattern derived from 'counter' so it changes over time.
    """
    total = BATCH_SIZE * BATCH_COUNT  # 1024
    buf = bytearray(total)
    for i in range(BATCH_COUNT):
        base = i * BATCH_SIZE
        # 0: header
        buf[base + 0] = KEEPALIVE_HEADER
        # 1..6: simple pattern (here all zeros; can be changed if needed)
        buf[base + 1] = 0
        buf[base + 2] = 0
        buf[base + 3] = 0
        buf[base + 4] = 0
        buf[base + 5] = 0
        buf[base + 6] = 0
        # 7: checksum of bytes 0..6 (same as your verify_1024)
        s = 0
        for k in range(BATCH_SIZE - 1):  # 0..6
            s = (s + buf[base + k]) & 0xFF
        buf[base + 7] = s
    return bytes(buf)


class FrameOnUartRxFeeder:
    def __init__(self,
                 uart,
                 frames: List[bytes] = None,
                 verify_fn: Optional[Callable[[bytes], bool]] = None,
                 loop_frames: bool = True,
                 poll_sleep_s: float = 0.001,
                 rx_qmax: int = 32,
                 keepalive_when_empty: bool = True,
                 invalid_threshold: int = 10,
                 pc_tail_provider: Optional[Callable[[], Optional[bytes]]] = None):
        self.uart = uart
        self._frames = frames or []
        self._verify_fn = verify_fn
        self._loop_frames = loop_frames
        self._poll_sleep_s = poll_sleep_s
        self._keepalive_when_empty = keepalive_when_empty

        self._idx = 0
        self._stop = threading.Event()
        self._thr: Optional[threading.Thread] = None
        self._lock = threading.Lock()

        self._ka_counter = 0  # increments each keep-alive

        # RX-from-uC buffer for the PC forwarder
        self._rx_queue: "queue.Queue[bytes]" = queue.Queue(maxsize=rx_qmax)

        # invalid RX counter
        self._invalid_count = 0
        self._invalid_threshold = invalid_threshold

        # callback to fetch last 64B from PC (BufferClient)
        self._pc_tail_provider = pc_tail_provider

    # ---------- lifecycle ----------
    def start(self):
        if self._thr and self._thr.is_alive():
            return
        self._stop.clear()
        self._thr = threading.Thread(target=self._run, daemon=True)
        self._thr.start()

    def stop(self):
        self._stop.set()

    # ---------- frames management ----------
    def reset_with_frames(self, frames: List[bytes]):
        """Hot-swap source frames atomically; reset index to 0."""
        if frames is None:
            frames = []
        with self._lock:
            self._frames = frames
            self._idx = 0

    def _next_frame(self) -> Optional[bytes]:
        with self._lock:
            if not self._frames:
                return None
            if self._idx >= len(self._frames):
                if not self._loop_frames:
                    return None
                self._idx = 0
            frm = self._frames[self._idx]
            self._idx += 1

        if self._verify_fn and frm is not None and not self._verify_fn(frm):
            # Skip invalid and fetch the next one
            return self._next_frame()
        return frm

    def _push_rx_for_pc(self, rx: Optional[bytes]):
        # Treat empty bytes as "no frame"
        if not rx or len(rx) == 0:
            # Optional: comment this out if it's too noisy
            print("[UART] RX empty -> drop")
            return
        try:
            self._rx_queue.put_nowait(rx)
            print(f"[UART -> PC] queued RX len={len(rx)}")
        except queue.Full:
            # Drop oldest to keep real-time behavior
            try:
                _ = self._rx_queue.get_nowait()
            except Exception:
                pass
            try:
                self._rx_queue.put_nowait(rx)
            except Exception:
                pass

    def get_rx_frame_nowait(self) -> Optional[bytes]:
        """Non-blocking read for the PC forwarder."""
        try:
            frm = self._rx_queue.get_nowait()
            print(f"[Forwarder] got RX len={len(frm)}")
            return frm
        except queue.Empty:
            return None

    # ---------- main loop ----------
    def _pulse(self, pin: int, dur: float = 0.005):
        GPIO.output(pin, GPIO.HIGH)
        time.sleep(dur)
        GPIO.output(pin, GPIO.LOW)

    def _run(self):
        pending: Optional[bytes] = None
        while not self._stop.is_set():
            rx = self.uart.get_rx_nowait()
            if rx:
                # validate RX; if you expect 1024B here, keep verify_fn; otherwise skip
                if (self._verify_fn is None) or self._verify_fn(rx):
                    # valid packet -> reset invalid counter
                    if self._invalid_count != 0:
                        log.debug("Valid Rx Packet -> reset invalid_count (was %d)", self._invalid_count)
                    self._invalid_count = 0

                    self._pulse(rx_pin)
                    self._push_rx_for_pc(rx)

                    # choose next outbound 1024B frame
                    if pending is None:
                        pending = self._next_frame()
                        if pending is None and self._keepalive_when_empty:
                            pending = build_keepalive_frame(self._ka_counter)
                            self._ka_counter = (self._ka_counter + 1) & 0xFFFFFFFF

                    if pending is not None:
                        # -------- inject 64B from PC at the end of this 1024B frame --------
                        try:
                            if self._pc_tail_provider is not None:
                                tail = self._pc_tail_provider()
                                if tail is not None and len(tail) == 64:
                                    # Overwrite last 64 bytes in the 1024B frame
                                    tmp = bytearray(pending)
                                    tmp[-64:] = tail
                                    pending = bytes(tmp)
                                # If tail is None or wrong length, just send pending as-is
                        except Exception as e:
                            log.exception("Error injecting PC tail into UART frame: %s", e)

                        # --------------------------------------------------------------------
                        try:
                            self.uart.put_tx(pending)
                            self._pulse(tx_pin)
                        except Exception:
                            pass
                        pending = None
                else:
                    # invalid frame received
                    self._invalid_count += 1
                    log.error("Invalid Rx Packet (%d/%d)",
                              self._invalid_count, self._invalid_threshold)

                    # trigger reset after N invalid packets
                    if self._invalid_count >= self._invalid_threshold:
                        log.warning("Too many invalid packets -> calling uart.request_reset()")
                        self._invalid_count = 0
                        try:
                            self.uart.request_reset()
                        except Exception as e:
                            log.exception("uart.request_reset() failed: %s", e)

            time.sleep(self._poll_sleep_s)
