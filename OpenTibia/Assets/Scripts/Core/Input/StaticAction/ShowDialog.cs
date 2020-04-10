namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class ShowDialog : StaticAction
    {
        private DialogType _dialogType;

        public ShowDialog(int id, string label, InputEvent eventMask, DialogType dialogType) : base(id, label, eventMask, false) {
            _dialogType = dialogType;
        }

        public override bool Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;

            switch (_dialogType) {
                case DialogType.OptionsHotkey:
                    OpenTibiaUnity.GameManager.onRequestHotkeysDialog.Invoke();
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
            return new ShowDialog(_id, _label, _eventMask, _dialogType);
        }
    }
}
