namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal class PlayerTurn : StaticAction
    {
        private Direction m_Direction;
        internal PlayerTurn(int id, string label, uint eventMask, Direction direction) : base(id, label, eventMask, false) {
            if (direction < Direction.North || direction > Direction.West)
                throw new System.ArgumentException("PlayerTurn.PlayerTurn: Invalid direction.");

            m_Direction = direction;
        }

        public override bool Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                switch (m_Direction) {
                    case Direction.North:
                        protocolGame.SendTurnNorth();
                        break;
                    case Direction.East:
                        protocolGame.SendTurnEast();
                        break;
                    case Direction.South:
                        protocolGame.SendTurnSouth();
                        break;
                    case Direction.West:
                        protocolGame.SendTurnWest();
                        break;
                }

                return true;
            }

            return false;
        }

        public override IAction Clone() {
            return new PlayerTurn(m_ID, m_Label, m_EventMask, m_Direction);
        }
    }
}
