using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Animation
{
    internal class EnhancedAnimator : IAppearanceAnimator
    {
        // constants for the new movement animation formula
        internal const int MinSpeedFrameDuration = 90 * 8;
        internal const int MaxSpeedFrameDuration = 35 * 8;

        internal const int MinSpeedDelay = 500;
        internal const int MaxSpeedDelay = 100;

        private Protobuf.Appearances.SpriteAnimation m_AnimationInstance;
        private IAppearanceFrameStategy m_NextFrameStrategy;

        private readonly int m_PhaseCount;
        private int m_CurrentPhaseDuration = 0;
        private int m_CurrentPhase = 0;

        public int LastAnimationTick { get; private set; } = 0;
        public bool Finished { get; set; } = false;

        public int Phase {
            get => m_CurrentPhase;
            set {
                if (m_AnimationInstance == null) {
                    return;
                } else if (m_AnimationInstance.Synchornized) {
                    CalculateSynchronousPhase();
                } else {
                    if (value == Constants.PhaseAsynchronous)
                        m_CurrentPhase = 0;
                    else if (value == Constants.PhaseRandom)
                        m_CurrentPhase = (int)(Random.Range(0, 10) / 10f * m_PhaseCount);
                    else if (value >= 0 && value < m_PhaseCount)
                        m_CurrentPhase = value;
                    else
                        m_CurrentPhase = (int)m_AnimationInstance.DefaultStartPhase;
                }
            }
        }

        internal EnhancedAnimator(Protobuf.Appearances.SpriteAnimation animation, int phaseCount) {
            m_AnimationInstance = animation;
            m_PhaseCount = phaseCount;
            LastAnimationTick = OpenTibiaUnity.TicksMillis;

            Phase = Constants.PhaseAutomatic;

            if (m_AnimationInstance?.LoopCount < 0)
                m_NextFrameStrategy = new PingPongFrameStrategy();
            else
                m_NextFrameStrategy = new LoopFrameStrategy((uint)m_AnimationInstance?.LoopCount);
        }

        public void Animate(int ticks, int delay = 0) {
            // if delay != 0, then this is movement animation delay
            if (ticks != LastAnimationTick && !Finished) {
                int elapsedTicks = ticks - LastAnimationTick;
                if (elapsedTicks >= m_CurrentPhaseDuration) {
                    var nextPhase = m_NextFrameStrategy.NextFrame(m_CurrentPhase, m_PhaseCount);
                    if (m_CurrentPhase != nextPhase) {
                        int duration = delay == 0
                            ? m_AnimationInstance.SpritePhases[nextPhase].Duration() - (elapsedTicks - m_CurrentPhaseDuration)
                            : CalculateMovementPhaseDuration(delay);

                        if (duration < 0 && m_AnimationInstance != null && m_AnimationInstance.Synchornized) {
                            CalculateSynchronousPhase();
                        } else {
                            m_CurrentPhase = nextPhase;
                            m_CurrentPhaseDuration = Mathf.Max(0, duration);
                        }
                    } else {
                        Finished = true;
                    }
                } else {
                    m_CurrentPhaseDuration -= elapsedTicks;
                }

                LastAnimationTick = ticks;
            }
        }

        public void SetEndless() {
            m_NextFrameStrategy = new LoopFrameStrategy(0);
        }

        public void Reset() {
            Phase = Constants.PhaseAutomatic;
            Finished = false;
            m_NextFrameStrategy.Reset();
        }

        private void CalculateSynchronousPhase() {
            int totalDurations = 0;
            for (int i = 0; i < m_PhaseCount; i++)
                totalDurations += m_AnimationInstance.SpritePhases[i].Duration();

            int ticks = OpenTibiaUnity.TicksMillis;
            int loc4 = ticks % totalDurations;

            int tmpDurations = 0;
            for (int i = 0; i < m_PhaseCount; i++) {
                int duration = m_AnimationInstance.SpritePhases[i].Duration();
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
            delay = Mathf.Clamp(delay, MaxSpeedDelay, MinSpeedDelay);
            int loc5 = (delay - MaxSpeedDelay) / (MinSpeedDelay - MaxSpeedDelay);
            int loc6 = MinSpeedFrameDuration / m_PhaseCount;
            int loc7 = MaxSpeedFrameDuration / m_PhaseCount;

            return (loc6 - loc7) * loc5 + loc7;
        }

        public IAppearanceAnimator Clone() {
            return new EnhancedAnimator(m_AnimationInstance, m_PhaseCount); 
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
