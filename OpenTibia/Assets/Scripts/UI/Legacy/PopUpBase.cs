using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public enum PopUpKeyMask
    {
        None = 1 << 0, // trigger nothing
        Enter = 1 << 1,
        Escape = 1 << 2,
        Both = Enter | Escape
    }

    public enum PopUpButtonMask
    {
        None = 1 << 0, // does nothing
        Ok = 1 << 1,
        Close = 1 << 2,
        Cancel = 1 << 3,
        Yes = 1 << 4,
        No = 1 << 5,
        Abort = 1 << 6,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class PopUpBase : Core.Components.Base.Module
    {
        public const int PopupPriorityMax = int.MaxValue;
        public const int PopupPriorityDefault = 0;

        // non-serialized fields
        [System.NonSerialized]
        public PopUpEvent onOpen = new PopUpEvent();
        [System.NonSerialized]
        public PopUpEvent onClose = new PopUpEvent();

        // serialized fields
        [SerializeField]
        protected TMPro.TextMeshProUGUI _title = null;
        [SerializeField]
        protected RectTransform _content = null;
        [SerializeField]
        protected RectTransform _footerSeparator = null;
        [SerializeField]
        protected UnityUI.HorizontalLayoutGroup _footer = null;

        // non-serialized fields
        public GameObject queueBlocker = null;

        // fields
        private Dictionary<PopUpButtonMask, Button> _buttons;

        // properties
        public bool Visible { get => enabled && gameObject.activeSelf; }
        public bool ChangingVisibility { get; set; } = false;
        public PopUpKeyMask KeyMask { get; set; } = PopUpKeyMask.Both;
        public int Priority { get; set; } = 0;

        private Canvas _canvas = null;
        public Canvas canvas {
            get {
                if (!_canvas)
                    _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }

        public string title {
            get => _title.text;
            set {
                _title.text = value;
            }
        }

        protected override void Awake() {
            base.Awake();

            _footerSeparator.gameObject.SetActive(false);
            _footer.gameObject.SetActive(false);

            _buttons = new Dictionary<PopUpButtonMask, Button>();
        }

        protected override void OnEnable() {
            base.OnEnable();

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null)
                inputHandler.AddKeyDownListener(Core.Utils.EventImplPriority.High, OnInternalKeyDown);
        }

        protected override void OnDisable() {
            base.OnDisable();

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler != null)
                inputHandler.RemoveKeyDownListener(OnInternalKeyDown);
        }

        private void OnInternalKeyDown(Event e, bool repeat) {
            if (!Core.Input.InputHandler.IsHighlighted(this))
                return;

            OnKeyDown(e, repeat);
        }

        protected virtual void OnKeyDown(Event e, bool repeat) {
            PopUpButtonMask buttonMask = PopUpButtonMask.None;
            switch (e.keyCode) {
                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                    if ((KeyMask & PopUpKeyMask.Enter) != 0) {
                        e.Use();
                        buttonMask = PopUpButtonMask.Ok | PopUpButtonMask.Close | PopUpButtonMask.Yes;
                    }
                    break;

                case KeyCode.Escape:
                    if ((KeyMask & PopUpKeyMask.Escape) != 0) {
                        e.Use();
                        buttonMask = PopUpButtonMask.Close | PopUpButtonMask.Cancel | PopUpButtonMask.No | PopUpButtonMask.Abort;
                    }
                    break;
            }

            if (buttonMask != PopUpButtonMask.None) {
                InternalInvokeButton(buttonMask);
                onClose.Invoke(this);
            }
        }

        public virtual Button AddButton(string text, UnityAction pressedCallback) {
            var button = AddButton(PopUpButtonMask.None, text);
            button.onClick.AddListener(pressedCallback);
            return button;
        }

        public virtual Button AddButton(PopUpButtonMask buttonMask, UnityAction pressedCallback) {
            return AddButton(buttonMask, null, pressedCallback);
        }

        public virtual Button AddButton(PopUpButtonMask buttonMask, string text, UnityAction pressedCallback) {
            var button = AddButton(buttonMask, text);
            button.onClick.AddListener(pressedCallback);
            if (buttonMask != PopUpButtonMask.None)
                _buttons.Add(buttonMask, button);
            return button;
        }

        public virtual Button AddButton(PopUpButtonMask buttonMask, string text = null /* = default */) {
            _footerSeparator.gameObject.SetActive(true);
            _footer.gameObject.SetActive(true);

            if (string.IsNullOrEmpty(text))
                text = GetTextForMask(buttonMask);

            var button = Instantiate(OpenTibiaUnity.GameManager.ButtonPrefab, _footer.transform);
            button.text = text;

            var preferredValues = button.GetPreferredValues();
            var layoutElement = button.gameObject.AddComponent<UnityUI.LayoutElement>();
            layoutElement.preferredWidth = preferredValues.x + 20;
            layoutElement.preferredHeight = 20;

            switch (buttonMask) {
                case PopUpButtonMask.Ok:
                case PopUpButtonMask.Close:
                case PopUpButtonMask.Cancel:
                case PopUpButtonMask.Yes:
                case PopUpButtonMask.No:
                case PopUpButtonMask.Abort:
                    button.onClick.AddListener(() => onClose.Invoke(this));
                    break;
            }

            return button;
        }

        public virtual void Show() {
            Core.Game.PopUpQueue.Instance.Show(this);
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => Select());
        }

        public virtual void Hide() {
            Core.Game.PopUpQueue.Instance.Hide(this);
        }

        private void InternalInvokeButton(PopUpButtonMask mask) {
            foreach (var p in _buttons) {
                if ((mask & p.Key) != 0) {
                    p.Value.onClick.Invoke();
                    break;
                }
            }
        }

        private static string GetTextForMask(PopUpButtonMask mask) {
            switch (mask) {
                case PopUpButtonMask.Ok:
                    return TextResources.LABEL_OK;
                case PopUpButtonMask.Close:
                    return TextResources.LABEL_CLOSE;
                case PopUpButtonMask.Cancel:
                    return TextResources.LABEL_CANCEL;
                case PopUpButtonMask.Yes:
                    return TextResources.LABEL_YES;
                case PopUpButtonMask.No:
                    return TextResources.LABEL_NO;
                case PopUpButtonMask.Abort:
                    return TextResources.LABEL_ABORT;
            }

            return null;
        }

        public class PopUpEvent : UnityEvent<PopUpBase> {}
    }
}