using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
    internal class ChannelInformation
    {
        public Channel channel;
        public Core.Utility.RingBuffer<ChannelMessage> talkHistory;

        internal ChannelInformation(Channel channel) {
            this.channel = channel;
            talkHistory = new Core.Utility.RingBuffer<ChannelMessage>(Constants.MaxTalkHistory);
        }
    }

    internal class ConsoleModule : Core.Components.Base.Module
    {
        [SerializeField] private Sprite m_ChannelButtonActiveSprite = null;
        [SerializeField] private Sprite m_ChannelButtonInactiveSprite = null;

        [SerializeField] private RectTransform m_ConsoleMessagesContent = null;
        [SerializeField] private RectTransform m_ChannelsButtonsTransform = null;
        [SerializeField] private Button m_ToggleSound = null;
        [SerializeField] private Button m_ToggleChat = null;
        [SerializeField] private TMPro.TMP_InputField m_ChatInputField = null;
        [SerializeField] private ConsoleBuffer m_ConsoleBuffer = null;
        [SerializeField] private Button m_IgnoreListButton = null;
        [SerializeField] private Button m_NewChannelButton = null;
        [SerializeField] private Toggle m_ToggleShowServerMessages = null;
        [SerializeField] private Button m_ButtonCloseChannel = null;
        
        private SortedDictionary<object, ChannelTab> m_ChannelTabs = null;
        private SortedDictionary<object, ChannelInformation> m_KnownChannels = null;
        private Channel m_ActiveChannel = null;
        private ChannelTab m_ActiveChannelTab = null;
        
        protected override void Awake() {
            base.Awake();
            
            m_ChannelTabs = new SortedDictionary<object, ChannelTab>();
            m_KnownChannels = new SortedDictionary<object, ChannelInformation>();

            OpenTibiaUnity.ChatStorage.onAddChannel.AddListener(OnAddChannel);
            OpenTibiaUnity.ChatStorage.onClearChannels.AddListener(OnClearChannels);

            m_ButtonCloseChannel.onClick.AddListener(OnCloseChannelButtonClicked);
        }

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.GameManager.onGameStart.AddListener(OnGameStart);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utility.EventImplPriority.Default, OnKeyUp);
        }

        private void OnGameStart() {
            m_ChatInputField.ActivateInputField();
            m_ChatInputField.text = string.Empty;
        }

        private void OnKeyUp(Event e, bool repeated) {
            if (!Core.Input.InputHandler.IsGameObjectHighlighted(m_ChatInputField.gameObject))
                return;

            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                SendChannelMessage();
        }

        private void OnAddChannel(Channel channel) {
            // make sure we know this channel before doing anything
            m_KnownChannels[channel.ID] = new ChannelInformation(channel);

            // get the channel button if exists, force create if not
            var channelButton = GetChannelTab(channel, true);

            // if there is no channel or this channel is NPC, then select it!
            if (channel.ID == ChatStorage.NpcChannelID || m_ActiveChannel == null) {
                SelectChannelButton(channelButton);
            } else {
                channelButton.SetState(ChannelButtonState.Inactive);
                channelButton.SetImage(m_ChannelButtonInactiveSprite);
            }

            channel.onAddChannelMessage.AddListener(OnAddChannelMessage);
        }

        private void OnClearChannels() {
            foreach (var p in m_ChannelTabs)
                Destroy(p.Value.gameObject);

            m_ChannelTabs.Clear();
            m_KnownChannels.Clear();
            m_ActiveChannel = null;
            m_ActiveChannelTab = null;
        }
        
        private void OnAddChannelMessage(Channel channel, ChannelMessage channelMessage) {
            if (channel.ID == ChatStorage.NpcChannelID || channel.ID == ChatStorage.DebugChannelID)
                SelectChannelButton(GetChannelTab(channel, true));
            else
                GetChannelTab(channel, true);

            m_KnownChannels[channel.ID].talkHistory.AddItem(channelMessage);

            if (channel == m_ActiveChannel)
                m_ConsoleBuffer.AddChannelMessage(channelMessage);
            else
                m_ChannelTabs[channel.ID].SetState(ChannelButtonState.Flashing);
        }

        private void OnChannelButtonClicked(ChannelTab channelTab) {
            SelectChannelButton(channelTab);
        }

        private void OnCloseChannelButtonClicked() {
            if (!m_ActiveChannelTab || !m_ActiveChannel.Closable)
                return;
            
            m_KnownChannels.Remove(m_ActiveChannel.ID);
            m_ChannelTabs.Remove(m_ActiveChannel.ID);

            OpenTibiaUnity.ChatStorage.RemoveChannel(m_ActiveChannel.ID);
            m_ActiveChannel.onAddChannelMessage.RemoveListener(OnAddChannelMessage);

            var channelButtonRectTransform = m_ActiveChannelTab.rectTransform;
            int index = channelButtonRectTransform.GetSiblingIndex();

            int relativeIndex;
            if (index == 0)
                relativeIndex = 1;
            else
                relativeIndex = index - 1;
            
            
            var otherChannelButton = channelButtonRectTransform.parent.GetChild(relativeIndex).GetComponent<ChannelTab>();
            Destroy(m_ActiveChannelTab.gameObject);

            m_ActiveChannel = null;
            m_ActiveChannelTab = null;
            SelectChannelButton(otherChannelButton);
        }

        private void OnShowServerMessagesToggleValueChanged(bool value) {

        }

        internal void SetInputText(string text) {
            m_ChatInputField.text = text;
            m_ChatInputField.MoveTextEnd(false);
        }

        internal void SendChannelMessage() {
            var text = m_ChatInputField.text;
            if (text.Length == 0)
                return;

            m_ChatInputField.text = OpenTibiaUnity.ChatStorage.SendChannelMessage(text, m_ActiveChannel, MessageModeType.None);
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                m_ChatInputField.ActivateInputField();
                m_ChatInputField.MoveTextEnd(false);
            });
        }
        
        private ChannelTab GetChannelTab(Channel channel, bool forceCreate) {
            ChannelTab channelTab;
            if (m_ChannelTabs.TryGetValue(channel.ID, out channelTab))
                return channelTab;

            if (forceCreate) {
                channelTab = Instantiate(ModulesManager.Instance.ChannelTabPrefab, m_ChannelsButtonsTransform);
                channelTab.SetText(channel.Name);
                channelTab.onClick.AddListener(OnChannelButtonClicked);
                channelTab.Channel = channel;

                m_ChannelTabs.Add(channel.ID, channelTab);
            } else {
                channelTab = null;
            }

            return channelTab;
        }

        private void SelectChannelButton(ChannelTab channelTab) {
            // this button is already selected
            if (m_ActiveChannel == channelTab.Channel) {
                return;

            // set the previous button to non-selected
            } else if (m_ActiveChannel != null) {
                m_ActiveChannelTab.SetImage(m_ChannelButtonInactiveSprite);
                m_ActiveChannelTab.SetState(ChannelButtonState.Inactive);
            }

            // set the button as selected
            m_ActiveChannel = channelTab.Channel;
            m_ActiveChannelTab = channelTab;
            m_ActiveChannelTab.SetImage(m_ChannelButtonActiveSprite);
            m_ActiveChannelTab.SetState(ChannelButtonState.Active);

            m_ConsoleBuffer.ResetTalkHistory(m_KnownChannels[m_ActiveChannel.ID].talkHistory);

            m_ButtonCloseChannel.gameObject.SetActive(m_ActiveChannel.Closable);
            m_ToggleShowServerMessages.gameObject.SetActive(m_ActiveChannel.Closable);

            m_ToggleShowServerMessages.onValueChanged.RemoveListener(OnShowServerMessagesToggleValueChanged);
            //m_ToggleShowServerMessages.isOn = m_ActiveChannel.ShowServerMessages;
            m_ToggleShowServerMessages.onValueChanged.AddListener(OnShowServerMessagesToggleValueChanged);
        }

        internal void SelectChannel(Channel channel, bool forceCreate) {
            var channelButton = GetChannelTab(channel, forceCreate);
            if (channelButton)
                SelectChannelButton(channelButton);
        }
        
        internal ChannelMessageContextMenu CreateChannelMessageContextMenu(ChannelMessage channelMessage, TMPro.TMP_InputField inputField) {

            var gameManager = OpenTibiaUnity.GameManager;
            var canvas = gameManager.ActiveCanvas;
            var gameObject = Instantiate(gameManager.ContextMenuBasePrefab, canvas.transform);

            var channelMessageContextMenu = gameObject.AddComponent<ChannelMessageContextMenu>();
            channelMessageContextMenu.Set(m_ActiveChannel, channelMessage, inputField);
            return channelMessageContextMenu;
        }
    }
}
