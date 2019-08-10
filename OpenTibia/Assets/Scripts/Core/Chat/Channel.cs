using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    internal class Channel
    {
        internal class ChannelMessageAddEvent : UnityEvent<Channel, ChannelMessage> { }

        protected const int MessagesSize = 50000;
        internal const int MaxNameLength = 30;

        protected Utility.UnionStrInt m_ID = null;
        protected string m_Name = null;
        
        protected bool m_SendAllowed = true;
        protected bool m_Closable = true;
        protected MessageModeType m_SendMode = 0;

        protected List<object> m_NicklistItems = null;
        protected List<ChannelMessage> m_Messages = null;

        internal ChannelMessageAddEvent onAddChannelMessage = new ChannelMessageAddEvent();


        internal string Name {
            get { return m_Name; }
            set { m_Name = value; }
        }

        internal bool SendAllowed {
            get { return m_SendAllowed; }
            set { m_SendAllowed = value; }
        }

        internal bool Closable {
            get { return m_Closable; }
            set { m_Closable = value; }
        }

        internal Utility.UnionStrInt ID {
            get { return m_ID; }
            set { m_ID = value; }
        }

        internal MessageModeType SendMode {
            get { return m_SendMode; }
        }

        internal bool CanModerate { get; set; } = false;

        internal bool IsPrivate {
            get { return ChatStorage.s_IsPrivateChannel(ID); }
        }

        internal Channel(Utility.UnionStrInt ID, string name, MessageModeType sendMode) {
            m_ID = ID;
            m_Name = name;
            m_SendMode = sendMode;
            m_Closable = true;
            m_SendAllowed = true;
            m_NicklistItems = new List<object>();
            m_Messages = new List<ChannelMessage>(MessagesSize);
        }

        internal void ClearMessages() {
            m_Messages.Clear();
        }

        internal void AppendMessage(ChannelMessage message) {
            m_Messages.Add(message);
            onAddChannelMessage.Invoke(this, message);
        }

        internal void PlayerJoined(string name) {
            // TODO
        }

        internal void PlayerLeft(string name) {
            // TODO
        }

        internal void PlayerInvited(string name) {
            // TODO
        }

        internal void PlayerExcluded(string name) {
            // TODO
        }

        internal void PlayerPending(string name) {
            // TODO
        }
    }
}
