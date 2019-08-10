using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    internal abstract class AppearanceInstance {
        internal const int UnknownCreature = 97;
        internal const int OutdatedCreature = 98;
        internal const int Creature = 99;

        internal const int HookEast = 19;
        internal const int HookSouth = 20;

        internal static Vector2 s_TempPoint = Vector2.zero;
        internal static Rect s_TempRect = Rect.zero;
        internal static Vector2 s_FieldVector = new Vector2(Constants.FieldSize, Constants.FieldSize);

        internal int MapData = -1; // stack position
        internal int MapField = -1; // field index in cached map

        protected bool m_CacheDirty = false;
        protected uint m_ID;
        protected int m_LastPhase = -1;
        protected int m_LastinternalPhase = -1;
        protected int m_LastCachedSpriteIndex = -1;
        protected int m_LastInternalPhase = -1;
        protected int m_ActiveFrameGroupIndex = 0;
        protected int m_LastPatternX = -1;
        protected int m_LastPatternY = -1;
        protected int m_LastPatternZ = -1;


        protected AppearanceType m_Type;
        protected Animation.IAppearanceAnimator[] m_Animators;
        protected List<CachedSpriteInformation[]> m_CachedSpriteInformations;

        protected Protobuf.Appearances.FrameGroup m_ActiveFrameGroup {
            get {
                if (m_ActiveFrameGroupIndex < 0)
                    return null;

                if (m_ActiveFrameGroupIndex != m_ActiveFrameGroupIndexInternal) {
                    m_ActiveFrameGroupIndexInternal = m_ActiveFrameGroupIndex;
                    m_ActiveFrameGroupInternal = m_Type.FrameGroups?[m_ActiveFrameGroupIndex];
                }

                return m_ActiveFrameGroupInternal;
            }
        }

        private int m_ActiveFrameGroupIndexInternal = -1;
        private Protobuf.Appearances.FrameGroup m_ActiveFrameGroupInternal = null;

        protected CachedSpriteInformation m_CachedSpriteInformation = null;

        internal uint ID {
            get => m_ID;
        }
        internal AppearanceType Type {
            get => m_Type;
        }

        internal virtual int Phase {
            set {
                var animator = m_Animators?[m_ActiveFrameGroupIndex];
                if (animator != null)
                    animator.Phase = value;
            }
            
            get {
                var animator = m_Animators?[m_ActiveFrameGroupIndex];
                return animator != null ? animator.Phase : 0;
            }
        }

        internal AppearanceInstance(uint id, AppearanceType type) {
            m_ID = id;
            m_Type = type;

            if (!!m_Type && m_Type.FrameGroups != null) {
                m_Animators = new Animation.IAppearanceAnimator[m_Type.FrameGroups.Count];
                for (int i = 0; i < m_Animators.Length; i++) {
                    var animator = type.FrameGroups[i].SpriteInfo.Animator?.Clone();
                    if (animator is Animation.LegacyAnimator legacyAnimator)
                        legacyAnimator.Initialise(type);

                    m_Animators[i] = animator;
                }
            }
        }

        internal virtual int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int phase = Phase % (int)m_ActiveFrameGroup.SpriteInfo.Phases;
            if (!(phase == m_LastInternalPhase && patternX == m_LastPatternX && patternX >= 0 && patternY == m_LastPatternY && patternY >= 0 && patternZ == m_LastPatternZ && patternZ >= 0)) {
                m_LastInternalPhase = phase;
                m_LastPatternX = patternX;
                m_LastPatternY = patternY;
                m_LastPatternZ = patternZ;

                m_LastCachedSpriteIndex = m_ActiveFrameGroup.SpriteInfo.CalculateSpriteIndex(phase, patternX, patternY, patternZ);
            }

            return m_LastCachedSpriteIndex + (layer >= 0 ? layer % (int)m_ActiveFrameGroup.SpriteInfo.Layers : 0);
        }

        internal CachedSpriteInformation GetSprite(int layer, int patternX, int patternY, int patternZ, bool animation) {
            if (m_Type.FrameGroups == null)
                return null;

            var appearanceStorage = OpenTibiaUnity.AppearanceStorage;
            
            var spriteIndex = GetSpriteIndex(layer, patternX, patternY, patternZ);
            if (m_CachedSpriteInformations == null) {
                m_CachedSpriteInformations = new List<CachedSpriteInformation[]>();
                foreach (var frameGroup in m_Type.FrameGroups)
                    m_CachedSpriteInformations.Add(new CachedSpriteInformation[frameGroup.SpriteInfo.SpriteIDs.Count]);
                
                uint spriteId = m_ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                m_CachedSpriteInformations[m_ActiveFrameGroupIndex][spriteIndex] = appearanceStorage.GetSprite(spriteId);
            } else if (m_CachedSpriteInformations[m_ActiveFrameGroupIndex][spriteIndex] == null) {
                var spriteId = m_ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                m_CachedSpriteInformations[m_ActiveFrameGroupIndex][spriteIndex] = appearanceStorage.GetSprite(spriteId);
            }

            return m_CachedSpriteInformations[m_ActiveFrameGroupIndex][spriteIndex];
        }
        
        internal virtual void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            // this requires a bit of explanation
            // on outfits, layers are not useful, instead it relies on phases
            // while on the rest of categories, layers are treated as indepedant sprites..
            // the phase is used to determine the initial sprite index (at the animation property)
            // then the amount of layers based on that index is drawn
            // mostly you'd see this in objects like doors, or in effects like assassin outfit
            for (int layer = 0; layer < m_ActiveFrameGroup.SpriteInfo.Layers; layer++) {
                var cachedInformation = GetSprite(layer, patternX, patternY, patternZ, m_ActiveFrameGroup.SpriteInfo.IsAnimation);
                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, cachedInformation);
            }
        }
        
        protected void InternalDrawTo(float screenX, float screenY, Vector2 zoom, bool highlighted, float highlightOpacity,
            CachedSpriteInformation cachedSpriteInfo, Material material = null) {
            s_TempPoint.Set(screenX - m_Type.OffsetX, screenY - m_Type.OffsetY);
            s_TempRect.position = (s_TempPoint - cachedSpriteInfo.spriteSize + s_FieldVector) * zoom;
            s_TempRect.size = cachedSpriteInfo.spriteSize * zoom;

            if (material == null)
                material = OpenTibiaUnity.GameManager.AppearanceTypeMaterial;

            material.SetFloat("_HighlightOpacity", highlighted ? highlightOpacity : 0);
            Graphics.DrawTexture(s_TempRect, cachedSpriteInfo.texture, cachedSpriteInfo.rect, 0, 0, 0, 0, material);
        }

        internal virtual void SwitchFrameGroup(int _, int __) { }

        internal virtual bool Animate(int ticks, int delay = 0) {
            var animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null) {
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
