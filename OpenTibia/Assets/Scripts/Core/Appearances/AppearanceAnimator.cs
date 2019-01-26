using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceAnimator
    {
        public const int AnimationDelayBeforeReset = 1000;
        public const int PhaseAutomatic = -1;
        public const int PhaseAsynchronous = 255;
        public const int PhaseRandom = 254;

        public const int AnimationAsynchron = 0;
        public const int AnimationSynchron = 1;

        public const int MinSpeedFrameDuration = 90 * 8;
        public const int MaxSpeedFrameDuration = 35 * 8;

        public const int MinSpeedDelay = 500;
        public const int MaxSpeedDelay = 100;

        public int LastAnimationTick { get; private set; } = 0;

        private Proto.Appearances001.FrameAnimation m_AnimationInstance;
        private IAppearanceFrameStategy m_NextFrameStrategy;

        private readonly int m_PhaseCount;
        private int m_CurrentPhaseDuration = 0;
        private int m_CurrentPhase = 0;
        private bool m_HasFinishedAnimation = false;

        public int Phase {
            get => m_CurrentPhase;
            set {
                if (m_AnimationInstance.Async) {
                    if (value == PhaseAsynchronous) {
                        m_CurrentPhase = 0;
                    } else if (value == PhaseRandom) {
                        m_CurrentPhase = (int)(Random.Range(0, 10) / 10f * m_PhaseCount);
                    } else if (value >= 0 && value < m_PhaseCount) {
                        m_CurrentPhase = value;
                    } else {
                        m_CurrentPhase = m_AnimationInstance.StartPhase;
                    }
                } else {
                    CalculateSynchronousPhase();
                }
            }
        }

        public bool Finished {
            get => m_HasFinishedAnimation;
            set => m_HasFinishedAnimation = value;
        }

        public AppearanceAnimator(Proto.Appearances001.FrameAnimation animation, int phaseCount) {
            m_AnimationInstance = animation;
            m_PhaseCount = phaseCount;
            LastAnimationTick = OpenTibiaUnity.TicksMillis;

            Phase = PhaseAutomatic;

            if (m_AnimationInstance.LoopCount < 0) {
                m_NextFrameStrategy = new PingPongFrameStrategy();
            } else {
                m_NextFrameStrategy = new LoopFrameStrategy((uint)m_AnimationInstance.LoopCount);
            }
        }

        public void Animate(int animationTick, int delay = 0) {
            if (animationTick != LastAnimationTick && !m_HasFinishedAnimation) {
                int ticks = animationTick - LastAnimationTick;
                if (ticks >= m_CurrentPhaseDuration) {
                    var nextPhase = m_NextFrameStrategy.NextFrame(m_CurrentPhase, m_PhaseCount);
                    if (m_CurrentPhase != nextPhase) {
                        int duration = delay == 0
                            ? m_AnimationInstance.FrameGroupDurations[nextPhase].Duration() - (ticks - m_CurrentPhaseDuration)
                            : CalculateMovementPhaseDuration(delay);

                        if (duration < 0 && !m_AnimationInstance.Async) {
                            CalculateSynchronousPhase();
                        } else {
                            m_CurrentPhase = nextPhase;
                            m_CurrentPhaseDuration = Mathf.Max(0, duration);
                        }
                    } else {
                        m_HasFinishedAnimation = true;
                    }
                } else {
                    m_CurrentPhaseDuration -= ticks;
                }

                LastAnimationTick = animationTick;
            }
        }

        public void SetEndless() {
            m_NextFrameStrategy = new LoopFrameStrategy(0);
        }

        public void Reset() {
            Phase = PhaseAutomatic;
            m_HasFinishedAnimation = false;
            m_NextFrameStrategy.Reset();
        }

        private void CalculateSynchronousPhase() {
            int totalDurations = 0;
            for (int i = 0; i < m_PhaseCount; i++) {
                totalDurations += m_AnimationInstance.FrameGroupDurations[i].Duration();
            }

            int ticks = OpenTibiaUnity.TicksMillis;
            int loc4 = ticks % totalDurations;

            int tmpDurations = 0;
            for (int i = 0; i < m_PhaseCount; i++) {
                int duration = m_AnimationInstance.FrameGroupDurations[i].Duration();
                if (loc4 >= duration && loc4 < duration + tmpDurations) {
                    m_CurrentPhase = i;
                    int loc8 = loc4 - tmpDurations;
                    m_CurrentPhaseDuration = duration - loc8;
                    break;
                }

                tmpDurations += duration;
            }

            LastAnimationTick = ticks;
        }

        private int CalculateMovementPhaseDuration(int delay) {
            delay = Mathf.Clamp(delay, MinSpeedDelay, MaxSpeedDelay);
            int loc5 = (delay - MaxSpeedDelay) / (MinSpeedDelay - MaxSpeedDelay);
            int loc6 = MinSpeedFrameDuration / m_PhaseCount;
            int loc7 = MaxSpeedFrameDuration / m_PhaseCount;

            return (loc6 - loc7) * loc5 + loc7;
        }
        
        public AppearanceAnimator Clone() {
            return new AppearanceAnimator(m_AnimationInstance, m_PhaseCount); 
        }

        public static bool operator !(AppearanceAnimator instance) {
            return instance == null;
        }

        public static bool operator true(AppearanceAnimator instance) {
            return !!instance;
        }

        public static bool operator false(AppearanceAnimator instance) {
            return !instance;
        }
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
        private uint m_LoopCount;
        private uint m_CurrentLoop = 0;

        public LoopFrameStrategy(uint loopCount) {
            m_LoopCount = loopCount;
        }

        public int NextFrame(int phase, int phaseCount) {
            int tmpPhase = phase + 1;
            if (tmpPhase < phaseCount) {
                return tmpPhase;
            }

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
