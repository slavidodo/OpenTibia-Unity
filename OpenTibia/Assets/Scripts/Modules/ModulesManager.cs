using System;
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
        public Battle.BattleWindow BattleWindowPrefab = null;

        [Header("Module.Console")]
        public Console.ConsoleBuffer ConsoleBufferPrefab = null;
        public Console.ChannelTab ChannelTabPrefab = null;

        [Header("Module.Container")]
        public Container.ContainerWindow ContainerWindowPrefab = null;
        public Container.ItemView ItemViewPrefab = null;

        [Header("Module.Hotkeys")]
        public Hotkeys.HotkeyActionPanel HotkeysActionPanelPrefab = null;
        public Hotkeys.HotkeysWindow HotkeysWindow = null;

        [Header("Module.Login")]
        public Login.CharactersWindow CharactersWindow = null;
        public Login.LoginWindow LoginWindow = null;
        public Login.AuthenticatorWindow AuthenticatorWindow = null;
        public Login.CharacterPanel CharacterPanelPrefab = null;

        [Header("Module.Options")]
        public Options.LegacyGeneralOptionsWindow LegacyGeneralOptionsWindow = null;
        public Options.LegacyGraphicsOptionsWindow LegacyGraphicsOptionWindow = null;
        public Options.LegacyConsoleOptionsWindow LegacyConsoleOptionsWindow = null;
        public Options.LegacyOptionsWindow LegacyOptionsWindow = null;

        [Header("Module.Outfit")]
        public Outfit.OutfitWindow OutfitWindow = null;

        [Header("Module.Skills")]
        public Skills.SkillsWindow SkillsWindowPrefab = null;

        [Header("Module.Trade")]
        public Trade.NPCTradeWindow NPCTradeWindowPrefab = null;

        protected void Awake() {
            Instance = this;

            HotkeysWindow.gameObject.SetActive(true);
            HotkeysWindow.gameObject.SetActive(false);
        }

        protected void Start() {
            LoginWindow.Show();

            var gameManager = OpenTibiaUnity.GameManager;

            gameManager.onRequestHotkeysDialog.AddListener(OnRequestHotkeysDialog);
            gameManager.onRequestChatHistoryPrev.AddListener(OnRequestChatHistoryPrev);
            gameManager.onRequestChatHistoryNext.AddListener(OnRequestChatHistoryNext);
            gameManager.onRequestChatSend.AddListener(OnRequestChatSend);
            gameManager.onRequestOutfitDialog.AddListener(OnRequestOutfitDialog);
            gameManager.onRequestNPCTrade.AddListener(OnRequestNPCTrade);
            gameManager.onRequestCloseNPCTrade.AddListener(OnRequestCloseNPCTrade);
        }
        
        private void OnRequestHotkeysDialog() {
            HotkeysWindow.Open();
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
                OutfitWindow.Open();
        }
        private void OnRequestNPCTrade(string npcName, List<TradeObjectRef> buyList, List<TradeObjectRef> sellList) {
            var npcTradeWindow = OpenTibiaUnity.GameManager.GetModule<Trade.NPCTradeWindow>();
            if (!npcTradeWindow) {
                var gameWindow = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
                npcTradeWindow = gameWindow.CreateMiniWindow(NPCTradeWindowPrefab);
            }

            npcTradeWindow.Setup(npcName, buyList, sellList);
        }

        private void OnRequestCloseNPCTrade() {
            var npcTradeWindow = OpenTibiaUnity.GameManager.GetModule<Trade.NPCTradeWindow>();
            if (npcTradeWindow)
                npcTradeWindow.CloseWithoutNotifying();
        }
    }
}
