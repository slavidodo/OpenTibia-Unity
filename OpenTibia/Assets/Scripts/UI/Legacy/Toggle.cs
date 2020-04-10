using UnityEngine;
using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class Toggle : UnityUI.Toggle, IBasicUIComponent
    {
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

        public string text {
            get => _label?.text;
            set {
                if (_label)
                    _label.text = value;
            }
        }

        public TMPro.TextAlignmentOptions alignment {
            get {
                if (_label)
                    return _label.alignment;
                return TMPro.TextAlignmentOptions.Center;
            }
            set {
                if (_label)
                    _label.alignment = value;
            }
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
            if (effect)
                _label.margin = new Vector4(1, 1, 0, 0);
            else
                _label.margin = new Vector4(0, 0, 0, 0);
        }

        protected override void DoStateTransition(SelectionState state, bool instant) {
            // TODO this is not working as expected with switching the value of on/off
            base.DoStateTransition(state, instant);
            UpdateLabelState(state == SelectionState.Pressed || isOn);
        }
    }
}
