using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    internal class OutfitInstance : AppearanceInstance
    {
        internal const int InvisibleOutfitID = 0;

        protected int m_HeadHsiColor = 0;
        protected int m_TorsoHsiColor = 0;
        protected int m_LegsHsiColor = 0;
        protected int m_DetailHsiColor = 0;
        protected int m_AddOns = 0;

        private Color m_HeadColor = Color.black;
        private Color m_TorsoColor = Color.black;
        private Color m_LegsColor = Color.black;
        private Color m_DetailColor = Color.black;

        private int m_Phase = 0;
        private bool m_Walking = false;

        internal int Head { get => m_HeadHsiColor; }
        internal int Torso { get => m_TorsoHsiColor; }
        internal int Legs { get => m_LegsHsiColor; }
        internal int Detail { get => m_DetailHsiColor; }
        internal int AddOns { get => m_AddOns; }

        internal OutfitInstance(uint id, AppearanceType type, int head, int torso, int legs, int feet, int addons) : base(id, type) {
            UpdateProperties(head, torso, legs, feet, addons);
        }

        internal void UpdateProperties(int head, int torso, int legs, int detail, int addons) {
            if (m_HeadHsiColor != head) {
                m_HeadHsiColor = head;
                m_HeadColor = Colors.ColorFromHSI(head);
            }

            if (m_TorsoHsiColor != torso) {
                m_TorsoHsiColor = torso;
                m_TorsoColor = Colors.ColorFromHSI(torso);
            }

            if (m_LegsHsiColor != legs) {
                m_LegsHsiColor = legs;
                m_LegsColor = Colors.ColorFromHSI(legs);
            }

            if (m_DetailHsiColor != detail) {
                m_DetailHsiColor = detail;
                m_DetailColor = Colors.ColorFromHSI(detail);
            }

            if (m_AddOns != addons)
                m_AddOns = addons;
        }

        internal override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            return (int)((((
                (layer >= 0 ? layer : m_Phase) % m_ActiveFrameGroup.SpriteInfo.Phases
                * m_ActiveFrameGroup.SpriteInfo.PatternDepth + (patternZ >= 0 ? (int)(patternZ % m_ActiveFrameGroup.SpriteInfo.PatternDepth) : 0))
                * m_ActiveFrameGroup.SpriteInfo.PatternHeight + (patternY >= 0 ? (int)(patternY % m_ActiveFrameGroup.SpriteInfo.PatternHeight) : 0))
                * m_ActiveFrameGroup.SpriteInfo.PatternWidth + (patternX >= 0 ? (int)(patternX % m_ActiveFrameGroup.SpriteInfo.PatternWidth) : 0))
                * m_ActiveFrameGroup.SpriteInfo.Layers);
        }
        
        internal override void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            if (m_ActiveFrameGroup.SpriteInfo.Layers != 2) {
                var cachedInformation = GetSprite(-1, patternX, patternY, patternZ, m_ActiveFrameGroup.SpriteInfo.IsAnimation);
                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, cachedInformation);
                return;
            }
            
            var colouriseMaterial = OpenTibiaUnity.GameManager.OutfitTypeMaterial;
            colouriseMaterial.SetColor("_HeadColor", m_HeadColor);
            colouriseMaterial.SetColor("_TorsoColor", m_TorsoColor);
            colouriseMaterial.SetColor("_LegsColor", m_LegsColor);
            colouriseMaterial.SetColor("_DetailColor", m_DetailColor);
            
            for (patternY = 0; patternY < m_ActiveFrameGroup.SpriteInfo.PatternHeight; patternY++) {
                if (patternY > 0 && (m_AddOns & 1 << (patternY - 1)) == 0)
                    continue;
                
                int spriteIndex = GetSpriteIndex(-1, patternX, patternY, patternZ);
                uint spriteID = m_ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                var baseCachedSprite = OpenTibiaUnity.GameManager.AppearanceStorage.GetSprite(m_ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex++]);
                var channelsCachedSprites = OpenTibiaUnity.GameManager.AppearanceStorage.GetSprite(m_ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex]);

                colouriseMaterial.SetTexture("_ChannelsTex", channelsCachedSprites.texture);
                colouriseMaterial.SetTextureOffset("_ChannelsTex", channelsCachedSprites.rect.position - baseCachedSprite.rect.position);

                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, baseCachedSprite);
                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, baseCachedSprite, colouriseMaterial);
            }
        }

        internal override void SwitchFrameGroup(int ticks, int frameGroupIndex) {
            bool updateAnimator = false, forceUpdate = false;
            bool walking = m_Walking;

            m_Walking = frameGroupIndex == (int)Protobuf.Shared.FrameGroupType.Walking;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSeparateAnimationGroups)) {
                frameGroupIndex = Mathf.Min(frameGroupIndex, m_Type.FrameGroups.Count - 1);
                if (frameGroupIndex != m_ActiveFrameGroupIndex) {
                    m_ActiveFrameGroupIndex = frameGroupIndex;
                    updateAnimator = true;
                    forceUpdate = !m_Walking;
                }
            } else if (m_Walking != walking) {
                updateAnimator = forceUpdate = true;
            }

            if (updateAnimator) {
                var animator = m_Animators?[m_ActiveFrameGroupIndex];
                if (animator != null && (animator.LastAnimationTick + Constants.AnimationDelayBeforeReset < ticks || forceUpdate))
                    animator.Reset();
            }
        }

        internal override bool Animate(int ticks, int delay = 0) {
            Animation.IAppearanceAnimator animator = m_Animators?[m_ActiveFrameGroupIndex];
            if (animator != null) {
                animator.Animate(ticks, delay);
                m_Phase = animator.Phase;
                return !animator.Finished;
            }
            return false;
        }
    }
}
