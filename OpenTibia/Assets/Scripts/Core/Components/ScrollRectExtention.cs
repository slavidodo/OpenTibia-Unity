using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectExtention : Base.AbstractComponent
    {
        private ScrollRect m_ScrollRect;
        public ScrollRect scrollRect {
            get {
                if (m_ScrollRect == null)
                    m_ScrollRect = GetComponent<ScrollRect>();

                return m_ScrollRect;
            }
        }

        protected void LateUpdate() {
            var horizontalScrollbar = scrollRect.horizontalScrollbar;
            var verticalScrollBar = scrollRect.verticalScrollbar;
            var scrollRectTransform = verticalScrollBar.transform as RectTransform;
            
            if (verticalScrollBar) {
                var minSize = 20f / scrollRectTransform.rect.height;
                if (verticalScrollBar.size < minSize)
                    verticalScrollBar.size = minSize;
            }

            if (horizontalScrollbar) {
                var minSize = 20f / scrollRectTransform.rect.width;
                if (horizontalScrollbar.size < minSize)
                    horizontalScrollbar.size = minSize;
            }
        }
    }
}
