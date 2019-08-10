using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private Dictionary<MessageModeType, int> m_MessageModesDict;

        private void BuildMessageModesMap(int version) {
            if (m_MessageModesDict == null)
                m_MessageModesDict = new Dictionary<MessageModeType, int>();
            else
                m_MessageModesDict.Clear();

            if (version >= 1094) {
                m_MessageModesDict.Add(MessageModeType.Mana, 43);
            }

            if (version >= 1055) { // might be 1054
                m_MessageModesDict.Add(MessageModeType.None, 0);
                m_MessageModesDict.Add(MessageModeType.Say, 1);
                m_MessageModesDict.Add(MessageModeType.Whisper, 2);
                m_MessageModesDict.Add(MessageModeType.Yell, 3);
                m_MessageModesDict.Add(MessageModeType.PrivateFrom, 4);
                m_MessageModesDict.Add(MessageModeType.PrivateTo, 5);
                m_MessageModesDict.Add(MessageModeType.ChannelManagement, 6);
                m_MessageModesDict.Add(MessageModeType.Channel, 7);
                m_MessageModesDict.Add(MessageModeType.ChannelHighlight, 8);
                m_MessageModesDict.Add(MessageModeType.Spell, 9);
                m_MessageModesDict.Add(MessageModeType.NpcFromStartBlock, 10);
                m_MessageModesDict.Add(MessageModeType.NpcFrom, 11);
                m_MessageModesDict.Add(MessageModeType.NpcTo, 12);
                m_MessageModesDict.Add(MessageModeType.GamemasterBroadcast, 13);
                m_MessageModesDict.Add(MessageModeType.GamemasterChannel, 14);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 15);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateTo, 16);
                m_MessageModesDict.Add(MessageModeType.Login, 17);
                m_MessageModesDict.Add(MessageModeType.Admin, 18);
                m_MessageModesDict.Add(MessageModeType.Game, 19);
                m_MessageModesDict.Add(MessageModeType.GameHighlight, 20);
                m_MessageModesDict.Add(MessageModeType.Failure, 21);
                m_MessageModesDict.Add(MessageModeType.Look, 22);
                m_MessageModesDict.Add(MessageModeType.DamageDealed, 23);
                m_MessageModesDict.Add(MessageModeType.DamageReceived, 24);
                m_MessageModesDict.Add(MessageModeType.Heal, 25);
                m_MessageModesDict.Add(MessageModeType.Exp, 26);
                m_MessageModesDict.Add(MessageModeType.DamageOthers, 27);
                m_MessageModesDict.Add(MessageModeType.HealOthers, 28);
                m_MessageModesDict.Add(MessageModeType.ExpOthers, 29);
                m_MessageModesDict.Add(MessageModeType.Status, 30);
                m_MessageModesDict.Add(MessageModeType.Loot, 31);
                m_MessageModesDict.Add(MessageModeType.TradeNpc, 32);
                m_MessageModesDict.Add(MessageModeType.Guild, 33);
                m_MessageModesDict.Add(MessageModeType.PartyManagement, 34);
                m_MessageModesDict.Add(MessageModeType.Party, 35);
                m_MessageModesDict.Add(MessageModeType.BarkLow, 36);
                m_MessageModesDict.Add(MessageModeType.BarkLoud, 37);
                m_MessageModesDict.Add(MessageModeType.Report, 38);
                m_MessageModesDict.Add(MessageModeType.HotkeyUse, 39);
                m_MessageModesDict.Add(MessageModeType.TutorialHint, 40);
                m_MessageModesDict.Add(MessageModeType.Thankyou, 41);
                m_MessageModesDict.Add(MessageModeType.Market, 42);
            } else if (version >= 1036) {
                for (var i = MessageModeType.None; i <= MessageModeType.BeyondLast; ++i) {
                    if (i >= MessageModeType.NpcTo)
                        m_MessageModesDict.Add(i, (int)i + 1);
                    else
                        m_MessageModesDict.Add(i, (int)i);
                }
            } else if (version >= 900) {
                for (var i = MessageModeType.None; i <= MessageModeType.BeyondLast; ++i)
                    m_MessageModesDict.Add(i, (int)i);
            } else if (version >= 861) {
                m_MessageModesDict.Add(MessageModeType.None, 0);
                m_MessageModesDict.Add(MessageModeType.Say, 1);
                m_MessageModesDict.Add(MessageModeType.Whisper, 2);
                m_MessageModesDict.Add(MessageModeType.Yell, 3);
                m_MessageModesDict.Add(MessageModeType.NpcTo, 4);
                m_MessageModesDict.Add(MessageModeType.NpcFrom, 5);
                m_MessageModesDict.Add(MessageModeType.PrivateFrom, 6);
                m_MessageModesDict.Add(MessageModeType.PrivateTo, 6);
                m_MessageModesDict.Add(MessageModeType.Channel, 7);
                m_MessageModesDict.Add(MessageModeType.ChannelManagement, 8);
                m_MessageModesDict.Add(MessageModeType.GamemasterBroadcast, 9);
                m_MessageModesDict.Add(MessageModeType.GamemasterChannel, 10);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 11);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateTo, 11);
                m_MessageModesDict.Add(MessageModeType.ChannelHighlight, 12);
                m_MessageModesDict.Add(MessageModeType.MonsterSay, 13);
                m_MessageModesDict.Add(MessageModeType.MonsterYell, 14);
                m_MessageModesDict.Add(MessageModeType.Admin, 15);
                m_MessageModesDict.Add(MessageModeType.Game, 16);
                m_MessageModesDict.Add(MessageModeType.Login, 17);
                m_MessageModesDict.Add(MessageModeType.Status, 18);
                m_MessageModesDict.Add(MessageModeType.Look, 19);
                m_MessageModesDict.Add(MessageModeType.Failure, 20);
                m_MessageModesDict.Add(MessageModeType.Blue, 21);
                m_MessageModesDict.Add(MessageModeType.Red, 22);
            } else if (version >= 840) {
                m_MessageModesDict.Add(MessageModeType.None, 0);
                m_MessageModesDict.Add(MessageModeType.Say, 1);
                m_MessageModesDict.Add(MessageModeType.Whisper, 2);
                m_MessageModesDict.Add(MessageModeType.Yell, 3);
                m_MessageModesDict.Add(MessageModeType.NpcTo, 4);
                m_MessageModesDict.Add(MessageModeType.NpcFromStartBlock, 5);
                m_MessageModesDict.Add(MessageModeType.PrivateFrom, 6);
                m_MessageModesDict.Add(MessageModeType.PrivateTo, 6);
                m_MessageModesDict.Add(MessageModeType.Channel, 7);
                m_MessageModesDict.Add(MessageModeType.ChannelManagement, 8);
                m_MessageModesDict.Add(MessageModeType.RVRChannel, 9);
                m_MessageModesDict.Add(MessageModeType.RVRAnswer, 10);
                m_MessageModesDict.Add(MessageModeType.RVRContinue, 11);
                m_MessageModesDict.Add(MessageModeType.GamemasterBroadcast, 12);
                m_MessageModesDict.Add(MessageModeType.GamemasterChannel, 13);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 14);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateTo, 14);
                m_MessageModesDict.Add(MessageModeType.ChannelHighlight, 15);
                // 16, 17 ??
                m_MessageModesDict.Add(MessageModeType.Red, 18);
                m_MessageModesDict.Add(MessageModeType.MonsterSay, 19);
                m_MessageModesDict.Add(MessageModeType.MonsterYell, 20);
                m_MessageModesDict.Add(MessageModeType.Admin, 21);
                m_MessageModesDict.Add(MessageModeType.Game, 22);
                m_MessageModesDict.Add(MessageModeType.Login, 23);
                m_MessageModesDict.Add(MessageModeType.Status, 24);
                m_MessageModesDict.Add(MessageModeType.Look, 25);
                m_MessageModesDict.Add(MessageModeType.Failure, 26);
                m_MessageModesDict.Add(MessageModeType.Blue, 27);
            } else if (version >= 760) {
                m_MessageModesDict.Add(MessageModeType.None, 0);
                m_MessageModesDict.Add(MessageModeType.Say, 1);
                m_MessageModesDict.Add(MessageModeType.Whisper, 2);
                m_MessageModesDict.Add(MessageModeType.Yell, 3);
                m_MessageModesDict.Add(MessageModeType.PrivateFrom, 4);
                m_MessageModesDict.Add(MessageModeType.PrivateTo, 4);
                m_MessageModesDict.Add(MessageModeType.Channel, 5);
                m_MessageModesDict.Add(MessageModeType.RVRChannel, 6);
                m_MessageModesDict.Add(MessageModeType.RVRAnswer, 7);
                m_MessageModesDict.Add(MessageModeType.RVRContinue, 8);
                m_MessageModesDict.Add(MessageModeType.GamemasterBroadcast, 9);
                m_MessageModesDict.Add(MessageModeType.GamemasterChannel, 10);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 11);
                m_MessageModesDict.Add(MessageModeType.GamemasterPrivateTo, 11);
                m_MessageModesDict.Add(MessageModeType.ChannelHighlight, 12);
                // 13, 14, 15 ??
                m_MessageModesDict.Add(MessageModeType.MonsterSay, 16);
                m_MessageModesDict.Add(MessageModeType.MonsterYell, 17);
                m_MessageModesDict.Add(MessageModeType.Admin, 18);
                m_MessageModesDict.Add(MessageModeType.Game, 19);
                m_MessageModesDict.Add(MessageModeType.Login, 20);
                m_MessageModesDict.Add(MessageModeType.Status, 21);
                m_MessageModesDict.Add(MessageModeType.Look, 22);
                m_MessageModesDict.Add(MessageModeType.Failure, 23);
                m_MessageModesDict.Add(MessageModeType.Blue, 24);
                m_MessageModesDict.Add(MessageModeType.Red, 25);
            }
        }
        
        private MessageModeType TranslateMessageModeFromServer(int mode) {
            foreach (var p in m_MessageModesDict) {
                if (p.Value == mode)
                    return p.Key;
            }
            return MessageModeType.Invalid;
        }

        private byte TranslateMessageModeToServer(MessageModeType mode) {
            if (mode < 0 || mode >= MessageModeType.LastMessage)
                return (int)MessageModeType.Invalid;

            foreach (var p in m_MessageModesDict) {
                if (p.Key == mode)
                    return (byte)p.Value;
            }
            return (int)MessageModeType.Invalid;
        }

        private void ParseTalk(Internal.ByteArray message) {
            uint statementID = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageStatements))
                statementID = message.ReadUnsignedInt();

            string speaker = message.ReadString();
            ushort speakerLevel = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel))
                speakerLevel = message.ReadUnsignedShort();

            int rawMode = message.ReadUnsignedByte();
            MessageModeType mode = TranslateMessageModeFromServer(rawMode);
            
            Vector3Int? absolutePosition = null;
            Utility.UnionStrInt channelID = null;

            switch (mode) {
                case MessageModeType.Say:
                case MessageModeType.Whisper:
                case MessageModeType.Yell:
                    absolutePosition = message.ReadPosition();
                    channelID = Chat.ChatStorage.LocalChannelID;
                    break;

                case MessageModeType.PrivateFrom:
                    channelID = speaker;
                    break;

                case MessageModeType.Channel:
                case MessageModeType.ChannelManagement:
                case MessageModeType.ChannelHighlight:
                    channelID = message.ReadUnsignedShort();
                    break;

                case MessageModeType.Spell:
                    absolutePosition = message.ReadPosition();
                    channelID = Chat.ChatStorage.LocalChannelID;
                    break;

                case MessageModeType.NpcFromStartBlock:
                    absolutePosition = message.ReadPosition();
                    break;

                case MessageModeType.NpcFrom:
                    break;

                case MessageModeType.GamemasterBroadcast:
                    break;

                case MessageModeType.GamemasterChannel:
                    channelID = message.ReadUnsignedShort();
                    break;

                case MessageModeType.GamemasterPrivateFrom:
                    channelID = speaker;
                    break;

                case MessageModeType.BarkLow:
                case MessageModeType.BarkLoud:
                case MessageModeType.MonsterSay:
                case MessageModeType.MonsterYell:
                    absolutePosition = message.ReadPosition();
                    channelID = -1;
                    break;

                case MessageModeType.Game:
                    break;

                case MessageModeType.RVRAnswer:
                case MessageModeType.RVRContinue:
                    channelID = Chat.ChatStorage.RVRChannelID;
                    break;

                case MessageModeType.RVRChannel:
                    message.ReadUnsignedInt();
                    channelID = Chat.ChatStorage.RVRChannelID;
                    break;

                default:
                    throw new System.Exception(string.Format("ProtocolGame.ParseTalk: invalid message mode (raw = {0}, mode = {1})", rawMode, mode));

            }

            string text = message.ReadString();
            if(mode != MessageModeType.NpcFromStartBlock && mode != MessageModeType.NpcFrom) {
                try {
                    WorldMapStorage.AddOnscreenMessage(absolutePosition, (int)statementID, speaker, speakerLevel, mode, text);
                    ChatStorage.AddChannelMessage(channelID, (int)statementID, speaker, speakerLevel, mode, text);
                } catch (System.Exception e) {
                    throw new System.Exception("ProtocolGame.ParseTalk: Failed to add message: " + e.Message + "\n" + e.StackTrace);
                }
            } else if (mode == MessageModeType.NpcFromStartBlock) {
                MessageStorage.StartMessageBlock(speaker, absolutePosition, text);
            } else if (mode == MessageModeType.NpcFrom) {
                MessageStorage.AddTextToBlock(speaker, text);
            }
        }

        private void ParseChannels(Internal.ByteArray message) {
            int count = message.ReadUnsignedByte();
            List<Chat.Channel> channels = new List<Chat.Channel>();
            for (int i = 0; i < count; i++) {
                int id = message.ReadUnsignedShort();
                string name = message.ReadString();
                channels.Add(new Chat.Channel(id, name, MessageModeType.None));
            }
        }

        private void ParseOpenChannel(Internal.ByteArray message) {
            int channelID = message.ReadUnsignedShort();
            string channelName = message.ReadString();
            Chat.Channel channel = ChatStorage.AddChannel(channelID, channelName, MessageModeType.Channel);
            channel.CanModerate = true;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameChannelPlayerList)) {
                int joinedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < joinedUsers; i++)
                    channel.PlayerJoined(message.ReadString());

                int invitedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < invitedUsers; i++)
                    channel.PlayerInvited(message.ReadString());
            }
        }

        private void ParsePrivateChannel(Internal.ByteArray message) {
            string channelName = message.ReadString();
            ChatStorage.AddChannel(channelName, channelName, MessageModeType.PrivateTo);
        }

        private void ParseOpenOwnChannel(Internal.ByteArray message) {
            int channelID = message.ReadUnsignedShort();
            string channelName = message.ReadString();
            var channel = ChatStorage.AddChannel(channelID, channelName, MessageModeType.Channel);
            channel.CanModerate = true;

            if (channel.IsPrivate)
                ChatStorage.OwnPrivateChannelID = channelID;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameChannelPlayerList)) {
                int joinedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < joinedUsers; i++)
                    channel.PlayerJoined(message.ReadString());

                int invitedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < invitedUsers; i++)
                    channel.PlayerInvited(message.ReadString());
            }
        }

        private void ParseCloseChannel(Internal.ByteArray message) {
            int channelID = message.ReadUnsignedShort();
            ChatStorage.CloseChannel(channelID);
        }

        private void ParseTextMessage(Internal.ByteArray message) {
            var mode = TranslateMessageModeFromServer(message.ReadUnsignedByte());

            try {
                switch (mode) {
                    case MessageModeType.ChannelManagement:
                        int channelID = message.ReadUnsignedShort();
                        string text = message.ReadString();
                        // TODO name filter
                        //var regex = new System.Text.RegularExpressions.Regex(@"^(.+?) invites you to |^You have been excluded from the channel ([^']+)'s Channel\.$");
                        //var match = regex.Match(text);
                        //string speaker = match != null && match.Success ? match.Value : null;

                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.Guild:
                    case MessageModeType.PartyManagement:
                    case MessageModeType.Party:
                        channelID = message.ReadUnsignedShort();
                        text = message.ReadString();
                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.Login:
                    case MessageModeType.Admin:
                    case MessageModeType.Game:
                    case MessageModeType.GameHighlight:
                    case MessageModeType.Failure:
                    case MessageModeType.Look:
                    case MessageModeType.Status:
                    case MessageModeType.Loot:
                    case MessageModeType.TradeNpc:
                    case MessageModeType.HotkeyUse:
                        channelID = -1;
                        text = message.ReadString();
                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelID, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.Market:
                        text = message.ReadString();
                        // TODO: market
                        break;

                    case MessageModeType.Report:
                        // TODO
                        //ReportWidget.s_ReportTimestampReset();
                        text = message.ReadString();
                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.DamageDealed:
                    case MessageModeType.DamageReceived:
                    case MessageModeType.DamageOthers:
                        Vector3Int absolutePosition = message.ReadPosition();
                        int value = message.ReadInt();
                        int color = message.ReadUnsignedByte();
                        if (value > 0)
                            WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        value = message.ReadInt();
                        color = message.ReadUnsignedByte();
                        if (value > 0)
                            WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        text = message.ReadString();
                        ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.Heal:
                    case MessageModeType.Mana:
                    case MessageModeType.Exp:
                    case MessageModeType.HealOthers:
                    case MessageModeType.ExpOthers:
                        absolutePosition = message.ReadPosition();
                        value = message.ReadInt();
                        color = message.ReadUnsignedByte();
                        WorldMapStorage.AddOnscreenMessage(absolutePosition, -1, null, 0, mode, value, color);

                        text = message.ReadString();
                        ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;

                    default:
                        text = message.ReadString();
                        ChatStorage.AddChannelMessage(-1, -1, null, 0, mode, text);
                        break;
                }
            } catch (System.Exception e) {
                throw new System.Exception("ProtocolGame.ParseTextMessage: Failed to add message of type " + mode + ": " + e.Message + "\n" + e.StackTrace);
            }
        }

        private void ParseChannelEvent(Internal.ByteArray message) {
            int channelID = message.ReadUnsignedShort();
            var channel = ChatStorage.GetChannel(channelID);
            string playerName = message.ReadString();
            var eventType = message.ReadEnum<ChannelEvent>();

            switch (eventType) {
                case ChannelEvent.PlayerJoined:
                    channel.PlayerJoined(playerName);
                    break;

                case ChannelEvent.PlayerLeft:
                    channel.PlayerLeft(playerName);
                    break;
                    
                case ChannelEvent.PlayerInvited:
                    channel.PlayerInvited(playerName);
                    break;

                case ChannelEvent.PlayerExcluded:
                    channel.PlayerExcluded(playerName);
                    break;

                case ChannelEvent.PlayerPending:
                    channel.PlayerPending(playerName);
                    break;
            }
        }
    }
}
