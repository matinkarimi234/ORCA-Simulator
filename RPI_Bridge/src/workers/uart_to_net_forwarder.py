import threading, time
from typing import Callable, Optional

class UartToNetForwarder:
    """
    Polls feeder for RX-from-uC frames and sends them to PC via BufferClient.
    Keeps network out of the UART thread. Drops frames if PC is slow (feeder queue policy).
    """
    def __init__(self, feeder, net_client, verify_fn: Optional[Callable[[bytes], bool]] = None,
                 poll_sleep_s: float = 0.005):
        self._feeder = feeder          # FrameOnUartRxFeeder
        self._net = net_client         # BufferClient
        self._verify_fn = verify_fn
        self._poll_sleep_s = poll_sleep_s
        self._stop = threading.Event()
        self._thr: Optional[threading.Thread] = None

    def start(self):
        if self._thr and self._thr.is_alive(): return
        self._stop.clear()
        self._thr = threading.Thread(target=self._run, daemon=True)
        self._thr.start()

    def stop(self):
        self._stop.set()

    def _run(self):
        while not self._stop.is_set():
            frm = self._feeder.get_rx_frame_nowait()
            if frm is not None:
                if (self._verify_fn is None) or self._verify_fn(frm):
                    # Optional: skip send attempts if not connected (saves exceptions)
                    if self._net.is_connected:
                        try:
                            self._net.send_frame(frm)
                        except Exception:
                            pass
            time.sleep(self._poll_sleep_s)
