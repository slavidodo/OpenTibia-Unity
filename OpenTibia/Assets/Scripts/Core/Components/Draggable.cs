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
        
        private Vector2 _initialMousePositionOverrider = Vector2.zero;
        private Vector2 _initialMousePosition = Vector2.zero;
        private Vector3 _initialPosition = Vector3.zero;
        private bool _draggingEnabled = false;

        public DragEvent onBeginDrag = null;
        public DragEvent onDrag = null;
        public DragEvent onEndDrag = null;

        protected override void Awake() {
            base.Awake();

            onBeginDrag = new DragEvent();
            onDrag = new DragEvent();
            onEndDrag = new DragEvent();
        }

        public override Vector2 CalculateAbsoluteMousePosition(Vector2 mousePosition) {
            return new Vector2(mousePosition.x, mousePosition.y) + _initialMousePositionOverrider;
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (_draggingEnabled) {
                onBeginDrag.Invoke(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData) {
            if (_draggingEnabled) {
                var relativeMousePosition = CalculateAbsoluteMousePosition(eventData.position);

                float x = _initialPosition.x + (relativeMousePosition.x - _initialMousePosition.x);
                float y = _initialPosition.y + (relativeMousePosition.y - _initialMousePosition.y);
                if (transform.position.x != x || transform.position.y != y)
                    transform.position = new Vector2(x, y);

                if (bindRectToParent)
                    ClampToParent();
                
                onDrag.Invoke(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            if (_draggingEnabled) {
                _draggingEnabled = false;

                onEndDrag.Invoke(eventData);
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            var pivotDelta = rectTransform.pivot - new Vector2(0, 1);
            var size = rectTransform.rect.size;

            _initialMousePositionOverrider = pivotDelta * size;
            _initialMousePosition = CalculateAbsoluteMousePosition(eventData.position);
            _initialPosition = rectTransform.position;

            Vector2 initialOffset = new Vector2() {
                x = Mathf.Abs(_initialMousePosition.x - _initialPosition.x),
                y = Mathf.Abs(_initialPosition.y - _initialMousePosition.y)
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

            _draggingEnabled = true;
        }
    }
}
