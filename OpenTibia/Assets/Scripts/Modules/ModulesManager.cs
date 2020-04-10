using System.Collections.Generic;
using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Game;
using OpenTibiaUnity.Core.Trade;
using UnityEngine;

namespace OpenTibiaUnity.Modules
{
    public class ModulesManager : MonoBehaviour
    {
        public static ModulesManager Instance { get; private set; }
        
        [Header("Module.Battle")]
        public Battle.BattleCreature BattleCreaturePrefab = null;
        public Battle.BattleWidget BattleWidgetPrefab = null;

        [Header("Module.Console")]
        public Console.ConsoleBuffer ConsoleBufferPrefab = null;
        public Console.ChannelTab ChannelTabPrefab = null;
        public Console.ChannelSelectionWidget ChannelSelectionWidgetPrefab = null;
        public Console.ChannelSelectionItem ChannelSelectionItemPrefab = null;

        [Header("Module.Container")]
        public Container.ContainerWidget ContainerWidgetPrefab = null;

        [Header("Module.Login")]
        public Login.CharacterSelectionWidget CharacterSelectionWidget = null;
        public Login.LoginWidget LoginWidget = null;
        public Login.AuthenticatorWidget AuthenticatorWidget = null;
        public Login.AccountCharacter AccountCharacterPrefab = null;

        [Header("Module.ModalDialog")]
        public ModalDialog.ServerModalDialog ServerModalDialogPrefab = null;

        [Header("Module.Options")]
        public Options.LegacyConsoleOptionsWidget LegacyConsoleOptionsWidget = null;
        public Options.LegacyGeneralOptionsWidget LegacyGeneralOptionsWidget = null;
        public Options.LegacyGraphicsOptionsWidget LegacyGraphicsOptionWidget = null;
        public Options.LegacyHotkeyOptionsWidget LegacyHotkeyOptionsWidget = null;
        public Options.LegacyOptionsWidget LegacyOptionsWidget = null;
        public Options.HotkeyActionPanel HotkeysActionPanelPrefab = null;

        [Header("Module.Outfit")]
        public Outfit.OutfitWidget OutfitWindow = null;

        [Header("Module.Skills")]
        public Skills.SkillsWidget SkillsWidgetPrefab = null;
        public Skills.RawSkillPanel RawSkillPanelPrefab = null;
        public Skills.ProgressSkillPanel ProgressSkillPanelPrefab = null;
        public Skills.ProgressIconSkillPanel ProgressIconSkillPanelPrefab = null;

        [Header("Module.Trade")]
        public Trade.NPCTradeWindow NPCTradeWindowPrefab = null;

        protected void Awake() {
            Instance = this;

            LegacyHotkeyOptionsWidget.gameObject.SetActive(true);
            LegacyHotkeyOptionsWidget.gameObject.SetActive(false);
        }

        protected void Start() {
            LoginWidget.Show();

            var gameManager = OpenTibiaUnity.GameManager;

            gameManager.onRequestHotkeysDialog.AddListener(OnRequestHotkeysDialog);
            gameManager.onRequestChatHistoryPrev.AddListener(OnRequestChatHistoryPrev);
            gameManager.onRequestChatHistoryNext.AddListener(OnRequestChatHistoryNext);
            gameManager.onRequestChatSend.AddListener(OnRequestChatSend);
            gameManager.onRequestOutfitDialog.AddListener(OnRequestOutfitDialog);
            gameManager.onRequestNPCTrade.AddListener(OnRequestNPCTrade);
            gameManager.onRequestCloseNPCTrade.AddListener(OnRequestCloseNPCTrade);
            gameManager.onRequestModalDialog.AddListener(OnRequestModalDialog);
        }
        
        private void OnRequestHotkeysDialog() {
            LegacyHotkeyOptionsWidget.Show();
        }

        private void OnRequestChatHistoryPrev() {
            var chatModule = OpenTibiaUnity.GameManager.GetModule<Console.ConsoleModule>();
            if (!chatModule)
                return;

            chatModule.OnChatHistory(-1);
        }

        private void OnRequestChatHistoryNext() {
            var chatModule = OpenTibiaUnity.GameManager.GetModule<Console.ConsoleModule>();
            if (!chatModule)
                return;

            chatModule.OnChatHistory(1);
        }

        private void OnRequestChatSend(string text, bool autoSend, int channelId) {
            var chatModule = OpenTibiaUnity.GameManager.GetModule<Console.ConsoleModule>();
            if (!chatModule)
                return;

            if (channelId != -1)
                chatModule.SelectChannel(OpenTibiaUnity.ChatStorage.GetChannel(channelId), true);

            if (text != null)
                chatModule.SetInputText(text);

            if (autoSend)
                chatModule.SendChannelMessage();
        }

        private void OnRequestOutfitDialog(AppearanceInstance outfit, AppearanceInstance mountOutfit, List<ProtocolOutfit> outfits, List<ProtocolMount> mounts) {
            if (OutfitWindow.UpdateProperties(outfit, mountOutfit, outfits, mounts))
                OutfitWindow.Show();
        }
        private void OnRequestNPCTrade(string npcName, List<TradeObjectRef> buyList, List<TradeObjectRef> sellList) {
            var npcTradeWindow = OpenTibiaUnity.GameManager.GetModule<Trade.NPCTradeWindow>();
            if (!npcTradeWindow) {
                var gameWindow = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
                npcTradeWindow = gameWindow.CreateSidebarWidget(NPCTradeWindowPrefab);
            }

            npcTradeWindow.Setup(npcName, buyList, sellList);
        }

        private void OnRequestCloseNPCTrade() {
            var npcTradeWindow = OpenTibiaUnity.GameManager.GetModule<Trade.NPCTradeWindow>();
            if (npcTradeWindow)
                npcTradeWindow.CloseWithoutNotifying();
        }

        private void OnRequestModalDialog(ProtocolModalDialog pModalDialog) {
            ModalDialog.ServerModalDialog.CreateModalDialog(pModalDialog);
        }
    }
}
