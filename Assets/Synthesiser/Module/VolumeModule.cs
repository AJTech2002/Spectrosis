using NAudio.Wave;

namespace Synth.Module {
	public class VolumeModule : ISampleProvider {
		public WaveFormat WaveFormat => source.WaveFormat;

		public float Volume { get; set; } = 1f;
		
		private readonly ISampleProvider source;
		
		public VolumeModule(ISampleProvider source) {
			this.source = source;
		}

		public int Read(float[] buffer, int offset, int count) {
			var samples = source.Read(buffer, offset, count);
			
			if (Volume == 1f) 
				return samples;
			
			for (var i = 0; i < count; i++)
				buffer[i + offset] *= Volume;
			
			return samples;
		}
	}
}