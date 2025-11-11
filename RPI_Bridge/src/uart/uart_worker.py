# uart/uart_worker.py
import serial, threading, time, queue, logging
log = logging.getLogger("uart")

class UartWorker:
    """
    Non-blocking UART with dedicated RX/TX threads and bounded queues.
    Supports hot re-open via request_reset() without killing the threads.
    """
    def __init__(self, port="/dev/serial0", baud=115200, rtscts=False,
                 rx_qmax=256, tx_qmax=256, tx_gap_ms=2):
        self.port = port
        self.baud = baud
        self.rtscts = rtscts
        self.tx_gap_ms = tx_gap_ms

        # queues
        self.rx_q = queue.Queue(maxsize=rx_qmax)
        self.tx_q = queue.Queue(maxsize=tx_qmax)

        # control flags
        self._stop = threading.Event()
        self._reset_req = threading.Event()
        self._ser_lock = threading.Lock()

        # serial object (opened in _open_serial)
        self.ser = None
        self._open_serial()  # initial open

        # threads
        self._rx_t = threading.Thread(target=self._rx_loop, daemon=True)
        self._tx_t = threading.Thread(target=self._tx_loop, daemon=True)

    # ---------- public ----------
    def start(self):
        if not self._rx_t.is_alive(): self._rx_t.start()
        if not self._tx_t.is_alive(): self._tx_t.start()

    def stop(self):
        self._stop.set()
        try:
            if self.ser and self.ser.is_open:
                self.ser.close()
        except: pass

    def request_reset(self):
        """Ask the threads to re-open the serial port (no thread stop)."""
        self._reset_req.set()

    def put_tx(self, frame: bytes):
        try:
            self.tx_q.put_nowait(frame)
        except queue.Full:
            log.warning("TX queue full; drop")

    def get_rx_nowait(self):
        try:
            return self.rx_q.get_nowait()
        except queue.Empty:
            return None

    # ---------- internals ----------
    def _open_serial(self):
        with self._ser_lock:
            try:
                if self.ser and self.ser.is_open:
                    self.ser.close()
            except: pass
            try:
                self.ser = serial.Serial(self.port, baudrate=self.baud,
                                         timeout=0, rtscts=self.rtscts)
                # flush buffers
                try:
                    self.ser.reset_input_buffer()
                    self.ser.reset_output_buffer()
                except: pass
                log.info("[UART] opened %s @ %d", self.port, self.baud)
                return True
            except Exception as e:
                log.error("[UART] open failed: %s", e)
                self.ser = None
                return False

    def _ensure_open(self, backoff=0.2):
        """Re-open if needed or if reset was requested."""
        if self._reset_req.is_set() or (self.ser is None) or (not self.ser.is_open):
            self._reset_req.clear()
            # try to reopen with backoff
            tried = self._open_serial()
            if not tried:
                time.sleep(backoff)

    def _rx_loop(self):
        buf = bytearray()
        backoff = 0.2
        while not self._stop.is_set():
            try:
                self._ensure_open(backoff)
                if not (self.ser and self.ser.is_open):
                    time.sleep(backoff)
                    continue

                data = self.ser.read(4096)
                if data:
                    buf += data
                    # Example framing: 1024B chunks; adjust if your uC sends 64B frames
                    while len(buf) >= 1024:
                        frm = bytes(buf[:1024]); del buf[:1024]
                        try:
                            self.rx_q.put_nowait(frm)
                        except queue.Full:
                            pass
                    backoff = 0.2
                else:
                    time.sleep(0.01)

            except Exception as e:
                log.error("[UART RX] error: %s", e)
                # request reopen and back off
                self._reset_req.set()
                time.sleep(backoff)
                backoff = min(backoff * 2, 2.0)

    def _tx_loop(self):
        backoff = 0.2
        while not self._stop.is_set():
            try:
                frm = None
                try:
                    frm = self.tx_q.get(timeout=0.05)
                except queue.Empty:
                    self._ensure_open(backoff)
                    continue

                self._ensure_open(backoff)
                if not (self.ser and self.ser.is_open):
                    # drop or requeue; here we drop to keep real-time
                    continue

                try:
                    self.ser.write(frm)
                    backoff = 0.2
                except Exception as e:
                    log.error("[UART TX] write error: %s", e)
                    self._reset_req.set()

                time.sleep(self.tx_gap_ms / 1000.0)
            except Exception as e:
                log.error("[UART TX] loop error: %s", e)
                self._reset_req.set()
                time.sleep(backoff)
                backoff = min(backoff * 2, 2.0)
