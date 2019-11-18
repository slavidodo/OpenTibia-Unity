using OpenTibiaUnity.Core.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    public class PopupWindow : Base.Window, IPointerClickHandler
    {
        public delegate void PopupWindowCallback();

        public enum ButtonType
        {
            Ok,
            Cancel,
            Custom,
        }

        public struct ButtonDescriptor
        {
            public static void DefaultCallback() { }

            public PopupWindowCallback pressedCallback;
            public ButtonType buttonType;
            public KeyCode[] keyCodes;
            public string text;

            public ButtonDescriptor(ButtonType buttonType, PopupWindowCallback pressedCallback = default) {
                if (pressedCallback == null)
                    pressedCallback = DefaultCallback;

                this.buttonType = buttonType;
                this.pressedCallback = pressedCallback;
                switch (buttonType) {
                    case ButtonType.Ok:
                        keyCodes = new KeyCode[] { KeyCode.Return, KeyCode.KeypadEnter };
                        text = "Ok";
                        break;
                    case ButtonType.Cancel:
                        keyCodes = new KeyCode[] { KeyCode.Escape };
                        text = "Cancel";
                        break;
                    default:
                        throw new System.ArgumentException("Invalid button type");
                }
            }

            public ButtonDescriptor(string text, KeyCode[] keyCodes, PopupWindowCallback pressedCallback = null) {
                if (pressedCallback == null)
                    pressedCallback = DefaultCallback;

                this.buttonType = ButtonType.Custom;
                this.pressedCallback = pressedCallback;
                this.text = text;
                this.keyCodes = keyCodes;
            }

            public ButtonDescriptor(string text, KeyCode keyCode, PopupWindowCallback pressedCallback = null) {
                if (pressedCallback == null)
                    pressedCallback = DefaultCallback;

                this.buttonType = ButtonType.Custom;
                this.pressedCallback = pressedCallback;
                this.text = text;
                this.keyCodes = new KeyCode[] { keyCode };
            }
        }

        private static int s_popupCounter = 0;

        [SerializeField] private TMPro.TextMeshProUGUI _titleLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI _messagesLabel = null;
        [SerializeField] private RectTransform _separatorPanel = null;
        [SerializeField] private RectTransform _buttonsPanel = null;

        private List<ButtonDescriptor> _buttons;

        protected override void Awake() {
            base.Awake();
            OpenTibiaUnity.InputHandler.AddKeyDownListener(Utils.EventImplPriority.Default, OnKeyDown);

            _buttons = new List<ButtonDescriptor>();

            _buttonsPanel.gameObject.SetActive(false);
            _separatorPanel.gameObject.SetActive(false);
        }

        protected override void OnDestroy() {
            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.RemoveKeyDownListener(OnKeyDown);
        }

        private void OnKeyDown(Event e, bool _) {
            if (!InputHandler.IsHighlighted(this))
                return;

            ButtonDescriptor descriptor = default;
            if (FindDescriptor(ref descriptor, e.keyCode)) {
                e.Use();
                OnButtonClick(descriptor);
            }
        }

        public void OnPointerClick(PointerEventData eventData) {
            Select();
        }

        private void OnButtonClick(ButtonDescriptor descriptor) {
            Destroy();
            descriptor.pressedCallback.Invoke();
        }

        public void SetTitle(string title) {
            _titleLabel.SetText(title);
        }

        public void SetMessage(string message, int maxWidth = -1, int maxHeight = -1) {
            _messagesLabel.SetText(message);

            var preferedSize = _messagesLabel.GetPreferredValues();

            var layoutElement = _messagesLabel.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = Mathf.Min(maxWidth, preferedSize.x);
            layoutElement.preferredHeight = Mathf.Min(maxHeight, preferedSize.y);
        }

        public void SetMessageAlignment(TMPro.TextAlignmentOptions alignment) {
            _messagesLabel.alignment = alignment;
        }

        public void Destroy() {
            UnlockFromOverlay();
            Destroy(gameObject);
        }

        private bool FindDescriptor(ref ButtonDescriptor descriptor, KeyCode keyCode) {
            int index = _buttons.FindIndex((x) => x.keyCodes.Contains(keyCode));
            if (index == -1)
                return false;

            descriptor = _buttons[index];
            return true;
        }

        public bool ButtonConflicts(ButtonDescriptor other) {
            foreach (var buttonDescriptor in _buttons) {
                foreach (var keyCode in other.keyCodes) {
                    if (buttonDescriptor.keyCodes.Contains(keyCode))
                        return true;
                }
            }
            return false;
        }

        public void AddButton(ButtonDescriptor descriptor) {
            if (ButtonConflicts(descriptor))
                throw new System.ArgumentException("There is a conflict adding this button.");

            var button = Instantiate(OpenTibiaUnity.GameManager.DefaultButtonWithLabel, _buttonsPanel.transform);
            button.onClick.AddListener(() => {
                OnButtonClick(descriptor);
            });

            var label = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            label.SetText(descriptor.text);

            var labelSize = label.GetPreferredValues();

            var layoutElement = button.gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = labelSize.x + 20;

            _buttons.Add(descriptor);

            _buttonsPanel.gameObject.SetActive(true);
            _separatorPanel.gameObject.SetActive(true);
        }

        private static PopupWindow InstantiatePrefab(Transform parent) {
            var instance = Instantiate(OpenTibiaUnity.GameManager.PopupWindowPrefab, parent);
            instance.gameObject.name = $"PopupWindow_{s_popupCounter++}";
            return instance;
        }

        public static PopupWindow CreateOkCancelPopup(Transform parent, string title, string message, PopupWindowCallback onOk, PopupWindowCallback onCancel) {
            var okDesc = new ButtonDescriptor(ButtonType.Ok, onOk);
            var cancelDesc = new ButtonDescriptor(ButtonType.Cancel, onCancel);
            return CreatePopupWindow(parent, title, message, okDesc, cancelDesc);
        }

        public static PopupWindow CreateOkPopup(Transform parent, string title, string message, PopupWindowCallback onOk) {
            var okDesc = new ButtonDescriptor(ButtonType.Ok, onOk);
            return CreatePopupWindow(parent, title, message, okDesc);
        }

        public static PopupWindow CreateCancelPopup(Transform parent, string title, string message, PopupWindowCallback onCancel) {
            var cancelDesc = new ButtonDescriptor(ButtonType.Cancel, onCancel);
            return CreatePopupWindow(parent, title, message, cancelDesc);
        }

        public static PopupWindow CreatePopupWindow(Transform parent, string title, string message, params ButtonDescriptor[] buttons) {
            PopupWindow instance = InstantiatePrefab(parent);
            instance.SetTitle(title);
            instance.SetMessage(message);
            foreach (var desc in buttons)
                instance.AddButton(desc);

            instance.ResetLocalPosition();
            instance.Open();
            return instance;
        }
    }
}
