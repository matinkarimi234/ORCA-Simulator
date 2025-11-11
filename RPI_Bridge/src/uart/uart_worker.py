import serial, threading, time, queue, logging
log = logging.getLogger("uart")

class UartWorker:
    """
    Non-blocking UART with dedicated RX/TX threads and bounded queues.
    - put_tx(frame: bytes) to enqueue a frame for TX.
    - get_rx_nowait() to poll parsed frames.
    You can keep your 1024B/8B-batch validation in the consumer.
    """
    def __init__(self, port="/dev/serial0", baud=115200, rtscts=False,
                 rx_qmax=256, tx_qmax=256, tx_gap_ms=2):
        self.ser = serial.Serial(port, baudrate=baud, timeout=0, rtscts=rtscts)
        self.rx_q = queue.Queue(maxsize=rx_qmax)
        self.tx_q = queue.Queue(maxsize=tx_qmax)
        self.tx_gap_ms = tx_gap_ms

        self._stop = threading.Event()
        self._rx_t = threading.Thread(target=self._rx_loop, daemon=True)
        self._tx_t = threading.Thread(target=self._tx_loop, daemon=True)

    def start(self):
        self._rx_t.start(); self._tx_t.start()

    def stop(self):
        self._stop.set()
        try: self.ser.close()
        except: pass

    def put_tx(self, frame: bytes):
        try: self.tx_q.put_nowait(frame)
        except queue.Full: log.warning("TX queue full; drop")

    def get_rx_nowait(self):
        try: return self.rx_q.get_nowait()
        except queue.Empty: return None

    def _rx_loop(self):
        buf = bytearray()
        while not self._stop.is_set():
            data = self.ser.read(4096)
            if data:
                buf += data
                # Example: if you always receive exact 1024B frames, slice them:
                while len(buf) >= 1024:
                    frm = bytes(buf[:1024]); del buf[:1024]
                    try: self.rx_q.put_nowait(frm)
                    except queue.Full: pass
            else:
                time.sleep(0.05)

    def _tx_loop(self):
        while not self._stop.is_set():
            try:
                frm = self.tx_q.get(timeout=0.05)
            except queue.Empty:
                continue
            try:
                self.ser.write(frm)
            except Exception as e:
                log.error("UART write error: %s", e)
            time.sleep(self.tx_gap_ms / 1000.0)
