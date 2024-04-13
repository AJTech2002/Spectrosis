using System;

namespace Synth.Filter {
    public class NotchFilter : Filter {
        public NotchFilter(float sampleRate, float center, float bandwidth) {
            var w0 = 2 * Math.PI * center / sampleRate;
            var cosw0 = Math.Cos(w0);
            var sinw0 = Math.Sin(w0);
            var alpha = sinw0 / (2 * bandwidth);
            
            // b0 = 1
            var b1 = -2 * cosw0;
            // b2 = 1
            var a0 = 1 + alpha;
            var a1 = -2 * cosw0;
            var a2 = 1 - alpha;
            
            B0 = 1 / a0;
            B1 = b1 / a0;
            B2 = 1 / a0;
            A1 = a1 / a0;
            A2 = a2 / a0;
        }
    }
}