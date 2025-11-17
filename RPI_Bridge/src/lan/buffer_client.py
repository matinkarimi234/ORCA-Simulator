# lan/buffer_client.py
import socket
import threading
import time
from typing import Optional, Callable

from common.framing import RAW_SIZE, verify_raw64, BATCH_SIZE, BATCH_COUNT


class BufferClient:
    """
    TCP client with:
      - RX: fixed-size frames from PC (default 64B -> RAW_SIZE) with optional verify_fn
      - TX: fixed-size frames to PC (default 1024B -> BATCH_SIZE * BATCH_COUNT)

    Backward-compatible helpers:
      - send_once(): sends the current Tx_Frame buffer
      - Tx_Frame / Rx_Frame bytearrays kept for shared-state usage

    Typical use:
      net = BufferClient(ip, port)
      net.start()
      net.send_frame(frame_1024)         # one-shot 1024B send
      last_64 = net.copy_rx_frame()      # get last valid 64B frame
    """

    def __init__(self,
                 server_ip: str,
                 server_port: int = 5001,
                 tx_frame_size: Optional[int] = None,
                 rx_frame_size: Optional[int] = None,
                 rx_verify_fn: Optional[Callable[[bytes], bool]] = None):
        self.server_ip = server_ip
        self.server_port = int(server_port)

        # Sizes / verification
        # 1024B TX by default: 8-byte batches * 128 batches
        self.tx_frame_size = int(tx_frame_size) if tx_frame_size is not None else (BATCH_SIZE * BATCH_COUNT)
        # 64B RX by default
        self.rx_frame_size = int(rx_frame_size) if rx_frame_size is not None else RAW_SIZE
        self.rx_verify_fn = rx_verify_fn or verify_raw64

        # Shared state buffers
        self.Rx_Frame = bytearray(b"\x00" * self.rx_frame_size)   # last good RX frame (from PC)
        self.Tx_Frame = bytearray(b"\x00" * self.tx_frame_size)   # buffer used by send_once()
        self.is_connected = 0

        # Track if at least one valid RX frame has been received
        self._has_valid_rx = False

        # Internals
        self._stop = threading.Event()
        self._th = threading.Thread(target=self._loop, daemon=True)
        self._sock: Optional[socket.socket] = None
        self._lock = threading.Lock()

    # ---------------- Lifecycle ----------------
    def start(self):
        if self._th.is_alive():
            return
        self._stop.clear()
        self._th.start()

    def stop(self):
        self._stop.set()
        try:
            if self._sock:
                self._sock.close()
        except Exception:
            pass

    # ---------------- TX API ----------------
    def set_tx_frame(self, frame: bytes) -> bool:
        """Copy user-provided frame into the internal Tx_Frame buffer (size-checked)."""
        if not frame or len(frame) != self.tx_frame_size:
            return False
        with self._lock:
            self.Tx_Frame[:] = frame
        return True

    def send_frame(self, frame: bytes) -> bool:
        """Send one frame (size must match tx_frame_size)."""
        if not frame or len(frame) != self.tx_frame_size:
            return False
        s = self._sock
        if not s:
            return False
        try:
            # Disable Nagle for low latency
            s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
            s.sendall(frame)
            return True
        except Exception:
            return False

    def send_once(self) -> bool:
        """Backward-compat: send the current Tx_Frame buffer."""
        s = self._sock
        if not s:
            return False
        try:
            with self._lock:
                data = bytes(self.Tx_Frame)
            s.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
            s.sendall(data)
            return True
        except Exception:
            return False

    # ---------------- RX helpers ----------------
    def copy_rx_frame(self) -> bytes:
        """Thread-safe copy of the last received verified RX frame (may be zeros at startup)."""
        with self._lock:
            return bytes(self.Rx_Frame)

    def has_valid_rx(self) -> bool:
        """True once at least one verified 64B frame has been received from PC."""
        with self._lock:
            return self._has_valid_rx

    def copy_rx_frame_for_uart_tail(self) -> Optional[bytes]:
        """
        Helper specifically for the UART feeder:
        Returns last valid 64B frame or None if we have not yet received a valid one.
        """
        with self._lock:
            if not self._has_valid_rx:
                return None
            return bytes(self.Rx_Frame)

    # ---------------- Internal loop ----------------
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

                    # fixed-size RX framing (64B)
                    while len(rx_buf) >= self.rx_frame_size:
                        frame = bytes(rx_buf[:self.rx_frame_size])
                        del rx_buf[:self.rx_frame_size]
                        if (self.rx_verify_fn is None) or self.rx_verify_fn(frame):
                            with self._lock:
                                self.Rx_Frame[:] = frame
                                self._has_valid_rx = True
                            print("[BUF-CLIENT] RX verified")
                        else:
                            print("[BUF-CLIENT] RX failed verify")
                    time.sleep(0.005)

            except Exception:
                time.sleep(backoff)
                backoff = min(backoff * 2, 5.0)
            finally:
                try:
                    if self._sock:
                        self._sock.close()
                except Exception:
                    pass
                self._sock = None
                self.is_connected = 0
