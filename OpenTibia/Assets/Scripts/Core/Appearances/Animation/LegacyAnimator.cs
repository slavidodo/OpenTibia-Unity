using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Animation
{
    public class LegacyAnimator : IAppearanceAnimator
    {

        private IAppearanceFrameStategy _nextFrameStrategy;
        private LegacyOutfitFrameStrategy _outfitFrameStrategy;

        private readonly int _phaseCount;
        private readonly int _walkPhaseCount;
        private int _currentPhaseDuration = 0;
        private int _currentPhase = 0;

        private bool _isCreature  = false;

        public int LastAnimationTick { get; private set; } = 0;
        public bool Finished { get; set; } = false;
        public int PhaseDuration { get; set; }
        public bool Async { get; set; } = true;

        public int Phase {
            get => _currentPhase;
            set {
                if (Async) {
                    if (value == Constants.PhaseAsynchronous)
                        _currentPhase = 0;
                    else if (value == Constants.PhaseRandom)
                        _currentPhase = (int)(Random.Range(0, 10) / 10f * _phaseCount);
                    else if (value >= 0 && value < _phaseCount)
                        _currentPhase = value;
                    else
                        _currentPhase = 0;
                } else {
                    CalculateSynchronousPhase(OpenTibiaUnity.TicksMillis);
                }
            }
        }
        
        public AppearanceType AppearanceType { get; set; }

        public LegacyAnimator(int phaseCount, int phaseDuration = 0) {
            _phaseCount = phaseCount;
            _walkPhaseCount = _phaseCount > 1 ? _phaseCount - 1 : 0;

            LastAnimationTick = OpenTibiaUnity.TicksMillis;

            if (phaseDuration != 0)
                PhaseDuration = phaseDuration;
            else if (_phaseCount != 0)
                PhaseDuration = 1000 / _phaseCount;
            else
                PhaseDuration = 80;
        }

        public void Initialise(AppearanceType type) {
            if (type.IsAnimateAlways || type.Category == AppearanceCategory.Object) {
                _nextFrameStrategy = new LoopFrameStrategy(0);
            } else if (type.Category == AppearanceCategory.Outfit) {
                _isCreature = true;
                _outfitFrameStrategy = new LegacyOutfitFrameStrategy();
                _nextFrameStrategy = _outfitFrameStrategy;
            } else {
                _nextFrameStrategy = new LoopFrameStrategy(1);
            }
        }

        public void Animate(int ticks, int delay = 0) {
            if (ticks != LastAnimationTick && !Finished) {
                int elapsedTicks = ticks - LastAnimationTick;
                if (elapsedTicks >= _currentPhaseDuration) {
                    if (_isCreature)
                        _outfitFrameStrategy.UpdateState(delay != 0);

                    var nextPhase = _nextFrameStrategy.NextFrame(_currentPhase, _phaseCount);
                    if (_currentPhase != nextPhase) {
                        int duration = delay == 0
                            ? PhaseDuration - (elapsedTicks - _currentPhaseDuration)
                            : CalculateMovementPhaseDuration(delay);

                        if (duration < 0 && !Async) {
                            CalculateSynchronousPhase(ticks);
                        } else {
                            _currentPhase = nextPhase;
                            _currentPhaseDuration = Mathf.Max(0, duration);
                        }
                    } else {
                        Finished = true;
                    }

                    LastAnimationTick = ticks;
                }
            }
        }

        public void SetEndless() {
            _nextFrameStrategy = new LoopFrameStrategy(0);
        }

        public void Reset() {
            Phase = Constants.PhaseAutomatic;
            Finished = false;
            _nextFrameStrategy.Reset();
        }

        private void CalculateSynchronousPhase(int ticks) {
            int fullCycleDuration = PhaseDuration * _phaseCount;
            int durationLeft = ticks % fullCycleDuration;

            int tmpDurations = 0;
            for (int i = 0; i < _phaseCount; i++) {
                if (durationLeft >= PhaseDuration && durationLeft < PhaseDuration + tmpDurations) {
                    _currentPhase = i;
                    _currentPhaseDuration = PhaseDuration - (durationLeft - tmpDurations);
                    break;
                }

                tmpDurations += PhaseDuration;
            }

            LastAnimationTick = ticks;
        }

        private int CalculateMovementPhaseDuration(int delay) {
            return delay / 3;
        }

        public IAppearanceAnimator Clone() {
            return new LegacyAnimator(_phaseCount);
        }

        public static bool operator !(LegacyAnimator instance) {
            return instance == null;
        }

        public static bool operator true(LegacyAnimator instance) {
            return !!instance;
        }

        public static bool operator false(LegacyAnimator instance) {
            return !instance;
        }
    }

    public class LegacyOutfitFrameStrategy : IAppearanceFrameStategy
    {
        private bool _walking = false;

        public void UpdateState(bool walking) {
            _walking = walking;
        }

        public int NextFrame(int phase, int phaseCount) {
            if (!_walking)
                return 0;


            int tmpPhase = phase + 1;
            if (tmpPhase < phaseCount)
                return tmpPhase;

            return 0;
        }

        public void Reset() {
            _walking = false;
        }
    }
}
