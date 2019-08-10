using System;
using System.Collections.Generic;
using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Game;
using OpenTibiaUnity.Core.Trade;
using UnityEngine;

namespace OpenTibiaUnity.Modules
{
    internal class ModulesManager : MonoBehaviour
    {
        internal static ModulesManager Instance { get; private set; }
        
        [Header("Module.Battle")]
        [SerializeField] internal Battle.BattleWindow BattleWindowPrefab = null;

        [Header("Module.Console")]
        [SerializeField] internal Console.ConsoleBuffer ConsoleBufferPrefab = null;
        [SerializeField] internal Console.ChannelTab ChannelTabPrefab = null;

        [Header("Module.Container")]
        [SerializeField] internal Container.ContainerWindow ContainerWindowPrefab = null;
        [SerializeField] internal Container.ItemView ItemViewPrefab = null;

        [Header("Module.Hotkeys")]
        [SerializeField] internal Hotkeys.HotkeyActionPanel HotkeysActionPanelPrefab = null;
        [SerializeField] internal Hotkeys.HotkeysWindow HotkeysWindow = null;

        [Header("Module.Login")]
        [SerializeField] internal Login.CharactersWindow CharactersWindow = null;
        [SerializeField] internal Login.LoginWindow LoginWindow = null;
        [SerializeField] internal Login.AuthenticatorWindow AuthenticatorWindow = null;
        [SerializeField] internal Login.CharacterPanel CharacterPanelPrefab = null;

        [Header("Module.Options")]
        [SerializeField] internal Options.LegacyGeneralOptionsWindow LegacyGeneralOptionsWindow = null;
        [SerializeField] internal Options.LegacyOptionsWindow LegacyOptionsWindow = null;

        [Header("Module.Outfit")]
        [SerializeField] internal Outfit.OutfitWindow OutfitWindow = null;

        [Header("Module.Skills")]
        [SerializeField] internal Skills.SkillsWindow SkillsWindowPrefab = null;

        [Header("Module.Trade")]
        [SerializeField] internal Trade.NPCTradeWindow NPCTradeWindowPrefab = null;

        protected void Awake() {
            Instance = this;
        }

        protected void Start() {
            LoginWindow.ShowWindow();

            var gameManager = OpenTibiaUnity.GameManager;

            gameManager.onRequestShowOptionsHotkey.AddListener(OnRequestShowOptionsHotkey);
            gameManager.onRequestChatSend.AddListener(OnRequestChatSend);
            gameManager.onRequestOutfitDialog.AddListener(OnRequestOutfitDialog);
            gameManager.onRequestNPCTrade.AddListener(OnRequestNPCTrade);
            gameManager.onRequestCloseNPCTrade.AddListener(OnRequestCloseNPCTrade);
        }
        
        private void OnRequestShowOptionsHotkey() {
            HotkeysWindow.OpenWindow();
        }

        private void OnRequestChatSend(string text, bool autoSend, int channelID) {
            var chatModule = OpenTibiaUnity.GameManager.GetModule<Console.ConsoleModule>();
            if (!chatModule)
                return;

            if (channelID != -1)
                chatModule.SelectChannel(OpenTibiaUnity.ChatStorage.GetChannel(channelID), true);

            chatModule.SetInputText(text);
            if (autoSend)
                chatModule.SendChannelMessage();
        }

        private void OnRequestOutfitDialog(AppearanceInstance outfit, AppearanceInstance mountOutfit, List<ProtocolOutfit> outfits, List<ProtocolMount> mounts) {
            if (OutfitWindow.UpdateProperties(outfit, mountOutfit, outfits, mounts))
                OutfitWindow.OpenWindow();
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
                Destroy(npcTradeWindow.gameObject);
        }
    }
}
