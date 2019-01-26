using OpenTibiaUnity.Core.Creatures;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.HealthInfo
{
    public class HealthInfoWidget : Core.Components.Base.AbstractComponent
    {
        public const int BarWidth = 94;

#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RawImage m_HealthBarImageComponent;
        [SerializeField] private RawImage m_ManaBarImageComponent;

        [SerializeField] private TMPro.TextMeshProUGUI m_HealthValueText;
        [SerializeField] private TMPro.TextMeshProUGUI m_ManaValueText;
#pragma warning restore CS0649 // never assigned to

        protected override void Awake() {
            base.Awake();
            Creature.onSkillChange.AddListener(OnSkillChange);
        }

        private void OnSkillChange(Creature creature, SkillTypes skillType, SkillStruct skill) {
            var player = creature as Player;
            if (!player || (skillType != SkillTypes.Health && skillType != SkillTypes.Mana))
                return;

            RawImage imageComponent;
            TMPro.TextMeshProUGUI textComponent;
            if (skillType == SkillTypes.Health) {
                imageComponent = m_HealthBarImageComponent;
                textComponent = m_HealthValueText;
            } else {
                imageComponent = m_ManaBarImageComponent;
                textComponent = m_ManaValueText;
            }

            var rectTransform = imageComponent.GetComponent<RectTransform>();

            // setting new width
            var percent = skill.level / (float)skill.baseLevel;
            var rect = new Rect(imageComponent.uvRect);
            rect.width = percent;
            imageComponent.uvRect = rect;

            rect = new Rect(rectTransform.rect);
            rect.width = BarWidth * percent;
            rectTransform.sizeDelta = new Vector2(BarWidth * percent, rectTransform.sizeDelta.y);

            // setting text
            textComponent.text = skill.level.ToString();
        }
    }
}
