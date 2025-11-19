using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motor_Trajectory_Component
{
    /// <summary>
    /// Parameters for one wave component (sine or cosine).
    /// </summary>
    public struct WaveParams
    {
        public bool Enabled;       // use this component or not
        public double FrequencyHz;   // Hz
        public int Amplitude;     // int scale for your stepper
        public int Offset;        // DC offset
        public double PhaseRad;      // phase in radians

        public WaveParams(bool enabled,
                          double frequencyHz,
                          int amplitude,
                          int offset,
                          double phaseRad)
        {
            Enabled = enabled;
            FrequencyHz = frequencyHz;
            Amplitude = amplitude;
            Offset = offset;
            PhaseRad = phaseRad;
        }
    }

    /// <summary>
    /// Builds a single motor trajectory: int32 samples (0..TotalSamples-1)
    /// based on BatchMs, PeriodSeconds, and Sine/Cosine WaveParams.
    /// </summary>
    public class MotorTrajectory
    {
        // --- timing properties (configurable) ---
        public int BatchMs { get; }
        public double PeriodSeconds { get; }
        public int TotalSamples { get; }

        // --- wave components ---
        public WaveParams Sine { get; }
        public WaveParams Cosine { get; }

        private readonly List<int> _values;
        public IReadOnlyList<int> Values => _values;

        public int this[int index] => _values[index];

        /// <summary>
        /// Full constructor: you control timing and both wave components.
        /// </summary>
        public MotorTrajectory(
            WaveParams sine,
            WaveParams cosine,
            int batchMs,
            double periodSeconds)
        {
            if (batchMs <= 0) throw new ArgumentOutOfRangeException(nameof(batchMs));
            if (periodSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(periodSeconds));

            Sine = sine;
            Cosine = cosine;
            BatchMs = batchMs;
            PeriodSeconds = periodSeconds;

            TotalSamples = (int)Math.Round(PeriodSeconds * 1000.0 / BatchMs);
            _values = BuildValues();
        }

        /// <summary>
        /// Convenience ctor matching your original defaults:
        /// BatchMs = 25 ms, PeriodSeconds = 20 s,
        /// sine+cosine both enabled, same amplitude/offset/phase.
        /// </summary>
        public MotorTrajectory(
            double freqSine,
            double freqCosine,
            int amplitude,
            int offset,
            double phaseRad,
            bool useCosine = true)
            : this(
                new WaveParams(
                    enabled: true,
                    frequencyHz: freqSine,
                    amplitude: amplitude,
                    offset: offset,
                    phaseRad: phaseRad),
                new WaveParams(
                    enabled: useCosine,
                    frequencyHz: freqCosine,
                    amplitude: amplitude,
                    offset: offset,
                    phaseRad: phaseRad),
                batchMs: 25,
                periodSeconds: 20.0)
        {
        }

        private List<int> BuildValues()
        {
            var vals = new List<int>(TotalSamples);
            double dt = BatchMs / 1000.0; // seconds per sample

            for (int k = 0; k < TotalSamples; k++)
            {
                double t = k * dt;
                double v = 0.0;

                if (Sine.Enabled)
                {
                    v += Sine.Offset
                         + Sine.Amplitude * Math.Sin(2.0 * Math.PI * Sine.FrequencyHz * t + Sine.PhaseRad);
                }

                if (Cosine.Enabled)
                {
                    v += Cosine.Offset
                         + Cosine.Amplitude * Math.Cos(2.0 * Math.PI * Cosine.FrequencyHz * t + Cosine.PhaseRad);
                }

                vals.Add((int)Math.Round(v, MidpointRounding.AwayFromZero));
            }

            return vals;
        }
    }
}
