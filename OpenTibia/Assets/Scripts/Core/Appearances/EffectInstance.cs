namespace OpenTibiaUnity.Core.Appearances
{
    internal sealed class EffectInstance : AppearanceInstance
    {
        internal EffectInstance(uint id, AppearanceType type) : base(id, type) {
            Phase = Constants.PhaseAsynchronous;
            foreach (var animator in m_Animators) {
                if (animator is Animation.LegacyAnimator legacyAnimator)
                    legacyAnimator.PhaseDuration = 75;
            }
        }

        internal void SetEndless() {
            Animation.IAppearanceAnimator animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null)
                animator.SetEndless();
        }

        internal void End() {
            Animation.IAppearanceAnimator animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null)
                animator.Finished = true;
        }
    }
}
