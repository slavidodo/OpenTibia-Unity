using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleWrapper : Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        public TMPro.TextMeshProUGUI label = null;

        private bool _pointerDown = false;
        private bool _lastValue = false;
        private bool _havingEffect = false;

        private Toggle _toggle = null;
        public Toggle toggle {
            get {
                if (!_toggle)
                    _toggle = GetComponent<Toggle>();
                return _toggle;
            }
        }

        protected override void Awake() {
            base.Start();

            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        public void DisableComponent() {
            toggle.interactable = false;
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        public void EnableComponent() {
            toggle.interactable = true;
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (_pointerDown)
                return;
            
            _pointerDown = true;
            if (!_havingEffect && label) {
                _havingEffect = !toggle.isOn;
                label.margin += new Vector4(1, 1, 0, 0);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            if (!_pointerDown)
                return;

            _pointerDown = false;
            if (_havingEffect && label) {
                _havingEffect = false;
                label.margin -= new Vector4(1, 1, 0, 0);
            }
        }

        void OnToggleValueChanged(bool value) {
            if (value) {
                if (_pointerDown)
                    return;

                if (!_havingEffect && label) {
                    _havingEffect = true;
                    label.margin += new Vector4(1, 1, 0, 0);
                }
            } else {
                if (_pointerDown)
                    _pointerDown = false;

                if (_havingEffect && label) {
                    _havingEffect = false;
                    label.margin -= new Vector4(1, 1, 0, 0);
                }
            }
        }
    }
}
