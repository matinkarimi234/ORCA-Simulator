import socket, threading, time
from typing import Optional
from common.framing import RAW_SIZE, verify_raw64

class BufferClient:
    """
    RPi 64B client that connects to the C# BufferServer (PC).
    - Keeps the latest received frame in Rx_Frame.
    - Call send_once() to push current Tx_Frame (exactly 64 bytes).
    - Auto-reconnect with backoff; non-blocking loops.
    """
    def __init__(self, server_ip: str, server_port: int = 5001):
        self.server_ip = server_ip
        self.server_port = int(server_port)

        self.Rx_Frame = bytearray(b"\x00" * RAW_SIZE)
        self.Tx_Frame = bytearray(b"\x00" * RAW_SIZE)
        self.is_connected = 0

        self._stop = threading.Event()
        self._th = threading.Thread(target=self._loop, daemon=True)
        self._sock: Optional[socket.socket] = None
        self._lock = threading.Lock()

    def start(self):
        if self._th.is_alive(): return
        self._stop.clear(); self._th.start()

    def stop(self):
        self._stop.set()
        try:
            if self._sock: self._sock.close()
        except: pass

    def send_once(self) -> bool:
        s = self._sock
        if not s: return False
        try:
            with self._lock:
                data = bytes(self.Tx_Frame[:RAW_SIZE])
            s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
            s.sendall(data)
            return True
        except Exception:
            return False

    def _loop(self):
        backoff = 0.5
        while not self._stop.is_set():
            try:
                self.is_connected = 0
                s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
                s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
                s.settimeout(3.0)
                s.connect((self.server_ip, self.server_port))
                s.settimeout(0.2)
                self._sock = s
                self.is_connected = 1
                backoff = 0.5
                print(f"[BUF-CLIENT] Connected {self.server_ip}:{self.server_port}")

                rx_buf = bytearray()
                while not self._stop.is_set():
                    # receive
                    try:
                        chunk = s.recv(4096)
                        if not chunk:
                            print("[BUF-CLIENT] PC closed")
                            break
                        rx_buf.extend(chunk)
                    except socket.timeout:
                        pass
                    except Exception:
                        break

                    # fixed 64B frames
                    while len(rx_buf) >= RAW_SIZE:
                        frame = bytes(rx_buf[:RAW_SIZE])
                        del rx_buf[:RAW_SIZE]
                        if verify_raw64(frame):
                            with self._lock:
                                self.Rx_Frame[:] = frame
                    time.sleep(0.005)

            except Exception:
                time.sleep(backoff)
                backoff = min(backoff * 2, 5.0)
            finally:
                try:
                    if self._sock: self._sock.close()
                except: pass
                self._sock = None
                self.is_connected = 0
