using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Button))]
    internal class ButtonWrapper : Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] internal TMPro.TextMeshProUGUI label = null;

        private bool m_PointerDown = false;

        private Button m_Button = null;
        internal Button button {
            get {
                if (!m_Button)
                    m_Button = GetComponent<Button>();
                return m_Button;
            }
        }
        
        internal void DisableComponent() {
            button.interactable = false;
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        internal void EnableComponent() {
            button.interactable = true;
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (m_PointerDown)
                return;

            m_PointerDown = true;
            if (label)
                label.margin += new Vector4(1, -1, 0, 0);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            m_PointerDown = false;
            if (label)
                label.margin -= new Vector4(1, -1, 0, 0);
        }
    }
}
