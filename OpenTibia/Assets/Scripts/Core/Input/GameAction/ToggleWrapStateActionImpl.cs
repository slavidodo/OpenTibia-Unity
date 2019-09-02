namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class ToggleWrapStateActionImpl : IActionImpl
    {

        UnityEngine.Vector3Int _absolute;
        Appearances.AppearanceType _appearanceType;
        int _stackPos;

        public ToggleWrapStateActionImpl(UnityEngine.Vector3Int absolute, Appearances.ObjectInstance @object, int stackPos) {
            _absolute = absolute;
            _appearanceType = @object?.Type ?? throw new System.ArgumentNullException("ToggleWrapStateActionImpl.ToggleWrapStateActionImpl: Invalid object.");
            _stackPos = stackPos;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendToggleWrapState(_absolute, _appearanceType.Id, _stackPos);
        }
    }
}
