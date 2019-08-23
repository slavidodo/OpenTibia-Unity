namespace OpenTibiaUnity.Core.Creatures
{
    public class SkillCounter
    {
        protected const int NumDataPoints = 15;

        protected double[] _dataPoints;
        protected uint[] _dataTimestamps;

        public SkillCounter() {
            _dataPoints = new double[SkillCounter.NumDataPoints];
            _dataTimestamps = new uint[SkillCounter.NumDataPoints];
            Reset();
        }

        public void AddSkillGain(double gain) {
            if (gain < 0)
                throw new System.ArgumentException("SkillCounter.AddSkillGain: Gain can't be negative.");

            uint ticks = (uint)(OpenTibiaUnity.TicksMillis / 60000);
            int index = (int)(ticks % SkillCounter.NumDataPoints);
            if (_dataTimestamps[index] != ticks) {
                _dataTimestamps[index] = ticks;
                _dataPoints[index] = gain;
            } else {
                _dataPoints[index] = _dataPoints[index] + gain;
            }
        }

        public double GetAverageGain() {
            double avg = 0;
            uint ticks = (uint)(OpenTibiaUnity.TicksMillis / 60000);
            for (int i = SkillCounter.NumDataPoints - 1; i >= 0; i--) {
                if (_dataTimestamps[i] + SkillCounter.NumDataPoints >= ticks)
                    avg += _dataPoints[i];
            }

            return (avg * 60) / SkillCounter.NumDataPoints;
        }

        public void Reset() {
            for (int i = 0; i < SkillCounter.NumDataPoints; i++) {
                _dataPoints[i] = 0;
                _dataTimestamps[i] = 0;
            }
        }
    }
}
