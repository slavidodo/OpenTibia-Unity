namespace OpenTibiaUnity.Core.Creatures
{
    public class ExperienceGainInfo
    {
        private float _baseXpGain;
        private float _voucherAddend;
        private float _grindingAddend;
        private float _storeBoostAddend;
        private float _huntingBoostFactor;

        private uint _remainingStoreXpBoostSeconds;
        private bool _banBuyMoreStoreXpBoosts;

        public ExperienceGainInfo() {
            Reset();
        }

        public void UpdateStoreXpBoost(uint remainingSeconds, bool canBuyMoreXpBoosts) {
            if (_remainingStoreXpBoostSeconds != remainingSeconds || _banBuyMoreStoreXpBoosts != canBuyMoreXpBoosts) {
                _remainingStoreXpBoostSeconds = remainingSeconds;
                _banBuyMoreStoreXpBoosts = canBuyMoreXpBoosts;
            }
        }

        public void UpdateGainInfo(float baseXpGain, float voucherAddend, float grindingAddend, float storeBoostAddend, float huntingBoostFactor) {

            if (_baseXpGain != baseXpGain || _voucherAddend != voucherAddend || _grindingAddend != grindingAddend || _storeBoostAddend != storeBoostAddend || _huntingBoostFactor != huntingBoostFactor) {
                _baseXpGain = baseXpGain;
                _voucherAddend = voucherAddend;
                _grindingAddend = grindingAddend;
                _storeBoostAddend = storeBoostAddend;
                _huntingBoostFactor = huntingBoostFactor;
            }
        }

        public void Reset() {
            _baseXpGain = 1;
            _voucherAddend = 0;
            _grindingAddend = 0;
            _storeBoostAddend = 0;
            _huntingBoostFactor = 1;
            _remainingStoreXpBoostSeconds = 0;
            _banBuyMoreStoreXpBoosts = true;
        }

        public float ComputeXpGainModifier() {
            return (_baseXpGain + _voucherAddend + _grindingAddend + _storeBoostAddend) * _huntingBoostFactor;
        }

        public bool CanCurrentlyBuyXpBoost() {
            return _remainingStoreXpBoostSeconds == 0 && _banBuyMoreStoreXpBoosts;
        }
    }
}
