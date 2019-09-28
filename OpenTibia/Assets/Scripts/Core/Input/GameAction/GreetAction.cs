using System;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class GreetAction : TalkActionImpl
    {
        private Creatures.Creature _nPC;

        public GreetAction(Creatures.Creature npcCreature) : base("Hi", true) {
            if (!npcCreature || !npcCreature.IsNPC)
                throw new Exception("GreetAction.GreetAction: Unknown creature or creature is not NPC.");
            _nPC = npcCreature;
        }

        public override void Perform(bool repeat = false) {
            if (IsNpcInReach(_nPC))
                base.Perform(repeat);
            else
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_NPC_TOO_FAR);
        }

        public static bool IsNpcInReach(Creatures.Creature creature) {
            var delta = OpenTibiaUnity.Player.Position - creature.Position;
            return delta.z == 0 && Math.Abs(delta.x) <= Constants.MaxNpcDistance && Math.Abs(delta.y) <= Constants.MaxNpcDistance;
        }
    }
}
