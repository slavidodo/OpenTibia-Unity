using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public abstract class AppearanceInstance {
        public const int UnknownCreature = 97;
        public const int OutdatedCreature = 98;
        public const int Creature = 99;

        public const int HookEast = 19;
        public const int HookSouth = 20;

        public static Vector2 s_TempPoint = Vector2.zero;
        public static Rect s_TempRect = Rect.zero;
        public static Vector2 s_FieldVector = new Vector2(Constants.FieldSize, Constants.FieldSize);

        protected int m_LastPhase = -1;
        protected int m_LastpublicPhase = -1;
        protected bool m_CacheDirty = false;
        protected int m_LastCachedSpriteIndex = -1;
        protected int m_LastInternalPhase = -1;
        public int MapData = -1; // stack pos
        protected int m_ActiveFrameGroup = 0;
        protected AppearanceType m_Type;
        protected uint m_ID;
        protected int m_LastPatternX = -1;
        protected int m_LastPatternY = -1;
        protected int m_LastPatternZ = -1;
        protected AppearanceAnimator[] m_Animators = new AppearanceAnimator[2];
        public int MapField = -1;
        protected List<uint> m_TempAlternativePhases;

        protected CachedSpriteInformation m_CachedSpriteInformation = null;

        public uint ID {
            get => m_ID;
        }
        public AppearanceType Type {
            get => m_Type;
        }

        public int Phase {
            set {
                AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
                if (animator) {
                    animator.Phase = value;
                }
            }
            
            get {
                AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
                if (animator) {
                    return animator.Phase;
                }
                return 0;
            }
        }

        public AppearanceInstance(uint id, AppearanceType type) {
            m_ID = id;
            m_Type = type;

            if (!!m_Type && m_Type.FrameGroups != null) {
                if (m_Type.FrameGroups[m_ActiveFrameGroup].IsAnimation) {
                    m_TempAlternativePhases = new List<uint>((int)m_Type.FrameGroups[m_ActiveFrameGroup].Phases);
                }

                int i = 0;
                foreach (var fg in type.FrameGroups) {
                    if (fg.Animator)
                        m_Animators[i] = fg.Animator.Clone();
                    i++;
                }
            }
        }

        public virtual int GetSpriteIndex(int _, int patternX, int patternY, int patternZ) {
            int phase = Phase;
            if (!(phase == m_LastInternalPhase && patternX == m_LastPatternX && patternX >= 0 && patternY == m_LastPatternY && patternY >= 0 && patternZ == m_LastPatternZ && patternZ >= 0)) {
                m_LastInternalPhase = phase;
                m_LastPatternX = patternX;
                m_LastPatternY = patternY;
                m_LastPatternZ = patternZ;

                var fg = m_Type.FrameGroups[m_ActiveFrameGroup];

                int z = patternZ >= 0 ? (int)(patternZ % fg.PatternDepth) : 0;
                int y = patternY >= 0 ? (int)(patternY % fg.PatternHeight) : 0;
                int x = patternX >= 0 ? (int)(patternX % fg.PatternWidth) : 0;
                
                m_LastCachedSpriteIndex = 
                        (int)(((phase * fg.PatternDepth + z)
                                      * fg.PatternHeight + y)
                                      * fg.PatternWidth + x);
            }
            return m_LastCachedSpriteIndex;
        }

        public CachedSpriteInformation GetSprite(int layer, int patternX, int patternY, int patternZ, bool animation) {
            if (!Type.IsAnimation) {
                if (m_CachedSpriteInformation == null) {
                    m_CachedSpriteInformation = OpenTibiaUnity.AppearanceStorage.GetSprite(
                        m_Type.FrameGroups[m_ActiveFrameGroup].Sprites[GetSpriteIndex(layer, patternX, patternY, patternZ)]);
                }

                return m_CachedSpriteInformation;
            }
            
            // TODO(priority=med)
            // store all sprites of this item in a mapped list
            // OR - use indepedant spriteProvider for each appearance type

            int spriteIndex = GetSpriteIndex(layer, patternX, patternY, patternZ);
            uint spriteID = m_Type.FrameGroups[m_ActiveFrameGroup].Sprites[spriteIndex];
            return OpenTibiaUnity.AppearanceStorage.GetSprite(spriteID);
        }
        
        public virtual void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ) {
            var cachedInformation = GetSprite(-1, patternX, patternY, patternZ, m_Type.FrameGroups[m_ActiveFrameGroup].IsAnimation);
            InternalDrawTo(screenPosition.x, screenPosition.y, zoom, cachedInformation);
        }
        
        protected void InternalDrawTo(float screenX, float screenY, Vector2 zoom,
            CachedSpriteInformation cachedSpriteInfo, Material material = null) {
            
            s_TempPoint.Set(screenX - m_Type.DisplacementX, screenY - m_Type.DisplacementY);
            s_TempRect.position = (s_TempPoint - cachedSpriteInfo.spriteSize + s_FieldVector) * zoom;
            s_TempRect.size = cachedSpriteInfo.spriteSize * zoom;
            
            if (material != null) {
                Graphics.DrawTexture(s_TempRect, cachedSpriteInfo.texture, cachedSpriteInfo.rect, 0, 0, 0, 0, material);
            } else {
                Graphics.DrawTexture(s_TempRect, cachedSpriteInfo.texture, cachedSpriteInfo.rect, 0, 0, 0, 0, OpenTibiaUnity.GameManager.DefaultMaterial);
            }
        }

        public virtual void SwitchFrameGroup(int _, int __) { }

        public virtual bool Animate(int ticks, int delay = 0) {
            AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
            if (!!animator) {
                animator.Animate(ticks, delay);
                return !animator.Finished;
            }
            return false;
        }

        public static bool operator !(AppearanceInstance instance) {
            return instance == null;
        }

        public static bool operator true(AppearanceInstance instance) {
            return !!instance;
        }

        public static bool operator false(AppearanceInstance instance) {
            return !instance;
        }
    }
}
