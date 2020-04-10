using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.Toggle), typeof(UnityUI.Image))]
    public class Checkbox : BasicElement
    {
        [SerializeField] protected Sprite _onSprite = null;
        [SerializeField] protected Sprite _offSprite = null;
        [SerializeField] protected Sprite _disabledOnSprite = null;

        private UnityUI.Toggle _toggleComponent;
        public UnityUI.Toggle toggleComponent {
            get {
                if (!_toggleComponent)
                    _toggleComponent = GetComponent<UnityUI.Toggle>();

                return _toggleComponent;
            }
        }

        private UnityUI.Image _imageComponent;
        public UnityUI.Image imageComponent {
            get {
                if (!_imageComponent)
                    _imageComponent = GetComponent<UnityUI.Image>();

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
