using System;
using System.IO.Ports;
using System.Threading;
using Diagnostics = System.Diagnostics;

namespace CNC_Controller_Initial
{
    public class DataStreamEventArgs : EventArgs
    {
        #region Defines
        private byte[] _bytes;
        #endregion

        #region Constructors
        public DataStreamEventArgs(byte[] bytes)
        {
            _bytes = bytes;
        }
        #endregion

        #region Properties
        public byte[] Response
        {
            get { return _bytes; }
        }
        #endregion
    }

    public class SerialClient : IDisposable
    {
        #region Defines
        private string _port;
        private int _baudRate;
        private SerialPort _serialPort;
        private Thread serThread;
        private double _PacketsRate;
        private DateTime _lastReceive;
        /*The Critical Frequency of Communication to Avoid Any Lag*/
        private const int freqCriticalLimit = 5;
        #endregion

        #region Constructors
        public SerialClient(string port)
        {
            _port = port;
            _baudRate = 9600;
            _lastReceive = DateTime.MinValue;

            serThread = new Thread(new ThreadStart(SerialReceiving));
            serThread.Priority = ThreadPriority.Highest;
            serThread.Name = "SerialHandle" + serThread.ManagedThreadId;
        }
        public SerialClient(string Port, int baudRate)
            : this(Port)
        {
            _baudRate = baudRate;
        }
        #endregion

        #region Custom Events
        public event EventHandler<DataStreamEventArgs> OnReceiving;
        #endregion

        #region Properties
        public string Port
        {
            get { return _port; }
        }
        public int BaudRate
        {
            get { return _baudRate; }
        }
        public string ConnectionString
        {
            get
            {
                return String.Format("[Serial] Port: {0} | Baudrate: {1}",
                    _serialPort.PortName, _serialPort.BaudRate.ToString());
            }
        }

        private int rx_bytes_count = 8;
        public  int Rx_Bytes_Count
        {
            get 
            { 
                return rx_bytes_count;
            }
            set 
            {
                rx_bytes_count = value; 
            }
        }
        private int receivedBytesThreshold = 128;
        public int ReceivedBytesThreshold
        {
            get
            {
                return receivedBytesThreshold;
            }
            set
            {
                receivedBytesThreshold = value;
                if (_serialPort != null)
                {
                    _serialPort.ReceivedBytesThreshold = receivedBytesThreshold;
                }
            }
        }

        #endregion

        #region Methods
        #region Port Control
        public bool OpenConn()
        {
            try
            {
                if (_serialPort == null)
                    _serialPort = new SerialPort(_port, _baudRate, Parity.None);

                if (!_serialPort.IsOpen)
                {
                    _serialPort.ReadTimeout = -1;
                    _serialPort.WriteTimeout = -1;

                    _serialPort.Open();

                    if (_serialPort.IsOpen)
                        serThread.Start(); /*Start The Communication Thread*/
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        public bool OpenConn(string port, int baudRate)
        {
            _port = port;
            _baudRate = baudRate;

            return OpenConn();
        }
        public void CloseConn()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                serThread.Abort();

                if (serThread.ThreadState == ThreadState.Aborted)
                    _serialPort.Close();
            }
        }
        public bool ResetConn()
        {
            CloseConn();
            return OpenConn();
        }
        #endregion
        #region Transmit/Receive
        public void Transmit(byte[] packet)
        {
            _serialPort.Write(packet, 0, packet.Length);
        }
        public int Receive(byte[] bytes, int offset, int count)
        {
            int readBytes = 0;

            if (count > 0)
            {
                readBytes = _serialPort.Read(bytes, offset, count);
            }

            return readBytes;
        }
        #endregion
        #region IDisposable Methods
        public void Dispose()
        {
            CloseConn();

            if (_serialPort != null)
            {
                _serialPort.Dispose();
                _serialPort = null;
            }
        }
        #endregion
        #endregion

        #region Threading Loops
        private void SerialReceiving()
        {
            byte[] frame = new byte[ReceivedBytesThreshold];
            int filled = 0;

            while (true)
            {
                try
                {
                    // Re-sync local buffer size if user changes ReceivedBytesThreshold at runtime
                    if (frame.Length != ReceivedBytesThreshold)
                    {
                        frame = new byte[ReceivedBytesThreshold];
                        filled = 0;
                    }

                    int available = _serialPort.BytesToRead;

                    // If nothing to read, yield briefly
                    if (available <= 0)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    // For frequency control: interval since last receive BEFORE we update it
                    TimeSpan tmpInterval = (DateTime.Now - _lastReceive);

                    int need = ReceivedBytesThreshold - filled;
                    int toRead = Math.Min(available, need);

                    int readBytes = Receive(frame, filled, toRead); // may be < toRead
                    if (readBytes > 0)
                    {
                        filled += readBytes;

                        // Update simple moving average of packet rate
                        _PacketsRate = ((_PacketsRate + readBytes) / 2.0);
                        _lastReceive = DateTime.Now;

                        // Full frame collected → fire event with a copy
                        if (filled == ReceivedBytesThreshold)
                        {
                            var payload = new byte[ReceivedBytesThreshold];
                            Buffer.BlockCopy(frame, 0, payload, 0, ReceivedBytesThreshold);

                            filled = 0; // reset for next frame
                            OnSerialReceiving(payload);
                        }

                        // ---- Frequency Control (kept from your original) ----
                        int pending = _serialPort.BytesToRead;
                        if ((double)(readBytes + pending) / 2.0 <= _PacketsRate)
                        {
                            if (tmpInterval.Milliseconds > 0)
                                Thread.Sleep(tmpInterval.Milliseconds > freqCriticalLimit ? freqCriticalLimit : tmpInterval.Milliseconds);

                            //Diagnostics.Debug.Write(tmpInterval.Milliseconds.ToString());
                            //Diagnostics.Debug.Write(" - ");
                            //Diagnostics.Debug.Write(readBytes.ToString());
                            //Diagnostics.Debug.Write("\r\n");
                        }
                    }
                    else
                    {
                        // Read returned 0 → yield a bit
                        Thread.Sleep(1);
                    }
                }
                catch (TimeoutException)
                {
                    // benign; port timeouts can happen depending on ReadTimeout settings
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Diagnostics.Debug.WriteLine($"SerialReceiving error: {ex.Message}");
                    Thread.Sleep(5);
                }
            }
        }

        #endregion

        #region Custom Events Invoke Functions
        private void OnSerialReceiving(byte[] res)
        {
            if (OnReceiving != null)
            {
                OnReceiving(this, new DataStreamEventArgs(res));
            }
        }
        #endregion
    }

}
