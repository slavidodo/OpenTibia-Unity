using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    internal class GameInterface : Core.Components.Base.Module
    {
        [SerializeField] internal GamePanelContainer GameRightContainer = null;
        [SerializeField] internal GamePanelContainer GameLeftContainer = null;
        [SerializeField] internal GamePanelContainer GameBottomContainer = null;
        [SerializeField] internal GamePanelContainer GameMapContainer = null;

        [SerializeField] private Camera m_Camera = null;

        private Canvas m_GameCanvas;
        internal Canvas gameCanvas {
            get {
                if (!m_GameCanvas)
                    m_GameCanvas = GetComponent<Canvas>();
                return m_GameCanvas;
            }
        }

        private bool m_LayoutIsUpdating = false;

        protected override void Start() {
            base.Start();
            UpdateLayout();
            ScaleToScreen();
        }
        
        internal void UpdateLayout() {
            if (!GameRightContainer || !GameLeftContainer || !GameBottomContainer || !GameMapContainer || m_LayoutIsUpdating)
                return;

            m_LayoutIsUpdating = true;
            var rightTransform = GameRightContainer.rectTransform;
            var leftTransform = GameLeftContainer.rectTransform;
            var bottomTransform = GameBottomContainer.rectTransform;
            var mapTransform = GameMapContainer.rectTransform;

            float leftWidth = leftTransform.rect.width;
            float rightWidth = rightTransform.rect.width;
            float bottomHeight = bottomTransform.rect.height;

            mapTransform.offsetMin = new Vector2(leftWidth, bottomHeight);
            mapTransform.offsetMax = new Vector2(-rightWidth, 0);

            bottomTransform.offsetMin = new Vector2(leftWidth, 0);
            bottomTransform.offsetMax = new Vector2(-rightWidth, bottomHeight);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(bottomTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(mapTransform);
            m_LayoutIsUpdating = false;
        }

        internal void ScaleToScreen() {
            float distance = Vector3.Distance(m_Camera.transform.position, transform.position);
            float camHeight;

            if (m_Camera.orthographic)
                camHeight = m_Camera.orthographicSize * 2;
            else
                camHeight = 2.0f * distance * Mathf.Tan(Mathf.Deg2Rad * m_Camera.fieldOfView * 0.5f);
            
            float scale = (camHeight / Screen.height);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        internal T CreateMiniWindow<T>(T prefab) where T : Core.Components.Base.MiniWindow {
            return AddMiniWindow(Instantiate(prefab));
        }

        internal T AddMiniWindow<T>(T miniWindow) where T : Core.Components.Base.MiniWindow {
            Core.Components.Base.MiniWindowContainer suggestedContainer = null;

            float requiredMinHeight = miniWindow.MinHeight;
            float requiredPreferredHeight = miniWindow.PreferredHeight;

            int rightChildCount = GameRightContainer.rectTransform.childCount;
            int leftChildCount = GameLeftContainer.rectTransform.childCount;
            
            for (int i = 0; i < rightChildCount + leftChildCount; i++) {
                Transform child;
                if (i < rightChildCount)
                    child = GameRightContainer.rectTransform.GetChild(i);
                else
                    child = GameLeftContainer.rectTransform.GetChild(i);

                var miniWindowContainer = child.GetComponent<Core.Components.Base.MiniWindowContainer>();
                if (!miniWindowContainer) {
                    continue;
                }

                var remainingHeight = miniWindowContainer.GetRemainingHeight();
                if (remainingHeight >= requiredPreferredHeight)
                    return miniWindowContainer.AddMiniWindow(miniWindow);
                else if (remainingHeight >= requiredMinHeight && suggestedContainer == null)
                    suggestedContainer = miniWindowContainer;
            }

            if (suggestedContainer) // it satisfies the minimum height only, but not the preferred one
                return suggestedContainer.AddMiniWindow(miniWindow);

            // worst case, no empty space found
            // on the last opened container, keep resizing or keep closing
            var lastMiniWindowContainer = OpenTibiaUnity.GameManager.GetModule<Core.Components.Base.MiniWindowContainer>();
            float truncatedHeight = lastMiniWindowContainer.GetRemainingHeight();
            
            foreach (RectTransform child in lastMiniWindowContainer.contentPanel) {
                var childMiniWindow = child.GetComponent<Core.Components.Base.MiniWindow>();
                var height = child.rect.height;
                var availableHeight = height - childMiniWindow.MinHeight;
                if (truncatedHeight + availableHeight < miniWindow.MinHeight) {
                    truncatedHeight += availableHeight;
                    Destroy(miniWindow.gameObject);
                    continue;
                }

                float deltaHeight = requiredMinHeight - truncatedHeight;
                child.sizeDelta = new Vector2(child.sizeDelta.x, child.sizeDelta.y - deltaHeight);
                break;
            }
            
            return lastMiniWindowContainer.AddMiniWindow(miniWindow);
        }
    }
}
