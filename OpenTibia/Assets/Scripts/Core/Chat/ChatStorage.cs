using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    public class ChatStorage
    {
        public class ChannelAddEvent : UnityEvent<Channel> { }
        public class ChannelsClearEvent : UnityEvent { }

        public const int MainAdvertisingChannelId = 5;
        public const int RookAdvertisingChannelId = 6;
        public const int HelpChannelId = 7;

        public const int FirstPrivateChannelId = 10;
        public const int LastPrivateChannelId = 9999;
        public const int FirstGuildChannelId = 10000;
        public const int LastGuildChannelId = 19999;
        public const int FirstPartyChannelId = 20000;
        public const int LastPartyChannelId = 65533;

        public const int NpcChannelId = 65534;
        public const int PrivateChannelId = 65535;
        public const int LootChannelId = 131067;
        //public const int SessionDumpChannelId = 131068;
        public const int DebugChannelId = 131069;
        public const int ServerChannelId = 131070;
        public const int LocalChannelId = 131071;
        public const int RVRChannelId = 131072;

        public const int ChannelActivationTimeout = 500;

        private List<Channel> _channels;
        private int _ownPrivateChannelId = -1;
        private int _channelActivationTimeout = 0;

        public ChannelAddEvent onAddChannel = new ChannelAddEvent();
        public ChannelsClearEvent onClearChannels = new ChannelsClearEvent();

        public int OwnPrivateChannelId { get => _ownPrivateChannelId; set => _ownPrivateChannelId = value; }

        public bool HasOwnPrivateChannel { get => s_IsPrivateChannel(OwnPrivateChannelId); }

        public ChatStorage(Options.OptionStorage optionStorage) {
            _channels = new List<Channel>();
        }

        public int GetChannelIndex(Utils.UnionStrInt channelId) {
            for (int i = 0; i < _channels.Count; i++) {
                if (_channels[i].Id == channelId) {
                    return i;
                }
            }

            return -1;
        }

        public void LeaveChannel(Utils.UnionStrInt channelId) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            if (s_IsPrivateChannel(channelId)) {
                var channel = GetChannel(channelId);
                if (channel != null && channel.SendAllowed)
                    protocolGame.SendLeaveChannel(channelId);
            } else if (channelId == NpcChannelId) {
                protocolGame.SendCloseNPCChannel();
            } else {
                protocolGame.SendLeaveChannel(channelId);
            }
            
            if (channelId == _ownPrivateChannelId)
                _ownPrivateChannelId = -1;

            RemoveChannel(channelId);
        }

        public void CloseChannel(Utils.UnionStrInt channelId) {
            AddChannelMessage(channelId, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_CHANNEL_CLOSED);

            Channel channel = GetChannel(channelId);
            if (channel != null) {
                channel.SendAllowed = false;
                channel.Closable = true;
                //channel.ClearNicklist(); TODO
            }

            if (_ownPrivateChannelId == channelId)
                _ownPrivateChannelId = -1;
        }

        public Channel GetChannelAt(int channelIndex) {
            return channelIndex >= 0 && channelIndex < _channels.Count ? _channels[channelIndex] : null; 
        }

        public Channel GetChannel(Utils.UnionStrInt channelId) {
            foreach (var channel in _channels) {
                if (channel.Id == channelId)
                    return channel;
            }
            
            return null;
        }

        public Channel AddChannel(Utils.UnionStrInt channelId, string name, MessageModeType mode) {
            Channel channel = GetChannel(channelId);

            if (channel != null) {
                channel.Name = name;
                channel.SendAllowed = true;
            } else {
                channel = new Channel(channelId, name, mode);
                _channels.Add(channel);
                onAddChannel.Invoke(channel);

                switch ((int)channelId) {
                    case HelpChannelId:
                        if (OpenTibiaUnity.GameManager.ClientVersion >= 854)
                            AddChannelMessage(channelId, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_HELP);
                        else
                            AddChannelMessage(channelId, -1, null, 0, MessageModeType.GamemasterChannel, TextResources.CHANNEL_MSG_HELP_LEGACY);
                        break;
                    case MainAdvertisingChannelId:
                    case RookAdvertisingChannelId:
                        AddChannelMessage(channelId, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_ADVERTISING);
                        break;
                    default:
                        break;
                }
            }

            var player = OpenTibiaUnity.Player;
            if (!!player && player.Name != null)
                channel.PlayerJoined(player.Name);

            return channel;
        }

        public void JoinChannel(Utils.UnionStrInt channelId) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            if (channelId.IsInt) {
                if (channelId >= 0 && channelId < NpcChannelId) {
                    protocolGame.SendJoinChannel(channelId);
                } else if (channelId == NpcChannelId) {
                    AddChannel(NpcChannelId, "NPCs", MessageModeType.NpcTo);
                } else if (channelId == PrivateChannelId) {
                    protocolGame.SendOpenChannel();
                } else if (channelId == LootChannelId) {
                    // todo loot channel
                }
            } else {
                var player = OpenTibiaUnity.Player;
                if (channelId == player.Name) {
                    // TODO; add to TR
                    OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, "You can't chat with yourself.");
                } else {
                    protocolGame.SendPrivateChannel(channelId);
                }
            }
        }

        public Channel RemoveChannel(Utils.UnionStrInt channelId) {
            var channel = GetChannel(channelId);
            if (channel != null)
                _channels.Remove(channel);

            if (channel != null && channel.Id == _ownPrivateChannelId)
                _ownPrivateChannelId = -1;

            return channel;
        }

        public void LoadChannels() {

        }

        public void Reset() {
            _channels.Clear();
            onClearChannels.Invoke();

            var tmpChannel = GetChannel(ChatStorage.LocalChannelId);
            if (tmpChannel == null) {
                if (OpenTibiaUnity.GameManager.ClientVersion >= 870)
                    tmpChannel = AddChannel(ChatStorage.LocalChannelId, TextResources.CHANNEL_NAME_DEFAULT, MessageModeType.Say);
                else
                    tmpChannel = AddChannel(ChatStorage.LocalChannelId, TextResources.CHANNEL_NAME_DEFAULT_LEGACY, MessageModeType.Say);
                tmpChannel.Closable = false;
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog)) {
                tmpChannel = GetChannel(ServerChannelId);
                if (tmpChannel == null) {
                    tmpChannel = AddChannel(ServerChannelId, TextResources.CHANNEL_NAME_SERVERLOG, MessageModeType.Say);
                    tmpChannel.Closable = false;
                }
            }
            
            _ownPrivateChannelId = -1;
            ResetChannelActivationTimeout();
        }

        public void ResetChannelActivationTimeout() {
            _channelActivationTimeout = OpenTibiaUnity.TicksMillis + ChannelActivationTimeout;
        }

        public ChannelMessage AddChannelMessage(Utils.UnionStrInt channelId, int statementId, string speaker, int speakerLevel, MessageModeType mode, string text) {
            var messageFilterSet = OpenTibiaUnity.OptionStorage.GetMessageFilterSet(MessageFilterSet.DefaultSet);
            var messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowChannelMessage)
                return null;

            var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
            if (!messageMode.IgnoreNameFilter && (nameFilterSet == null || !nameFilterSet.AcceptMessage(mode, speaker, text)))
                return null;

            bool isReportName = speaker != null && channelId.IsInt && ((int)channelId == ChatStorage.HelpChannelId || speakerLevel > 0);
            bool isReportStatement = statementId > 0;
            Channel channel = null;
            ChannelMessage channelMessage = new ChannelMessage(statementId, speaker, speakerLevel, mode, text);
            channelMessage.FormatMessage(messageFilterSet.ShowTimeStamps, messageFilterSet.ShowLevels, messageMode.TextARGB, messageMode.HighlightARGB);

            switch (mode) {
                case MessageModeType.Say:
                case MessageModeType.Yell:
                case MessageModeType.Whisper:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.PrivateFrom:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelId);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.PrivateTo:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelId);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.ChannelManagement:
                    channel = GetChannel(channelId);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.Channel:
                case MessageModeType.ChannelHighlight:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelId);
                    break;
                case MessageModeType.Spell:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.NpcFromStartBlock:
                case MessageModeType.NpcFrom:
                case MessageModeType.NpcTo:
                    channel = AddChannel(ChatStorage.NpcChannelId, "NPCs", MessageModeType.NpcTo);
                    break;
                case MessageModeType.GamemasterBroadcast:
                    channel = GetChannel(ChatStorage.ServerChannelId);
                    break;
                case MessageModeType.GamemasterChannel:
                    channel = GetChannel(channelId);
                    break;
                case MessageModeType.GamemasterPrivateFrom:
                case MessageModeType.GamemasterPrivateTo:
                    channel = GetChannel(ChatStorage.ServerChannelId);
                    break;
                case MessageModeType.Login:
                case MessageModeType.Admin:
                case MessageModeType.Game:
                case MessageModeType.GameHighlight:
                case MessageModeType.Look:
                case MessageModeType.DamageDealed:
                case MessageModeType.DamageReceived:
                case MessageModeType.Heal:
                case MessageModeType.Mana:
                case MessageModeType.Exp:
                case MessageModeType.DamageOthers:
                case MessageModeType.HealOthers:
                case MessageModeType.ExpOthers:
                case MessageModeType.Status:
                case MessageModeType.Loot:
                case MessageModeType.TradeNpc:
                case MessageModeType.Report:
                case MessageModeType.HotkeyUse:
                case MessageModeType.TutorialHint:
                case MessageModeType.Thankyou:
                case MessageModeType.BoostedCreature:
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                        channel = GetChannel(ChatStorage.ServerChannelId);
                    else
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;
                case MessageModeType.Guild:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelId);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannelId);
                        else
                            channel = GetChannel(ChatStorage.LocalChannelId);
                    }
                    break;
                case MessageModeType.PartyManagement:
                case MessageModeType.Party:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelId);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannelId);
                        else
                            channel = GetChannel(ChatStorage.LocalChannelId);
                    }
                    break;
                case MessageModeType.BarkLow:
                case MessageModeType.BarkLoud:
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;

                case MessageModeType.MonsterSay:
                case MessageModeType.MonsterYell:
                    break;

                case MessageModeType.Blue:
                case MessageModeType.Red:
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelId);
                    break;

                default:
                    AddDebugMessage(string.Format("ChatStorage.AddChannelMessage: Unhandled MessageMode ({0}).", mode));
                    break;
            }

            if (channel != null) {
                channel.AppendMessage(channelMessage);
                return channelMessage;
            }

            return null;
        }

        public ChannelMessage AddDebugMessage(string text) {
#if DEBUG || NDEBUG
            UnityEngine.Debug.LogWarning(text);
#endif

            if (GetChannel(DebugChannelId) == null) {
                var channel = AddChannel(DebugChannelId, "Debug", MessageModeType.ChannelManagement);
                channel.SendAllowed = false;
            }

            return AddChannelMessage(DebugChannelId, -1, null, 0, MessageModeType.ChannelManagement, text);
        }

        public string SendChannelMessage(string text, Channel channel, MessageModeType mode) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var player = OpenTibiaUnity.Player;
            if (protocolGame == null || !protocolGame.IsGameRunning)
                return "";
            
            text = text.Trim();
            if (text.Length > Constants.MaxTalkLength)
                text = text.Substring(0, Constants.MaxTalkLength);
            
            if (text.Length == 0)
                return "";

            mode = mode != MessageModeType.None ? mode : channel.SendMode;

            Utils.UnionStrInt channelId = null;
            if ((!channel.Id.IsInt || (channel.Id != DebugChannelId && channel.Id != LocalChannelId && channel.Id != ServerChannelId)) && channel.SendAllowed)
                channelId = channel.Id;
            
            string channelName = null;

            var regex1 = new System.Text.RegularExpressions.Regex(@"^#([sywbixc])\s+(.*)");
            var regex2 = new System.Text.RegularExpressions.Regex(@"^([*@$])([^\1]+?)\1\s*(.*)");

            string externalCommand = null;

            System.Text.RegularExpressions.Match match;
            if ((match = regex1.Match(text)) != null && match.Success) {
                externalCommand = match.Groups[1].ToString().ToLower();
                if (externalCommand == "b")
                    mode = MessageModeType.GamemasterBroadcast;
                else if (externalCommand == "c")
                    mode = MessageModeType.GamemasterChannel;
                else if (externalCommand == "i")
                    mode = MessageModeType.None;
                else if (externalCommand == "s")
                    mode = MessageModeType.Say;
                else if (externalCommand == "w")
                    mode = MessageModeType.Whisper;
                else if (externalCommand == "x")
                    mode = MessageModeType.None;
                else if (externalCommand == "y")
                    mode = MessageModeType.Yell;

                text = match.Groups[2].ToString();
            } else if ((match = regex2.Match(text)) != null && match.Success) {
                externalCommand = match.Groups[1].ToString().ToLower();
                if (externalCommand == "*")
                    mode = MessageModeType.PrivateTo;
                else if (externalCommand == "@")
                    mode = MessageModeType.GamemasterPrivateTo;

                channelId = match.Groups[2].ToString();
                channelName = channelId;
                if (channelName.Length > Constants.MaxChannelLength)
                    channelName = channelName.Substring(0, Constants.MaxChannelLength);

                text = match.Groups[3].ToString();
            }

            if (mode == MessageModeType.GamemasterChannel && (!channelId.IsInt || channelId == NpcChannelId)) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_CHANNEL_NO_ANONYMOUS);
                return "";
            }

            if (HasOwnPrivateChannel) {
                if (externalCommand == "i") {
                    protocolGame.SendInviteToChannel(text, OwnPrivateChannelId);
                } else if (externalCommand == "x") {
                    protocolGame.SendExcludeFromChannel(text, OwnPrivateChannelId);
                }
            }

            switch (mode) {
                case MessageModeType.None: break;
                case MessageModeType.Say:
                case MessageModeType.Whisper:
                case MessageModeType.Yell:
                case MessageModeType.GamemasterBroadcast:
                    protocolGame.SendTalk(mode, 0, null, text);
                    break;

                case MessageModeType.Channel:
                case MessageModeType.GamemasterChannel:
                    protocolGame.SendTalk(mode, channelId, null, text);
                    break;

                case MessageModeType.PrivateTo:
                case MessageModeType.GamemasterPrivateTo:
                    AddChannelMessage(channelId, -1, player.Name, (ushort)player.Level, mode, text);
                    if (channelId != player.Name.ToLower())
                        protocolGame.SendTalk(mode, 0, channelId, text);
                    break;
                case MessageModeType.NpcTo:
                    AddChannelMessage(channelId, -1, player.Name, (ushort)player.Level, mode, text);
                    protocolGame.SendTalk(mode, 0, null, text);
                    break;
            }

            if (channelId != channel.Id && (mode == MessageModeType.PrivateTo || mode == MessageModeType.GamemasterPrivateTo))
                return "*" + channelName + "* ";

            return "";
        }

        public static bool s_IsRestorableChannel(int channelId) {
            return channelId < FirstPrivateChannelId;
        }
        public static bool s_IsPrivateChannel(Utils.UnionStrInt channelId) {
            return channelId >= FirstPrivateChannelId && channelId <= LastPrivateChannelId;
        }
        public static bool s_IsGuildChannel(int channelId) {
            return channelId >= FirstGuildChannelId && channelId <= LastGuildChannelId;
        }
        public static bool s_IsPartyChannel(int channelId) {
            return channelId >= FirstPartyChannelId && channelId <= LastPartyChannelId;
        }
    }
}
