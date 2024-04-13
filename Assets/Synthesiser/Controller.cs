using System;
using System.Collections.Generic;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Synth.Filter;
using Synth.Module;

namespace Synth {
	public class Controller {
		public bool Osc1Enable {
			set {
				osc1Enable = value;
				CheckEnable();
			}
		}

		public bool Osc2Enable {
			set {
				osc2Enable = value;
				CheckEnable();
			}
		}

		public bool FilterEnable {
			get => filterModule?.Enabled ?? false;
			set => filterModule.Enabled = value;
		}

		public bool DistortEnable {
			get => distortionModule?.Enabled ?? false;
			set => distortionModule.Enabled = value;
		}

		public bool TremoloEnable {
			get => tremoloModule?.Enabled ?? false;
			set => tremoloModule.Enabled = value;
		}

		public bool DelayEnable {
			get => delayModule?.Enabled ?? false;
			set => delayModule.Enabled = value;
		}
		
		public float Osc1Volume {
			get => volumeControl1?.Volume ?? 0.25f;
			set {
				volumeControl1.Volume = value;
				distortionModule.MaxAmplitude = Math.Max(Osc1Volume, Osc2Volume);
			}
		}

		public float Osc2Volume {
			get => volumeControl2?.Volume ?? 0.25f;
			set {
				volumeControl2.Volume = value;
				distortionModule.MaxAmplitude = Math.Max(Osc1Volume, Osc2Volume);
			}
		}

		public SignalType Osc1Waveform { get; set; } = SignalType.Sine;

		public SignalType Osc2Waveform { get; set; } = SignalType.Sine;

		public FilterType FilterType {
			get => filterModule?.Type ?? FilterType.LowPass;
			set => filterModule.Type = value;
		}

		public int Osc1Octave { get; set; } = 3;

		public int Osc2Octave { get; set; } = 3;

		public float Attack { get; set; } = 0.01f;

		public float Decay { get; set; }

		public float Sustain { get; set; } = 1;

		public float Release { get; set; } = 0.3f;

		public int Cutoff {
			get => filterModule?.Frequency ?? 8000;
			set => filterModule.Frequency = value;
		}

		public float Bandwidth {
			get => filterModule?.Bandwidth ?? 0.5f;
			set => filterModule.Bandwidth = value;
		}

		public float DistortAmount {
			get => distortionModule?.Amount ?? 2f;
			set => distortionModule.Amount = value;
		}

		public float DistortMix {
			get => distortionModule?.Mix ?? 1f;
			set => distortionModule.Mix = value;
		}

		public int TremoloFrequency {
			get => tremoloModule?.Frequency ?? 5;
			set => tremoloModule.Frequency = value;
		}

		public float TremoloAmplitude {
			get => tremoloModule?.Amplitude ?? 0.2f;
			set => tremoloModule.Amplitude = value;
		}

		public double Delay {
			get => delayModule?.DelayMs ?? 1f;
			set => delayModule.DelayMs = value;
		}
		public float Feedback {
			get => delayModule?.Feedback ?? 0.5f;
			set => delayModule.Feedback = value;
		}
		public float Mix {
			get => delayModule?.Mix ?? 0.5f;
			set => delayModule.Mix = value;
		}
		public float Wet {
			get => delayModule?.OutputWet ?? 0.5f;
			set => delayModule.OutputWet = value;
		}
		public float Dry {
			get => delayModule?.OutputDry ?? 0.5f;
			set => delayModule.OutputDry = value;
		}

		private bool osc1Enable;
		private bool osc2Enable;

		private readonly EnvelopeModule[,] signals;
		private readonly MixingSampleProvider mixer1;
		private readonly MixingSampleProvider mixer2;
		private readonly VolumeModule volumeControl1;
		private readonly VolumeModule volumeControl2;
		private readonly MixingSampleProvider mixerAll;
		private readonly DistortionModule distortionModule;
		private readonly TremoloModule tremoloModule;
		private readonly DelayModule delayModule;
		private readonly FilterModule filterModule;
		public readonly VibratoModule vibratoModule;
		public readonly ChorusModule chorusModule;
		public readonly ReverbModule reverbModule;

		private IWavePlayer player;

		private double[] frequencies = new double[8];

        private readonly int[] majorScaleIntervals =
		{
			0,
			2,
			4,
			5,
			7,
			9,
			11,
			12
		};

		private readonly int[] minorScaleIntervals =
		{
			0,
			2,
			3,
			5,
			7,
			8,
			10,
			12
		};

        public Controller() {
			var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
			signals = new EnvelopeModule[2, 88];
			mixer1 = new MixingSampleProvider(waveFormat) { ReadFully = true };
			mixer2 = new MixingSampleProvider(waveFormat) { ReadFully = true };
			volumeControl1 = new VolumeModule(mixer1);
			volumeControl2 = new VolumeModule(mixer2);
			mixerAll = new MixingSampleProvider(waveFormat) { ReadFully = true };
			distortionModule = new DistortionModule(mixerAll);
			tremoloModule = new TremoloModule(distortionModule);
			delayModule = new DelayModule(tremoloModule);
			filterModule = new FilterModule(delayModule);
			chorusModule = new ChorusModule(filterModule);	
			//vibratoModule = new VibratoModule(filterModule);
			reverbModule = new ReverbModule(chorusModule);
			mixerAll.AddMixerInput(volumeControl1);
			mixerAll.AddMixerInput(volumeControl2);
			SetScale(0, true);
		}

		public void SetScale(int rootNote, bool isMajor)
		{
			frequencies = new double[8];
			if (isMajor)
			{
				for (int i = 0; i < majorScaleIntervals.Length; i++)
				{
					frequencies[i] = GetFrequency(majorScaleIntervals[i], rootNote);
                }
			}
			else
			{
                for (int i = 0; i < minorScaleIntervals.Length; i++)
				{
					frequencies[i] = GetFrequency(minorScaleIntervals[i], rootNote);
                }
            }
		}

        private double GetFrequency(int noteIndex, int root)
        {
            return (440 * Math.Pow(2.00, (noteIndex + root) / 12.00));
        }

        public void NoteDown(int keyIndex1, int keyIndex2) {
			if (osc1Enable && keyIndex1 >= 0 && signals[0, keyIndex1] == null) {
				signals[0, keyIndex1] = new EnvelopeModule(
					new SignalModule(Osc1Waveform, frequencies[keyIndex1]), Attack, Decay, Sustain, Release);
				mixer1.AddMixerInput(signals[0, keyIndex1]);
			}

			if (osc2Enable && keyIndex2 >= 0 && signals[1, keyIndex2] == null) {
				signals[1, keyIndex2] = new EnvelopeModule(
					new SignalModule(Osc2Waveform, frequencies[keyIndex2]), Attack, Decay, Sustain, Release);
				mixer2.AddMixerInput(signals[1, keyIndex2]);
			}
		}

		public void NoteUp(int keyIndex1, int keyIndex2) {
			if (keyIndex1 >= 0 && signals[0, keyIndex1] != null) {
				signals[0, keyIndex1].Stop();
				signals[0, keyIndex1] = null;
			}

			if (keyIndex2 >= 0 && signals[1, keyIndex2] != null) {
				signals[1, keyIndex2].Stop();
				signals[1, keyIndex2] = null;
			}
		}

		private void CheckEnable() {
			if (osc1Enable || osc2Enable)
				EnablePlayer();
			else
				DisablePlayer();
		}

		private void EnablePlayer() {
			if (player != null)
				return;
			
			player = new WaveOutEvent { NumberOfBuffers = 2, DesiredLatency = 100 };
			player.Init(new SampleToWaveProvider(reverbModule));
			player.Play();
		}

		private void DisablePlayer() {
			if (player == null)
				return;
			
			player.Dispose();
			player = null;
		}
	}
}