using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
    public class ChannelInformation
    {
        public Channel channel;
        public Core.Utils.RingBuffer<ChannelMessage> talkHistory;

        public ChannelInformation(Channel channel) {
            this.channel = channel;
            talkHistory = new Core.Utils.RingBuffer<ChannelMessage>(Constants.MaxTalkHistory);
        }
    }

    public class ConsoleModule : Core.Components.Base.Module
    {
        [SerializeField] private Sprite _channelButtonActiveSprite = null;
        [SerializeField] private Sprite _channelButtonInactiveSprite = null;
        
        [SerializeField] private RectTransform _channelsButtonsTransform = null;
        [SerializeField] private Button _toggleSound = null;
        [SerializeField] private Button _toggleChat = null;
        [SerializeField] private TMPro.TMP_InputField _chatInputField = null;
        [SerializeField] private ConsoleBuffer _consoleBuffer = null;
        [SerializeField] private Button _ignoreListButton = null;
        [SerializeField] private Button _newChannelButton = null;
        [SerializeField] private Toggle _toggleShowServerMessages = null;
        [SerializeField] private Button _buttonCloseChannel = null;
        
        private SortedDictionary<object, ChannelTab> _channelTabs = null;
        private SortedDictionary<object, ChannelInformation> _knownChannels = null;
        private Channel _activeChannel = null;
        private ChannelTab _activeChannelTab = null;
        
        protected override void Awake() {
            base.Awake();
            
            _channelTabs = new SortedDictionary<object, ChannelTab>();
            _knownChannels = new SortedDictionary<object, ChannelInformation>();

            OpenTibiaUnity.ChatStorage.onAddChannel.AddListener(OnAddChannel);
            OpenTibiaUnity.ChatStorage.onClearChannels.AddListener(OnClearChannels);

            _buttonCloseChannel.onClick.AddListener(OnCloseChannelButtonClicked);
        }

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.GameManager.onGameStart.AddListener(OnGameStart);
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utils.EventImplPriority.Default, OnKeyUp);
        }

        private void OnGameStart() {
            _chatInputField.ActivateInputField();
            _chatInputField.text = string.Empty;
        }

        private void OnKeyUp(Event e, bool repeated) {
            if (!Core.Input.InputHandler.IsGameObjectHighlighted(_chatInputField.gameObject))
                return;

            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                SendChannelMessage();
        }

        private void OnAddChannel(Channel channel) {
            // make sure we know this channel before doing anything
            _knownChannels[channel.Id] = new ChannelInformation(channel);

            // get the channel button if exists, force create if not
            var channelButton = GetChannelTab(channel, true);

            // if there is no channel or this channel is NPC, then select it!
            if (channel.Id == ChatStorage.NpcChannelId || _activeChannel == null) {
                SelectChannelButton(channelButton);
            } else {
                channelButton.SetState(ChannelButtonState.Inactive);
                channelButton.SetImage(_channelButtonInactiveSprite);
            }

            channel.onAddChannelMessage.AddListener(OnAddChannelMessage);
        }

        private void OnClearChannels() {
            foreach (var p in _channelTabs)
                Destroy(p.Value.gameObject);

            _channelTabs.Clear();
            _knownChannels.Clear();
            _activeChannel = null;
            _activeChannelTab = null;
        }
        
        private void OnAddChannelMessage(Channel channel, ChannelMessage channelMessage) {
            if (channel.Id == ChatStorage.NpcChannelId || channel.Id == ChatStorage.DebugChannelId)
                SelectChannelButton(GetChannelTab(channel, true));
            else
                GetChannelTab(channel, true);

            _knownChannels[channel.Id].talkHistory.AddItem(channelMessage);

            if (channel == _activeChannel)
                _consoleBuffer.AddChannelMessage(channelMessage);
            else
                _channelTabs[channel.Id].SetState(ChannelButtonState.Flashing);
        }

        private void OnChannelButtonClicked(ChannelTab channelTab) {
            SelectChannelButton(channelTab);
        }

        private void OnCloseChannelButtonClicked() {
            if (!_activeChannelTab || !_activeChannel.Closable)
                return;
            
            _knownChannels.Remove(_activeChannel.Id);
            _channelTabs.Remove(_activeChannel.Id);

            OpenTibiaUnity.ChatStorage.LeaveChannel(_activeChannel.Id);
            _activeChannel.onAddChannelMessage.RemoveListener(OnAddChannelMessage);

            var channelButtonRectTransform = _activeChannelTab.rectTransform;
            int index = channelButtonRectTransform.GetSiblingIndex();

            int relativeIndex;
            if (index == 0)
                relativeIndex = 1;
            else
                relativeIndex = index - 1;
            
            
            var otherChannelButton = channelButtonRectTransform.parent.GetChild(relativeIndex).GetComponent<ChannelTab>();
            Destroy(_activeChannelTab.gameObject);

            _activeChannel = null;
            _activeChannelTab = null;
            SelectChannelButton(otherChannelButton);
        }

        private void OnShowServerMessagesToggleValueChanged(bool value) {

        }

        public void SetInputText(string text) {
            _chatInputField.text = text;
            _chatInputField.MoveTextEnd(false);
        }

        public void SendChannelMessage() {
            var text = _chatInputField.text;
            if (text.Length == 0)
                return;
            
            _chatInputField.text = OpenTibiaUnity.ChatStorage.SendChannelMessage(text, _activeChannel, MessageModeType.None);
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                _chatInputField.ActivateInputField();
                _chatInputField.MoveTextEnd(false);
            });
        }
        
        private ChannelTab GetChannelTab(Channel channel, bool forceCreate) {
            ChannelTab channelTab;
            if (_channelTabs.TryGetValue(channel.Id, out channelTab))
                return channelTab;

            if (forceCreate) {
                channelTab = Instantiate(ModulesManager.Instance.ChannelTabPrefab, _channelsButtonsTransform);
                channelTab.SetText(channel.Name);
                channelTab.onClick.AddListener(OnChannelButtonClicked);
                channelTab.Channel = channel;

                _channelTabs.Add(channel.Id, channelTab);
            } else {
                channelTab = null;
            }

            return channelTab;
        }

        private void SelectChannelButton(ChannelTab channelTab) {
            // this button is already selected
            if (_activeChannel == channelTab.Channel) {
                return;

            // set the previous button to non-selected
            } else if (_activeChannel != null) {
                _activeChannelTab.SetImage(_channelButtonInactiveSprite);
                _activeChannelTab.SetState(ChannelButtonState.Inactive);
            }

            // set the button as selected
            _activeChannel = channelTab.Channel;
            _activeChannelTab = channelTab;
            _activeChannelTab.SetImage(_channelButtonActiveSprite);
            _activeChannelTab.SetState(ChannelButtonState.Active);

            _consoleBuffer.ResetTalkHistory(_knownChannels[_activeChannel.Id].talkHistory);

            _buttonCloseChannel.gameObject.SetActive(_activeChannel.Closable);
            _toggleShowServerMessages.gameObject.SetActive(_activeChannel.Closable);

            _toggleShowServerMessages.onValueChanged.RemoveListener(OnShowServerMessagesToggleValueChanged);
            //_toggleShowServerMessages.isOn = _activeChannel.ShowServerMessages;
            _toggleShowServerMessages.onValueChanged.AddListener(OnShowServerMessagesToggleValueChanged);
        }

        public void SelectChannel(Channel channel, bool forceCreate) {
            var channelButton = GetChannelTab(channel, forceCreate);
            if (channelButton)
                SelectChannelButton(channelButton);
        }
        
        public ChannelMessageContextMenu CreateChannelMessageContextMenu(ChannelMessage channelMessage, TMPro.TMP_InputField inputField) {

            var gameManager = OpenTibiaUnity.GameManager;
            var canvas = gameManager.ActiveCanvas;
            var gameObject = Instantiate(gameManager.ContextMenuBasePrefab, canvas.transform);

            var channelMessageContextMenu = gameObject.AddComponent<ChannelMessageContextMenu>();
            channelMessageContextMenu.Set(_activeChannel, channelMessage, inputField);
            return channelMessageContextMenu;
        }
    }
}
