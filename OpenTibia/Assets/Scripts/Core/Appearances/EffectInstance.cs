namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class EffectInstance : AppearanceInstance
    {
        public EffectInstance(uint id, AppearanceType type) : base(id, type) {
            Phase = Constants.PhaseAsynchronous;
            foreach (var animator in m_Animators) {
                if (animator is Animation.LegacyAnimator legacyAnimator)
                    legacyAnimator.PhaseDuration = 75;
            }
        }

        public void SetEndless() {
            Animation.IAppearanceAnimator animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null)
                animator.SetEndless();
        }

        public void End() {
            Animation.IAppearanceAnimator animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null)
                animator.Finished = true;
        }
    }
}
