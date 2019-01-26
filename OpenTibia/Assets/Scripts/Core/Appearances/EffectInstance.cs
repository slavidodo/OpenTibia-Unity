namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class EffectInstance : AppearanceInstance
    {
        public EffectInstance(uint id, AppearanceType type) : base(id, type) {
            Phase = AppearanceAnimator.PhaseAsynchronous;
        }

        public void SetEndless() {
            AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
            if (animator) {
                animator.SetEndless();
            }
        }

        public void End() {
            AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
            if (animator) {
                animator.Finished = true;
            }
        }
    }
}
