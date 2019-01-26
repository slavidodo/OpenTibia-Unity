using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private void ParseTalk(InputMessage message) {
            uint statementID = message.GetU32();
            string speaker = message.GetString();
            ushort speakerLevel = message.GetU16();
            
            MessageModes mode = (MessageModes)message.GetU8();

            Vector3Int? absolutePosition = null;
            Utility.UnionStrInt channelID = null;

            switch (mode) {
                case MessageModes.Say:
                case MessageModes.Whisper:
                case MessageModes.Yell:
                    absolutePosition = message.GetPosition();
                    channelID = Chat.ChatStorage.LocalChannelID;
                    break;

                case MessageModes.PrivateFrom:
                    channelID = speaker;
                    break;

                case MessageModes.Channel:
                case MessageModes.ChannelHighlight:
                    channelID = message.GetU16();
                    break;

                case MessageModes.Spell:
                    absolutePosition = message.GetPosition();
                    channelID = Chat.ChatStorage.LocalChannelID;
                    break;

                case MessageModes.NpcFromStartBlock:
                    absolutePosition = message.GetPosition();
                    break;

                case MessageModes.GamemasterBroadcast:
                    break;

                case MessageModes.GamemasterChannel:
                    channelID = message.GetU16();
                    break;

                case MessageModes.GamemasterPrivateFrom:
                    channelID = speaker;
                    break;

                case MessageModes.BarkLow:
                case MessageModes.BarkLoud:
                    absolutePosition = message.GetPosition();
                    channelID = -1;
                    break;

                case MessageModes.Game:
                    break;

                default:
                    throw new System.Exception("ProtocolGame.ParseTalk: invalid message mode.");

            }

            string text = message.GetString();
            if(mode != MessageModes.NpcFromStartBlock && mode != MessageModes.NpcFrom) {
                try {
                    m_WorldMapStorage.AddOnscreenMessage(absolutePosition, (int)statementID, speaker, speakerLevel, mode, text);
                    m_ChatStorage.AddChannelMessage(channelID, (int)statementID, speaker, speakerLevel, mode, text);
                } catch (System.Exception e) {
                    throw new System.Exception("ProtocolGame.ParseTalk: Failed to add message: " + e.Message + "\n" + e.StackTrace);
                }
            } else if (mode == MessageModes.NpcFromStartBlock) {
                m_MessageStorage.StartMessageBlock(speaker, absolutePosition, text);
            } else if (mode == MessageModes.NpcFrom) {
                m_MessageStorage.AddTextToBlock(speaker, text);
            }
        }

        private void ParseChannels(InputMessage message) {
            int count = message.GetU8();
            List<Chat.Channel> channels = new List<Chat.Channel>();
            for (int i = 0; i < count; i++) {
                int id = message.GetU16();
                string name = message.GetString();
                channels.Add(new Chat.Channel(id, name, MessageModes.None));
            }
        }

        private void ParseOpenChannel(InputMessage message) {
            int channelID = message.GetU16();
            string channelName = message.GetString();
            Chat.Channel channel = m_ChatStorage.AddChannel(channelID, channelName, MessageModes.Channel);
            channel.CanModerate = true;

            int joinedUsers = message.GetU16();
            for (int i = 0; i < joinedUsers; i++)
                channel.PlayerJoined(message.GetString());

            int invitedUsers = message.GetU16();
            for (int i = 0; i < invitedUsers; i++)
                channel.PlayerInvited(message.GetString());
        }

        private void ParsePrivateChannel(InputMessage message) {
            string channelName = message.GetString();
            m_ChatStorage.AddChannel(channelName, channelName, MessageModes.PrivateFrom);
        }

        private void ParseOpenOwnChannel(InputMessage message) {
            int channelID = message.GetU16();
            string channelName = message.GetString();
            Chat.Channel channel = m_ChatStorage.AddChannel(channelID, channelName, MessageModes.Channel);
            channel.CanModerate = true;

            if (channel.IsPrivate)
                m_ChatStorage.OwnPrivateChannelID = channelID;

            int joinedUsers = message.GetU16();
            for (int i = 0; i < joinedUsers; i++)
                channel.PlayerJoined(message.GetString());

            int invitedUsers = message.GetU16();
            for (int i = 0; i < invitedUsers; i++)
                channel.PlayerInvited(message.GetString());
        }

        private void ParseCloseChannel(InputMessage message) {
            int channelID = message.GetU16();
            m_ChatStorage.CloseChannel(channelID);
        }

        private void ParseTextMessage(InputMessage message) {
            MessageModes mode = (MessageModes)message.GetU8();

            try {
                switch (mode) {
                    case MessageModes.ChannelManagment:
                        int channelID = message.GetU16();
                        string text = message.GetString();
                        // TODO name filter
                        //var regex = new System.Text.RegularExpressions.Regex(@"^(.+?) invites you to |^You have been excluded from the channel ([^']+)'s Channel\.$");
                        //var match = regex.Match(text);
                        //string speaker = match != null && match.Success ? match.Value : null;

                        m_WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        m_ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModes.Guild:
                    case MessageModes.PartyManagement:
                    case MessageModes.Party:
                        channelID = message.GetU16();
                        text = message.GetString();
                        m_WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        m_ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModes.Login:
                    case MessageModes.Admin:
                    case MessageModes.Game:
                    case MessageModes.GameHighlight:
                    case MessageModes.Failure:
                    case MessageModes.Look:
                    case MessageModes.Status:
                    case MessageModes.Loot:
                    case MessageModes.TradeNpc:
                    case MessageModes.HotkeyUse:
                        channelID = -1;
                        text = message.GetString();
                        m_WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        m_ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModes.Market:
                        text = message.GetString();
                        // TODO: market
                        break;

                    case MessageModes.Report:
                        // TODO
                        //ReportWidget.s_ReportTimestampReset();
                        text = message.GetString();
                        m_WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    case MessageModes.DamageDealed:
                    case MessageModes.DamageReceived:
                    case MessageModes.DamageOthers:
                        Vector3Int position = message.GetPosition();
                        uint value = message.GetU32();
                        uint color = message.GetU8();
                        if (value > 0)
                            m_WorldMapStorage.AddOnscreenMessage(position, -1, null, 0, mode, (int)value, color);

                        value = message.GetU32();
                        color = message.GetU8();
                        if (value > 0)
                            m_WorldMapStorage.AddOnscreenMessage(position, -1, null, 0, mode, (int)value, color);

                        text = message.GetString();
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    case MessageModes.Heal:
                    case MessageModes.Mana:
                    case MessageModes.Exp:
                    case MessageModes.HealOthers:
                    case MessageModes.ExpOthers:
                        position = message.GetPosition();
                        value = message.GetU32();
                        color = message.GetU8();
                        m_WorldMapStorage.AddOnscreenMessage(position, -1, null, 0, mode, (int)value, color);

                        text = message.GetString();
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    default:
                        throw new System.Exception("Invalid message mode " + mode + ".");
                }
            } catch (System.Exception e) {
                throw new System.Exception("Connection.readSMESSAGE: Failed to add message of type " + mode + ": " + e.Message + "\n" + e.StackTrace);
            }
        }

        private void ParseChannelEvent(InputMessage message) {
            int channelID = message.GetU16();
            Chat.Channel channel = m_ChatStorage.GetChannel(channelID);
            string playerName = message.GetString();
            int eventType = message.GetU8();

            switch (eventType) {
                case 0:
                    channel.PlayerJoined(playerName);
                    break;

                case 1:
                    channel.PlayerLeft(playerName);
                    break;
                    
                case 2:
                    channel.PlayerInvited(playerName);
                    break;

                case 3:
                    channel.PlayerExcluded(playerName);
                    break;

                case 4:
                    channel.PlayerPending(playerName);
                    break;
            }
        }
    }
}
