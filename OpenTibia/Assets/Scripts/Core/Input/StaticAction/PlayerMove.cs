namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal sealed class PlayerMove : StaticAction
    {
        private int m_DeltaX;
        private int m_DeltaY;
        private Direction m_Direction;

        internal PlayerMove(int id, string label, uint eventMask, Direction direction) : base(id, label, eventMask, false) {
            m_Direction = direction;
            switch (direction) {
                case Direction.North:
                    m_DeltaX = 0;
                    m_DeltaY = -1;
                    break;
                case Direction.East:
                    m_DeltaX = 1;
                    m_DeltaY = 0;
                    break;
                case Direction.South:
                    m_DeltaX = 0;
                    m_DeltaY = 1;
                    break;
                case Direction.West:
                    m_DeltaX = -1;
                    m_DeltaY = 0;
                    break;
                case Direction.NorthEast:
                    m_DeltaX = 1;
                    m_DeltaY = -1;
                    break;
                case Direction.SouthEast:
                    m_DeltaX = 1;
                    m_DeltaY = 1;
                    break;
                case Direction.SouthWest:
                    m_DeltaX = -1;
                    m_DeltaY = 1;
                    break;
                case Direction.NorthWest:
                    m_DeltaX = -1;
                    m_DeltaY = -1;
                    break;
                case Direction.Stop:
                    m_DeltaX = 0;
                    m_DeltaY = 0;
                    break;
                default: throw new System.ArgumentException("PlayerMove.PlayerMove: Invalid movement direction: " + direction + ".");
            }
        }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                if (m_DeltaX == 0 && m_DeltaY == 0) {
                    protocolGame.SendStop();
                } else {
                    var player = OpenTibiaUnity.Player;

                    var position = player.AnticipatedPosition;
                    var forceDiagonal = System.Math.Abs(m_DeltaX) + System.Math.Abs(m_DeltaY) > 1;
                    player.StartAutowalk(position.x + m_DeltaX, position.y + m_DeltaY, position.z, forceDiagonal, true);
                }

                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerMove(m_ID, m_Label, m_EventMask, m_Direction);
        }
    }
}
