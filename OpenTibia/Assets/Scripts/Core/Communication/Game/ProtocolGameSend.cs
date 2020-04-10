using OpenTibiaUnity.Core.Communication.Attributes;
using OpenTibiaUnity.Core.Communication.Types;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        [ClientFeature(GameFeature.GameWorldProxyIdentification)]
        private void SendProxyWorldNameIdentification() {
            var message = new Internal.CommunicationStream();
            message.WriteString(WorldName + "\n", true);
            _connection.Send(message);
        }
        [ClientVersion(0)]
        public void SendLogin(uint challengeTimestamp, byte challengeRandom) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.PendingGame);
            message.WriteUnsignedShort((ushort)Utils.Utility.GetCurrentOs());

            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;
            
            message.WriteUnsignedShort((ushort)gameManager.ProtocolVersion);

            if (gameManager.GetFeature(GameFeature.GameClientVersion))
                message.WriteUnsignedInt((uint)gameManager.ClientVersion);

            // TODO; use a dynamic value obtained from the spriteprovider instead of a statis once that
            // must be maintained within every update..
            if (gameManager.GetFeature(GameFeature.GameContentRevision))
                message.WriteUnsignedShort(OpenTibiaUnity.GetContentRevision(gameManager.ClientVersion, gameManager.BuildVersion));

            if (gameManager.GetFeature(GameFeature.GamePreviewState))
                message.WriteUnsignedByte(0);

            if (OpenTibiaUnity.GameManager.BuildVersion >= 6018 && OpenTibiaUnity.GameManager.BuildVersion < 7695) {
                message.WriteBoolean(optionStorage.OptimiseConnectionStability);
            }
            
            int payloadStart = (int)message.Position;
            message.WriteUnsignedByte(0); // first byte must be zero;
            
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption)) {
                _xTEA.WriteKey(message);
                message.WriteUnsignedByte(0x00); // gm
            }

            if (gameManager.GetFeature(GameFeature.GameSessionKey)) {
                message.WriteString(SessionKey);
                message.WriteString(CharacterName);
            } else {
                if (gameManager.GetFeature(GameFeature.GameAccountNames))
                    message.WriteString(AccountName);
                else
                    message.WriteUnsignedInt(uint.Parse(AccountName));

                message.WriteString(CharacterName);
                message.WriteString(Password);

                if (gameManager.GetFeature(GameFeature.GameAuthenticator))
                    message.WriteString(Token);
            }

            if (gameManager.GetFeature(GameFeature.GameChallengeOnLogin)) {
                message.WriteUnsignedInt(challengeTimestamp);
                message.WriteUnsignedByte(challengeRandom);
            }
            
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption))
                Cryptography.PublicRSA.EncryptMessage(message, payloadStart, Cryptography.PublicRSA.RSABlockSize);
            
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendEnterGame() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.EnterWorld);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendQuitGame() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.QuitGame);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameClientPing)]
        public void SendPing() {
            // this function should only be called from protocolgame
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Ping);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameClientPing)]
        public void SendPingBack() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.PingBack);
            _packetWriter.FinishMessage();
        }
        //[ClientFeature(GameFeature.PerformanceMetrics)] // 950?
        public void SendPerformanceMetrics(short minObjects, short maxObjects, short averageObjects, ushort minFps, ushort maxFps, ushort averageFps, ushort fpsLimit) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.PerformanceMetrics);
            message.WriteShort(minObjects);
            message.WriteShort(maxObjects);
            message.WriteShort(averageObjects);
            message.WriteUnsignedShort(minFps);
            message.WriteUnsignedShort(maxFps);
            message.WriteUnsignedShort(averageFps);
            message.WriteUnsignedShort(fpsLimit);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGo(List<int> pathSteps) {
            if (pathSteps == null)
                return;
            
            CreatureStorage.ClearTargets();
            var message = _packetWriter.PrepareStream();
            if (pathSteps.Count == 1) {
                switch ((PathDirection)(pathSteps[0] & 65535)) {
                    case PathDirection.East: message.WriteEnum(GameclientMessageType.GoEast); break;
                    case PathDirection.NorthEast: message.WriteEnum(GameclientMessageType.GoNorthEast); break;
                    case PathDirection.North: message.WriteEnum(GameclientMessageType.GoNorth); break;
                    case PathDirection.NorthWest: message.WriteEnum(GameclientMessageType.GoNorthWest); break;
                    case PathDirection.West: message.WriteEnum(GameclientMessageType.GoWest); break;
                    case PathDirection.SouthWest: message.WriteEnum(GameclientMessageType.GoSouthWest); break;
                    case PathDirection.South: message.WriteEnum(GameclientMessageType.GoSouth); break;
                    case PathDirection.SouthEast: message.WriteEnum(GameclientMessageType.GoSouthEast); break;
                    default: return;
                }
            } else {
                int pathMaxSteps = (byte)System.Math.Min(byte.MaxValue, pathSteps.Count);

                message.WriteEnum(GameclientMessageType.GoPath);
                message.WriteUnsignedByte((byte)pathMaxSteps);
                int i = 0;
                while (i < pathMaxSteps) {
                    message.WriteUnsignedByte((byte)(pathSteps[i] & 65535));
                    i++;
                }
            }

            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendStop() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Stop);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendTurn(Direction direction) {
            var message = _packetWriter.PrepareStream();
            switch (direction) {
                case Direction.North:
                    message.WriteEnum(GameclientMessageType.TurnNorth);
                    break;
                case Direction.East:
                    message.WriteEnum(GameclientMessageType.TurnEast);
                    break;
                case Direction.South:
                    message.WriteEnum(GameclientMessageType.TurnSouth);
                    break;
                case Direction.West:
                    message.WriteEnum(GameclientMessageType.TurnWest);
                    break;
                default:
                    throw new System.Exception("ProtocolGameSend.SendTurn: unknown direction: " + direction + ".");
            }
            _packetWriter.FinishMessage();
        }
        [ClientVersion(910)]
        public void SendEquipObject(ushort objectId, int data) {
            Player.StopAutowalk(false);
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.EquipObject);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)data);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendMoveObject(UnityEngine.Vector3Int sourceAbsolute, ushort objectId, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            if (sourceAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MoveObject);
            message.WritePosition(sourceAbsolute);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)stackPos);
            message.WritePosition(destAbsolute);
            message.WriteUnsignedByte((byte)moveAmount);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameNPCInterface)]
        public void SendLookInNpcTrade(ushort objectId, int count) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.LookInNpcTrade);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)count);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameNPCInterface)]
        public void SendBuyObject(ushort objectId, int subType, int amount, bool ignoreCapacity, bool buyWithBackpack) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BuyObject);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)subType);
            message.WriteUnsignedByte((byte)amount);
            message.WriteBoolean(ignoreCapacity);
            message.WriteBoolean(buyWithBackpack);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameNPCInterface)]
        public void SendSellObject(ushort objectId, int subType, int amount, bool ignoreEquiped) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.SellObject);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)subType);
            message.WriteUnsignedByte((byte)amount);
            message.WriteBoolean(ignoreEquiped);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameNPCInterface)]
        public void SendCloseNPCTrade() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.CloseNpcTrade);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendTradeObject(UnityEngine.Vector3Int position, ushort objectId, int stackposOrData, uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.TradeObject);
            message.WritePosition(position);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)stackposOrData);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendLookTrade(bool counterOffer, int index) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.LookTrade);
            message.WriteBoolean(counterOffer);
            message.WriteUnsignedByte((byte)index);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendAcceptTrade() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.AcceptTrade);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendRejectTrade() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RejectTrade);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendUseObject(UnityEngine.Vector3Int absolute, uint typeId, int stackPosOrData, int window) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.UseObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedByte((byte)window); // for containers
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendUseTwoObjects(UnityEngine.Vector3Int firstAbsolute, uint firstId, int firstData, UnityEngine.Vector3Int secondAbsolute, uint secondId, int secondData) {
            if (firstAbsolute.x != 65535 || secondAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.UseTwoObjects);
            message.WritePosition(firstAbsolute);
            message.WriteUnsignedShort((ushort)firstId);
            message.WriteUnsignedByte((byte)firstData);
            message.WritePosition(secondAbsolute);
            message.WriteUnsignedShort((ushort)secondId);
            message.WriteUnsignedByte((byte)secondData);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendUseOnCreature(UnityEngine.Vector3Int absolute, uint typeId, int stackPosOrData, uint creatureId) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.UseOnCreature);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendTurnObject(UnityEngine.Vector3Int absolute, uint typeId, int stackPos) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.TurnObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendCloseContainer(int containerId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.CloseContainer);
            message.WriteUnsignedByte((byte)containerId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendUpContainer(int containerId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.UpContainer);
            message.WriteUnsignedByte((byte)containerId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendEditText(uint textId, string text) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.EditText);
            message.WriteUnsignedInt(textId);
            message.WriteString(text);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendEditList(uint listId, byte doorId, string text) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.EditList);
            message.WriteUnsignedByte(doorId);
            message.WriteUnsignedInt(listId);
            message.WriteString(text);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameWrappableFurniture)]
        public void SendToggleWrapState(UnityEngine.Vector3Int absolute, uint typeId, int stackPos) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendLook(UnityEngine.Vector3Int absolute, ushort typeId, int stackPos) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Look);
            message.WritePosition(absolute);
            message.WriteUnsignedShort(typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendLookAtCreature(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Look);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePVPMode)]
        public void SendJoinAggression(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.JoinAggression);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameQuickLoot)]
        public void SendQuickLoot(UnityEngine.Vector3Int absolute, ushort objectId, int stackposOrData) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.QuickLoot);
            message.WritePosition(absolute);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)stackposOrData);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameQuickLoot)]
        public void SendLootContainer(LootContainerAction action, params object[] rest) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.QuickLoot);
            message.WriteEnum(action);
            switch (action) {
                case LootContainerAction.AssignContainer: {
                    var objectCategory = (ObjectCategory)rest[0];
                    UnityEngine.Vector3Int absolute = (UnityEngine.Vector3Int)rest[1];
                    ushort objectId = (ushort)rest[2];
                    int stackposOrData = (int)rest[3];

                    message.WriteEnum(objectCategory);
                    message.WritePosition(absolute);
                    message.WriteUnsignedShort(objectId);
                    message.WriteUnsignedByte((byte)stackposOrData);
                    break;
                }
                case LootContainerAction.ClearContainer: {
                    var objectCategory = (ObjectCategory)rest[0];
                    message.WriteEnum(objectCategory);
                    break;
                }
                case LootContainerAction.SetUseFallback: {
                    bool useFallback = (bool)rest[0];
                    message.WriteBoolean(useFallback);
                    break;
                }
                default:
                    throw new System.Exception("ProtocolGameSend.SendLootContainer: unknown action: " + action + ".");

            }
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameQuickLoot)]
        public void SendQuickLootBlackWhitelist(QuickLootFilter filter, List<ushort> objectIds) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.QuickLoot);
            message.WriteEnum(filter);
            message.WriteUnsignedShort((ushort)objectIds.Count);
            foreach (ushort objectId in objectIds)
                message.WriteUnsignedShort(objectId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendTalk(MessageModeType mode, int channelId, string receiver, string text) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Talk);
            message.WriteUnsignedByte(TranslateMessageModeToServer(mode));
            switch (mode) {
                case MessageModeType.PrivateTo:
                case MessageModeType.GamemasterPrivateTo:
                case MessageModeType.RVRAnswer:
                    message.WriteString(receiver);
                    break;
                case MessageModeType.Channel:
                case MessageModeType.ChannelHighlight:
                case MessageModeType.ChannelManagement:
                case MessageModeType.GamemasterChannel:
                    message.WriteUnsignedShort((ushort)channelId);
                    break;
                default:
                    break;
            }

            message.WriteString(text);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGetChannels() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetChannels);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendJoinChannel(int channelId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.JoinChannel);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendLeaveChannel(int channelId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.LeaveChannel);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendPrivateChannel(string playerName) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.PrivateChannel);
            message.WriteString(playerName);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(800)]
        public void SendCloseNPCChannel() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.CloseNPCChannel);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendSetTactics() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.SetTactics);
            message.WriteUnsignedByte((byte)optionStorage.CombatAttackMode);
            message.WriteUnsignedByte((byte)optionStorage.CombatChaseMode);
            message.WriteUnsignedByte(optionStorage.CombatSecureMode ? (byte)1 : (byte)0);
            message.WriteUnsignedByte((byte)optionStorage.CombatPvPMode);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendAttack(uint creatureId) {
            if (creatureId != 0)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Attack);
            message.WriteUnsignedInt(creatureId);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureId); // Sequence
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendFollow(uint creatureId) {
            if (creatureId != 0)
                Player.StopAutowalk(false);

            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Follow);
            message.WriteUnsignedInt(creatureId);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureId); // Sequence
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendInviteToParty(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.InviteToParty);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendJoinParty(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.JoinParty);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendRevokeInvitation(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RevokeInvitation);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendPassLeadership(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.PassLeadership);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendLeaveParty() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.LeaveParty);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendShareExperience(bool shared) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ShareExperience);
            message.WriteBoolean(shared);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendOpenChannel() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenChannel);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendInviteToChannel(string name, int channelId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.InviteToChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendExcludeFromChannel(string name, int channelId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ExcludeFromChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendCancel() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Cancel);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameBrowseField)]
        public void SendBrowseField(UnityEngine.Vector3Int absolute) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BrowseField);
            message.WritePosition(absolute);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameContainerPagination)]
        public void SendSeekInContainer(byte containerId, ushort index) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.SeekInContainer);
            message.WriteUnsignedByte(containerId);
            message.WriteUnsignedShort(index);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameInspectionWindow)]
        public void SendInspectObject(InspectObjectTypes type, params object[] rest) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.InspectObject);
            message.WriteEnum(type);
            switch (type) {
                case InspectObjectTypes.NormalObject: {
                    UnityEngine.Vector3Int absolute = (UnityEngine.Vector3Int)rest[0];
                    message.WritePosition(absolute);
                    break;
                }
                case InspectObjectTypes.NpcTrade: {
                    var objectId = (ushort)rest[0];
                    int stackposOrData = (int)rest[1];
                    message.WriteUnsignedShort(objectId);
                    message.WriteUnsignedByte((byte)stackposOrData);
                    break;
                }
                case InspectObjectTypes.Cyclopedia: {
                    var objectId = (ushort)rest[0];
                    message.WriteUnsignedShort(objectId);
                    break;
                }
                default:
                    throw new System.Exception("ProtocolGameSend.SendLootContainer: unknown action: " + type + ".");
            }
        }
        [ClientFeature(GameFeature.GameInspectionWindow)]
        public void SendInspectPlayer(ClientInspectPlayerState state, uint creatureId = 0) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.InspectPlayer);
            message.WriteEnum(state);
            if (state <= ClientInspectPlayerState.RevokePermission)
                message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameAnalytics)]
        public void SendTrackQuestflags(List<ushort> missionIds) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.TrackQuestflags);
            message.WriteUnsignedByte((byte)missionIds.Count);
            foreach (ushort missionId in missionIds)
                message.WriteUnsignedShort(missionId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCyclopediaMonsters)]
        public void SendMarketStatistics() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketStatistics);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGetOutfit() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetOutfit);
            // unconfirmed
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1180)
                message.WriteUnsignedShort(0); // try on outfit
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendSetOutfit(Appearances.OutfitInstance outfit, Appearances.OutfitInstance mount) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.SetOutfit);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameOutfitIdU16))
                message.WriteUnsignedShort((ushort)outfit.Id);
            else
                message.WriteUnsignedByte((byte)outfit.Id);
            message.WriteUnsignedByte((byte)outfit.Head);
            message.WriteUnsignedByte((byte)outfit.Torso);
            message.WriteUnsignedByte((byte)outfit.Legs);
            message.WriteUnsignedByte((byte)outfit.Detail);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons))
                message.WriteUnsignedByte((byte)outfit.AddOns);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                message.WriteUnsignedShort((ushort)mount?.Id);

            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMounts)]
        public void SendMount(bool toggle) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.Mount);
            message.WriteBoolean(toggle);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameImbuing)]
        public void SendApplyImbuement(byte slot, uint imbuementId, bool protectiveCharm) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ApplyImbuement);
            message.WriteUnsignedByte(slot);
            message.WriteUnsignedInt(imbuementId);
            message.WriteBoolean(protectiveCharm);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameImbuing)]
        public void SendApplyClearingCharm(byte slot) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ApplyClearingCharm);
            message.WriteUnsignedByte(slot);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameImbuing)]
        public void SendCloseImbuingDialog() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.CloseImbuingDialog);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameRewardWall)]
        public void SendOpenRewardWall() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenRewardWall);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameRewardWall)]
        public void SendDailyRewardHistory() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenRewardWall);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameRewardWall)]
        public void SendCollectDailyReward(bool shrine, Dictionary<ushort, byte> pickedObjects = null) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenRewardWall);
            message.WriteBoolean(shrine);
            if (pickedObjects != null) {
                message.WriteUnsignedByte((byte)pickedObjects.Count);
                foreach (var pickedObject in pickedObjects) {
                    message.WriteUnsignedShort(pickedObject.Key); // objectId
                    message.WriteUnsignedByte(pickedObject.Value); // count
                }
            }
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendAddBuddy(string name) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.AddBuddy);
            message.WriteString(name);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendRemoveBuddy(uint creatureId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RemoveBuddy);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameAdditionalVipInfo)]
        public void SendEditBuddy(uint creatureId, string description, uint iconId, bool notifyLogin, List<byte> groupIds) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.EditBuddy);
            message.WriteUnsignedInt(creatureId);
            message.WriteString(description);
            message.WriteUnsignedInt(iconId);
            message.WriteBoolean(notifyLogin);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBuddyGroups)) {
                message.WriteUnsignedByte((byte)groupIds.Count);
                foreach (byte groupId in groupIds)
                    message.WriteUnsignedByte(groupId);
            }
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameBuddyGroups)]
        public void SendAddBuddyGroup(bool add, string name) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.AddBuddyGroup);
            message.WriteBoolean(add); // 0 to remove.. todo; check
            message.WriteString(name);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCompendium)]
        public void SendMarkGameNewsAsRead(uint newsId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarkGameNewsAsRead);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCyclopediaMonsters)]
        public void SendOpenMonsterCyclopedia() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenMonsterCyclopedia);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCyclopediaMonsters)]
        public void SendOpenMonsterCyclopediaMonsters(string classification) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenMonsterCyclopediaMonsters);
            message.WriteString(classification);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCyclopediaMonsters)]
        public void SendOpenMonsterCyclopediaRace(ushort raceId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenMonsterCyclopediaRace);
            message.WriteUnsignedShort(raceId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameCyclopediaMonsters)]
        public void SendMonsterBonusEffectAction(CyclopediaBonusEffectAction action, byte charmId, ushort raceId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenMonsterCyclopediaRace);
            message.WriteUnsignedByte(charmId);
            message.WriteEnum(action);
            if (action == CyclopediaBonusEffectAction.Select)
                message.WriteUnsignedShort(raceId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameAnalytics)]
        public void SendOpenCyclopediaCharacterInfo(CyclopediaCharacterInfoType type) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenCyclopediaCharacterInfo);
            // in 12.04 there is extra 4 bytes, i suspect them to be related to friend list
            message.WriteEnum(type);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendBugReport(string comment) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BugReport);
            message.WriteString(comment);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendThankYou(uint statementId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ThankYou);
            message.WriteUnsignedInt(statementId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameDebugAssertion)]
        public void SendErrorFileEntry(string line, string date, string decription, string comment) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.ErrorFileEntry);
            message.WriteString(line);
            message.WriteString(date);
            message.WriteString(decription);
            message.WriteString(comment);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(1180)]
        public void SendGetOfferDescription(uint offerId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetOfferDescription);
            message.WriteUnsignedInt(offerId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendStoreEvent(StoreEvent type, params object[] rest) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.StoreEvent);
            message.WriteEnum(type);
            switch (type) {
                case StoreEvent.SelectOffer: {
                    uint offerId = (uint)rest[0];
                    message.WriteUnsignedInt(offerId);
                    break;
                }
                default:
                    throw new System.Exception("ProtocolGameSend.SendStoreEvent: unknown event type.");
            }
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameAnalytics)]
        public void SendFeatureEvent(FeatureEventType type, bool active) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.FeatureEvent);
            message.WriteEnum(type);
            message.WriteBoolean(active);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePrey)]
        public void SendPreyAction(PreyAction action, int listIndex) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.FeatureEvent);
            message.WriteEnum(action);
            if (action == PreyAction.SelectPrey)
                message.WriteUnsignedByte((byte)listIndex);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(1100)]
        public void SendRequestResourceBalance(ResourceType resource) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RequestResourceBalance);
            message.WriteEnum(resource);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendTransferCurrency(string recipient, uint amount) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.TransferCurrency);
            message.WriteString(recipient);
            message.WriteUnsignedInt(amount);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGetQuestLog() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetQuestLog);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGetQuestLine(ushort questId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetQuestLine);
            message.WriteUnsignedShort(questId);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendRuleViolationReport(byte reason, byte action, string characterName, string comment, string translation) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RuleViolationReport);
            message.WriteUnsignedByte(reason);
            message.WriteUnsignedByte(action);
            message.WriteString(characterName);
            message.WriteString(comment);
            message.WriteString(translation);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(0)]
        public void SendGetObjectInfo(params Appearances.AppearanceTypeRef[] appearances) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.GetObjectInfo);
            message.WriteUnsignedByte((byte)appearances.Length);
            foreach (var appearance in appearances) {
                message.WriteUnsignedShort(appearance.Id);
                message.WriteUnsignedByte((byte)appearance.Data);
            }
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMarket)]
        public void SendMarketLeave() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketLeave);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMarket)]
        public void SendMarketBrowse(ushort browseId) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketBrowse);
            message.WriteUnsignedShort(browseId);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMarket)]
        public void SendMarketCreate(MarketOfferType type, ushort objectId, ushort amount, uint price, bool anonymous) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketBrowse);
            message.WriteEnum(type);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedShort(amount);
            message.WriteUnsignedInt(price);
            message.WriteBoolean(anonymous);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMarket)]
        public void SendMarketCancel(uint timestamp, ushort counter) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketBrowse);
            message.WriteUnsignedInt(timestamp);
            message.WriteUnsignedShort(counter);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GamePlayerMarket)]
        public void SendMarketAccept(uint timestamp, ushort counter, ushort amount) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketBrowse);
            message.WriteUnsignedInt(timestamp);
            message.WriteUnsignedShort(counter);
            message.WriteUnsignedShort(amount);
            _packetWriter.FinishMessage();
        }
        [ClientVersion(970)]
        public void SendAnswerModalDialog(uint windowId, byte button, byte choice) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.MarketBrowse);
            message.WriteUnsignedInt(windowId);
            message.WriteUnsignedByte(button);
            message.WriteUnsignedByte(choice);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendOpenStore(StoreServiceType serviceType = StoreServiceType.Unknown) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.OpenStore);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStoreServiceType))
                message.WriteEnum(serviceType);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendRequestStoreOffers(Store.StoreOpenParameters openParams = null, string categoryName = null) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.RequestStoreOffers);
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1180) {
                openParams.WriteTo(message);
            } else {
                message.WriteEnum(StoreServiceType.Unknown);
                message.WriteString(categoryName);
            }
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendBuyStoreOffer(uint offerId, StoreServiceType serviceType = StoreServiceType.Unknown) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BuyStoreOffer);
            message.WriteUnsignedInt(offerId);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStoreServiceType))
                message.WriteEnum(serviceType);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendOpenTransactionHistory(byte entriesPerPage) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BuyStoreOffer);
            message.WriteUnsignedByte(entriesPerPage);
            _packetWriter.FinishMessage();
        }
        [ClientFeature(GameFeature.GameIngameStore)]
        public void SendGetTransactionHistory(uint currentPage, byte entriesPerPage) {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(GameclientMessageType.BuyStoreOffer);
            message.WriteUnsignedInt(currentPage);
            message.WriteUnsignedByte(entriesPerPage);
            _packetWriter.FinishMessage();
        }
    }
}
