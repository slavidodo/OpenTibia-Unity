using OpenTibiaUnity.Core;
using OpenTibiaUnity.Core.Chat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    
    public class ChannelTab : Core.Components.DraggableTabButton
    {
        private static Color s_ActiveColor = Colors.ColorFromRGB(MessageColors.White);
        private static Color s_InactiveColor = Colors.ColorFromRGB(MessageColors.Grey);
        private static Color s_FlashingColor = Colors.ColorFromRGB(MessageColors.Red);
        
        private const float FlashingDuration = 2f;
        private const float FlashingReverseAfter = 1f;

        public class ChannelButtonClickedEvent : UnityEvent<ChannelTab> { }
        
        [SerializeField] private TMPro.TextMeshProUGUI _channelNameLabel = null;

        public ChannelButtonClickedEvent onClick = new ChannelButtonClickedEvent();
        public Channel Channel;
        private ChannelButtonState _state = ChannelButtonState.Active;
        private float _flashingTicks = 0;
        private float _lastFlashingTime = 0;
        private bool _lastColorIsFlashing = false;

        protected override void Start() {
            base.Start();

            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        protected void Update() {
            if (_state == ChannelButtonState.Flashing) {
                _flashingTicks += Time.deltaTime;
                if (_flashingTicks >= FlashingDuration) {
                    _state = ChannelButtonState.FlashingFinished;
                    _channelNameLabel.color = s_FlashingColor;
                    return;
                }
                
                if (Time.time - _lastFlashingTime >= FlashingReverseAfter) {
                    if (_lastColorIsFlashing)
                        _channelNameLabel.color = s_ActiveColor;
                    else
                        _channelNameLabel.color = s_FlashingColor;

                    _lastColorIsFlashing = !_lastColorIsFlashing;
                    _lastFlashingTime = Time.time;
                }
            }
        }

        public void SetText(string text) {
            _channelNameLabel.SetText(text);
            GetComponent<LayoutElement>().preferredWidth = Mathf.Max(120, _channelNameLabel.preferredWidth * 1.5f);
        }

        public void SetImage(Sprite sprite) {
            GetComponent<Image>().sprite = sprite;
        }

        public void SetState(ChannelButtonState state) {
            if (state == ChannelButtonState.FlashingFinished)
                state = ChannelButtonState.Flashing;

            _state = state;
            _flashingTicks = 0;
            _lastFlashingTime = Time.time;
            _lastColorIsFlashing = true;

            switch (state) {
                case ChannelButtonState.Active:
                    _channelNameLabel.color = s_ActiveColor;
                    break;
                case ChannelButtonState.Inactive:
                    _channelNameLabel.color = s_InactiveColor;
                    break;
                case ChannelButtonState.Flashing:
                    _channelNameLabel.color = s_FlashingColor;
                    break;
            }
        }

        private void OnButtonClick() {
            onClick.Invoke(this);
        }
    }
}
