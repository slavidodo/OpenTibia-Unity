using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class GameWindowLayout : Core.Components.Base.AbstractComponent
    {
#pragma warning disable CS0649 // never assigned to
        public GamePanelContainer GameRightContainer;
        public GamePanelContainer GameLeftContainer;
        public GamePanelContainer GameBottomContainer;
        public GamePanelContainer GameMapContainer;

        [SerializeField] private Camera m_Camera;
#pragma warning restore CS0649 // never assigned to

        private Canvas m_GameCanvas;
        public Canvas gameCanvas {
            get {
                if (!m_GameCanvas)
                    m_GameCanvas = GetComponent<Canvas>();
                return m_GameCanvas;
            }
        }

        private bool m_LayoutIsUpdating = false;
        private float m_ScreenHeight = 0;

        protected override void Start() {
            base.Start();
            UpdateLayout();
            ScaleToScreen();
        }

        protected void Update() {
        }

        public void UpdateLayout() {
            if (!GameRightContainer || !GameLeftContainer || !GameBottomContainer || !GameMapContainer || m_LayoutIsUpdating)
                return;

            m_LayoutIsUpdating = true;
            RectTransform rightTransform = GameRightContainer.transform as RectTransform;
            RectTransform leftTransform = GameLeftContainer.transform as RectTransform;
            RectTransform bottomTransform = GameBottomContainer.transform as RectTransform;
            RectTransform mapTransform = GameMapContainer.transform as RectTransform;

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

        public void ScaleToScreen() {
            float distance = Vector3.Distance(m_Camera.transform.position, transform.position);
            float camHeight;

            if (m_Camera.orthographic)
                camHeight = m_Camera.orthographicSize * 2;
            else
                camHeight = 2.0f * distance * Mathf.Tan(Mathf.Deg2Rad * m_Camera.fieldOfView * 0.5f);
            
            float scale = (camHeight / Screen.height);
            transform.localScale = new Vector3(scale, scale, scale);
            
            m_ScreenHeight = Screen.height;
        }
    }
}
