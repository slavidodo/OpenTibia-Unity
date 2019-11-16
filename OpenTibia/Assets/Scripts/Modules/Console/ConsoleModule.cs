using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
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
        private Core.Utils.RingBuffer<string> _talkHistory = null;
        private Channel _activeChannel = null;
        private ChannelTab _activeChannelTab = null;
        private int _historyIndex = 0;

        protected override void Awake() {
            base.Awake();
            
            _channelTabs = new SortedDictionary<object, ChannelTab>();
            _talkHistory = new Core.Utils.RingBuffer<string>(200);

            OpenTibiaUnity.ChatStorage.onAddChannel.AddListener(OnAddChannel);
            OpenTibiaUnity.ChatStorage.onClearChannels.AddListener(OnClearChannels);

            OpenTibiaUnity.GameManager.onGameStart.AddListener(OnGameStart);
        }

        protected override void Start() {
            base.Start();

            _buttonCloseChannel.onClick.AddListener(OnCloseChannelButtonClicked);
        }

        private void OnGameStart() {
            _chatInputField.ActivateInputField();
            _chatInputField.text = string.Empty;
        }

        private void OnAddChannel(Channel channel) {
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
            _activeChannel = null;
            _activeChannelTab = null;
        }
        
        private void OnAddChannelMessage(Channel channel, ChannelMessage channelMessage) {
            if (channel.Id == ChatStorage.NpcChannelId || channel.Id == ChatStorage.DebugChannelId)
                SelectChannelButton(GetChannelTab(channel, true));
            else
                GetChannelTab(channel, true);

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

        public void OnChatHistory(int offset) {
            int length = _talkHistory.Length;
            if (length < 1)
                _historyIndex = -1;
            else
                _historyIndex = Mathf.Clamp(_historyIndex + offset, 0, length);

            string text;
            if (_historyIndex >= 0 && _historyIndex < length)
                text = _talkHistory.GetItemAt(_historyIndex);
            else
                text = string.Empty;

            SetInputText(text);
        }

        public void SetNextMessage() {
            if (_historyIndex == _talkHistory.Length)
                return;

            SetInputText(_talkHistory.GetItemAt(_historyIndex++));
        }

        public void SetInputText(string text) {
            _chatInputField.text = text;
            _chatInputField.MoveTextEnd(false);
        }

        public void SendChannelMessage() {
            var text = _chatInputField.text;
            if (text.Length != 0) {
                _talkHistory.AddItem(text);
                _historyIndex = _talkHistory.Length;
                _chatInputField.text = OpenTibiaUnity.ChatStorage.SendChannelMessage(text, _activeChannel, MessageModeType.None);

                OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                    _chatInputField.ActivateInputField();
                    _chatInputField.MoveTextEnd(false);
                });
            }
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

            _consoleBuffer.ResetChannelHistory(_activeChannel.History);

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
