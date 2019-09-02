using OpenTibiaUnity.Core.Communication.Types;
using System.IO;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Game
{
    using Inflater = Compression.Inflater3;

    public enum ConnectionState
    {
        Disconnected,
        ConnectingStage1,
        ConnectingStage2, // supported if pending is supported //
        Pending,
        Game,
    }

    public partial class ProtocolGame : Internal.Protocol {
        public class ConnectionError : UnityEvent<string, bool> {}
        public class LoginErrorEvent : UnityEvent<string> {}
        public class LoginAdviceEvent : UnityEvent<string> {}
        public class LoginWaitEvent : UnityEvent<string, int> {}
        public class ConnectionStatusEvent : UnityEvent { }

        private static Appearances.AppearanceStorage AppearanceStorage { get { return OpenTibiaUnity.AppearanceStorage; } }
        private static Creatures.CreatureStorage CreatureStorage { get { return OpenTibiaUnity.CreatureStorage; } }
        private static MiniMap.MiniMapStorage MiniMapStorage { get { return OpenTibiaUnity.MiniMapStorage; } }
        private static WorldMap.WorldMapStorage WorldMapStorage { get { return OpenTibiaUnity.WorldMapStorage; } }
        private static Chat.ChatStorage ChatStorage { get { return OpenTibiaUnity.ChatStorage; } }
        private static Chat.MessageStorage MessageStorage { get { return OpenTibiaUnity.MessageStorage; } }
        private static Container.ContainerStorage ContainerStorage { get { return OpenTibiaUnity.ContainerStorage; } }
        private static Creatures.Player Player { get { return OpenTibiaUnity.Player; } }
        
        private ConnectionState _connectionState = ConnectionState.Disconnected;
        private bool _firstReceived = false;
        private bool _connectionWasLost = false;
        
        private System.Diagnostics.Stopwatch _pingTimer = new System.Diagnostics.Stopwatch();
        private int _pingReceived = 0;
        private int _pingSent = 0;
        private int _ping = 0;
        //private Compression.InflateWrapper _inflateWrapper;

        public string WorldName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string SessionKey { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public int Ping { get => _ping; }
        public ConnectionError onConnectionError { get; } = new ConnectionError();
        public LoginErrorEvent onLoginError { get; } = new LoginErrorEvent();
        public LoginAdviceEvent onLoginAdvice { get; } = new LoginAdviceEvent();
        public LoginWaitEvent onLoginWait { get; } = new LoginWaitEvent();
        public ConnectionStatusEvent onConnectionLost { get; } = new ConnectionStatusEvent();
        public ConnectionStatusEvent onConnectionRecovered { get; } = new ConnectionStatusEvent();

        public ConnectionState ConnectionState { get => _connectionState; }

        public bool IsGameRunning {
            get => _connectionState == ConnectionState.Game;
        }

        public bool IsPending {
            get => _connectionState == ConnectionState.Pending;
        }

        public ProtocolGame() : base() {
            SetConnectionState(ConnectionState.Disconnected, false);
        }

        public override void Connect(string address, int port) {
            _pingTimer.Start();

            BuildMessageModesMap(OpenTibiaUnity.GameManager.ClientVersion);
            SetConnectionState(ConnectionState.ConnectingStage1);
            base.Connect(address, port);
        }

        public override void Disconnect(bool dispatch = true) {
            if ((_connectionState == ConnectionState.Game || _connectionState == ConnectionState.Pending) && !dispatch) {
                SendQuitGame();
            } else {
                base.Disconnect();
                SetConnectionState(ConnectionState.Disconnected, dispatch);
            }
        }

        protected override void OnConnectionEstablished() {
            var gameManager = OpenTibiaUnity.GameManager;

            // if proxy is required, send identification to the server
            // and it will do the rest for us
            if (gameManager.GetFeature(GameFeature.GameWorldProxyIdentification)) {
                // send the world name and the server will do the rest
                SendProxyWorldNameIdentification();

                // reset the inflater to clear sync
                //_inflateWrapper = new Compression.InflateWrapper();

                // older clients doesn't wait for challange, so we need to send the
                // login packet from now on.
            } else if (!gameManager.GetFeature(GameFeature.GameChallengeOnLogin)) {
                SendLogin(0, 0);
                SetConnectionState(ConnectionState.ConnectingStage2);
            }

            _connection.Receive();
        }

        protected override void OnConnectionTerminated() {
            base.OnConnectionTerminated();
            UnityAction action = () => {
                if (_connectionState != ConnectionState.Game && _connectionState != ConnectionState.Pending) {
                    switch (_connectionState) {
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
                    _connectionState = ConnectionState.Disconnected;
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
            if (!_firstReceived) {
                _firstReceived = true;

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageSizeCheck)) {
                    int size = _inputBuffer.ReadUnsignedShort();
                    int unread = _inputBuffer.BytesAvailable;

                    if (unread != size) {
                        OnConnectionError(string.Format("ProtocolGame.OnCommunicationDataReady: Invalid message size (size: {0}, unread: {1})", size, unread));
                        return;
                    }
                }
            }

            if (_inputBuffer.BytesAvailable == 0)
                return;

            if (_packetReader.Compressed && !InflateInputBuffer()) {
                OnConnectionError("ProtocolGame.OnCommunicationDataReady: Failed to decompress the message.");
                return;
            }

            GameserverMessageType prevMessageType = 0;
            GameserverMessageType lastMessageType = 0;
            while (_inputBuffer.BytesAvailable > 0) {
                var messageType = _inputBuffer.ReadEnum<GameserverMessageType>();
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
                        _inputBuffer.BytesAvailable,
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
            var oldState = _connectionState;
            _connectionState = connectionState;
            switch (_connectionState) {
                case ConnectionState.Disconnected:
                    if (dispatch) {
                        if (oldState == ConnectionState.Game || oldState == ConnectionState.Pending)
                            OpenTibiaUnity.GameManager.ProcessGameEnd();
                    }
                    break;
                case ConnectionState.ConnectingStage1:
                    _packetReader.XTEA = null;
                    _packetWriter.XTEA = null;
                    break;
                case ConnectionState.ConnectingStage2:
                    _packetReader.XTEA = _xTEA;
                    _packetWriter.XTEA = _xTEA;
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
            int length = _inputBuffer.Length;
            int offset = _inputBuffer.Position;

            var compressedBuffer = new byte[length - offset];
            _inputBuffer.ReadBytes(compressedBuffer, 0, compressedBuffer.Length);
            
            try {
                if (Inflater.Inflate(compressedBuffer, out byte[] uncompressedBuffer)) {
                    _inputBuffer.Position -= compressedBuffer.Length;
                    _inputBuffer.Length += uncompressedBuffer.Length - compressedBuffer.Length;
                    _inputBuffer.WriteBytes(uncompressedBuffer, 0, uncompressedBuffer.Length);
                    _inputBuffer.Position -= uncompressedBuffer.Length;
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
                        ParseLoginSuccess(_inputBuffer);
                    break;
                case GameserverMessageType.GMActions_ReadyForSecondaryConnection:
                    if (gameManager.ClientVersion < 1100)
                        ParseGmActions(_inputBuffer);
                    else
                        ParseReadyForSecondaryConnection(_inputBuffer);
                    break;
                case GameserverMessageType.WorldEntered:
                    ParseWorldEntered(_inputBuffer);
                    break;
                case GameserverMessageType.LoginError:
                    ParseLoginError(_inputBuffer);
                    break;
                case GameserverMessageType.LoginAdvice:
                    ParseLoginAdvice(_inputBuffer);
                    break;
                case GameserverMessageType.LoginWait:
                    ParseLoginWait(_inputBuffer);
                    break;
                case GameserverMessageType.LoginSuccess:
                    ParseLoginSuccess(_inputBuffer);
                    break;
                case GameserverMessageType.LoginToken:
                    ParseLoginToken(_inputBuffer);
                    break;
                case GameserverMessageType.StoreButtonIndicators:
                    ParseStoreButtonIndicators(_inputBuffer);
                    break;
                case GameserverMessageType.Ping:
                case GameserverMessageType.PingBack: {
                    if ((messageType == GameserverMessageType.Ping && gameManager.GetFeature(GameFeature.GameClientPing)) ||
                        (messageType == GameserverMessageType.PingBack && !gameManager.GetFeature(GameFeature.GameClientPing)))
                        ParsePingBack(_inputBuffer);
                    else
                        ParsePing(_inputBuffer);
                    break;
                }
                case GameserverMessageType.Challenge:
                    if (!gameManager.GetFeature(GameFeature.GameChallengeOnLogin))
                        goto default;
                    ParseChallange(_inputBuffer);
                    break;
                    
                case GameserverMessageType.Death:
                    ParseDeath(_inputBuffer);
                    break;
                case GameserverMessageType.OTClientExtendedOpcode:
                    ParseOtclientExtendedOpcode(_inputBuffer);
                    break;
                    
                case GameserverMessageType.ClientCheck:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    ParseClientCheck(_inputBuffer);
                    break;
                case GameserverMessageType.FullMap:
                    ParseFullMap(_inputBuffer);
                    break;
                case GameserverMessageType.MapTopRow:
                    ParseMapTopRow(_inputBuffer);
                    break;
                case GameserverMessageType.MapRightRow:
                    ParseMapRightRow(_inputBuffer);
                    break;
                case GameserverMessageType.MapBottomRow:
                    ParseMapBottomRow(_inputBuffer);
                    break;
                case GameserverMessageType.MapLeftRow:
                    ParseMapLeftRow(_inputBuffer);
                    break;
                case GameserverMessageType.FieldData:
                    ParseFieldData(_inputBuffer);
                    break;
                case GameserverMessageType.CreateOnMap:
                    ParseCreateOnMap(_inputBuffer);
                    break;
                case GameserverMessageType.ChangeOnMap:
                    ParseChangeOnMap(_inputBuffer);
                    break;
                case GameserverMessageType.DeleteOnMap:
                    ParseDeleteOnMap(_inputBuffer);
                    break;
                case GameserverMessageType.MoveCreature:
                    ParseCreatureMove(_inputBuffer);
                    break;

                case GameserverMessageType.OpenContainer:
                    ParseOpenContainer(_inputBuffer);
                    break;
                case GameserverMessageType.CloseContainer:
                    ParseCloseContainer(_inputBuffer);
                    break;
                case GameserverMessageType.CreateInContainer:
                    ParseCreateInContainer(_inputBuffer);
                    break;
                case GameserverMessageType.ChangeInContainer:
                    ParseChangeInContainer(_inputBuffer);
                    break;
                case GameserverMessageType.DeleteInContainer:
                    ParseDeleteInContainer(_inputBuffer);
                    break;

                case GameserverMessageType.SetInventory:
                    ParseSetInventory(_inputBuffer);
                    break;
                case GameserverMessageType.DeleteInventory:
                    ParseDeleteInventory(_inputBuffer);
                    break;
                case GameserverMessageType.NpcOffer:
                    ParseNPCOffer(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerGoods:
                    ParsePlayerGoods(_inputBuffer);
                    break;
                case GameserverMessageType.CloseNpcTrade:
                    ParseCloseNPCTrade(_inputBuffer);
                    break;
                case GameserverMessageType.OwnOffer:
                    ParseOwnOffer(_inputBuffer);
                    break;
                case GameserverMessageType.CounterOffer:
                    ParseCounterOffer(_inputBuffer);
                    break;
                case GameserverMessageType.CloseTrade:
                    ParseCloseTrade(_inputBuffer);
                    break;
                case GameserverMessageType.AmbientLight:
                    ParseAmbientLight(_inputBuffer);
                    break;
                case GameserverMessageType.GraphicalEffect:
                    ParseGraphicalEffect(_inputBuffer);
                    break;
                case GameserverMessageType.TextEffect_RemoveGraphicalEffect:
                    if (gameManager.ClientVersion < 900)
                        ParseTextEffect(_inputBuffer);
                    else if (gameManager.ClientVersion >= 1200)
                        ParseRemoveGraphicalEffect(_inputBuffer);
                    else
                        goto default;
                    break;
                case GameserverMessageType.MissleEffect:
                    ParseMissleEffect(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureMark:
                    ParseCreatureMark(_inputBuffer);
                    break;

                case GameserverMessageType.CreatureHealth:
                    ParseCreatureHealth(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureLight:
                    ParseCreatureLight(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureOutfit:
                    ParseCreatureOutfit(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureSpeed:
                    ParseCreatureSpeed(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureSkull:
                    ParseCreatureSkull(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureShield:
                    ParseCreatureShield(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureUnpass:
                    ParseCreatureUnpass(_inputBuffer);
                    break;
                case GameserverMessageType.CreatureMarks:
                    ParseCreatureMarks(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerHelpers:
                    if (gameManager.ClientVersion < 1185)
                        ParsePlayerHelpers(_inputBuffer);
                    else
                        throw new System.Exception("opcode (PlayerHelpers) changed and must be verified.");
                    break;
                case GameserverMessageType.CreatureType:
                    ParseCreatureType(_inputBuffer);
                    break;
                case GameserverMessageType.GameNews:
                    ParseGameNews(_inputBuffer);
                    break;

                case GameserverMessageType.Blessings:
                    ParseBlessings(_inputBuffer);
                    break;
                case GameserverMessageType.PremiumTrigger:
                    ParsePremiumTrigger(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerBasicData:
                    ParseBasicData(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerStats:
                    ParsePlayerStats(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerSkills:
                    ParsePlayerSkills(_inputBuffer);
                    break;
                case GameserverMessageType.PlayerStates:
                    ParsePlayerStates(_inputBuffer);
                    break;
                case GameserverMessageType.ClearTarget:
                    ParseClearTarget(_inputBuffer);
                    break;

                case GameserverMessageType.SetTactics:
                    ParseSetTactics(_inputBuffer);
                    break;

                case GameserverMessageType.RestingAreaState:
                    ParseRestingAreaState(_inputBuffer);
                    break;
                case GameserverMessageType.Talk:
                    ParseTalk(_inputBuffer);
                    break;
                case GameserverMessageType.Channels:
                    ParseChannels(_inputBuffer);
                    break;
                case GameserverMessageType.OpenChannel:
                    ParseOpenChannel(_inputBuffer);
                    break;
                case GameserverMessageType.PrivateChannel:
                    ParsePrivateChannel(_inputBuffer);
                    break;

                case GameserverMessageType.OpenOwnChannel:
                    ParseOpenOwnChannel(_inputBuffer);
                    break;
                case GameserverMessageType.CloseChannel:
                    ParseCloseChannel(_inputBuffer);
                    break;
                case GameserverMessageType.TextMessage:
                    ParseTextMessage(_inputBuffer);
                    break;
                case GameserverMessageType.CancelWalk:
                    ParseCancelWalk(_inputBuffer);
                    break;
                case GameserverMessageType.Wait:
                    ParseWait(_inputBuffer);
                    break;
                case GameserverMessageType.UnjustifiedPoints:
                    ParseUnjustifiedPoints(_inputBuffer);
                    break;
                case GameserverMessageType.PvpSituations:
                    ParsePvpSituations(_inputBuffer);
                    break;

                case GameserverMessageType.TopFloor:
                    ParseMapTopFloor(_inputBuffer);
                    break;
                case GameserverMessageType.BottomFloor:
                    ParseMapBottomFloor(_inputBuffer);
                    break;
                case GameserverMessageType.UpdateLootContainers:
                    if (!gameManager.GetFeature(GameFeature.GameQuickLoot))
                        goto default;
                    ParseUpdateLootContainers(_inputBuffer);
                    break;

                case GameserverMessageType.OutfitDialog:
                    ParseOutfitDialog(_inputBuffer);
                    break;

                case GameserverMessageType.SupplyStash:
                    if (gameManager.ClientVersion < 1200)
                        goto default;
                    ParseSupplyStash(_inputBuffer);
                    break;

                case GameserverMessageType.MarketStatistics:
                    if (gameManager.ClientVersion < 1140)
                        goto default;
                    ParseMarketStatistics(_inputBuffer);
                    break;

                case GameserverMessageType.TrackedQuestFlags:
                    if (!gameManager.GetFeature(GameFeature.QuestTracker))
                        goto default;
                    ParseTrackedQuestFlags(_inputBuffer);
                    break;

                case GameserverMessageType.BuddyAdd:
                    ParseBuddyAdd(_inputBuffer);
                    break;
                case GameserverMessageType.BuddyState:
                    ParseBuddyState(_inputBuffer);
                    break;
                case GameserverMessageType.BuddyLogout_BuddyGroupData:
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBuddyGroups))
                        ParseBuddyGroupData(_inputBuffer);
                    else
                        ParseBuddyLogout(_inputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopedia:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopedia(_inputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaMonsters:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaMonsters(_inputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaRace:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaRace(_inputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaBonusEffects:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaBonusEffects(_inputBuffer);
                    break;
                case GameserverMessageType.MonsterCyclopediaNewDetails:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopedia))
                        goto default;
                    ParseMonsterCyclopediaNewDetails(_inputBuffer);
                    break;
                case GameserverMessageType.CyclopediaCharacterInfo:
                    if (gameManager.ClientVersion < 1200) // was introduced within tibia 12
                        goto default;
                    ParseCyclopediaCharacterInfo(_inputBuffer);
                    break;
                    
                case GameserverMessageType.TutorialHint:
                    _inputBuffer.ReadUnsignedByte(); // hintId
                    break;
                case GameserverMessageType.AutomapFlag_CyclopediaMapData:
                    if (gameManager.GetFeature(GameFeature.GameCyclopediaMap))
                        ParseCyclopediaMapData(_inputBuffer);
                    else
                        ParseAutomapFlag(_inputBuffer);
                    break;
                case GameserverMessageType.DailyRewardCollectionState:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardCollectionState(_inputBuffer);
                    break;

                case GameserverMessageType.OpenRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseOpenRewardWall(_inputBuffer);
                    break;
                case GameserverMessageType.CloseRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseCloseRewardWall(_inputBuffer);
                    break;
                case GameserverMessageType.DailyRewardBasic:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardBasic(_inputBuffer);
                    break;
                case GameserverMessageType.DailyRewardHistory:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardHistory(_inputBuffer);
                    break;

                case GameserverMessageType.PreyFreeListRerollAvailability:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyFreeListRerollAvailability(_inputBuffer);
                    break;
                case GameserverMessageType.PreyTimeLeft:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyTimeLeft(_inputBuffer);
                    break;
                case GameserverMessageType.PreyData:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyData(_inputBuffer);
                    break;
                case GameserverMessageType.PreyPrices:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyPrices(_inputBuffer);
                    break;

                case GameserverMessageType.CloseImbuingDialog:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseCloseImbuingDialog(_inputBuffer);
                    break;

                case GameserverMessageType.ResourceBalance:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseResourceBalance(_inputBuffer);
                    break;
                case GameserverMessageType.TibiaTime:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    _inputBuffer.ReadUnsignedByte(); // hrs
                    _inputBuffer.ReadUnsignedByte(); // mins
                    break;
                case GameserverMessageType.ChannelEvent:
                    ParseChannelEvent(_inputBuffer);
                    break;

                case GameserverMessageType.PlayerInventory:
                    ParsePlayerInventory(_inputBuffer);
                    break;

                default:
                    throw new System.Exception("unknown message type");
            }
            return true;
        }

        private void MessageProcessingFinished() {
            WorldMapStorage.RefreshFields();
            MiniMapStorage.RefreshSectors();
            CreatureStorage.RefreshOpponents();
        }

        public void OnCheckAlive() {
            if (IsGameRunning || IsPending) {
                // TODO, check for on connection lost :)
            }
        }

        private void ParsePingBack(Internal.ByteArray message) {
            _pingReceived++;
            
            if (_pingReceived == _pingSent)
                _ping = (int)_pingTimer.ElapsedMilliseconds;
            else
                ChatStorage.AddDebugMessage("ProtocolGame.ParsePingBack: Got an invalid ping from server");

            OpenTibiaUnity.GameManager.ShouldSendPingAt = OpenTibiaUnity.TicksMillis + Constants.PingDelay;
        }

        private void ParsePing(Internal.ByteArray message) {
            // onPing.Invoke();
            InternalSendPingBack();
        }

        public void SendPing() {
            if (_pingReceived != _pingSent)
                return;

            InternalSendPing();
            _pingSent++;
            _pingTimer.Restart();
        }
    }
}
