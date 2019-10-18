using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class CachedSpriteRequest
    {
        public CachedSpriteRequest(SpriteLoadingStatus status, CachedSprite cachedSprite) {
            Status = status;
            CachedSprite = cachedSprite;
        }

        public SpriteLoadingStatus Status;
        public CachedSprite CachedSprite;
    }

    public abstract class AppearanceInstance {
        public const int UnknownCreature = 97;
        public const int OutdatedCreature = 98;
        public const int Creature = 99;

        public const int HookEast = 19;
        public const int HookSouth = 20;

        public static Vector2 s_TempPoint = Vector2.zero;
        public static Rect s_TempRect = Rect.zero;
        public static Vector2 s_FieldVector = new Vector2(Constants.FieldSize, Constants.FieldSize);

        public int MapData = -1; // stack position
        public int MapField = -1; // field index in cached map

        protected bool _cachedirty = false;
        protected uint _id;
        protected int _lastCachedSpriteIndex = -1;
        protected int _lastInternalPhase = -1;
        protected int _activeFrameGroupIndex = 0;
        protected int _lastPatternX = -1;
        protected int _lastPatternY = -1;
        protected int _lastPatternZ = -1;

        protected AppearanceType _type;
        protected Animation.IAppearanceAnimator[] _animators;
        protected List<CachedSpriteRequest[]> _cachedSprites;

        public bool ClampeToFieldSize = false;

        protected Protobuf.Appearances.FrameGroup _activeFrameGroup {
            get {
                if (_activeFrameGroupIndex < 0)
                    return null;

                if (_activeFrameGroupIndex != _activeFrameGroupIndexInternal) {
                    _activeFrameGroupIndexInternal = _activeFrameGroupIndex;
                    _activeFrameGroupInternal = _type.FrameGroups?[_activeFrameGroupIndex];
                }

                return _activeFrameGroupInternal;
            }
        }

        private int _activeFrameGroupIndexInternal = -1;
        private Protobuf.Appearances.FrameGroup _activeFrameGroupInternal = null;
        
        public uint Id {
            get => _id;
        }
        public AppearanceType Type {
            get => _type;
        }

        public virtual int Phase {
            set {
                var animator = _animators?[_activeFrameGroupIndex];
                if (animator != null)
                    animator.Phase = value;
            }
            
            get {
                var animator = _animators?[_activeFrameGroupIndex];
                return animator != null ? animator.Phase : 0;
            }
        }

        public AppearanceInstance(uint id, AppearanceType type) {
            _id = id;
            _type = type;

            if (!!_type && _type.FrameGroups != null) {
                _animators = new Animation.IAppearanceAnimator[_type.FrameGroups.Count];
                for (int i = 0; i < _animators.Length; i++) {
                    var animator = type.FrameGroups[i].SpriteInfo.Animator?.Clone();
                    if (animator is Animation.LegacyAnimator legacyAnimator)
                        legacyAnimator.Initialise(type);

                    _animators[i] = animator;
                }
            }
        }

        public virtual int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int phase = Phase % (int)_activeFrameGroup.SpriteInfo.Phases;
            if (!(phase == _lastInternalPhase && patternX == _lastPatternX && patternX >= 0 && patternY == _lastPatternY && patternY >= 0 && patternZ == _lastPatternZ && patternZ >= 0)) {
                _lastInternalPhase = phase;
                _lastPatternX = patternX;
                _lastPatternY = patternY;
                _lastPatternZ = patternZ;

                _lastCachedSpriteIndex = _activeFrameGroup.SpriteInfo.CalculateSpriteIndex(phase, patternX, patternY, patternZ);
            }

            return _lastCachedSpriteIndex + (layer >= 0 ? layer % (int)_activeFrameGroup.SpriteInfo.Layers : 0);
        }

        public CachedSprite GetSprite(int layer, int patternX, int patternY, int patternZ, bool animation) {
            if (_type.FrameGroups == null)
                return null;

            var spriteIndex = GetSpriteIndex(layer, patternX, patternY, patternZ);
            if (_cachedSprites == null) {
                _cachedSprites = new List<CachedSpriteRequest[]>();
                foreach (var frameGroup in _type.FrameGroups)
                    _cachedSprites.Add(new CachedSpriteRequest[frameGroup.SpriteInfo.SpriteIDs.Count]);
            }

            CachedSpriteRequest cachedRequest = _cachedSprites[_activeFrameGroupIndex][spriteIndex];
            if (cachedRequest == null) {
                CachedSprite cachedSprite;

                var spriteId = _activeFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                var status = OpenTibiaUnity.AppearanceStorage.GetSprite(spriteId, out cachedSprite);
                _cachedSprites[_activeFrameGroupIndex][spriteIndex] = new CachedSpriteRequest(status, cachedSprite);
                return cachedSprite; // may be null if this is the first time to load this texture
            }

            // if it was loading, then update status
            if (cachedRequest.Status == SpriteLoadingStatus.Loading) {
                var spriteId = _activeFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                cachedRequest.Status = OpenTibiaUnity.AppearanceStorage.GetSprite(spriteId, out cachedRequest.CachedSprite);
            }

            return cachedRequest.CachedSprite;
        }
        
        public virtual void Draw(Vector2 screenPosition, Vector2 zoom, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            // this requires a bit of explanation
            // on outfits, layers are not useful, instead it relies on phases
            // while on the rest of categories, layers are treated as indepedant sprites..
            // the phase is used to determine the initial sprite index (at the animation property)
            // then the amount of layers based on that index is drawn
            // mostly you'd see this in objects like doors, or in effects like assassin outfit

            bool dontDraw = false;
            var cachedSprites = new CachedSprite[_activeFrameGroup.SpriteInfo.Layers];
            for (int layer = 0; layer < _activeFrameGroup.SpriteInfo.Layers; layer++) {
                var cachedSprite = GetSprite(layer, patternX, patternY, patternZ, _activeFrameGroup.SpriteInfo.IsAnimation);
                if (cachedSprite == null)
                    dontDraw = true;

                cachedSprites[layer] = cachedSprite;
            }

            if (dontDraw)
                return;

            foreach (var cachedSprite in cachedSprites)
                InternalDrawTo(screenPosition.x, screenPosition.y, zoom, highlighted, highlightOpacity, cachedSprite);
        }
        
        protected void InternalDrawTo(float screenX, float screenY, Vector2 zoom, bool highlighted, float highlightOpacity,
            CachedSprite cachedSprite, Material material = null) {
            s_TempPoint.Set(screenX - _type.OffsetX, screenY - _type.OffsetY);
            if (ClampeToFieldSize) {
                s_TempRect.position = s_TempPoint * zoom;
                s_TempRect.size = s_FieldVector * zoom;
            } else {
                s_TempRect.position = (s_TempPoint - cachedSprite.size + s_FieldVector) * zoom;
                s_TempRect.size = cachedSprite.size * zoom;
            }

            if (material == null)
                material = OpenTibiaUnity.GameManager.AppearanceTypeMaterial;

            material.SetFloat("_HighlightOpacity", highlighted ? highlightOpacity : 0);
            Graphics.DrawTexture(s_TempRect, cachedSprite.texture, cachedSprite.rect, 0, 0, 0, 0, material);
        }

        public virtual void SwitchFrameGroup(int _, int __) { }

        public virtual bool Animate(int ticks, int delay = 0) {
            var animator = _animators?[_activeFrameGroupIndex];
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
