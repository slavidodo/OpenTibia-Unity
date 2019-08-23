namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class ToggleAttackTargetActionImpl : IActionImpl
    {
        Creatures.Creature _creature;
        bool _send;

        public ToggleAttackTargetActionImpl(Creatures.Creature creature, bool send) {
            _creature = creature;
            _send = send;
        }

        public void Perform(bool _ = false) {
            OpenTibiaUnity.CreatureStorage.ToggleAttackTarget(_creature, _send);
        }

    }
}
