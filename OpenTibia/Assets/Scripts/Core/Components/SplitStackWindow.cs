using OpenTibiaUnity.Core.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

        protected override void Start() {
            base.Start();
            _oKButton.onClick.AddListener(TriggerOk);
            _cancelButton.onClick.AddListener(TriggerCancel);
            _sliderWrapper.slider.onValueChanged.AddListener(TriggerSliderChange);

            OpenTibiaUnity.InputHandler.AddKeyUpListener(Utils.EventImplPriority.Default, (Event e, bool repeat) => {
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
            });

            onOk = new SplitStackWindowButtonEvent();
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
                _renderTexture.filterMode = FilterMode.Point;
                _itemImage.texture = _renderTexture;
            }

            RenderTexture.active = _renderTexture;
            Utils.GraphicsUtility.ClearWithTransparency();
            if (!!_objectType) {
                if (_objectInstance == null || _objectInstance.Id != _objectType._id)
                    _objectInstance = OpenTibiaUnity.AppearanceStorage.CreateObjectInstance(_objectType._id, _objectAmount);

                var zoom = new Vector2(Screen.width / (float)_renderTexture.width, Screen.height / (float)_renderTexture.height);
                _objectInstance.Draw(new Vector2(0, 0), zoom, 0, 0, 0);
            }

            RenderTexture.active = null;
        }
    }
}
