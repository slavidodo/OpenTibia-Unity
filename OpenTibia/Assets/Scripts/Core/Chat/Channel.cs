using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    public class Channel
    {
        public class ChannelMessageAddEvent : UnityEvent<Channel, ChannelMessage> { }

        protected const int MessagesSize = 200;
        public const int MaxNameLength = 30;

        protected Utils.UnionStrInt _id = null;
        protected string _name = null;
        
        protected bool _sendAllowed = true;
        protected bool _closable = true;
        protected MessageModeType _sendMode = 0;

        protected List<object> _nicklistItems = null;
        protected Utils.RingBuffer<ChannelMessage> _history = null;

        public ChannelMessageAddEvent onAddChannelMessage = new ChannelMessageAddEvent();

        public Utils.UnionStrInt Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public bool SendAllowed { get => _sendAllowed; set => _sendAllowed = value; }
        public bool Closable { get => _closable; set => _closable = value; }
        public MessageModeType SendMode { get => _sendMode; }
        public bool CanModerate { get; set; } = false;

        public Utils.RingBuffer<ChannelMessage> History { get => _history; }

        public bool IsPrivate { get => ChatStorage.s_IsPrivateChannel(_id); }

        public Channel(Utils.UnionStrInt id, string name, MessageModeType sendMode) {
            _id = id;
            _name = name;
            _sendMode = sendMode;
            _closable = true;
            _sendAllowed = true;
            _nicklistItems = new List<object>();
            _history = new Utils.RingBuffer<ChannelMessage>(MessagesSize);
        }

        public void ClearMessages() {
            _history.RemoveAll();
        }

        public void AppendMessage(ChannelMessage message) {
            _history.AddItem(message);
            onAddChannelMessage.Invoke(this, message);
        }

        public void PlayerJoined(string name) {
            // TODO
        }

        public void PlayerLeft(string name) {
            // TODO
        }

        public void PlayerInvited(string name) {
            // TODO
        }

        public void PlayerExcluded(string name) {
            // TODO
        }

        public void PlayerPending(string name) {
            // TODO
        }
    }
}
