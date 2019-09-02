using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Hotkeys
{
    public class HotkeyActionPanel : Core.Components.Base.AbstractComponent
    {
        public TMPro.TextMeshProUGUI textComponent = null;
        
        public Color normalColor;
        public Color highlightColor;

        private Toggle _toggleComponent;
        public Toggle toggleComponent {
            get {
                if (!_toggleComponent)
                    _toggleComponent = GetComponent<Toggle>();

                return _toggleComponent;
            }
        }

        private Image _imageComponent;
        public Image imageComponent {
            get {
                if (!_imageComponent)
                    _imageComponent = GetComponent<Image>();

                return _imageComponent;
            }
        }

        public KeyCode KeyCode;
        public EventModifiers EventModifiers;
        public string BaseText = string.Empty;

        protected override void Awake() {
            base.Awake();

            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
        }

        protected void OnToggleValueChanged(bool value) {
            imageComponent.color = value ? highlightColor : normalColor;
        }

        public override void Select() {
            base.Select();
            toggleComponent.isOn = true;
        }

    }
}
