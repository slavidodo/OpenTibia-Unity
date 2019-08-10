namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class ShowDialog : StaticAction
    {
        private DialogType m_DialogType;

        internal ShowDialog(int id, string label, uint eventMask, DialogType dialogType) : base(id, label, eventMask, false) {
            m_DialogType = dialogType;
        }

        public override bool Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            switch (m_DialogType) {
                case DialogType.OptionsHotkey:
                    OpenTibiaUnity.GameManager.onRequestShowOptionsHotkey.Invoke();
                    break;
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

        public override IAction Clone() {
            return new ShowDialog(m_ID, m_Label, m_EventMask, m_DialogType);
        }
    }
}
