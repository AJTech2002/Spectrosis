using NAudio.Wave;

namespace Synth.Module {
	public class TremoloModule : ISampleProvider {
		public WaveFormat WaveFormat => source.WaveFormat;
		
		public bool Enabled { get; set; }

		public int Frequency {
			get => frequency;
			set { frequency = value; SetLFO(); }
		}

		public float Amplitude {
			get => amplitude;
			set { amplitude = value; SetLFO(); }
		}

		private int frequency;
		private float amplitude;

		private readonly ISampleProvider source;
		private SignalModule lfo;

		public TremoloModule(ISampleProvider source, int frequency = 5, float amplitude = 0.2f) {
			this.source = source;
			
			Frequency = frequency;
			Amplitude = amplitude;
			
			SetLFO();
		}

		public int Read(float[] buffer, int offset, int count) {
			var samples = source.Read(buffer, offset, count);

			if (!Enabled)
				return samples;
			
			var lfoBuffer = new float[count];
			lfo.Read(lfoBuffer, offset, count);

			for (var i = 0; i < samples; i++)
				if (lfo.Gain > 0.0f) 
					buffer[offset + i] += buffer[offset + i] * lfoBuffer[offset + i];

			return samples;
		}
		
		private void SetLFO() {
			lfo = new SignalModule(SignalType.Sine, frequency, amplitude);
		}
	}
}