using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Animation
{
    public class EnhancedAnimator : IAppearanceAnimator
    {
        // constants for the new movement animation formula
        public const int MinSpeedFrameDuration = 90 * 8;
        public const int MaxSpeedFrameDuration = 35 * 8;

        public const int MinSpeedDelay = 500;
        public const int MaxSpeedDelay = 100;

        private Protobuf.Appearances.SpriteAnimation _animationInstance;
        private IAppearanceFrameStategy _nextFrameStrategy;

        private readonly int _phaseCount;
        private int _currentPhaseDuration = 0;
        private int _currentPhase = 0;

        public int LastAnimationTick { get; private set; } = 0;
        public bool Finished { get; set; } = false;

        public int Phase {
            get => _currentPhase;
            set {
                if (_animationInstance == null) {
                    return;
                } else if (_animationInstance.Synchronized) {
                    CalculateSynchronousPhase();
                } else {
                    if (value == Constants.PhaseAsynchronous)
                        _currentPhase = 0;
                    else if (value == Constants.PhaseRandom)
                        _currentPhase = (int)(Random.Range(0, 10) / 10f * _phaseCount);
                    else if (value >= 0 && value < _phaseCount)
                        _currentPhase = value;
                    else
                        _currentPhase = (int)_animationInstance.DefaultStartPhase;
                }
            }
        }

        public EnhancedAnimator(Protobuf.Appearances.SpriteAnimation animation, int phaseCount) {
            _animationInstance = animation;
            _phaseCount = phaseCount;
            LastAnimationTick = OpenTibiaUnity.TicksMillis;

            Phase = Constants.PhaseAutomatic;

            if (_animationInstance?.LoopCount < 0)
                _nextFrameStrategy = new PingPongFrameStrategy();
            else
                _nextFrameStrategy = new LoopFrameStrategy((uint)_animationInstance?.LoopCount);
        }

        public void Animate(int ticks, int delay = 0) {
            // if delay != 0, then this is movement animation delay
            if (ticks != LastAnimationTick && !Finished) {
                int elapsedTicks = ticks - LastAnimationTick;
                if (elapsedTicks >= _currentPhaseDuration) {
                    var nextPhase = _nextFrameStrategy.NextFrame(_currentPhase, _phaseCount);
                    if (_currentPhase != nextPhase) {
                        int duration = delay == 0
                            ? _animationInstance.SpritePhases[nextPhase].Duration() - (elapsedTicks - _currentPhaseDuration)
                            : CalculateMovementPhaseDuration(delay);

                        if (duration < 0 && _animationInstance != null && _animationInstance.Synchronized) {
                            CalculateSynchronousPhase();
                        } else {
                            _currentPhase = nextPhase;
                            _currentPhaseDuration = Mathf.Max(0, duration);
                        }
                    } else {
                        Finished = true;
                    }
                } else {
                    _currentPhaseDuration -= elapsedTicks;
                }

                LastAnimationTick = ticks;
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

        private void CalculateSynchronousPhase() {
            int totalDurations = 0;
            for (int i = 0; i < _phaseCount; i++)
                totalDurations += _animationInstance.SpritePhases[i].Duration();

            int ticks = OpenTibiaUnity.TicksMillis;
            int loc4 = ticks % totalDurations;

            int tmpDurations = 0;
            for (int i = 0; i < _phaseCount; i++) {
                int duration = _animationInstance.SpritePhases[i].Duration();
                if (loc4 >= duration && loc4 < duration + tmpDurations) {
                    _currentPhase = i;
                    int loc8 = loc4 - tmpDurations;
                    _currentPhaseDuration = duration - loc8;
                    break;
                }

                tmpDurations += duration;
            }

            LastAnimationTick = ticks;
        }

        private int CalculateMovementPhaseDuration(int delay) {
            delay = Mathf.Clamp(delay, MaxSpeedDelay, MinSpeedDelay);
            int loc5 = (delay - MaxSpeedDelay) / (MinSpeedDelay - MaxSpeedDelay);
            int loc6 = MinSpeedFrameDuration / _phaseCount;
            int loc7 = MaxSpeedFrameDuration / _phaseCount;

            return (loc6 - loc7) * loc5 + loc7;
        }

        public IAppearanceAnimator Clone() {
            return new EnhancedAnimator(_animationInstance, _phaseCount); 
        }

        public static bool operator !(EnhancedAnimator instance) {
            return instance == null;
        }

        public static bool operator true(EnhancedAnimator instance) {
            return !!instance;
        }

        public static bool operator false(EnhancedAnimator instance) {
            return !instance;
        }
    }
}
