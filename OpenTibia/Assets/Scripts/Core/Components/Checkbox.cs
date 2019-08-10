using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Toggle), typeof(Image))]
    internal class Checkbox : Base.AbstractComponent
    {
        [SerializeField] protected Sprite m_OnSprite = null;
        [SerializeField] protected Sprite m_OffSprite = null;
        [SerializeField] protected Sprite m_DisabledOnSprite = null;

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
        
        internal bool Checked {
            get => toggleComponent.isOn;
            set {
                if (toggleComponent.isOn != value)
                    toggleComponent.isOn = value;
            }
        }

        protected override void Start() {
            base.Start();

            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
            OnToggleValueChanged(toggleComponent.isOn);
        }

        private void OnToggleValueChanged(bool value) {
            if (value)
                imageComponent.sprite = m_OnSprite;
            else
                imageComponent.sprite = m_OffSprite;
        }

        internal void DisableToggle() {
            toggleComponent.interactable = false;
            if (Checked)
                imageComponent.sprite = m_DisabledOnSprite;
            else
                imageComponent.sprite = m_OffSprite;
        }

        internal void EnableToggle() {
            toggleComponent.interactable = true;
            if (Checked)
                imageComponent.sprite = m_OnSprite;
            else
                imageComponent.sprite = m_OffSprite;
        }
    }
}
