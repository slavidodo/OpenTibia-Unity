using OpenTibiaUnity.Core.Creatures;
using UnityEngine;

namespace OpenTibiaUnity.Modules.HealthInfo
{
    public class HealthInfoWidget : UI.Legacy.SidebarWidget
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.Slider _healthBar = null;
        [SerializeField]
        private UI.Legacy.Slider _manaBar = null;

        protected override void Start() {
            base.Start();

            var player = OpenTibiaUnity.Player;
            if (player != null) {
                OnSkillChange(player, SkillType.Health, player.GetSkill(SkillType.Health));
                OnSkillChange(player, SkillType.Mana, player.GetSkill(SkillType.Mana));
            }
        }

        protected override void OnEnable() {
            base.OnEnable();
            Creature.onSkillChange.AddListener(OnSkillChange);
        }

        protected override void OnDisable() {
            base.OnDisable();
            Creature.onSkillChange.RemoveListener(OnSkillChange);
        }

        private void OnSkillChange(Creature creature, SkillType skillType, Skill skill) {
            var player = creature as Player;
            if (!player || (skillType != SkillType.Health && skillType != SkillType.Mana))
                return;

            var progressBar = (skillType == SkillType.Health) ? _healthBar : _manaBar;
            progressBar.SetMinMax(0, skill.BaseLevel);
            progressBar.value = skill.Level;
        }
    }
}
