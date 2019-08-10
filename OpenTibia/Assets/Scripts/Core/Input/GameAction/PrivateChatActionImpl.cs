namespace OpenTibiaUnity.Core.Input.GameAction
{
    public enum PrivateChatActionType
    {
        OpenChatChannel = 0,
        ChatChannelInvite = 1,
        ChatChannelExclude = 2,
        OpenMessageChannel = 3,
        
    }

    public class PrivateChatActionImpl : IActionImpl
    {
        public const int ChatChannelNoChannel = -1;

        PrivateChatActionType m_ActionType;
        int m_ChannelID;
        string m_Name;

        public PrivateChatActionImpl(PrivateChatActionType actionType, int channelID, string name) {
            m_ActionType = actionType;
            m_ChannelID = channelID;
            m_Name = name;
        }

        public void Perform(bool _ = false) {
            if (m_ActionType == PrivateChatActionType.OpenChatChannel || m_Name != null) {
                PerformInternal(m_ActionType, m_Name);
                return;
            }

            // Todo? request name? idk about this :)
        }

        private void PerformInternal(PrivateChatActionType actionType, string playerName) {
            if (!OpenTibiaUnity.GameManager.IsGameRunning)
                return;

            var chatStorage = OpenTibiaUnity.ChatStorage;
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            switch (actionType) {
                case PrivateChatActionType.OpenChatChannel: {
                    var channel = chatStorage.GetChannel(chatStorage.OwnPrivateChannelID);
                    if (channel == null) {
                        protocolGame.SendOpenChannel();
                    } else {
                        // TODO select chat
                    }

                    break;
                }
                case PrivateChatActionType.ChatChannelInvite: {
                    if (m_ChannelID > -1)
                        protocolGame.SendInviteToChannel(m_Name, m_ChannelID);
                    break;
                }
                case PrivateChatActionType.ChatChannelExclude: {
                    if (m_ChannelID > -1)
                        protocolGame.SendExcludeFromChannel(m_Name, m_ChannelID);
                    break;
                }
                case PrivateChatActionType.OpenMessageChannel: {
                    var channel = chatStorage.GetChannel(playerName);
                    if (channel == null) {
                        protocolGame.SendPrivateChannel(playerName);
                    } else {
                        // TODO select chat
                    }

                    break;
                }
            }
        }
    }
}
