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

        // Optional: tiny counter demo
        int _counter = 0;

        public Form1()
        {
            InitializeComponent();

            // ==== Configure the BUFFER SERVER control (PC listens) ====
            // Bind to all local interfaces; the RPi BufferClient will connect here
            lan_Server.BindAddress = "0.0.0.0";  // or a specific NIC IP
            lan_Server.Port = 5001;
            lan_Server.RX_Byte_Count = RAW_SIZE;

            // ---- hook events ----
            lan_Server.Connected += lan_Server_OnConnected;
            lan_Server.Disconnected += lan_Server_OnDisconnected;
            lan_Server.DataReceived += lan_Server_DataReceived;

            // ---- start server ----
            lan_Server.Start();
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
            if (raw == null || raw.Length != RAW_SIZE) return;
            if (!VerifyRaw64(raw)) return; // drop bad frames

            // Example: echo back (non-blocking; the control has an internal send queue)
            lan_Server.Send(raw);

            // OPTIONAL: parse payload bytes [1..62]
            // byte cmd = raw[1];
            // ... update UI safely if you want:
            //try
            //{
            //    if (InvokeRequired)
            //        BeginInvoke((MethodInvoker)(() => labelLastRx.Text = $"RX: {BitConverter.ToString(raw, 0, 8)}"));
            //    else
            //        labelLastRx.Text = $"RX: {BitConverter.ToString(raw, 0, 8)}";
            //}
            //catch { }
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

        private bool VerifyRaw64(byte[] frame)
        {
            if (frame == null || frame.Length != RAW_SIZE) return false;
            if (frame[0] != RAW_HEADER) return false;

            int sum = 0;
            for (int i = 0; i < RAW_SIZE - 1; i++)
                sum = (sum + frame[i]) & 0xFF;

            return (byte)sum == frame[RAW_SIZE - 1];
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // tidy shutdown
            lan_Server.Stop();
            base.OnFormClosing(e);
        }
    }
}
