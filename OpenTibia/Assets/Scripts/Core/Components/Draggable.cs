using UnityEngine;
using UnityEngine.Events;
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

    public class Draggable : Base.AbstractComponent, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public class DragEvent : UnityEvent<PointerEventData> { }

        public bool bindRectToParent = true;
        public DraggingPolicy draggingPolicy = DraggingPolicy.FullTransform;
        public int draggingBoxSize = 5;
        
        private Vector2 m_InitialMousePositionOverrider = Vector2.zero;
        private Vector2 m_InitialMousePosition = Vector2.zero;
        private Vector3 m_InitialPosition = Vector3.zero;
        private bool m_DraggingEnabled = false;

        public DragEvent onBeginDrag;
        public DragEvent onDrag;
        public DragEvent onEndDrag;

        protected override void Awake() {
            base.Awake();

            onBeginDrag = new DragEvent();
            onDrag = new DragEvent();
            onEndDrag = new DragEvent();
        }
        
        public override Vector2 CalculateRelativeMousePosition(Vector3 mousePosition) {
            return new Vector2(mousePosition.x, mousePosition.y) + m_InitialMousePositionOverrider;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (m_DraggingEnabled) {
                onBeginDrag.Invoke(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (m_DraggingEnabled) {
                var relativeMousePosition = CalculateRelativeMousePosition(eventData.position);

                float x = m_InitialPosition.x + (relativeMousePosition.x - m_InitialMousePosition.x);
                float y = m_InitialPosition.y + (relativeMousePosition.y - m_InitialMousePosition.y);
                if (transform.position.x != x || transform.position.y != y)
                    transform.position = new Vector2(x, y);

                if (bindRectToParent)
                    ClampToParent();
                
                onDrag.Invoke(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (m_DraggingEnabled) {
                m_DraggingEnabled = false;

                onEndDrag.Invoke(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            var pivotDelta = rectTransform.pivot - new Vector2(0, 1);
            var size = rectTransform.rect.size;

            m_InitialMousePositionOverrider = pivotDelta * size;
            m_InitialMousePosition = CalculateRelativeMousePosition(eventData.position);
            m_InitialPosition = rectTransform.position;

            Vector2 initialOffset = new Vector2() {
                x = Mathf.Abs(m_InitialMousePosition.x - m_InitialPosition.x),
                y = Mathf.Abs(m_InitialPosition.y - m_InitialMousePosition.y)
            };
            
            if (draggingPolicy != DraggingPolicy.FullTransform) {
                if (draggingBoxSize <= 0)
                    return;
                
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
