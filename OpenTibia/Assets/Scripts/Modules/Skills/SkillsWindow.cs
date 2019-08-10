using System;
using OpenTibiaUnity.Core.Components.Base;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Skills
{
    internal class SkillsWindow : MiniWindow
    {
        [SerializeField] private SkillRawPanel m_SkillRawPanelPrefab = null;
        [SerializeField] private SkillProgressPanel m_SkillProgressPanelPrefab = null;
        [SerializeField] private SkillProgressIconPanel m_SkillProgressIconPanelPrefab = null;

        [SerializeField] private Sprite m_MagicIcon = null;
        [SerializeField] private Sprite m_FistIcon = null;
        [SerializeField] private Sprite m_ClubIcon = null;
        [SerializeField] private Sprite m_SwordIcon = null;
        [SerializeField] private Sprite m_AxeIcon = null;
        [SerializeField] private Sprite m_DistIcon = null;
        [SerializeField] private Sprite m_ShieldingIcon = null;
        [SerializeField] private Sprite m_FishingIcon = null;

        private SkillPanel m_ExpPanel = null;
        private SkillPanel m_LevelPanel = null;
        private SkillPanel m_ExpGainRatePanel = null;
        private SkillPanel m_HitPointsPanel = null;
        private SkillPanel m_ManaPanel = null;
        private SkillPanel m_SoulPointsPanel = null;
        private SkillPanel m_CapacityPanel = null;
        private SkillPanel m_SpeedPanel = null;
        private SkillPanel m_RegenerationPanel = null;
        private SkillPanel m_StaminaPanel = null;

        // known skills
        private SkillPanel m_MagicPanel = null;
        private SkillPanel m_FistPanel = null;
        private SkillPanel m_ClubPanel = null;
        private SkillPanel m_SwordPanel = null;
        private SkillPanel m_AxePanel = null;
        private SkillPanel m_DistancePanel = null;
        private SkillPanel m_ShieldingPanel = null;
        private SkillPanel m_FishingPanel = null;

        private SkillPanel m_CriticalChancePanel = null;
        private SkillPanel m_CriticalExtraDamagePanel = null;
        private SkillPanel m_LifeLeechChancePanel = null;
        private SkillPanel m_LifeLeechAmountPanel = null;
        private SkillPanel m_ManaLeechChancePanel = null;
        private SkillPanel m_ManaLeechAmountPanel = null;

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
                m_LevelPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_LEVEL);
                m_ExpPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_EXPERIENCE_MINIMAL);

                // exp panel is yellow if experienceBonus > 0 (10.54+ and was removed at some point)
            } else {
                m_ExpPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_LEVEL);
                m_LevelPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_EXPERIENCE);
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceGain))
                m_ExpGainRatePanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_XPGAIN);

            m_HitPointsPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_HITPOINTS);
            m_ManaPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_MANA);
            m_SoulPointsPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_SOULPOINTS);
            m_CapacityPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_CAPACITY);
            
            if (gameManager.GetFeature(GameFeature.GameSkillsBase))
                m_SpeedPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_SPEED);

            if (gameManager.GetFeature(GameFeature.GamePlayerRegenerationTime))
                m_RegenerationPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_REGENERATION);

            if (gameManager.GetFeature(GameFeature.GamePlayerStamina))
                m_StaminaPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_STAMINA);
            
            CreateSeparator();
            if (newVersion > 1150) {
                m_MagicPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, greenColor, m_MagicIcon);
                m_FistPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_FIST, 0, 0, greenColor, m_FistIcon);
                m_ClubPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_CLUB, 0, 0, greenColor, m_ClubIcon);
                m_SwordPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_SWORD, 0, 0, greenColor, m_SwordIcon);
                m_AxePanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_AXE, 0, 0, greenColor, m_AxeIcon);
                m_DistancePanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_DISTANCE, 0, 0, greenColor, m_DistIcon);
                m_ShieldingPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, greenColor, m_ShieldingIcon);
                m_FishingPanel = CreateSkillPanel(m_SkillProgressIconPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, greenColor, m_FishingIcon);
            } else {
                m_MagicPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_MAGIC, 0, 0, greenColor);
                m_FistPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_FIST_LEGACY, 0, 0, greenColor);
                m_ClubPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_CLUB_LEGACY, 0, 0, greenColor);
                m_SwordPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_SWORD_LEGACY, 0, 0, greenColor);
                m_AxePanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_AXE_LEGACY, 0, 0, greenColor);
                m_DistancePanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_DISTANCE_LEGACY, 0, 0, greenColor);
                m_ShieldingPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_SHIELDING, 0, 0, greenColor);
                m_ShieldingPanel = CreateSkillPanel(m_SkillProgressPanelPrefab, TextResources.SKILLS_FISHING, 0, 0, greenColor);
            }
            
            if (gameManager.GetFeature(GameFeature.GameAdditionalSkills)) {
                CreateSeparator();

                CreateSkillLabel(TextResources.SKILLS_CRITICALHIT);
                m_CriticalChancePanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_CHANCE);
                m_CriticalExtraDamagePanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_CRITICALHIT_EXTRADMG);
                CreateSkillLabel(TextResources.SKILLS_LIFELEECH);
                m_LifeLeechChancePanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_CHANCE);
                m_LifeLeechAmountPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_LIFELEECH_AMOUNT);
                CreateSkillLabel(TextResources.SKILLS_MANALEECH);
                m_ManaLeechChancePanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_MANALEECH_CHANCE);
                m_ManaLeechAmountPanel = CreateSkillPanel(m_SkillRawPanelPrefab, TextResources.SKILLS_MANALEECH_AMOUNT);

                var skillPanels = new SkillPanel[] {
                    m_CriticalChancePanel, m_CriticalExtraDamagePanel,
                    m_LifeLeechChancePanel, m_LifeLeechAmountPanel,
                    m_ManaLeechChancePanel, m_ManaLeechAmountPanel
                };

                foreach (var skillPanel in skillPanels) {
                    var rt = skillPanel.labelText.rectTransform;
                    rt.offsetMin = new Vector2(5, rt.offsetMin.y);
                }
            }

            UpdateMaxHeight();
        }

        protected void DestroyOldComponents() {
            if (m_ExpPanel) { Destroy(m_ExpPanel.gameObject); m_ExpPanel = null; }
            if (m_LevelPanel) { Destroy(m_LevelPanel.gameObject); m_LevelPanel = null; }
            if (m_ExpGainRatePanel) { Destroy(m_ExpGainRatePanel.gameObject); m_ExpGainRatePanel = null; }
            if (m_HitPointsPanel) { Destroy(m_HitPointsPanel.gameObject); m_HitPointsPanel = null; }
            if (m_ManaPanel) { Destroy(m_ManaPanel.gameObject); m_ManaPanel = null; }
            if (m_SoulPointsPanel) { Destroy(m_SoulPointsPanel.gameObject); m_SoulPointsPanel = null; }
            if (m_CapacityPanel) { Destroy(m_CapacityPanel.gameObject); m_CapacityPanel = null; }
            if (m_SpeedPanel) { Destroy(m_SpeedPanel.gameObject); m_SpeedPanel = null; }
            if (m_RegenerationPanel) { Destroy(m_RegenerationPanel.gameObject); m_RegenerationPanel = null; }
            if (m_StaminaPanel) { Destroy(m_StaminaPanel.gameObject); m_StaminaPanel = null; }

            if (m_MagicPanel) { Destroy(m_MagicPanel.gameObject); m_MagicPanel = null; }
            if (m_FistPanel) { Destroy(m_FistPanel.gameObject); m_FistPanel = null; }
            if (m_ClubPanel) { Destroy(m_ClubPanel.gameObject); m_ClubPanel = null; }
            if (m_SwordPanel) { Destroy(m_SwordPanel.gameObject); m_SwordPanel = null; }
            if (m_AxePanel) { Destroy(m_AxePanel.gameObject); m_AxePanel = null; }
            if (m_DistancePanel) { Destroy(m_DistancePanel.gameObject); m_DistancePanel = null; }
            if (m_ShieldingPanel) { Destroy(m_ShieldingPanel.gameObject); m_ShieldingPanel = null; }

            if (m_CriticalChancePanel) { Destroy(m_CriticalChancePanel.gameObject); m_CriticalChancePanel = null; }
            if (m_CriticalExtraDamagePanel) { Destroy(m_CriticalExtraDamagePanel.gameObject); m_CriticalExtraDamagePanel = null; }
            if (m_LifeLeechChancePanel) { Destroy(m_LifeLeechChancePanel.gameObject); m_LifeLeechChancePanel = null; }
            if (m_LifeLeechAmountPanel) { Destroy(m_LifeLeechAmountPanel.gameObject); m_LifeLeechAmountPanel = null; }
            if (m_ManaLeechChancePanel) { Destroy(m_ManaLeechChancePanel.gameObject); m_ManaLeechChancePanel = null; }
            if (m_ManaLeechAmountPanel) { Destroy(m_ManaLeechAmountPanel.gameObject); m_ManaLeechAmountPanel = null; }

            foreach (Transform child in m_PanelContent) {
                Destroy(child.gameObject);
            }
        }

        private SkillPanel CreateSkillPanel(SkillPanel prefab, string name, long value = 0, float percent = 0, Color? color = null, Sprite icon = null) {
            var skillPanel = Instantiate(prefab, m_PanelContent);
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
            newGameObject.transform.parent = m_PanelContent;
            newGameObject.name = "Separator_" + newGameObject.transform.GetSiblingIndex();
            return newGameObject;
        }

        private TMPro.TextMeshProUGUI CreateSkillLabel(string text) {
            var label = Instantiate(OpenTibiaUnity.GameManager.DefaultLabel, m_PanelContent);
            label.SetText(string.Format("{0}:", text));
            return label;
        }

        private void UpdateMaxHeight() {
            var conetntVerticalLayoutGroup = m_PanelContent.GetComponent<VerticalLayoutGroup>();
            if (!conetntVerticalLayoutGroup)
                return;

            float maxComtentHeight = (m_PanelContent.childCount - 1) * conetntVerticalLayoutGroup.spacing + 2;
            foreach (Transform child in m_PanelContent) {
                var childLayoutElement = child.GetComponent<LayoutElement>();
                if (childLayoutElement)
                    maxComtentHeight += childLayoutElement.minHeight;
            }

            m_MaxContentHeight = maxComtentHeight;
        }

        private SkillPanel GetSkillPanelForSkillType(SkillType skillType) {
            switch (skillType) {
                case SkillType.Level: return m_LevelPanel;
                case SkillType.Experience: return m_ExpPanel;
                case SkillType.ExperienceGain: return m_ExpGainRatePanel;
                case SkillType.Health: return m_HitPointsPanel;
                case SkillType.Mana: return m_ManaPanel;
                case SkillType.SoulPoints: return m_SoulPointsPanel;
                case SkillType.Capacity: return m_CapacityPanel;
                case SkillType.Speed: return m_SpeedPanel;
                case SkillType.Food: return m_RegenerationPanel;
                case SkillType.Stamina: return m_StaminaPanel;
                case SkillType.MagLevel: return m_MagicPanel;
                case SkillType.Fist: return m_FistPanel;
                case SkillType.Club: return m_ClubPanel;
                case SkillType.Sword: return m_SwordPanel;
                case SkillType.Axe: return m_AxePanel;
                case SkillType.Distance: return m_DistancePanel;
                case SkillType.Shield: return m_ShieldingPanel;
                case SkillType.CriticalHitChance: return m_CriticalChancePanel;
                case SkillType.CriticalHitDamage: return m_CriticalExtraDamagePanel;
                case SkillType.LifeLeechChance: return m_LifeLeechChancePanel;
                case SkillType.LifeLeechAmount: return m_LifeLeechAmountPanel;
                case SkillType.ManaLeechChance: return m_ManaLeechChancePanel;
                case SkillType.ManaLeechAmount: return m_ManaLeechAmountPanel;
            }

            return null;
        }
    }
}
