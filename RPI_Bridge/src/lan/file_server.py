import os, socket, threading, struct, time
from typing import Optional

class FileServer:
    """
    Big-endian protocol:
      [8] file_size (u64)
      [2] name_len  (u16)
      [N] filename  (UTF-8)
      [file_size] content
    Saves to incoming_dir/filename and exposes last_file_* for main.
    """
    def __init__(self, ip="0.0.0.0", port=5002, incoming_dir="/home/pi/incoming"):
        self.ip, self.port = ip, int(port)
        self.incoming_dir = incoming_dir
        os.makedirs(self.incoming_dir, exist_ok=True)

        self._srv: Optional[socket.socket] = None
        self._thr: Optional[threading.Thread] = None
        self._stop = threading.Event()

        self.last_file_path: Optional[str] = None
        self.last_file_name: Optional[str] = None
        self.last_file_size: Optional[int] = None

    def start(self):
        if self._thr and self._thr.is_alive(): return
        self._stop.clear()
        self._thr = threading.Thread(target=self._server_loop, daemon=True)
        self._thr.start()

    def stop(self):
        self._stop.set()
        try:
            if self._srv: self._srv.close()
        except: pass

    def _server_loop(self):
        self._srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self._srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self._srv.bind((self.ip, self.port))
        self._srv.listen(4)
        print(f"[FILE] Listening {self.ip}:{self.port} -> {self.incoming_dir}")
        while not self._stop.is_set():
            try:
                self._srv.settimeout(0.5)
                conn, addr = self._srv.accept()
                conn.settimeout(5.0)
                threading.Thread(target=self._handle_client, args=(conn, addr), daemon=True).start()
            except socket.timeout:
                continue
            except OSError:
                break
            except Exception as e:
                print("[FILE] accept error:", e); time.sleep(0.2)
        print("[FILE] stopped")

    def _recv_exact(self, conn: socket.socket, n: int) -> Optional[bytes]:
        buf = bytearray()
        while len(buf) < n:
            try:
                chunk = conn.recv(n - len(buf))
                if not chunk: return None
                buf.extend(chunk)
            except socket.timeout:
                return None
            except Exception:
                return None
        return bytes(buf)

    def _handle_client(self, conn: socket.socket, addr):
        try:
            hdr = self._recv_exact(conn, 10)
            if not hdr: conn.close(); return
            fsize, name_len = struct.unpack(">QH", hdr)
            if name_len == 0 or name_len > 4096 or fsize > (1 << 30):  # 1 GiB guard
                conn.close(); return

            name_bytes = self._recv_exact(conn, name_len)
            if not name_bytes: conn.close(); return
            fname = os.path.basename(name_bytes.decode("utf-8", "ignore"))
            if not fname: conn.close(); return
            # sanitize filename
            fname = fname.replace("..","_").replace("/","_").replace("\\","_")

            dst = os.path.join(self.incoming_dir, fname)
            remaining = fsize
            with open(dst, "wb") as f:
                while remaining > 0:
                    try:
                        chunk = conn.recv(min(128*1024, remaining))
                    except socket.timeout:
                        chunk = b""
                    if not chunk: conn.close(); return
                    f.write(chunk)
                    remaining -= len(chunk)

            self.last_file_name = fname
            self.last_file_path = dst
            self.last_file_size = fsize
            try: conn.sendall(b"OK")
            except: pass
            print(f"[FILE] Received {fname} ({fsize} B) from {addr}")
        except Exception as e:
            print("[FILE] client error:", e)
        finally:
            try: conn.close()
            except: pass
