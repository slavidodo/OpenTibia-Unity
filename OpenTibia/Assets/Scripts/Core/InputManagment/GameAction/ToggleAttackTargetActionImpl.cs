namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class ToggleAttackTargetActionImpl : IActionImpl
    {
        Creatures.Creature m_Creature;
        bool m_Send;

        public ToggleAttackTargetActionImpl(Creatures.Creature creature, bool send) {
            m_Creature = creature;
            m_Send = send;
        }

        public void Perform(bool _ = false) {
            OpenTibiaUnity.CreatureStorage.ToggleAttackTarget(m_Creature, m_Send);
        }

    }
}
