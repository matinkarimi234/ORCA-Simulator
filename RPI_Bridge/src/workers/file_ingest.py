# rpi/src/workers/file_ingest.py
import os, threading, time
from typing import Optional, Callable, List

class FileIngestWorker:
    """
    Watches FileServer's last_file_path and, when a new file arrives,
    parses it into frames and injects them into the feeder via a callback.
    Runs in its own thread (no main-thread work).
    """
    def __init__(self,
                 files_obj,
                 on_frames: Callable[[List[bytes]], None],
                 txt_loader: Callable[[str], List[bytes]],
                 bin_loader: Callable[[str], List[bytes]],
                 poll_s: float = 0.2):
        self.files = files_obj
        self.on_frames = on_frames
        self.txt_loader = txt_loader
        self.bin_loader = bin_loader
        self.poll_s = poll_s

        self._seen: Optional[str] = None
        self._stop = threading.Event()
        self._thr: Optional[threading.Thread] = None

    def start(self):
        if self._thr and self._thr.is_alive():
            return
        self._stop.clear()
        self._thr = threading.Thread(target=self._run, daemon=True)
        self._thr.start()

    def stop(self):
        self._stop.set()

    def _run(self):
        while not self._stop.is_set():
            p = self.files.last_file_path
            if p and p != self._seen and os.path.exists(p):
                self._seen = p
                ext = os.path.splitext(p)[1].lower()
                try:
                    if ext == ".txt":
                        frames = self.txt_loader(p)
                    elif ext in (".bin", ".dat"):
                        frames = self.bin_loader(p)
                    else:
                        # default: try TXT
                        frames = self.txt_loader(p)
                    self.on_frames(frames)
                    print(f"[INGEST] Loaded {len(frames)} frames from {os.path.basename(p)}")
                except Exception as e:
                    print(f"[INGEST] Failed to load {p}: {e}")
            time.sleep(self.poll_s)
