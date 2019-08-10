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

        [SerializeField] private float m_MinSize = 14f;

        protected void LateUpdate() {
            var horizontalScrollbar = scrollRect.horizontalScrollbar;
            var verticalScrollBar = scrollRect.verticalScrollbar;
            var scrollRectTransform = scrollRect.transform as RectTransform;
            
            if (verticalScrollBar) {
                var totalSliderAreaHeight = (verticalScrollBar.handleRect.parent as RectTransform).rect.height;
                var minSize = m_MinSize / totalSliderAreaHeight;
                if (verticalScrollBar.size < minSize)
                    verticalScrollBar.size = minSize;
            }

            if (horizontalScrollbar) {
                var totalSliderAreaWidth = (horizontalScrollbar.handleRect.parent as RectTransform).rect.width;
                var minSize = m_MinSize / totalSliderAreaWidth;
                if (horizontalScrollbar.size < minSize)
                    horizontalScrollbar.size = minSize;
            }
        }
    }
}
