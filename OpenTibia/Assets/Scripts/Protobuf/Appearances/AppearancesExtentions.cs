using UnityEngine;

namespace OpenTibiaUnity.Protobuf.Appearances
{
    public sealed partial class SpriteInfo
    {
        Core.Appearances.Animation.IAppearanceAnimator _animator = null;
        public Core.Appearances.Animation.IAppearanceAnimator Animator {
            get {
                if (!IsAnimation) {
                    return null;
                } else if (_animator == null) {
                    if (Animation != null)
                        _animator = new Core.Appearances.Animation.EnhancedAnimator(Animation, Animation.SpritePhases.Count);
                    else
                        _animator = new Core.Appearances.Animation.LegacyAnimator((int)Phases);
                }

                return _animator;
            }
        }

        public bool IsAnimation {
            get => Phases > 1;
        }

        public int CalculateSpriteIndex(int phase, int patternX, int patternY, int patternZ) {
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
        public int Duration() {
            if (DurationMin == DurationMax) {
                return (int)DurationMin;
            }

            return Random.Range((int)DurationMin, (int)DurationMax);
        }
    }
}
