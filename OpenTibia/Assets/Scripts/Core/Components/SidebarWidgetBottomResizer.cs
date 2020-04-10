using UnityEngine;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class SidebarWidgetBottomResizer : BasicElement,
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        private SidebarWidget _sidebarWidget = null;
        protected SidebarWidget sidebarWidget {
            get {
                if (_sidebarWidget)
                    return _sidebarWidget;

                _sidebarWidget = transform.parent.GetComponent<SidebarWidget>();
                return _sidebarWidget;
            }
        }
        
        private UnityUI.LayoutElement _contentLayout = null;
        protected UnityUI.LayoutElement contentLayout {
            get {
                if (_contentLayout)
                    return _contentLayout;

                _contentLayout = sidebarWidget.content.GetComponent<UnityUI.LayoutElement>();
                return _contentLayout;
            }
        }

        private bool _dragAllowed = false;
        private bool _cursorActivated = false;
        private bool _shouldDeactivateCursor = false;
        private float _initialHeight = 0;
        private Vector2 _initialMousePosition = Vector2.zero;
        
        void IDragHandler.OnDrag(PointerEventData eventData) {
            if (!_dragAllowed)
                return;

            // the y decresed the more we go to the bottom
            // thus we must be reverse the sign
            float deltaY = _initialMousePosition.y - eventData.position.y;
            if (deltaY == 0)
                return;
            
            // remainingHeight will always be equal or greater than minHeight
            // if it wasn't then the widget would't have been placed at this
            // container at all.
            float minHeight = Mathf.Max(20, sidebarWidget.MinHeight);
            float remainingHeight = sidebarWidget.parentContainer.GetRemainingHeight(sidebarWidget);
            float maxHeight = Mathf.Clamp(sidebarWidget.MaxHeight == -1 ? remainingHeight : sidebarWidget.MaxHeight, minHeight, remainingHeight);

            var sizeDelta = sidebarWidget.rectTransform.sizeDelta;
            sidebarWidget.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Clamp(_initialHeight + deltaY, minHeight, maxHeight));
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if (_dragAllowed) {
                _dragAllowed = false;
                _initialHeight = 0;

                if (_shouldDeactivateCursor) {
                    _shouldDeactivateCursor = false;
                    _cursorActivated = false;
                    PopResizeCursor();
                }
            }
        }
        
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            if (sidebarWidget.Resizable && !OpenTibiaUnity.InputHandler.IsAnyMousePressed()) {
                _shouldDeactivateCursor = false;
                _cursorActivated = true;
                PushResizeCursor();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            if (_dragAllowed) {
                _shouldDeactivateCursor = true;
                return;
            }

            if (_cursorActivated) {
                _cursorActivated = false;
                _shouldDeactivateCursor = false;
                PopResizeCursor();
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left || !sidebarWidget.Resizable)
                return;

            _dragAllowed = true;
            _initialMousePosition = eventData.position;
            _initialHeight = sidebarWidget.rectTransform.sizeDelta.y;
        }

        private void PushResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.NResize, CursorPriority.High);
        }


        private void PopResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
        }
    }
}
