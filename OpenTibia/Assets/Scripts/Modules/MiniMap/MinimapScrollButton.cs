using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.MiniMap
{
    [RequireComponent(typeof(Button))]
    public class MinimapScrollButton : Core.Components.Base.AbstractComponent
    {
        private Button m_ButtonComponent = null;
        public Button buttonComponent {
            get {
                if (!m_ButtonComponent)
                    m_ButtonComponent = GetComponent<Button>();
                return m_ButtonComponent;
            }
        }

        private RawImage m_RawImageComponent = null;
        public RawImage rawImageComponent {
            get {
                if (!m_RawImageComponent)
                    m_RawImageComponent = GetComponent<RawImage>();
                return m_RawImageComponent;
            }
        }
        
        [SerializeField] private Texture2D m_DefaultTexture = null;
        [SerializeField] private Texture2D m_NorthClicked = null;
        [SerializeField] private Texture2D m_EastClicked = null;
        [SerializeField] private Texture2D m_SouthClicked = null;
        [SerializeField] private Texture2D m_WestClicked = null;
        [SerializeField] private Texture2D m_NorthEastClicked = null;
        [SerializeField] private Texture2D m_NorthWestClicked = null;
        [SerializeField] private Texture2D m_SouthWestClicked = null;
        [SerializeField] private Texture2D m_SouthEastClicked = null;

        private Direction m_Direction = Direction.Stop;
        private Direction m_LastDirection = Direction.Stop;
        private bool m_MouseDown = false;

        private void OnGUI() {
            var e = Event.current;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) {
                if (!m_MouseDown)
                    return;

                var direction = CalculateDirection(e);
                if (direction != m_Direction)
                    direction = Direction.Stop;

                if (direction != m_LastDirection)
                    ChangeTexture(direction);
                else
                    m_LastDirection = direction;
            } else if (e.type == EventType.MouseDown) {
                m_MouseDown = true;

                m_Direction = CalculateDirection(e);
                ChangeTexture(m_Direction);
            } else if (e.type == EventType.MouseUp) {
                m_MouseDown = false;
                rawImageComponent.texture = m_DefaultTexture;

                if (m_LastDirection == m_Direction) {
                    switch (m_Direction) {
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
                
                m_LastDirection = Direction.Stop;
                m_Direction = Direction.Stop;
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
                    texture = m_NorthClicked;
                    break;
                case Direction.East:
                    texture = m_EastClicked;
                    break;
                case Direction.South:
                    texture = m_SouthClicked;
                    break;
                case Direction.West:
                    texture = m_WestClicked;
                    break;
                case Direction.NorthEast:
                    texture = m_NorthEastClicked;
                    break;
                case Direction.SouthEast:
                    texture = m_SouthEastClicked;
                    break;
                case Direction.SouthWest:
                    texture = m_SouthWestClicked;
                    break;
                case Direction.NorthWest:
                    texture = m_NorthWestClicked;
                    break;
                default:
                    texture = m_DefaultTexture;
                    break;
            }

            rawImageComponent.texture = texture;
            m_LastDirection = direction;
        }
    }
}
