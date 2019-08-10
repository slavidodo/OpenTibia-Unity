using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.MiniWindows
{
    internal class MiniWindowButtons : Core.Components.Base.MiniWindow
    {
        
        [SerializeField] private Button m_StoreButton = null;
        [SerializeField] private Toggle m_RestToggle = null;
        [SerializeField] private Toggle m_SkillsToggle = null;
        [SerializeField] private Toggle m_BattleToggle = null;
        [SerializeField] private Toggle m_BuddyToggle = null;
        [SerializeField] private Button m_QuestLogButton = null;
        [SerializeField] private Button m_RewardWallButton = null;
        [SerializeField] private Toggle m_SpellListToggle = null;
        [SerializeField] private Toggle m_UnjustifiedWindowToggle = null;
        [SerializeField] private Toggle m_PreyListToggle = null;
        [SerializeField] private Button m_OptionsButton = null;
        [SerializeField] private Button m_LogoutButton = null;
        [SerializeField] private Toggle m_AnalyticsToggle = null;

        private bool m_HandlingSkills = false;
        private bool m_HandlingBattle = false;
        private bool m_HandlingBuddy = false;

        protected Inventory.InventoryWindow inventoryWindow {
            get => OpenTibiaUnity.GameManager.GetModule<Inventory.InventoryWindow>();
        }

        protected override void Start() {
            base.Start();

            m_SkillsToggle.onValueChanged.AddListener(OnSkillsToggleValueChanged);
            m_BattleToggle.onValueChanged.AddListener(OnBattleToggleValueChanged);
            m_BuddyToggle.onValueChanged.AddListener(OnBuddyToggleValueChanged);

            m_RestToggle.isOn = true;
            m_RestToggle.onValueChanged.AddListener(OnRestToggleValueChanged);

            m_QuestLogButton.onClick.AddListener(OnQuestsButtonClicked);
            m_OptionsButton.onClick.AddListener(OnOptionsButtonClicked);
            m_LogoutButton.onClick.AddListener(OnLogoutButtonClicked);
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            gameObject.SetActive(newVersion >= 1000);
            if (newVersion < 1000)
                return;

            bool isTibia11 = newVersion >= 1100;
            
            m_SpellListToggle.gameObject.SetActive(isTibia11);
            m_UnjustifiedWindowToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameUnjustifiedPoints));
            m_PreyListToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePrey));
            m_RewardWallButton.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameRewardWall));
            m_AnalyticsToggle.gameObject.SetActive(OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAnalytics));
            m_RestToggle.gameObject.SetActive(isTibia11);
            m_StoreButton.gameObject.SetActive(isTibia11);

            //layoutElement.minHeight = newVersion < 1100 ? 20 : 64;
            m_RestToggle.isOn = true;

            int yExtention = 0;
            if (isTibia11)
                yExtention += 22;

            (m_SkillsToggle.transform.parent as RectTransform).anchoredPosition = new Vector2(5, -yExtention);
            (m_LogoutButton.transform.parent as RectTransform).anchoredPosition = new Vector2(-5, -yExtention);
        }

        public void OnSkillsToggleValueChanged(bool value) {
            if (m_HandlingSkills)
                return;

            try {
                m_HandlingSkills = true;
                m_SkillsToggle.isOn = value;
                inventoryWindow.skillsToggle.isOn = value;

                ToggleWindow(ModulesManager.Instance.SkillsWindowPrefab, value);
            } catch (System.Exception) {
            } finally {
                m_HandlingSkills = false;
            }
        }

        public void OnBattleToggleValueChanged(bool value) {
            if (m_HandlingBattle)
                return;

            try {
                m_HandlingBattle = true;
                m_BattleToggle.isOn = value;
                inventoryWindow.battleToggle.isOn = value;

                ToggleWindow(ModulesManager.Instance.BattleWindowPrefab, value);
            } catch (System.Exception) {

            } finally {
                m_HandlingBattle = false;
            }
        }

        public void OnBuddyToggleValueChanged(bool value) {
            if (m_HandlingBuddy)
                return;

            try {
                m_HandlingBuddy = true;
                m_BuddyToggle.isOn = value;
                inventoryWindow.buddyToggle.isOn = value;

            } catch (System.Exception) {
            } finally {
                m_HandlingBuddy = false;
            }
        }

        private void OnRestToggleValueChanged(bool value) {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                return;

            m_SkillsToggle.transform.parent.gameObject.SetActive(value);
            m_LogoutButton.transform.parent.gameObject.SetActive(value);

            layoutElement.preferredHeight = value ? 64 : 20;
        }

        public void OnQuestsButtonClicked() {

        }

        public void OnOptionsButtonClicked() {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                ModulesManager.Instance.LegacyOptionsWindow.OpenWindow();
        }

        public void OnHelpButtonClicked() {

        }

        public void OnLogoutButtonClicked() {

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
