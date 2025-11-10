using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simple_Client_LAN_Control
{
    public partial class Simple_Client_LAN_Control : UserControl, IDisposable
    {
        // ===========================
        // PUBLIC PROPERTIES / EVENTS
        // ===========================

        [Browsable(true)]
        [Category("LAN Client")]
        [Description("Target Pi IP address (static IP).")]
        public string IPAddress { get; set; } = "192.168.0.102";

        [Browsable(true)]
        [Category("LAN Client")]
        [Description("Target TCP port on Pi (5001 for 64B raw).")]
        public int Port { get; set; } = 5001;

        [Browsable(true)]
        [Category("LAN Client")]
        [Description("How many bytes we expect per received frame.")]
        public int RX_Byte_Count
        {
            get => _rxByteCount;
            set
            {
                if (value <= 0) return;
                _rxByteCount = value;

                _rxBuffer = new byte[_rxByteCount];          // last full frame
                _tempFrameCopy = new byte[_rxByteCount];      // temp copy for event firing

                _assemblyBuf = new byte[Math.Max(4096, _rxByteCount * 4)];
                _assemblyLen = 0;
            }
        }

        [Browsable(false)]
        public bool IsConnected => _isConnected;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        // ===========================
        // STATUS ICON STATE
        // ===========================

        private enum StatusState { Disconnected, Connected, RxBlink }

        // ===========================
        // INTERNAL STATE
        // ===========================

        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private CancellationTokenSource _mainCts;     // connect/reconnect loop lifetime
        private CancellationTokenSource _sessionCts;  // per-connection session lifetime

        private volatile bool _isConnected = false;

        private int _rxByteCount = 64;

        private byte[] _rxBuffer = new byte[64];
        private byte[] _tempFrameCopy = new byte[64];

        // rolling buffer for TCP receive
        private byte[] _assemblyBuf = new byte[4096];
        private int _assemblyLen = 0;

        // outgoing send queue
        private readonly ConcurrentQueue<byte[]> _txQueue = new ConcurrentQueue<byte[]>();

        // rx flash state
        private bool _rxBlinkToggle = false;

        public Simple_Client_LAN_Control()
        {
            InitializeComponent();
            SafeSetStatusImage(StatusState.Disconnected);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
                components?.Dispose();
                timerRxFlash?.Dispose();
            }
            base.Dispose(disposing);
        }

        // ===========================
        // PUBLIC START/STOP
        // ===========================

        public void Start()
        {
            if (_mainCts != null && !_mainCts.IsCancellationRequested) return;
            _mainCts = new CancellationTokenSource();
            _ = RunConnectionLoopAsync(_mainCts.Token);
        }

        public void Stop()
        {
            try { _mainCts?.Cancel(); } catch { }

            timerRxFlash?.Stop();

            _isConnected = false;

            CleanupSocket();

            _sessionCts?.Dispose();
            _sessionCts = null;

            _mainCts?.Dispose();
            _mainCts = null;
        }

        // enqueue frame to send
        public void Send(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            _txQueue.Enqueue(data);
        }

        // ===========================
        // CONNECT / RECONNECT LOOP
        // ===========================

        private async Task RunConnectionLoopAsync(CancellationToken token)
        {
            int backoffMs = 500;
            int backoffMaxMs = 5000;
            var rnd = new Random();

            while (!token.IsCancellationRequested)
            {
                if (!_isConnected)
                {
                    bool ok = await TryConnectOnceAsync(token);
                    if (!ok)
                    {
                        int waitMs = backoffMs + rnd.Next(0, 150);
                        try { await Task.Delay(waitMs, token); } catch { }
                        backoffMs = Math.Min(backoffMs * 2, backoffMaxMs);
                        continue;
                    }
                    backoffMs = 500;
                }
                try { await Task.Delay(200, token); } catch { }
            }
        }

        private async Task<bool> TryConnectOnceAsync(CancellationToken outerToken)
        {
            var client = new TcpClient { NoDelay = true };
            try { await client.ConnectAsync(IPAddress, Port); }
            catch { return false; }

            _tcpClient = client;
            _stream = _tcpClient.GetStream();
            _isConnected = true;
            _assemblyLen = 0;

            SafeSetStatusImage(StatusState.Connected);
            SafeFireConnected();

            try { _sessionCts?.Cancel(); } catch { }
            _sessionCts?.Dispose();
            _sessionCts = new CancellationTokenSource();

            // no heartbeat needed for 64B mode
            // timerHeartbeat?.Start();

            _ = ReceiveLoopAsync(_sessionCts.Token);
            _ = SendLoopAsync(_sessionCts.Token);
            return true;
        }

        // ===========================
        // RECEIVE LOOP
        // ===========================

        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _isConnected)
                {
                    if (_assemblyBuf.Length - _assemblyLen < 2048)
                    {
                        var bigger = new byte[_assemblyBuf.Length * 2];
                        Buffer.BlockCopy(_assemblyBuf, 0, bigger, 0, _assemblyLen);
                        _assemblyBuf = bigger;
                    }

                    int n;
                    try { n = await _stream.ReadAsync(_assemblyBuf, _assemblyLen, _assemblyBuf.Length - _assemblyLen, token); }
                    catch { n = 0; }

                    if (n <= 0) { HandleDisconnect(); return; }
                    _assemblyLen += n;

                    while (_assemblyLen >= _rxByteCount)
                    {
                        Buffer.BlockCopy(_assemblyBuf, 0, _rxBuffer, 0, _rxByteCount);
                        int remaining = _assemblyLen - _rxByteCount;
                        Buffer.BlockCopy(_assemblyBuf, _rxByteCount, _assemblyBuf, 0, remaining);
                        _assemblyLen = remaining;

                        Buffer.BlockCopy(_rxBuffer, 0, _tempFrameCopy, 0, _rxByteCount);
                        TriggerImmediateRxBlink();
                        SafeFireDataReceived(_tempFrameCopy);
                    }
                }
            }
            finally { HandleDisconnect(); }
        }

        // ===========================
        // SEND LOOP
        // ===========================

        private async Task SendLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _isConnected)
                {
                    if (_txQueue.TryDequeue(out var frame))
                    {
                        try
                        {
                            if (_stream != null)
                            {
                                await _stream.WriteAsync(frame, 0, frame.Length, token);
                                await _stream.FlushAsync(token);
                            }
                        }
                        catch { HandleDisconnect(); return; }
                    }
                    else
                    {
                        try { await Task.Delay(5, token); } catch { }
                    }
                }
            }
            finally { HandleDisconnect(); }
        }

        // ===========================
        // RX FLASH HANDLING
        // ===========================

        private void TriggerImmediateRxBlink()
        {
            if (this.IsHandleCreated && this.InvokeRequired)
            {
                try { this.BeginInvoke((MethodInvoker)TriggerImmediateRxBlink); } catch { }
                return;
            }

            _rxBlinkToggle = !_rxBlinkToggle;
            SafeSetStatusImage(StatusState.RxBlink);

            if (timerRxFlash != null)
            {
                timerRxFlash.Stop();
                timerRxFlash.Start();
            }
        }

        private void timerRxFlash_Tick(object sender, EventArgs e)
        {
            timerRxFlash.Stop();
            SafeSetStatusImage(_isConnected ? StatusState.Connected : StatusState.Disconnected);
        }

        // ===========================
        // DISCONNECT HANDLING
        // ===========================

        private void HandleDisconnect()
        {
            if (!_isConnected) return;

            _isConnected = false;
            try { _sessionCts?.Cancel(); } catch { }
            _sessionCts?.Dispose();
            _sessionCts = null;

            CleanupSocket();
            SafeOnDisconnectedUI();
        }

        private void SafeOnDisconnectedUI()
        {
            if (this.IsHandleCreated && this.InvokeRequired)
            {
                try { this.BeginInvoke((MethodInvoker)SafeOnDisconnectedUI); } catch { }
                return;
            }

            timerRxFlash?.Stop();

            SafeSetStatusImage(StatusState.Disconnected);
            SafeFireDisconnected();
        }

        private void CleanupSocket()
        {
            try { _stream?.Close(); } catch { }
            try { _tcpClient?.Close(); } catch { }
            _stream = null; _tcpClient = null;
        }

        // ===========================
        // SAFE UI HELPERS / EVENTS
        // ===========================

        private void SafeSetStatusImage(StatusState state)
        {
            if (pictureBox_Status == null) return;

            Image imgToShow = null;
            switch (state)
            {
                case StatusState.Disconnected:
                    imgToShow = Properties.Resources.Connection_Fail; break;
                case StatusState.Connected:
                    imgToShow = Properties.Resources.Connection_IDLE; break;
                case StatusState.RxBlink:
                    imgToShow = _rxBlinkToggle
                        ? Properties.Resources.Connection_RX_OK
                        : Properties.Resources.Connection_RX_IDLE;
                    break;
            }

            if (pictureBox_Status.InvokeRequired)
            {
                try { pictureBox_Status.BeginInvoke((MethodInvoker)(() => pictureBox_Status.Image = imgToShow)); } catch { }
            }
            else
            {
                pictureBox_Status.Image = imgToShow;
            }
        }

        private void SafeFireConnected()
        {
            if (Connected == null) return;
            try
            {
                if (InvokeRequired) BeginInvoke((MethodInvoker)(() => Connected?.Invoke(this, EventArgs.Empty)));
                else Connected?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        private void SafeFireDisconnected()
        {
            if (Disconnected == null) return;
            try
            {
                if (InvokeRequired) BeginInvoke((MethodInvoker)(() => Disconnected?.Invoke(this, EventArgs.Empty)));
                else Disconnected?.Invoke(this, EventArgs.Empty);
            }
            catch { }
        }

        private void SafeFireDataReceived(byte[] frameCopy)
        {
            if (DataReceived == null || frameCopy == null) return;
            var evtBytes = new byte[_rxByteCount];
            Buffer.BlockCopy(frameCopy, 0, evtBytes, 0, _rxByteCount);

            try
            {
                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)(() => DataReceived?.Invoke(this, new DataReceivedEventArgs(evtBytes))));
                else
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(evtBytes));
            }
            catch { }
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; }
        public DataReceivedEventArgs(byte[] data) { Data = data; }
    }
}
