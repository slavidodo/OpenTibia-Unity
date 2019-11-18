namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private void ParseShowModalDialog(Internal.CommunicationStream message) {
            uint windowId = message.ReadUnsignedInt();
            string title = message.ReadString();
            string windowMessage = message.ReadString();

            byte buttonCount = message.ReadUnsignedByte();
            for (int i = 0; i < buttonCount; i++) {
                string buttonText = message.ReadString();
                byte buttonId = message.ReadUnsignedByte();
            }

            byte choiceCount = message.ReadUnsignedByte();
            for (int i = 0; i < choiceCount; i++) {
                string choiceText = message.ReadString();
                byte choiceId = message.ReadUnsignedByte();
            }

            byte defaultEscapeButtonId = message.ReadUnsignedByte();
            byte defaultEnterButtonId = message.ReadUnsignedByte();
            bool priority = message.ReadBoolean();
        }
    }
}
