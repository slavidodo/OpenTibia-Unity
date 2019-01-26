using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Chat
{
    public class ChatStorage
    {
        public class ChannelAddEvent : UnityEvent<Channel> { }

        public const int MainAdvertisingChannelID = 5;
        public const int RookAdvertisingChannelID = 6;
        public const int HelpChannelID = 7;

        public const int FirstPrivateChannelID = 10;
        public const int LastPrivateChannelID = 9999;
        public const int FirstGuildChannelID = 10000;
        public const int LastGuildChannelID = 19999;
        public const int FirstPartyChannelID = 20000;
        public const int LastPartyChannelID = 65533;

        public const int NpcChannelID = 65534;
        public const int PrivateChannelID = 65535;
        public const int LootChannelID = 131067;
        //public const int SessionDumpChannelID = 131068;
        public const int DebugChannelID = 131069;
        public const int ServerChannelID = 131070;
        public const int LocalChannelID = 131071;

        public const int ChannelActivationTimeout = 500;

        private List<Channel> m_Channels;
        private int m_OwnPrivateChannelID = -1;
        private int m_ChannelActivationTimeout = 0;

        public ChannelAddEvent onAddChannel = new ChannelAddEvent();

        public int OwnPrivateChannelID {
            get { return m_OwnPrivateChannelID; }
            set { m_OwnPrivateChannelID = value; }
        }

        public bool HasOwnPrivateChannel {
            get { return s_IsPrivateChannel(OwnPrivateChannelID); }
        }

        public ChatStorage(Options.OptionStorage optionStorage) {
            m_Channels = new List<Channel>();
        }

        public int GetChannelIndex(Utility.UnionStrInt channelID) {
            for (int i = 0; i < m_Channels.Count; i++) {
                if (m_Channels[i].ID == channelID) {
                    return i;
                }
            }

            return -1;
        }

        public void LeaveChannel(Utility.UnionStrInt channelID) {
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

        public void CloseChannel(Utility.UnionStrInt channelID) {
            AddChannelMessage(channelID, -1, null, 0, MessageModes.ChannelManagment, "The channel has been closed. You need to re-join the channel if you get invited.");

            Channel channel = GetChannel(channelID);
            if (channel != null) {
                channel.SendAllowed = false;
                channel.Closable = true;
                //channel.ClearNicklist(); TODO
            }

            if (m_OwnPrivateChannelID == channelID)
                m_OwnPrivateChannelID = -1;
        }

        public Channel GetChannelAt(int channelIndex) {
            return channelIndex >= 0 && channelIndex < m_Channels.Count ? m_Channels[channelIndex] : null; 
        }

        public Channel GetChannel(Utility.UnionStrInt channelID) {
            foreach (var channel in m_Channels) {
                if (channel.ID == channelID)
                    return channel;
            }
            
            return null;
        }

        public Channel AddChannel(Utility.UnionStrInt channelID, string name, MessageModes mode) {
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
                        AddChannelMessage(channelID, -1, null, 0, MessageModes.ChannelManagment, "Welcome to the help channel. In this channel you can ask questions about/ Tibia. Experienced players will gladly help you to the best of their knowledge. If their answer was helpful, reward them with a \"Thank You!\" which/you /can select by right-clicking on their statement. For detailed information about quests and other game content, please take a look at our /supported fansite in /the community section of the official Tibia website.");
                        break;
                    case MainAdvertisingChannelID:
                    case RookAdvertisingChannelID:
                        AddChannelMessage(channelID, -1, null, 0, MessageModes.ChannelManagment, "Here you can advertise all kinds of things. Among others, you can trade Tibia/ items, advertise ingame events, seek characters for a quest or a hunting group, find members for your guild or look for somebody to help you with /something. It goes without saying that all advertisements must conform to the Tibia Rules. Keep in mind that it is illegal to advertise trades /including real money or Tibia characters.");
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

        public Channel RemoveChannel(Utility.UnionStrInt channelID) {
            var channel = GetChannel(channelID);
            if (channel != null)
                m_Channels.Remove(channel);

            if (channel != null && channel.ID == m_OwnPrivateChannelID)
                m_OwnPrivateChannelID = -1;

            return channel;
        }

        public void LoadChannels() {

        }

        public void Reset() {
            m_Channels.Clear();
            
            var tmpChannel = GetChannel(ChatStorage.LocalChannelID);
            if (tmpChannel == null) {
                tmpChannel = AddChannel(ChatStorage.LocalChannelID, "Local Chat", MessageModes.Say);
                tmpChannel.Closable = false;
            }
            
            tmpChannel = GetChannel(ServerChannelID);
            if (tmpChannel == null) {
                tmpChannel = AddChannel(ServerChannelID, "Server Log", MessageModes.Say);
                tmpChannel.Closable = false;
            }
            
            m_OwnPrivateChannelID = -1;
            ResetChannelActivationTimeout();
        }

        public void ResetChannelActivationTimeout() {
            m_ChannelActivationTimeout = OpenTibiaUnity.TicksMillis + ChannelActivationTimeout;
        }

        public ChannelMessage AddChannelMessage(Utility.UnionStrInt channelID, int statementID, string speaker, int speakerLevel, MessageModes mode, string text) {
            var messageFilterSet = OpenTibiaUnity.OptionStorage.GetMessageFilterSet(MessageFilterSet.Default);
            var messageMode = messageFilterSet.GetMessageMode(mode);
            if (messageMode == null || !messageMode.ShowChannelMessage)
                return null;

            // TODO: NameFilterSet

            bool isReportName = speaker != null && channelID.IsInt && ((int)channelID == ChatStorage.HelpChannelID || speakerLevel > 0);
            bool isReportStatement = statementID > 0;
            Channel channel = null;
            ChannelMessage channelMessage = new ChannelMessage(statementID, speaker, speakerLevel, mode, text);
            channelMessage.FormatMessage(messageFilterSet.ShowTimeStamps, messageFilterSet.ShowLevels, messageMode.TextARGB, messageMode.HighlightARGB);
            
            switch (mode) {
                case MessageModes.Say:
                case MessageModes.Yell:
                case MessageModes.Whisper:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModes.PrivateFrom:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModes.PrivateTo:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModes.ChannelManagment:
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModes.Channel:
                case MessageModes.ChannelHighlight:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    break;
                case MessageModes.Spell:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channel = GetChannel(ChatStorage.LocalChannelID);
                    break;
                case MessageModes.NpcFromStartBlock:
                case MessageModes.NpcFrom:
                case MessageModes.NpcTo:
                    channel = AddChannel(ChatStorage.NpcChannelID, "NPCs", MessageModes.NpcTo);
                    break;
                case MessageModes.GamemasterBroadcast:
                    channel = GetChannel(ChatStorage.ServerChannelID);
                    break;
                case MessageModes.GamemasterChannel:
                    channel = GetChannel(channelID);
                    break;
                case MessageModes.GamemasterPrivateFrom:
                case MessageModes.GamemasterPrivateTo:
                    channel = GetChannel(ChatStorage.ServerChannelID);
                    break;
                case MessageModes.Login:
                case MessageModes.Admin:
                case MessageModes.Game:
                case MessageModes.GameHighlight:
                case MessageModes.Look:
                case MessageModes.DamageDealed:
                case MessageModes.DamageReceived:
                case MessageModes.Heal:
                case MessageModes.Mana:
                case MessageModes.Exp:
                case MessageModes.DamageOthers:
                case MessageModes.HealOthers:
                case MessageModes.ExpOthers:
                case MessageModes.Status:
                case MessageModes.Loot:
                case MessageModes.TradeNpc:
                case MessageModes.Report:
                case MessageModes.HotkeyUse:
                case MessageModes.TutorialHint:
                case MessageModes.Thankyou:
                    channel = GetChannel(ChatStorage.ServerChannelID);
                    break;
                case MessageModes.Guild:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.ServerChannelID);
                    break;
                case MessageModes.PartyManagement:
                case MessageModes.Party:
                    channelMessage.SetReportTypeAllowed(ReportTypes.Name, isReportName);
                    channelMessage.SetReportTypeAllowed(ReportTypes.Statement, isReportStatement);
                    channel = GetChannel(channelID);
                    if (channel == null)
                        channel = GetChannel(ChatStorage.ServerChannelID);
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

            if (GetChannel(DebugChannelID) == null) {
                var channel = AddChannel(DebugChannelID, "Debug", MessageModes.ChannelManagment);
                channel.SendAllowed = false;
            }

            return AddChannelMessage(DebugChannelID, -1, null, 0, MessageModes.ChannelManagment, text);
        }
        public string SendChannelMessage(string text, Channel channel, MessageModes mode) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var player = OpenTibiaUnity.Player;
            if (protocolGame == null || !protocolGame.IsGameRunning)
                return "";
            
            text = text.Trim();
            if (text.Length > Constants.MaxTalkLength)
                text = text.Substring(0, Constants.MaxTalkLength);
            
            if (text.Length == 0)
                return "";

            mode = mode != MessageModes.None ? mode : channel.SendMode;

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
                    mode = MessageModes.GamemasterBroadcast;
                else if (externalCommand == "c")
                    mode = MessageModes.GamemasterChannel;
                else if (externalCommand == "i")
                    mode = MessageModes.None;
                else if (externalCommand == "s")
                    mode = MessageModes.Say;
                else if (externalCommand == "w")
                    mode = MessageModes.Whisper;
                else if (externalCommand == "x")
                    mode = MessageModes.None;
                else if (externalCommand == "y")
                    mode = MessageModes.Yell;

                text = match.Groups[2].ToString();
            } else if ((match = regex2.Match(text)) != null && match.Success) {
                externalCommand = match.Groups[1].ToString().ToLower();
                if (externalCommand == "*")
                    mode = MessageModes.PrivateTo;
                else if (externalCommand == "@")
                    mode = MessageModes.GamemasterPrivateTo;

                channelID = match.Groups[2].ToString();
                channelName = channelID;
                if (channelName.Length > Constants.MaxChannelLength)
                    channelName = channelName.Substring(0, Constants.MaxChannelLength);

                text = match.Groups[3].ToString();
            }

            if (mode == MessageModes.GamemasterChannel && (!channelID.IsInt || channelID == NpcChannelID)) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_CHANNEL_NO_ANONYMOUS);
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
                case MessageModes.None: break;
                case MessageModes.Say:
                case MessageModes.Whisper:
                case MessageModes.Yell:
                    protocolGame.SendTalk(mode, text);
                    break;

                case MessageModes.Channel:
                    protocolGame.SendTalk(mode, (int)channelID, text);
                    break;

                case MessageModes.PrivateTo:
                    AddChannelMessage(channelID, -1, player.Name, player.Level, mode, text);
                    if (channelID != player.Name.ToLower())
                        protocolGame.SendTalk(mode, channelID, text);
                    break;
            }

            if (channelID != channel.ID && (mode == MessageModes.PrivateTo || mode == MessageModes.GamemasterPrivateTo))
                return "*" + channelName + "* ";

            return "";
        }

        public static bool s_IsRestorableChannel(int channelID) {
            return channelID < FirstPrivateChannelID;
        }
        public static bool s_IsPrivateChannel(Utility.UnionStrInt channelID) {
            return channelID >= FirstPrivateChannelID && channelID <= LastPrivateChannelID;
        }
        public static bool s_IsGuildChannel(int channelID) {
            return channelID >= FirstGuildChannelID && channelID <= LastGuildChannelID;
        }
        public static bool s_IsPartyChannel(int channelID) {
            return channelID >= FirstPartyChannelID && channelID <= LastPartyChannelID;
        }
    }
}
