using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.MiniWindows
{
    public class MiniWindowButtons : Core.Components.Base.MiniWindow
    {
        
        [SerializeField] private Button _storeButton = null;
        [SerializeField] private Toggle _restToggle = null;
        [SerializeField] private Toggle _skillsToggle = null;
        [SerializeField] private Toggle _battleToggle = null;
        [SerializeField] private Toggle _buddyToggle = null;
        [SerializeField] private Button _questLogButton = null;
        [SerializeField] private Button _rewardWallButton = null;
        [SerializeField] private Toggle _spellListToggle = null;
        [SerializeField] private Toggle _unjustifiedWindowToggle = null;
        [SerializeField] private Toggle _preyListToggle = null;
        [SerializeField] private Button _optionsButton = null;
        [SerializeField] private Button _logoutButton = null;
        [SerializeField] private Toggle _analyticsToggle = null;

        private bool _handlingSkills = false;
        private bool _handlingBattle = false;
        private bool _handlingBuddy = false;

        protected Inventory.InventoryWindow inventoryWindow {
            get => OpenTibiaUnity.GameManager.GetModule<Inventory.InventoryWindow>();
        }

        protected override void Start() {
            base.Start();

            _skillsToggle.onValueChanged.AddListener(OnSkillsToggleValueChanged);
            _battleToggle.onValueChanged.AddListener(OnBattleToggleValueChanged);
            _buddyToggle.onValueChanged.AddListener(OnBuddyToggleValueChanged);

            _restToggle.isOn = true;
            _restToggle.onValueChanged.AddListener(OnRestToggleValueChanged);

            _questLogButton.onClick.AddListener(OnQuestsButtonClicked);
            _optionsButton.onClick.AddListener(OnOptionsButtonClicked);
            _logoutButton.onClick.AddListener(OnLogoutButtonClicked);
            _storeButton.onClick.AddListener(OnStoreButtonClicked);
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            gameObject.SetActive(newVersion >= 1000);
            if (newVersion < 1000)
                return;

            bool isTibia11 = newVersion >= 1100;
            
            _spellListToggle.gameObject.SetActive(isTibia11);
            _unjustifiedWindowToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameUnjustifiedPoints));
            _preyListToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePrey));
            _rewardWallButton.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameRewardWall));
            _analyticsToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAnalytics));
            _restToggle.gameObject.SetActive(isTibia11);
            _storeButton.gameObject.SetActive(isTibia11);

            //layoutElement.minHeight = newVersion < 1100 ? 20 : 64;
            _restToggle.isOn = true;

            int yExtention = 0;
            if (isTibia11) {
                yExtention += 22;
                layoutElement.preferredHeight = _restToggle.isOn ? 64 : 20;
            } else {
                layoutElement.preferredHeight = 20;
            }

            (_skillsToggle.transform.parent as RectTransform).anchoredPosition = new Vector2(5, -yExtention);
            (_logoutButton.transform.parent as RectTransform).anchoredPosition = new Vector2(-5, -yExtention);
        }

        public void OnSkillsToggleValueChanged(bool value) {
            if (_handlingSkills)
                return;

            try {
                _handlingSkills = true;
                _skillsToggle.isOn = value;
                inventoryWindow.skillsToggle.isOn = value;

                ToggleWindow(ModulesManager.Instance.SkillsWindowPrefab, value);
            } catch (System.Exception) {
            } finally {
                _handlingSkills = false;
            }
        }

        public void OnBattleToggleValueChanged(bool value) {
            if (_handlingBattle)
                return;

            try {
                _handlingBattle = true;
                _battleToggle.isOn = value;
                inventoryWindow.battleToggle.isOn = value;

                ToggleWindow(ModulesManager.Instance.BattleWindowPrefab, value);
            } catch (System.Exception) {

            } finally {
                _handlingBattle = false;
            }
        }

        public void OnBuddyToggleValueChanged(bool value) {
            if (_handlingBuddy)
                return;

            try {
                _handlingBuddy = true;
                _buddyToggle.isOn = value;
                inventoryWindow.buddyToggle.isOn = value;

            } catch (System.Exception) {
            } finally {
                _handlingBuddy = false;
            }
        }

        private void OnRestToggleValueChanged(bool value) {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                return;

            _skillsToggle.transform.parent.gameObject.SetActive(value);
            _logoutButton.transform.parent.gameObject.SetActive(value);

            layoutElement.preferredHeight = value ? 64 : 20;
        }

        public void OnQuestsButtonClicked() {

        }

        public void OnOptionsButtonClicked() {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                ModulesManager.Instance.LegacyOptionsWindow.Open();
        }

        public void OnHelpButtonClicked() {

        }

        public void OnLogoutButtonClicked() {

        }

        public void OnStoreButtonClicked() {
            var gameManager = OpenTibiaUnity.GameManager;
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning) {
                protocolGame.SendOpenStore();
                if (gameManager.ClientVersion >= 1180) {
                    var openParamaters = new Core.Store.StoreOpenParameters(StoreOpenParameterAction.Invalid, null);
                    protocolGame.SendRequestStoreOffers(openParamaters.OpenAction, openParamaters);
                }
            }
        }

        private void ToggleWindow<T>(T prefab, bool value) where T : Core.Components.Base.MiniWindow {
            var miniWindow = OpenTibiaUnity.GameManager.GetModule<T>();
            if (value) {
                if (!miniWindow) {
                    var gameWindowLayout = OpenTibiaUnity.GameManager.GetModule<GameWindow.GameInterface>();
                    miniWindow = gameWindowLayout.CreateMiniWindow(prefab);
                }
            } else {
                if (miniWindow)
                    Destroy(miniWindow.gameObject);
            }
        }
    }
}
