using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    [RequireComponent(typeof(Toggle))]
    public class CharacterPanel : Core.Components.Base.AbstractComponent, IPointerClickHandler
    {
        public TMPro.TextMeshProUGUI characterName = null;
        public TMPro.TextMeshProUGUI worldName = null;
        
        public UnityEvent onDoubleClick { get; } = new UnityEvent();
        public Color normalColor = Core.Colors.ColorFromRGB(0x414141);
        public Color alternateColor = Core.Colors.ColorFromRGB(0x484848);
        public Color highlightColor = Core.Colors.ColorFromRGB(0x585858);
        
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

        protected override void Awake() {
            base.Awake();

            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
            imageComponent.color = normalColor;
        }
        
        protected void OnToggleValueChanged(bool value) {
            if (value)
                imageComponent.color = highlightColor;
            else
                imageComponent.color = (transform.GetSiblingIndex() & 1) == 0 ? alternateColor : normalColor;
        }
        
        public override void Select() {
            toggleComponent.isOn = true;
            toggleComponent.Select();
        }
        
        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.clickCount == 2) {
                onDoubleClick.Invoke();
            }
        }
    }
}
