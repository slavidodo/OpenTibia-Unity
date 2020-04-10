using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.UI.Legacy
{
    public class SidebarWidgetContainer : Core.Components.Base.Module
    {
        // serialized fields
        [SerializeField]
        protected RectTransform _content = null;
        [SerializeField]
        protected RectTransform _tempContent = null;

        private List<SidebarWidget> _sidebarWidgets;

        // properties
        public RectTransform content { get => _content; }
        public RectTransform tempContent { get => _tempContent; }

        protected override void Awake() {
            base.Awake();

            _sidebarWidgets = new List<SidebarWidget>();
        }


        public void RegisterSidebarWidget(SidebarWidget widget) {
            if (_sidebarWidgets.Contains(widget))
                return;

            _sidebarWidgets.Add(widget);
        }

        public void UnregisterSidebarWidget(SidebarWidget widget) {
            if (!_sidebarWidgets.Contains(widget))
                return;

            _sidebarWidgets.Remove(widget);
        }
        
        public T AddSidebarWidget<T>(T widget) where T : SidebarWidget {
            widget.rectTransform.SetParent(_content);
            var remainingHeight = GetRemainingHeight(widget);
            var sizeDelta = widget.rectTransform.sizeDelta;
            widget.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Min(sizeDelta.y, remainingHeight));
            return widget;
        }

        public float GetRemainingHeight(SidebarWidget exclude = null) {
            float totalHeight = 0;
            foreach (RectTransform child in _content) {
                if (exclude == null || child.gameObject != exclude.gameObject)
                    totalHeight += child.rect.size.y;
            }
            
            return _content.rect.size.y - totalHeight;
        }
    }
}
