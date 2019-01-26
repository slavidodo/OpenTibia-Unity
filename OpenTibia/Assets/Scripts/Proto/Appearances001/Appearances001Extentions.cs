using UnityEngine;

namespace OpenTibiaUnity.Proto.Appearances001
{
    public sealed partial class FrameGroup
    {
        Core.Appearances.AppearanceAnimator m_Animator = null;

        public Core.Appearances.AppearanceAnimator Animator {
            get {
                if (IsAnimation) {
                    if (m_Animator == null) {
                        m_Animator = new Core.Appearances.AppearanceAnimator(FrameAnimation, (int)Phases);
                    }

                    return m_Animator;
                }

                return null;
            }
        }

        public bool IsAnimation {
            get {
                return Phases > 1;
            }
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
