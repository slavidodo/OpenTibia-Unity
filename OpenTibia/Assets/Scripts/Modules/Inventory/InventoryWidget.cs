using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Inventory
{
    public class InventoryWidget : UI.Legacy.SidebarWidget
    {
        // constants
        public const int ModernMaximizedWindowHeight = 160;
        public const int LegacyMaximizedWindowHeight = ModernMaximizedWindowHeight + 26;
        public const int ModernMinimizedWindowHeight = 58;
        public const int LegacyMinimizedWindowHeight = ModernMinimizedWindowHeight + 26;

        // serialized fields
        [SerializeField]
        private BodyContainer _bodyContainer = null;
        [SerializeField]
        private CombatPanel _combatPanel = null;
        [SerializeField]
        private UI.Legacy.Button _stopButton = null;
        [SerializeField]
        private UI.Legacy.Button _toggleStyleButton = null;
        [SerializeField]
        private UI.Legacy.Button _storeInboxButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyStoreButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyStoreInboxButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyStopButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyQuestlogButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyOptionsButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyHelpButton = null;
        [SerializeField]
        private UI.Legacy.Button _legacyLogoutButton = null;
        [SerializeField]
        private UI.Legacy.Toggle _legacySkillsToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _legacyBattleToggle = null;
        [SerializeField]
        private UI.Legacy.Toggle _legacyBuddyToggle = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _capacityLabel = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _soulPointsLabel = null;
        [SerializeField]
        private ConditionsPanel _conditionsPanel = null;
        [SerializeField]
        private Sprite _minimizeNormalSprite = null;
        [SerializeField]
        private Sprite _minimizePressedSprite = null;
        [SerializeField]
        private Sprite _maximizeNormalSprite = null;
        [SerializeField]
        private Sprite _maximizePressedSprite = null;

        // field
        private bool _isMinimized = false;

        // properties
        public UI.Legacy.Toggle skillsToggle { get => _legacySkillsToggle; }
        public UI.Legacy.Toggle battleToggle { get => _legacyBattleToggle; }
        public UI.Legacy.Toggle buddyToggle { get => _legacyBuddyToggle; }

        protected ToggleButtons.ToggleButtonsWidget toggleButtonsWidget {
            get => OpenTibiaUnity.GameManager.GetModule<ToggleButtons.ToggleButtonsWidget>();
        }

        protected override void Awake() {
            base.Awake();
            
            // setup events
            Creature.onSkillChange.AddListener(OnSkillChange);
        }

        protected override void Start() {
            base.Start();

            _stopButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            _legacyStopButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            _toggleStyleButton.onClick.AddListener(ToggleStyle);

            _legacySkillsToggle.onValueChanged.AddListener(toggleButtonsWidget.OnSkillsToggleValueChanged);
            _legacyBattleToggle.onValueChanged.AddListener(toggleButtonsWidget.OnBattleToggleValueChanged);
            _legacyBuddyToggle.onValueChanged.AddListener(toggleButtonsWidget.OnBuddyToggleValueChanged);
            _legacyQuestlogButton.onClick.AddListener(toggleButtonsWidget.OnQuestlogButtonClick);
            _legacyOptionsButton.onClick.AddListener(toggleButtonsWidget.OnOptionsButtonClick);
            _legacyHelpButton.onClick.AddListener(toggleButtonsWidget.OnHelpButtonClicked);
            _legacyLogoutButton.onClick.AddListener(toggleButtonsWidget.OnLogoutButtonClick);
            _legacyStoreButton.onClick.AddListener(toggleButtonsWidget.OnStoreButtonClick);

            _storeInboxButton.onClick.AddListener(OnStoreInboxButtonClick);
            _legacyStoreInboxButton.onClick.AddListener(OnStoreInboxButtonClick);
        }

        protected override void OnClientVersionChange(int _, int __) {
            base.OnClientVersionChange(_, __);
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            bool hasStoreButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStore) && OpenTibiaUnity.GameManager.ClientVersion < 1100;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);
            bool hasLegacyStoreInbox = hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100;

            layoutElement.preferredHeight = CalculatePreferredHeight();

            _stopButton.rectTransform.anchoredPosition = new Vector2(-5.0f, hasLegacyStoreInbox ? 14.0f : 0.0f);

            _stopButton.gameObject.SetActive(hasExpertPvp);
            _storeInboxButton.gameObject.SetActive(hasStoreInbox && !hasLegacyStoreInbox);
            _legacyStoreInboxButton.gameObject.SetActive(hasLegacyStoreInbox);
            _legacyStoreButton.gameObject.SetActive(hasStoreButton);
            _legacyStopButton.gameObject.SetActive(!hasExpertPvp);
            _legacyQuestlogButton.gameObject.SetActive(!hasExpertPvp);
            _legacyOptionsButton.gameObject.SetActive(!hasExpertPvp);
            _legacyHelpButton.gameObject.SetActive(!hasExpertPvp);
            _legacyLogoutButton.gameObject.SetActive(!hasExpertPvp);
            _legacySkillsToggle.gameObject.SetActive(!hasExpertPvp);
            _legacyBattleToggle.gameObject.SetActive(!hasExpertPvp);
            _legacyBuddyToggle.gameObject.SetActive(!hasExpertPvp);
        }

        protected void OnSkillChange(Creature creature, SkillType skillType, Skill skill) {
            var player = creature as Player;
            if (!player)
                return;

            if (skillType == SkillType.Capacity) {
                if (player.FreeCapacity <= -1)
                    _capacityLabel.text = "<color=#00EB00>infinity</color>";
                else
                    _capacityLabel.text = string.Format("{0}", Core.Utils.Utility.Commafy(player.FreeCapacity / 1000));
            } else if (skillType == SkillType.SoulPoints) {
                _soulPointsLabel.text = string.Format("{0}", skill.Level);
            }
        }

        public void ToggleStyle() {
            var spriteState = _toggleStyleButton.spriteState;
            var imageComponent = _toggleStyleButton.GetComponent<UnityUI.Image>();

            _isMinimized = !_isMinimized;
            if (_isMinimized) {
                MinimizeStyle();
                imageComponent.sprite = _maximizeNormalSprite;
                spriteState.pressedSprite = _maximizePressedSprite;
            } else {
                MaximizeStyle();
                imageComponent.sprite = _minimizeNormalSprite;
                spriteState.pressedSprite = _minimizePressedSprite;
            }

            layoutElement.preferredHeight = CalculatePreferredHeight();
            _toggleStyleButton.spriteState = spriteState;
        }

        private void MinimizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);

            _bodyContainer.ToggleStyle(true);
            _combatPanel.ToggleStyle(true);
            
            _storeInboxButton.gameObject.SetActive(false);

            _legacyQuestlogButton.gameObject.SetActive(false);
            _legacyOptionsButton.gameObject.SetActive(false);
            _legacyHelpButton.gameObject.SetActive(false);

            _stopButton.gameObject.SetActive(hasExpertPvp);
            _legacyStopButton.gameObject.SetActive(!hasExpertPvp);
            
            (_capacityLabel.rectTransform.parent as RectTransform).anchoredPosition = new Vector2(19.0f, 0.0f);
            (_soulPointsLabel.rectTransform.parent as RectTransform).anchoredPosition = new Vector2(19.0f, -23.0f);

            _conditionsPanel.rectTransform.anchoredPosition = new Vector2(19.0f, -46.0f);
            _conditionsPanel.rectTransform.sizeDelta = new Vector2(96, 12);
            _legacyStopButton.rectTransform.anchoredPosition = new Vector2(-5.0f, -0.0f);
        }

        private void MaximizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.ClientVersion > 1000;

            _bodyContainer.ToggleStyle(false);
            _combatPanel.ToggleStyle(false);

            _legacyQuestlogButton.gameObject.SetActive(!hasExpertPvp);
            _legacyOptionsButton.gameObject.SetActive(!hasExpertPvp);
            _legacyHelpButton.gameObject.SetActive(!hasExpertPvp);

            (_capacityLabel.rectTransform.parent as RectTransform).anchoredPosition = new Vector2(79.0f, -125.0f);
            (_soulPointsLabel.rectTransform.parent as RectTransform).anchoredPosition = new Vector2(5.0f, -125.0f);

            _conditionsPanel.rectTransform.anchoredPosition = new Vector2(5.0f, -148.0f);
            _conditionsPanel.rectTransform.sizeDelta = new Vector2(108, 12);
            _legacyStopButton.rectTransform.anchoredPosition = new Vector2(-5.0f, -69.0f);
        }

        private int CalculatePreferredHeight() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);
            bool hasLegacyStoreInbox = hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100;

            int newHeight;
            if (_isMinimized)
                newHeight = hasExpertPvp ? ModernMinimizedWindowHeight : LegacyMinimizedWindowHeight;
            else
                newHeight = hasExpertPvp ? ModernMaximizedWindowHeight : LegacyMaximizedWindowHeight;

            if (hasLegacyStoreInbox)
                newHeight += 14;

            return newHeight;
        }

        private void OnStoreInboxButtonClick() {
            var storeInbox = OpenTibiaUnity.ContainerStorage.BodyContainerView.GetObject(ClothSlots.StoreInbox);
            if (!!storeInbox) {
                var absolutePosition = new Vector3Int(65535, (int)ClothSlots.StoreInbox, 0);
                new UseActionImpl(absolutePosition, storeInbox.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
            }
        }
    }
}
