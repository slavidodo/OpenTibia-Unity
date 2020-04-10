using OpenTibiaUnity.Core;
using OpenTibiaUnity.Core.Chat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
    public enum ChannelButtonState
    {
        Active,
        Inactive,
        Flashing,

        // public only
        FlashingFinished,
    }
    
    [RequireComponent(typeof(UnityUI.Image))]
    public class ChannelTab : UI.Legacy.DraggableTabButton
    {
        // constants
        private static Color s_InactiveColor = Colors.ColorFromRGB(MessageColors.Grey);
        
        private const float FlashingDuration = 2f;
        private const float FlashingReverseAfter = 1f;

        // serialized fields
        [SerializeField]
        private UnityUI.Image _activeFixImage = null;

        // fields
        private ChannelButtonState _state = ChannelButtonState.Active;
        private float _flashingTicks = 0;
        private float _lastFlashingTime = 0;
        private bool _lastColorIsFlashing = false;
        private Channel _channel = null;
        private Sprite _activeSprite = null;
        private Sprite _inactiveSprite = null;

        // public fields
        public UnityEvent onPointerDown = new UnityEvent();

        // properties
        public Channel channel {
            get => _channel;
            set {
                if (_channel != value) {
                    _channel = value;
                    text = _channel.Name;
                    layoutElement.preferredWidth = Mathf.Max(120, preferredValues.x * 1.5f);
                }
            }
        }

        public Sprite activeSprite { set => _activeSprite = value; }
        public Sprite inactiveSprite { set => _inactiveSprite = value; }

        public ChannelButtonState state {
            get => _state;
            set {
                if (value == ChannelButtonState.FlashingFinished)
                    value = ChannelButtonState.Flashing;

                _state = value;
                _flashingTicks = 0;
                _lastFlashingTime = Time.time;
                _lastColorIsFlashing = true;

                switch (_state) {
                    case ChannelButtonState.Active:
                        color = Color.white;
                        image.sprite = _activeSprite;
                        break;
                    case ChannelButtonState.Inactive:
                        color = s_InactiveColor;
                        image.sprite = _inactiveSprite;
                        break;
                    case ChannelButtonState.Flashing:
                        color = Colors.Red;
                        break;
                }

                _activeFixImage.gameObject.SetActive(_state == ChannelButtonState.Active);
            }
        }

        protected override void Start() {
            base.Start();

            AllowLabelTransitions = false;
        }

        protected void Update() {
            // todo, replace with InvokeRepeating
            if (_state == ChannelButtonState.Flashing) {
                _flashingTicks += Time.deltaTime;
                if (_flashingTicks >= FlashingDuration) {
                    _state = ChannelButtonState.FlashingFinished;
                    color = Colors.Red;
                    return;
                }
                
                if (Time.time - _lastFlashingTime >= FlashingReverseAfter) {
                    if (_lastColorIsFlashing)
                        color = Color.white;
                    else
                        color = Colors.Red;

                    _lastColorIsFlashing = !_lastColorIsFlashing;
                    _lastFlashingTime = Time.time;
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData) {
            base.OnPointerDown(eventData);

            onPointerDown.Invoke();
        }
    }
}
