using System;

namespace Synth.Filter {
    public class LowPassFilter : Filter {
        public LowPassFilter(float sampleRate, float cutoff, float bandwidth) {
            var w0 = 2 * Math.PI * cutoff / sampleRate;
            var cosw0 = Math.Cos(w0);
            var alpha = Math.Sin(w0) / (2 * bandwidth);

            var b0 = (1 - cosw0) / 2;
            var b1 = 1 - cosw0;
            var b2 = (1 - cosw0) / 2;
            var a0 = 1 + alpha;
            var a1 = -2 * cosw0;
            var a2 = 1 - alpha;

            B0 = b0 / a0;
            B1 = b1 / a0;
            B2 = b2 / a0;
            A1 = a1 / a0;
            A2 = a2 / a0;
        }
    }
}