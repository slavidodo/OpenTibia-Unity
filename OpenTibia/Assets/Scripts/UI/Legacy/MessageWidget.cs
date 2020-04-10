using OpenTibiaUnity.Core.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.VerticalLayoutGroup))]
    public class MessageWidget : MessageWidgetBase
    {
        private static int s_messageCounter = 0;

        // fields
        private Dictionary<KeyCode, UnityAction> _actions;

        protected override void Awake() {
            base.Awake();

            Priority = PopupPriorityMax;
            _actions = new Dictionary<KeyCode, UnityAction>();
        }

        protected override void OnKeyDown(Event e, bool repeat) {
            base.OnKeyDown(e, repeat);
            if (_actions.TryGetValue(e.keyCode, out UnityAction action)) {
                // if the default action consumed the event, so we don't need
                // to hide the window anymore
                if (e.type == EventType.KeyDown) {
                    e.Use();
                    Hide();
                }

                // even if the event is consumed, the custom event should
                // be invoked
                action.Invoke();
            }
        }

        public override void Hide() {
            base.Hide();
            Destroy(gameObject);
        }

        public Button AddButton(PopUpButtonMask buttonMask, KeyCode keyCode) {
            var button = AddButton(buttonMask);
            _actions.Add(keyCode, Hide);
            return button;
        }

        public Button AddButton(PopUpButtonMask buttonMask, KeyCode[] keyCodes) {
            var button = AddButton(buttonMask);
            foreach (var keyCode in keyCodes)
                _actions.Add(keyCode, Hide);
            return button;
        }

        public Button AddButton(PopUpButtonMask buttonMask, KeyCode keyCode, UnityAction action) {
            var button = AddButton(buttonMask, action);
            _actions.Add(keyCode, action);
            return button;
        }

        public Button AddButton(PopUpButtonMask buttonMask, KeyCode[] keyCodes, UnityAction action) {
            var button = AddButton(buttonMask, action);
            foreach (var keyCode in keyCodes)
                _actions.Add(keyCode, action);
            return button;
        }

        public Button AddButton(string text, KeyCode keyCode) {
            var button = AddButton(text, Hide);
            _actions.Add(keyCode, Hide);
            return button;
        }

        public Button AddButton(string text, KeyCode[] keyCodes) {
            var button = AddButton(text, Hide);
            foreach (var keyCode in keyCodes)
                _actions.Add(keyCode, Hide);
            return button;
        }

        public Button AddButton(string text, KeyCode keyCode, UnityAction action) {
            var button = AddButton(text, MakeCloseAction(action));
            _actions.Add(keyCode, MakeCloseAction(action));
            return button;
        }

        public Button AddButton(string text, KeyCode[] keyCodes, UnityAction action) {
            var button = AddButton(text, MakeCloseAction(action));
            foreach (var keyCode in keyCodes)
                _actions.Add(keyCode, MakeCloseAction(action));
            return button;
        }

        private UnityAction MakeCloseAction(UnityAction inner) {
            return () => {
                inner.Invoke();
                Hide();
            };
        }

        private static MessageWidget InstantiateMessageWidgetObject(Transform parent) {
            var instance = Instantiate(OpenTibiaUnity.GameManager.MessageWidgetPrefab, parent);
            instance.gameObject.name = $"MessageWidget_{s_messageCounter++}";
            instance.gameObject.SetActive(false);
            return instance;
        }

        public static MessageWidget CreateOkCancelPopUp(Transform parent, string title, string message, UnityAction onOk, UnityAction onCancel) {
            var widget = CreateMessageWidget(parent, title, message);
            widget.AddButton(PopUpButtonMask.Ok, onOk);
            widget.AddButton(PopUpButtonMask.Cancel, onCancel);
            widget.KeyMask = PopUpKeyMask.Both;
            return widget;
        }

        public static MessageWidget CreateOkPopUp(Transform parent, string title, string message, UnityAction onOk) {
            var widget = CreateMessageWidget(parent, title, message);
            widget.AddButton(PopUpButtonMask.Ok, onOk);
            widget.KeyMask = PopUpKeyMask.Enter;
            return widget;
        }

        public static MessageWidget CreateCancelPopUp(Transform parent, string title, string message, UnityAction onCancel) {
            var widget = CreateMessageWidget(parent, title, message);
            widget.AddButton(PopUpButtonMask.Cancel, onCancel);
            widget.KeyMask = PopUpKeyMask.Escape;
            return widget;
        }

        public static MessageWidget CreateMessageWidget(Transform parent, string title, string message) {
            MessageWidget instance = InstantiateMessageWidgetObject(parent);
            instance.title = title;
            instance.message = message;

            instance.Show();
            return instance;
        }
    }
}
