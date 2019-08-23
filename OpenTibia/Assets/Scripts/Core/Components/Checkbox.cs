using System;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Toggle), typeof(Image))]
    public class Checkbox : Base.AbstractComponent
    {
        [SerializeField] protected Sprite _onSprite = null;
        [SerializeField] protected Sprite _offSprite = null;
        [SerializeField] protected Sprite _disabledOnSprite = null;

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
        
        public bool Checked {
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
                imageComponent.sprite = _onSprite;
            else
                imageComponent.sprite = _offSprite;
        }

        public void DisableToggle() {
            toggleComponent.interactable = false;
            if (Checked)
                imageComponent.sprite = _disabledOnSprite;
            else
                imageComponent.sprite = _offSprite;
        }

        public void EnableToggle() {
            toggleComponent.interactable = true;
            if (Checked)
                imageComponent.sprite = _onSprite;
            else
                imageComponent.sprite = _offSprite;
        }
    }
}
