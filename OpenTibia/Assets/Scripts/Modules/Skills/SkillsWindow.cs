using System;
using OpenTibiaUnity.Core.Components.Base;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillsWindow : MiniWindow
    {
        private static Color RedColor = new Color(1, 0, 0);
        private static Color GreenColor = new Color(0, 1, 0);

        [SerializeField] private SkillRawPanel _skillRawPanelPrefab = null;
        [SerializeField] private SkillProgressPanel _skillProgressPanelPrefab = null;
        [SerializeField] private SkillProgressIconPanel _skillProgressIconPanelPrefab = null;

        [SerializeField] private Sprite _magicIcon = null;
        [SerializeField] private Sprite _fistIcon = null;
        [SerializeField] private Sprite _clubIcon = null;
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
        private SkillPanel _offlineTrainingPanel = null;

        // known skills
        private SkillPanel _magicPanel = null;
        private SkillPanel _fistPanel = null;
        private SkillPanel _clubPanel = null;
        private SkillPanel _swordPanel = null;
        private SkillPanel _axePanel = null;
        private SkillPanel _distancePanel = null;
        private SkillPanel _shieldingPanel = null;
        private SkillPanel _fishingPanel = null;

        private SkillPanel _criticalChancePanel = null;
        private SkillPanel _criticalExtraDamagePanel = null;
        private SkillPanel _lifeLeechChancePanel = null;
        private SkillPanel _lifeLeechAmountPanel = null;
        private SkillPanel _manaLeechChancePanel = null;
        private SkillPanel _manaLeechAmountPanel = null;

        protected override void Awake() {
            base.Awake();

            Creature.onSkillChange.AddListener(OnCreatureSkillChange);
        }

        protected override void Start() {
            base.Start();

            if (OpenTibiaUnity.GameManager.IsGameRunning) {
                var player = OpenTibiaUnity.Player;
                for (SkillType skillType = SkillType.First; skillType < SkillType.Last; skillType++)
                    OnCreatureSkillChange(player, skillType, player.GetSkill(skillType));

                OnCreatureSkillChange(player, SkillType.Experience, player.GetSkill(SkillType.Experience));
            }
        }

        private void OnCreatureSkillChange(Creature creature, SkillType skillType, Skill skill) {
            if (creature != OpenTibiaUnity.Player)
                return;

            var skillPanel = GetSkillPanelForSkillType(skillType);
            if (!skillPanel)
                return;

            switch (skillType) {
                case SkillType.Stamina:
                case SkillType.Food:
                case SkillType.OfflineTraining: {
                    int value = (int)Mathf.Round(Mathf.Max(0, skill.Level - skill.BaseLevel) / 60000f);
                    float percentage = 0;
                    if (skillType == SkillType.Stamina) {
                        int happyHour = 1;
                        int totalHours = 56;
                        if (OpenTibiaUnity.GameManager.ClientVersion >= 841) {
                            happyHour = 2;
                            totalHours = 42;
                        }

                        percentage = value / (totalHours * 60f);
                        if ((totalHours * 60) - value > happyHour * 60)
                            skillPanel.SetProgressColor(RedColor);
                        else
                            skillPanel.SetProgressColor(GreenColor);
                    } else if (skillType == SkillType.OfflineTraining) {
                        percentage = value / (12 * 60f);
                    }

                    skillPanel.SetValue(string.Format("{0:D2}:{1:D2}", value / 60, value % 60), percentage * 100);
                    break;
                }

                default: {
                    long skillLevel = skill.Level;
                    if (skillType == SkillType.Capacity)
                        skillLevel /= 100;

                    skillPanel.SetValue(skillLevel, skill.Percentage);
                    break;
                }
            }

            
        }

        protected override void OnClientVersionChange(int _, int newVersion) {
            base.OnClientVersionChange(_, newVersion);
            var gameManager = OpenTibiaUnity.GameManager;

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

            if (gameManager.GetFeature(GameFeature.GameOfflineTrainingTime))
                _offlineTrainingPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_OFFLINETRAINING);

            if (newVersion > 1150) {
                CreateSeparator();
                _magicPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, GreenColor, _magicIcon);
                _fistPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_FIST, 0, 0, GreenColor, _fistIcon);
                _clubPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_CLUB, 0, 0, GreenColor, _clubIcon);
                _swordPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_SWORD, 0, 0, GreenColor, _swordIcon);
                _axePanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_AXE, 0, 0, GreenColor, _axeIcon);
                _distancePanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_DISTANCE, 0, 0, GreenColor, _distIcon);
                _shieldingPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, GreenColor, _shieldingIcon);
                _fishingPanel = CreateSkillPanel(_skillProgressIconPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, GreenColor, _fishingIcon);
            } else {
                _magicPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, RedColor);
                CreateSeparator();
                _fistPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_FIST_LEGACY, 0, 0, GreenColor);
                _clubPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_CLUB_LEGACY, 0, 0, GreenColor);
                _swordPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_SWORD_LEGACY, 0, 0, GreenColor);
                _axePanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_AXE_LEGACY, 0, 0, GreenColor);
                _distancePanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_DISTANCE_LEGACY, 0, 0, GreenColor);
                _shieldingPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, GreenColor);
                _shieldingPanel = CreateSkillPanel(_skillProgressPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, GreenColor);
            }
            
            if (gameManager.GetFeature(GameFeature.GameAdditionalSkills)) {
                CreateSeparator();

                CreateSkillLabel(TextResources.SKILLS_CRITICALHIT);
                _criticalChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_CHANCE);
                _criticalExtraDamagePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_EXTRADMG);
                CreateSkillLabel(TextResources.SKILLS_LIFELEECH);
                _lifeLeechChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_CHANCE);
                _lifeLeechAmountPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_AMOUNT);
                CreateSkillLabel(TextResources.SKILLS_MANALEECH);
                _manaLeechChancePanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_MANALEECH_CHANCE);
                _manaLeechAmountPanel = CreateSkillPanel(_skillRawPanelPrefab, TextResources.SKILLS_MANALEECH_AMOUNT);

                var skillPanels = new SkillPanel[] {
                    _criticalChancePanel, _criticalExtraDamagePanel,
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
            if (_offlineTrainingPanel) { Destroy(_offlineTrainingPanel.gameObject); _offlineTrainingPanel = null; }

            if (_magicPanel) { Destroy(_magicPanel.gameObject); _magicPanel = null; }
            if (_fistPanel) { Destroy(_fistPanel.gameObject); _fistPanel = null; }
            if (_clubPanel) { Destroy(_clubPanel.gameObject); _clubPanel = null; }
            if (_swordPanel) { Destroy(_swordPanel.gameObject); _swordPanel = null; }
            if (_axePanel) { Destroy(_axePanel.gameObject); _axePanel = null; }
            if (_distancePanel) { Destroy(_distancePanel.gameObject); _distancePanel = null; }
            if (_shieldingPanel) { Destroy(_shieldingPanel.gameObject); _shieldingPanel = null; }

            if (_criticalChancePanel) { Destroy(_criticalChancePanel.gameObject); _criticalChancePanel = null; }
            if (_criticalExtraDamagePanel) { Destroy(_criticalExtraDamagePanel.gameObject); _criticalExtraDamagePanel = null; }
            if (_lifeLeechChancePanel) { Destroy(_lifeLeechChancePanel.gameObject); _lifeLeechChancePanel = null; }
            if (_lifeLeechAmountPanel) { Destroy(_lifeLeechAmountPanel.gameObject); _lifeLeechAmountPanel = null; }
            if (_manaLeechChancePanel) { Destroy(_manaLeechChancePanel.gameObject); _manaLeechChancePanel = null; }
            if (_manaLeechAmountPanel) { Destroy(_manaLeechAmountPanel.gameObject); _manaLeechAmountPanel = null; }

            foreach (Transform child in _panelContent)
                Destroy(child.gameObject);
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
            newGameObject.transform.SetParent(_panelContent);
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
                case SkillType.OfflineTraining: return _offlineTrainingPanel;
                case SkillType.MagLevel: return _magicPanel;
                case SkillType.Fist: return _fistPanel;
                case SkillType.Club: return _clubPanel;
                case SkillType.Sword: return _swordPanel;
                case SkillType.Axe: return _axePanel;
                case SkillType.Distance: return _distancePanel;
                case SkillType.Shield: return _shieldingPanel;
                case SkillType.CriticalHitChance: return _criticalChancePanel;
                case SkillType.CriticalHitDamage: return _criticalExtraDamagePanel;
                case SkillType.LifeLeechChance: return _lifeLeechChancePanel;
                case SkillType.LifeLeechAmount: return _lifeLeechAmountPanel;
                case SkillType.ManaLeechChance: return _manaLeechChancePanel;
                case SkillType.ManaLeechAmount: return _manaLeechAmountPanel;
            }

            return null;
        }
    }
}
