namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class ShowDialog : StaticAction
    {
        DialogType m_DialogType;

        public ShowDialog(int id, string label, uint eventMask, DialogType dialogType) : base(id, label, eventMask, false) {
            m_DialogType = dialogType;
        }

        public override bool Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            switch (m_DialogType) {
                case DialogType.CharacterOutfit:
                    if (!!protocolGame && protocolGame.IsGameRunning)
                        protocolGame.SendGetOutfit();
                    break;
                case DialogType.ChatChannelSelection:
                    if (!!protocolGame && protocolGame.IsGameRunning)
                        protocolGame.SendGetChannels();
                    break;
                case DialogType.HelpQuestLog:
                    if (!!protocolGame && protocolGame.IsGameRunning)
                        protocolGame.SendGetQuestLog();
                    break;
            }

            return true;
        }
    }
}
