namespace OpenTibiaUnity.Core.InputManagment.GameAction
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
            // TODO
        }
    }
}
