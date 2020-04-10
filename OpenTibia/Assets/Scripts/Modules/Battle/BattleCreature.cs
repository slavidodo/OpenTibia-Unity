using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Battle
{
    public class BattleCreature : Core.Components.Base.AbstractComponent, IPointerEnterHandler, IPointerExitHandler
    {
        // static fields
        private static Core.Appearances.Rendering.MarksView _creaturesMarksView;

        static BattleCreature() {
            _creaturesMarksView = new Core.Appearances.Rendering.MarksView(0);
            _creaturesMarksView.AddMarkToView(MarkType.ClientBattleList, Constants.MarkThicknessBold);
            _creaturesMarksView.AddMarkToView(MarkType.Permenant, Constants.MarkThicknessBold);
        }

        // serialized field
        [SerializeField]
        private UnityUI.RawImage _image = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _nameLabel = null;
        [SerializeField]
        private UI.Legacy.Slider _healthBar = null;

        // fields
        private RenderTexture _renderTexture;
        private AppearanceInstance _outfit;

        // properties
        public string creatureName {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }

        public int healthPercent {
            get => (int)_healthBar.value;
            set {
                if (value != _healthBar.value) {
                    _healthBar.value = value;
                    UpdateHealthBarColor();
                }
            }
        }

        private Creature _creature = null;
        public Creature creature {
            get => _creature;
            set {
                if (_creature == value)
                    return;

                _creature = value;
                _outfit = _creature?.Outfit?.Clone();
                if (_creature != null) {
                    creatureName = creature.Name;
                    healthPercent = (int)creature.GetSkillValue(SkillType.HealthPercent);
                }

                if (_outfit != null) {
                    _outfit.OffsetDisabled = true; // disable offset to use all space
                    _outfit.SetClamping(true);
                }
            }
        }

        private void OnGUI() {
            var e = Event.current;
            if (e.type != EventType.Repaint || !_outfit)
                return;

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Bilinear;
                _renderTexture.Create();

                _image.texture = _renderTexture;
            }

            using (var commandBuffer = new CommandBuffer()) {
                commandBuffer.SetRenderTarget(_renderTexture);
                commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

                var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
                commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                    OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

                _outfit.Animate(OpenTibiaUnity.TicksMillis);
                _outfit.Draw(commandBuffer, Vector2Int.zero, (int)Direction.South, 0, 0);

                // here we actually draw marks after the creature
                // this is sustained for better view
                _creaturesMarksView.DrawMarks(commandBuffer, creature.Marks, 0, 0);

                Graphics.ExecuteCommandBuffer(commandBuffer);
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            if (_renderTexture != null) {
                _renderTexture.Release();
                _renderTexture = null;
            }

            var battleWidget = OpenTibiaUnity.GameManager.GetModule<BattleWidget>();
            if (battleWidget)
                battleWidget.activeWidget = null;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            OpenTibiaUnity.CreatureStorage.Aim = creature;
            OpenTibiaUnity.GameManager.GetModule<BattleWidget>().activeWidget = this;
        }

        public void OnPointerExit(PointerEventData eventData) {
            OpenTibiaUnity.CreatureStorage.Aim = null;
            OpenTibiaUnity.GameManager.GetModule<BattleWidget>().activeWidget = null;
        }

        public void UpdateMark(bool set, uint color) {
            if (!set) {
                _nameLabel.color = Core.Colors.Default;
                return;
            }

            _nameLabel.color = Core.Appearances.Rendering.MarksView.GetMarksColor(color);
        }

        private void UpdateHealthBarColor() {
            _healthBar.fillColor = Creature.GetHealthColor((int)_healthBar.value);
        }
    }
}
