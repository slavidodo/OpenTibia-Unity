using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OpenTibiaUnity.Core.Input;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    internal class PopupWindow : Base.Window, IPointerClickHandler
    {
        [SerializeField] private TMPro.TextMeshProUGUI m_TitleLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_MessagesLabel = null;
        [SerializeField] private RectTransform m_SeparatorPanel = null;
        [SerializeField] private RectTransform m_ButtonsPanel = null;

        [SerializeField] private Button m_OKButton = null;
        [SerializeField] private Button m_CancelButton = null;

        [SerializeField] private bool m_SizeCheckRequired = false;

        private Vector2Int m_RefMaximumSize = Vector2Int.zero;

        private PopupMenuType m_PopupMenuType = PopupMenuType.OKCancel;
        internal PopupMenuType PopupType {
            set {
                if (value != m_PopupMenuType) {
                    m_OKButton.gameObject.SetActive((value & PopupMenuType.OK) != 0);
                    m_CancelButton.gameObject.SetActive((value & PopupMenuType.Cancel) != 0);
                    m_ButtonsPanel.gameObject.SetActive(value != PopupMenuType.NoButtons);
                    m_PopupMenuType = value;

                    if (value == PopupMenuType.NoButtons) {
                        m_SeparatorPanel.gameObject.SetActive(false);
                        m_ButtonsPanel.gameObject.SetActive(false);
                    } else {
                        m_SeparatorPanel.gameObject.SetActive(true);
                        m_ButtonsPanel.gameObject.SetActive(true);
                    }
                }
            }
        }

        internal Button.ButtonClickedEvent onOKClick { get; } = new Button.ButtonClickedEvent();
        internal Button.ButtonClickedEvent onCancelClick { get; } = new Button.ButtonClickedEvent();

        protected override void Start() {
            base.Start();
            m_OKButton.onClick.AddListener(TriggerOk);
            m_CancelButton.onClick.AddListener(TriggerCancel);

            OpenTibiaUnity.InputHandler.AddKeyUpListener(Utility.EventImplPriority.Default, (Event e, bool repeat) => {
                if (!InputHandler.IsHighlighted(this))
                    return;
                
                switch (e.keyCode) {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        e.Use();
                        TriggerHideWindow(true);
                        break;

                    case KeyCode.Escape:
                        e.Use();
                        TriggerHideWindow(false);
                        break;
                }
            });
        }

        protected new void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            if (m_SizeCheckRequired) {
                int maxWidth = m_RefMaximumSize.x;
                int maxHeight = m_RefMaximumSize.y;

                var layoutElement = m_MessagesLabel.GetComponent<LayoutElement>();
                if (maxWidth > 0 && rectTransform.sizeDelta.x > maxWidth) {
                    layoutElement.preferredWidth = maxWidth;
                } else {
                    layoutElement.preferredWidth = -1;
                }

                if (maxHeight > 0 && rectTransform.sizeDelta.y > maxHeight) {
                    layoutElement.preferredHeight = maxHeight;
                } else {
                    layoutElement.preferredHeight = -1;
                }
            }
        }

        internal void SetTitle(string title) {
            m_TitleLabel.SetText(title);
        }

        internal void SetMessage(string message, int maxWidth = -1, int maxHeight = -1) {
            m_RefMaximumSize = new Vector2Int(maxWidth, maxHeight);
            m_SizeCheckRequired = true;

            var layoutElement = m_MessagesLabel.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = -1;
            layoutElement.preferredHeight = -1;

            m_MessagesLabel.SetText(message);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        internal void SetMessageAlignment(TMPro.TextAlignmentOptions alignment) {
            m_MessagesLabel.alignment = alignment;
        }

        protected void TriggerHideWindow(bool enter) {
            HideWindow();

            if (enter && (m_PopupMenuType & PopupMenuType.OK) != 0)
                TriggerOk();
            else if (!enter && (m_PopupMenuType & PopupMenuType.Cancel) != 0)
                TriggerCancel();
        }
        
        public void OnPointerClick(PointerEventData eventData) {
            Select();
        }

        internal void TriggerOk() {
            if (LockedToOverlay)
                CloseWindow();
            else
                HideWindow();

            onOKClick.Invoke();
        }

        internal void TriggerCancel() {
            if (LockedToOverlay)
                CloseWindow();
            else
                HideWindow();

            onCancelClick.Invoke();
        }
    }
}
