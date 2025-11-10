using Simple_Client_LAN_Control;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ORCA_Simulator_Wireless_Test_App
{
    public partial class Form1 : Form
    {
        // ---- 64B RAW link constants (PC <-> Pi :5001) ----
        const int RAW_SIZE = 64;
        const byte RAW_HEADER = 0xFB; // matches Pi
        // Port 5002 is used by PiFileClient for file uploads

        // Optional: keep a tiny payload counter demo
        int counter = 0;

        public Form1()
        {
            InitializeComponent();

            // Configure the component for RAW64
            esp.IPAddress = "192.168.0.200";  // set your Pi IP
            esp.Port = 5001;
            esp.RX_Byte_Count = RAW_SIZE;

            // hook events
            esp.Connected += Esp_onConnected;
            esp.Disconnected += Esp_onDisconnect;
            esp.DataReceived += Esp_DataReceived;

            // start client
            esp.Start();
        }

        private void Esp_onConnected(object sender, EventArgs e)
        {
            // No timers; we’re event-driven. Optionally send a 64B hello once:
            var helloPayload = new byte[62];
            for (int i = 0; i < helloPayload.Length; i++) helloPayload[i] = (byte)i;
            var frame = BuildRaw64Frame(helloPayload);
            esp.Send(frame);
        }

        private void Esp_onDisconnect(object sender, EventArgs e)
        {
            // Nothing special; auto-reconnect handled inside the control
        }

        // 64B RAW receive -> validate -> echo back (off UI thread)
        private void Esp_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var raw = e.Data;
            if (raw == null || raw.Length != RAW_SIZE) return;

            if (!VerifyRaw64(raw))
            {
                // Bad frame: drop (do not echo)
                return;
            }

            // Echo clean frame (do not block UI)
            ThreadPool.QueueUserWorkItem(_ => esp.Send(raw));

            // OPTIONAL: inspect payload bytes [1..62], e.g., show in UI or parse commands
            // byte p0 = raw[1];
        }

        // -----------------------------------------------
        // Build/send a PC->Pi command (64B RAW frame)
        // -----------------------------------------------
        private byte[] BuildRaw64Frame(byte[] payload62)
        {
            if (payload62 == null) payload62 = Array.Empty<byte>();
            if (payload62.Length > 62) throw new ArgumentException("payload62 must be <= 62 bytes");

            var frame = new byte[RAW_SIZE];
            frame[0] = RAW_HEADER;

            // copy payload; rest defaults to 0
            Array.Copy(payload62, 0, frame, 1, payload62.Length);

            byte sum = 0;
            for (int i = 0; i < RAW_SIZE - 1; i++)
                sum += frame[i];

            frame[RAW_SIZE - 1] = sum;
            return frame;
        }

        private bool VerifyRaw64(byte[] frame)
        {
            if (frame == null || frame.Length != RAW_SIZE) return false;
            if (frame[0] != RAW_HEADER) return false;

            byte sum = 0;
            for (int i = 0; i < RAW_SIZE - 1; i++)
                sum += frame[i];

            return sum == frame[RAW_SIZE - 1];
        }

        // -----------------------------------------------
        // Example “Send command” (e.g., wire to a button)
        // -----------------------------------------------
        private void btnSendCommand_Click(object sender, EventArgs e)
        {
            // Example payload: incrementing counter LSB first in bytes [1..4]
            var payload = new byte[62];
            payload[0] = (byte)((counter >> 0) & 0xFF);
            payload[1] = (byte)((counter >> 8) & 0xFF);
            payload[2] = (byte)((counter >> 16) & 0xFF);
            payload[3] = (byte)((counter >> 24) & 0xFF);
            counter++;

            var frame = BuildRaw64Frame(payload);
            esp.Send(frame);
        }

        // -----------------------------------------------
        // File upload to Pi (:5002) — e.g., wire to a button
        // -----------------------------------------------
        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog()
            {
                Title = "Select file to upload to Pi",
                Filter = "CSV/Text|*.csv;*.txt|All files|*.*"
            })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                string path = ofd.FileName;

                try
                {
                    // Use the same IP as the 64B port; file server listens on 5002
                    bool ok = await PiFileClient.SendFileAsync(esp.IPAddress, 5002, path);
                    MessageBox.Show(ok ? "File uploaded." : "Upload failed.", "Upload", MessageBoxButtons.OK,
                        ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Upload error: " + ex.Message, "Upload", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
