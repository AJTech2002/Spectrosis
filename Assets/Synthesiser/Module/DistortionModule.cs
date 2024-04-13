using System;
using NAudio.Wave;

namespace Synth.Module {
    public class DistortionModule : ISampleProvider {
        public WaveFormat WaveFormat => source.WaveFormat;

        public bool Enabled { get; set; }

        public float Amount { get; set; }
        
        public float Mix { get; set; }

        public float MaxAmplitude { get; set; } = 0.25f;

        private readonly ISampleProvider source;

        public DistortionModule(ISampleProvider source, float amount = 2f) {
            this.source = source;
			
            Amount = amount;
        }

        public int Read(float[] buffer, int offset, int count) {
            var samples = source.Read(buffer, offset, count);

            if (!Enabled)
                return samples;

            for (var i = 0; i < samples; i++)
                buffer[i] = Process(buffer[i]);

            return samples;
        }

        private float Process(float sample)
        {
            var dry = sample;
            
            // Overdrive
            sample *= Amount;
            
            // Clamp to [-1, 1]
            Math.Clamp(sample, -1, 1);

            // Scale to a maximum amplitude
            sample *= MaxAmplitude;
            
            return sample * Mix + dry * (1 - Mix);
        }
    }
}