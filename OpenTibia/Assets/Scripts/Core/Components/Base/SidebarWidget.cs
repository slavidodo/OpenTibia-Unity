using UnityEngine;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class SidebarWidget : Core.Components.Base.Module, IPointerEnterHandler, IPointerExitHandler
    {
        // serialized fields
        [SerializeField]
        protected bool _closable = false;
        [SerializeField]
        protected bool _resizable = false;
        [SerializeField]
        protected float _minimizedHeight = 23;
        [SerializeField]
        protected float _minContentHeight = 50;
        [SerializeField]
        protected float _maxContentHeight = -1;
        [SerializeField]
        protected float _preferredContentHeight = 50;
        [SerializeField]
        protected RectTransform _content = null;
        [SerializeField]
        protected TMPro.TextMeshProUGUI _title = null;
        [SerializeField]
        protected Button _closeButton = null;
        [SerializeField]
        protected Button _minimizeButton = null;
        [SerializeField]
        protected Button _maximizeButton = null;

        // fields
        protected bool _mouseCursorOverRenderer = false;
        protected bool _minimized = false;
        protected RectTransform _draggedFromParent = null;
        protected RectTransform _draggedToParent = null;
        protected GameObject _shadowVariantGameObject = null;
        private float _cachedHeight = 0;

        // properties
        public SidebarWidgetContainer parentContainer { get; private set; } = null;
        public RectTransform content { get => _content; }
        public bool Closable { get => _closable; }
        public bool Resizable { get => _resizable; }
        public bool IsMouseOverRenderer { get => _mouseCursorOverRenderer; }
        public float MinContentHeight { get => _minContentHeight; }
        public float MaxContentHeight { get => _maxContentHeight; }

        private Core.Components.Draggable _draggableComponent;
        protected Core.Components.Draggable draggableComponent {
            get {
                if (!_draggableComponent)
                    _draggableComponent = GetComponent<Core.Components.Draggable>();
                return _draggableComponent;
            }
        }

        private UnityUI.LayoutElement _layoutElement;
        protected UnityUI.LayoutElement layoutElement {
            get {
                if (!_layoutElement)
                    _layoutElement = GetComponent<UnityUI.LayoutElement>();
                return _layoutElement;
            }
        }

        private UnityUI.VerticalLayoutGroup _vertialLayoutGroup = null;
        public UnityUI.VerticalLayoutGroup verticalLayoutGroup {
            get {
                if (!_vertialLayoutGroup)
                    _vertialLayoutGroup = GetComponent<UnityUI.VerticalLayoutGroup>();
                return _vertialLayoutGroup;
            }
        }

        public float MinHeight {
            get {
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + _minContentHeight + 15;
            }
        }

        public float MaxHeight {
            get {
                if (_maxContentHeight == -1)
                    return -1;
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + _maxContentHeight + 15;
            }
        }

        public float PreferredHeight {
            get {
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + _preferredContentHeight + 15;
            }
        }

        protected override void Start() {
            base.Start();

            var parentContainerRectTransform = transform.parent?.parent;
            parentContainer = parentContainerRectTransform?.GetComponent<SidebarWidgetContainer>();
            if (parentContainer)
                parentContainer.RegisterSidebarWidget(this);

            if (draggableComponent) {
                // we can't proceed with dragging if parent container doesn't exist ..
                if (parentContainer) {
                    draggableComponent.onBeginDrag.AddListener(OnBeginDrag);
                    draggableComponent.onDrag.AddListener(OnDrag);
                    draggableComponent.onEndDrag.AddListener(OnEndDrag);

                    draggableComponent.bindRectToParent = true;
                } else {
                    Destroy(draggableComponent);
                    _draggableComponent = null;
                }
            }

            _closeButton?.onClick?.AddListener(() => Close());
            _minimizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
            _maximizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
        }
        
        public void OnPointerEnter(PointerEventData _) => _mouseCursorOverRenderer = true;
        public void OnPointerExit(PointerEventData _) => _mouseCursorOverRenderer = false;

        public virtual void Close() {
            Destroy(gameObject);
        }

        private void ToggleMinimizedMaximized() {
            if (_minimized) {
                _minimizeButton.gameObject.SetActive(true);
                _maximizeButton.gameObject.SetActive(false);
                _content?.gameObject?.SetActive(true);

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _cachedHeight);
            } else {
                _minimizeButton.gameObject.SetActive(false);
                _maximizeButton.gameObject.SetActive(true);
                _content?.gameObject?.SetActive(false);

                _cachedHeight = rectTransform.rect.height;
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _minimizedHeight);
            }
            
            _minimized = !_minimized;
        }

        protected virtual void OnBeginDrag(PointerEventData eventData) {
            _draggedFromParent = parentRectTransform;
            
            _shadowVariantGameObject = CreateShadowVariant();

            // on tibia 11, we are allowed to drag to another container
            if (OpenTibiaUnity.GameManager.ClientVersion > 1100)
                transform.SetParent(OpenTibiaUnity.GameManager.ActiveCanvas.transform);
            else
                transform.SetParent(parentContainer.tempContent);
        }

        protected virtual void OnDrag(PointerEventData eventData) {
            int currentIndex = _shadowVariantGameObject.transform.GetSiblingIndex();
            Vector3 localPosition = transform.localPosition;

            var parent = GetCurrentParent();
            int childCount = parent.childCount;

            var foundIndex = -1;

            // check for next elements (starting from the end)
            for (int i = childCount - 1; i > currentIndex; i--) {
                var child = parent.GetChild(i) as RectTransform;
                if (localPosition.y < child.localPosition.y + child.rect.size.y / 3f) {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1 && foundIndex != currentIndex) {
                _shadowVariantGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }

            // check for previous elements (starting from the begin)
            for (int i = 0; i < currentIndex; i++) {
                var child = parent.GetChild(i) as RectTransform;
                if (localPosition.y > child.localPosition.y - child.rect.size.y / 1.5f) {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1 && foundIndex != currentIndex) {
                _shadowVariantGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }
        }

        protected virtual void OnEndDrag(PointerEventData eventData) {
            if (!_draggedToParent) {
                UnityUI.LayoutRebuilder.ForceRebuildLayoutImmediate(_draggedFromParent);
                transform.SetParent(_draggedFromParent);
            }

            transform.SetSiblingIndex(_shadowVariantGameObject.transform.GetSiblingIndex());
            Destroy(_shadowVariantGameObject);
        }

        private GameObject CreateShadowVariant() {
            var shadowVariant = Instantiate(OpenTibiaUnity.GameManager.SidebarWidgetShadowVariant, _draggedFromParent);
            shadowVariant.name = name + "_ShadowVariant";
            shadowVariant.transform.SetSiblingIndex(transform.GetSiblingIndex());

            var newRectTransform = shadowVariant.GetComponent<RectTransform>();
            newRectTransform.sizeDelta = rectTransform.sizeDelta;
            return shadowVariant;
        }

        protected RectTransform GetCurrentParent() {
            if (_draggedToParent)
                return _draggedToParent;

            return _draggedFromParent;
        }

        protected void UpdateLayout() {
            var height = Mathf.Clamp(PreferredHeight, MinHeight, MaxHeight);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }
}