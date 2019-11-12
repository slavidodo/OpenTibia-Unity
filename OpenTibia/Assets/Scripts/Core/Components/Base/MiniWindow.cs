using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components.Base
{
    public class MiniWindow : Module, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected bool _resizable = false;
        [SerializeField] protected float _minimizedHeight = 23;
        [SerializeField] protected float _minContentHeight = 50;
        [SerializeField] protected float _maxContentHeight = -1;
        [SerializeField] protected float _preferredContentHeight = 50;
        [SerializeField] protected RectTransform _panelContent = null;
        [SerializeField] protected TMPro.TextMeshProUGUI _titleLabel = null;
        [SerializeField] protected Button _closeButton = null;
        [SerializeField] protected Button _minimizeButton = null;
        [SerializeField] protected Button _maximizeButton = null;

        public RectTransform panelContent {
            get => _panelContent;
        }

        protected bool _mouseCursorOverRenderer = false;
        protected bool _minimized = false;
        protected RectTransform _draggedFromParent = null;
        protected RectTransform _draggedToParent = null;
        protected GameObject _shadowVariantGameObject = null;

        private float _cachedHeight = 0;
        
        private Draggable _draggableComponent;
        protected Draggable draggableComponent {
            get {
                if (!_draggableComponent)
                    _draggableComponent = GetComponent<Draggable>();
                return _draggableComponent;
            }
        }

        private MiniWindowContainer _parentContainer = null;
        public MiniWindowContainer parentContainer {
            get => _parentContainer;
        }

        private LayoutElement _layoutElement;
        protected LayoutElement layoutElement {
            get {
                if (!_layoutElement)
                    _layoutElement = GetComponent<LayoutElement>();
                return _layoutElement;
            }
        }

        private VerticalLayoutGroup _vertialLayoutGroup;
        public VerticalLayoutGroup verticalLayoutGroup {
            get {
                if (!_vertialLayoutGroup)
                    _vertialLayoutGroup = GetComponent<VerticalLayoutGroup>();
                return _vertialLayoutGroup;
            }
        }

        public bool Resizable { get => _resizable; }
        public float MinContentHeight { get => _minContentHeight; }
        public float MaxContentHeight { get => _maxContentHeight; }

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
            _parentContainer = parentContainerRectTransform?.GetComponent<MiniWindowContainer>();
            if (_parentContainer)
                _parentContainer.RegisterMiniWindow(this);

            if (draggableComponent) {
                // we can't proceed with dragging if parent container doesn't exist ..
                if (_parentContainer) {
                    draggableComponent.onBeginDrag.AddListener(OnBeginDrag);
                    draggableComponent.onDrag.AddListener(OnDrag);
                    draggableComponent.onEndDrag.AddListener(OnEndDrag);

                    draggableComponent.bindRectToParent = true;
                } else {
                    Destroy(draggableComponent);
                }
            }

            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);

            _closeButton?.onClick?.AddListener(() => Close());
            _minimizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
            _maximizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
        }
        
        public void OnPointerEnter(PointerEventData _) => _mouseCursorOverRenderer = true;
        public void OnPointerExit(PointerEventData _) => _mouseCursorOverRenderer = false;

        public override void Close() {
            base.Close();

            Destroy(gameObject);
        }

        private void ToggleMinimizedMaximized() {
            if (_minimized) {
                _minimizeButton.gameObject.SetActive(true);
                _maximizeButton.gameObject.SetActive(false);
                _panelContent?.gameObject?.SetActive(true);

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, _cachedHeight);
            } else {
                _minimizeButton.gameObject.SetActive(false);
                _maximizeButton.gameObject.SetActive(true);
                _panelContent?.gameObject?.SetActive(false);

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
                transform.SetParent(_parentContainer.tmpContentPanel);
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
                LayoutRebuilder.ForceRebuildLayoutImmediate(_draggedFromParent);
                transform.SetParent(_draggedFromParent);
            }

            transform.SetSiblingIndex(_shadowVariantGameObject.transform.GetSiblingIndex());
            Destroy(_shadowVariantGameObject);
        }

        private GameObject CreateShadowVariant() {
            var shadowVariant = Instantiate(OpenTibiaUnity.GameManager.MiniWindowShadowVariant, _draggedFromParent);
            shadowVariant.name = name + "_ShadowVariant";
            shadowVariant.transform.SetSiblingIndex(transform.GetSiblingIndex());

            var newRectTransform = shadowVariant.transform as RectTransform;
            newRectTransform.sizeDelta = rectTransform.sizeDelta;
            return shadowVariant;
        }

        protected RectTransform GetCurrentParent() {
            if (_draggedToParent)
                return _draggedToParent;

            return _draggedFromParent;
        }
        
        protected virtual void OnClientVersionChange(int _, int __) {}

        protected void UpdateLayout() {
            var height = Mathf.Clamp(PreferredHeight, MinHeight, MaxHeight);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }
}