# rpi/src/workers/frame_on_uart_rx.py
import threading, time, queue
from typing import List, Optional, Callable
import RPi.GPIO as GPIO
# NEW: use your framing constants so keep-alive matches verify_1024
from common.framing import BATCH_SIZE, BATCH_COUNT  # 8, 128

tx_pin = 20
rx_pin = 21
GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)
GPIO.setup(tx_pin, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(rx_pin, GPIO.OUT, initial=GPIO.LOW)

KEEPALIVE_HEADER = 0xFB  # must match your 1024B verify

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
        # 1..6: simple pattern (deterministic + changing with counter)
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
    """
    Watches UART RX. Each time the uC sends anything:
      - stash that RX in an internal queue (for PC forwarding),
      - send ONE next 1024B frame back to the uC (from preloaded frames),
      - if no file frames available, send a KEEP-ALIVE frame to prevent uC reset.
    """

    def __init__(self,
                 uart,
                 frames: List[bytes] = None,
                 verify_fn: Optional[Callable[[bytes], bool]] = None,
                 loop_frames: bool = True,
                 poll_sleep_s: float = 0.001,
                 rx_qmax: int = 32,
                 keepalive_when_empty: bool = True):
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
                # Optionally validate RX; if you expect 1024B here, keep verify_fn; otherwise skip
                if (self._verify_fn is None) or self._verify_fn(rx):
                    self._pulse(rx_pin)
                    self._push_rx_for_pc(rx)

                    # choose next outbound
                    if pending is None:
                        pending = self._next_frame()
                        if pending is None and self._keepalive_when_empty:
                            pending = build_keepalive_frame(self._ka_counter)
                            self._ka_counter = (self._ka_counter + 1) & 0xFFFFFFFF

                    if pending is not None:
                        try:
                            self.uart.put_tx(pending)
                            self._pulse(tx_pin)
                        except Exception:
                            pass
                        pending = None
                else:
                    # RX was invalid; still consider sending keep-alive so uC doesn't reset
                    if self._keepalive_when_empty:
                        ka = build_keepalive_frame(self._ka_counter)
                        self._ka_counter = (self._ka_counter + 1) & 0xFFFFFFFF
                        try:
                            self.uart.put_tx(ka)
                            self._pulse(tx_pin)
                        except Exception:
                            pass
            time.sleep(self._poll_sleep_s)
