namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        public void ParseCloseImbuingDialog(Internal.CommunicationStream message) {
            // TODO
        }

        public void ParseShowMessageDialog(Internal.CommunicationStream message) {
            var type = message.ReadEnum<MessageDialogType>();
            string content = message.ReadString();
        }
    }
}
