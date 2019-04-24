using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private Dictionary<MessageModes, int> m_MessageModesDict;

        private void BuildMessageNodesMap(int version) {
            if (m_MessageModesDict == null)
                m_MessageModesDict = new Dictionary<MessageModes, int>();
            else
                m_MessageModesDict.Clear();

            if (version >= 1094) {
                m_MessageModesDict.Add(MessageModes.Mana, 43);
            }

            if (version >= 1055) { // might be 1054
                m_MessageModesDict.Add(MessageModes.None, 0);
                m_MessageModesDict.Add(MessageModes.Say, 1);
                m_MessageModesDict.Add(MessageModes.Whisper, 2);
                m_MessageModesDict.Add(MessageModes.Yell, 3);
                m_MessageModesDict.Add(MessageModes.PrivateFrom, 4);
                m_MessageModesDict.Add(MessageModes.PrivateTo, 5);
                m_MessageModesDict.Add(MessageModes.ChannelManagement, 6);
                m_MessageModesDict.Add(MessageModes.Channel, 7);
                m_MessageModesDict.Add(MessageModes.ChannelHighlight, 8);
                m_MessageModesDict.Add(MessageModes.Spell, 9);
                m_MessageModesDict.Add(MessageModes.NpcFromStartBlock, 10);
                m_MessageModesDict.Add(MessageModes.NpcFrom, 11);
                m_MessageModesDict.Add(MessageModes.NpcTo, 12);
                m_MessageModesDict.Add(MessageModes.GamemasterBroadcast, 13);
                m_MessageModesDict.Add(MessageModes.GamemasterChannel, 14);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateFrom, 15);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateTo, 16);
                m_MessageModesDict.Add(MessageModes.Login, 17);
                m_MessageModesDict.Add(MessageModes.Admin, 18);
                m_MessageModesDict.Add(MessageModes.Game, 19);
                m_MessageModesDict.Add(MessageModes.GameHighlight, 20);
                m_MessageModesDict.Add(MessageModes.Failure, 21);
                m_MessageModesDict.Add(MessageModes.Look, 22);
                m_MessageModesDict.Add(MessageModes.DamageDealed, 23);
                m_MessageModesDict.Add(MessageModes.DamageReceived, 24);
                m_MessageModesDict.Add(MessageModes.Heal, 25);
                m_MessageModesDict.Add(MessageModes.Exp, 26);
                m_MessageModesDict.Add(MessageModes.DamageOthers, 27);
                m_MessageModesDict.Add(MessageModes.HealOthers, 28);
                m_MessageModesDict.Add(MessageModes.ExpOthers, 29);
                m_MessageModesDict.Add(MessageModes.Status, 30);
                m_MessageModesDict.Add(MessageModes.Loot, 31);
                m_MessageModesDict.Add(MessageModes.TradeNpc, 32);
                m_MessageModesDict.Add(MessageModes.Guild, 33);
                m_MessageModesDict.Add(MessageModes.PartyManagement, 34);
                m_MessageModesDict.Add(MessageModes.Party, 35);
                m_MessageModesDict.Add(MessageModes.BarkLow, 36);
                m_MessageModesDict.Add(MessageModes.BarkLoud, 37);
                m_MessageModesDict.Add(MessageModes.Report, 38);
                m_MessageModesDict.Add(MessageModes.HotkeyUse, 39);
                m_MessageModesDict.Add(MessageModes.TutorialHint, 40);
                m_MessageModesDict.Add(MessageModes.Thankyou, 41);
                m_MessageModesDict.Add(MessageModes.Market, 42);
            } else if (version >= 1036) {
                for (var i = MessageModes.None; i <= MessageModes.BeyondLast; ++i) {
                    if (i >= MessageModes.NpcTo)
                        m_MessageModesDict.Add(i, (int)i + 1);
                    else
                        m_MessageModesDict.Add(i, (int)i);
                }
            } else if (version >= 900) {
                for (var i = MessageModes.None; i <= MessageModes.BeyondLast; ++i)
                    m_MessageModesDict.Add(i, (int)i);
            } else if (version >= 861) {
                m_MessageModesDict.Add(MessageModes.None, 0);
                m_MessageModesDict.Add(MessageModes.Say, 1);
                m_MessageModesDict.Add(MessageModes.Whisper, 2);
                m_MessageModesDict.Add(MessageModes.Yell, 3);
                m_MessageModesDict.Add(MessageModes.NpcTo, 4);
                m_MessageModesDict.Add(MessageModes.NpcFrom, 5);
                m_MessageModesDict.Add(MessageModes.PrivateFrom, 6);
                m_MessageModesDict.Add(MessageModes.PrivateTo, 6);
                m_MessageModesDict.Add(MessageModes.Channel, 7);
                m_MessageModesDict.Add(MessageModes.ChannelManagement, 8);
                m_MessageModesDict.Add(MessageModes.GamemasterBroadcast, 9);
                m_MessageModesDict.Add(MessageModes.GamemasterChannel, 10);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateFrom, 11);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateTo, 11);
                m_MessageModesDict.Add(MessageModes.ChannelHighlight, 12);
                m_MessageModesDict.Add(MessageModes.MonsterSay, 13);
                m_MessageModesDict.Add(MessageModes.MonsterYell, 14);
                m_MessageModesDict.Add(MessageModes.Admin, 15);
                m_MessageModesDict.Add(MessageModes.Game, 16);
                m_MessageModesDict.Add(MessageModes.Login, 17);
                m_MessageModesDict.Add(MessageModes.Status, 18);
                m_MessageModesDict.Add(MessageModes.Look, 19);
                m_MessageModesDict.Add(MessageModes.Failure, 20);
                m_MessageModesDict.Add(MessageModes.Blue, 21);
                m_MessageModesDict.Add(MessageModes.Red, 22);
            } else if (version >= 840) {
                m_MessageModesDict.Add(MessageModes.None, 0);
                m_MessageModesDict.Add(MessageModes.Say, 1);
                m_MessageModesDict.Add(MessageModes.Whisper, 2);
                m_MessageModesDict.Add(MessageModes.Yell, 3);
                m_MessageModesDict.Add(MessageModes.NpcTo, 4);
                m_MessageModesDict.Add(MessageModes.NpcFromStartBlock, 5);
                m_MessageModesDict.Add(MessageModes.PrivateFrom, 6);
                m_MessageModesDict.Add(MessageModes.PrivateTo, 6);
                m_MessageModesDict.Add(MessageModes.Channel, 7);
                m_MessageModesDict.Add(MessageModes.ChannelManagement, 8);
                m_MessageModesDict.Add(MessageModes.RVRChannel, 9);
                m_MessageModesDict.Add(MessageModes.RVRAnswer, 10);
                m_MessageModesDict.Add(MessageModes.RVRContinue, 11);
                m_MessageModesDict.Add(MessageModes.GamemasterBroadcast, 12);
                m_MessageModesDict.Add(MessageModes.GamemasterChannel, 13);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateFrom, 14);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateTo, 14);
                m_MessageModesDict.Add(MessageModes.ChannelHighlight, 15);
                // 16, 17 ??
                m_MessageModesDict.Add(MessageModes.Red, 18);
                m_MessageModesDict.Add(MessageModes.MonsterSay, 19);
                m_MessageModesDict.Add(MessageModes.MonsterYell, 20);
                m_MessageModesDict.Add(MessageModes.Admin, 21);
                m_MessageModesDict.Add(MessageModes.Game, 22);
                m_MessageModesDict.Add(MessageModes.Login, 23);
                m_MessageModesDict.Add(MessageModes.Status, 24);
                m_MessageModesDict.Add(MessageModes.Look, 25);
                m_MessageModesDict.Add(MessageModes.Failure, 26);
                m_MessageModesDict.Add(MessageModes.Blue, 27);
            } else if (version >= 760) {
                m_MessageModesDict.Add(MessageModes.None, 0);
                m_MessageModesDict.Add(MessageModes.Say, 1);
                m_MessageModesDict.Add(MessageModes.Whisper, 2);
                m_MessageModesDict.Add(MessageModes.Yell, 3);
                m_MessageModesDict.Add(MessageModes.PrivateFrom, 4);
                m_MessageModesDict.Add(MessageModes.PrivateTo, 4);
                m_MessageModesDict.Add(MessageModes.Channel, 5);
                m_MessageModesDict.Add(MessageModes.RVRChannel, 6);
                m_MessageModesDict.Add(MessageModes.RVRAnswer, 7);
                m_MessageModesDict.Add(MessageModes.RVRContinue, 8);
                m_MessageModesDict.Add(MessageModes.GamemasterBroadcast, 9);
                m_MessageModesDict.Add(MessageModes.GamemasterChannel, 10);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateFrom, 11);
                m_MessageModesDict.Add(MessageModes.GamemasterPrivateTo, 11);
                m_MessageModesDict.Add(MessageModes.ChannelHighlight, 12);
                // 13, 14, 15 ??
                m_MessageModesDict.Add(MessageModes.MonsterSay, 16);
                m_MessageModesDict.Add(MessageModes.MonsterYell, 17);
                m_MessageModesDict.Add(MessageModes.Admin, 18);
                m_MessageModesDict.Add(MessageModes.Game, 19);
                m_MessageModesDict.Add(MessageModes.Login, 20);
                m_MessageModesDict.Add(MessageModes.Status, 21);
                m_MessageModesDict.Add(MessageModes.Look, 22);
                m_MessageModesDict.Add(MessageModes.Failure, 23);
                m_MessageModesDict.Add(MessageModes.Blue, 24);
                m_MessageModesDict.Add(MessageModes.Red, 25);
            }
        }
        
        private MessageModes TranslateMessageModeFromServer(int mode) {
            foreach (var p in m_MessageModesDict) {
                if (p.Value == mode)
                    return p.Key;
            }
            return MessageModes.Invalid;
        }

        private byte TranslateMessageModeToServer(MessageModes mode) {
            if (mode < 0 || mode >= MessageModes.LastMessage)
                return (int)MessageModes.Invalid;

            foreach (var p in m_MessageModesDict) {
                if (p.Key == mode)
                    return (byte)p.Value;
            }
            return (int)MessageModes.Invalid;
        }

        private void ParseTalk(InputMessage message) {
            uint statementID = message.GetU32();
            string speaker = message.GetString();
            ushort speakerLevel = message.GetU16();

            int rawMode = message.GetU8();
            MessageModes mode = TranslateMessageModeFromServer(rawMode);

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
                case MessageModes.ChannelManagement:
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

                case MessageModes.NpcFrom:
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
                case MessageModes.MonsterSay:
                case MessageModes.MonsterYell:
                    absolutePosition = message.GetPosition();
                    channelID = -1;
                    break;

                case MessageModes.Game:
                    break;

                case MessageModes.RVRAnswer:
                case MessageModes.RVRContinue:
                    channelID = Chat.ChatStorage.RVRChannelID;
                    break;

                case MessageModes.RVRChannel:
                    message.GetU32();
                    channelID = Chat.ChatStorage.RVRChannelID;
                    break;

                default:
                    throw new System.Exception(string.Format("ProtocolGame.ParseTalk: invalid message mode (raw = {0}, mode = {1})", rawMode, mode));

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

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameChannelPlayerList)) {
                int joinedUsers = message.GetU16();
                for (int i = 0; i < joinedUsers; i++)
                    channel.PlayerJoined(message.GetString());

                int invitedUsers = message.GetU16();
                for (int i = 0; i < invitedUsers; i++)
                    channel.PlayerInvited(message.GetString());
            }
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

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameChannelPlayerList)) {
                int joinedUsers = message.GetU16();
                for (int i = 0; i < joinedUsers; i++)
                    channel.PlayerJoined(message.GetString());

                int invitedUsers = message.GetU16();
                for (int i = 0; i < invitedUsers; i++)
                    channel.PlayerInvited(message.GetString());
            }
        }

        private void ParseCloseChannel(InputMessage message) {
            int channelID = message.GetU16();
            m_ChatStorage.CloseChannel(channelID);
        }

        private void ParseTextMessage(InputMessage message) {
            MessageModes mode = TranslateMessageModeFromServer(message.GetU8());

            try {
                switch (mode) {
                    case MessageModes.ChannelManagement:
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
                        Vector3Int absolutePosition = message.GetPosition();
                        int value = message.GetS32();
                        int color = message.GetU8();
                        if (value > 0)
                            m_WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        value = message.GetS32();
                        color = message.GetU8();
                        if (value > 0)
                            m_WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        text = message.GetString();
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    case MessageModes.Heal:
                    case MessageModes.Mana:
                    case MessageModes.Exp:
                    case MessageModes.HealOthers:
                    case MessageModes.ExpOthers:
                        absolutePosition = message.GetPosition();
                        value = message.GetS32();
                        color = message.GetU8();
                        m_WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        text = message.GetString();
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    default:
                        text = message.GetString();
                        m_ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;
                }
            } catch (System.Exception e) {
                throw new System.Exception("ProtocolGame.ParseTextMessage: Failed to add message of type " + mode + ": " + e.Message + "\n" + e.StackTrace);
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
