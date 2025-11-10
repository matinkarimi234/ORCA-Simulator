using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Simple_Client_LAN_Control
{
    public partial class Simple_Buffer_Server : UserControl, IDisposable
    {
        // ===========================
        // PUBLIC PROPERTIES / EVENTS
        // ===========================

        [Browsable(true)]
        [Category("LAN Server")]
        [Description("Local IP to bind (use 0.0.0.0 for all).")]
        public string BindAddress { get; set; } = "0.0.0.0";

        [Browsable(true)]
        [Category("LAN Server")]
        [Description("TCP port to listen on (5001 for 64B).")]
        public int Port { get; set; } = 5001;

        [Browsable(true)]
        [Category("LAN Server")]
        [Description("How many bytes per frame (fixed).")]
        public int RX_Byte_Count
        {
            get { return _rxByteCount; }
            set
            {
                if (value <= 0) return;
                _rxByteCount = value;

                _rxBuffer = new byte[_rxByteCount];
                _tempFrameCopy = new byte[_rxByteCount];

                _assemblyBuf = new byte[Math.Max(4096, _rxByteCount * 4)];
                _assemblyLen = 0;
            }
        }

        [Browsable(false)]
        public bool IsConnected { get { return _isConnected; } }

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

        private TcpListener _listener;
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private CancellationTokenSource _mainCts;     // accept loop lifetime
        private CancellationTokenSource _sessionCts;  // per-connection lifetime

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

        public Simple_Buffer_Server()
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

            IPAddress ip;
            if (!IPAddress.TryParse(BindAddress, out ip))
                ip = IPAddress.Any;

            _listener = new TcpListener(ip, Port);
            _ = AcceptLoopAsync(_mainCts.Token);
        }

        public void Stop()
        {
            try { _mainCts?.Cancel(); } catch { }

            timerRxFlash?.Stop();
            _isConnected = false;

            CleanupSocket();

            try { _listener?.Stop(); } catch { }

            if (_sessionCts != null) { try { _sessionCts.Cancel(); } catch { } _sessionCts.Dispose(); _sessionCts = null; }
            if (_mainCts != null) { _mainCts.Dispose(); _mainCts = null; }
        }

        // enqueue a frame to send to the connected client
        public void Send(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            _txQueue.Enqueue(data);
        }

        // ===========================
        // ACCEPT LOOP (SERVER)
        // ===========================

        private async Task AcceptLoopAsync(CancellationToken token)
        {
            try { _listener.Start(); }
            catch (Exception ex)
            {
                // can't start listener; surface as disconnected
                SafeOnDisconnectedUI();
                return;
            }

            while (!token.IsCancellationRequested)
            {
                TcpClient client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                    client.NoDelay = true;

                    // If a client is already connected, drop it in favor of the new one (optional policy)
                    CleanupSocket();

                    _tcpClient = client;
                    _stream = _tcpClient.GetStream();
                    _isConnected = true;
                    _assemblyLen = 0;

                    SafeSetStatusImage(StatusState.Connected);
                    SafeFireConnected();

                    if (_sessionCts != null) { try { _sessionCts.Cancel(); } catch { } _sessionCts.Dispose(); }
                    _sessionCts = new CancellationTokenSource();

                    _ = ReceiveLoopAsync(_sessionCts.Token);
                    _ = SendLoopAsync(_sessionCts.Token);
                }
                catch (ObjectDisposedException) { break; }
                catch (InvalidOperationException) { break; }
                catch (Exception)
                {
                    try { await Task.Delay(200, token); } catch { }
                }
            }
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
                    byte[] frame;
                    if (_txQueue.TryDequeue(out frame))
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
            if (_sessionCts != null) { _sessionCts.Dispose(); _sessionCts = null; }

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
            var h = Connected;
            if (h == null) return;
            try
            {
                if (InvokeRequired) BeginInvoke((MethodInvoker)(() => h(this, EventArgs.Empty)));
                else h(this, EventArgs.Empty);
            }
            catch { }
        }

        private void SafeFireDisconnected()
        {
            var h = Disconnected;
            if (h == null) return;
            try
            {
                if (InvokeRequired) BeginInvoke((MethodInvoker)(() => h(this, EventArgs.Empty)));
                else h(this, EventArgs.Empty);
            }
            catch { }
        }

        private void SafeFireDataReceived(byte[] frameCopy)
        {
            var h = DataReceived;
            if (h == null || frameCopy == null) return;

            var evtBytes = new byte[_rxByteCount];
            Buffer.BlockCopy(frameCopy, 0, evtBytes, 0, _rxByteCount);

            try
            {
                if (InvokeRequired)
                    BeginInvoke((MethodInvoker)(() => h(this, new DataReceivedEventArgs(evtBytes))));
                else
                    h(this, new DataReceivedEventArgs(evtBytes));
            }
            catch { }
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; private set; }
        public DataReceivedEventArgs(byte[] data) { Data = data; }
    }
}
