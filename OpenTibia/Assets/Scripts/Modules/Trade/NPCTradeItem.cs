using OpenTibiaUnity.Core.Trade;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Trade
{
    [RequireComponent(typeof(Toggle))]
    public class NPCTradeItem : Core.Components.Base.AbstractComponent
    {
        public TMPro.TextMeshProUGUI itemLabel = null;

        public Color normalColor = new Color(0, 0, 0, 0); // transparent
        public Color highlightColor = Core.Colors.ColorFromRGB(0x585858);
        public TradeObjectRef tradeObject = null;

        private Toggle _toggleComponent = null;
        public Toggle toggleComponent {
            get {
                if (!_toggleComponent)
                    _toggleComponent = GetComponent<Toggle>();
                return _toggleComponent;
            }
        }

        private RawImage _imageComponent;
        public RawImage imageComponent {
            get {
                if (!_imageComponent)
                    _imageComponent = GetComponent<RawImage>();

                return _imageComponent;
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
