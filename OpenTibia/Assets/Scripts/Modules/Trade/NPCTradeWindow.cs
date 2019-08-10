using OpenTibiaUnity.Core.Trade;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Trade
{
    internal class NPCTradeWindow : Core.Components.Base.MiniWindow
    {
        [SerializeField] private NPCTradeItem m_NPCTradeItemTemplate = null;

        internal void Setup(string npcName, List<TradeObjectRef> buyList, List<TradeObjectRef> sellList) {
            // setup window title
            string title;
            if (npcName != null)
                title = string.Format(TextResources.WINDOWTITLE_NPCTRADE_WITH_NAME, npcName);
            else
                title = TextResources.WINDOWTITLE_NPCTRADE_NO_NAME;

            m_TitleLabel.text = title;

            // setup objects

            foreach (var tradeObj in buyList) {
                var tradeItem = Instantiate(m_NPCTradeItemTemplate, m_PanelContent);
                tradeItem.itemLabel.text = string.Format("{0}: {1} gold", tradeObj.Name, tradeObj.Price);

                tradeItem.gameObject.SetActive(true);
            }
        }
    }
}
