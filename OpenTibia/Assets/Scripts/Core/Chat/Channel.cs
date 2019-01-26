using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    public class Channel
    {
        public class ChannelMessageAddEvent : UnityEvent<Channel, ChannelMessage> { }

        protected const int MessagesSize = 50000;
        public const int MaxNameLength = 30;

        protected Utility.UnionStrInt m_ID = null;
        protected string m_Name = null;
        
        protected bool m_SendAllowed = true;
        protected bool m_Closable = true;
        protected MessageModes m_SendMode = 0;

        protected List<object> m_NicklistItems = null;
        protected List<ChannelMessage> m_Messages = null;

        public ChannelMessageAddEvent onAddChannelMessage = new ChannelMessageAddEvent();


        public string Name {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public bool SendAllowed {
            get { return m_SendAllowed; }
            set { m_SendAllowed = value; }
        }

        public bool Closable {
            get { return m_Closable; }
            set { m_Closable = value; }
        }

        public Utility.UnionStrInt ID {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public MessageModes SendMode {
            get { return m_SendMode; }
        }

        public bool CanModerate { get; set; } = false;

        public bool IsPrivate {
            get { return ChatStorage.s_IsPrivateChannel(ID); }
        }

        public Channel(Utility.UnionStrInt ID, string name, MessageModes sendMode) {
            m_ID = ID;
            m_Name = name;
            m_SendMode = sendMode;
            m_Closable = true;
            m_SendAllowed = true;
            m_NicklistItems = new List<object>();
            m_Messages = new List<ChannelMessage>(MessagesSize);
        }

        public void ClearMessages() {
            m_Messages.Clear();
        }

        public void AppendMessage(ChannelMessage message) {
            m_Messages.Add(message);
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
