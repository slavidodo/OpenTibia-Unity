using OpenTibiaUnity.Core.Chat;
using OpenTibiaUnity.Core.Components;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;

namespace OpenTibiaUnity.Modules.Console
{
    internal class ChannelMessageContextMenu : ContextMenuBase
    {
        private Channel m_Channel = null;
        private ChannelMessage m_ChannelMessage = null;
        private TMPro.TMP_InputField m_InputField = null;

        internal void Set(Channel channel, ChannelMessage channelMessage, TMPro.TMP_InputField inputField) {
            m_Channel = channel;
            m_ChannelMessage = channelMessage;
            m_InputField = inputField;
        }

        internal override void InitialiseOptions() {
            var chatStorage = OpenTibiaUnity.ChatStorage;

            bool isHuman = false;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel))
                isHuman = m_ChannelMessage.SpeakerLevel > 0;
            else // this was a bug in older clients, it treated NPCs as humans as well
                isHuman = !string.IsNullOrEmpty(m_ChannelMessage.Speaker);

            if (isHuman && m_ChannelMessage.Speaker != OpenTibiaUnity.Player.Name) {
                CreateTextItem(string.Format(TextResources.CTX_VIEW_PRIVATE_MESSAGE, m_ChannelMessage.Speaker), () => {
                    // todo, notify about expected channel :)
                    new PrivateChatActionImpl(PrivateChatActionType.OpenMessageChannel, PrivateChatActionImpl.ChatChannelNoChannel, m_ChannelMessage.Speaker).Perform();
                });

                // TODO, this should only appear if this player isn't already on the vip list //
                if (true) {
                    CreateTextItem(string.Format(TextResources.CTX_VIEW_ADD_BUDDY, m_ChannelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                }
                
                if (chatStorage.HasOwnPrivateChannel) {
                    if (m_Channel.ID == chatStorage.OwnPrivateChannelID) {
                        CreateTextItem(TextResources.CTX_VIEW_PRIVATE_INVITE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelInvite, chatStorage.OwnPrivateChannelID, m_ChannelMessage.Speaker).Perform();
                        });
                    } else {
                        CreateTextItem(TextResources.CTX_VIEW_PRIVATE_EXCLUDE, () => {
                            new PrivateChatActionImpl(PrivateChatActionType.ChatChannelExclude, chatStorage.OwnPrivateChannelID, m_ChannelMessage.Speaker).Perform();
                        });
                    }
                }
                
                var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
                if (nameFilterSet.IsBlacklisted(m_ChannelMessage.Speaker)) {
                    CreateTextItem(string.Format(TextResources.CTX_VIEW_PLAYER_UNIGNORE, m_ChannelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                } else {
                    CreateTextItem(string.Format(TextResources.CTX_VIEW_PLAYER_IGNORE, m_ChannelMessage.Speaker), "TODO", () => {
                        // TODO
                    });
                }
                
                CreateSeparatorItem();
            }

            if (!string.IsNullOrEmpty(m_ChannelMessage.Speaker)) {
                CreateTextItem(TextResources.CTX_VIEW_COPY_NAME, () => {
                    GUIUtility.systemCopyBuffer = m_ChannelMessage.Speaker;
                });
            }

            var selectedString = Core.StringHelper.GetSelection(m_InputField);
            if (!string.IsNullOrEmpty(selectedString)) {
                CreateTextItem(TextResources.CTX_VIEW_COPY_SELECTED, () => {
                    GUIUtility.systemCopyBuffer = selectedString;
                });
            }

            CreateTextItem(TextResources.CTX_VIEW_COPY_MESSAGE, () => {
                GUIUtility.systemCopyBuffer = m_ChannelMessage.PlainText;
            });

            CreateSeparatorItem();

            CreateTextItem(TextResources.CTX_VIEW_SELECT_ALL, () => {
                m_InputField.selectionStringAnchorPosition = 0;
                m_InputField.selectionStringFocusPosition = m_InputField.text.Length - 1;
            });

            // TODO(priority:verylow) add report types
        }
    }
}
