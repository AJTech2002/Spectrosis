using NAudio.Wave;

namespace Synth.Module
{
    public class VibratoModule : ISampleProvider
    {
        public WaveFormat WaveFormat => source.WaveFormat;

        public bool Enabled { get; set; }

        public int Frequency
        {
            get => frequency;
            set { frequency = value; SetLFO(); }
        }

        public float Amplitude
        {
            get => amplitude;
            set { amplitude = value; SetLFO(); }
        }

        private int frequency;
        private float amplitude;

        private readonly ISampleProvider source;
        private SignalModule lfo;

        public VibratoModule(ISampleProvider source, int frequency = 5, float amplitude = 0.2f)
        {
            this.source = source;

            Frequency = frequency;
            Amplitude = amplitude;

            SetLFO();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samples = source.Read(buffer, offset, count);

            if (!Enabled)
                return samples;

            var lfoBuffer = new float[count];
            lfo.Read(lfoBuffer, offset, count);

            for (var i = 0; i < samples; i++)
            {
                int intPart = (int)lfoBuffer[i];
                float fracPart = lfoBuffer[i] - intPart;

                int j = samples + i + intPart;
                float val1 = buffer[j];
                if (j >= samples - 1) j = 0;
                float val2 = buffer[j];
                float interpolated = intPart * val1 + fracPart * val2;
                buffer[i] = interpolated;
            }

            return samples;
        }

        private void SetLFO()
        {
            lfo = new SignalModule(SignalType.Sine, frequency, amplitude);
        }
    }
}
