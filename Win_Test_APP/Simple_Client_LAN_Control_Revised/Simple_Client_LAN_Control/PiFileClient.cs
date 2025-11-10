using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_Client_LAN_Control
{
    public static class PiFileClient
    {
        // Protocol:
        // [8]  file_size (UInt64 big-endian)
        // [2]  name_len  (UInt16 big-endian)
        // [N]  filename UTF-8
        // [file_size] content

        public static async Task<bool> SendFileAsync(string host, int port, string filePath, string remoteName = null, CancellationToken ct = default)
        {
            if (!File.Exists(filePath)) return false;
            var fi = new FileInfo(filePath);
            ulong size = (ulong)fi.Length;
            string name = string.IsNullOrEmpty(remoteName) ? fi.Name : remoteName;
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            if (nameBytes.Length > ushort.MaxValue) throw new ArgumentException("Filename too long.");

            byte[] header = new byte[10];
            // UInt64 big-endian
            header[0] = (byte)((size >> 56) & 0xFF);
            header[1] = (byte)((size >> 48) & 0xFF);
            header[2] = (byte)((size >> 40) & 0xFF);
            header[3] = (byte)((size >> 32) & 0xFF);
            header[4] = (byte)((size >> 24) & 0xFF);
            header[5] = (byte)((size >> 16) & 0xFF);
            header[6] = (byte)((size >> 8) & 0xFF);
            header[7] = (byte)((size >> 0) & 0xFF);
            // UInt16 big-endian
            ushort nl = (ushort)nameBytes.Length;
            header[8] = (byte)((nl >> 8) & 0xFF);
            header[9] = (byte)(nl & 0xFF);

            using (var client = new TcpClient() { NoDelay = true })
            {
                await client.ConnectAsync(host, port);
                using (var ns = client.GetStream())
                {
                    await ns.WriteAsync(header, 0, header.Length, ct);
                    await ns.WriteAsync(nameBytes, 0, nameBytes.Length, ct);

                    byte[] buf = new byte[64 * 1024];
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        int r;
                        while ((r = await fs.ReadAsync(buf, 0, buf.Length, ct)) > 0)
                        {
                            await ns.WriteAsync(buf, 0, r, ct);
                        }
                    }

                    // optional: read "OK"
                    var ack = new byte[2];
                    try { await ns.ReadAsync(ack, 0, ack.Length, ct); } catch { }
                }
            }
            return true;
        }
    }
}
