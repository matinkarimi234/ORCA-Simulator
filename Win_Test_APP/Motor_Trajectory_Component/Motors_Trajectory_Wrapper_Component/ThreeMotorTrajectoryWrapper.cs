using Motor_Trajectory_Component;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Motors_Trajectory_Wrapper_Component
{
    public class ThreeMotorTrajectoryWrapper
    {
        private const byte HEADER = 0xFB;
        private const int BATCH_SIZE = 8;
        private const int BATCH_COUNT = 128;

        private const int ACTIVE_BATCHES_PER_MOTOR = 40;
        private const int FRAME_STEP_BATCHES = 10;  // time shift = BatchMs * 10

        private readonly MotorTrajectory _motor1;
        private readonly MotorTrajectory _motor2;
        private readonly MotorTrajectory _motor3;

        private readonly bool _motor1Enabled;
        private readonly bool _motor2Enabled;
        private readonly bool _motor3Enabled;

        public int BatchMs => _motor1.BatchMs;
        public double PeriodSeconds => _motor1.PeriodSeconds;
        public int TotalSamples => _motor1.TotalSamples;

        /// <summary>
        /// Wrapper for 3 motors. Enabled flags control whether that motor
        /// actually writes data, or leaves its region as header+zeros+checksum.
        /// </summary>
        public ThreeMotorTrajectoryWrapper(
            MotorTrajectory motor1, bool motor1Enabled,
            MotorTrajectory motor2, bool motor2Enabled,
            MotorTrajectory motor3, bool motor3Enabled)
        {
            _motor1 = motor1 ?? throw new ArgumentNullException(nameof(motor1));
            _motor2 = motor2 ?? throw new ArgumentNullException(nameof(motor2));
            _motor3 = motor3 ?? throw new ArgumentNullException(nameof(motor3));

            _motor1Enabled = motor1Enabled;
            _motor2Enabled = motor2Enabled;
            _motor3Enabled = motor3Enabled;

            // timing sanity checks (all 3 trajectories must match)
            if (_motor1.BatchMs != _motor2.BatchMs || _motor1.BatchMs != _motor3.BatchMs)
                throw new ArgumentException("All motor trajectories must have the same BatchMs.");

            if (_motor1.TotalSamples != _motor2.TotalSamples ||
                _motor1.TotalSamples != _motor3.TotalSamples)
                throw new ArgumentException("All motor trajectories must have the same TotalSamples.");
        }

        public void WritePrettyFile(string path)
        {
            int totalSamples = TotalSamples;

            using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
            {
                int frameIdx = 0;
                while (true)
                {
                    int startBatch = frameIdx * FRAME_STEP_BATCHES;
                    if (startBatch >= totalSamples)
                        break;

                    var batches = BuildFrameBatches(startBatch);

                    writer.WriteLine($"FRAME {frameIdx:0000}");
                    for (int i = 0; i < batches.Count; i++)
                    {
                        writer.WriteLine($"  {i:000}: {BatchBytesAsHexLine(batches[i])}");
                    }

                    frameIdx++;
                }
            }
        }

        private List<byte[]> BuildFrameBatches(int startBatchIndex)
        {
            var batches = new List<byte[]>(BATCH_COUNT);

            // Initialize all batches: header + zeros (1..7 will be overwritten or stay zero)
            for (int i = 0; i < BATCH_COUNT; i++)
            {
                var b = new byte[BATCH_SIZE];
                b[0] = HEADER;
                batches.Add(b);
            }

            // Fill each motor region only if that motor is enabled
            FillMotorRegion(batches, _motor1, _motor1Enabled, startBatchIndex, baseBatchIndex: 0);
            FillMotorRegion(batches, _motor2, _motor2Enabled, startBatchIndex, baseBatchIndex: 40);
            FillMotorRegion(batches, _motor3, _motor3Enabled, startBatchIndex, baseBatchIndex: 80);

            // Compute checksum for all batches
            for (int i = 0; i < BATCH_COUNT; i++)
            {
                var b = batches[i];
                b[7] = Checksum7(b);
            }

            return batches;
        }

        private void FillMotorRegion(
            List<byte[]> batches,
            MotorTrajectory motor,
            bool enabled,
            int startBatchIndex,
            int baseBatchIndex)
        {
            if (!enabled)
            {
                // Motor disabled: leave this 40-batch region as header + zeros + checksum.
                // (Checksum is added later in BuildFrameBatches.)
                return;
            }

            int totalSamples = motor.TotalSamples;

            for (int local = 0; local < ACTIVE_BATCHES_PER_MOTOR; local++)
            {
                int g = startBatchIndex + local;
                if (g >= totalSamples)
                    break;

                int batchIndex = baseBatchIndex + local;
                if (batchIndex < 0 || batchIndex >= BATCH_COUNT)
                    break;

                byte[] b = batches[batchIndex];

                int value = motor[g];
                PutLeI32(b, 1, value);
                PutLeI16(b, 5, g);
            }
        }

        private static void PutLeI32(byte[] buf, int idx, int v)
        {
            unchecked
            {
                uint v32 = (uint)v;
                buf[idx + 0] = (byte)((v32 >> 0) & 0xFF);
                buf[idx + 1] = (byte)((v32 >> 8) & 0xFF);
                buf[idx + 2] = (byte)((v32 >> 16) & 0xFF);
                buf[idx + 3] = (byte)((v32 >> 24) & 0xFF);
            }
        }

        private static void PutLeI16(byte[] buf, int idx, int v)
        {
            unchecked
            {
                ushort v16 = (ushort)v;
                buf[idx + 0] = (byte)((v16 >> 0) & 0xFF);
                buf[idx + 1] = (byte)((v16 >> 8) & 0xFF);
            }
        }

        private static byte Checksum7(byte[] b)
        {
            int s = 0;
            for (int k = 0; k < 7; k++)
            {
                s = (s + b[k]) & 0xFF;
            }
            return (byte)s;
        }

        private static string BatchBytesAsHexLine(byte[] batch)
        {
            var sb = new StringBuilder(batch.Length * 3);
            for (int i = 0; i < batch.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(batch[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }
    }
}
