using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Components
{
    public enum DraggingPolicy
    {
        FullTransform,
        TopExpanding,
        LeftExpanding,
        BottomExpanding,
        RightExpanding
    }

    public class Draggable : Base.AbstractComponent, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        public bool bindRectToParent = true;
        public DraggingPolicy draggingPolicy = DraggingPolicy.FullTransform;
        public int draggingBoxSize = 5;
        
        private Vector2 m_InitialMousePosition = Vector2.zero;
        private Vector3 m_InitialPosition = Vector3.zero;
        private bool m_DraggingEnabled = false;

        public Vector2 CalculateRelativeMousePosition() {
            return CalculateRelativeMousePosition(Input.mousePosition);
        }

        public Vector2 CalculateRelativeMousePosition(Vector3 mousePosition) {
            var pivotDelta = rectTransform.pivot - new Vector2(0, 1);
            var size = rectTransform.rect.size;
            return new Vector2(mousePosition.x + (pivotDelta.x * size.x), mousePosition.y + (pivotDelta.y * size.y));
        }

        public void OnDrag(PointerEventData eventData) {
            if (m_DraggingEnabled) {
                var relativeMousePosition = CalculateRelativeMousePosition();

                float x = m_InitialPosition.x + (relativeMousePosition.x - m_InitialMousePosition.x);
                float y = m_InitialPosition.y + (relativeMousePosition.y - m_InitialMousePosition.y);
                var newPosition = new Vector2(x, y);

                transform.position = newPosition;
                if (bindRectToParent)
                    ClampToParent();
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            m_DraggingEnabled = false;
        }

        public void OnPointerDown(PointerEventData eventData) {
            m_InitialMousePosition = CalculateRelativeMousePosition(eventData.position);
            m_InitialPosition = transform.position;

            Vector2 initialOffset = new Vector2() {
                x = Mathf.Abs(m_InitialMousePosition.x - m_InitialPosition.x),
                y= Mathf.Abs(m_InitialPosition.y - m_InitialMousePosition.y)
            };
            
            if (draggingPolicy != DraggingPolicy.FullTransform) {
                if (draggingBoxSize <= 0)
                    return;

                var size = (transform as RectTransform).rect.size;
                switch (draggingPolicy) {
                    case DraggingPolicy.TopExpanding:
                        // any x but y must be clamped
                        if (initialOffset.y > draggingBoxSize)
                            return;
                        break;

                    case DraggingPolicy.LeftExpanding:
                        if (initialOffset.x > draggingBoxSize)
                            return;
                        break;

                    case DraggingPolicy.BottomExpanding:
                        if (initialOffset.y > (size.y - draggingBoxSize))
                            return;
                        break;

                    case DraggingPolicy.RightExpanding:
                        if (initialOffset.x > (size.x - draggingBoxSize))
                            return;
                        break;

                    default:
                        return;
                }
            }

            m_DraggingEnabled = true;
        }
    }
}
