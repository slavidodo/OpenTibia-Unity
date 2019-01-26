using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleCreature : Core.Components.Base.AbstractComponent
    {
        public RawImage markImageComponent;
        public RawImage outfitImageCompoenent;

        public TMPro.TextMeshProUGUI nameTextComponent;
        public Slider healthProgressBar;
        public RawImage healthProgressFillArea;

        protected CachedSpriteInformation m_CachedSpriteInformation;
        private Creature m_Creature = null;

        public Creature Creature { get => m_Creature; }

        protected override void Start() {
            base.Start();

            healthProgressBar.minValue = 0;
            healthProgressBar.maxValue = 100;
        }

        public void UpdateDetails(Creature creature) {
            m_Creature = creature;

            nameTextComponent.text = creature.Name;
            healthProgressBar.value = creature.HealthPercent;
            healthProgressFillArea.color = CreatureStatusPanel.GetHealthColor(healthProgressBar.value / 100f);

            var outfit = creature.Outfit;
            if (outfit) {
                m_CachedSpriteInformation = outfit.GetSprite(0, (int)Directions.South, 0, 0, false);
                outfitImageCompoenent.texture = m_CachedSpriteInformation.texture;
                outfitImageCompoenent.uvRect = m_CachedSpriteInformation.rect;
            }
        }
    }
}
