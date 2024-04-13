using NAudio.Wave;

namespace Synth.Module {
	public class EnvelopeModule : ISampleProvider {
		public WaveFormat WaveFormat => source.WaveFormat;
		
		public float AttackTime {
			get => attackTime;
			set {
				attackTime = value;
				adsr.AttackRate = attackTime * WaveFormat.SampleRate;
			}
		}

		public float DecayTime {
			get => decayTime;
			set {
				decayTime = value;
				adsr.DecayRate = decayTime * WaveFormat.SampleRate;
			}
		}

		public float SustainLevel {
			get => sustainLevel;
			set {
				sustainLevel = value;
				adsr.SustainLevel = sustainLevel;
			}
		}

		public float ReleaseTime {
			get => releaseTime;
			set {
				releaseTime = value;
				adsr.ReleaseRate = releaseTime * WaveFormat.SampleRate;
			}
		}

		private readonly ISampleProvider source;
        private readonly Envelope adsr;
        private float attackTime;
        private float decayTime;
        private float sustainLevel;
        private float releaseTime;
        
        public EnvelopeModule(ISampleProvider source, 
	        float attack = 0.01f, 
	        float decay = 0f, 
	        float sustain = 1f,
	        float release = 0.3f) {
	        
	        this.source = source;
            adsr = new Envelope();
            
            AttackTime = attack;
            DecayTime = decay;
            SustainLevel = sustain;
            ReleaseTime = release;
            
            adsr.GateOn();
        }

        public int Read(float[] buffer, int offset, int count) {
            if (adsr.State == Envelope.EnvelopeState.Idle) 
	            return 0;
            
            var samples = source.Read(buffer, offset, count);
            
            for (var n = 0; n < samples; n++)
                buffer[offset++] *= adsr.Process();
            
            return samples;
        }
        
        public void Stop() {
            adsr.GateOff();
        }
	}
}