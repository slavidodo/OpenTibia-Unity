namespace OpenTibiaUnity.Core.Creatures
{
    public class SkillCounter
    {
        protected const int NumDataPoints = 15;

        protected double[] m_DataPoints;
        protected uint[] m_DataTimestamps;

        public SkillCounter() {
            m_DataPoints = new double[SkillCounter.NumDataPoints];
            m_DataTimestamps = new uint[SkillCounter.NumDataPoints];
            Reset();
        }

        public void AddSkillGain(double gain) {
            if (gain < 0)
                throw new System.ArgumentException("SkillCounter.AddSkillGain: Gain can't be negative.");

            uint ticks = (uint)(OpenTibiaUnity.TicksMillis / 60000);
            int index = (int)(ticks % SkillCounter.NumDataPoints);
            if (m_DataTimestamps[index] != ticks) {
                m_DataTimestamps[index] = ticks;
                m_DataPoints[index] = gain;
            } else {
                m_DataPoints[index] = m_DataPoints[index] + gain;
            }
        }

        public double GetAverageGain() {
            double avg = 0;
            uint ticks = (uint)(OpenTibiaUnity.TicksMillis / 60000);
            for (int i = SkillCounter.NumDataPoints - 1; i >= 0; i--) {
                if (m_DataTimestamps[i] + SkillCounter.NumDataPoints >= ticks)
                    avg += m_DataPoints[i];
            }

            return (avg * 60) / SkillCounter.NumDataPoints;
        }

        public void Reset() {
            for (int i = 0; i < SkillCounter.NumDataPoints; i++) {
                m_DataPoints[i] = 0;
                m_DataTimestamps[i] = 0;
            }
        }
    }
}
