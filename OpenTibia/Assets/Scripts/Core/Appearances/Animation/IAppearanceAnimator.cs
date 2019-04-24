namespace OpenTibiaUnity.Core.Appearances.Animation
{
    public interface IAppearanceAnimator
    {
        bool Finished { get; set; }
        int Phase { get; set; }
        int LastAnimationTick { get; }

        void Animate(int ticks, int delay = 0);

        void SetEndless();

        void Reset();

        IAppearanceAnimator Clone();
    }

    public interface IAppearanceFrameStategy
    {
        int NextFrame(int phase, int phaseCount);
        void Reset();
    }

    public class PingPongFrameStrategy : IAppearanceFrameStategy
    {
        private const int PhaseForward = 0;
        private const int PhaseBackword = 1;

        private int m_CurrentDirection = 0;

        public int NextFrame(int phase, int phaseCount) {
            int phaseConstant = m_CurrentDirection == PhaseForward ? 1 : -1;
            int tmpPhase = phase + phaseConstant;

            if (tmpPhase < 0 || tmpPhase >= phaseCount) {
                m_CurrentDirection = m_CurrentDirection == PhaseForward ? PhaseBackword : PhaseForward;
                phaseConstant *= -1;
            }

            return phase + phaseConstant;
        }

        public void Reset() {
            m_CurrentDirection = PhaseForward;
        }
    }

    public class LoopFrameStrategy : IAppearanceFrameStategy
    {
        private readonly uint m_LoopCount;
        private uint m_CurrentLoop = 0;

        public LoopFrameStrategy(uint loopCount) {
            m_LoopCount = loopCount;
        }

        public int NextFrame(int phase, int phaseCount) {
            int tmpPhase = phase + 1;
            if (tmpPhase < phaseCount)
                return tmpPhase;

            if (m_CurrentLoop < m_LoopCount - 1 || m_LoopCount == 0) {
                m_CurrentLoop++;
                return 0;
            }

            return phase;
        }

        public void Reset() {
            m_CurrentLoop = 0;
        }
    }
}
