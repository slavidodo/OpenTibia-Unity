using System.Collections.Generic;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Appearances
{
    public class OutfitInstance : AppearanceInstance
    {
        public const int InvisibleOutfitId = 0;

        protected int _headHsiColor = 0;
        protected int _torsoHsiColor = 0;
        protected int _legsHsiColor = 0;
        protected int _detailHsiColor = 0;
        protected int _addOns = 0;

        private Color _headColor = Color.white;
        private Color _torsoColor = Color.white;
        private Color _legsColor = Color.white;
        private Color _detailColor = Color.white;

        private Dictionary<uint, MaterialPropertyBlock> _channelProps = new Dictionary<uint, MaterialPropertyBlock>();

        private int _phase = 0;
        private bool _walking = false;

        public int Head { get => _headHsiColor; }
        public int Torso { get => _torsoHsiColor; }
        public int Legs { get => _legsHsiColor; }
        public int Detail { get => _detailHsiColor; }
        public int AddOns { get => _addOns; }

        public OutfitInstance(uint id, AppearanceType type, int head, int torso, int legs, int detail, int addons) : base(id, type) {
            UpdateProperties(head, torso, legs, detail, addons);
        }

        public void UpdateProperties(int head, int torso, int legs, int detail, int addons) {
            if (_headHsiColor != head) {
                _headHsiColor = head;
                _headColor = Colors.ColorFromHSI(head);
            }

            if (_torsoHsiColor != torso) {
                _torsoHsiColor = torso;
                _torsoColor = Colors.ColorFromHSI(torso);
            }

            if (_legsHsiColor != legs) {
                _legsHsiColor = legs;
                _legsColor = Colors.ColorFromHSI(legs);
            }

            if (_detailHsiColor != detail) {
                _detailHsiColor = detail;
                _detailColor = Colors.ColorFromHSI(detail);
            }

            if (_addOns != addons)
                _addOns = addons;

            foreach (var props in _channelProps) {
                UpdateMaterialProppertyBlock(props.Value);
            }
        }

        private void UpdateMaterialProppertyBlock(MaterialPropertyBlock props) {
            props.SetColor("_HeadColor", _headColor);
            props.SetColor("_TorsoColor", _torsoColor);
            props.SetColor("_LegsColor", _legsColor);
            props.SetColor("_DetailColor", _detailColor);
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            return (int)((((
                (layer >= 0 ? layer : (_phase > 0 ? _phase : 0)) % ActiveFrameGroup.SpriteInfo.Phases
                * ActiveFrameGroup.SpriteInfo.PatternDepth + (patternZ >= 0 ? (int)(patternZ % ActiveFrameGroup.SpriteInfo.PatternDepth) : 0))
                * ActiveFrameGroup.SpriteInfo.PatternHeight + (patternY >= 0 ? (int)(patternY % ActiveFrameGroup.SpriteInfo.PatternHeight) : 0))
                * ActiveFrameGroup.SpriteInfo.PatternWidth + (patternX >= 0 ? (int)(patternX % ActiveFrameGroup.SpriteInfo.PatternWidth) : 0))
                * ActiveFrameGroup.SpriteInfo.Layers);
        }

        public override void Draw(CommandBuffer commandBuffer, Vector2Int screenPosition, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            if (ActiveFrameGroup.SpriteInfo.Layers != 2) {
                var cachedSprite = GetSprite(-1, patternX, patternY, patternZ, ActiveFrameGroup.SpriteInfo.IsAnimation);
                if (cachedSprite != null)
                    InternalDraw(commandBuffer, screenPosition, highlighted, highlightOpacity, cachedSprite);
                return;
            }

            var colouriseMaterial = OpenTibiaUnity.GameManager.OutfitTypeMaterial;

            bool dontDraw = false;
            for (patternY = 0; patternY < ActiveFrameGroup.SpriteInfo.PatternHeight; patternY++) {
                if (patternY > 0 && (_addOns & 1 << (patternY - 1)) == 0)
                    continue;

                int spriteIndex = GetSpriteIndex(-1, patternX, patternY, patternZ);

                var baseSpriteId = ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                var channelSpriteId = ActiveFrameGroup.SpriteInfo.SpriteIDs[++spriteIndex];

                OpenTibiaUnity.AppearanceStorage.GetSprite(baseSpriteId, out CachedSprite baseSprite);
                OpenTibiaUnity.AppearanceStorage.GetSprite(channelSpriteId, out CachedSprite channelSprite);

                // if these are not loaded yet we should still continue to
                // ensure that next time all layers are loaded!
                if (baseSprite == null || channelSprite == null)
                    dontDraw = true;

                if (!dontDraw) {
                    if (!_channelProps.TryGetValue(channelSpriteId, out MaterialPropertyBlock props)) {
                        props = new MaterialPropertyBlock();
                        baseSprite.GenerateMaterialProps(props);
                        channelSprite.GenerateChannelsMaterialProps(props);
                        UpdateMaterialProppertyBlock(props);
                        _channelProps.Add(channelSpriteId, props);
                    }

                    InternalDraw(commandBuffer, screenPosition, highlighted, highlightOpacity, baseSprite);
                    InternalDraw(commandBuffer, screenPosition, highlighted, highlightOpacity, baseSprite, colouriseMaterial, props);
                }
            }
        }

        public override void SwitchFrameGroup(int ticks, int frameGroupIndex) {
            bool updateAnimator = false, forceUpdate = false;
            bool walking = _walking;

            _walking = frameGroupIndex == (int)Protobuf.Shared.FrameGroupType.Walking;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSeparateAnimationGroups)) {
                frameGroupIndex = Mathf.Min(frameGroupIndex, _type.FrameGroups.Count - 1);
                if (frameGroupIndex != _activeFrameGroupIndex) {
                    _activeFrameGroupIndex = frameGroupIndex;
                    updateAnimator = true;
                    forceUpdate = !_walking;
                }
            } else if (_walking != walking) {
                updateAnimator = forceUpdate = true;
            }

            if (updateAnimator) {
                var animator = _animators?[_activeFrameGroupIndex];
                if (animator != null && (animator.LastAnimationTick + Constants.AnimationDelayBeforeReset < ticks || forceUpdate))
                    animator.Reset();
            }
        }

        public override bool Animate(int ticks, int delay = 0) {
            Animation.IAppearanceAnimator animator = _animators?[_activeFrameGroupIndex];
            if (animator != null) {
                animator.Animate(ticks, delay);
                _phase = animator.Phase;
                return !animator.Finished;
            }
            return false;
        }

        public override AppearanceInstance Clone() {
            return new OutfitInstance(Id, Type, Head, Torso, Legs, Detail, AddOns);
        }
    }
}
