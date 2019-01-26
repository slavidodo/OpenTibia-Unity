using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OpenTibiaUnity.Core.InputManagment;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class PopupWindow : Base.Window, IPointerClickHandler
    {
        private bool m_Selected = true;
        public bool selected {
            get { return m_Selected; }
        }

#pragma warning disable CS0649 // never assigned to
        [SerializeField] private VerticalLayoutGroup m_VerticalLayoutGroup;
        [SerializeField] private TMPro.TextMeshProUGUI m_TitleLabel;
        [SerializeField] private TMPro.TextMeshProUGUI m_MessagesLabel;
        [SerializeField] private RectTransform m_SeparatorPanel;
        [SerializeField] private RectTransform m_ButtonsPanel;

        [SerializeField] private Button m_OKButton;
        [SerializeField] private Button m_CancelButton;

        [SerializeField] private bool m_SizeCheckRequired = false;
#pragma warning restore CS0649 // never assigned to

        private Vector2Int m_RefMaximumSize = Vector2Int.zero;

        private PopupMenuType m_PopupMenuType = PopupMenuType.OKCancel;
        public PopupMenuType PopupType {
            set {
                if (value != m_PopupMenuType) {
                    m_OKButton.gameObject.SetActive((value & PopupMenuType.OK) != 0);
                    m_CancelButton.gameObject.SetActive((value & PopupMenuType.Cancel) != 0);
                    m_ButtonsPanel.gameObject.SetActive(value != PopupMenuType.NoButtons);
                    m_PopupMenuType = value;

                    if (value == PopupMenuType.NoButtons) {
                        m_SeparatorPanel.gameObject.SetActive(false);
                        m_ButtonsPanel.gameObject.SetActive(false);
                        //m_VerticalLayoutGroup.padding.bottom = 20;
                    } else {
                        m_SeparatorPanel.gameObject.SetActive(true);
                        m_ButtonsPanel.gameObject.SetActive(true);
                        //m_VerticalLayoutGroup.padding.bottom = 10;
                    }
                }
            }
        }

        public Button.ButtonClickedEvent OnClickOk { get; } = new Button.ButtonClickedEvent();
        public Button.ButtonClickedEvent OnClickCancel { get; } = new Button.ButtonClickedEvent();

        protected override void Start() {
            base.Start();
            m_OKButton.onClick.AddListener(TriggerOk);
            m_CancelButton.onClick.AddListener(TriggerCancel);

            OpenTibiaUnity.InputHandler.AddKeyUpListener((Event e, bool repeat) => {
                if (!InputHandler.IsHighlighted(this))
                    return;

                switch (e.keyCode) {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        HideWindow(true);
                        break;

                    case KeyCode.Escape:
                        HideWindow(false);
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
                if (maxWidth > 0 && (transform as RectTransform).sizeDelta.x > maxWidth) {
                    layoutElement.preferredWidth = maxWidth;
                } else {
                    layoutElement.preferredWidth = -1;
                }

                if (maxHeight > 0 && (transform as RectTransform).sizeDelta.y > maxHeight) {
                    layoutElement.preferredHeight = maxHeight;
                } else {
                    layoutElement.preferredHeight = -1;
                }
            }
        }

        public void SetTitle(string title) {
            m_TitleLabel.SetText(title);
        }

        public void SetMessage(string message, int maxWidth = -1, int maxHeight = -1) {
            m_RefMaximumSize = new Vector2Int(maxWidth, maxHeight);
            m_SizeCheckRequired = true;

            var layoutElement = m_MessagesLabel.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = -1;
            layoutElement.preferredHeight = -1;

            m_MessagesLabel.SetText(message);
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        public void SetMessageAlignment(TMPro.TextAlignmentOptions alignment) {
            m_MessagesLabel.alignment = alignment;
        }

        public void Show() {
            gameObject.SetActive(true);
            Select();
        }
        public void HideWindow(bool enter) {
            UnlockFromOverlay();

            if (enter && (m_PopupMenuType & PopupMenuType.OK) != 0)
                TriggerOk();
            else if (!enter && (m_PopupMenuType & PopupMenuType.Cancel) != 0)
                TriggerCancel();
            else
                gameObject.SetActive(false);
            
        }

        public void Select() {
            if (EventSystem.current.alreadySelecting)
                return;

            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData) {
            Select();
        }

        public void TriggerOk() {
            gameObject.SetActive(false);

            OnClickOk.Invoke();
        }

        public void TriggerCancel() {
            gameObject.SetActive(false);

            OnClickCancel.Invoke();
        }
    }
}
