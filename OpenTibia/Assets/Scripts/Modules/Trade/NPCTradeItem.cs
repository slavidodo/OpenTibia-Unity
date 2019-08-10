using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Trade
{
    [RequireComponent(typeof(Toggle))]
    internal class NPCTradeItem : Core.Components.Base.AbstractComponent
    {
        [SerializeField] internal TMPro.TextMeshProUGUI itemLabel = null;

        internal Color normalColor = new Color(0, 0, 0, 0); // transparent
        internal Color highlightColor = Core.Colors.ColorFromRGB(0x585858);

        private Toggle m_ToggleComponent = null;
        internal Toggle toggleComponent {
            get {
                if (!m_ToggleComponent)
                    m_ToggleComponent = GetComponent<Toggle>();
                return m_ToggleComponent;
            }
        }

        private RawImage m_ImageComponent;
        internal RawImage imageComponent {
            get {
                if (!m_ImageComponent)
                    m_ImageComponent = GetComponent<RawImage>();

                return m_ImageComponent;
            }
        }

        protected override void Start() {
            base.Start();

            toggleComponent.onValueChanged.AddListener(OnToggleValueChanged);
            imageComponent.color = normalColor;
        }

        protected void OnToggleValueChanged(bool value) {
            imageComponent.color = value ? highlightColor : normalColor;
        }
    }
}
