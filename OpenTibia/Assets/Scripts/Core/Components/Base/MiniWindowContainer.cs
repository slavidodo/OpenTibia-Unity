using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Components.Base
{
    internal class MiniWindowContainer : Module
    {
        [SerializeField] internal RectTransform contentPanel = null;
        [SerializeField] internal RectTransform tmpContentPanel = null;

        private List<MiniWindow> m_MiniWindows;

        protected override void Awake() {
            base.Awake();

            m_MiniWindows = new List<MiniWindow>();
        }


        internal void RegisterMiniWindow(MiniWindow miniWindow) {
            if (m_MiniWindows.Contains(miniWindow))
                return;

            m_MiniWindows.Add(miniWindow);
        }

        internal void UnregisterMiniWindow(MiniWindow miniWindow) {
            if (!m_MiniWindows.Contains(miniWindow))
                return;

            m_MiniWindows.Remove(miniWindow);
        }
        
        internal T AddMiniWindow<T>(T miniWindow) where T : MiniWindow {
            miniWindow.rectTransform.SetParent(contentPanel);
            var remainingHeight = GetRemainingHeight(miniWindow);
            var sizeDelta = miniWindow.rectTransform.sizeDelta;
            miniWindow.rectTransform.sizeDelta = new Vector2(sizeDelta.x, Mathf.Min(sizeDelta.y, remainingHeight));
            return miniWindow;
        }

        internal float GetRemainingHeight(MiniWindow exclude = null) {
            float totalHeight = 0;
            foreach (RectTransform child in contentPanel) {
                if (exclude == null || child.gameObject != exclude.gameObject)
                    totalHeight += child.rect.size.y;
            }
            
            return contentPanel.rect.size.y - totalHeight;
        }
    }
}
