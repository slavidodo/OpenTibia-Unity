using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    public class ChatStorage
    {
        public class ChannelAddEvent : UnityEvent<Channel> { }
        public class ChannelsClearEvent : UnityEvent { }

        public const int MainAdvertisingChannel_id = 5;
        public const int RookAdvertisingChannel_id = 6;
        public const int HelpChannel_id = 7;

        public const int FirstPrivateChannel_id = 10;
        public const int LastPrivateChannel_id = 9999;
        public const int FirstGuildChannel_id = 10000;
        public const int LastGuildChannel_id = 19999;
        public const int FirstPartyChannel_id = 20000;
        public const int LastPartyChannel_id = 65533;

        public const int NpcChannel_id = 65534;
        public const int PrivateChannel_id = 65535;
        public const int LootChannel_id = 131067;
        //public const int SessionDumpChannel_id = 131068;
        public const int DebugChannel_id = 131069;
        public const int ServerChannel_id = 131070;
        public const int LocalChannel_id = 131071;
        public const int RVRChannel_id = 131072;

        public const int ChannelActivationTimeout = 500;

        private List<Channel> _channels;
        private int _ownPrivateChannel_id = -1;
        private int _channelActivationTimeout = 0;

        public ChannelAddEvent onAddChannel = new ChannelAddEvent();
        public ChannelsClearEvent onClearChannels = new ChannelsClearEvent();

        public int OwnPrivateChannel_id {
            get { return _ownPrivateChannel_id; }
            set { _ownPrivateChannel_id = value; }
        }

        public bool HasOwnPrivateChannel {
            get { return s_IsPrivateChannel(OwnPrivateChannel_id); }
        }

        public ChatStorage(Options.OptionStorage optionStorage) {
            _channels = new List<Channel>();
        }

        public int GetChannelIndex(Utils.UnionStrInt channel_id) {
            for (int i = 0; i < _channels.Count; i++) {
                if (_channels[i].Id == channel_id) {
                    return i;
                }
            }

            return -1;
        }

        public void LeaveChannel(Utils.UnionStrInt channel_id) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (protocolGame == null || protocolGame.IsGameRunning)
                return;

            if (s_IsPrivateChannel(channel_id)) {
                var channel = GetChannel(channel_id);
                if (channel != null && channel.SendAllowed)
                    protocolGame.SendLeaveChannel(channel_id);
            } else if (channel_id == NpcChannel_id) {
                protocolGame.SendCloseNPCChannel();
            } else {
                protocolGame.SendLeaveChannel(channel_id);
            }
            
            if (channel_id == _ownPrivateChannel_id)
                _ownPrivateChannel_id = -1;

            RemoveChannel(channel_id);
        }

        public void CloseChannel(Utils.UnionStrInt channel_id) {
            AddChannelMessage(channel_id, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_CHANNEL_CLOSED);

            Channel channel = GetChannel(channel_id);
            if (channel != null) {
                channel.SendAllowed = false;
                channel.Closable = true;
                //channel.ClearNicklist(); TODO
            }

            if (_ownPrivateChannel_id == channel_id)
                _ownPrivateChannel_id = -1;
        }

        public Channel GetChannelAt(int channelIndex) {
            return channelIndex >= 0 && channelIndex < _channels.Count ? _channels[channelIndex] : null; 
        }

        public Channel GetChannel(Utils.UnionStrInt channel_id) {
            foreach (var channel in _channels) {
                if (channel.Id == channel_id)
                    return channel;
            }
            
            return null;
        }

        public Channel AddChannel(Utils.UnionStrInt channel_id, string name, MessageModeType mode) {
            Channel channel = GetChannel(channel_id);

            if (channel != null) {
                channel.Name = name;
                channel.SendAllowed = true;
            } else {
                channel = new Channel(channel_id, name, mode);
                _channels.Add(channel);
                onAddChannel.Invoke(channel);

                switch ((int)channel_id) {
                    case HelpChannel_id:
                        if (OpenTibiaUnity.GameManager.ClientVersion >= 854)
                            AddChannelMessage(channel_id, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_HELP);
                        else
                            AddChannelMessage(channel_id, -1, null, 0, MessageModeType.GamemasterChannel, TextResources.CHANNEL_MSG_HELP_LEGACY);
                        break;
                    case MainAdvertisingChannel_id:
                    case RookAdvertisingChannel_id:
                        AddChannelMessage(channel_id, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_ADVERTISING);
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

        public Channel RemoveChannel(Utils.UnionStrInt channel_id) {
            var channel = GetChannel(channel_id);
            if (channel != null)
                _channels.Remove(channel);

            if (channel != null && channel.Id == _ownPrivateChannel_id)
                _ownPrivateChannel_id = -1;

            return channel;
        }

        public void LoadChannels() {

        }

        public void Reset() {
            _channels.Clear();
            onClearChannels.Invoke();

            var tmpChannel = GetChannel(ChatStorage.LocalChannel_id);
            if (tmpChannel == null) {
                if (OpenTibiaUnity.GameManager.ClientVersion >= 870)
                    tmpChannel = AddChannel(ChatStorage.LocalChannel_id, TextResources.CHANNEL_NAME_DEFAULT, MessageModeType.Say);
                else
                    tmpChannel = AddChannel(ChatStorage.LocalChannel_id, TextResources.CHANNEL_NAME_DEFAULT_LEGACY, MessageModeType.Say);
                tmpChannel.Closable = false;
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog)) {
                tmpChannel = GetChannel(ServerChannel_id);
                if (tmpChannel == null) {
                    tmpChannel = AddChannel(ServerChannel_id, TextResources.CHANNEL_NAME_SERVERLOG, MessageModeType.Say);
                    tmpChannel.Closable = false;
                }
            }
            
            _ownPrivateChannel_id = -1;
            ResetChannelActivationTimeout();
        }

        public void ResetChannelActivationTimeout() {
            _channelActivationTimeout = OpenTibiaUnity.TicksMillis + ChannelActivationTimeout;
        }

        public ChannelMessage AddChannelMessage(Utils.UnionStrInt channel_id, int statement_id, string speaker, int speakerLevel, MessageModeType mode, string text) {
            var messageFilterSet = OpenTibiaUnity.OptionStorage.GetMessageFilterSet(MessageFilterSet.DefaultSet);
            var messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowChannelMessage)
                return null;

            var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
            if (!messageMode.IgnoreNameFilter && (nameFilterSet == null || !nameFilterSet.AcceptMessage(mode, speaker, text)))
                return null;

            bool isReportName = speaker != null && channel_id.IsInt && ((int)channel_id == ChatStorage.HelpChannel_id || speakerLevel > 0);
            bool isReportStatement = statement_id > 0;
            Channel channel = null;
            ChannelMessage channelMessage = new ChannelMessage(statement_id, speaker, speakerLevel, mode, text);
            channelMessage.FormatMessage(messageFilterSet.ShowTimeStamps, messageFilterSet.ShowLevels, messageMode.TextARGB, messageMode.HighlightARGB);
            
            switch (mode) {
                case MessageModeType.Say:
                case MessageModeType.Yell:
                case MessageModeType.Whisper:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.PrivateFrom:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channel_id);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.PrivateTo:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channel_id);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.ChannelManagement:
                    channel = GetChannel(channel_id);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.Channel:
                case MessageModeType.ChannelHighlight:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channel_id);
                    break;
                case MessageModeType.Spell:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.NpcFromStartBlock:
                case MessageModeType.NpcFrom:
                case MessageModeType.NpcTo:
                    channel = AddChannel(ChatStorage.NpcChannel_id, "NPCs", MessageModeType.NpcTo);
                    break;
                case MessageModeType.GamemasterBroadcast:
                    channel = GetChannel(ChatStorage.ServerChannel_id);
                    break;
                case MessageModeType.GamemasterChannel:
                    channel = GetChannel(channel_id);
                    break;
                case MessageModeType.GamemasterPrivateFrom:
                case MessageModeType.GamemasterPrivateTo:
                    channel = GetChannel(ChatStorage.ServerChannel_id);
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
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                        channel = GetChannel(ChatStorage.ServerChannel_id);
                    else
                        channel = GetChannel(ChatStorage.LocalChannel_id);
                    break;
                case MessageModeType.Guild:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channel_id);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannel_id);
                        else
                            channel = GetChannel(ChatStorage.LocalChannel_id);
                    }
                    break;
                case MessageModeType.PartyManagement:
                case MessageModeType.Party:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channel_id);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannel_id);
                        else
                            channel = GetChannel(ChatStorage.LocalChannel_id);
                    }
                    break;
                case MessageModeType.BarkLow:
                case MessageModeType.BarkLoud:
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannel_id);
                        else
                            channel = GetChannel(ChatStorage.LocalChannel_id);
                    }
                    break;

                case MessageModeType.MonsterSay:
                case MessageModeType.MonsterYell:
                    break;

                case MessageModeType.Blue:
                case MessageModeType.Red:
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannel_id);
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

            if (GetChannel(DebugChannel_id) == null) {
                var channel = AddChannel(DebugChannel_id, "Debug", MessageModeType.ChannelManagement);
                channel.SendAllowed = false;
            }

            return AddChannelMessage(DebugChannel_id, -1, null, 0, MessageModeType.ChannelManagement, text);
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

            Utils.UnionStrInt channel_id = null;
            if ((!channel.Id.IsInt || (channel.Id != DebugChannel_id && channel.Id != LocalChannel_id && channel.Id != ServerChannel_id)) && channel.SendAllowed)
                channel_id = channel.Id;
            
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

                channel_id = match.Groups[2].ToString();
                channelName = channel_id;
                if (channelName.Length > Constants.MaxChannelLength)
                    channelName = channelName.Substring(0, Constants.MaxChannelLength);

                text = match.Groups[3].ToString();
            }

            if (mode == MessageModeType.GamemasterChannel && (!channel_id.IsInt || channel_id == NpcChannel_id)) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_CHANNEL_NO_ANONYMOUS);
                return "";
            }

            if (HasOwnPrivateChannel) {
                if (externalCommand == "i") {
                    protocolGame.SendInviteToChannel(text, OwnPrivateChannel_id);
                } else if (externalCommand == "x") {
                    protocolGame.SendExcludeFromChannel(text, OwnPrivateChannel_id);
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
                    protocolGame.SendTalk(mode, channel_id, null, text);
                    break;

                case MessageModeType.PrivateTo:
                case MessageModeType.GamemasterPrivateTo:
                    AddChannelMessage(channel_id, -1, player.Name, (ushort)player.Level, mode, text);
                    if (channel_id != player.Name.ToLower())
                        protocolGame.SendTalk(mode, 0, channel_id, text);
                    break;
                case MessageModeType.NpcTo:
                    AddChannelMessage(channel_id, -1, player.Name, (ushort)player.Level, mode, text);
                    protocolGame.SendTalk(mode, 0, null, text);
                    break;
            }

            if (channel_id != channel.Id && (mode == MessageModeType.PrivateTo || mode == MessageModeType.GamemasterPrivateTo))
                return "*" + channelName + "* ";

            return "";
        }

        public static bool s_IsRestorableChannel(int channel_id) {
            return channel_id < FirstPrivateChannel_id;
        }
        public static bool s_IsPrivateChannel(Utils.UnionStrInt channel_id) {
            return channel_id >= FirstPrivateChannel_id && channel_id <= LastPrivateChannel_id;
        }
        public static bool s_IsGuildChannel(int channel_id) {
            return channel_id >= FirstGuildChannel_id && channel_id <= LastGuildChannel_id;
        }
        public static bool s_IsPartyChannel(int channel_id) {
            return channel_id >= FirstPartyChannel_id && channel_id <= LastPartyChannel_id;
        }
    }
}
