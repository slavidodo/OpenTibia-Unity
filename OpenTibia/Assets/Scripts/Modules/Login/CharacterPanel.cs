using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    [RequireComponent(typeof(Toggle))]
    internal class CharacterPanel : Core.Components.Base.AbstractComponent, IPointerClickHandler
    {
        [SerializeField] internal TMPro.TextMeshProUGUI characterName = null;
        [SerializeField] internal TMPro.TextMeshProUGUI worldName = null;
        
        internal UnityEvent onDoubleClick { get; } = new UnityEvent();
        internal Color normalColor = Core.Colors.ColorFromRGB(0x414141);
        internal Color alternateColor = Core.Colors.ColorFromRGB(0x484848);
        internal Color highlightColor = Core.Colors.ColorFromRGB(0x585858);
        
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
        
        public void Select() {
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
