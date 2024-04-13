using System;
using NAudio.Wave;

namespace Synth.Module {
	public class DelayModule : ISampleProvider {
		public WaveFormat WaveFormat => source.WaveFormat;

		public bool Enabled { get; set; }

		public double DelayMs {
			get => delayMs;
			set { delayMs = value; delayPosition = 0; Reslide(); }
		}

		public float Feedback {
			get => feedback;
			set { feedback = value; Reslide(); }
		}

		public float Mix {
			get => mix;
			set { mix = value; Reslide(); }
		}
		
		public float OutputWet {
			get => outputWet;
			set { outputWet = value; Reslide(); }
		}
		
		public float OutputDry {
			get => outputDry;
			set { outputDry = value; Reslide(); }
		}

		private double delayMs;
		private float feedback;
		private float mix;
		private float outputWet;
		private float outputDry;
		
		private readonly ISampleProvider source;
		private float delayPosition;
		private float delayLength;
		private float delay;
		private float resamplePosition;
		private int resamplePositionInt;
		private float delayResamplePosition;
		private int pos;
		private float[] delayBuffer = new float[500000];

		public DelayModule(ISampleProvider source, double delay = 1, float feedback = 0.5f, float mix = 0.5f,
			float wet = 0.25f, float dry = 1f) {
			
			this.source = source;
			DelayMs = delay;
			Feedback = feedback;
			Mix = mix;
			OutputWet = wet;
			OutputDry = dry;
			
			delayPosition = 0;

			Reslide();
		}

		public int Read(float[] buffer, int offset, int count) {
			var samples = source.Read(buffer, offset, count);

			if (!Enabled)
				return samples;

			for (var i = 0; i < samples; i++)
				buffer[offset + i] = Process(buffer[offset + i]);

			return samples;
		}

		private float Process(float inputSample) {
			var delayPositionInt = (int) delayPosition;
			var outputSample = delayBuffer[delayPositionInt];

			delayBuffer[delayPositionInt] = Math.Min(Math.Max(inputSample * Mix + outputSample * Feedback, -4), 4);

			if ((delayPosition += 1) >= delayLength)
				delayPosition = 0;

			return inputSample * OutputDry + outputSample * OutputWet;
		}

		private void Reslide() {
			delay = delayLength;
			delayLength = (float) Math.Min((DelayMs * WaveFormat.SampleRate) / 1000, delayBuffer.Length);

			if (delay == delayLength)
				return;

			if (delay > delayLength) {
				resamplePosition = 0;
				resamplePositionInt = 0;
				delayResamplePosition = delay / delayLength;

				for (var i = 0; i < delayLength; i++) {
					pos = (int) resamplePosition * 2;
					delayBuffer[resamplePositionInt] = delayBuffer[pos];
					delayBuffer[resamplePositionInt + 1] = delayBuffer[pos + 1];
					resamplePositionInt += 2;
					resamplePosition += delayResamplePosition;
				}

				delayPosition = delayResamplePosition != 0.0f ? delayPosition / delayResamplePosition : delayPosition;
				delayPosition = (delayPosition < 0) ? 0 : (int) delayPosition;
			}
			else if (delay < delayLength) {
				delayResamplePosition = delay / delayLength;
				resamplePosition = delay;
				resamplePositionInt = (int) delayLength * 2;

				for (var i = 0; i < (int) delayLength; i++) {
					resamplePosition -= delayResamplePosition;
					resamplePositionInt -= 2;

					pos = Math.Abs((int) resamplePosition * 2);
					delayBuffer[resamplePositionInt] = delayBuffer[pos];
					delayBuffer[resamplePositionInt + 1] = delayBuffer[pos + 1];
				}

				delayPosition = delayResamplePosition != 0.0f ? delayPosition / delayResamplePosition : delayPosition;
				delayPosition = (delayPosition < 0) ? 0 : (int) delayPosition;
			}
		}
	}
}