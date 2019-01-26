namespace OpenTibiaUnity.Modules.BodyContainerView_Combat
{
    public interface ICombatViewWidget
    {
        void SetChaseMode(CombatChaseModes chaseMode, bool send, bool toggle);
        void SetAttackMode(CombatAttackModes attackMode, bool send, bool toggle);
        void SetSecureMode(bool secureMode, bool send, bool toggle);
        void SetPvPMode(CombatPvPModes pvpMode, bool send, bool toggle);
    }
}
