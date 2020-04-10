using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class GameInterface : Core.Components.Base.Module
    {
        [SerializeField]
        private GamePanelContainer _gameRightContainer = null;
        [SerializeField]
        private GamePanelContainer _gameLeftContainer = null;
        [SerializeField]
        private GamePanelContainer _gameBottomContainer = null;
        [SerializeField]
        private GamePanelContainer _gameMapContainer = null;
        [SerializeField]
        private int _mapPadding = 0;
        [SerializeField]
        private Camera _camera = null;

        // fields
        private bool _updatingLayout = false;
        
        // properties
        private Canvas _gameCanvas;
        public Canvas gameCanvas {
            get {
                if (!_gameCanvas)
                    _gameCanvas = GetComponent<Canvas>();
                return _gameCanvas;
            }
        }

        protected override void Start() {
            base.Start();
            UpdateLayout();
            ScaleToScreen();
        }
        
        public void UpdateLayout(GamePanelContainer except = null) {
            if (!_gameRightContainer || !_gameLeftContainer || !_gameBottomContainer || !_gameMapContainer || _updatingLayout)
                return;

            _updatingLayout = true;
            var rightTransform = _gameRightContainer.rectTransform;
            var leftTransform = _gameLeftContainer.rectTransform;
            var bottomTransform = _gameBottomContainer.rectTransform;
            var mapTransform = _gameMapContainer.rectTransform;

            float leftWidth = leftTransform.rect.width;
            float rightWidth = rightTransform.rect.width;
            float bottomHeight = bottomTransform.rect.height;

            if (except != _gameMapContainer) {
                mapTransform.offsetMin = new Vector2(leftWidth + _mapPadding, bottomHeight + _mapPadding);
                mapTransform.offsetMax = new Vector2(-rightWidth - _mapPadding, -_mapPadding);
                UnityUI.LayoutRebuilder.ForceRebuildLayoutImmediate(mapTransform);
            }

            if (except != _gameBottomContainer) {
                bottomTransform.offsetMin = new Vector2(leftWidth, 0);
                bottomTransform.offsetMax = new Vector2(-rightWidth, bottomHeight);
                UnityUI.LayoutRebuilder.ForceRebuildLayoutImmediate(bottomTransform);
            }

            _updatingLayout = false;
        }

        public void ScaleToScreen() {
            if (!_camera)
                return;

            float distance = Vector3.Distance(_camera.transform.position, transform.position);
            float camHeight;

            if (_camera.orthographic)
                camHeight = _camera.orthographicSize * 2;
            else
                camHeight = 2.0f * distance * Mathf.Tan(Mathf.Deg2Rad * _camera.fieldOfView * 0.5f);

            float scale = (camHeight / Screen.height);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public T CreateSidebarWidget<T>(T prefab) where T : UI.Legacy.SidebarWidget {
            return AddSidebarWidget(Instantiate(prefab));
        }

        public T AddSidebarWidget<T>(T widget) where T : UI.Legacy.SidebarWidget {
            UI.Legacy.SidebarWidgetContainer suggestedContainer = null;

            float requiredMinHeight = widget.MinHeight;
            float requiredPreferredHeight = widget.PreferredHeight;

            int rightChildCount = _gameRightContainer.rectTransform.childCount;
            int leftChildCount = _gameLeftContainer.rectTransform.childCount;
            
            for (int i = 0; i < rightChildCount + leftChildCount; i++) {
                Transform child;
                if (i < rightChildCount)
                    child = _gameRightContainer.rectTransform.GetChild(i);
                else
                    child = _gameLeftContainer.rectTransform.GetChild(i);

                var miniWindowContainer = child.GetComponent<UI.Legacy.SidebarWidgetContainer>();
                if (!miniWindowContainer) {
                    continue;
                }

                var remainingHeight = miniWindowContainer.GetRemainingHeight();
                if (remainingHeight >= requiredPreferredHeight)
                    return miniWindowContainer.AddSidebarWidget(widget);
                else if (remainingHeight >= requiredMinHeight && suggestedContainer == null)
                    suggestedContainer = miniWindowContainer;
            }

            if (suggestedContainer) // it satisfies the minimum height only, but not the preferred one
                return suggestedContainer.AddSidebarWidget(widget);

            // worst case, no empty space found
            // on the last opened container, keep resizing or keep closing
            var lastMiniWindowContainer = OpenTibiaUnity.GameManager.GetModule<UI.Legacy.SidebarWidgetContainer>();
            float truncatedHeight = lastMiniWindowContainer.GetRemainingHeight();
            
            foreach (RectTransform child in lastMiniWindowContainer.content) {
                var childMiniWindow = child.GetComponent<UI.Legacy.SidebarWidget>();
                var height = child.rect.height;
                var availableHeight = height - childMiniWindow.MinHeight;
                if (truncatedHeight + availableHeight < widget.MinHeight) {
                    truncatedHeight += availableHeight;
                    Destroy(widget.gameObject);
                    continue;
                }

                float deltaHeight = requiredMinHeight - truncatedHeight;
                child.sizeDelta = new Vector2(child.sizeDelta.x, child.sizeDelta.y - deltaHeight);
                break;
            }
            
            return lastMiniWindowContainer.AddSidebarWidget(widget);
        }
    }
}
