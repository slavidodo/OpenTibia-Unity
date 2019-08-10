using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    internal class ChatStorage
    {
        internal class ChannelAddEvent : UnityEvent<Channel> { }
        internal class ChannelsClearEvent : UnityEvent { }

        internal const int MainAdvertisingChannelID = 5;
        internal const int RookAdvertisingChannelID = 6;
        internal const int HelpChannelID = 7;

        internal const int FirstPrivateChannelID = 10;
        internal const int LastPrivateChannelID = 9999;
        internal const int FirstGuildChannelID = 10000;
        internal const int LastGuildChannelID = 19999;
        internal const int FirstPartyChannelID = 20000;
        internal const int LastPartyChannelID = 65533;

        internal const int NpcChannelID = 65534;
        internal const int PrivateChannelID = 65535;
        internal const int LootChannelID = 131067;
        //internal const int SessionDumpChannelID = 131068;
        internal const int DebugChannelID = 131069;
        internal const int ServerChannelID = 131070;
        internal const int LocalChannelID = 131071;
        internal const int RVRChannelID = 131072;

        internal const int ChannelActivationTimeout = 500;

        private List<Channel> m_Channels;
        private int m_OwnPrivateChannelID = -1;
        private int m_ChannelActivationTimeout = 0;

        internal ChannelAddEvent onAddChannel = new ChannelAddEvent();
        internal ChannelsClearEvent onClearChannels = new ChannelsClearEvent();

        internal int OwnPrivateChannelID {
            get { return m_OwnPrivateChannelID; }
            set { m_OwnPrivateChannelID = value; }
        }

        internal bool HasOwnPrivateChannel {
            get { return s_IsPrivateChannel(OwnPrivateChannelID); }
        }

        internal ChatStorage(Options.OptionStorage optionStorage) {
            m_Channels = new List<Channel>();
        }

        internal int GetChannelIndex(Utility.UnionStrInt channelID) {
            for (int i = 0; i < m_Channels.Count; i++) {
                if (m_Channels[i].ID == channelID) {
                    return i;
                }
            }

            return -1;
        }

        internal void LeaveChannel(Utility.UnionStrInt channelID) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (protocolGame == null || protocolGame.IsGameRunning)
                return;

            if (s_IsPrivateChannel(channelID)) {
                var channel = GetChannel(channelID);
                if (channel != null && channel.SendAllowed)
                    protocolGame.SendLeaveChannel(channelID);
            } else if (channelID == NpcChannelID) {
                protocolGame.SendCloseNPCChannel();
            } else {
                protocolGame.SendLeaveChannel(channelID);
            }
            
            if (channelID == m_OwnPrivateChannelID)
                m_OwnPrivateChannelID = -1;

            RemoveChannel(channelID);
        }

        internal void CloseChannel(Utility.UnionStrInt channelID) {
            AddChannelMessage(channelID, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_CHANNEL_CLOSED);

            Channel channel = GetChannel(channelID);
            if (channel != null) {
                channel.SendAllowed = false;
                channel.Closable = true;
                //channel.ClearNicklist(); TODO
            }

            if (m_OwnPrivateChannelID == channelID)
                m_OwnPrivateChannelID = -1;
        }

        internal Channel GetChannelAt(int channelIndex) {
            return channelIndex >= 0 && channelIndex < m_Channels.Count ? m_Channels[channelIndex] : null; 
        }

        internal Channel GetChannel(Utility.UnionStrInt channelID) {
            foreach (var channel in m_Channels) {
                if (channel.ID == channelID)
                    return channel;
            }
            
            return null;
        }

        internal Channel AddChannel(Utility.UnionStrInt channelID, string name, MessageModeType mode) {
            Channel channel = GetChannel(channelID);

            if (channel != null) {
                channel.Name = name;
                channel.SendAllowed = true;
            } else {
                channel = new Channel(channelID, name, mode);
                m_Channels.Add(channel);
                onAddChannel.Invoke(channel);

                switch ((int)channelID) {
                    case HelpChannelID:
                        if (OpenTibiaUnity.GameManager.ClientVersion >= 854)
                            AddChannelMessage(channelID, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_HELP);
                        else
                            AddChannelMessage(channelID, -1, null, 0, MessageModeType.GamemasterChannel, TextResources.CHANNEL_MSG_HELP_LEGACY);
                        break;
                    case MainAdvertisingChannelID:
                    case RookAdvertisingChannelID:
                        AddChannelMessage(channelID, -1, null, 0, MessageModeType.ChannelManagement, TextResources.CHANNEL_MSG_ADVERTISING);
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

        internal Channel RemoveChannel(Utility.UnionStrInt channelID) {
            var channel = GetChannel(channelID);
            if (channel != null)
                m_Channels.Remove(channel);

            if (channel != null && channel.ID == m_OwnPrivateChannelID)
                m_OwnPrivateChannelID = -1;

            return channel;
        }

        internal void LoadChannels() {

        }

        internal void Reset() {
            m_Channels.Clear();
            onClearChannels.Invoke();

            var tmpChannel = GetChannel(ChatStorage.LocalChannelID);
            if (tmpChannel == null) {
                if (OpenTibiaUnity.GameManager.ClientVersion >= 870)
                    tmpChannel = AddChannel(ChatStorage.LocalChannelID, TextResources.CHANNEL_NAME_DEFAULT, MessageModeType.Say);
                else
                    tmpChannel = AddChannel(ChatStorage.LocalChannelID, TextResources.CHANNEL_NAME_DEFAULT_LEGACY, MessageModeType.Say);
                tmpChannel.Closable = false;
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog)) {
                tmpChannel = GetChannel(ServerChannelID);
                if (tmpChannel == null) {
                    tmpChannel = AddChannel(ServerChannelID, TextResources.CHANNEL_NAME_SERVERLOG, MessageModeType.Say);
                    tmpChannel.Closable = false;
                }
            }
            
            m_OwnPrivateChannelID = -1;
            ResetChannelActivationTimeout();
        }

        internal void ResetChannelActivationTimeout() {
            m_ChannelActivationTimeout = OpenTibiaUnity.TicksMillis + ChannelActivationTimeout;
        }

        internal ChannelMessage AddChannelMessage(Utility.UnionStrInt channelID, int statementID, string speaker, int speakerLevel, MessageModeType mode, string text) {
            var messageFilterSet = OpenTibiaUnity.OptionStorage.GetMessageFilterSet(MessageFilterSet.DefaultSet);
            var messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowChannelMessage)
                return null;

            var nameFilterSet = OpenTibiaUnity.OptionStorage.GetNameFilterSet(NameFilterSet.DefaultSet);
            if (!messageMode.IgnoreNameFilter && (nameFilterSet == null || !nameFilterSet.AcceptMessage(mode, speaker, text)))
                return null;

            bool isReportName = speaker != null && channelID.IsInt && ((int)channelID == ChatStorage.HelpChannelID || speakerLevel > 0);
            bool isReportStatement = statementID > 0;
            Channel channel = null;
            ChannelMessage channelMessage = new ChannelMessage(statementID, speaker, speakerLevel, mode, text);
            channelMessage.FormatMessage(messageFilterSet.ShowTimeStamps, messageFilterSet.ShowLevels, messageMode.TextARGB, messageMode.HighlightARGB);
            
            switch (mode) {
                case MessageModeType.Say:
                case MessageModeType.Yell:
                case MessageModeType.Whisper:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.PrivateFrom:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.PrivateTo:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.ChannelManagement:
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.Channel:
                case MessageModeType.ChannelHighlight:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    break;
                case MessageModeType.Spell:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.NpcFromStartBlock:
                case MessageModeType.NpcFrom:
                case MessageModeType.NpcTo:
                    channel = AddChannel(ChatStorage.NpcChannelID, "NPCs", MessageModeType.NpcTo);
                    break;
                case MessageModeType.GamemasterBroadcast:
                    channel = GetChannel(ChatStorage.ServerChannelID);
                    break;
                case MessageModeType.GamemasterChannel:
                    channel = GetChannel(channelID);
                    break;
                case MessageModeType.GamemasterPrivateFrom:
                case MessageModeType.GamemasterPrivateTo:
                    channel = GetChannel(ChatStorage.ServerChannelID);
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
                        channel = GetChannel(ChatStorage.ServerChannelID);
                    else
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModeType.Guild:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannelID);
                        else
                            channel = GetChannel(ChatStorage.LocalChannelID);
                    }
                    break;
                case MessageModeType.PartyManagement:
                case MessageModeType.Party:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannelID);
                        else
                            channel = GetChannel(ChatStorage.LocalChannelID);
                    }
                    break;
                case MessageModeType.BarkLow:
                case MessageModeType.BarkLoud:
                    if (channel == null) {
                        if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameServerLog))
                            channel = GetChannel(ChatStorage.ServerChannelID);
                        else
                            channel = GetChannel(ChatStorage.LocalChannelID);
                    }
                    break;

                case MessageModeType.MonsterSay:
                case MessageModeType.MonsterYell:
                    break;

                case MessageModeType.Blue:
                case MessageModeType.Red:
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
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

        internal ChannelMessage AddDebugMessage(string text) {
#if DEBUG || NDEBUG
            UnityEngine.Debug.LogWarning(text);
#endif

            if (GetChannel(DebugChannelID) == null) {
                var channel = AddChannel(DebugChannelID, "Debug", MessageModeType.ChannelManagement);
                channel.SendAllowed = false;
            }

            return AddChannelMessage(DebugChannelID, -1, null, 0, MessageModeType.ChannelManagement, text);
        }

        internal string SendChannelMessage(string text, Channel channel, MessageModeType mode) {
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

            Utility.UnionStrInt channelID = null;
            if ((!channel.ID.IsInt || (channel.ID != DebugChannelID && channel.ID != LocalChannelID && channel.ID != ServerChannelID)) && channel.SendAllowed)
                channelID = channel.ID;
            
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

                channelID = match.Groups[2].ToString();
                channelName = channelID;
                if (channelName.Length > Constants.MaxChannelLength)
                    channelName = channelName.Substring(0, Constants.MaxChannelLength);

                text = match.Groups[3].ToString();
            }

            if (mode == MessageModeType.GamemasterChannel && (!channelID.IsInt || channelID == NpcChannelID)) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_CHANNEL_NO_ANONYMOUS);
                return "";
            }

            if (HasOwnPrivateChannel) {
                if (externalCommand == "i") {
                    protocolGame.SendInviteToChannel(text, OwnPrivateChannelID);
                } else if (externalCommand == "x") {
                    protocolGame.SendExcludeFromChannel(text, OwnPrivateChannelID);
                }
            }

            switch (mode) {
                case MessageModeType.None: break;
                case MessageModeType.Say:
                case MessageModeType.Whisper:
                case MessageModeType.Yell:
                    protocolGame.SendTalk(mode, 0, null, text);
                    break;

                case MessageModeType.Channel:
                    protocolGame.SendTalk(mode, channelID, null, text);
                    break;

                case MessageModeType.PrivateTo:
                    AddChannelMessage(channelID, -1, player.Name, (ushort)player.Level, mode, text);
                    if (channelID != player.Name.ToLower())
                        protocolGame.SendTalk(mode, 0, channelID, text);
                    break;
            }

            if (channelID != channel.ID && (mode == MessageModeType.PrivateTo || mode == MessageModeType.GamemasterPrivateTo))
                return "*" + channelName + "* ";

            return "";
        }

        internal static bool s_IsRestorableChannel(int channelID) {
            return channelID < FirstPrivateChannelID;
        }
        internal static bool s_IsPrivateChannel(Utility.UnionStrInt channelID) {
            return channelID >= FirstPrivateChannelID && channelID <= LastPrivateChannelID;
        }
        internal static bool s_IsGuildChannel(int channelID) {
            return channelID >= FirstGuildChannelID && channelID <= LastGuildChannelID;
        }
        internal static bool s_IsPartyChannel(int channelID) {
            return channelID >= FirstPartyChannelID && channelID <= LastPartyChannelID;
        }
    }
}
