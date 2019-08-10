using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Hotkeys
{
    internal class HotkeyActionPanel : Core.Components.Base.AbstractComponent
    {
        [SerializeField] internal TMPro.TextMeshProUGUI textComponent = null;
        
        internal Color normalColor;
        internal Color highlightColor;

        private Toggle m_ToggleComponent;
        internal Toggle toggleComponent {
            get {
                if (!m_ToggleComponent)
                    m_ToggleComponent = GetComponent<Toggle>();

                return m_ToggleComponent;
            }
        }

        private Image m_ImageComponent;
        internal Image imageComponent {
            get {
                if (!m_ImageComponent)
                    m_ImageComponent = GetComponent<Image>();

                return m_ImageComponent;
            }
        }

        internal KeyCode KeyCode;
        internal EventModifiers EventModifiers;
        internal string BaseText = string.Empty;

        protected override void Awake() {
            base.Awake();

            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
        }

        protected void OnToggleValueChanged(bool value) {
            imageComponent.color = value ? highlightColor : normalColor;
        }

        public void Select() {
            toggleComponent.isOn = true;
            toggleComponent.Select();
        }

    }
}
