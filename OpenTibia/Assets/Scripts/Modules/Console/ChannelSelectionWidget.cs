using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Console
{
    public class ChannelSelectionWidget : UI.Legacy.PopUpBase
    {
        // serialized fields
        [SerializeField]
        private UI.Legacy.ScrollRect _channelScrollRect = null;
        [SerializeField]
        private TMPro.TMP_InputField _channelInput = null;
        [SerializeField]
        private UnityUI.ToggleGroup _channelToggleGroup = null;

        // fields
        private Core.Utils.UnionStrInt _currentChannelId = -1;

        protected override void Start() {
            base.Start();

            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkButtonClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel);
        }

        private void OnOkButtonClick() {
            JoinSelectedChannel();
        }

        private void OnChannelItemDoubleClick() {
            // make text empty so that it detects the selection
            _channelInput.text = "";
            Hide();
            JoinSelectedChannel();
        }

        private void OnChannelItemToggleValueChanged(ChannelSelectionItem channelItem, bool value) {
            if (value) {
                _currentChannelId = channelItem.ChannelId;
                _channelToggleGroup.allowSwitchOff = false; // you can't revert this check anymore
            }
        }

        public void Setup(List<Channel> channels) {
            foreach (var channel in channels) {
                var channelItem = Instantiate(ModulesManager.Instance.ChannelSelectionItemPrefab, _channelScrollRect.content);
                channelItem.UseAlternateColor = OpenTibiaUnity.GameManager.ClientVersion >= 1100;
                channelItem.ChannelName = channel.Name;
                channelItem.ChannelId = channel.Id;
                channelItem.onDoubleClick.AddListener(OnChannelItemDoubleClick);

                channelItem.toggle.group = _channelToggleGroup;
                channelItem.toggle.onValueChanged.AddListener((value) => OnChannelItemToggleValueChanged(channelItem, value));
            }
        }

        private void JoinSelectedChannel() {
            Core.Utils.UnionStrInt channelId;
            if (!string.IsNullOrEmpty(_channelInput.text))
                channelId = _channelInput.text;
            else if (!_currentChannelId.IsInt || _currentChannelId != -1)
                channelId = _currentChannelId;
            else
                return;

            OpenTibiaUnity.ChatStorage.JoinChannel(channelId);
        }

        public override void Hide() {
            base.Hide();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => Destroy(gameObject));
        }
    }
}
