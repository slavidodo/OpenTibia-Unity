using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Animation
{
    internal class LegacyAnimator : IAppearanceAnimator
    {

        private IAppearanceFrameStategy m_NextFrameStrategy;
        private LegacyOutfitFrameStrategy m_OutfitFrameStrategy;

        private readonly int m_PhaseCount;
        private readonly int m_WalkPhaseCount;
        private int m_CurrentPhaseDuration = 0;
        private int m_CurrentPhase = 0;

        private bool m_IsCreature  = false;

        public int LastAnimationTick { get; private set; } = 0;
        public bool Finished { get; set; } = false;
        internal int PhaseDuration { get; set; }
        internal bool Async { get; set; } = true;

        public int Phase {
            get => m_CurrentPhase;
            set {
                if (Async) {
                    if (value == Constants.PhaseAsynchronous)
                        m_CurrentPhase = 0;
                    else if (value == Constants.PhaseRandom)
                        m_CurrentPhase = (int)(Random.Range(0, 10) / 10f * m_PhaseCount);
                    else if (value >= 0 && value < m_PhaseCount)
                        m_CurrentPhase = value;
                    else
                        m_CurrentPhase = 0;
                } else {
                    CalculateSynchronousPhase();
                }
            }
        }
        
        internal AppearanceType AppearanceType { get; set; }

        internal LegacyAnimator(int phaseCount, int phaseDuration = 0) {
            m_PhaseCount = phaseCount;
            m_WalkPhaseCount = m_PhaseCount > 1 ? m_PhaseCount - 1 : 0;

            LastAnimationTick = OpenTibiaUnity.TicksMillis;

            if (phaseDuration != 0)
                PhaseDuration = phaseDuration;
            else if (m_PhaseCount != 0)
                PhaseDuration = 1000 / m_PhaseCount;
            else
                PhaseDuration = 40;
        }

        internal void Initialise(AppearanceType type) {
            if (type.IsAnimateAlways || type.Category == AppearanceCategory.Object) {
                m_NextFrameStrategy = new LoopFrameStrategy(0);
            } else if (type.Category == AppearanceCategory.Outfit) {
                m_IsCreature = true;
                m_OutfitFrameStrategy = new LegacyOutfitFrameStrategy();
                m_NextFrameStrategy = m_OutfitFrameStrategy;
            } else {
                m_NextFrameStrategy = new LoopFrameStrategy(1);
            }
        }

        public void Animate(int ticks, int delay = 0) {
            if (ticks != LastAnimationTick && !Finished) {
                int elapsedTicks = ticks - LastAnimationTick;
                if (elapsedTicks >= m_CurrentPhaseDuration) {
                    if (m_IsCreature)
                        m_OutfitFrameStrategy.UpdateState(delay != 0);

                    var nextPhase = m_NextFrameStrategy.NextFrame(m_CurrentPhase, m_PhaseCount);
                    if (m_CurrentPhase != nextPhase) {
                        int duration = delay == 0
                            ? PhaseDuration - (elapsedTicks - m_CurrentPhaseDuration)
                            : CalculateMovementPhaseDuration(delay);

                        if (duration < 0 && !Async) {
                            CalculateSynchronousPhase();
                        } else {
                            m_CurrentPhase = nextPhase;
                            m_CurrentPhaseDuration = Mathf.Max(0, duration);
                        }
                    } else {
                        Finished = true;
                    }

                    LastAnimationTick = ticks;
                }
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
            int totalDurations = PhaseDuration * m_PhaseCount;
            int ticks = OpenTibiaUnity.TicksMillis;
            int loc4 = ticks % totalDurations;

            int tmpDurations = 0;
            for (int i = 0; i < m_PhaseCount; i++) {
                if (loc4 >= PhaseDuration && loc4 < PhaseDuration + tmpDurations) {
                    m_CurrentPhase = i;
                    int loc8 = loc4 - tmpDurations;
                    m_CurrentPhaseDuration = PhaseDuration - loc8;
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
            return new LegacyAnimator(m_PhaseCount);
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

    internal class LegacyOutfitFrameStrategy : IAppearanceFrameStategy
    {
        private bool m_Walking = false;

        internal void UpdateState(bool walking) {
            m_Walking = walking;
        }

        public int NextFrame(int phase, int phaseCount) {
            if (!m_Walking)
                return 0;


            int tmpPhase = phase + 1;
            if (tmpPhase < phaseCount)
                return tmpPhase;

            return 0;
        }

        public void Reset() {
            m_Walking = false;
        }
    }
}
