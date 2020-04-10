using OpenTibiaUnity.Core.Trade;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Trade
{
    [RequireComponent(typeof(UnityUI.Toggle))]
    public class NPCTradeItem : UI.Legacy.ToggleListItem
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI _label = null;

        // properties
        public string text {
            get => _label.text;
            set => _label.text = value;
        }

        public TradeObjectRef tradeObject { get; set; } = null;
    }
}
