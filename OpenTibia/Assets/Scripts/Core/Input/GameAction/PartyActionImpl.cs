namespace OpenTibiaUnity.Core.Input.GameAction
{
    public enum PartyActionType
    {
        Join = 0,
        Leave = 1,
        Invite = 2,
        Exclude = 3,
        PassLeadership = 4,
        EnableSharedExperience = 5,
        DisableSharedExperience = 6,
        JoinAggression = 7,
    }

    internal class PartyActionImpl : IActionImpl
    {
        Creatures.Creature m_Creature;
        PartyActionType m_ActionType;

        internal PartyActionImpl(PartyActionType actionType, Creatures.Creature creature) {
            m_ActionType = actionType;
            if (actionType == PartyActionType.Join || actionType == PartyActionType.Invite || actionType == PartyActionType.Exclude || actionType == PartyActionType.PassLeadership || actionType == PartyActionType.JoinAggression)
                m_Creature = creature ?? throw new System.ArgumentNullException("PartyActionImpl.PartyActionImpl: invalid argument (null passed to creature).");
            else
                m_Creature = creature;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            switch (m_ActionType) {
                case PartyActionType.Join:
                    protocolGame.SendJoinParty(m_Creature.ID);
                    break;
                case PartyActionType.Leave:
                    protocolGame.SendLeaveParty();
                    break;
                case PartyActionType.Invite:
                    protocolGame.SendInviteToParty(m_Creature.ID);
                    break;
                case PartyActionType.Exclude:
                    protocolGame.SendRevokeInvitation(m_Creature.ID);
                    break;
                case PartyActionType.PassLeadership:
                    protocolGame.SendPassLeadership(m_Creature.ID);
                    break;
                case PartyActionType.EnableSharedExperience:
                    protocolGame.SendShareExperience(true);
                    break;
                case PartyActionType.DisableSharedExperience:
                    protocolGame.SendShareExperience(false);
                    break;
                case PartyActionType.JoinAggression:
                    protocolGame.SendJoinAggression(m_Creature.ID);
                    break;
            }
        }
    }
}
