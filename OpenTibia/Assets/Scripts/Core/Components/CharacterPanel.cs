using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Toggle))]
    public class CharacterPanel : UIBehaviour, IDeselectHandler, IPointerClickHandler
    {
        public TMPro.TextMeshProUGUI characterName;
        public TMPro.TextMeshProUGUI worldName;

        private bool m_ColorSwitched = false;

        public UnityEvent onDoubleClick;

        public bool ColorSwitched {
            get {
                return m_ColorSwitched;
            }

            set {
                if (m_ColorSwitched == value)
                    return;

                m_ColorSwitched = value;
                var colorBlock = ToggleComponent.colors;
                if (m_ColorSwitched)
                    colorBlock.normalColor = Colors.ColorFromARGB(0x414141);
                else
                    colorBlock.normalColor = Colors.ColorFromARGB(0x484848);

                ToggleComponent.colors = colorBlock;
            }
        }

        private Toggle m_ToggleComponent;
        public Toggle ToggleComponent {
            get {
                if (!m_ToggleComponent)
                    m_ToggleComponent = GetComponent<Toggle>();

                return m_ToggleComponent;
            }
        }

        void SwitchColor() {
            ColorSwitched = !ColorSwitched;
        }

        public void Select() {
            ToggleComponent.Select();
        }

        public void OnDeselect(BaseEventData eventData) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                if (!Utility.SelectableMehcanism.CanDeselect<CharacterPanel>())
                    Select();
            });
        }

        public void OnPointerClick(PointerEventData eventData) {
            if (eventData.clickCount == 2) {
                onDoubleClick.Invoke();
            }
        }
    }
}
