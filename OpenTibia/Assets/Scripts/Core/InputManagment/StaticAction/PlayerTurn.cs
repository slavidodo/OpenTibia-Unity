namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public class PlayerTurn : StaticAction
    {
        private Directions m_Direction;
        public PlayerTurn(int id, string label, uint eventMask, Directions direction) : base(id, label, eventMask, false) {
            if (direction < Directions.North || direction > Directions.West)
                throw new System.ArgumentException("PlayerTurn.PlayerTurn: Invalid direction.");

            m_Direction = direction;
        }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                switch (m_Direction) {
                    case Directions.North:
                        protocolGame.SendTurnNorth();
                        break;
                    case Directions.East:
                        protocolGame.SendTurnEast();
                        break;
                    case Directions.South:
                        protocolGame.SendTurnSouth();
                        break;
                    case Directions.West:
                        protocolGame.SendTurnWest();
                        break;
                }

                return true;
            }

            return false;
        }
    }
}
