using System.Collections.Generic;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

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

    public abstract class AppearanceInstance
    {
        public const int UnknownCreature = 97;
        public const int OutdatedCreature = 98;
        public const int Creature = 99;

        public const int HookEast = 19;
        public const int HookSouth = 20;

        private static readonly Mesh s_mesh;

        static AppearanceInstance() {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            s_mesh = gameObject.GetComponent<MeshFilter>().mesh;
            Object.Destroy(gameObject);

            s_mesh.vertices = new Vector3[] {
                new Vector3(0, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
            };
        }

        public int MapData { get; set; } = -1; // stack position
        public int MapField { get; set; } = -1; // field index in cached map

        protected uint _id;
        protected int _lastCachedSpriteIndex = -1;
        protected int _lastInternalPhase = -1;
        protected int _activeFrameGroupIndex = 0;
        protected int _lastPatternX = -1;
        protected int _lastPatternY = -1;
        protected int _lastPatternZ = -1;

        private Vector2 _screenPosition = Vector2.zero;
        protected Matrix4x4 _trsMatrix = Matrix4x4.identity;
        protected bool _shouldRecalculateTRS = true;

        protected AppearanceType _type;
        protected Animation.IAppearanceAnimator[] _animators;
        protected List<CachedSpriteRequest[]> _cachedSprites;

        private bool _clamping = false;
        private Vector2 _clampingArea = Vector2.zero;

        private bool _offsetDisabled = false;
        public bool OffsetDisabled {
            get => _offsetDisabled;
            set {
                if (value != _clamping) {
                    _shouldRecalculateTRS = true;
                    _offsetDisabled = value;
                }
            }
        }

        protected Protobuf.Appearances.FrameGroup ActiveFrameGroup {
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

        public void InvalidateTRS() {
            _shouldRecalculateTRS = true;
        }

        public void SetClamping(bool clamping, float square = Constants.FieldSize) {
            SetClamping(clamping, new Vector2(square, square));
        }

        public void SetClamping(bool clamping, Vector2 square) {
            if (clamping != _clamping) {
                _clamping = clamping;
                _clampingArea = square;
                InvalidateTRS();
            } else if (_clamping && square != _clampingArea) {
                _clampingArea = square;
                InvalidateTRS();
            }
        }

        public virtual int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int phase = Phase % (int)ActiveFrameGroup.SpriteInfo.Phases;
            if (!(phase == _lastInternalPhase && patternX == _lastPatternX && patternX >= 0 && patternY == _lastPatternY && patternY >= 0 && patternZ == _lastPatternZ && patternZ >= 0)) {
                _lastInternalPhase = phase;
                _lastPatternX = patternX;
                _lastPatternY = patternY;
                _lastPatternZ = patternZ;

                _lastCachedSpriteIndex = ActiveFrameGroup.SpriteInfo.CalculateSpriteIndex(phase, patternX, patternY, patternZ);
            }

            return _lastCachedSpriteIndex + (layer >= 0 ? layer % (int)ActiveFrameGroup.SpriteInfo.Layers : 0);
        }

        public CachedSprite GetSprite(int layer, int patternX, int patternY, int patternZ, bool _) {
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
                var spriteId = ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                var status = OpenTibiaUnity.AppearanceStorage.GetSprite(spriteId, out CachedSprite cachedSprite);

                cachedRequest = new CachedSpriteRequest(status, cachedSprite);
                _cachedSprites[_activeFrameGroupIndex][spriteIndex] = cachedRequest;
            } else if (cachedRequest.Status == SpriteLoadingStatus.Loading) { // if it was loading, then update status
                var spriteId = ActiveFrameGroup.SpriteInfo.SpriteIDs[spriteIndex];
                cachedRequest.Status = OpenTibiaUnity.AppearanceStorage.GetSprite(spriteId, out cachedRequest.CachedSprite);
            }

            return cachedRequest.CachedSprite;
        }

        public virtual void Draw(CommandBuffer commandBuffer, Vector2Int screenPosition, int patternX,
                                int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            // this requires a bit of explanation
            // on outfits, layers are not useful, instead it relies on phases
            // while on the rest of categories, layers are treated as indepedant sprites..
            // the phase is used to determine the initial sprite index (at the animation property)
            // then the amount of layers based on that index is drawn
            // mostly you'd see this in objects like doors, or in effects like assassin outfit

            bool dontDraw = false;
            var cachedSprites = new CachedSprite[ActiveFrameGroup.SpriteInfo.Layers];
            for (int layer = 0; layer < ActiveFrameGroup.SpriteInfo.Layers; layer++) {
                var cachedSprite = GetSprite(layer, patternX, patternY, patternZ, ActiveFrameGroup.SpriteInfo.IsAnimation);
                if (cachedSprite == null)
                    dontDraw = true;

                cachedSprites[layer] = cachedSprite;
            }

            if (dontDraw)
                return;

            foreach (var cachedSprite in cachedSprites)
                InternalDraw(commandBuffer, screenPosition, highlighted, highlightOpacity, cachedSprite);
        }
        
        protected void InternalDraw(CommandBuffer commandBuffer, Vector2Int screenPosition, bool highlighted,
                                      float highlightOpacity, CachedSprite cachedSprite, Material material = null, MaterialPropertyBlock props = null) {
            if (_shouldRecalculateTRS) {
                var position = new Vector2(screenPosition.x, screenPosition.y);
                if (!OffsetDisabled)
                    position -= _type.Offset;

                if (_clamping) {
                    var square = new Vector2(Mathf.Max(_clampingArea.x, Type.BoundingSquare), Mathf.Max(_clampingArea.y, Type.BoundingSquare));
                    var size = cachedSprite.size;

                    position -= size - square; // the area left is actually empty
                    position /= square / _clampingArea;
                    _trsMatrix = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), _clampingArea * (size / square));
                } else {
                    position += new Vector2(Constants.FieldSize, Constants.FieldSize) - cachedSprite.size;
                    _trsMatrix = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), cachedSprite.size);
                }

                _screenPosition = screenPosition;
                _shouldRecalculateTRS = false;
            } else if (_screenPosition != screenPosition) {
                _trsMatrix[0, 3] += (screenPosition.x - _screenPosition.x);
                _trsMatrix[1, 3] += (screenPosition.y - _screenPosition.y);
                _screenPosition = screenPosition;
            }

            if (material == null)
                material = OpenTibiaUnity.GameManager.AppearanceTypeMaterial;

            if (props == null)
                props = cachedSprite.materialProperyBlock;

            props.SetFloat("_HighlightOpacity", highlighted ? highlightOpacity : 0);
            if (commandBuffer != null)
                commandBuffer.DrawMesh(s_mesh, _trsMatrix, material, 0, 0, props);
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

        public abstract AppearanceInstance Clone();

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
