using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectExtention : Base.AbstractComponent
    {
        private ScrollRect _scrollRect;
        public ScrollRect scrollRect {
            get {
                if (_scrollRect == null)
                    _scrollRect = GetComponent<ScrollRect>();

                return _scrollRect;
            }
        }

        [SerializeField] private float _minSize = 14f;

        protected void LateUpdate() {
            var horizontalScrollbar = scrollRect.horizontalScrollbar;
            var verticalScrollBar = scrollRect.verticalScrollbar;
            var scrollRectTransform = scrollRect.transform as RectTransform;
            
            if (verticalScrollBar) {
                var totalSliderAreaHeight = (verticalScrollBar.handleRect.parent as RectTransform).rect.height;
                var minSize = _minSize / totalSliderAreaHeight;
                if (verticalScrollBar.size < minSize)
                    verticalScrollBar.size = minSize;
            }

            if (horizontalScrollbar) {
                var totalSliderAreaWidth = (horizontalScrollbar.handleRect.parent as RectTransform).rect.width;
                var minSize = _minSize / totalSliderAreaWidth;
                if (horizontalScrollbar.size < minSize)
                    horizontalScrollbar.size = minSize;
            }
        }
    }
}
