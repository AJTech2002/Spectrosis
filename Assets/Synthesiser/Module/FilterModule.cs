using System;
using NAudio.Wave;
using Synth.Filter;

namespace Synth.Module {
    public class FilterModule : ISampleProvider {
        public WaveFormat WaveFormat => source.WaveFormat;
        
        public bool Enabled { get; set; }
        
        public FilterType Type {
	        get => type;
	        set { type = value; SetFilter(); }
        }
        
        public int Frequency {
	        get => frequency;
	        set { frequency = value; SetFilter(); }
        }
        
        public float Bandwidth {
	        get => bandwidth;
	        set { bandwidth = value; SetFilter(); }
        }

        private FilterType type;
        private int frequency;
        private float bandwidth;

        private readonly ISampleProvider source;
        private Filter.Filter filter;

        public FilterModule(ISampleProvider source, FilterType type = FilterType.LowPass, int frequency = 3000,
            float bandwidth = 0.3f) {
            this.source = source;

            Type = type;
            Frequency = frequency;
            Bandwidth = bandwidth;

            SetFilter();
        }

        public int Read(float[] buffer, int offset, int count) {
            var samples = source.Read(buffer, offset, count);

            if (!Enabled)
	            return samples;

            for (var i = 0; i < samples; i++)
				buffer[offset + i] = filter.Process(buffer[offset + i]);
            
	        return samples;
        }

        private void SetFilter() {
	        filter = type switch {
		        FilterType.LowPass => new LowPassFilter(WaveFormat.SampleRate, frequency, bandwidth),
		        FilterType.HighPass => new HighPassFilter(WaveFormat.SampleRate, frequency, bandwidth),
		        FilterType.Notch => new NotchFilter(WaveFormat.SampleRate, frequency, bandwidth),
		        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	        };
        }
    }
}