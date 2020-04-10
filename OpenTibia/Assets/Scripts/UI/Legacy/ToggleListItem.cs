using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.Toggle), typeof(UnityUI.RawImage))]
    public class ToggleListItem : BasicElement, IPointerClickHandler
    {
        // fields
        private bool _useAlternate = false;
        private UnityUI.Toggle _toggle; // dont-assign
        private UnityUI.RawImage _image; // dont-assign


        // properties
        public UnityEvent onDoubleClick { get; } = new UnityEvent();

        public bool UseAlternateColor {
            get => _useAlternate;
            set {
                if (_useAlternate != value) {
                    _useAlternate = value;
                    if (!toggle.isOn)
                        image.color = GetDefaultColor();
                }
            }
        }

        public UnityUI.Toggle toggle {
            get {
                if (!_toggle)
                    _toggle = GetComponent<UnityUI.Toggle>();

                return _toggle;
            }
        }

        public UnityUI.RawImage image {
            get {
                if (!_image)
                    _image = GetComponent<UnityUI.RawImage>();

                return _image;
            }
        }

        protected override void Awake() {
            base.Awake();

            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            image.color = GetDefaultColor();
        }

        protected void OnToggleValueChanged(bool value) {
            if (value)
                image.color = OpenTibiaUnity.GameManager.HighlightColor;
            else
                image.color = GetDefaultColor();
        }

        public override void Select() {
            toggle.isOn = true;
            toggle.Select();
        }

        private Color GetDefaultColor() {
            bool alternate = (transform.GetSiblingIndex() & 1) != 0;
            if (UseAlternateColor && alternate)
                return OpenTibiaUnity.GameManager.AlternateColor;
            else
                return OpenTibiaUnity.GameManager.NormalColor;
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.clickCount == 2)
                onDoubleClick.Invoke();
        }
    }
}
