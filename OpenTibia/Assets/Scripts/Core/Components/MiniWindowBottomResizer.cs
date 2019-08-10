using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    internal class MiniWindowBottomResizer : Base.AbstractComponent,
        IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        private Base.MiniWindow m_MiniWindow = null;
        protected Base.MiniWindow miniWindow {
            get {
                if (m_MiniWindow)
                    return m_MiniWindow;

                m_MiniWindow = transform.parent.GetComponent<Base.MiniWindow>();
                return m_MiniWindow;
            }
        }
        
        private LayoutElement m_PanelContentLayout = null;
        protected LayoutElement panelContentLayout {
            get {
                if (m_PanelContentLayout)
                    return m_PanelContentLayout;

                m_PanelContentLayout = miniWindow.panelContent.GetComponent<LayoutElement>();
                return m_PanelContentLayout;
            }
        }

        private bool m_DragAllowed = false;
        private bool m_CursorActivated = false;
        private bool m_ShouldDeactivateCursor = false;
        private float m_InitialHeight = 0;
        private Vector2 m_InitialMousePosition = Vector2.zero;
        
        void IDragHandler.OnDrag(PointerEventData eventData) {
            if (!m_DragAllowed)
                return;

            // the y decresed the more we go to the bottom
            // thus we must be reverse the sign
            float deltaY = m_InitialMousePosition.y - eventData.position.y;
            if (deltaY == 0)
                return;
            
            // remainingHeight will always be equal or greater than minHeight
            // if it wasn't then the miniwindow would't have been placed at this
            // container at all.
            float minHeight = Mathf.Max(20, miniWindow.MinHeight);
            float remainingHeight = miniWindow.parentContainer.GetRemainingHeight(miniWindow);
            float maxHeight = Mathf.Clamp(miniWindow.MaxHeight == -1 ? remainingHeight : miniWindow.MaxHeight, minHeight, remainingHeight);

            var sizeDelta = miniWindow.rectTransform.sizeDelta;
            miniWindow.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Clamp(m_InitialHeight + deltaY, minHeight, maxHeight));
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            if (m_DragAllowed) {
                m_DragAllowed = false;
                m_InitialHeight = 0;

                if (m_ShouldDeactivateCursor) {
                    m_ShouldDeactivateCursor = false;
                    m_CursorActivated = false;
                    PopResizeCursor();
                }
            }
        }
        
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
            if (miniWindow.Resizable) {
                m_ShouldDeactivateCursor = false;
                m_CursorActivated = true;
                PushResizeCursor();
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
            if (m_DragAllowed) {
                m_ShouldDeactivateCursor = true;
                return;
            }

            if (m_CursorActivated) {
                m_CursorActivated = false;
                m_ShouldDeactivateCursor = false;
                PopResizeCursor();
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (!miniWindow.Resizable)
                return;

            m_DragAllowed = true;
            m_InitialMousePosition = eventData.position;
            m_InitialHeight = miniWindow.rectTransform.sizeDelta.y;
        }

        private void PushResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.NResize, CursorPriority.High);
        }


        private void PopResizeCursor() {
            OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.High);
        }
    }
}
