using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.MiniMap
{
    [RequireComponent(typeof(UI.Legacy.Button))]
    public class MiniMapScroll : Core.Components.Base.AbstractComponent
    {
        // serialized fields
        [SerializeField]
        private Texture2D _defaultTexture = null;
        [SerializeField]
        private Texture2D _northClicked = null;
        [SerializeField]
        private Texture2D _eastClicked = null;
        [SerializeField]
        private Texture2D _southClicked = null;
        [SerializeField]
        private Texture2D _westClicked = null;
        [SerializeField]
        private Texture2D _northEastClicked = null;
        [SerializeField]
        private Texture2D _northWestClicked = null;
        [SerializeField]
        private Texture2D _southWestClicked = null;
        [SerializeField]
        private Texture2D _southEastClicked = null;

        // fields
        private Direction _direction = Direction.Stop;
        private Direction _lastDirection = Direction.Stop;
        private bool _mouseDown = false;

        // properties
        private UI.Legacy.Button _buttonComponent = null;
        public UI.Legacy.Button buttonComponent {
            get {
                if (!_buttonComponent)
                    _buttonComponent = GetComponent<UI.Legacy.Button>();
                return _buttonComponent;
            }
        }

        private UnityUI.RawImage _rawImageComponent = null;
        public UnityUI.RawImage rawImageComponent {
            get {
                if (!_rawImageComponent)
                    _rawImageComponent = GetComponent<UnityUI.RawImage>();
                return _rawImageComponent;
            }
        }

        private void OnGUI() {
            var e = Event.current;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) {
                if (!_mouseDown)
                    return;

                var direction = CalculateDirection(e);
                if (direction != _direction)
                    direction = Direction.Stop;

                if (direction != _lastDirection)
                    ChangeTexture(direction);
                else
                    _lastDirection = direction;
            } else if (e.type == EventType.MouseDown) {
                _mouseDown = true;

                _direction = CalculateDirection(e);
                ChangeTexture(_direction);
            } else if (e.type == EventType.MouseUp) {
                _mouseDown = false;
                rawImageComponent.texture = _defaultTexture;

                if (_lastDirection == _direction) {
                    switch (_direction) {
                        case Direction.North:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(0, -1, 0);
                            break;
                        case Direction.East:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, 0, 0);
                            break;
                        case Direction.South:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(0, 1, 0);
                            break;
                        case Direction.West:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, 0, 0);
                            break;
                        case Direction.NorthEast:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, -1, 0);
                            break;
                        case Direction.SouthEast:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, 1, 0);
                            break;
                        case Direction.SouthWest:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, 1, 0);
                            break;
                        case Direction.NorthWest:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, -1, 0);
                            break;
                        default:
                            break;
                    }
                }
                
                _lastDirection = Direction.Stop;
                _direction = Direction.Stop;
            }
        }

        private Direction CalculateDirection(Event e) {
            var mousePosition = Input.mousePosition;
            var position = transform.position;
            var size = rawImageComponent.rectTransform.rect.size;

            // (0, 0) at bottomleft
            var rA = size.y / 2f;
            var rB = rA - 6;
            var distance = Mathf.Sqrt(Mathf.Pow(mousePosition.x - position.x, 2) + Mathf.Pow(mousePosition.y - position.y, 2));

            Direction direction = Direction.Stop;
            if (distance >= rB && distance <= rA) {
                var delta = mousePosition - position;
                if (delta.y <= 7 & delta.y >= -7)
                    direction = delta.x < 0 ? Direction.West : Direction.East;
                else if (delta.x >= -7 && delta.x <= 7)
                    direction = delta.y > 0 ? Direction.North : Direction.South;
                else
                    direction = delta.y > 0 ?
                        delta.x < 0 ? Direction.NorthWest : Direction.NorthEast :
                        delta.x < 0 ? Direction.SouthWest : Direction.SouthEast;
            }

            return direction;
        }

        private void ChangeTexture(Direction direction) {
            Texture2D texture = null;
            switch (direction) {
                case Direction.North:
                    texture = _northClicked;
                    break;
                case Direction.East:
                    texture = _eastClicked;
                    break;
                case Direction.South:
                    texture = _southClicked;
                    break;
                case Direction.West:
                    texture = _westClicked;
                    break;
                case Direction.NorthEast:
                    texture = _northEastClicked;
                    break;
                case Direction.SouthEast:
                    texture = _southEastClicked;
                    break;
                case Direction.SouthWest:
                    texture = _southWestClicked;
                    break;
                case Direction.NorthWest:
                    texture = _northWestClicked;
                    break;
                default:
                    texture = _defaultTexture;
                    break;
            }

            rawImageComponent.texture = texture;
            _lastDirection = direction;
        }
    }
}
