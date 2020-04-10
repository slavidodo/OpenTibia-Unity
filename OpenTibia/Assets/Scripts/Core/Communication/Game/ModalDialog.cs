namespace OpenTibiaUnity.Core.Communication.Game
{
    public struct ProtocolModalDialogEntity
    {
        public string Text;
        public byte Id;
    }

    public struct ProtocolModalDialog
    {
        public uint Id;
        public string Title;
        public string Message;
        public byte DefaultEnterButton;
        public byte DefaultEscapeButton;
        public bool Priority;

        public ProtocolModalDialogEntity[] Buttons;
        public ProtocolModalDialogEntity[] Choices;
    }

    public partial class ProtocolGame
    {
        private void ParseShowModalDialog(Internal.CommunicationStream message) {
            var modalDialog = new ProtocolModalDialog();

            modalDialog.Id = message.ReadUnsignedInt();
            modalDialog.Title = message.ReadString();
            modalDialog.Message = message.ReadString();

            byte buttonCount = message.ReadUnsignedByte();
            modalDialog.Buttons = new ProtocolModalDialogEntity[buttonCount];
            for (int i = 0; i < buttonCount; i++) {
                string text = message.ReadString();
                byte id = message.ReadUnsignedByte();
                modalDialog.Buttons[i] = new ProtocolModalDialogEntity() {
                    Id = id,
                    Text = text
                };
            }

            byte choiceCount = message.ReadUnsignedByte();
            modalDialog.Choices = new ProtocolModalDialogEntity[choiceCount];
            for (int i = 0; i < choiceCount; i++) {
                string text = message.ReadString();
                byte id = message.ReadUnsignedByte();
                modalDialog.Choices[i] = new ProtocolModalDialogEntity() {
                    Id = id,
                    Text = text
                };
            }

            modalDialog.DefaultEnterButton = message.ReadUnsignedByte();
            modalDialog.DefaultEscapeButton = message.ReadUnsignedByte();
            modalDialog.Priority = message.ReadBoolean();

            OpenTibiaUnity.GameManager.onRequestModalDialog.Invoke(modalDialog);
        }
    }
}
