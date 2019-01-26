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

#pragma warning disable CS0649 // never assigned to
        [SerializeField] private Texture2D m_DefaultTexture;
        [SerializeField] private Texture2D m_NorthClicked;
        [SerializeField] private Texture2D m_EastClicked;
        [SerializeField] private Texture2D m_SouthClicked;
        [SerializeField] private Texture2D m_WestClicked;
        [SerializeField] private Texture2D m_NorthEastClicked;
        [SerializeField] private Texture2D m_NorthWestClicked;
        [SerializeField] private Texture2D m_SouthWestClicked;
        [SerializeField] private Texture2D m_SouthEastClicked;
#pragma warning restore CS0649 // never assigned to

        private Directions m_Direction = Directions.Stop;
        private Directions m_LastDirection = Directions.Stop;
        private bool m_MouseDown = false;

        private void OnGUI() {
            var e = Event.current;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) {
                if (!m_MouseDown)
                    return;

                var direction = CalculateDirection(e);
                if (direction != m_Direction)
                    direction = Directions.Stop;

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
                        case Directions.North:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(0, -1, 0);
                            break;
                        case Directions.East:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, 0, 0);
                            break;
                        case Directions.South:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(0, 1, 0);
                            break;
                        case Directions.West:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, 0, 0);
                            break;
                        case Directions.NorthEast:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, -1, 0);
                            break;
                        case Directions.SouthEast:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(1, 1, 0);
                            break;
                        case Directions.SouthWest:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, 1, 0);
                            break;
                        case Directions.NorthWest:
                            OpenTibiaUnity.MiniMapRenderer.TranslatePosition(-1, -1, 0);
                            break;
                        default:
                            break;
                    }
                }
                
                m_LastDirection = Directions.Stop;
                m_Direction = Directions.Stop;
            }
        }

        private Directions CalculateDirection(Event e) {
            var mousePosition = Input.mousePosition;
            var position = transform.position;
            var size = rawImageComponent.rectTransform.rect.size;

            // (0, 0) at bottomleft
            var rA = size.y / 2f;
            var rB = rA - 6;
            var distance = Mathf.Sqrt(Mathf.Pow(mousePosition.x - position.x, 2) + Mathf.Pow(mousePosition.y - position.y, 2));

            Directions direction = Directions.Stop;
            if (distance >= rB && distance <= rA) {
                var delta = mousePosition - position;
                if (delta.y <= 7 & delta.y >= -7)
                    direction = delta.x < 0 ? Directions.West : Directions.East;
                else if (delta.x >= -7 && delta.x <= 7)
                    direction = delta.y > 0 ? Directions.North : Directions.South;
                else
                    direction = delta.y > 0 ?
                        delta.x < 0 ? Directions.NorthWest : Directions.NorthEast :
                        delta.x < 0 ? Directions.SouthWest : Directions.SouthEast;
            }

            return direction;
        }

        private void ChangeTexture(Directions direction) {
            Texture2D texture = null;
            switch (direction) {
                case Directions.North:
                    texture = m_NorthClicked;
                    break;
                case Directions.East:
                    texture = m_EastClicked;
                    break;
                case Directions.South:
                    texture = m_SouthClicked;
                    break;
                case Directions.West:
                    texture = m_WestClicked;
                    break;
                case Directions.NorthEast:
                    texture = m_NorthEastClicked;
                    break;
                case Directions.SouthEast:
                    texture = m_SouthEastClicked;
                    break;
                case Directions.SouthWest:
                    texture = m_SouthWestClicked;
                    break;
                case Directions.NorthWest:
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
