# rpi/src/workers/frame_on_uart_rx.py
import threading, time
from typing import List, Optional, Callable
import RPi.GPIO as GPIO

tx_pin = 20
rx_pin = 21

GPIO.setmode(GPIO.BCM) # Broadcom pin-numbering scheme
GPIO.setup(tx_pin, GPIO.OUT) 
GPIO.setup(rx_pin, GPIO.OUT) 

class FrameOnUartRxFeeder:
    """
    Sends ONE next 1024B frame each time the uC sends *anything*.
    Runs in its own thread; does not block main.
    """
    def __init__(self,
                 uart,
                 frames: List[bytes] = None,
                 verify_fn: Optional[Callable[[bytes], bool]] = None,
                 loop_frames: bool = True,
                 poll_sleep_s: float = 0.001):
        self.uart = uart
        self._frames = frames or []
        self._verify_fn = verify_fn
        self._loop_frames = loop_frames
        self._poll_sleep_s = poll_sleep_s

        self._idx = 0
        self._stop = threading.Event()
        self._thr: Optional[threading.Thread] = None
        self._lock = threading.Lock()

    def start(self):
        if self._thr and self._thr.is_alive():
            return
        self._stop.clear()
        self._thr = threading.Thread(target=self._run, daemon=True)
        self._thr.start()

    def stop(self):
        self._stop.set()

    def reset_with_frames(self, frames: List[bytes]):
        """
        Hot-swap the source frames atomically and reset index to 0.
        Safe to call from any thread (e.g., file ingest worker).
        """
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
            # Skip invalid; try next (tail recursion avoided)
            return self._next_frame()
        return frm

    def _run(self):
        pending: Optional[bytes] = None
        while not self._stop.is_set():
            rx = self.uart.get_rx_nowait()
            if rx is not None:
                GPIO.output(rx_pin, GPIO.HIGH)
                time.sleep(0.005)
                GPIO.output(rx_pin, GPIO.LOW)
                if pending is None:
                    pending = self._next_frame()
                    if pending is None:
                        time.sleep(0.01)
                        continue
                try:
                    self.uart.put_tx(pending)
                    GPIO.output(tx_pin, GPIO.HIGH)
                    time.sleep(0.005)
                    GPIO.output(tx_pin, GPIO.LOW)
                except Exception:
                    pass
                pending = None
            time.sleep(self._poll_sleep_s)
