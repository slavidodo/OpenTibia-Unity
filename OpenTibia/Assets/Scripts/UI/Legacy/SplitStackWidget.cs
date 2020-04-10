using OpenTibiaUnity.Core.Input;
using UnityEngine;
using UnityEngine.Events;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.UI.Legacy
{
    public class SplitStackWidget : PopUpBase
    {
        public class SplitStackWindowButtonEvent : UnityEvent<SplitStackWidget> { }

        // serialized fields
        [SerializeField]
        private Slider _slider = null;
        [SerializeField]
        private ItemPanel _itemPanel = null;

        // fields
        private Core.Appearances.ObjectInstance _objectInstance = null;
        private int _objectAmount = 0;
        private int _selectedAmount = 0;
        private RenderTexture _renderTexture = null;

        // properties
        public SplitStackWindowButtonEvent onOk { get; set; }
        public Core.Appearances.AppearanceType ObjectType { get; set; } = null;
        public int ObjectAmount {
            get => _objectAmount;
            set {
                if (_objectAmount != value) {
                    _objectAmount = value;
                    _slider.SetMinMax(1, _objectAmount);
                    _selectedAmount = _itemPanel.objectAmount = (int)_slider.value;
                }
            }
        }
        public int SelectedAmount {
            get => _selectedAmount;
            set {
                if (_selectedAmount != value) {
                    _selectedAmount = value;
                    _slider.value = value;
                }
            }
        }

        protected override void Awake() {
            base.Awake();

            onOk = new SplitStackWindowButtonEvent();
        }

        protected override void Start() {
            base.Start();

            AddButton(PopUpButtonMask.Ok, () => onOk.Invoke(this));
            AddButton(PopUpButtonMask.Cancel);

            _slider.onValueChanged.AddListener(OnSliderValueChanged);

            OpenTibiaUnity.GameManager.GetModule<WorldMapWidget>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        protected override void OnEnable() {
            base.OnEnable();

            OpenTibiaUnity.InputHandler.AddKeyUpListener(Core.Utils.EventImplPriority.Default, OnKeyUp);
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (OpenTibiaUnity.InputHandler != null)
                OpenTibiaUnity.InputHandler.RemoveKeyUpListener(OnKeyUp);
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            var worldmapWidget = OpenTibiaUnity.GameManager?.GetModule<WorldMapWidget>();
            if (worldmapWidget)
                worldmapWidget.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
        }

        private void OnKeyUp(Event e, bool repeat) {
            if (!InputHandler.IsHighlighted(this))
                return;

            switch (e.keyCode) {
                case KeyCode.LeftArrow:
                    SelectedAmount = Mathf.Max(0, SelectedAmount - (e.shift ? 10 : 1));
                    e.Use();
                    break;

                case KeyCode.RightArrow:
                    SelectedAmount = Mathf.Min(_objectAmount, SelectedAmount + (e.shift ? 10 : 1));
                    e.Use();
                    break;
            }
        }

        protected void OnInvalidateTRS() {
            if (!!_objectInstance)
                _objectInstance.InvalidateTRS();
        }

        protected void OnSliderValueChanged(float newValue) {
            _selectedAmount = (int)newValue;
            _itemPanel.objectAmount = _selectedAmount;
        }

        public override void Show() {
            base.Show();

            SelectedAmount = ObjectAmount;
        }
        
        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Bilinear;
                _renderTexture.Create();
                _itemPanel.image.texture = _renderTexture;
            }

            using (var commandBuffer = new CommandBuffer()) {
                commandBuffer.SetRenderTarget(_renderTexture);
                commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

                var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
                commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                    OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

                if (!!ObjectType) {
                    if (_objectInstance == null || _objectInstance.Id != ObjectType.Id)
                        _objectInstance = OpenTibiaUnity.AppearanceStorage.CreateObjectInstance(ObjectType.Id, _objectAmount);


                    _objectInstance.Draw(commandBuffer, Vector2Int.zero, 0, 0, 0);
                }

                Graphics.ExecuteCommandBuffer(commandBuffer);
            }
        }
    }
}
