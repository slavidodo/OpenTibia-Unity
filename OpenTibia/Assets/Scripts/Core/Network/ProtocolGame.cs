using System;
using System.Net.Sockets;
using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        public delegate void OnPendingState();
        
        public string SessionKey;
        public string CharacterName;
        public string AccountName;
        public string Password;
        public string AuthenticatorToken;
        public string WorldName;
        public string WorldIp;
        public int WorldPort;

        readonly OnPendingState m_OnPendingState;
        
        private Appearances.AppearanceStorage m_AppearanceStorage { get { return OpenTibiaUnity.AppearanceStorage; } }
        private Creatures.CreatureStorage m_CreatureStorage { get { return OpenTibiaUnity.CreatureStorage; } }
        private MiniMap.MiniMapStorage m_MiniMapStorage { get { return OpenTibiaUnity.MiniMapStorage; } }
        private WorldMap.WorldMapStorage m_WorldMapStorage { get { return OpenTibiaUnity.WorldMapStorage; } }
        private Chat.ChatStorage m_ChatStorage { get { return OpenTibiaUnity.ChatStorage; } }
        private Chat.MessageStorage m_MessageStorage { get { return OpenTibiaUnity.MessageStorage; } }
        private Creatures.Player m_Player { get { return OpenTibiaUnity.Player; } }

        private bool m_FirstRecv = true;
        private byte m_PrevOpcode = 0;
        private byte m_LastOpcode = 0;
        
        public int BeatDuration = 0;

        // More accurate about the game play..
        public bool IsGameRunning { get; private set; } = false;
        public bool GameEndProcessed { get; private set; } = false;

        public ProtocolGame() : base() {
            BuildMessageNodesMap(OpenTibiaUnity.GameManager.ClientVersion);
        }

        public override void Connect() {
            m_Player.Name = CharacterName;

            Connect(WorldIp, WorldPort);
        }
        public void Disconnect(bool forceLogout = true) {
            if (forceLogout && IsGameRunning)
                SendLeaveGame();
            else
                base.Disconnect();
        }

        protected override void OnConnect() {
            var gameManager = OpenTibiaUnity.GameManager;

            if (gameManager.GetFeature(GameFeatures.GameWorldName))
                SendWorldName();

            if (gameManager.GetFeature(GameFeatures.GameProtocolChecksum)) {
                ChecksumEnabled = true;
            }

            if (!gameManager.GetFeature(GameFeatures.GameChallengeOnLogin)) {
                try {
                    SendLoginPacket(0, 0);
                } catch (Exception e) {
                    Debug.Log(e);
                    OnError(SocketError.SocketError, e.Message);
                }
            }

            BeginRecv();
        }

        protected override void OnError(SocketError _, string __) {
            Disconnect();
        }

        protected override void OnDisconnect() {
            IsGameRunning = false;
            if (!GameEndProcessed) {
                GameEndProcessed = true;
                OpenTibiaUnity.GameManager.InvokeOnMainThread(OpenTibiaUnity.GameManager.ProcessGameEnd);
            }
        }

        protected override void OnRecv(InputMessage message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(delegate {
                try {
                    base.OnRecv(message);
                } catch (Exception e) {
                    m_ChatStorage.AddDebugMessage(string.Format("ProtocolGame.OnRecv: {0}\nStackTrace: {1}", e.Message, new System.Diagnostics.StackTrace(e, true)));
                    return;
                }

                if (m_FirstRecv) {
                    m_FirstRecv = false;

                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameMessageSizeCheck)) {
                        ushort size = message.GetU16();
                        int unreadSize = message.GetUnreadSize();
                        if (unreadSize != size) {
                            m_ChatStorage.AddDebugMessage(string.Format("ProtocolGame.OnRecv: Invalid message size (size: {0}, unread: {1})", size, unreadSize));
                            return;
                        }
                    }
                }

                bool read = false;
                while (message.CanRead()) {
                    read = true;
                    byte opcode = message.PeekU8();

                    if (!OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameLoginPending)) {
                        if (!IsGameRunning && opcode > GameServerOpCodes.GameServerFirstGameOpcode) {
                            IsGameRunning = true;
                            OpenTibiaUnity.GameManager.ProcessGameStart();
                        }
                    }

                    try {
                        if (!ParsePacket(message))
                            break;
                    } catch (Exception e) {
                        string err = string.Format("ProtocolGame: failed parsing opcode: {0}, last opcode ({1}), prev opcode ({2}), unread ({3}).\nMessage: {4}\nStackTrace: {5}",
                            opcode, m_LastOpcode, m_PrevOpcode, message.GetUnreadSize(), e.Message, new System.Diagnostics.StackTrace(e, true));

                        m_ChatStorage.AddDebugMessage(err);
                        break;
                    }
                }

                if (read)
                    m_WorldMapStorage.ProtocolGameMessageProcessingFinished();
            });
        }

        private void SendWorldName() {
            int index = WorldName.IndexOf("\n");
            if (index > 0 && index != WorldName.Length - 1)
                throw new Exception(@"ProtocolGame.SendWorldName: World name can't contain \n or \r\n.");

            if (!WorldName.EndsWith("\n"))
                WorldName += "\n";

            var message = new OutputMessage();
            message.AddString(WorldName, true);
            WriteToOutput(message, true);
        }

        private bool ParsePacket(InputMessage message) {
            byte opcode = message.GetU8();
            switch (opcode) {
                case GameServerOpCodes.LoginOrPendingState:
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameLoginPending))
                        OpenTibiaUnity.GameManager.ProcessGamePending();
                    else
                        ParseLoginSuccess(message);
                    break;
                case GameServerOpCodes.GMActions:
                    ParseGmActions(message);
                    break;
                case GameServerOpCodes.WorldEntered:
                    ParseWorldEntered(message);

                    if (!IsGameRunning) {
                        IsGameRunning = true;
                        OpenTibiaUnity.GameManager.ProcessGameStart();
                    }

                    break;
                case GameServerOpCodes.LoginError:
                    ParseLoginError(message);
                    break;
                case GameServerOpCodes.LoginAdvice:
                    ParseLoginAdvice(message);
                    break;
                case GameServerOpCodes.LoginWait:
                    ParseLoginWait(message);
                    break;
                case GameServerOpCodes.LoginSuccess:
                    ParseLoginSuccess(message);
                    break;
                case GameServerOpCodes.LoginToken:
                    ParseLoginToken(message);
                    break;
                case GameServerOpCodes.Ping:
                    SendPingBack();
                    break;
                case GameServerOpCodes.PingBack:
                    break;
                case GameServerOpCodes.Challenge:
                    ParseChallange(message);
                    break;
                case GameServerOpCodes.Death:
                    ParseDeath(message);
                    break;
                case GameServerOpCodes.OTClientExtendedOpcode:
                    ParseOtclientExtendedOpcode(message);
                    break;
                case GameServerOpCodes.FullMap:
                    ParseFullMap(message);
                    break;
                case GameServerOpCodes.MapTopRow:
                    ParseMapTopRow(message);
                    break;
                case GameServerOpCodes.MapRightRow:
                    ParseMapRightRow(message);
                    break;
                case GameServerOpCodes.MapBottomRow:
                    ParseMapBottomRow(message);
                    break;
                case GameServerOpCodes.MapLeftRow:
                    ParseMapLeftRow(message);
                    break;
                case GameServerOpCodes.FieldData:
                    ParseFieldData(message);
                    break;
                case GameServerOpCodes.CreateOnMap:
                    ParseCreateOnMap(message);
                    break;
                case GameServerOpCodes.ChangeOnMap:
                    ParseChangeOnMap(message);
                    break;
                case GameServerOpCodes.DeleteOnMap:
                    ParseDeleteOnMap(message);
                    break;
                case GameServerOpCodes.MoveCreature:
                    ParseCreatureMove(message);
                    break;

                case GameServerOpCodes.OpenContainer:
                    ParseOpenContainer(message);
                    break;
                case GameServerOpCodes.CloseContainer:
                    ParseCloseContainer(message);
                    break;
                case GameServerOpCodes.CreateContainer:
                    ParseContainerAddItem(message);
                    break;
                case GameServerOpCodes.ChangeInContainer:
                    ParseContainerUpdateItem(message);
                    break;
                case GameServerOpCodes.DeleteInContainer:
                    ParseContainerRemoveItem(message);
                    break;

                case GameServerOpCodes.SetInventory:
                    ParseSetInventory(message);
                    break;
                case GameServerOpCodes.DeleteInventory:
                    ParseDeleteInventory(message);
                    break;

                case GameServerOpCodes.AmbientLight:
                    ParseAmbientLight(message);
                    break;
                case GameServerOpCodes.GraphicalEffect:
                    ParseGraphicalEffect(message);
                    break;
                case GameServerOpCodes.TextEffect:
                    ParseTextEffect(message);
                    break;
                case GameServerOpCodes.MissleEffect:
                    ParseMissleEffect(message);
                    break;
                case GameServerOpCodes.CreatureMark:
                    ParseCreatureMark(message);
                    break;

                case GameServerOpCodes.CreatureHealth:
                    ParseCreatureHealth(message);
                    break;
                case GameServerOpCodes.CreatureLight:
                    ParseCreatureLight(message);
                    break;
                case GameServerOpCodes.CreatureOutfit:
                    ParseCreatureOutfit(message);
                    break;
                case GameServerOpCodes.CreatureSpeed:
                    ParseCreatureSpeed(message);
                    break;
                case GameServerOpCodes.CreatureSkull:
                    ParseCreatureSkull(message);
                    break;
                case GameServerOpCodes.CreatureShield:
                    ParseCreatureShield(message);
                    break;
                case GameServerOpCodes.CreatureUnpass:
                    ParseCreatureUnpass(message);
                    break;
                case GameServerOpCodes.CreatureMarks:
                    ParseCreatureMarks(message);
                    break;
                case GameServerOpCodes.PlayerHelpers:
                    ParsePlayerHelpers(message);
                    break;
                case GameServerOpCodes.CreatureType:
                    ParseCreatureType(message);
                    break;

                case GameServerOpCodes.PlayerBlessings:
                    ParsePlayerBlessings(message);
                    break;
                case GameServerOpCodes.PlayerBasicData:
                    ParseBasicData(message);
                    break;
                case GameServerOpCodes.PlayerStats:
                    ParsePlayerStats(message);
                    break;
                case GameServerOpCodes.PlayerSkills:
                    ParsePlayerSkills(message);
                    break;
                case GameServerOpCodes.PlayerStates:
                    ParsePlayerStates(message);
                    break;
                case GameServerOpCodes.ClearTarget:
                    ParseClearTarget(message);
                    break;

                case GameServerOpCodes.SetTactics:
                    ParseSetTactics(message);
                    break;

                case GameServerOpCodes.Talk:
                    ParseTalk(message);
                    break;
                case GameServerOpCodes.Channels:
                    ParseChannels(message);
                    break;
                case GameServerOpCodes.OpenChannel:
                    ParseOpenChannel(message);
                    break;
                case GameServerOpCodes.PrivateChannel:
                    ParsePrivateChannel(message);
                    break;

                case GameServerOpCodes.OpenOwnChannel:
                    ParseOpenOwnChannel(message);
                    break;
                case GameServerOpCodes.CloseChannel:
                    ParseCloseChannel(message);
                    break;
                case GameServerOpCodes.TextMessage:
                    ParseTextMessage(message);
                    break;
                case GameServerOpCodes.CancelWalk:
                    ParseCancelWalk(message);
                    break;

                case GameServerOpCodes.TopFloor:
                    ParseMapTopFloor(message);
                    break;
                case GameServerOpCodes.BottomFloor:
                    ParseMapBottomFloor(message);
                    break;

                case GameServerOpCodes.TrackedQuestFlags:
                    ParseTrackedQuestFlags(message);
                    break;

                case GameServerOpCodes.VipAdd:
                    ParseVipAdd(message);
                    break;
                case GameServerOpCodes.VipState:
                    ParseVipState(message);
                    break;
                case GameServerOpCodes.VipLogout:
                    ParseVipLogout(message);
                    break;

                case GameServerOpCodes.PreyFreeListRerollAvailability:
                    ParsePreyFreeListRerollAvailability(message);
                    break;
                case GameServerOpCodes.PreyTimeLeft:
                    ParsePreyTimeLeft(message);
                    break;
                case GameServerOpCodes.PreyData:
                    ParsePreyData(message);
                    break;
                case GameServerOpCodes.PreyRerollPrice:
                    ParsePreyRerollPrice(message);
                    break;

                case GameServerOpCodes.ResourceBalance:
                    ParsePlayerResource(message);
                    break;

                case GameServerOpCodes.ChannelEvent:
                    ParseChannelEvent(message);
                    break;

                case GameServerOpCodes.PlayerInventory:
                    ParsePlayerInventory(message);
                    break;

                default:
                    string err = string.Format("<ProtocolGame> Unknown opcode received ({0}), last opcode ({1}), prev opcode ({2}), unread ({3})", opcode, m_LastOpcode, m_PrevOpcode, message.GetUnreadSize());
                    m_ChatStorage.AddDebugMessage(err);
                    return false;
            }

            m_PrevOpcode = m_LastOpcode;
            m_LastOpcode = opcode;
            return true;
        }

        private void ParseDeath(InputMessage message) {
            DeathType deathType = DeathType.DeathTypeRegular;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameDeathType))
                deathType = (DeathType)message.GetU8();

            int penalty = 100;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePenalityOnDeath) && deathType == DeathType.DeathTypeRegular)
                penalty = message.GetU8();
            
            // TODO death actions...
            //LocalPlayer.OnDeath(deathType, penalty);
        }
    }
}