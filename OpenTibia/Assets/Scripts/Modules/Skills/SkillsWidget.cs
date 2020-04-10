using OpenTibiaUnity.Core.Creatures;
using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillsWidget : UI.Legacy.SidebarWidget
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.ScrollRect _skillScrollRect = null;
        [SerializeField]
        private Sprite _magicIcon = null;
        [SerializeField]
        private Sprite _fistIcon = null;
        [SerializeField]
        private Sprite _clubIcon = null;
        [SerializeField]
        private Sprite _swordIcon = null;
        [SerializeField]
        private Sprite _axeIcon = null;
        [SerializeField]
        private Sprite _distIcon = null;
        [SerializeField]
        private Sprite _shieldingIcon = null;
        [SerializeField]
        private Sprite _fishingIcon = null;

        // fields
        private Dictionary<SkillType, SkillPanel> _skillPanels = new Dictionary<SkillType, SkillPanel>();

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
                            skillPanel.SetProgressColor(Color.red);
                        else
                            skillPanel.SetProgressColor(Color.green);
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
                CreateSkillPanel(SkillType.Level, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_LEVEL);
                CreateSkillPanel(SkillType.Experience, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_EXPERIENCE_MINIMAL);

                // exp panel is yellow if experienceBonus > 0 (10.54+ and was removed at some point)
            } else {
                CreateSkillPanel(SkillType.Level, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_LEVEL);
                CreateSkillPanel(SkillType.Experience, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_EXPERIENCE);
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceGain))
                CreateSkillPanel(SkillType.ExperienceGain, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_XPGAIN);

            CreateSkillPanel(SkillType.Health, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_HITPOINTS);
            CreateSkillPanel(SkillType.Mana, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_MANA);
            CreateSkillPanel(SkillType.SoulPoints, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_SOULPOINTS);
            CreateSkillPanel(SkillType.Capacity, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_CAPACITY);
            
            if (gameManager.GetFeature(GameFeature.GameSkillsBase))
                CreateSkillPanel(SkillType.Speed, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_SPEED);

            if (gameManager.GetFeature(GameFeature.GamePlayerRegenerationTime))
                CreateSkillPanel(SkillType.Food, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_REGENERATION);

            if (gameManager.GetFeature(GameFeature.GamePlayerStamina))
                CreateSkillPanel(SkillType.Stamina, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_STAMINA);

            if (gameManager.GetFeature(GameFeature.GameOfflineTrainingTime))
                CreateSkillPanel(SkillType.OfflineTraining, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_OFFLINETRAINING);

            if (newVersion > 1150) {
                CreateSeparator();
                CreateSkillPanel(SkillType.MagLevel, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, Color.green, _magicIcon);
                CreateSkillPanel(SkillType.Fist, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_FIST, 0, 0, Color.green, _fistIcon);
                CreateSkillPanel(SkillType.Club, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_CLUB, 0, 0, Color.green, _clubIcon);
                CreateSkillPanel(SkillType.Sword, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_SWORD, 0, 0, Color.green, _swordIcon);
                CreateSkillPanel(SkillType.Axe, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_AXE, 0, 0, Color.green, _axeIcon);
                CreateSkillPanel(SkillType.Distance, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_DISTANCE, 0, 0, Color.green, _distIcon);
                CreateSkillPanel(SkillType.Shield, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, Color.green, _shieldingIcon);
                CreateSkillPanel(SkillType.Fishing, ModulesManager.Instance.ProgressIconSkillPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, Color.green, _fishingIcon);
            } else {
                CreateSkillPanel(SkillType.MagLevel, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, Color.red);
                CreateSeparator();
                CreateSkillPanel(SkillType.Fist, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_FIST_LEGACY, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Club, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_CLUB_LEGACY, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Sword, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_SWORD_LEGACY, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Axe, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_AXE_LEGACY, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Distance, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_DISTANCE_LEGACY, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Shield, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, Color.green);
                CreateSkillPanel(SkillType.Fishing, ModulesManager.Instance.ProgressSkillPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, Color.green);
            }
            
            if (gameManager.GetFeature(GameFeature.GameAdditionalSkills)) {
                CreateSeparator();

                SkillPanel[] extraPanels = new SkillPanel[6];

                CreateSkillLabel(TextResources.SKILLS_CRITICALHIT);
                extraPanels[0] = CreateSkillPanel(SkillType.CriticalHitChance, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_CRITICALHIT_CHANCE);
                extraPanels[1] = CreateSkillPanel(SkillType.CriticalHitDamage, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_CRITICALHIT_EXTRADMG);
                CreateSkillLabel(TextResources.SKILLS_LIFELEECH);
                extraPanels[2] = CreateSkillPanel(SkillType.LifeLeechChance, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_LIFELEECH_CHANCE);
                extraPanels[3] = CreateSkillPanel(SkillType.LifeLeechAmount, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_LIFELEECH_AMOUNT);
                CreateSkillLabel(TextResources.SKILLS_MANALEECH);
                extraPanels[4] = CreateSkillPanel(SkillType.ManaLeechChance, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_MANALEECH_CHANCE);
                extraPanels[5] = CreateSkillPanel(SkillType.ManaLeechAmount, ModulesManager.Instance.RawSkillPanelPrefab, TextResources.SKILLS_MANALEECH_AMOUNT);

                foreach (var skillPanel in extraPanels) {
                    var rt = skillPanel.labelText.rectTransform;
                    rt.offsetMin = new Vector2(5, rt.offsetMin.y);
                }
            }

            UpdateMaxHeight();
        }

        protected void DestroyOldComponents() {
            foreach (var it in _skillPanels)
                Destroy(it.Value);

            _skillPanels.Clear();
        }

        private SkillPanel CreateSkillPanel(SkillType type, SkillPanel prefab, string name, long value = 0, float percent = 0, Color? color = null, Sprite icon = null) {
            if (_skillPanels.ContainsKey(type))
                _skillPanels.Remove(type);

            var skillPanel = Instantiate(prefab, _skillScrollRect.content);
            skillPanel.name = string.Format("Skill_{0}", name);
            skillPanel.SetText(name);
            skillPanel.SetValue(value, percent);
            skillPanel.SetIcon(icon);

            if (color != null)
                skillPanel.SetProgressColor(color.Value);

            _skillPanels.Add(type, skillPanel);
            return skillPanel;
        }

        private GameObject CreateSeparator() {
            var newGameObject = new GameObject();
            var newRectTransform = newGameObject.AddComponent<RectTransform>();

            var horizontalSeparator = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, newRectTransform);
            var hrRectTransform = horizontalSeparator.GetComponent<RectTransform>();
            hrRectTransform.anchorMin = new Vector2(0, 0.5f);
            hrRectTransform.anchorMax = new Vector2(1, 0.5f);
            hrRectTransform.pivot = new Vector2(0.5f, 0.5f);
            hrRectTransform.anchoredPosition = Vector2.zero;

            newGameObject.AddComponent<UnityUI.LayoutElement>().minHeight = 10;
            newGameObject.transform.SetParent(_skillScrollRect.content);
            newGameObject.name = "Separator_" + newGameObject.transform.GetSiblingIndex();
            return newGameObject;
        }

        private TMPro.TextMeshProUGUI CreateSkillLabel(string text) {
            var label = Instantiate(OpenTibiaUnity.GameManager.DefaultLabel, _skillScrollRect.content);
            label.SetText(string.Format("{0}:", text));
            return label;
        }

        private void UpdateMaxHeight() {
            var conetntVerticalLayoutGroup = _skillScrollRect.content.GetComponent<UnityUI.VerticalLayoutGroup>();
            if (!conetntVerticalLayoutGroup)
                return;

            float maxComtentHeight = (_skillScrollRect.content.childCount - 1) * conetntVerticalLayoutGroup.spacing + 2;
            foreach (Transform child in _skillScrollRect.content) {
                var childLayoutElement = child.GetComponent<UnityUI.LayoutElement>();
                if (childLayoutElement)
                    maxComtentHeight += childLayoutElement.minHeight;
            }

            _maxContentHeight = maxComtentHeight;
        }

        private SkillPanel GetSkillPanelForSkillType(SkillType skillType) {
            if (_skillPanels.TryGetValue(skillType, out SkillPanel panel))
                return panel;

            return null;
        }
    }
}
