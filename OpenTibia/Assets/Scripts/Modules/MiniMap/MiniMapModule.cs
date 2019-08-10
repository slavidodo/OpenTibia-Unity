using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Modules.MiniMap
{
    public class MiniMapModule : Core.Components.Base.AbstractComponent
    {
        [SerializeField] private RawImage m_MiniMapRenderingRawImage = null;

        [SerializeField] private Button m_ZLayerUpButton = null;
        [SerializeField] private Button m_ZLayerDownButton = null;
        [SerializeField] private Button m_ZoomOutButton = null;
        [SerializeField] private Button m_ZoomInButton = null;
        [SerializeField] private Button m_CenterButton = null;

        private RectTransform m_MiniMapRenderingRectTransform;

        protected override void Start() {
            base.Start();
            m_MiniMapRenderingRectTransform = m_MiniMapRenderingRawImage.rectTransform;

            m_ZLayerUpButton.onClick.AddListener(OnZLayerUpButtonClicked);
            m_ZLayerDownButton.onClick.AddListener(OnZLayerDownButtonClicked);
            m_ZoomOutButton.onClick.AddListener(OnZoomOutButtonClicked);
            m_ZoomInButton.onClick.AddListener(OnZoomInButtonClicked);
            m_CenterButton.onClick.AddListener(OnCenterButtonClicked);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            OpenTibiaUnity.GameManager.MiniMapRenderingTexture.Release();
            RenderTexture.active = OpenTibiaUnity.GameManager.MiniMapRenderingTexture;
            OpenTibiaUnity.MiniMapRenderer.Render(null);
            RenderTexture.active = null;
        }

        protected void OnZLayerUpButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ++;
        }
        protected void OnZLayerDownButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ--;
        }
        protected void OnZoomOutButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom--;
        }
        protected void OnZoomInButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom++;
        }
        protected void OnCenterButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Position = OpenTibiaUnity.MiniMapStorage.Position;
        }

        protected void OnMouseDrag() {
        }
    }
}
