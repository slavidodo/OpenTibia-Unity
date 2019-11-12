using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OpenTibiaUnity.Core.Input;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class PopupWindow : Base.Window, IPointerClickHandler
    {
        [SerializeField] private TMPro.TextMeshProUGUI _titleLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI _messagesLabel = null;
        [SerializeField] private RectTransform _separatorPanel = null;
        [SerializeField] private RectTransform _buttonsPanel = null;

        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;

        [SerializeField] private bool _sizeCheckRequired = false;

        private Vector2Int _refMaximumSize = Vector2Int.zero;

        private PopupMenuType _popupMenuType = PopupMenuType.OKCancel;
        public PopupMenuType PopupType {
            set {
                if (value != _popupMenuType) {
                    _oKButton.gameObject.SetActive((value & PopupMenuType.OK) != 0);
                    _cancelButton.gameObject.SetActive((value & PopupMenuType.Cancel) != 0);
                    _buttonsPanel.gameObject.SetActive(value != PopupMenuType.NoButtons);
                    _popupMenuType = value;

                    if (value == PopupMenuType.NoButtons) {
                        _separatorPanel.gameObject.SetActive(false);
                        _buttonsPanel.gameObject.SetActive(false);
                    } else {
                        _separatorPanel.gameObject.SetActive(true);
                        _buttonsPanel.gameObject.SetActive(true);
                    }
                }
            }
        }

        public Button.ButtonClickedEvent onOKClick { get; } = new Button.ButtonClickedEvent();
        public Button.ButtonClickedEvent onCancelClick { get; } = new Button.ButtonClickedEvent();

        protected override void Awake() {
            base.Awake();
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Utils.EventImplPriority.Default, OnKeyDown);
        }

        protected override void Start() {
            base.Start();
            _oKButton.onClick.AddListener(TriggerOk);
            _cancelButton.onClick.AddListener(TriggerCancel);
        }

        private void OnKeyDown(Event e, bool _) {
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
        }

        protected new void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();
            if (_sizeCheckRequired) {
                int maxWidth = _refMaximumSize.x;
                int maxHeight = _refMaximumSize.y;

                var layoutElement = _messagesLabel.GetComponent<LayoutElement>();
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

        public void SetTitle(string title) {
            _titleLabel.SetText(title);
        }

        public void SetMessage(string message, int maxWidth = -1, int maxHeight = -1) {
            _refMaximumSize = new Vector2Int(maxWidth, maxHeight);
            _sizeCheckRequired = true;

            var layoutElement = _messagesLabel.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = -1;
            layoutElement.preferredHeight = -1;

            _messagesLabel.SetText(message);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        public void SetMessageAlignment(TMPro.TextAlignmentOptions alignment) {
            _messagesLabel.alignment = alignment;
        }

        protected void TriggerHideWindow(bool enter) {
            Hide();

            if (enter && (_popupMenuType & PopupMenuType.OK) != 0)
                TriggerOk();
            else if (!enter && (_popupMenuType & PopupMenuType.Cancel) != 0)
                TriggerCancel();
        }
        
        public void OnPointerClick(PointerEventData eventData) {
            Select();
        }

        public void TriggerOk() {
            if (LockedToOverlay)
                Close();
            else
                Hide();

            onOKClick.Invoke();
        }

        public void TriggerCancel() {
            if (LockedToOverlay)
                Close();
            else
                Hide();

            onCancelClick.Invoke();
        }
    }
}
