using System;
using OpenTibiaUnity.Core.Components.Base;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillsWindow : MiniWindow
    {
        [SerializeField] private SkillRawPanel _skillRawPanelPrefab = null;
        [SerializeField] private SkillProgressPanel _skillProgressPanelPrefab = null;
        [SerializeField] private SkillProgressIconPanel _skillProgressIconPanelPrefab = null;

        [SerializeField] private Sprite _magicIcon = null;
        [SerializeField] private Sprite _fistIcon = null;
        [SerializeField] private Sprite _blubIcon = null;
        [SerializeField] private Sprite _swordIcon = null;
        [SerializeField] private Sprite _axeIcon = null;
        [SerializeField] private Sprite _distIcon = null;
        [SerializeField] private Sprite _shieldingIcon = null;
        [SerializeField] private Sprite _fishingIcon = null;

        private SkillPanel _expPanel = null;
        private SkillPanel _levelPanel = null;
        private SkillPanel _expGainRatePanel = null;
        private SkillPanel _hitPointsPanel = null;
        private SkillPanel _manaPanel = null;
        private SkillPanel _soulPointsPanel = null;
        private SkillPanel _capacityPanel = null;
        private SkillPanel _speedPanel = null;
        private SkillPanel _regenerationPanel = null;
        private SkillPanel _staminaPanel = null;

        // known skills
        private SkillPanel _magicPanel = null;
        private SkillPanel _fistPanel = null;
        private SkillPanel _clubPanel = null;
        private SkillPanel _swordPanel = null;
        private SkillPanel _axePanel = null;
        private SkillPanel _distancePanel = null;
        private SkillPanel _shieldingPanel = null;
        private SkillPanel _fishingPanel = null;

        private SkillPanel _briticalChancePanel = null;
        private SkillPanel _briticalExtraDamagePanel = null;
        private SkillPanel _lifeLeechChancePanel = null;
        private SkillPanel _lifeLeechAmountPanel = null;
        private SkillPanel _manaLeechChancePanel = null;
        private SkillPanel _manaLeechAmountPanel = null;

        protected override void Awake() {
            base.Awake();

            Creature.onSkillChange.AddListener(OnCreatureSkillChange);
        }

        private void OnCreatureSkillChange(Creature creature, SkillType skillType, Skill skill) {
            if (creature != OpenTibiaUnity.Player)
                return;

            var skillPanel = GetSkillPanelForSkillType(skillType);
            skillPanel.SetValue(skill.Level, skill.Percentage);
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            base.OnClientVersionChange(_, newVersion);
            var gameManager = OpenTibiaUnity.GameManager;
            var greenColor = Core.Colors.ColorFromRGB(Core.Chat.MessageColors.Green);

            DestroyOldComponents();

            if (newVersion > 1000) {
                _levelPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_LEVEL);
                _expPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_EXPERIENCE_MINIMAL);

                // exp panel is yellow if experienceBonus > 0 (10.54+ and was removed at some point)
            } else {
                _expPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_LEVEL);
                _levelPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_EXPERIENCE);
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceGain))
                _expGainRatePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_XPGAIN);

            _hitPointsPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_HITPOINTS);
            _manaPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_MANA);
            _soulPointsPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_SOULPOINTS);
            _capacityPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_CAPACITY);
            
            if (gameManager.GetFeature(GameFeature.GameSkillsBase))
                _speedPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_SPEED);

            if (gameManager.GetFeature(GameFeature.GamePlayerRegenerationTime))
                _regenerationPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_REGENERATION);

            if (gameManager.GetFeature(GameFeature.GamePlayerStamina))
                _staminaPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_STAMINA);
            
            CreateSeparator();
            if (newVersion > 1150) {
                _magicPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, greenColor, _magicIcon);
                _fistPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_FIST, 0, 0, greenColor, _fistIcon);
                _clubPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_CLUB, 0, 0, greenColor, _blubIcon);
                _swordPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_SWORD, 0, 0, greenColor, _swordIcon);
                _axePanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_AXE, 0, 0, greenColor, _axeIcon);
                _distancePanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_DISTANCE, 0, 0, greenColor, _distIcon);
                _shieldingPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, greenColor, _shieldingIcon);
                _fishingPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, greenColor, _fishingIcon);
            } else {
                _magicPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, greenColor);
                _fistPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_FIST_LEGACY, 0, 0, greenColor);
                _clubPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_CLUB_LEGACY, 0, 0, greenColor);
                _swordPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_SWORD_LEGACY, 0, 0, greenColor);
                _axePanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_AXE_LEGACY, 0, 0, greenColor);
                _distancePanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_DISTANCE_LEGACY, 0, 0, greenColor);
                _shieldingPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, greenColor);
                _shieldingPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, greenColor);
            }
            
            if (gameManager.GetFeature(GameFeature.GameAdditionalSkills)) {
                CreateSeparator();

                CreateSkillLabel(TextResources.SKILLS_CRITICALHIT);
                _briticalChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_CHANCE);
                _briticalExtraDamagePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_EXTRADMG);
                CreateSkillLabel(TextResources.SKILLS_LIFELEECH);
                _lifeLeechChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_CHANCE);
                _lifeLeechAmountPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_AMOUNT);
                CreateSkillLabel(TextResources.SKILLS_MANALEECH);
                _manaLeechChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_MANALEECH_CHANCE);
                _manaLeechAmountPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_MANALEECH_AMOUNT);

                var skillPanels = new SkillPanel[] {
                    _briticalChancePanel, _briticalExtraDamagePanel,
                    _lifeLeechChancePanel, _lifeLeechAmountPanel,
                    _manaLeechChancePanel, _manaLeechAmountPanel
                };

                foreach (var skillPanel in skillPanels) {
                    var rt = skillPanel.labelText.rectTransform;
                    rt.offsetMin = new Vector2(5, rt.offsetMin.y);
                }
            }

            UpdateMaxHeight();
        }

        protected void DestroyOldComponents() {
            if (_expPanel) { Destroy(_expPanel.gameObject); _expPanel = null; }
            if (_levelPanel) { Destroy(_levelPanel.gameObject); _levelPanel = null; }
            if (_expGainRatePanel) { Destroy(_expGainRatePanel.gameObject); _expGainRatePanel = null; }
            if (_hitPointsPanel) { Destroy(_hitPointsPanel.gameObject); _hitPointsPanel = null; }
            if (_manaPanel) { Destroy(_manaPanel.gameObject); _manaPanel = null; }
            if (_soulPointsPanel) { Destroy(_soulPointsPanel.gameObject); _soulPointsPanel = null; }
            if (_capacityPanel) { Destroy(_capacityPanel.gameObject); _capacityPanel = null; }
            if (_speedPanel) { Destroy(_speedPanel.gameObject); _speedPanel = null; }
            if (_regenerationPanel) { Destroy(_regenerationPanel.gameObject); _regenerationPanel = null; }
            if (_staminaPanel) { Destroy(_staminaPanel.gameObject); _staminaPanel = null; }

            if (_magicPanel) { Destroy(_magicPanel.gameObject); _magicPanel = null; }
            if (_fistPanel) { Destroy(_fistPanel.gameObject); _fistPanel = null; }
            if (_clubPanel) { Destroy(_clubPanel.gameObject); _clubPanel = null; }
            if (_swordPanel) { Destroy(_swordPanel.gameObject); _swordPanel = null; }
            if (_axePanel) { Destroy(_axePanel.gameObject); _axePanel = null; }
            if (_distancePanel) { Destroy(_distancePanel.gameObject); _distancePanel = null; }
            if (_shieldingPanel) { Destroy(_shieldingPanel.gameObject); _shieldingPanel = null; }

            if (_briticalChancePanel) { Destroy(_briticalChancePanel.gameObject); _briticalChancePanel = null; }
            if (_briticalExtraDamagePanel) { Destroy(_briticalExtraDamagePanel.gameObject); _briticalExtraDamagePanel = null; }
            if (_lifeLeechChancePanel) { Destroy(_lifeLeechChancePanel.gameObject); _lifeLeechChancePanel = null; }
            if (_lifeLeechAmountPanel) { Destroy(_lifeLeechAmountPanel.gameObject); _lifeLeechAmountPanel = null; }
            if (_manaLeechChancePanel) { Destroy(_manaLeechChancePanel.gameObject); _manaLeechChancePanel = null; }
            if (_manaLeechAmountPanel) { Destroy(_manaLeechAmountPanel.gameObject); _manaLeechAmountPanel = null; }

            foreach (Transform child in _panelContent) {
                Destroy(child.gameObject);
            }
        }

        private SkillPanel CreateSkillPanel(SkillPanel prefab, string name, long value = 0, float percent = 0, Color? color = null, Sprite icon = null) {
            var skillPanel = Instantiate(prefab, _panelContent);
            skillPanel.SetText(name);
            skillPanel.SetValue(value, percent);
            skillPanel.SetIcon(icon);

            if (color != null)
                skillPanel.SetProgressColor(color.Value);

            skillPanel.name = string.Format("Skill_{0}", name);
            return skillPanel;
        }

        private GameObject CreateSeparator() {
            var newGameObject = new GameObject();
            var newRectTransform = newGameObject.AddComponent<RectTransform>();

            var horizontalSeparator = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, newRectTransform);
            var hrRectTransform = horizontalSeparator.transform as RectTransform;
            hrRectTransform.anchorMin = new Vector2(0, 0.5f);
            hrRectTransform.anchorMax = new Vector2(1, 0.5f);
            hrRectTransform.pivot = new Vector2(0.5f, 0.5f);
            hrRectTransform.anchoredPosition = Vector2.zero;

            newGameObject.AddComponent<LayoutElement>().minHeight = 10;
            newGameObject.transform.parent = _panelContent;
            newGameObject.name = "Separator_" + newGameObject.transform.GetSiblingIndex();
            return newGameObject;
        }

        private TMPro.TextMeshProUGUI CreateSkillLabel(string text) {
            var label = Instantiate(OpenTibiaUnity.GameManager.DefaultLabel, _panelContent);
            label.SetText(string.Format("{0}:", text));
            return label;
        }

        private void UpdateMaxHeight() {
            var conetntVerticalLayoutGroup = _panelContent.GetComponent<VerticalLayoutGroup>();
            if (!conetntVerticalLayoutGroup)
                return;

            float maxComtentHeight = (_panelContent.childCount - 1) * conetntVerticalLayoutGroup.spacing + 2;
            foreach (Transform child in _panelContent) {
                var childLayoutElement = child.GetComponent<LayoutElement>();
                if (childLayoutElement)
                    maxComtentHeight += childLayoutElement.minHeight;
            }

            _maxContentHeight = maxComtentHeight;
        }

        private SkillPanel GetSkillPanelForSkillType(SkillType skillType) {
            switch (skillType) {
                case SkillType.Level: return _levelPanel;
                case SkillType.Experience: return _expPanel;
                case SkillType.ExperienceGain: return _expGainRatePanel;
                case SkillType.Health: return _hitPointsPanel;
                case SkillType.Mana: return _manaPanel;
                case SkillType.SoulPoints: return _soulPointsPanel;
                case SkillType.Capacity: return _capacityPanel;
                case SkillType.Speed: return _speedPanel;
                case SkillType.Food: return _regenerationPanel;
                case SkillType.Stamina: return _staminaPanel;
                case SkillType.MagLevel: return _magicPanel;
                case SkillType.Fist: return _fistPanel;
                case SkillType.Club: return _clubPanel;
                case SkillType.Sword: return _swordPanel;
                case SkillType.Axe: return _axePanel;
                case SkillType.Distance: return _distancePanel;
                case SkillType.Shield: return _shieldingPanel;
                case SkillType.CriticalHitChance: return _briticalChancePanel;
                case SkillType.CriticalHitDamage: return _briticalExtraDamagePanel;
                case SkillType.LifeLeechChance: return _lifeLeechChancePanel;
                case SkillType.LifeLeechAmount: return _lifeLeechAmountPanel;
                case SkillType.ManaLeechChance: return _manaLeechChancePanel;
                case SkillType.ManaLeechAmount: return _manaLeechAmountPanel;
            }

            return null;
        }
    }
}
