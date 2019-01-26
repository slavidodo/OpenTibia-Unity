using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
    public class ConsoleModule : Core.Components.Base.AbstractComponent
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private Sprite m_ChannelButtonActiveSprite;
        [SerializeField] private Sprite m_ChannelButtonInactiveSprite;

        [SerializeField] private RectTransform m_ConsoleMessagesContent;
        [SerializeField] private RectTransform m_ChannelsButtonsTransform;
        [SerializeField] private Button m_SoundToggleButton;
        [SerializeField] private Button m_ChatOffToggleButton;
        [SerializeField] private TMPro.TMP_InputField m_ChatInputField;
#pragma warning restore CS0649 // never assigned to

        private List<Channel> m_KnownChannels;
        private SortedDictionary<object, ChannelButton> m_ChannelButtons;

        private Channel m_ActiveChannel = null;
        private ChannelButton m_ActiveChannelButton = null;

        private SortedDictionary<object, Core.Utility.RingBuffer<ChannelMessageLabel>> m_ChannelsHistory;
        private List<string> m_TalkHistory;


        protected override void Awake() {
            base.Awake();

            m_KnownChannels = new List<Channel>();
            m_ChannelButtons = new SortedDictionary<object, ChannelButton>();
            m_ChannelsHistory = new SortedDictionary<object, Core.Utility.RingBuffer<ChannelMessageLabel>>();
            m_TalkHistory = new List<string>();

            OpenTibiaUnity.ChatStorage.onAddChannel.AddListener(OnAddChannel);
        }

        protected override void Start() {
            base.Start();

            OpenTibiaUnity.InputHandler.AddKeyUpListener((Event e, bool repeated) => {
                if (!Core.InputManagment.InputHandler.IsGameObjectHighlighted(m_ChatInputField.gameObject))
                    return;

                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    SendChannelMessage();
            });
        }

        private void SendChannelMessage() {
            var text = m_ChatInputField.text;
            if (text == null || text.Length == 0)
                return;

            m_ChatInputField.text = OpenTibiaUnity.ChatStorage.SendChannelMessage(text, m_ActiveChannel, MessageModes.None);
            m_TalkHistory.Add(text);

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(m_ChatInputField.gameObject);
            m_ChatInputField.MoveTextEnd(false);
        }

        private void OnAddChannel(Channel channel) {
            var channelButton = GetChannelButton(channel, true);

            // if there is no channel or this channel is NPC, then select it!
            if ((channel.ID.IsInt && channel.ID == ChatStorage.NpcChannelID) || m_ActiveChannel == null) {
                SelectChannelButton(channelButton);
            } else {
                channelButton.SetState(ChannelButtonState.Inactive);
                channelButton.SetImage(m_ChannelButtonInactiveSprite);
            }
        }

        private void OnAddChannelMessage(Channel channel, ChannelMessage channelMessage) {
            if (channel.ID.IsInt && (channel.ID == ChatStorage.NpcChannelID || channel.ID == ChatStorage.DebugChannelID))
                SelectChannelButton(GetChannelButton(channel, true));
            else
                GetChannelButton(channel, true); // force creation of this button //
            
            var channelMessageLabel = CreateChannelMessageLabel(channel, channelMessage);
            channelMessageLabel.gameObject.SetActive(channel == m_ActiveChannel);

            if (channel != m_ActiveChannel)
                m_ChannelButtons[channel.ID].SetState(ChannelButtonState.Flashing);
        }

        private void OnChannelButtonClicked(ChannelButton channelButton) {
            SelectChannelButton(channelButton);
        }

        private void OnCloseChannelButtonClicked() {
            if (!m_ActiveChannelButton || !m_ActiveChannel.Closable)
                return;

            if (m_ChannelsHistory.ContainsKey(m_ActiveChannel.ID)) {
                Core.Utility.RingBuffer<ChannelMessageLabel> history = m_ChannelsHistory[m_ActiveChannel.ID];
                foreach (ChannelMessageLabel message in history.ToArray())
                    Destroy(message.gameObject);
                history.RemoveAll();

                m_ChannelsHistory.Remove(m_ActiveChannel.ID);
            }

            m_ChannelButtons.Remove(m_ActiveChannel.ID);

            var rectTransform = m_ActiveChannelButton.transform as RectTransform;
            var index = rectTransform.GetSiblingIndex();

            int relativeIndex;
            if (index == 0)
                relativeIndex = 1;
            else
                relativeIndex = index - 1;
            
            var otherChannelButton = rectTransform.parent.GetChild(relativeIndex).GetComponent<ChannelButton>();

            m_ActiveChannel = null;
            m_ActiveChannelButton = null;
            SelectChannelButton(otherChannelButton);
        }

        private ChannelButton GetChannelButton(Channel channel, bool forceCreate) {
            ChannelButton channelButton;
            if (m_ChannelButtons.TryGetValue(channel.ID, out channelButton))
                return channelButton;

            if (forceCreate) {
                // avoid adding the same listener twice
                // this will cause the same message to be broadcasted twice
                if (m_KnownChannels.Find((c) => c == channel) == null) {
                    m_KnownChannels.Add(channel);
                    channel.onAddChannelMessage.AddListener(OnAddChannelMessage);
                }

                channelButton = Instantiate(OpenTibiaUnity.GameManager.ChannelButtonPrefab, m_ChannelsButtonsTransform);
                channelButton.SetText(channel.Name);
                channelButton.onClick.AddListener(OnChannelButtonClicked);
                channelButton.Channel = channel;

                m_ChannelButtons.Add(channel.ID, channelButton);
            } else {
                channelButton = null;
            }

            return channelButton;
        }

        private void SelectChannelButton(ChannelButton channelButton) {
            // this button is already selected
            if (m_ActiveChannel == channelButton.Channel) {
                return;

            // set the previous button to non-selected
            } else if (m_ActiveChannel != null) {
                m_ActiveChannelButton.SetImage(m_ChannelButtonInactiveSprite);
                m_ActiveChannelButton.SetState(ChannelButtonState.Inactive);
            }

            // set the button as selected
            m_ActiveChannel = channelButton.Channel;
            m_ActiveChannelButton = channelButton;
            m_ActiveChannelButton.SetImage(m_ChannelButtonActiveSprite);
            m_ActiveChannelButton.SetState(ChannelButtonState.Active);

            // activate the messages of this channel
            var length = m_ConsoleMessagesContent.transform.childCount;
            for (int i = 0; i < length; i++) {
                var child = m_ConsoleMessagesContent.transform.GetChild(i);
                ChannelMessageLabel channelMessageLabel = child.GetComponent<ChannelMessageLabel>();
                if (channelMessageLabel != null)
                    channelMessageLabel.gameObject.SetActive(channelMessageLabel.Channel == m_ActiveChannel);
            }

            // TODO: Update misc buttons (close, show server messages, ...)
        }

        private ChannelMessageLabel CreateChannelMessageLabel(Channel channel, ChannelMessage channelMessage) {
            Core.Utility.RingBuffer<ChannelMessageLabel> history;
            if (!m_ChannelsHistory.TryGetValue(channel.ID, out history)) {
                history = new Core.Utility.RingBuffer<ChannelMessageLabel>(1000);
                m_ChannelsHistory.Add(channel.ID, history);
            }

            var channelMessageLabel = Instantiate(OpenTibiaUnity.GameManager.ChannelMessageLabelPrefab, m_ConsoleMessagesContent);
            channelMessageLabel.Channel = channel;
            channelMessageLabel.ChannelMessage = channelMessage;
            channelMessageLabel.SetText(channelMessage.RichText);

            var removedMessage = history.AddItem(channelMessageLabel);
            if (removedMessage != null)
                Destroy(removedMessage.gameObject);

            return channelMessageLabel;
        }
    }
}
