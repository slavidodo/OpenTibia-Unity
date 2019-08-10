using OpenTibiaUnity.Core.Communication.Types;
using System.IO;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Game
{
    using Inflater = Compression.Inflater3;

    internal enum ConnectionState
    {
        Disconnected,
        ConnectingStage1,
        ConnectingStage2, // supported if pending is supported //
        Pending,
        Game,
    }

    internal partial class ProtocolGame : Internal.Protocol {
        internal class ConnectionError : UnityEvent<string, bool> {}
        internal class LoginErrorEvent : UnityEvent<string> {}
        internal class LoginAdviceEvent : UnityEvent<string> {}
        internal class LoginWaitEvent : UnityEvent<string, int> {}
        internal class ConnectionStatusEvent : UnityEvent { }

        private static Appearances.AppearanceStorage AppearanceStorage { get { return OpenTibiaUnity.AppearanceStorage; } }
        private static Creatures.CreatureStorage CreatureStorage { get { return OpenTibiaUnity.CreatureStorage; } }
        private static MiniMap.MiniMapStorage MiniMapStorage { get { return OpenTibiaUnity.MiniMapStorage; } }
        private static WorldMap.WorldMapStorage WorldMapStorage { get { return OpenTibiaUnity.WorldMapStorage; } }
        private static Chat.ChatStorage ChatStorage { get { return OpenTibiaUnity.ChatStorage; } }
        private static Chat.MessageStorage MessageStorage { get { return OpenTibiaUnity.MessageStorage; } }
        private static Container.ContainerStorage ContainerStorage { get { return OpenTibiaUnity.ContainerStorage; } }
        private static Creatures.Player Player { get { return OpenTibiaUnity.Player; } }
        
        private ConnectionState m_ConnectionState = ConnectionState.Disconnected;
        private bool m_FirstReceived = false;
        private bool m_ConnectionWasLost = false;
        
        private System.Diagnostics.Stopwatch m_PingTimer = new System.Diagnostics.Stopwatch();
        private int m_PingReceived = 0;
        private int m_PingSent = 0;
        private int m_Ping = 0;
        //private Compression.InflateWrapper m_InflateWrapper;

        internal string WorldName { get; set; } = string.Empty;
        internal string AccountName { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        internal string Token { get; set; } = string.Empty;
        internal string SessionKey { get; set; } = string.Empty;
        internal string CharacterName { get; set; } = string.Empty;
        internal int Ping { get => m_Ping; }
        internal ConnectionError onConnectionError { get; } = new ConnectionError();
        internal LoginErrorEvent onLoginError { get; } = new LoginErrorEvent();
        internal LoginAdviceEvent onLoginAdvice { get; } = new LoginAdviceEvent();
        internal LoginWaitEvent onLoginWait { get; } = new LoginWaitEvent();
        internal ConnectionStatusEvent onConnectionLost { get; } = new ConnectionStatusEvent();
        internal ConnectionStatusEvent onConnectionRecovered { get; } = new ConnectionStatusEvent();

        internal ConnectionState ConnectionState { get => m_ConnectionState; }

        internal bool IsGameRunning {
            get => m_ConnectionState == ConnectionState.Game;
        }

        internal bool IsPending {
            get => m_ConnectionState == ConnectionState.Pending;
        }

        internal ProtocolGame() : base() {
            SetConnectionState(ConnectionState.Disconnected, false);
        }

        internal override void Connect(string address, int port) {
            m_PingTimer.Start();

            BuildMessageModesMap(OpenTibiaUnity.GameManager.ClientVersion);
            SetConnectionState(ConnectionState.ConnectingStage1);
            base.Connect(address, port);
        }

        internal override void Disconnect(bool dispatch = true) {
            if ((m_ConnectionState == ConnectionState.Game || m_ConnectionState == ConnectionState.Pending) && !dispatch) {
                SendQuitGame();
            } else {
                base.Disconnect();
                SetConnectionState(ConnectionState.Disconnected, dispatch);
            }
        }

        protected override void OnConnectionEstablished() {
            var gameManager = OpenTibiaUnity.GameManager;

            try {
                // if proxy world is required, send identification to the server
                // and it will do the rest for us
                if (gameManager.GetFeature(GameFeature.GameWorldProxyIdentification)) {
                    // send the world name and the server will do the rest
                    SendProxyWorldNameIdentification();

                    // reset the inflater to clear sync
                    //m_InflateWrapper = new Compression.InflateWrapper();

                    // older clients doesn't wait for challange, so we need to send the
                    // login packet from now on.
                } else if (!gameManager.GetFeature(GameFeature.GameChallengeOnLogin)) {
                    SendLogin(0, 0);
                    SetConnectionState(ConnectionState.ConnectingStage2);
                }

                m_Connection.Receive();
            } catch (System.Exception e) {
            }
        }

        protected override void OnConnectionTerminated() {
            base.OnConnectionTerminated();
            UnityAction action = () => {
                if (m_ConnectionState != ConnectionState.Game && m_ConnectionState != ConnectionState.Pending) {
                    switch (m_ConnectionState) {
                        case ConnectionState.Disconnected:
                            OnConnectionError("Could not initialize a connection to the game server. Please try again later or contact customer support if the problem persists.", true);
                            break;
                        case ConnectionState.ConnectingStage1:
                            OnConnectionError("Could not connect to the game server. Please try again later.", true);
                            break;
                        case ConnectionState.ConnectingStage2:
                            OnConnectionError("Lost connection to the game server. Please close the client and try again.", true);
                            break;
                    }
                } else {
                    m_ConnectionState = ConnectionState.Disconnected;
                    OpenTibiaUnity.GameManager.ProcessGameEnd();
                }
            };

            if (System.Threading.Thread.CurrentThread == OpenTibiaUnity.MainThread) {
                action.Invoke();
            } else {
                OpenTibiaUnity.GameManager.InvokeOnMainThread(action);
            }
        }
        
        protected override void OnConnectionError(string message, bool disconnecting = false) {
            base.OnConnectionError(message, disconnecting);
            onConnectionError.Invoke(message, disconnecting);
        }

        protected override void OnConnectionSocketError(System.Net.Sockets.SocketError e, string message) {
            OnConnectionError(message, true);
        }

        protected override void OnCommunicationDataReady() {
            if (!m_FirstReceived) {
                m_FirstReceived = true;

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageSizeCheck)) {
                    int size = m_InputBuffer.ReadUnsignedShort();
                    int unread = m_InputBuffer.BytesAvailable;

                    if (unread != size) {
                        OnConnectionError(string.Format("ProtocolGame.OnCommunicationDataReady: Invalid message size (size: {0}, unread: {1})", size, unread));
                        return;
                    }
                }
            }

            if (m_InputBuffer.BytesAvailable == 0)
                return;

            if (m_PacketReader.Compressed && !InflateInputBuffer()) {
                OnConnectionError("ProtocolGame.OnCommunicationDataReady: Failed to decompress the message.");
                return;
            }

            GameserverMessageType prevMessageType = 0;
            GameserverMessageType lastMessageType = 0;
            while (m_InputBuffer.BytesAvailable > 0) {
                var messageType = m_InputBuffer.ReadEnum<GameserverMessageType>();
                if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPending)) {
                    if (!IsGameRunning && messageType > GameserverMessageType.GameFirstMessageType) {
                        MiniMapStorage.Position.Set(0, 0, 0);
                        WorldMapStorage.Position.Set(0, 0, 0);
                        WorldMapStorage.ResetMap();
                        CreatureStorage.Reset();
                        WorldMapStorage.Valid = false;

                        SetConnectionState(ConnectionState.Game);
                    }
                }

                try {
                    CheckPing(messageType);
                    ParseMessage(messageType);

                    prevMessageType = lastMessageType;
                    lastMessageType = messageType;
                } catch (System.Exception e) {
#if DEBUG
                    var err = string.Format("ProtocolGame.ParsePacket: error: {0}, type: ({1}), last type ({2}), prev type ({2}), unread ({4}), StackTrace: \n{5}.",
                        e.Message,
                        messageType,
                        lastMessageType,
                        prevMessageType,
                        m_InputBuffer.BytesAvailable,
                        e.StackTrace);
#else
                    var err = "Invalid state of the protocol. Please contact support.";
                    // TODO, allow sending crash reports
#endif
                    
                    // tolerating map issues will result in undefined behaviour of the gameplay
                    bool forceDisconnect = false;
                    switch (messageType) {
                        case GameserverMessageType.FullMap:
                        case GameserverMessageType.MapTopRow:
                        case GameserverMessageType.MapRightRow:
                        case GameserverMessageType.MapBottomRow:
                        case GameserverMessageType.MapLeftRow:
                        case GameserverMessageType.FieldData:
                        case GameserverMessageType.CreateOnMap:
                        case GameserverMessageType.ChangeOnMap:
                        case GameserverMessageType.DeleteOnMap:
                        case GameserverMessageType.MoveCreature:
                            forceDisconnect = true;
                            break;
                    }

                    OnConnectionError(err, forceDisconnect);
                    if (forceDisconnect) {
                        Disconnect();
                        return;
                    }

                    break;
                }
            }

            MessageProcessingFinished();
        }
        
        private void SetConnectionState(ConnectionState connectionState, bool dispatch = true) {
            var oldState = m_ConnectionState;
            m_ConnectionState = connectionState;
            switch (m_ConnectionState) {
                case ConnectionState.Disconnected:
                    if (dispatch) {
                        if (oldState == ConnectionState.Game || oldState == ConnectionState.Pending)
                            OpenTibiaUnity.GameManager.ProcessGameEnd();
                    }
                    break;
                case ConnectionState.ConnectingStage1:
                    m_PacketReader.XTEA = null;
                    m_PacketWriter.XTEA = null;
                    break;
                case ConnectionState.ConnectingStage2:
                    m_PacketReader.XTEA = m_XTEA;
                    m_PacketWriter.XTEA = m_XTEA;
                    break;
                case ConnectionState.Pending:
                    if (dispatch) {
                        if (oldState != ConnectionState.Pending)
                            OpenTibiaUnity.GameManager.ProcessGamePending();
                    }
                    break;
                case ConnectionState.Game:
                    if (dispatch) {
                        if (oldState != ConnectionState.Game)
                            OpenTibiaUnity.GameManager.ProcessGameStart();
                    }
                    break;
            }
        }



        private bool InflateInputBuffer() {
            int length = m_InputBuffer.Length;
            int offset = m_InputBuffer.Position;

            var compressedBuffer = new byte[length - offset];
            m_InputBuffer.ReadBytes(compressedBuffer, 0, compressedBuffer.Length);
            
            try {
                if (Inflater.Inflate(compressedBuffer, out byte[] uncompressedBuffer)) {
                    m_InputBuffer.Position -= compressedBuffer.Length;
                    m_InputBuffer.Length += uncompressedBuffer.Length - compressedBuffer.Length;
                    m_InputBuffer.WriteBytes(uncompressedBuffer, 0, uncompressedBuffer.Length);
                    m_InputBuffer.Position -= uncompressedBuffer.Length;
                    return true;
                }
            } catch (System.Exception e) {
                UnityEngine.Debug.Log("Message: " + e);
            }
            
            return false;
        }
        
        private void CheckPing(GameserverMessageType type) {
            
        }

        private bool ParseMessage(GameserverMessageType messageType) {
            var gameManager = OpenTibiaUnity.GameManager;
            switch (messageType) {
                case GameserverMessageType.Login_PendingState:
                    if (gameManager.GetFeature(GameFeature.GameLoginPending))
                        SetConnectionState(ConnectionState.Pending);
                    else
                        ParseLoginSuccess(m_InputBuffer);
                    break;
                case GameserverMessageType.GMActions_ReadyForSecondaryConnection:
                    if (gameManager.ClientVersion < 1100)
                        ParseGmActions(m_InputBuffer);
                    else
                        ParseReadyForSecondaryConnection(m_InputBuffer);
                    break;
                case GameserverMessageType.WorldEntered:
                    ParseWorldEntered(m_InputBuffer);
                    break;
                case GameserverMessageType.LoginError:
                    ParseLoginError(m_InputBuffer);
                    break;
                case GameserverMessageType.LoginAdvice:
                    ParseLoginAdvice(m_InputBuffer);
                    break;
                case GameserverMessageType.LoginWait:
                    ParseLoginWait(m_InputBuffer);
                    break;
                case GameserverMessageType.LoginSuccess:
                    ParseLoginSuccess(m_InputBuffer);
                    break;
                case GameserverMessageType.LoginToken:
                    ParseLoginToken(m_InputBuffer);
                    break;
                case GameserverMessageType.StoreButtonIndicators:
                    ParseStoreButtonIndicators(m_InputBuffer);
                    break;
                case GameserverMessageType.Ping:
                case GameserverMessageType.PingBack: {
                    if ((messageType == GameserverMessageType.Ping && gameManager.GetFeature(GameFeature.GameClientPing)) ||
                        (messageType == GameserverMessageType.PingBack && !gameManager.GetFeature(GameFeature.GameClientPing)))
                        ParsePingBack(m_InputBuffer);
                    else
                        ParsePing(m_InputBuffer);
                    break;
                }
                case GameserverMessageType.Challenge:
                    if (!gameManager.GetFeature(GameFeature.GameChallengeOnLogin))
                        goto default;
                    ParseChallange(m_InputBuffer);
                    break;
                    
                case GameserverMessageType.Death:
                    ParseDeath(m_InputBuffer);
                    break;
                case GameserverMessageType.OTClientExtendedOpcode:
                    ParseOtclientExtendedOpcode(m_InputBuffer);
                    break;
                    
                case GameserverMessageType.ClientCheck:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    ParseClientCheck(m_InputBuffer);
                    break;
                case GameserverMessageType.FullMap:
                    ParseFullMap(m_InputBuffer);
                    break;
                case GameserverMessageType.MapTopRow:
                    ParseMapTopRow(m_InputBuffer);
                    break;
                case GameserverMessageType.MapRightRow:
                    ParseMapRightRow(m_InputBuffer);
                    break;
                case GameserverMessageType.MapBottomRow:
                    ParseMapBottomRow(m_InputBuffer);
                    break;
                case GameserverMessageType.MapLeftRow:
                    ParseMapLeftRow(m_InputBuffer);
                    break;
                case GameserverMessageType.FieldData:
                    ParseFieldData(m_InputBuffer);
                    break;
                case GameserverMessageType.CreateOnMap:
                    ParseCreateOnMap(m_InputBuffer);
                    break;
                case GameserverMessageType.ChangeOnMap:
                    ParseChangeOnMap(m_InputBuffer);
                    break;
                case GameserverMessageType.DeleteOnMap:
                    ParseDeleteOnMap(m_InputBuffer);
                    break;
                case GameserverMessageType.MoveCreature:
                    ParseCreatureMove(m_InputBuffer);
                    break;

                case GameserverMessageType.OpenContainer:
                    ParseOpenContainer(m_InputBuffer);
                    break;
                case GameserverMessageType.CloseContainer:
                    ParseCloseContainer(m_InputBuffer);
                    break;
                case GameserverMessageType.CreateInContainer:
                    ParseCreateInContainer(m_InputBuffer);
                    break;
                case GameserverMessageType.ChangeInContainer:
                    ParseChangeInContainer(m_InputBuffer);
                    break;
                case GameserverMessageType.DeleteInContainer:
                    ParseDeleteInContainer(m_InputBuffer);
                    break;

                case GameserverMessageType.SetInventory:
                    ParseSetInventory(m_InputBuffer);
                    break;
                case GameserverMessageType.DeleteInventory:
                    ParseDeleteInventory(m_InputBuffer);
                    break;
                case GameserverMessageType.NpcOffer:
                    ParseNPCOffer(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerGoods:
                    ParsePlayerGoods(m_InputBuffer);
                    break;
                case GameserverMessageType.CloseNpcTrade:
                    ParseCloseNPCTrade(m_InputBuffer);
                    break;
                case GameserverMessageType.OwnOffer:
                    ParseOwnOffer(m_InputBuffer);
                    break;
                case GameserverMessageType.CounterOffer:
                    ParseCounterOffer(m_InputBuffer);
                    break;
                case GameserverMessageType.CloseTrade:
                    ParseCloseTrade(m_InputBuffer);
                    break;
                case GameserverMessageType.AmbientLight:
                    ParseAmbientLight(m_InputBuffer);
                    break;
                case GameserverMessageType.GraphicalEffect:
                    ParseGraphicalEffect(m_InputBuffer);
                    break;
                case GameserverMessageType.TextEffect_RemoveGraphicalEffect:
                    if (gameManager.ClientVersion < 900)
                        ParseTextEffect(m_InputBuffer);
                    else if (gameManager.ClientVersion >= 1200)
                        ParseRemoveGraphicalEffect(m_InputBuffer);
                    else
                        goto default;
                    break;
                case GameserverMessageType.MissleEffect:
                    ParseMissleEffect(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureMark:
                    ParseCreatureMark(m_InputBuffer);
                    break;

                case GameserverMessageType.CreatureHealth:
                    ParseCreatureHealth(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureLight:
                    ParseCreatureLight(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureOutfit:
                    ParseCreatureOutfit(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureSpeed:
                    ParseCreatureSpeed(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureSkull:
                    ParseCreatureSkull(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureShield:
                    ParseCreatureShield(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureUnpass:
                    ParseCreatureUnpass(m_InputBuffer);
                    break;
                case GameserverMessageType.CreatureMarks:
                    ParseCreatureMarks(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerHelpers:
                    if (gameManager.ClientVersion < 1185)
                        ParsePlayerHelpers(m_InputBuffer);
                    else
                        throw new System.Exception("opcode (PlayerHelpers) changed and must be verified.");
                    break;
                case GameserverMessageType.CreatureType:
                    ParseCreatureType(m_InputBuffer);
                    break;
                case GameserverMessageType.GameNews:
                    ParseGameNews(m_InputBuffer);
                    break;

                case GameserverMessageType.Blessings:
                    ParseBlessings(m_InputBuffer);
                    break;
                case GameserverMessageType.PremiumTrigger:
                    ParsePremiumTrigger(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerBasicData:
                    ParseBasicData(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerStats:
                    ParsePlayerStats(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerSkills:
                    ParsePlayerSkills(m_InputBuffer);
                    break;
                case GameserverMessageType.PlayerStates:
                    ParsePlayerStates(m_InputBuffer);
                    break;
                case GameserverMessageType.ClearTarget:
                    ParseClearTarget(m_InputBuffer);
                    break;

                case GameserverMessageType.SetTactics:
                    ParseSetTactics(m_InputBuffer);
                    break;

                case GameserverMessageType.RestingAreaState:
                    ParseRestingAreaState(m_InputBuffer);
                    break;
                case GameserverMessageType.Talk:
                    ParseTalk(m_InputBuffer);
                    break;
                case GameserverMessageType.Channels:
                    ParseChannels(m_InputBuffer);
                    break;
                case GameserverMessageType.OpenChannel:
                    ParseOpenChannel(m_InputBuffer);
                    break;
                case GameserverMessageType.PrivateChannel:
                    ParsePrivateChannel(m_InputBuffer);
                    break;

                case GameserverMessageType.OpenOwnChannel:
                    ParseOpenOwnChannel(m_InputBuffer);
                    break;
                case GameserverMessageType.CloseChannel:
                    ParseCloseChannel(m_InputBuffer);
                    break;
                case GameserverMessageType.TextMessage:
                    ParseTextMessage(m_InputBuffer);
                    break;
                case GameserverMessageType.CancelWalk:
                    ParseCancelWalk(m_InputBuffer);
                    break;
                case GameserverMessageType.Wait:
                    ParseWait(m_InputBuffer);
                    break;
                case GameserverMessageType.UnjustifiedPoints:
                    ParseUnjustifiedPoints(m_InputBuffer);
                    break;
                case GameserverMessageType.PvpSituations:
                    ParsePvpSituations(m_InputBuffer);
                    break;

                case GameserverMessageType.TopFloor:
                    ParseMapTopFloor(m_InputBuffer);
                    break;
                case GameserverMessageType.BottomFloor:
                    ParseMapBottomFloor(m_InputBuffer);
                    break;
                case GameserverMessageType.UpdateLootContainers:
                    if (!gameManager.GetFeature(GameFeature.GameQuickLoot))
                        goto default;
                    ParseUpdateLootContainers(m_InputBuffer);
                    break;

                case GameserverMessageType.OutfitDialog:
                    ParseOutfitDialog(m_InputBuffer);
                    break;

                case GameserverMessageType.SupplyStash:
                    if (gameManager.ClientVersion < 1200)
                        goto default;
                    ParseSupplyStash(m_InputBuffer);
                    break;

                case GameserverMessageType.MarketStatistics:
                    if (gameManager.ClientVersion < 1140)
                        goto default;
                    ParseMarketStatistics(m_InputBuffer);
                    break;

                case GameserverMessageType.TrackedQuestFlags:
                    if (!gameManager.GetFeature(GameFeature.QuestTracker))
                        goto default;
                    ParseTrackedQuestFlags(m_InputBuffer);
                    break;

                case GameserverMessageType.BuddyAdd:
                    ParseBuddyAdd(m_InputBuffer);
                    break;
                case GameserverMessageType.BuddyState:
                    ParseBuddyState(m_InputBuffer);
                    break;
                case GameserverMessageType.BuddyLogout_BuddyGroupData:
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBuddyGroups))
                        ParseBuddyGroupData(m_InputBuffer);
                    else
                        ParseBuddyLogout(m_InputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopedia:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopedia(m_InputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaMonsters:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaMonsters(m_InputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaRace:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaRace(m_InputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaBonusEffects:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaBonusEffects(m_InputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaNewDetails:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaNewDetails(m_InputBuffer);
                    break;
                case GameserverMessageType.CyclopediaCharacterInfo:
                    if (gameManager.ClientVersion < 1200) // was introduced within tibia 12
                        goto default;
                    ParseCyclopediaCharacterInfo(m_InputBuffer);
                    break;
                    
                case GameserverMessageType.TutorialHint:
                    m_InputBuffer.ReadUnsignedByte(); // hintID
                    break;
                case GameserverMessageType.AutomapFlag_CyclopediaMapData:
                    if (gameManager.GetFeature(GameFeature.GameCyclopediaMap))
                        ParseCyclopediaMapData(m_InputBuffer);
                    else
                        ParseAutomapFlag(m_InputBuffer);
                    break;
                case GameserverMessageType.DailyRewardCollectionState:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardCollectionState(m_InputBuffer);
                    break;

                case GameserverMessageType.OpenRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseOpenRewardWall(m_InputBuffer);
                    break;
                case GameserverMessageType.CloseRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseCloseRewardWall(m_InputBuffer);
                    break;
                case GameserverMessageType.DailyRewardBasic:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardBasic(m_InputBuffer);
                    break;
                case GameserverMessageType.DailyRewardHistory:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardHistory(m_InputBuffer);
                    break;

                case GameserverMessageType.PreyFreeListRerollAvailability:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyFreeListRerollAvailability(m_InputBuffer);
                    break;
                case GameserverMessageType.PreyTimeLeft:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyTimeLeft(m_InputBuffer);
                    break;
                case GameserverMessageType.PreyData:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyData(m_InputBuffer);
                    break;
                case GameserverMessageType.PreyPrices:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyPrices(m_InputBuffer);
                    break;

                case GameserverMessageType.CloseImbuingDialog:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseCloseImbuingDialog(m_InputBuffer);
                    break;

                case GameserverMessageType.ResourceBalance:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseResourceBalance(m_InputBuffer);
                    break;
                case GameserverMessageType.TibiaTime:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    m_InputBuffer.ReadUnsignedByte(); // hrs
                    m_InputBuffer.ReadUnsignedByte(); // mins
                    break;
                case GameserverMessageType.ChannelEvent:
                    ParseChannelEvent(m_InputBuffer);
                    break;

                case GameserverMessageType.PlayerInventory:
                    ParsePlayerInventory(m_InputBuffer);
                    break;

                default:
                    throw new System.Exception("unknown message type");
            }
            return true;
        }

        private void MessageProcessingFinished(bool miniMap = true) {
            WorldMapStorage.RefreshFields();
            if (miniMap)
                MiniMapStorage.RefreshSectors();
            CreatureStorage.RefreshOpponents();
        }

        internal void OnCheckAlive() {
            if (IsGameRunning || IsPending) {
                // TODO, check for on connection lost :)
            }
        }

        private void ParsePingBack(Internal.ByteArray message) {
            m_PingReceived++;

            if (m_PingReceived == m_PingSent)
                m_Ping = (int)m_PingTimer.ElapsedMilliseconds;
            else
                ChatStorage.AddDebugMessage("ProtocolGame.ParsePingBack: Got an invalid ping from server");

            OpenTibiaUnity.GameManager.ShouldSendPingAt = OpenTibiaUnity.TicksMillis + Constants.PingDelay;
        }

        private void ParsePing(Internal.ByteArray message) {
            // onPing.Invoke();
            InternalSendPingBack();
        }

        internal void SendPing() {
            if (m_PingReceived != m_PingSent)
                return;

            InternalSendPing();
            m_PingSent++;
            m_PingTimer.Restart();
        }
    }
}
