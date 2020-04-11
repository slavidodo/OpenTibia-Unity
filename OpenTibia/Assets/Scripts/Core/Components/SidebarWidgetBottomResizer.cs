using UnityEngine;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class SidebarWidgetBottomResizer : BasicElement,
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {

        // fields
        private bool _mouseOverResizer = false;
        private bool _mouseHeldDown = false;
        private bool _cursorPushed = false;
        private float _initialHeight = 0;
        private Vector2 _initialMousePosition = Vector2.zero;

        // properties
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

        protected override void OnEnable() {
            base.OnEnable();

            OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utils.EventImplPriority.High, OnMouseUp);
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.RemoveMouseUpListener(OnMouseUp);
        }

        public void OnPointerDown(PointerEventData e) {
            if (e.button != PointerEventData.InputButton.Left || !sidebarWidget.Resizable)
                return;

            _mouseHeldDown = true;
            _initialMousePosition = e.position;
            _initialHeight = sidebarWidget.rectTransform.sizeDelta.y;
            e.Use();
        }

        private void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            // valid actions are using left mouse button & if drag is supposed to happen
            // also this might be handled alread by onDragEnd
            if (mouseButton != MouseButton.Left || !_mouseHeldDown)
                return;

            _mouseHeldDown = false;

            // if mouse is not currrently over the resizer, pop the cursor
            if (!_mouseOverResizer)
                PopResizeCursor();

            e.Use();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            _mouseOverResizer = true;

            // if no mouse button is down, then it's valid to update the cursor
            if (sidebarWidget.Resizable && !OpenTibiaUnity.InputHandler.IsAnyMousePressed()) {
                _cursorPushed = true;
                PushResizeCursor();
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            _mouseOverResizer = false;

            // if no drag is occuring and cursor has been pushed, then pop it
            if (!_mouseHeldDown && _cursorPushed)
                PopResizeCursor();
        }

        public void OnDrag(PointerEventData e) {
            // valid actions are using left mouse button & if drag is supposed to happen
            if (e.button != PointerEventData.InputButton.Left && _mouseHeldDown)
                return;

            UpdateWidgetSize(e.position);
            e.Use();
        }

        public void OnEndDrag(PointerEventData eventData) {
            // we want drag end and poiter up to have the same effect
            // and dragEnd will cancel pointer up most of the time
            // but in case the cursor didnt move at all, OnPointerUp
            // will be called diretly by the input module
            //OnPointerUp(eventData);
        }

        private void UpdateWidgetSize(Vector2 mousePosition) {
            // the y decresed the more we go to the bottom
            // thus we must be reverse the sign
            float deltaY = _initialMousePosition.y - mousePosition.y;
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

        private void PushResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.NResize, CursorPriority.High);
        }


        private void PopResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
        }
    }
}
