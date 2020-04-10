namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class EffectInstance : AppearanceInstance
    {
        public EffectInstance(uint id, AppearanceType type) : base(id, type) {
            Phase = Constants.PhaseAsynchronous;
            foreach (var animator in _animators) {
                if (animator is Animation.LegacyAnimator legacyAnimator)
                    legacyAnimator.PhaseDuration = 75;
            }
        }

        public void SetEndless() {
            Animation.IAppearanceAnimator animator = _animators?[_activeFrameGroupIndex];
            if (animator != null)
                animator.SetEndless();
        }

        public void End() {
            Animation.IAppearanceAnimator animator = _animators?[_activeFrameGroupIndex];
            if (animator != null)
                animator.Finished = true;
        }

        public override AppearanceInstance Clone() {
            return new EffectInstance(Id, Type);
        }
    }
}
