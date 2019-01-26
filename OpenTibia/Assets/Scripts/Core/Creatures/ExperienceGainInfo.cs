namespace OpenTibiaUnity.Core.Creatures
{
    public class ExperienceGainInfo
    {
        private float m_BaseXpGain;
        private float m_VoucherAddend;
        private float m_GrindingAddend;
        private float m_StoreBoostAddend;
        private float m_HuntingBoostFactor;

        private uint m_RemainingStoreXpBoostSeconds;
        private bool m_CanBuyMoreStoreXpBoosts;

        public ExperienceGainInfo() {
            Reset();
        }

        public void UpdateStoreXpBoost(uint remainingSeconds, bool canBuyMoreXpBoosts) {
            if (m_RemainingStoreXpBoostSeconds != remainingSeconds || m_CanBuyMoreStoreXpBoosts != canBuyMoreXpBoosts) {
                m_RemainingStoreXpBoostSeconds = remainingSeconds;
                m_CanBuyMoreStoreXpBoosts = canBuyMoreXpBoosts;
            }
        }

        public void UpdateGainInfo(float baseXpGain, float voucherAddend, float grindingAddend, float storeBoostAddend, float huntingBoostFactor) {

            if (m_BaseXpGain != baseXpGain || m_VoucherAddend != voucherAddend || m_GrindingAddend != grindingAddend || m_StoreBoostAddend != storeBoostAddend || m_HuntingBoostFactor != huntingBoostFactor) {
                m_BaseXpGain = baseXpGain;
                m_VoucherAddend = voucherAddend;
                m_GrindingAddend = grindingAddend;
                m_StoreBoostAddend = storeBoostAddend;
                m_HuntingBoostFactor = huntingBoostFactor;
            }
        }

        public void Reset() {
            m_BaseXpGain = 1;
            m_VoucherAddend = 0;
            m_GrindingAddend = 0;
            m_StoreBoostAddend = 0;
            m_HuntingBoostFactor = 1;
            m_RemainingStoreXpBoostSeconds = 0;
            m_CanBuyMoreStoreXpBoosts = true;
        }

        public float ComputeXpGainModifier() {
            return (m_BaseXpGain + m_VoucherAddend + m_GrindingAddend + m_StoreBoostAddend) * m_HuntingBoostFactor;
        }

        public bool CanCurrentlyBuyXpBoost() {
            return m_RemainingStoreXpBoostSeconds == 0 && m_CanBuyMoreStoreXpBoosts;
        }
    }
}
