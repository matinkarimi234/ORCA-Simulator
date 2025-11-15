using Farand_Chart_Lib_Ver3;
using Simple_Client_LAN_Control; // contains Simple_Buffer_Server + PiFileClient
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ORCA_Simulator_Wireless_Test_App
{
    public partial class Form1 : Form
    {
        // ---- 64B RAW link constants (RPi <-> PC :5001) ----
        const int RAW_SIZE = 64;
        const byte RAW_HEADER = 0xFB; // must match RPi

        const int BATCH_SIZE = 8;
        const int BATCH_COUNT = 128;

        const int RECEIVED_BATCH_COUNT = 10;

        struct Motor_Position_Sample
        {
            public byte Header;
            public Int32 Motor_Position;
            public Int16 Counter;
            public byte Checksum;
        }
        // 10 samples of 1 motor
        Motor_Position_Sample[] _incoming_Batch = new Motor_Position_Sample[RECEIVED_BATCH_COUNT];
        private bool _newBatchReady = false; // set true when new batch decoded



        // periodic TX
        private System.Threading.Timer _txTimer;
        private int _txPeriodMs = 125;         // e.g., 40 Hz
        private int _txCounter = 0;           // grows every tick
        private volatile bool _txTickBusy = false; // avoid overlapping ticks

        double d_t = 0.025;
        private Int32 current_Motor_Position;
        double time_Sec = 0;

        // Optional: tiny counter demo
        int _counter = 0;

        public Form1()
        {
            InitializeComponent();

            // ==== Configure the BUFFER SERVER control (PC listens) ====
            // Bind to all local interfaces; the RPi BufferClient will connect here
            lan_Server.BindAddress = "0.0.0.0";  // or a specific NIC IP
            lan_Server.Port = 5001;
            lan_Server.RX_Byte_Count = BATCH_SIZE * BATCH_COUNT;

            // ---- hook events ----
            lan_Server.Connected += lan_Server_OnConnected;
            lan_Server.Disconnected += lan_Server_OnDisconnected;
            lan_Server.DataReceived += lan_Server_DataReceived;

            // ---- start server ----
            lan_Server.Start();
            Initialize_Chart();

            // Start periodic TX (fires regardless; we’ll skip if not connected)
            _txTimer = new System.Threading.Timer(TxTimerTick, null, _txPeriodMs, _txPeriodMs);
        }

        private void Graph_Data()
        {
            Farand_Chart.Farand_Graph motor_Graph = farand_Chart1.Get_Farand_Graph_Object(0);


            // scroll window after ~10 seconds
            if (time_Sec > farand_Chart1.XAxis.Minimum + 50)
            {
                if (motor_Graph.Points.Count > 50) motor_Graph.Points.RemoveAt(0);

                farand_Chart1.XAxis.Initial_Minimum += 1;
                farand_Chart1.XAxis.Initial_Maximum += 1;
            }
            if (current_Motor_Position > farand_Chart1.YAxis.Maximum)
            {
                farand_Chart1.YAxis.Initial_Maximum += 10000;
                farand_Chart1.YAxis.MajorGrid.Interval += 1000;
            }
            if (current_Motor_Position < farand_Chart1.YAxis.Minimum)
            {
                farand_Chart1.YAxis.Initial_Minimum -= 10000;
                farand_Chart1.YAxis.MajorGrid.Interval += 1000;
            }

            motor_Graph.Add_Point(time_Sec, current_Motor_Position);
        }

        private void Initialize_Chart()
        {
            farand_Chart1.View_Changed += Farand_Chart1_View_Changed;

            farand_Chart1.XAxis.Initial_Minimum = 0;
            farand_Chart1.XAxis.Initial_Maximum = 50;
            farand_Chart1.XAxis.MajorGrid.Interval = 10;
            farand_Chart1.XAxis.MajorGrid.Labels.DecimalPlaces = 0;
            farand_Chart1.XAxis.Title.FrameSize = new SizeF(100F, 20F);
            farand_Chart1.XAxis.Title.TopMargin = -20F;
            farand_Chart1.XAxis.Title.LabelStyle.Font = new Font("Arial", 10.0F);

            farand_Chart1.YAxis.Initial_Minimum = -10000;
            farand_Chart1.YAxis.Initial_Maximum = 10000;
            farand_Chart1.YAxis.MajorGrid.Interval = 1000;
            farand_Chart1.YAxis.MajorGrid.Labels.DecimalPlaces = 0;
            farand_Chart1.YAxis.Title.FrameSize = new SizeF(100F, 20F);
            farand_Chart1.YAxis.Title.RightMargin = -100F;
            farand_Chart1.YAxis.Title.LabelStyle.Font = new Font("Arial", 10.0F);

            farand_Chart1.Title.LabelStyle.Font = new Font("Arial", 10.0F);
            farand_Chart1.Title.FrameSize = new SizeF(200F, 20F);
            farand_Chart1.Title.Text = "Motor Position";

            farand_Chart1.Coordinates.LabelStyle.Font = new Font("Arial", 10.0F);
            farand_Chart1.Coordinates.FrameSize = new SizeF(120F, 20F);

            farand_Chart1.Legends.LeftMargin = 10.0F;
            farand_Chart1.GraphArea.RightMargin = 200;

            farand_Chart1.Clear_All_Farand_Graphs();

            var g0 = new Farand_Chart.Farand_Graph();
            g0.Name = "Motor Position (Pulse)";
            g0.PointStyle.Visible = false;
            g0.LineStyle.Color = Color.SkyBlue;
            g0.PointStyle.Size = 3.0F;
            g0.PointStyle.FillColor = Color.SkyBlue;
            g0.PointStyle.LineColor = Color.SkyBlue;
            g0.PointStyle.LineWidth = 1.0F;
            g0.LineStyle.Width = 1.5F;
            farand_Chart1.Add_Farand_Graph(g0);


            farand_Chart1.Refresh_All();
        }

        private void Farand_Chart1_View_Changed(object sender, EventArgs e)
        {

        }

        private void TxTimerTick(object state)
        {
            if (_txTickBusy) return;
            _txTickBusy = true;
            try
            {
                // Optional: you can track lanServer.IsConnected if you exposed it.
                // If not exposed, just send; the control handles failures gracefully.

                // Build payload (example puts a counter in bytes [1..4])
                var payload = new byte[62];
                payload[0] = (byte)((_txCounter >> 0) & 0xFF);
                payload[1] = (byte)((_txCounter >> 8) & 0xFF);
                payload[2] = (byte)((_txCounter >> 16) & 0xFF);
                payload[3] = (byte)((_txCounter >> 24) & 0xFF);
                _txCounter++;

                var frame = BuildRaw64Frame(payload);
                lan_Server.Send(frame); // non-blocking enqueue inside the control
            }
            catch { /* ignore one-shot errors */ }
            finally { _txTickBusy = false; }
        }

        private void lan_Server_OnConnected(object sender, EventArgs e)
        {
            // Optionally send a one-shot hello 64B frame
            var helloPayload = new byte[62];
            for (int i = 0; i < helloPayload.Length; i++) helloPayload[i] = (byte)i;
            var frame = BuildRaw64Frame(helloPayload);
            lan_Server.Send(frame);
            // UI hint
            labelStatus.Text = "Connected";
        }

        private void lan_Server_OnDisconnected(object sender, EventArgs e)
        {
            labelStatus.Text = "Disconnected";
        }

        // === PC (server) received exactly one 64B frame from RPi ===
        private void lan_Server_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var raw = e.Data;
            if (raw == null || raw.Length != BATCH_COUNT * BATCH_SIZE) return;

            for (int i = 0; i < RECEIVED_BATCH_COUNT; i++)
            {
                int baseIndex = i * BATCH_SIZE;
                if (!VerifyChecksumAndHeader(raw, baseIndex))
                {
                    Console.WriteLine($"Bad Checksum or Header on sample {i}!!!");
                    continue;
                }

                Motor_Position_Sample motor_sample = new Motor_Position_Sample();

                motor_sample.Header = raw[baseIndex + 0];

                motor_sample.Motor_Position = GetInt32_LE(raw, baseIndex + 1);

                motor_sample.Counter = GetInt16_LE(raw, baseIndex + 5);
                Console.WriteLine($"Counter {motor_sample.Counter}");

                motor_sample.Checksum = raw[baseIndex + BATCH_SIZE - 1];

                _incoming_Batch[i] = motor_sample;
            }
            _newBatchReady = true;
        }

        private Int32 GetInt32_LE(byte[] buf, int start)
        {
            int b0 = buf[start + 0];
            int b1 = buf[start + 1] << 8;
            int b2 = buf[start + 2] << 16;
            int b3 = buf[start + 3] << 24;

            return b0 + b1 + b2 + b3;
        }

        private short GetInt16_LE(byte[] buf, int start)
        {
            int b0 = buf[start + 0];
            int b1 = buf[start + 1] << 8;

            return (short)(b0 + b1);
        }

        // -----------------------------------------------
        // Build/send a PC->RPi command (64B RAW frame)
        // -----------------------------------------------
        private byte[] BuildRaw64Frame(byte[] payload62)
        {
            if (payload62 == null) payload62 = new byte[0];
            if (payload62.Length > 62) throw new ArgumentException("payload62 must be <= 62 bytes");

            var frame = new byte[RAW_SIZE];
            frame[0] = RAW_HEADER;

            // copy payload; the rest remains 0
            Array.Copy(payload62, 0, frame, 1, payload62.Length);

            int sum = 0;
            for (int i = 0; i < RAW_SIZE - 1; i++)
                sum = (sum + frame[i]) & 0xFF;

            frame[RAW_SIZE - 1] = (byte)sum;
            return frame;
        }

        private bool VerifyChecksumAndHeader(byte[] buf, int baseIndex)
        {
            if (buf[baseIndex] != RAW_HEADER) 
                return false;

            byte sum = 0;
            for (int i = 0; i < BATCH_SIZE - 1; i++) 
            { 
                sum += buf[baseIndex + i]; 
            }

            byte cs = buf[baseIndex + BATCH_SIZE - 1];
            return (sum == cs);
        }


        // -----------------------------------------------
        // Example “Send command” (wire to a button)
        // -----------------------------------------------
        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            // Example payload: incrementing counter in bytes [1..4]
            var payload = new byte[62];
            payload[0] = (byte)((_counter >> 0) & 0xFF);
            payload[1] = (byte)((_counter >> 8) & 0xFF);
            payload[2] = (byte)((_counter >> 16) & 0xFF);
            payload[3] = (byte)((_counter >> 24) & 0xFF);
            _counter++;

            var frame = BuildRaw64Frame(payload);
            lan_Server.Send(frame);
        }

        // -----------------------------------------------
        // File upload to RPi (:5002) — wire to a button
        // -----------------------------------------------
        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog()
            {
                Title = "Select file to upload to RPi",
                Filter = "CSV/Text/bin|*.csv;*.txt;*.bin|All files|*.*"
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                string filePath = ofd.FileName;

                label_Load_Status.Text = "Loading...";

                try
                {
                    // txtRpiIp should contain the RPi's IP (where its file server listens on 5002)
                    bool ok = await PiFileClient.SendFileAsync(txtRpiIp.Text.Trim(), 5002, filePath);
                    label_Load_Status.Text = ok ? "Loaded" : "Loading Failed";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Upload error: " + ex.Message, "Upload",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_txTimer != null) { _txTimer.Dispose(); _txTimer = null; }
            lan_Server.Stop();
            base.OnFormClosing(e);
        }

        private void timerUiUpdate_Tick(object sender, EventArgs e)
        {
            if (!_newBatchReady)
                return;

            _newBatchReady = false;

            // copy batch locally
            Motor_Position_Sample[] localBatch = new Motor_Position_Sample[RECEIVED_BATCH_COUNT];
            for (int i = 0; i < RECEIVED_BATCH_COUNT; i++)
                localBatch[i] = _incoming_Batch[i];

            // newest sample is the last one in the batch
            Motor_Position_Sample latest = localBatch[RECEIVED_BATCH_COUNT - 1];

            UpdateLabelsFromSample(latest);
            PushBatchToChart(localBatch);
        }

        private void UpdateLabelsFromSample(Motor_Position_Sample s)
        {
            Int32 position = s.Motor_Position;

            // update UI labels
            label_Time.Text = $"Time: {time_Sec:00.00} sec";
            label_Current_Motor_Position.Text = $"Motor Position: {position} pulses";
        }

        private void PushBatchToChart(Motor_Position_Sample[] frame)
        {
            for (int i = 0; i < frame.Length; i++)
            {
                Motor_Position_Sample s = frame[i];

                // assign globals so Graph_Data() can consume them
                current_Motor_Position = s.Motor_Position;

                // each sub-sample is 25 ms apart
                time_Sec += d_t;

                Graph_Data();
            }
        }
    }
}
