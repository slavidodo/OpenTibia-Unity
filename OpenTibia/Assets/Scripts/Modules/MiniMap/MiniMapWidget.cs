using UnityEngine;

namespace OpenTibiaUnity.Modules.MiniMap
{
    public class MiniMapWidget : UI.Legacy.SidebarWidget
    {
        [SerializeField]
        private UI.Legacy.Button _zLayerUpButton = null;
        [SerializeField]
        private UI.Legacy.Button _zLayerDownButton = null;
        [SerializeField]
        private UI.Legacy.Button _zoomOutButton = null;
        [SerializeField]
        private UI.Legacy.Button _zoomInButton = null;
        [SerializeField]
        private UI.Legacy.Button _centerButton = null;

        protected override void Awake() {
            base.Awake();

            _zLayerUpButton.onClick.AddListener(OnZLayerUpButtonClicked);
            _zLayerDownButton.onClick.AddListener(OnZLayerDownButtonClicked);
            _zoomOutButton.onClick.AddListener(OnZoomOutButtonClicked);
            _zoomInButton.onClick.AddListener(OnZoomInButtonClicked);
            _centerButton.onClick.AddListener(OnCenterButtonClicked);
        }

        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            var gameManager = OpenTibiaUnity.GameManager;
            if (gameManager == null || gameManager.MiniMapRenderer == null || gameManager.MiniMapRenderingTexture == null)
                return;

            gameManager.MiniMapRenderer.Render(gameManager.MiniMapRenderingTexture);
        }

        private void OnZLayerUpButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ++;
        }
        private void OnZLayerDownButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.PositionZ--;
        }
        private void OnZoomOutButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom--;
            OpenTibiaUnity.OptionStorage.MiniMapZoom = OpenTibiaUnity.MiniMapRenderer.Zoom;
        }
        private void OnZoomInButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Zoom++;
            OpenTibiaUnity.OptionStorage.MiniMapZoom = OpenTibiaUnity.MiniMapRenderer.Zoom;
        }
        private void OnCenterButtonClicked() {
            OpenTibiaUnity.MiniMapRenderer.Position = OpenTibiaUnity.MiniMapStorage.Position;
        }
    }
}
