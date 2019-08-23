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

    public class PartyActionImpl : IActionImpl
    {
        Creatures.Creature _creature;
        PartyActionType _actionType;

        public PartyActionImpl(PartyActionType actionType, Creatures.Creature creature) {
            _actionType = actionType;
            if (actionType == PartyActionType.Join || actionType == PartyActionType.Invite || actionType == PartyActionType.Exclude || actionType == PartyActionType.PassLeadership || actionType == PartyActionType.JoinAggression)
                _creature = creature ?? throw new System.ArgumentNullException("PartyActionImpl.PartyActionImpl: invalid argument (null passed to creature).");
            else
                _creature = creature;
        }

        public void Perform(bool _ = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            switch (_actionType) {
                case PartyActionType.Join:
                    protocolGame.SendJoinParty(_creature.Id);
                    break;
                case PartyActionType.Leave:
                    protocolGame.SendLeaveParty();
                    break;
                case PartyActionType.Invite:
                    protocolGame.SendInviteToParty(_creature.Id);
                    break;
                case PartyActionType.Exclude:
                    protocolGame.SendRevokeInvitation(_creature.Id);
                    break;
                case PartyActionType.PassLeadership:
                    protocolGame.SendPassLeadership(_creature.Id);
                    break;
                case PartyActionType.EnableSharedExperience:
                    protocolGame.SendShareExperience(true);
                    break;
                case PartyActionType.DisableSharedExperience:
                    protocolGame.SendShareExperience(false);
                    break;
                case PartyActionType.JoinAggression:
                    protocolGame.SendJoinAggression(_creature.Id);
                    break;
            }
        }
    }
}
