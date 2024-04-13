namespace Synth.Filter {
    public enum FilterType {
        LowPass, HighPass, Notch
    }

    public abstract class Filter {
        protected double B0;
        protected double B1;
        protected double B2;
        protected double A1;
        protected double A2;

        private float x1;
        private float x2;
        private float y1;
        private float y2;

        public float Process(float sample) {
            var result = B0 * sample + B1 * x1 + B2 * x2 - A1 * y1 - A2 * y2;
            
            x2 = x1;
            x1 = sample;
            
            y2 = y1;
            y1 = (float) result;

            return y1;
        }
    }
}