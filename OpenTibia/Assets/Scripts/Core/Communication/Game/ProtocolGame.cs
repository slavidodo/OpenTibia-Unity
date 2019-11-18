using OpenTibiaUnity.Core.Communication.Types;
using System.IO;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Game
{
    using Inflater = Compression.Inflater;

    public enum ConnectionState
    {
        Disconnected,
        ConnectingStage1,
        ConnectingStage2, // supported if pending is supported //
        Pending,
        Game,
    }

    public partial class ProtocolGame : Internal.Protocol
    {
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

            Inflater.Cleanup();

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

            if (System.Threading.Thread.CurrentThread == OpenTibiaUnity.MainThread)
                action.Invoke();
            else
                OpenTibiaUnity.GameManager.InvokeOnMainThread(action);
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
                    int size = _inputStream.ReadUnsignedShort();
                    int unread = _inputStream.BytesAvailable;

                    if (unread != size) {
                        OnConnectionError(string.Format("ProtocolGame.OnCommunicationDataReady: Invalid message size (size: {0}, unread: {1})", size, unread));
                        return;
                    }
                }
            }

            if (_inputStream.BytesAvailable == 0)
                return;

            if (_packetReader.Compressed && !InflateInputBuffer()) {
                OnConnectionError("ProtocolGame.OnCommunicationDataReady: Failed to decompress the message.");
                return;
            }

            GameserverMessageType curMessageType = 0;
            GameserverMessageType lastMessageType = 0;
            GameserverMessageType prevMessageType = 0;
            while (_inputStream.BytesAvailable > 0) {
                try {
                    curMessageType = 0; // reset for debugging purposes
                    curMessageType = _inputStream.ReadEnum<GameserverMessageType>();
                    if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPending)) {
                        if (!IsGameRunning && curMessageType > GameserverMessageType.GameFirstMessageType) {
                            MiniMapStorage.Position.Set(0, 0, 0);
                            WorldMapStorage.Position.Set(0, 0, 0);
                            WorldMapStorage.ResetMap();
                            CreatureStorage.Reset();
                            WorldMapStorage.Valid = false;
                            SetConnectionState(ConnectionState.Game);
                        }
                    }

                    CheckPing(curMessageType);
                    ParseMessage(curMessageType);

                    prevMessageType = lastMessageType;
                    lastMessageType = curMessageType;
                } catch (System.Exception e) {
#if DEBUG
                    var err = string.Format("ProtocolGame.ParsePacket: error: {0}, type: (<color=white>{1}</color>), " +
                        "last type (<color=green>{2}</color>), prev type (<color=purple>{2}</color>), unread (<color=red>{4}</color>), compressed(<color=yellow>{5}</color>) StackTrace: \n{6}.",
                        e.Message,
                        curMessageType,
                        lastMessageType,
                        prevMessageType,
                        _inputStream.BytesAvailable,
                        _packetReader.Compressed ? "yes" : "no",
                        e.StackTrace);
#else
                    var err = "Invalid state of the protocol. Please contact support.";
                    // TODO, allow sending crash reports
#endif
                    
                    // tolerating map issues will result in undefined behaviour of the gameplay
                    bool forceDisconnect = false;
                    switch (curMessageType) {
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
            var compressedBuffer = new byte[_inputStream.BytesAvailable];
            _inputStream.Read(compressedBuffer, 0, compressedBuffer.Length);
            _inputStream.Position -= compressedBuffer.Length;

            try {
                if (Inflater.Inflate(compressedBuffer, out byte[] uncompressedBuffer)) {
                    _inputStream.Write(uncompressedBuffer, 0, uncompressedBuffer.Length);
                    _inputStream.Position -= uncompressedBuffer.Length;
                    return true;
                }
            } catch (System.Exception) {}
            
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
                        ParseLoginSuccess(_inputStream);
                    break;
                case GameserverMessageType.GMActions_ReadyForSecondaryConnection:
                    if (gameManager.ClientVersion < 1100)
                        ParseGmActions(_inputStream);
                    else
                        ParseReadyForSecondaryConnection(_inputStream);
                    break;
                case GameserverMessageType.WorldEntered:
                    ParseWorldEntered(_inputStream);
                    break;
                case GameserverMessageType.LoginError:
                    ParseLoginError(_inputStream);
                    break;
                case GameserverMessageType.LoginAdvice:
                    ParseLoginAdvice(_inputStream);
                    break;
                case GameserverMessageType.LoginWait:
                    ParseLoginWait(_inputStream);
                    break;
                case GameserverMessageType.LoginSuccess:
                    ParseLoginSuccess(_inputStream);
                    break;
                case GameserverMessageType.LoginToken:
                    ParseLoginToken(_inputStream);
                    break;
                case GameserverMessageType.StoreButtonIndicators:
                    ParseStoreButtonIndicators(_inputStream);
                    break;
                case GameserverMessageType.Ping:
                case GameserverMessageType.PingBack: {
                    if ((messageType == GameserverMessageType.Ping && gameManager.GetFeature(GameFeature.GameClientPing)) ||
                        (messageType == GameserverMessageType.PingBack && !gameManager.GetFeature(GameFeature.GameClientPing)))
                        ParsePingBack(_inputStream);
                    else
                        ParsePing(_inputStream);
                    break;
                }
                case GameserverMessageType.Challenge:
                    if (!gameManager.GetFeature(GameFeature.GameChallengeOnLogin))
                        goto default;
                    ParseChallange(_inputStream);
                    break;
                case GameserverMessageType.Death:
                    ParseDeath(_inputStream);
                    break;
                case GameserverMessageType.Stash:
                    if (gameManager.ClientVersion < 1200)
                        goto default;
                    ParseSupplyStash(_inputStream);
                    break;

                case GameserverMessageType.OTClientExtendedOpcode:
                    ParseOtclientExtendedOpcode(_inputStream);
                    break;
                    
                case GameserverMessageType.ClientCheck:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    ParseClientCheck(_inputStream);
                    break;
                case GameserverMessageType.FullMap:
                    ParseFullMap(_inputStream);
                    break;
                case GameserverMessageType.MapTopRow:
                    ParseMapTopRow(_inputStream);
                    break;
                case GameserverMessageType.MapRightRow:
                    ParseMapRightRow(_inputStream);
                    break;
                case GameserverMessageType.MapBottomRow:
                    ParseMapBottomRow(_inputStream);
                    break;
                case GameserverMessageType.MapLeftRow:
                    ParseMapLeftRow(_inputStream);
                    break;
                case GameserverMessageType.FieldData:
                    ParseFieldData(_inputStream);
                    break;
                case GameserverMessageType.CreateOnMap:
                    ParseCreateOnMap(_inputStream);
                    break;
                case GameserverMessageType.ChangeOnMap:
                    ParseChangeOnMap(_inputStream);
                    break;
                case GameserverMessageType.DeleteOnMap:
                    ParseDeleteOnMap(_inputStream);
                    break;
                case GameserverMessageType.MoveCreature:
                    ParseCreatureMove(_inputStream);
                    break;

                case GameserverMessageType.OpenContainer:
                    ParseOpenContainer(_inputStream);
                    break;
                case GameserverMessageType.CloseContainer:
                    ParseCloseContainer(_inputStream);
                    break;
                case GameserverMessageType.CreateInContainer:
                    ParseCreateInContainer(_inputStream);
                    break;
                case GameserverMessageType.ChangeInContainer:
                    ParseChangeInContainer(_inputStream);
                    break;
                case GameserverMessageType.DeleteInContainer:
                    ParseDeleteInContainer(_inputStream);
                    break;

                case GameserverMessageType.InspectionList:
                    if (!gameManager.GetFeature(GameFeature.GameInspectionWindow))
                        goto default;
                    ParseInspectionList(_inputStream);
                    break;
                case GameserverMessageType.InspectionState:
                    if (!gameManager.GetFeature(GameFeature.GameInspectionWindow))
                        goto default;
                    ParseInspectionState(_inputStream);
                    break;
                case GameserverMessageType.SetInventory:
                    ParseSetInventory(_inputStream);
                    break;
                case GameserverMessageType.DeleteInventory:
                    ParseDeleteInventory(_inputStream);
                    break;
                case GameserverMessageType.NpcOffer:
                    ParseNPCOffer(_inputStream);
                    break;
                case GameserverMessageType.PlayerGoods:
                    ParsePlayerGoods(_inputStream);
                    break;
                case GameserverMessageType.CloseNpcTrade:
                    ParseCloseNPCTrade(_inputStream);
                    break;
                case GameserverMessageType.OwnOffer:
                    ParseOwnOffer(_inputStream);
                    break;
                case GameserverMessageType.CounterOffer:
                    ParseCounterOffer(_inputStream);
                    break;
                case GameserverMessageType.CloseTrade:
                    ParseCloseTrade(_inputStream);
                    break;
                case GameserverMessageType.AmbientLight:
                    ParseAmbientLight(_inputStream);
                    break;
                case GameserverMessageType.GraphicalEffect:
                    if (OpenTibiaUnity.GameManager.ClientVersion >= 1203)
                        ParseGraphicalEffects(_inputStream);
                    else
                        ParseGraphicalEffect(_inputStream);
                    break;
                case GameserverMessageType.TextEffect_RemoveGraphicalEffect:
                    if (gameManager.ClientVersion < 900)
                        ParseTextEffect(_inputStream);
                    else if (gameManager.ClientVersion >= 1200)
                        ParseRemoveGraphicalEffect(_inputStream);
                    else
                        goto default;
                    break;
                case GameserverMessageType.MissleEffect:
                    ParseMissleEffect(_inputStream);
                    break;
                case GameserverMessageType.CreatureMark:
                    ParseCreatureMark(_inputStream);
                    break;

                case GameserverMessageType.CreatureHealth:
                    ParseCreatureHealth(_inputStream);
                    break;
                case GameserverMessageType.CreatureLight:
                    ParseCreatureLight(_inputStream);
                    break;
                case GameserverMessageType.CreatureOutfit:
                    ParseCreatureOutfit(_inputStream);
                    break;
                case GameserverMessageType.CreatureSpeed:
                    ParseCreatureSpeed(_inputStream);
                    break;
                case GameserverMessageType.CreatureSkull:
                    ParseCreatureSkull(_inputStream);
                    break;
                case GameserverMessageType.CreatureShield:
                    ParseCreatureShield(_inputStream);
                    break;
                case GameserverMessageType.CreatureUnpass:
                    ParseCreatureUnpass(_inputStream);
                    break;
                case GameserverMessageType.CreatureMarks:
                    ParseCreatureMarks(_inputStream);
                    break;
                case GameserverMessageType.PlayerHelpers:
                    if (gameManager.ClientVersion < 1185)
                        ParsePlayerHelpers(_inputStream);
                    else
                        throw new System.Exception("opcode (PlayerHelpers) changed and must be verified.");
                    break;
                case GameserverMessageType.CreatureType:
                    ParseCreatureType(_inputStream);
                    break;
                case GameserverMessageType.GameNews:
                    ParseGameNews(_inputStream);
                    break;

                case GameserverMessageType.Blessings:
                    ParseBlessings(_inputStream);
                    break;
                case GameserverMessageType.PremiumTrigger:
                    ParsePremiumTrigger(_inputStream);
                    break;
                case GameserverMessageType.PlayerBasicData:
                    ParseBasicData(_inputStream);
                    break;
                case GameserverMessageType.PlayerStats:
                    ParsePlayerStats(_inputStream);
                    break;
                case GameserverMessageType.PlayerSkills:
                    ParsePlayerSkills(_inputStream);
                    break;
                case GameserverMessageType.PlayerStates:
                    ParsePlayerStates(_inputStream);
                    break;
                case GameserverMessageType.ClearTarget:
                    ParseClearTarget(_inputStream);
                    break;
                case GameserverMessageType.SpellDelay:
                    ParseSpellDelay(_inputStream);
                    break;
                case GameserverMessageType.SpellGroupDelay:
                    ParseSpellGroupDelay(_inputStream);
                    break;
                case GameserverMessageType.MultiUseDelay:
                    ParsaeMultiUseDelay(_inputStream);
                    break;

                case GameserverMessageType.SetTactics:
                    ParseSetTactics(_inputStream);
                    break;
                case GameserverMessageType.SetStoreDeepLink:
                    ParseSetStoreDeepLink(_inputStream);
                    break;

                case GameserverMessageType.RestingAreaState:
                    ParseRestingAreaState(_inputStream);
                    break;
                case GameserverMessageType.Talk:
                    ParseTalk(_inputStream);
                    break;
                case GameserverMessageType.Channels:
                    ParseChannels(_inputStream);
                    break;
                case GameserverMessageType.OpenChannel:
                    ParseOpenChannel(_inputStream);
                    break;
                case GameserverMessageType.PrivateChannel:
                    ParsePrivateChannel(_inputStream);
                    break;

                case GameserverMessageType.OpenOwnChannel:
                    ParseOpenOwnChannel(_inputStream);
                    break;
                case GameserverMessageType.CloseChannel:
                    ParseCloseChannel(_inputStream);
                    break;
                case GameserverMessageType.TextMessage:
                    ParseTextMessage(_inputStream);
                    break;
                case GameserverMessageType.CancelWalk:
                    ParseCancelWalk(_inputStream);
                    break;
                case GameserverMessageType.Wait:
                    ParseWait(_inputStream);
                    break;
                case GameserverMessageType.UnjustifiedPoints:
                    ParseUnjustifiedPoints(_inputStream);
                    break;
                case GameserverMessageType.PvpSituations:
                    ParsePvpSituations(_inputStream);
                    break;

                case GameserverMessageType.TopFloor:
                    ParseMapTopFloor(_inputStream);
                    break;
                case GameserverMessageType.BottomFloor:
                    ParseMapBottomFloor(_inputStream);
                    break;
                case GameserverMessageType.UpdateLootContainers:
                    if (!gameManager.GetFeature(GameFeature.GameQuickLoot))
                        goto default;
                    ParseUpdateLootContainers(_inputStream);
                    break;

                case GameserverMessageType.OutfitDialog:
                    ParseOutfitDialog(_inputStream);
                    break;
                case GameserverMessageType.MessageExivaSuppressed:
                    ParseMessageExivaSuppressed(_inputStream);
                    break;
                case GameserverMessageType.UpdateExivaOptions:
                    ParseUpdateExivaOptions(_inputStream);
                    break;

                case GameserverMessageType.ImpactTracking:
                    if (!gameManager.GetFeature(GameFeature.GameAnalytics))
                        goto default;
                    ParseImpactTracking(_inputStream);
                    break;
                case GameserverMessageType.MarketStatistics:
                    if (gameManager.ClientVersion < 1140)
                        goto default;
                    ParseMarketStatistics(_inputStream);
                    break;
                case GameserverMessageType.ItemWasted:
                    if (!gameManager.GetFeature(GameFeature.GameAnalytics))
                        goto default;
                    ParseItemWasted(_inputStream);
                    break;
                case GameserverMessageType.ItemLooted:
                    if (!gameManager.GetFeature(GameFeature.GameAnalytics))
                        goto default;
                    ParseItemLooted(_inputStream);
                    break;
                case GameserverMessageType.TrackedQuestFlags:
                    if (!gameManager.GetFeature(GameFeature.GameQuestTracker))
                        goto default;
                    ParseTrackedQuestFlags(_inputStream);
                    break;
                case GameserverMessageType.KillTracking:
                    if (!gameManager.GetFeature(GameFeature.GameAnalytics))
                        goto default;
                    ParseKillTracking(_inputStream);
                    break;
                case GameserverMessageType.BuddyAdd:
                    ParseBuddyAdd(_inputStream);
                    break;
                case GameserverMessageType.BuddyState:
                    ParseBuddyState(_inputStream);
                    break;
                case GameserverMessageType.BuddyLogout_BuddyGroupData:
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBuddyGroups))
                        ParseBuddyGroupData(_inputStream);
                    else
                        ParseBuddyLogout(_inputStream);
                    break;
                case GameserverMessageType.MonsterCyclopedia:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopediaMonsters))
                        goto default;
                    ParseMonsterCyclopedia(_inputStream);
                    break;
                case GameserverMessageType.MonsterCyclopediaMonsters:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopediaMonsters))
                        goto default;
                    ParseMonsterCyclopediaMonsters(_inputStream);
                    break;
                case GameserverMessageType.MonsterCyclopediaRace:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopediaMonsters))
                        goto default;
                    ParseMonsterCyclopediaRace(_inputStream);
                    break;
                case GameserverMessageType.MonsterCyclopediaBonusEffects:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopediaMonsters))
                        goto default;
                    ParseMonsterCyclopediaBonusEffects(_inputStream);
                    break;
                case GameserverMessageType.MonsterCyclopediaNewDetails:
                    if (!gameManager.GetFeature(GameFeature.GameCyclopediaMonsters))
                        goto default;
                    ParseMonsterCyclopediaNewDetails(_inputStream);
                    break;
                case GameserverMessageType.CyclopediaCharacterInfo:
                    if (gameManager.ClientVersion < 1200)
                        goto default;
                    ParseCyclopediaCharacterInfo(_inputStream);
                    break;
                    
                case GameserverMessageType.TutorialHint:
                    _inputStream.ReadUnsignedByte(); // hintId
                    break;
                case GameserverMessageType.AutomapFlag_CyclopediaMapData:
                    if (gameManager.GetFeature(GameFeature.GameCyclopediaMapAdditionalDetails))
                        ParseCyclopediaMapData(_inputStream);
                    else
                        ParseAutomapFlag(_inputStream);
                    break;
                case GameserverMessageType.DailyRewardCollectionState:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardCollectionState(_inputStream);
                    break;
                case GameserverMessageType.CreditBalance:
                    if (!gameManager.GetFeature(GameFeature.GameIngameStore))
                        goto default;
                    ParseCreditBalance(_inputStream);
                    break;
                case GameserverMessageType.StoreError:
                    if (!gameManager.GetFeature(GameFeature.GameIngameStore))
                        goto default;
                    ParseStoreError(_inputStream);
                    break;
                case GameserverMessageType.RequestPurchaseData:
                    if (!gameManager.GetFeature(GameFeature.GameIngameStore))
                        goto default;
                    ParseRequestPurchaseData(_inputStream);
                    break;
                case GameserverMessageType.OpenRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseOpenRewardWall(_inputStream);
                    break;
                case GameserverMessageType.CloseRewardWall:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseCloseRewardWall(_inputStream);
                    break;
                case GameserverMessageType.DailyRewardBasic:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardBasic(_inputStream);
                    break;
                case GameserverMessageType.DailyRewardHistory:
                    if (!gameManager.GetFeature(GameFeature.GameRewardWall))
                        goto default;
                    ParseDailyRewardHistory(_inputStream);
                    break;

                case GameserverMessageType.PreyFreeListRerollAvailability:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyFreeListRerollAvailability(_inputStream);
                    break;
                case GameserverMessageType.PreyTimeLeft:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyTimeLeft(_inputStream);
                    break;
                case GameserverMessageType.PreyData:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyData(_inputStream);
                    break;
                case GameserverMessageType.PreyPrices:
                    if (!gameManager.GetFeature(GameFeature.GamePrey))
                        goto default;
                    ParsePreyPrices(_inputStream);
                    break;

                case GameserverMessageType.CloseImbuingDialog:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseCloseImbuingDialog(_inputStream);
                    break;
                case GameserverMessageType.ShowMessageDialog:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseShowMessageDialog(_inputStream);
                    break;

                case GameserverMessageType.ResourceBalance:
                    if (!gameManager.GetFeature(GameFeature.GameImbuing))
                        goto default;
                    ParseResourceBalance(_inputStream);
                    break;
                case GameserverMessageType.TibiaTime:
                    if (gameManager.ClientVersion < 1121)
                        goto default;
                    _inputStream.ReadUnsignedByte(); // hrs
                    _inputStream.ReadUnsignedByte(); // mins
                    break;
                case GameserverMessageType.UpdatingStoreBalance:
                    ParseUpdatingStoreBalance(_inputStream);
                    break;
                case GameserverMessageType.ChannelEvent:
                    ParseChannelEvent(_inputStream);
                    break;
                case GameserverMessageType.ObjectInfo:
                    ParseObjectInfo(_inputStream);
                    break;
                case GameserverMessageType.PlayerInventory:
                    ParsePlayerInventory(_inputStream);
                    break;
                case GameserverMessageType.MarketEnter:
                    ParseMarketEnter(_inputStream);
                    break;
                case GameserverMessageType.MarketLeave:
                    ParseMarketLeave(_inputStream);
                    break;
                case GameserverMessageType.MarketDetail:
                    ParseMarketDetail(_inputStream);
                    break;
                case GameserverMessageType.MarketBrowse:
                    ParseMarketBrowse(_inputStream);
                    break;
                case GameserverMessageType.ShowModalDialog:
                    ParseShowModalDialog(_inputStream);
                    break;
                case GameserverMessageType.PremiumStore:
                    ParseStoreCategories(_inputStream);
                    break;
                case GameserverMessageType.PremiumStoreOffers:
                    ParseStoreOffers(_inputStream);
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

        private void ParsePingBack(Internal.CommunicationStream message) {
            _pingReceived++;
            
            if (_pingReceived == _pingSent)
                _ping = (int)_pingTimer.ElapsedMilliseconds;
            else
                ChatStorage.AddDebugMessage("ProtocolGame.ParsePingBack: Got an invalid ping from server");

            OpenTibiaUnity.GameManager.ShouldSendPingAt = OpenTibiaUnity.TicksMillis + Constants.PingDelay;
        }

        private void ParsePing(Internal.CommunicationStream message) {
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
