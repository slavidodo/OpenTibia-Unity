using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleCreature : Core.Components.Base.AbstractComponent
    {
        public RawImage markImageComponent = null;
        public RawImage outfitImageCompoenent = null;

        public TMPro.TextMeshProUGUI nameTextComponent = null;
        public Slider healthProgressBar = null;
        public RawImage healthProgressFillArea = null;

        protected CachedSpriteInformation _cachedSpriteInformation;
        private Creature _creature = null;

        public Creature Creature { get => _creature; }

        protected override void Start() {
            base.Start();

            healthProgressBar.minValue = 0;
            healthProgressBar.maxValue = 100;
        }

        public void UpdateDetails(Creature creature) {
            _creature = creature;

            nameTextComponent.text = creature.Name;
            healthProgressBar.value = creature.HealthPercent;
            healthProgressFillArea.color = CreatureStatusPanel.GetHealthColor(healthProgressBar.value / 100f);

            var outfit = creature.Outfit;
            if (outfit) {
                _cachedSpriteInformation = outfit.GetSprite(0, (int)Direction.South, 0, 0, false);
                outfitImageCompoenent.texture = _cachedSpriteInformation.texture;
                outfitImageCompoenent.uvRect = _cachedSpriteInformation.rect;
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
