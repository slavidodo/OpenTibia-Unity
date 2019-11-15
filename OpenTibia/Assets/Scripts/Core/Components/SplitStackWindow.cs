using OpenTibiaUnity.Core.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Components
{
    public class SplitStackWindow : Base.Window
    {
        public class SplitStackWindowButtonEvent : UnityEvent<SplitStackWindow> { }
        
        [SerializeField] private Button _oKButton = null;
        [SerializeField] private Button _cancelButton = null;
        [SerializeField] private SliderWrapper _sliderWrapper = null;
        [SerializeField] private RawImage _itemImage = null;

        private Appearances.AppearanceType _objectType = null;
        private Appearances.ObjectInstance _objectInstance = null;
        private int _objectAmount = 0;
        private int _selectedAmount = 0;

        private int _screenWidth = 0;
        private int _screenHeight = 0;
        private Matrix4x4 _viewMatrix;

        private RenderTexture _renderTexture = null;

        public SplitStackWindowButtonEvent onOk;

        public Appearances.AppearanceType ObjectType {
            get => _objectType;
            set {
                _objectType = value;
            }
        }

        public int ObjectAmount {
            get => _objectAmount;
            set {
                if (_objectAmount != value) {
                    _objectAmount = value;
                    _sliderWrapper.SetMinMax(1, _objectAmount);
                }
            }
        }

        public int SelectedAmount {
            get => _selectedAmount;
            set {
                if (_selectedAmount != value) {
                    _selectedAmount = value;
                    _sliderWrapper.slider.value = value;
                }
            }
        }

        protected override void Awake() {
            base.Awake();

            // setup input
            OpenTibiaUnity.InputHandler.AddKeyUpListener(Utils.EventImplPriority.Default, OnKeyUp);
        }

        protected override void Start() {
            base.Start();
            _oKButton.onClick.AddListener(TriggerOk);
            _cancelButton.onClick.AddListener(TriggerCancel);
            _sliderWrapper.slider.onValueChanged.AddListener(TriggerSliderChange);

            onOk = new SplitStackWindowButtonEvent();
        }

        private void OnKeyUp(Event e, bool repeat) {
            if (!InputHandler.IsHighlighted(this))
                return;

            switch (e.keyCode) {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    TriggerOk();
                    break;

                case KeyCode.Escape:
                    TriggerCancel();
                    break;

                case KeyCode.LeftArrow:
                    SelectedAmount = Mathf.Max(0, SelectedAmount - (e.shift ? 10 : 1));
                    break;

                case KeyCode.RightArrow:
                    SelectedAmount = Mathf.Min(_objectAmount, SelectedAmount + (e.shift ? 10 : 1));
                    break;
            }
        }

        protected void Update() {
            if (Screen.width != _screenWidth || Screen.height != _screenHeight) {
                _screenWidth = Screen.width;
                _screenHeight = Screen.height;
                var zoom = new Vector2(Screen.width / (float)Constants.FieldSize, Screen.height / (float)Constants.FieldSize);
                _viewMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom)
                    * OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix;

                if (!!_objectInstance)
                    _objectInstance.InvalidateTRS();
            }
        }

        protected void TriggerOk() {
            onOk.Invoke(this);
            Hide();
        }

        protected void TriggerCancel() {
            Hide();
        }

        protected void TriggerSliderChange(float newValue) {
            _selectedAmount = (int)newValue;
        }

        public override void Show() {
            base.Show();

            SelectedAmount = 1;
        }
        
        protected void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            if (_renderTexture == null) {
                _renderTexture = new RenderTexture(Constants.FieldSize, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Bilinear;
                _renderTexture.Create();
                _itemImage.texture = _renderTexture;
            }

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(_renderTexture);
            commandBuffer.ClearRenderTarget(false, true, Utils.GraphicsUtility.TransparentColor);
            commandBuffer.SetViewMatrix(_viewMatrix);

            if (!!_objectType) {
                if (_objectInstance == null || _objectInstance.Id != _objectType.Id)
                    _objectInstance = OpenTibiaUnity.AppearanceStorage.CreateObjectInstance(_objectType.Id, _objectAmount);

                
                _objectInstance.Draw(commandBuffer, Vector2Int.zero, 0, 0, 0);
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();
        }
    }
}
