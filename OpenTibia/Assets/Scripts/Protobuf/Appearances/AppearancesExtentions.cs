using UnityEngine;

namespace OpenTibiaUnity.Protobuf.Appearances
{
    public sealed partial class SpriteInfo
    {
        Core.Appearances.Animation.IAppearanceAnimator m_Animator = null;
        internal Core.Appearances.Animation.IAppearanceAnimator Animator {
            get {
                if (!IsAnimation) {
                    return null;
                } else if (m_Animator == null) {
                    if (Animation != null)
                        m_Animator = new Core.Appearances.Animation.EnhancedAnimator(Animation, Animation.SpritePhases.Count);
                    else
                        m_Animator = new Core.Appearances.Animation.LegacyAnimator((int)Phases);
                }

                return m_Animator;
            }
        }

        internal bool IsAnimation {
            get => Phases > 1;
        }

        internal int CalculateSpriteIndex(int phase, int patternX, int patternY, int patternZ) {
            int z = patternZ >= 0 ? patternZ % (int)PatternDepth : 0;
            int y = patternY >= 0 ? patternY % (int)PatternHeight : 0;
            int x = patternX >= 0 ? patternX % (int)PatternWidth : 0;

            return (((phase * (int)PatternDepth + z)
                            * (int)PatternHeight + y)
                            * (int)PatternWidth + x) * (int)Layers;
        }
    }

    public sealed partial class SpritePhase
    {
        internal int Duration() {
            if (DurationMin == DurationMax) {
                return (int)DurationMin;
            }

            return Random.Range((int)DurationMin, (int)DurationMax);
        }
    }
}
