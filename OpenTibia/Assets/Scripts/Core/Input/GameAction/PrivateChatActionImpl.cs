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

        PrivateChatActionType _actionType;
        int _channel_id;
        string _name;

        public PrivateChatActionImpl(PrivateChatActionType actionType, int channel_id, string name) {
            _actionType = actionType;
            _channel_id = channel_id;
            _name = name;
        }

        public void Perform(bool _ = false) {
            if (_actionType == PrivateChatActionType.OpenChatChannel || _name != null) {
                Performpublic(_actionType, _name);
                return;
            }

            // Todo? request name? idk about this :)
        }

        private void Performpublic(PrivateChatActionType actionType, string playerName) {
            if (!OpenTibiaUnity.GameManager.IsGameRunning)
                return;

            var chatStorage = OpenTibiaUnity.ChatStorage;
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            switch (actionType) {
                case PrivateChatActionType.OpenChatChannel: {
                    var channel = chatStorage.GetChannel(chatStorage.OwnPrivateChannel_id);
                    if (channel == null) {
                        protocolGame.SendOpenChannel();
                    } else {
                        // TODO select chat
                    }

                    break;
                }
                case PrivateChatActionType.ChatChannelInvite: {
                    if (_channel_id > -1)
                        protocolGame.SendInviteToChannel(_name, _channel_id);
                    break;
                }
                case PrivateChatActionType.ChatChannelExclude: {
                    if (_channel_id > -1)
                        protocolGame.SendExcludeFromChannel(_name, _channel_id);
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
