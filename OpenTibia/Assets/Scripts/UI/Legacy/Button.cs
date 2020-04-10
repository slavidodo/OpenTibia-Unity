using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class Button : UnityUI.Button, IBasicUIComponent {
        // serialized fields
        [SerializeField]
        private TMPro.TextMeshProUGUI _label = null;

        // properties
        [System.NonSerialized] private RectTransform _rectTransform;
        public RectTransform rectTransform {
            get {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public bool AllowLabelTransitions { get; set; } = true;

        public string text {
            get => _label?.text;
            set {
                if (_label) _label.text = value;
            }
        }

        public Color color {
            get => _label != null ? _label.color : Color.white;
            set {
                if (_label) _label.color = value;
            }
        }

        public TMPro.TextAlignmentOptions alignment {
            get  =>_label != null ? _label.alignment : TMPro.TextAlignmentOptions.Center;
            set {
                if (_label) _label.alignment = value;
            }
        }

        public Vector2 preferredValues {
            get => _label != null ? _label.GetPreferredValues() : Vector2.zero;
        }

        public Vector2 GetPreferredValues() {
            if (!_label)
                return Vector2.zero;
            return _label.GetPreferredValues();
        }

        public bool IsEnabled() {
            return IsInteractable();
        }

        public void SetEnabled(bool enabled) {
            if (enabled)
                Enable();
            else
                Disable();
        }

        public void Enable() {
            base.interactable = true;
            if (_label)
                _label.color = Core.Colors.Default;
        }

        public void Disable() {
            base.interactable = false;
            if (_label)
                _label.color = Core.Colors.DefaultDisabled;
        }

        private void UpdateLabelState(bool effect) {
            if (_label) {
                if (effect)
                    _label.margin = new Vector4(1, 1, 0, 0);
                else
                    _label.margin = new Vector4(0, 0, 0, 0);
            } else {
                Debug.LogWarning("It's invalid to have a button without label, this is a bug and should be fixed");
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant) {
            base.DoStateTransition(state, instant);
            UpdateLabelState(state == SelectionState.Pressed);
        }
    }
}
