using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components.Base
{
    internal class MiniWindow : Module, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected bool m_Resizable = false;
        [SerializeField] protected float m_MinimizedHeight = 23;
        [SerializeField] protected float m_MinContentHeight = 50;
        [SerializeField] protected float m_MaxContentHeight = -1;
        [SerializeField] protected float m_PreferredContentHeight = 50;
        [SerializeField] protected RectTransform m_PanelContent = null;
        [SerializeField] protected TMPro.TextMeshProUGUI m_TitleLabel = null;
        [SerializeField] protected Button m_CloseButton = null;
        [SerializeField] protected Button m_MinimizeButton = null;
        [SerializeField] protected Button m_MaximizeButton = null;

        internal RectTransform panelContent {
            get => m_PanelContent;
        }

        protected bool m_MouseCursorOverRenderer = false;
        protected bool m_Minimized = false;
        protected RectTransform m_DraggedFromParent = null;
        protected RectTransform m_DraggedToParent = null;
        protected GameObject m_ShadowVariantGameObject = null;

        private float m_CachedHeight = 0;

        private bool m_ResizeStarted = false;
        private bool m_ResizeFailure = false;

        private Draggable m_DraggableComponent;
        protected Draggable draggableComponent {
            get {
                if (!m_DraggableComponent)
                    m_DraggableComponent = GetComponent<Draggable>();
                return m_DraggableComponent;
            }
        }

        private MiniWindowContainer m_ParentContainer = null;
        internal MiniWindowContainer parentContainer {
            get => m_ParentContainer;
        }

        private LayoutElement m_LayoutElement;
        protected LayoutElement layoutElement {
            get {
                if (!m_LayoutElement)
                    m_LayoutElement = GetComponent<LayoutElement>();
                return m_LayoutElement;
            }
        }

        private VerticalLayoutGroup m_VertialLayoutGroup;
        internal VerticalLayoutGroup verticalLayoutGroup {
            get {
                if (!m_VertialLayoutGroup)
                    m_VertialLayoutGroup = GetComponent<VerticalLayoutGroup>();
                return m_VertialLayoutGroup;
            }
        }

        internal bool Resizable { get => m_Resizable; }
        internal float MinContentHeight { get => m_MinContentHeight; }
        internal float MaxContentHeight { get => m_MaxContentHeight; }

        internal float MinHeight {
            get {
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + m_MinContentHeight;
            }
        }

        internal float MaxHeight {
            get {
                if (m_MaxContentHeight == -1)
                    return -1;
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + m_MaxContentHeight;
            }
        }

        internal float PreferredHeight {
            get {
                return verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom
                    + verticalLayoutGroup.spacing + m_PreferredContentHeight;
            }
        }

        protected virtual new void Start() {
            base.Start();

            var parentContainerRectTransform = transform.parent?.parent;
            m_ParentContainer = parentContainerRectTransform?.GetComponent<MiniWindowContainer>();
            if (m_ParentContainer)
                m_ParentContainer.RegisterMiniWindow(this);

            if (draggableComponent) {
                // we can't proceed with dragging if parent container doesn't exist ..
                if (m_ParentContainer) {
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

            m_CloseButton?.onClick?.AddListener(() => ExitMiniWindow());
            m_MinimizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
            m_MaximizeButton?.onClick?.AddListener(() => ToggleMinimizedMaximized());
        }
        
        public void OnPointerEnter(PointerEventData _) => m_MouseCursorOverRenderer = true;
        public void OnPointerExit(PointerEventData _) => m_MouseCursorOverRenderer = false;

        protected virtual void ExitMiniWindow() {
            Destroy(gameObject);
        }

        private void ToggleMinimizedMaximized() {
            if (m_Minimized) {
                m_MinimizeButton.gameObject.SetActive(true);
                m_MaximizeButton.gameObject.SetActive(false);
                m_PanelContent?.gameObject?.SetActive(true);

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, m_CachedHeight);
            } else {
                m_MinimizeButton.gameObject.SetActive(false);
                m_MaximizeButton.gameObject.SetActive(true);
                m_PanelContent?.gameObject?.SetActive(false);

                m_CachedHeight = rectTransform.rect.height;
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, m_MinimizedHeight);
            }
            
            m_Minimized = !m_Minimized;
        }

        protected virtual void OnBeginDrag(PointerEventData eventData) {
            m_DraggedFromParent = parentRectTransform;
            
            m_ShadowVariantGameObject = CreateShadowVariant();

            // on tibia 11, we are allowed to drag to another container
            if (OpenTibiaUnity.GameManager.ClientVersion > 1100)
                transform.SetParent(OpenTibiaUnity.GameManager.ActiveCanvas.transform);
            else
                transform.SetParent(m_ParentContainer.tmpContentPanel);
        }

        protected virtual void OnDrag(PointerEventData eventData) {
            int currentIndex = m_ShadowVariantGameObject.transform.GetSiblingIndex();
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
                m_ShadowVariantGameObject.transform.SetSiblingIndex(foundIndex);
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
                m_ShadowVariantGameObject.transform.SetSiblingIndex(foundIndex);
                return;
            }
        }

        protected virtual void OnEndDrag(PointerEventData eventData) {
            if (!m_DraggedToParent) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(m_DraggedFromParent);
                transform.SetParent(m_DraggedFromParent);
            }

            transform.SetSiblingIndex(m_ShadowVariantGameObject.transform.GetSiblingIndex());
            Destroy(m_ShadowVariantGameObject);
        }

        private GameObject CreateShadowVariant() {
            var shadowVariant = Instantiate(OpenTibiaUnity.GameManager.MiniWindowShadowVariant, m_DraggedFromParent);
            shadowVariant.name = name + "_ShadowVariant";
            shadowVariant.transform.SetSiblingIndex(transform.GetSiblingIndex());

            var newRectTransform = shadowVariant.transform as RectTransform;
            newRectTransform.sizeDelta = rectTransform.sizeDelta;
            return shadowVariant;
        }

        protected RectTransform GetCurrentParent() {
            if (m_DraggedToParent)
                return m_DraggedToParent;

            return m_DraggedFromParent;
        }
        
        protected virtual void OnClientVersionChange(int _, int __) {}

        protected void UpdateLayout() {
            var height = Mathf.Clamp(rectTransform.rect.height, m_MinContentHeight, m_MaxContentHeight);

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        }
    }
}