using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    public class MiniWindowBottomResizer : Base.AbstractComponent,
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        private Base.MiniWindow _miniWindow = null;
        protected Base.MiniWindow miniWindow {
            get {
                if (_miniWindow)
                    return _miniWindow;

                _miniWindow = transform.parent.GetComponent<Base.MiniWindow>();
                return _miniWindow;
            }
        }
        
        private LayoutElement _panelContentLayout = null;
        protected LayoutElement panelContentLayout {
            get {
                if (_panelContentLayout)
                    return _panelContentLayout;

                _panelContentLayout = miniWindow.panelContent.GetComponent<LayoutElement>();
                return _panelContentLayout;
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
            // if it wasn't then the miniwindow would't have been placed at this
            // container at all.
            float minHeight = Mathf.Max(20, miniWindow.MinHeight);
            float remainingHeight = miniWindow.parentContainer.GetRemainingHeight(miniWindow);
            float maxHeight = Mathf.Clamp(miniWindow.MaxHeight == -1 ? remainingHeight : miniWindow.MaxHeight, minHeight, remainingHeight);

            var sizeDelta = miniWindow.rectTransform.sizeDelta;
            miniWindow.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Clamp(_initialHeight + deltaY, minHeight, maxHeight));
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
            if (miniWindow.Resizable && !OpenTibiaUnity.InputHandler.IsAnyMousePressed()) {
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
            if (eventData.button != PointerEventData.InputButton.Left || !miniWindow.Resizable)
                return;

            _dragAllowed = true;
            _initialMousePosition = eventData.position;
            _initialHeight = miniWindow.rectTransform.sizeDelta.y;
        }

        private void PushResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.NResize, CursorPriority.High);
        }


        private void PopResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
        }
    }
}
