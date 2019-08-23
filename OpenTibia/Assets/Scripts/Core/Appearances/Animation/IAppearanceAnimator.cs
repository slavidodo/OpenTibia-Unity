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

        private int _currentDirection = 0;

        public int NextFrame(int phase, int phaseCount) {
            int phaseConstant = _currentDirection == PhaseForward ? 1 : -1;
            int tmpPhase = phase + phaseConstant;

            if (tmpPhase < 0 || tmpPhase >= phaseCount) {
                _currentDirection = _currentDirection == PhaseForward ? PhaseBackword : PhaseForward;
                phaseConstant *= -1;
            }

            return phase + phaseConstant;
        }

        public void Reset() {
            _currentDirection = PhaseForward;
        }
    }

    public class LoopFrameStrategy : IAppearanceFrameStategy
    {
        private readonly uint _loopCount;
        private uint _currentLoop = 0;

        public LoopFrameStrategy(uint loopCount) {
            _loopCount = loopCount;
        }

        public int NextFrame(int phase, int phaseCount) {
            int tmpPhase = phase + 1;
            if (tmpPhase < phaseCount)
                return tmpPhase;

            if (_currentLoop < _loopCount - 1 || _loopCount == 0) {
                _currentLoop++;
                return 0;
            }

            return phase;
        }

        public void Reset() {
            _currentLoop = 0;
        }
    }
}
