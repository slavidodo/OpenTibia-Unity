using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public sealed class MissileInstance : AppearanceInstance
    {
        private readonly int _patternX;
        private readonly int _patternY;
        private readonly int _animationEnd = 0;
        private Vector3Int _target;
        private Vector3Int _position;

        private Vector2Int _animationDelta;
        private readonly Vector3 _animationSpeed;

        public readonly int AnimationDirection;

        public Vector3Int Target { get => _target; }
        public Vector3Int Position { get => _position; }
        public Vector3Int AnimationDelta {
            get {
                return new Vector3Int {
                    x = _animationDelta.x + (_target.x - _position.x) * Constants.FieldSize,
                    y = _animationDelta.y + (_target.y - _position.y) * Constants.FieldSize
                };
            }
        }

        public MissileInstance(uint id, AppearanceType type, Vector3Int fromPosition, Vector3Int toPosition) : base(id, type) {
            Phase = Constants.PhaseAsynchronous;

            _animationDelta = new Vector2Int(toPosition.x - fromPosition.x, toPosition.y - fromPosition.y);
            if (_animationDelta.x == 0) {
                if (_animationDelta.y <= 0) {
                    AnimationDirection = 0;
                } else {
                    AnimationDirection = 4;
                }
            } else if (_animationDelta.x > 0) {
                if (256 * _animationDelta.y > 618 * _animationDelta.x) {
                    AnimationDirection = 4;
                } else if (256 * _animationDelta.y > 106 * _animationDelta.x) {
                    AnimationDirection = 3;
                } else if (256 * _animationDelta.y > -106 * _animationDelta.x) {
                    AnimationDirection = 2;
                } else if (256 * _animationDelta.y > -618 * _animationDelta.x) {
                    AnimationDirection = 1;
                } else {
                    AnimationDirection = 0;
                }
            } else if (-256 * _animationDelta.y < 618 * _animationDelta.x) {
                AnimationDirection = 4;
            } else if (-256 * _animationDelta.y < 106 * _animationDelta.x) {
                AnimationDirection = 5;
            } else if (-256 * _animationDelta.y < -106 * _animationDelta.x) {
                AnimationDirection = 6;
            } else if (-256 * _animationDelta.y < -618 * _animationDelta.x) {
                AnimationDirection = 7;
            } else {
                AnimationDirection = 0;
            }

            switch (AnimationDirection) {
                case 0:
                    _patternX = 1;
                    _patternY = 0;
                    break;
                case 1:
                    _patternX = 2;
                    _patternY = 0;
                    break;
                case 2:
                    _patternX = 2;
                    _patternY = 1;
                    break;
                case 3:
                    _patternX = 2;
                    _patternY = 2;
                    break;
                case 4:
                    _patternX = 1;
                    _patternY = 2;
                    break;
                case 5:
                    _patternX = 0;
                    _patternY = 2;
                    break;
                case 6:
                    _patternX = 0;
                    _patternY = 1;
                    break;
                case 7:
                    _patternX = 0;
                    _patternY = 0;
                    break;
            }

            float duration = Mathf.Sqrt(_animationDelta.magnitude) * 150;
            _animationDelta.x *= -Constants.FieldSize;
            _animationDelta.y *= -Constants.FieldSize;
            _animationSpeed = new Vector3(_animationDelta.x, _animationDelta.y, duration);
            _animationEnd = OpenTibiaUnity.TicksMillis + (int)duration;
            _target = toPosition;
            _position = fromPosition;
        }

        public override int GetSpriteIndex(int layer, int patternX, int patternY, int patternZ) {
            int idleIndex = (int)Protobuf.Shared.FrameGroupType.Idle;
            int phase = layer >= 0 ? (int)(layer % _type.FrameGroups[idleIndex].SpriteInfo.Phases) : Phase;
            return (int)(((phase * _type.FrameGroups[idleIndex].SpriteInfo.PatternDepth + 0)
                * _type.FrameGroups[idleIndex].SpriteInfo.PatternHeight + _patternY)
                * _type.FrameGroups[idleIndex].SpriteInfo.PatternWidth + _patternX);
        }

        public override bool Animate(int ticks, int delay = 0) {
            base.Animate(ticks, delay);

            int elapsedMillis = ticks - (_animationEnd - (int)_animationSpeed.z);
            if (elapsedMillis <= 0) {
                _animationDelta.x = (int)_animationSpeed.x;
                _animationDelta.y = (int)_animationSpeed.y;
            } else if (elapsedMillis >= _animationSpeed.z) {
                _animationDelta.x = 0;
                _animationDelta.y = 0;
            } else {
                _animationDelta.x = (int)_animationSpeed.x - (int)(_animationSpeed.x / _animationSpeed.z * elapsedMillis + 0.5f);
                _animationDelta.y = (int)_animationSpeed.y - (int)(_animationSpeed.y / _animationSpeed.z * elapsedMillis + 0.5f);
            }

            if ((_animationDelta.x == 0 && _animationDelta.y == 0) || ticks >= _animationEnd)
                return false;

            float mX = (_target.x + 1) * Constants.FieldSize - _type.OffsetX + _animationDelta.x;
            float mY = (_target.y + 1) * Constants.FieldSize - _type.OffsetY + _animationDelta.y;
            _position.x = (int)((mX - 1) / Constants.FieldSize);
            _position.y = (int)((mY - 1) / Constants.FieldSize);
            return true;
        }

        public override AppearanceInstance Clone() {
            return new MissileInstance(Id, Type, Position, Target);
        }
    }
}