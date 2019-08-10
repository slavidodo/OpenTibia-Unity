using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Battle
{
    internal class BattleCreature : Core.Components.Base.AbstractComponent
    {
        public RawImage markImageComponent = null;
        public RawImage outfitImageCompoenent = null;

        public TMPro.TextMeshProUGUI nameTextComponent = null;
        public Slider healthProgressBar = null;
        public RawImage healthProgressFillArea = null;

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
                m_CachedSpriteInformation = outfit.GetSprite(0, (int)Direction.South, 0, 0, false);
                outfitImageCompoenent.texture = m_CachedSpriteInformation.texture;
                outfitImageCompoenent.uvRect = m_CachedSpriteInformation.rect;
            }

            var markColor = creature.Marks.GetMarkColor(MarkType.ClientBattleList);
            if (markColor == Marks.MarkUnmarked) {
                markImageComponent.gameObject.SetActive(false);
            } else {
                markImageComponent.gameObject.SetActive(true);
                markImageComponent.color = Core.Colors.ColorFromARGB(markColor);
            }
        }
    }
}
