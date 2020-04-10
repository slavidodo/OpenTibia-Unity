namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public sealed class PlayerMove : StaticAction
    {
        private int _deltaX;
        private int _deltaY;
        private Direction _direction;

        public PlayerMove(int id, string label, InputEvent eventMask, Direction direction) : base(id, label, eventMask, false) {
            _direction = direction;
            switch (direction) {
                case Direction.North:
                    _deltaX = 0;
                    _deltaY = -1;
                    break;
                case Direction.East:
                    _deltaX = 1;
                    _deltaY = 0;
                    break;
                case Direction.South:
                    _deltaX = 0;
                    _deltaY = 1;
                    break;
                case Direction.West:
                    _deltaX = -1;
                    _deltaY = 0;
                    break;
                case Direction.NorthEast:
                    _deltaX = 1;
                    _deltaY = -1;
                    break;
                case Direction.SouthEast:
                    _deltaX = 1;
                    _deltaY = 1;
                    break;
                case Direction.SouthWest:
                    _deltaX = -1;
                    _deltaY = 1;
                    break;
                case Direction.NorthWest:
                    _deltaX = -1;
                    _deltaY = -1;
                    break;
                case Direction.Stop:
                    _deltaX = 0;
                    _deltaY = 0;
                    break;
                default: throw new System.ArgumentException("PlayerMove.PlayerMove: Invalid movement direction: " + direction + ".");
            }
        }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                if (_deltaX == 0 && _deltaY == 0) {
                    protocolGame.SendStop();
                } else {
                    var player = OpenTibiaUnity.Player;

                    var position = player.AnticipatedPosition;
                    var forceDiagonal = System.Math.Abs(_deltaX) + System.Math.Abs(_deltaY) > 1;
                    player.StartAutowalk(position.x + _deltaX, position.y + _deltaY, position.z, forceDiagonal, true);
                }

                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerMove(_id, _label, _eventMask, _direction);
        }
    }
}
