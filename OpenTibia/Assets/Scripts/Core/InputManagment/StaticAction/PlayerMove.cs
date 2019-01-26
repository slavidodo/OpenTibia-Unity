namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public sealed class PlayerMove : StaticAction
    {
        private int m_DeltaX = 0;
        private int m_DeltaY = 0;

        public PlayerMove(int id, string label, uint eventMask, Directions direction) : base(id, label, eventMask, false) {
            switch (direction) {
                case Directions.North:
                    m_DeltaX = 0;
                    m_DeltaY = -1;
                    break;
                case Directions.East:
                    m_DeltaX = 1;
                    m_DeltaY = 0;
                    break;
                case Directions.South:
                    m_DeltaX = 0;
                    m_DeltaY = 1;
                    break;
                case Directions.West:
                    m_DeltaX = -1;
                    m_DeltaY = 0;
                    break;
                case Directions.NorthEast:
                    m_DeltaX = 1;
                    m_DeltaY = -1;
                    break;
                case Directions.SouthEast:
                    m_DeltaX = 1;
                    m_DeltaY = 1;
                    break;
                case Directions.SouthWest:
                    m_DeltaX = -1;
                    m_DeltaY = 1;
                    break;
                case Directions.NorthWest:
                    m_DeltaX = -1;
                    m_DeltaY = -1;
                    break;
                case Directions.Stop:
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
    }
}
