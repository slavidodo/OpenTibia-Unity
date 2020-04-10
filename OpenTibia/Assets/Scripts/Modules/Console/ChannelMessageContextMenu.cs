using OpenTibiaUnity.Core.Chat;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;

using TR = OpenTibiaUnity.TextResources;

namespace OpenTibiaUnity.Modules.Console
{
    public class ChannelMessageContextMenu : UI.Legacy.ContextMenuBase
    {
        private Channel _channel = null;
        private ChannelMessage _channelMessage = null;
        private TMPro.TMP_InputField _inputField = null;

        public void Set(Channel channel, ChannelMessage channelMessage, TMPro.TMP_InputField inputField) {
            _channel = channel;
            _channelMessage = channelMessage;
            _inputField = inputField;
        }

        public override void InitialiseOptions() {
            var chatStorage = OpenTibiaUnity.ChatStorage;

            bool isHuman = false;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel))
                isHuman = _channelMessage.SpeakerLevel > 0;
            else // this was a bug in older clients, it treated NPCs as humans as well
                isHuman = !string.IsNullOrEmpty(_channelMessage.Speaker);

            if (isHuman && _channelMessage.Speaker != OpenTibiaUnity.Player.Name) {
                CreateTextItem(string.Format(TR.CTX_VIEW_PRIVATE_MESSAGE, _channelMessage.Speaker), () => {
                    // todo, notify about expected channel :)
                    new PrivateChatActionImpl(PrivateChatActionType.OpenMessageChannel, PrivateChatActionImpl.ChatChannelNoChannel, _channelMessage.Speaker).Perform();
                });

                // TODO, this should only appear if this player isn't already on the vip list //
                if (true) {
                    CreateTextItem(string.Format(TR.CTX_VIEW_ADD_BUDDY, _channelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                }
                
                if (chatStorage.HasOwnPrivateChannel) {
                    if (_channel.Id != chatStorage.OwnPrivateChannelId) {
                        CreateTextItem(TR.CTX_VIEW_PRIVATE_INVITE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelInvite, chatStorage.OwnPrivateChannelId, _channelMessage.Speaker).Perform();
                        });
                    } else {
                        CreateTextItem(TR.CTX_VIEW_PRIVATE_EXCLUDE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelExclude, chatStorage.OwnPrivateChannelId, _channelMessage.Speaker).Perform();
                        });
                    }
                }
                
                var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
                if (nameFilterSet.IsBlacklisted(_channelMessage.Speaker)) {
                    CreateTextItem(string.Format(TR.CTX_VIEW_PLAYER_UNIGNORE, _channelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                } else {
                    CreateTextItem(string.Format(TR.CTX_VIEW_PLAYER_IGNORE, _channelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                }
                
                CreateSeparatorItem();
            }

            if (!string.IsNullOrEmpty(_channelMessage.Speaker)) {
                CreateTextItem(TR.CTX_VIEW_COPY_NAME, () => {
                    GUIUtility.systemCopyBuffer = _channelMessage.Speaker;
                });
            }

            var selectedString = Core.StringHelper.GetSelection(_inputField);
            if (!string.IsNullOrEmpty(selectedString)) {
                CreateTextItem(TR.CTX_VIEW_COPY_SELECTED, () => {
                    GUIUtility.systemCopyBuffer = selectedString;
                });
            }

            CreateTextItem(TR.CTX_VIEW_COPY_MESSAGE, () => {
                GUIUtility.systemCopyBuffer = _channelMessage.PlainText;
            });

            CreateSeparatorItem();

            CreateTextItem(TR.CTX_VIEW_SELECT_ALL, () => {
                _inputField.selectionStringAnchorPosition = 0;
                _inputField.selectionStringFocusPosition = _inputField.text.Length - 1;
            });

            // TODO(priority:verylow) add report types
        }
    }
}
