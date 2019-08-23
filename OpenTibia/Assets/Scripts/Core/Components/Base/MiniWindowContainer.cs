using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Components.Base
{
    public class MiniWindowContainer : Module
    {
        public RectTransform contentPanel = null;
        public RectTransform tmpContentPanel = null;

        private List<MiniWindow> _miniWindows;

        protected override void Awake() {
            base.Awake();

            _miniWindows = new List<MiniWindow>();
        }


        public void RegisterMiniWindow(MiniWindow miniWindow) {
            if (_miniWindows.Contains(miniWindow))
                return;

            _miniWindows.Add(miniWindow);
        }

        public void UnregisterMiniWindow(MiniWindow miniWindow) {
            if (!_miniWindows.Contains(miniWindow))
                return;

            _miniWindows.Remove(miniWindow);
        }
        
        public T AddMiniWindow<T>(T miniWindow) where T : MiniWindow {
            miniWindow.rectTransform.SetParent(contentPanel);
            var remainingHeight = GetRemainingHeight(miniWindow);
            var sizeDelta = miniWindow.rectTransform.sizeDelta;
            miniWindow.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Min(sizeDelta.y, remainingHeight));
            return miniWindow;
        }

        public float GetRemainingHeight(MiniWindow exclude = null) {
            float totalHeight = 0;
            foreach (RectTransform child in contentPanel) {
                if (exclude == null || child.gameObject != exclude.gameObject)
                    totalHeight += child.rect.size.y;
            }
            
            return contentPanel.rect.size.y - totalHeight;
        }
    }
}
