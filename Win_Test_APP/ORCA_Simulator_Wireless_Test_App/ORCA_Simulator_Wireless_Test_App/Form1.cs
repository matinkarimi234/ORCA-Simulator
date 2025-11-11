using Simple_Client_LAN_Control; // contains Simple_Buffer_Server + PiFileClient
using System;
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

        // periodic TX
        private System.Threading.Timer _txTimer;
        private int _txPeriodMs = 125;         // e.g., 40 Hz
        private int _txCounter = 0;           // grows every tick
        private volatile bool _txTickBusy = false; // avoid overlapping ticks

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


            // Start periodic TX (fires regardless; we’ll skip if not connected)
            _txTimer = new System.Threading.Timer(TxTimerTick, null, _txPeriodMs, _txPeriodMs);
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

            bool ok = true;

            for (int i = 0; i < BATCH_COUNT; i++)
            {
                int baseIndex = i * BATCH_SIZE;
                if (!VerifyChecksumAndHeader(raw, baseIndex)) 
                {
                    ok = false; 
                    break;
                }

                if (!ok)
                {
                    Console.WriteLine("Bad frame dropped");
                    return;
                }
            }
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
                Filter = "CSV/Text|*.csv;*.txt|All files|*.*"
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                string filePath = ofd.FileName;

                try
                {
                    // txtRpiIp should contain the RPi's IP (where its file server listens on 5002)
                    bool ok = await PiFileClient.SendFileAsync(txtRpiIp.Text.Trim(), 5002, filePath);
                    MessageBox.Show(ok ? "File uploaded." : "Upload failed.",
                                    "Upload", MessageBoxButtons.OK,
                                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
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
    }
}
