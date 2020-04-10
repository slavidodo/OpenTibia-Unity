using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        private Dictionary<MessageModeType, int> _messageModesDict;

        private void BuildMessageModesMap(int version) {
            if (_messageModesDict == null)
                _messageModesDict = new Dictionary<MessageModeType, int>();
            else
                _messageModesDict.Clear();

            if (version >= 1200) {
                _messageModesDict.Add(MessageModeType.BoostedCreature, 49);
            }

            if (version >= 1094) {
                _messageModesDict.Add(MessageModeType.Mana, 43);
            }

            if (version >= 1055) { // might be 1054
                _messageModesDict.Add(MessageModeType.None, 0);
                _messageModesDict.Add(MessageModeType.Say, 1);
                _messageModesDict.Add(MessageModeType.Whisper, 2);
                _messageModesDict.Add(MessageModeType.Yell, 3);
                _messageModesDict.Add(MessageModeType.PrivateFrom, 4);
                _messageModesDict.Add(MessageModeType.PrivateTo, 5);
                _messageModesDict.Add(MessageModeType.ChannelManagement, 6);
                _messageModesDict.Add(MessageModeType.Channel, 7);
                _messageModesDict.Add(MessageModeType.ChannelHighlight, 8);
                _messageModesDict.Add(MessageModeType.Spell, 9);
                _messageModesDict.Add(MessageModeType.NpcFromStartBlock, 10);
                _messageModesDict.Add(MessageModeType.NpcFrom, 11);
                _messageModesDict.Add(MessageModeType.NpcTo, 12);
                _messageModesDict.Add(MessageModeType.GamemasterBroadcast, 13);
                _messageModesDict.Add(MessageModeType.GamemasterChannel, 14);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 15);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateTo, 16);
                _messageModesDict.Add(MessageModeType.Login, 17);
                _messageModesDict.Add(MessageModeType.Admin, 18);
                _messageModesDict.Add(MessageModeType.Game, 19);
                _messageModesDict.Add(MessageModeType.GameHighlight, 20);
                _messageModesDict.Add(MessageModeType.Failure, 21);
                _messageModesDict.Add(MessageModeType.Look, 22);
                _messageModesDict.Add(MessageModeType.DamageDealed, 23);
                _messageModesDict.Add(MessageModeType.DamageReceived, 24);
                _messageModesDict.Add(MessageModeType.Heal, 25);
                _messageModesDict.Add(MessageModeType.Exp, 26);
                _messageModesDict.Add(MessageModeType.DamageOthers, 27);
                _messageModesDict.Add(MessageModeType.HealOthers, 28);
                _messageModesDict.Add(MessageModeType.ExpOthers, 29);
                _messageModesDict.Add(MessageModeType.Status, 30);
                _messageModesDict.Add(MessageModeType.Loot, 31);
                _messageModesDict.Add(MessageModeType.TradeNpc, 32);
                _messageModesDict.Add(MessageModeType.Guild, 33);
                _messageModesDict.Add(MessageModeType.PartyManagement, 34);
                _messageModesDict.Add(MessageModeType.Party, 35);
                _messageModesDict.Add(MessageModeType.BarkLow, 36);
                _messageModesDict.Add(MessageModeType.BarkLoud, 37);
                _messageModesDict.Add(MessageModeType.Report, 38);
                _messageModesDict.Add(MessageModeType.HotkeyUse, 39);
                _messageModesDict.Add(MessageModeType.TutorialHint, 40);
                _messageModesDict.Add(MessageModeType.Thankyou, 41);
                _messageModesDict.Add(MessageModeType.Market, 42);
            } else if (version >= 1036) {
                for (var i = MessageModeType.None; i <= MessageModeType.Thankyou; ++i) {
                    if (i >= MessageModeType.NpcTo)
                        _messageModesDict.Add(i, (int)i + 1);
                    else
                        _messageModesDict.Add(i, (int)i);
                }
            } else if (version >= 900) {
                for (var i = MessageModeType.None; i <= MessageModeType.Thankyou; ++i)
                    _messageModesDict.Add(i, (int)i);
            } else if (version >= 861) {
                _messageModesDict.Add(MessageModeType.None, 0);
                _messageModesDict.Add(MessageModeType.Say, 1);
                _messageModesDict.Add(MessageModeType.Whisper, 2);
                _messageModesDict.Add(MessageModeType.Yell, 3);
                _messageModesDict.Add(MessageModeType.NpcTo, 4);
                _messageModesDict.Add(MessageModeType.NpcFrom, 5);
                _messageModesDict.Add(MessageModeType.PrivateFrom, 6);
                _messageModesDict.Add(MessageModeType.PrivateTo, 6);
                _messageModesDict.Add(MessageModeType.Channel, 7);
                _messageModesDict.Add(MessageModeType.ChannelManagement, 8);
                _messageModesDict.Add(MessageModeType.GamemasterBroadcast, 9);
                _messageModesDict.Add(MessageModeType.GamemasterChannel, 10);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 11);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateTo, 11);
                _messageModesDict.Add(MessageModeType.ChannelHighlight, 12);
                _messageModesDict.Add(MessageModeType.MonsterSay, 13);
                _messageModesDict.Add(MessageModeType.MonsterYell, 14);
                _messageModesDict.Add(MessageModeType.Admin, 15);
                _messageModesDict.Add(MessageModeType.Game, 16);
                _messageModesDict.Add(MessageModeType.Login, 17);
                _messageModesDict.Add(MessageModeType.Status, 18);
                _messageModesDict.Add(MessageModeType.Look, 19);
                _messageModesDict.Add(MessageModeType.Failure, 20);
                _messageModesDict.Add(MessageModeType.Blue, 21);
                _messageModesDict.Add(MessageModeType.Red, 22);
            } else if (version >= 840) {
                _messageModesDict.Add(MessageModeType.None, 0);
                _messageModesDict.Add(MessageModeType.Say, 1);
                _messageModesDict.Add(MessageModeType.Whisper, 2);
                _messageModesDict.Add(MessageModeType.Yell, 3);
                _messageModesDict.Add(MessageModeType.NpcTo, 4);
                _messageModesDict.Add(MessageModeType.NpcFromStartBlock, 5);
                _messageModesDict.Add(MessageModeType.PrivateFrom, 6);
                _messageModesDict.Add(MessageModeType.PrivateTo, 6);
                _messageModesDict.Add(MessageModeType.Channel, 7);
                _messageModesDict.Add(MessageModeType.ChannelManagement, 8);
                _messageModesDict.Add(MessageModeType.RVRChannel, 9);
                _messageModesDict.Add(MessageModeType.RVRAnswer, 10);
                _messageModesDict.Add(MessageModeType.RVRContinue, 11);
                _messageModesDict.Add(MessageModeType.GamemasterBroadcast, 12);
                _messageModesDict.Add(MessageModeType.GamemasterChannel, 13);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 14);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateTo, 14);
                _messageModesDict.Add(MessageModeType.ChannelHighlight, 15);
                // 16, 17 ??
                _messageModesDict.Add(MessageModeType.Red, 18);
                _messageModesDict.Add(MessageModeType.MonsterSay, 19);
                _messageModesDict.Add(MessageModeType.MonsterYell, 20);
                _messageModesDict.Add(MessageModeType.Admin, 21);
                _messageModesDict.Add(MessageModeType.Game, 22);
                _messageModesDict.Add(MessageModeType.Login, 23);
                _messageModesDict.Add(MessageModeType.Status, 24);
                _messageModesDict.Add(MessageModeType.Look, 25);
                _messageModesDict.Add(MessageModeType.Failure, 26);
                _messageModesDict.Add(MessageModeType.Blue, 27);
            } else if (version >= 760) {
                _messageModesDict.Add(MessageModeType.None, 0);
                _messageModesDict.Add(MessageModeType.Say, 1);
                _messageModesDict.Add(MessageModeType.Whisper, 2);
                _messageModesDict.Add(MessageModeType.Yell, 3);
                _messageModesDict.Add(MessageModeType.PrivateFrom, 4);
                _messageModesDict.Add(MessageModeType.PrivateTo, 4);
                _messageModesDict.Add(MessageModeType.Channel, 5);
                _messageModesDict.Add(MessageModeType.RVRChannel, 6);
                _messageModesDict.Add(MessageModeType.RVRAnswer, 7);
                _messageModesDict.Add(MessageModeType.RVRContinue, 8);
                _messageModesDict.Add(MessageModeType.GamemasterBroadcast, 9);
                _messageModesDict.Add(MessageModeType.GamemasterChannel, 10);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateFrom, 11);
                _messageModesDict.Add(MessageModeType.GamemasterPrivateTo, 11);
                _messageModesDict.Add(MessageModeType.ChannelHighlight, 12);
                // 13, 14, 15 ??
                _messageModesDict.Add(MessageModeType.MonsterSay, 16);
                _messageModesDict.Add(MessageModeType.MonsterYell, 17);
                _messageModesDict.Add(MessageModeType.Admin, 18);
                _messageModesDict.Add(MessageModeType.Game, 19);
                _messageModesDict.Add(MessageModeType.Login, 20);
                _messageModesDict.Add(MessageModeType.Status, 21);
                _messageModesDict.Add(MessageModeType.Look, 22);
                _messageModesDict.Add(MessageModeType.Failure, 23);
                _messageModesDict.Add(MessageModeType.Blue, 24);
                _messageModesDict.Add(MessageModeType.Red, 25);
            }
        }
        
        private MessageModeType TranslateMessageModeFromServer(int mode) {
            foreach (var p in _messageModesDict) {
                if (p.Value == mode)
                    return p.Key;
            }
            return MessageModeType.Invalid;
        }

        private byte TranslateMessageModeToServer(MessageModeType mode) {
            if (mode < 0 || mode >= MessageModeType.LastMessage)
                return (int)MessageModeType.Invalid;

            foreach (var p in _messageModesDict) {
                if (p.Key == mode)
                    return (byte)p.Value;
            }
            return (int)MessageModeType.Invalid;
        }

        private void ParseTalk(Internal.CommunicationStream message) {
            uint statementId = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageStatements))
                statementId = message.ReadUnsignedInt();

            string speaker = message.ReadString();
            ushort speakerLevel = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel))
                speakerLevel = message.ReadUnsignedShort();

            int rawMode = message.ReadUnsignedByte();
            MessageModeType mode = TranslateMessageModeFromServer(rawMode);
            
            Vector3Int? absolutePosition = null;
            Utils.UnionStrInt channelId = null;

            switch (mode) {
                case MessageModeType.Say:
                case MessageModeType.Whisper:
                case MessageModeType.Yell:
                    absolutePosition = message.ReadPosition();
                    channelId = Chat.ChatStorage.LocalChannelId;
                    break;

                case MessageModeType.PrivateFrom:
                    channelId = speaker;
                    break;

                case MessageModeType.Channel:
                case MessageModeType.ChannelManagement:
                case MessageModeType.ChannelHighlight:
                    channelId = message.ReadUnsignedShort();
                    break;

                case MessageModeType.Spell:
                    absolutePosition = message.ReadPosition();
                    channelId = Chat.ChatStorage.LocalChannelId;
                    break;

                case MessageModeType.NpcFromStartBlock:
                    absolutePosition = message.ReadPosition();
                    break;

                case MessageModeType.NpcFrom:
                    break;

                case MessageModeType.GamemasterBroadcast:
                    break;

                case MessageModeType.GamemasterChannel:
                    channelId = message.ReadUnsignedShort();
                    break;

                case MessageModeType.GamemasterPrivateFrom:
                    channelId = speaker;
                    break;

                case MessageModeType.BarkLow:
                case MessageModeType.BarkLoud:
                case MessageModeType.MonsterSay:
                case MessageModeType.MonsterYell:
                    absolutePosition = message.ReadPosition();
                    channelId = -1;
                    break;

                case MessageModeType.Game:
                    break;

                case MessageModeType.RVRAnswer:
                case MessageModeType.RVRContinue:
                    channelId = Chat.ChatStorage.RVRChannelId;
                    break;

                case MessageModeType.RVRChannel:
                    message.ReadUnsignedInt();
                    channelId = Chat.ChatStorage.RVRChannelId;
                    break;

                default:
                    throw new System.Exception(string.Format("ProtocolGame.ParseTalk: invalid message mode (raw = {0}, mode = {1})", rawMode, mode));

            }

            string text = message.ReadString();
            if(mode != MessageModeType.NpcFromStartBlock && mode != MessageModeType.NpcFrom) {
                try {
                    WorldMapStorage.AddOnscreenMessage(absolutePosition, (int)statementId, speaker, speakerLevel, mode, text);
                    if (mode != MessageModeType.BarkLoud)
                        ChatStorage.AddChannelMessage(channelId, (int)statementId, speaker, speakerLevel, mode, text);
                } catch (System.Exception e) {
                    throw new System.Exception("ProtocolGame.ParseTalk: Failed to add message: " + e.Message + "\n" + e.StackTrace);
                }
            } else if (mode == MessageModeType.NpcFromStartBlock) {
                MessageStorage.StartMessageBlock(speaker, absolutePosition, text);
            } else if (mode == MessageModeType.NpcFrom) {
                MessageStorage.AddTextToBlock(speaker, text);
            }
        }

        private void ParseChannels(Internal.CommunicationStream message) {
            int count = message.ReadUnsignedByte();
            List<Chat.Channel> channels = new List<Chat.Channel>(count);
            for (int i = 0; i < count; i++) {
                int id = message.ReadUnsignedShort();
                string name = message.ReadString();
                channels.Add(new Chat.Channel(id, name, MessageModeType.None));
            }

            OpenTibiaUnity.GameManager.onReceiveChannels.Invoke(channels);
        }

        private void ParseOpenChannel(Internal.CommunicationStream message) {
            int channelId = message.ReadUnsignedShort();
            string channelName = message.ReadString();
            Chat.Channel channel = ChatStorage.AddChannel(channelId, channelName, MessageModeType.Channel);
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

        private void ParsePrivateChannel(Internal.CommunicationStream message) {
            string channelName = message.ReadString();
            ChatStorage.AddChannel(channelName, channelName, MessageModeType.PrivateTo);
        }

        private void ParseOpenOwnChannel(Internal.CommunicationStream message) {
            int channelId = message.ReadUnsignedShort();
            string channelName = message.ReadString();
            var channel = ChatStorage.AddChannel(channelId, channelName, MessageModeType.Channel);
            channel.CanModerate = true;

            if (channel.IsPrivate)
                ChatStorage.OwnPrivateChannelId = channelId;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameChannelPlayerList)) {
                int joinedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < joinedUsers; i++)
                    channel.PlayerJoined(message.ReadString());

                int invitedUsers = message.ReadUnsignedShort();
                for (int i = 0; i < invitedUsers; i++)
                    channel.PlayerInvited(message.ReadString());
            }
        }

        private void ParseCloseChannel(Internal.CommunicationStream message) {
            int channelId = message.ReadUnsignedShort();
            ChatStorage.CloseChannel(channelId);
        }

        private void ParseTextMessage(Internal.CommunicationStream message) {
            var rawMode = message.ReadUnsignedByte();
            var mode = TranslateMessageModeFromServer(rawMode);

            try {
                int channelId = 0;
                string text = null;

                switch (mode) {
                    case MessageModeType.ChannelManagement:
                        channelId = message.ReadUnsignedShort();
                        text = message.ReadString();
                        // TODO name filter
                        //var regex = new System.Text.RegularExpressions.Regex(@"^(.+?) invites you to |^You have been excluded from the channel ([^']+)'s Channel\.$");
                        //var match = regex.Match(text);
                        //string speaker = match != null && match.Success ? match.Value : null;

                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelId, -1, null, 0, mode, text);
                        break;

                    case MessageModeType.Guild:
                    case MessageModeType.PartyManagement:
                    case MessageModeType.Party:
                        channelId = message.ReadUnsignedShort();
                        text = message.ReadString();
                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelId, -1, null, 0, mode, text);
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
                    case MessageModeType.BoostedCreature:
                        channelId = -1;
                        text = message.ReadString();
                        WorldMapStorage.AddOnscreenMessage(null, -1, null, 0, mode, text);
                        ChatStorage.AddChannelMessage(channelId, -1, null, 0, mode, text);
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
                throw new System.Exception("ProtocolGame.ParseTextMessage: Failed to add message of type " + rawMode + ": " + e.Message + "\n" + e.StackTrace);
            }
        }

        private void ParseChannelEvent(Internal.CommunicationStream message) {
            int channelId = message.ReadUnsignedShort();
            var channel = ChatStorage.GetChannel(channelId);
            string playerName = message.ReadString();
            var eventType = message.ReadEnum<ChannelEvent>();

            if (channel == null) {
#if DEBUG || NDEBUG
                Debug.LogWarning("ProtocolGame.ParseChannelEvent: invalid channel id " + channelId);
#endif
                return;
            }

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
