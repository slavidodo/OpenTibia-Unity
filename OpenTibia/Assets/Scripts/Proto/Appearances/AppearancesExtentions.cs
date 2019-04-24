using UnityEngine;

namespace OpenTibiaUnity.Proto.Appearances
{
    public sealed partial class FrameGroup
    {
        Core.Appearances.Animation.IAppearanceAnimator m_Animator = null;
        public Core.Appearances.Animation.IAppearanceAnimator Animator {
            get {
                if (!IsAnimation) {
                    return null;
                } else if (m_Animator == null) {
                    if (FrameAnimation != null)
                        m_Animator = new Core.Appearances.Animation.EnhancedAnimator(FrameAnimation, (int)Phases);
                    else
                        m_Animator = new Core.Appearances.Animation.LegacyAnimator((int)Phases);
                }

                return m_Animator;
            }
        }

        public bool IsAnimation {
            get => Phases > 1;
        }
    }

    public sealed partial class FrameGroupDuration
    {
        public int Duration() {
            if (Min == Max) {
                return (int)Min;
            }

            return Random.Range((int)Min, (int)Max);
        }
    }
}
