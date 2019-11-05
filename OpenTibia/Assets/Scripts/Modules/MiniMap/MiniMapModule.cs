using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Modules.MiniMap
{
    public class MiniMapModule : Core.Components.Base.AbstractComponent
    {
        [SerializeField] private RawImage _miniMapRenderingRawImage = null;

        [SerializeField] private Button _zLayerUpButton = null;
        [SerializeField] private Button _zLayerDownButton = null;
        [SerializeField] private Button _zoomOutButton = null;
        [SerializeField] private Button _zoomInButton = null;
        [SerializeField] private Button _benterButton = null;

        private RectTransform _miniMapRenderingRectTransform;

        protected override void Start() {
            base.Start();
            _miniMapRenderingRectTransform = _miniMapRenderingRawImage.rectTransform;

            _zLayerUpButton.onClick.AddListener(OnZLayerUpButtonClicked);
            _zLayerDownButton.onClick.AddListener(OnZLayerDownButtonClicked);
            _zoomOutButton.onClick.AddListener(OnZoomOutButtonClicked);
            _zoomInButton.onClick.AddListener(OnZoomInButtonClicked);
            _benterButton.onClick.AddListener(OnCenterButtonClicked);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            OpenTibiaUnity.MiniMapRenderer.Render(OpenTibiaUnity.GameManager.MiniMapRenderingTexture);
        }

        protected void OnZLayerUpButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ++;
        }
        protected void OnZLayerDownButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ--;
        }
        protected void OnZoomOutButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom--;
            OpenTibiaUnity.OptionStorage.MiniMapZoom = OpenTibiaUnity.MiniMapRenderer.Zoom;
        }
        protected void OnZoomInButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom++;
            OpenTibiaUnity.OptionStorage.MiniMapZoom = OpenTibiaUnity.MiniMapRenderer.Zoom;
        }
        protected void OnCenterButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Position = OpenTibiaUnity.MiniMapStorage.Position;
        }

        protected void OnMouseDrag() {
        }
    }
}
