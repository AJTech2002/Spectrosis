using System;

namespace Synth {
	public class Envelope {
		private const float AttackRatio = 0.3f;
		private const float DecayRatio = 0.0001f;
		
		public enum EnvelopeState {
			Idle = 0, Attack, Decay, Sustain, Release
		};

		public EnvelopeState State { get; private set; }

		public float AttackRate {
			get => attackRate;
			set {
				attackRate = value;
				attackCoefficient = ComputeCoeficient(value, AttackRatio);
				attackBase = (1f + AttackRatio) * (1f - attackCoefficient);
			}
		}
        
		public float DecayRate {
			get => decayRate;
			set {
				decayRate = value;
				decayCoefficient = ComputeCoeficient(value, DecayRatio);
				decayBase = (sustainLevel - DecayRatio) * (1f - decayCoefficient);
			}
		}
		
		public float SustainLevel {
			get => sustainLevel;
			set {
				sustainLevel = value;
				decayBase = (sustainLevel - DecayRatio) * (1f - decayCoefficient);
			}
		}
        
		public float ReleaseRate {
			get => releaseRate;
			set {
				releaseRate = value;
				releaseCoefficient = ComputeCoeficient(value, DecayRatio);
				releaseBase = -DecayRatio * (1f - releaseCoefficient);
			}
		}
		
		private float output;
        private float attackRate;
        private float decayRate;
        private float releaseRate;
        private float attackCoefficient;
        private float decayCoefficient;
        private float releaseCoefficient;
        private float attackBase;
        private float decayBase;
        private float releaseBase;
        private float sustainLevel;
        
        public Envelope() {
	        State = EnvelopeState.Idle;
	        output = 0f;
	        
            AttackRate = 0f;
            DecayRate = 0f;
            ReleaseRate = 0f;
            SustainLevel = 1f;
        }
        
        public float Process() {
            switch (State) {
                case EnvelopeState.Idle: break;
                case EnvelopeState.Attack:
                    output = attackBase + output * attackCoefficient;
                    
                    if (output >= 1f) {
                        output = 1f;
                        State = EnvelopeState.Decay;
                    }
                    
                    break;
                case EnvelopeState.Decay:
                    output = decayBase + output * decayCoefficient;
                    
                    if (output <= sustainLevel) {
                        output = sustainLevel;
                        State = EnvelopeState.Sustain;
                    }
                    
                    break;
                case EnvelopeState.Sustain: break;
                case EnvelopeState.Release:
                    output = releaseBase + output * releaseCoefficient;
                    
                    if (output <= 0f) {
                        output = 0f;
                        State = EnvelopeState.Idle;
                    }
                    
                    break;
                default:
	                throw new ArgumentOutOfRangeException();
            }
            
            return output;
        }

        public void GateOn() {
	        State = EnvelopeState.Attack;
        }

        public void GateOff() {
	        if (State != EnvelopeState.Idle)
		        State = EnvelopeState.Release;
        }
        
        private static float ComputeCoeficient(float rate, float ratio)  {
	        return (float) Math.Exp(-Math.Log((1f + ratio) / ratio) / rate);
        }
	}
}