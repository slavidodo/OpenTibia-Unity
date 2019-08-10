using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Toggle))]
    internal class ToggleWrapper : Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] internal TMPro.TextMeshProUGUI label = null;

        private bool m_PointerDown = false;

        private Toggle m_Toggle = null;
        internal Toggle toggle {
            get {
                if (!m_Toggle)
                    m_Toggle = GetComponent<Toggle>();
                return m_Toggle;
            }
        }

        protected override void Awake() {
            base.Start();

            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        internal void DisableComponent() {
            toggle.interactable = false;
            if (label)
                label.color = Colors.ColorFromRGB(0x6F6F6F);
        }

        internal void EnableComponent() {
            toggle.interactable = true;
            if (label)
                label.color = Colors.ColorFromRGB(0xC0C0C0);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            if (m_PointerDown)
                return;

            m_PointerDown = true;
            if (!toggle.isOn && label)
                label.margin += new Vector4(1, -1, 0, 0);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            if (!m_PointerDown)
                return;

            m_PointerDown = false;
            if (!toggle.isOn && label)
                label.margin -= new Vector4(1, -1, 0, 0);
        }

        void OnToggleValueChanged(bool value) {
            if (value) {
                if (m_PointerDown)
                    return;

                if (label)
                    label.margin += new Vector4(1, -1, 0, 0);
            } else {
                // if pointer was down, and we suddenly toggled off
                // we want to avoid unnessecary margin
                if (m_PointerDown)
                    m_PointerDown = false;

                if (label)
                    label.margin -= new Vector4(1, -1, 0, 0);
            }
        }
    }
}
