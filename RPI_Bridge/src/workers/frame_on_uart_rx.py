# rpi/src/workers/frame_on_uart_rx.py
import threading, time, queue
from typing import List, Optional, Callable
import RPi.GPIO as GPIO

tx_pin = 20
rx_pin = 21

GPIO.setwarnings(False)
GPIO.setmode(GPIO.BCM)  # Broadcom numbering
GPIO.setup(tx_pin, GPIO.OUT, initial=GPIO.LOW)
GPIO.setup(rx_pin, GPIO.OUT, initial=GPIO.LOW)

class FrameOnUartRxFeeder:
    """
    Watches UART RX. Each time the uC sends anything:
      - stash that RX in an internal queue (for PC forwarding),
      - send ONE next 1024B frame back to the uC (from preloaded frames).
    Keeps network completely out of the UART path.
    """

    def __init__(self,
                 uart,
                 frames: List[bytes] = None,
                 verify_fn: Optional[Callable[[bytes], bool]] = None,
                 loop_frames: bool = True,
                 poll_sleep_s: float = 0.001,
                 rx_qmax: int = 32):
        self.uart = uart
        self._frames = frames or []
        self._verify_fn = verify_fn
        self._loop_frames = loop_frames
        self._poll_sleep_s = poll_sleep_s

        self._idx = 0
        self._stop = threading.Event()
        self._thr: Optional[threading.Thread] = None
        self._lock = threading.Lock()

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

    # ---------- RX queue I/O ----------
    def _push_rx_for_pc(self, rx: Optional[bytes]):
        if not rx:
            return
        try:
            self._rx_queue.put_nowait(rx)
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
            return self._rx_queue.get_nowait()
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
            if rx is not None:
                # Visual RX pulse
                self._pulse(rx_pin)

                # Stash RX for PC
                self._push_rx_for_pc(rx)

                # Prepare next 1024B frame once per RX event
                if pending is None:
                    pending = self._next_frame()
                    if pending is None:
                        # Nothing to send right now
                        time.sleep(0.01)
                        time.sleep(self._poll_sleep_s)
                        continue

                # Transmit to uC (non-blocking via UartWorker queue)
                try:
                    self.uart.put_tx(pending)
                    self._pulse(tx_pin)
                except Exception:
                    pass
                pending = None

            time.sleep(self._poll_sleep_s)
