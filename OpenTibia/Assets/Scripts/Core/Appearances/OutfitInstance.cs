using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class OutfitInstance : AppearanceInstance
    {
        public const int InvisibleOutfitID = 0;

        protected int m_HeadHsiColor = 0;
        protected int m_TorsoHsiColor = 0;
        protected int m_LegsHsiColor = 0;
        protected int m_DetailHsiColor = 0;
        protected int m_Addons = 0;

        private Color m_HeadColor = Color.black;
        private Color m_TorsoColor = Color.black;
        private Color m_LegsColor = Color.black;
        private Color m_DetailColor = Color.black;

        private int m_Phase = 0;

        public OutfitInstance(uint id, AppearanceType type, int head, int body, int legs, int feet, int addons) : base(id, type) {
            UpdateProperties(head, body, legs, feet, addons);
        }

        public void UpdateProperties(int head, int torso, int legs, int detail, int addons) {
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

            if (m_Addons != addons)
                m_Addons = addons;
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            var fg = m_Type.FrameGroups[m_ActiveFrameGroup];
            return (int)((((
                (layer >= 0 ? layer : m_Phase) % fg.Phases
                * fg.PatternDepth + (patternZ >= 0 ? (int)(patternZ % fg.PatternDepth) : 0))
                * fg.PatternHeight + (patternY >= 0 ? (int)(patternY % fg.PatternHeight) : 0))
                * fg.PatternWidth + (patternX >= 0 ? (int)(patternX % fg.PatternWidth) : 0))
                * fg.Layers);
        }
        
        public override void DrawTo(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ) {
            var fg = m_Type.FrameGroups[m_ActiveFrameGroup];
            if (fg.Layers != 2) {
                base.DrawTo(screenPosition, zoom, patternX, patternY, patternZ);
                return;
            }
            
            var colouriseMaterial = OpenTibiaUnity.GameManager.OutfitsMaterial;
            colouriseMaterial.SetColor("_HeadColor", m_HeadColor);
            colouriseMaterial.SetColor("_TorsoColor", m_TorsoColor);
            colouriseMaterial.SetColor("_LegsColor", m_LegsColor);
            colouriseMaterial.SetColor("_DetailColor", m_DetailColor);
            
            for (patternY = 0; patternY < fg.PatternHeight; patternY++) {
                if (patternY > 0 && (m_Addons & 1 << (patternY - 1)) == 0)
                    break;
                
                int spriteIndex = GetSpriteIndex(-1, patternX, patternY, patternZ);
                uint spriteID = fg.Sprites[spriteIndex];
                var baseCachedSprite = OpenTibiaUnity.GameManager.AppearanceStorage.GetSprite(fg.Sprites[spriteIndex++]);
                var channelsCachedSprites = OpenTibiaUnity.GameManager.AppearanceStorage.GetSprite(fg.Sprites[spriteIndex]);

                colouriseMaterial.SetTexture("_ChannelsTex", channelsCachedSprites.texture);
                if (baseCachedSprite.texture == channelsCachedSprites.texture) {
                    // offset from the base rect
                    colouriseMaterial.SetTextureOffset("_ChannelsTex", channelsCachedSprites.rect.position - baseCachedSprite.rect.position);
                } else {
                    // offset from (0, 0)
                    // TODO, is this really correct? imo, this should be the same offset even if the texture is different.
                    colouriseMaterial.SetTextureOffset("_ChannelsTex", channelsCachedSprites.rect.position);
                }

                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, baseCachedSprite);
                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, baseCachedSprite, colouriseMaterial);
            }
        }

        public override void SwitchFrameGroup(int ticks, int fgType) {
            fgType = Mathf.Min(fgType, m_Type.FrameGroups.Count - 1); // i.e setting walk to animate-always group!
            if (fgType != m_ActiveFrameGroup) {
                m_ActiveFrameGroup = fgType;
                AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
                if (!!animator
                    && (animator.LastAnimationTick + AppearanceAnimator.AnimationDelayBeforeReset < ticks
                    || fgType != (int)Proto.Appearances001.FrameGroupType.Walking)) {
                    animator.Reset();
                }
            }
        }
        
        public override bool Animate(int ticks, int delay = 0) {
            AppearanceAnimator animator = m_Animators[m_ActiveFrameGroup];
            if (animator) {
                animator.Animate(ticks, delay);
                m_Phase = animator.Phase;
                return !animator.Finished;
            }
            return false;
        }
    }
}
