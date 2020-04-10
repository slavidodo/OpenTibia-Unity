namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public class PlayerTurn : StaticAction
    {
        private Direction _direction;
        public PlayerTurn(int id, string label, InputEvent eventMask, Direction direction) : base(id, label, eventMask, false) {
            if (direction < Direction.North || direction > Direction.West)
                throw new System.ArgumentException("PlayerTurn.PlayerTurn: Invalid direction.");

            _direction = direction;
        }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                protocolGame.SendTurn(_direction);
                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerTurn(_id, _label, _eventMask, _direction);
        }
    }
}
