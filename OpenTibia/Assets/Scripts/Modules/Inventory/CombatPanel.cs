using UnityEngine;

namespace OpenTibiaUnity.Modules.Inventory
{
    public class CombatPanel : Core.Components.Base.Module
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.Toggle _chaseOffToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _chaseOnToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _expertModeToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _expertModeSmallToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _pvpDoveToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _pvpWhiteHandToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _pvpYellowHandToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _pvpRedFistToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _attackOffensiveToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _attackBalancedToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _attackDefensiveToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _secureModeToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _legacySecureModeToggle = null;

        // fields
        private bool _handlingExpertPvPToggle = false;

        // properties
        protected RectTransform expertPvpPanel {
            get => _pvpDoveToggle.transform.parent as RectTransform;
        }

        protected override void Awake() {
            base.Awake();

            // setup events
            OpenTibiaUnity.GameManager.onTacticsChange.AddListener((attackMode, chaseMode, secureMode, pvpMode) => {
                SetAttackMode(attackMode, false, true);
                SetChaseMode(chaseMode, false, true);
                SetSecureMode(secureMode, false, true);
                SetPvPMode(pvpMode, false, true);
            });

            _chaseOffToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.Off, true, false); });
            _chaseOnToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.On, true, false); });

            _attackOffensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Offensive, true, false); });
            _attackBalancedToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Balanced, true, false); });
            _attackDefensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Defensive, true, false); });

            _pvpDoveToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.Dove, true, false); });
            _pvpWhiteHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.WhiteHand, true, false); });
            _pvpYellowHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.YellowHand, true, false); });
            _pvpRedFistToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.RedFist, true, false); });

            _secureModeToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });
            _legacySecureModeToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });

            _expertModeToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);
            _expertModeSmallToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);
        }

        protected override void OnClientVersionChange(int oldVersion, int newVersion) {
            base.OnClientVersionChange(oldVersion, newVersion);

            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            _secureModeToggle.gameObject.SetActive(hasExpertPvp);
            _legacySecureModeToggle.gameObject.SetActive(!hasExpertPvp);

            _expertModeToggle.gameObject.SetActive(hasExpertPvp);
            if (!hasExpertPvp)
                expertPvpPanel.gameObject.SetActive(false);
        }

        private void OnExpertPvPToggleValueChanged(bool value) {
            if (_handlingExpertPvPToggle)
                return;

            try {
                _handlingExpertPvPToggle = true;
                _expertModeToggle.isOn = value;
                _expertModeSmallToggle.isOn = value;
                expertPvpPanel.gameObject.SetActive(value);

                SetPvPMode(CombatPvPModes.Dove, true, true);
            } catch (System.Exception) {
            } finally {
                _handlingExpertPvPToggle = false;
            }
        }

        public void SetChaseMode(CombatChaseModes chaseMode, bool send, bool toggle) {
            if (chaseMode == OpenTibiaUnity.OptionStorage.CombatChaseMode)
                return;

            OpenTibiaUnity.OptionStorage.CombatChaseMode = chaseMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                switch (chaseMode) {
                    case CombatChaseModes.On:
                        _chaseOnToggle.isOn = true;
                        break;
                    case CombatChaseModes.Off:
                        _chaseOffToggle.isOn = true;
                        break;

                    default:
                        return;
                }
            }
        }

        public void SetAttackMode(CombatAttackModes attackMode, bool send, bool toggle) {
            if (attackMode == OpenTibiaUnity.OptionStorage.CombatAttackMode)
                return;

            OpenTibiaUnity.OptionStorage.CombatAttackMode = attackMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                switch (attackMode) {
                    case CombatAttackModes.Offensive:
                        _attackOffensiveToggle.isOn = true;
                        break;
                    case CombatAttackModes.Balanced:
                        _attackBalancedToggle.isOn = true;
                        break;
                    case CombatAttackModes.Defensive:
                        _attackBalancedToggle.isOn = true;
                        break;

                    default:
                        return;
                }
            }
        }

        public void SetSecureMode(bool secureMode, bool send, bool toggle) {
            if (secureMode == OpenTibiaUnity.OptionStorage.CombatSecureMode)
                return;

            OpenTibiaUnity.OptionStorage.CombatSecureMode = secureMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                _secureModeToggle.isOn = !secureMode;
                _legacySecureModeToggle.isOn = !secureMode;
            }
        }

        public void SetPvPMode(CombatPvPModes pvpMode, bool send, bool toggle) {
            if (pvpMode == OpenTibiaUnity.OptionStorage.CombatPvPMode)
                return;

            // force toggle of e-pvp button
            if (pvpMode != CombatPvPModes.Dove) {
                _expertModeToggle.isOn = true;
                _expertModeSmallToggle.isOn = true;
            }

            OpenTibiaUnity.OptionStorage.CombatPvPMode = pvpMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                _pvpDoveToggle.isOn = pvpMode == CombatPvPModes.Dove;
                _pvpWhiteHandToggle.isOn = pvpMode == CombatPvPModes.WhiteHand;
                _pvpYellowHandToggle.isOn = pvpMode == CombatPvPModes.YellowHand;
                _pvpRedFistToggle.isOn = pvpMode == CombatPvPModes.RedFist;
            }
        }

        public void ToggleStyle(bool minimized) {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.ClientVersion > 1000;

            if (minimized) {
                _secureModeToggle.gameObject.SetActive(false);
                _legacySecureModeToggle.gameObject.SetActive(true);
                _expertModeToggle.gameObject.SetActive(false);
                _expertModeSmallToggle.gameObject.SetActive(hasExpertPvp);

                _attackOffensiveToggle.rectTransform.anchoredPosition = new Vector2(-92.0f, 0.0f);
                _attackBalancedToggle.rectTransform.anchoredPosition = new Vector2(-72.0f, -0.0f);
                _attackDefensiveToggle.rectTransform.anchoredPosition = new Vector2(-52.0f, -0.0f);
                _chaseOffToggle.rectTransform.anchoredPosition = new Vector2(-92.0f, -23.0f);
                _chaseOnToggle.rectTransform.anchoredPosition = new Vector2(-72.0f, -23.0f);
                _legacySecureModeToggle.rectTransform.anchoredPosition = new Vector2(-52.0f, -23.0f);
            } else {
                _secureModeToggle.gameObject.SetActive(hasExpertPvp);
                _legacySecureModeToggle.gameObject.SetActive(!hasExpertPvp);
                _expertModeToggle.gameObject.SetActive(hasExpertPvp);
                _expertModeSmallToggle.gameObject.SetActive(false);

                _attackOffensiveToggle.rectTransform.anchoredPosition = new Vector2(-28.0f, 0.0f);
                _attackBalancedToggle.rectTransform.anchoredPosition = new Vector2(-28.0f, -23.0f);
                _attackDefensiveToggle.rectTransform.anchoredPosition = new Vector2(-28.0f, -46.0f);
                _chaseOffToggle.rectTransform.anchoredPosition = new Vector2(-5, 0.0f);
                _chaseOnToggle.rectTransform.anchoredPosition = new Vector2(-5, -23.0f);
                _legacySecureModeToggle.rectTransform.anchoredPosition = new Vector2(-5f, -46.0f);
            }
        }
    }
}
