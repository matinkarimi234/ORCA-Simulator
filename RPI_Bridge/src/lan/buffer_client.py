# lan/buffer_client.py
import socket, threading, time
from typing import Optional, Callable

class BufferClient:
    """
    TCP client for fixed-size frames (e.g., 1024 bytes).
    - frame_size: exact bytes per frame
    - verify_fn: optional Callable[[bytes], bool] to validate frames
    Stores last received valid frame in last_frame (bytearray).
    send_frame(frame) sends exactly one frame (must be frame_size bytes).
    """
    def __init__(self, server_ip: str, server_port: int, frame_size: int,
                 verify_fn: Optional[Callable[[bytes], bool]] = None):
        self.server_ip = server_ip
        self.server_port = int(server_port)
        self.frame_size = int(frame_size)
        self.verify_fn = verify_fn

        self.last_frame = bytearray(b"\x00" * self.frame_size)
        self.is_connected = 0

        self._sock: Optional[socket.socket] = None
        self._stop = threading.Event()
        self._lock = threading.Lock()
        self._th = threading.Thread(target=self._loop, daemon=True)

    def start(self):
        if self._th.is_alive(): return
        self._stop.clear()
        self._th.start()

    def stop(self):
        self._stop.set()
        try:
            if self._sock: self._sock.close()
        except: pass

    def send_frame(self, frame: bytes) -> bool:
        if not frame or len(frame) != self.frame_size: return False
        s = self._sock
        if not s: return False
        try:
            s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
            s.sendall(frame)
            return True
        except Exception:
            return False

    # --------------- internal ---------------
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
                print(f"[BUFFER-CLIENT] Connected {self.server_ip}:{self.server_port}")

                rx_buf = bytearray()
                while not self._stop.is_set():
                    try:
                        chunk = s.recv(8192)
                        if not chunk:
                            print("[BUFFER-CLIENT] server closed")
                            break
                        rx_buf.extend(chunk)
                    except socket.timeout:
                        pass
                    except Exception:
                        break

                    while len(rx_buf) >= self.frame_size:
                        frame = bytes(rx_buf[:self.frame_size])
                        del rx_buf[:self.frame_size]
                        if (self.verify_fn is None) or self.verify_fn(frame):
                            with self._lock:
                                self.last_frame[:] = frame
                    time.sleep(0.002)

            except Exception:
                time.sleep(backoff)
                backoff = min(backoff * 2, 5.0)
            finally:
                try:
                    if self._sock: self._sock.close()
                except: pass
                self._sock = None
                self.is_connected = 0
