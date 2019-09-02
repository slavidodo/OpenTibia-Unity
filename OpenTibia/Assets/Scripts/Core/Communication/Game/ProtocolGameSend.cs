using OpenTibiaUnity.Core.Communication.Types;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void SendProxyWorldNameIdentification() {
            var message = new Internal.ByteArray();
            message.WriteString(WorldName + "\n", 0, true);
            _connection.Send(message);
        }

        public void SendLogin(uint challengeTimestamp, byte challengeRandom) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PendingGame);
            message.WriteUnsignedShort((ushort)Utils.Utility.GetCurrentOs());

            var gameManager = OpenTibiaUnity.GameManager;
            var optionStorage = OpenTibiaUnity.OptionStorage;
            
            message.WriteUnsignedShort((ushort)gameManager.ProtocolVersion);

            if (gameManager.GetFeature(GameFeature.GameClientVersion))
                message.WriteUnsignedInt((uint)gameManager.ClientVersion);

            if (gameManager.GetFeature(GameFeature.GameContentRevision))
                message.WriteUnsignedShort(OpenTibiaUnity.GetContentRevision(gameManager.ClientVersion, gameManager.BuildVersion));

            if (gameManager.GetFeature(GameFeature.GamePreviewState))
                message.WriteUnsignedByte(0);

            if (OpenTibiaUnity.GameManager.BuildVersion >= 6018 && OpenTibiaUnity.GameManager.BuildVersion < 7695) {
                message.WriteBoolean(optionStorage.OptimiseConnectionStability);
            }
            
            int payloadStart = message.Position;
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
        public void SendEnterGame() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.EnterWorld);
            _packetWriter.FinishMessage();
        }
        public void SendQuitGame() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.QuitGame);
            _packetWriter.FinishMessage();
        }
        private void InternalSendPing() {
            // this function should only be called from protocolgame
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Ping);
            _packetWriter.FinishMessage();
        }
        public void InternalSendPingBack() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PingBack);
            _packetWriter.FinishMessage();
        }
        
        public void SendGo(List<int> pathSteps) {
            if (pathSteps == null)
                return;
            
            CreatureStorage.ClearTargets();
            var message = _packetWriter.CreateMessage();
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
        public void SendStop() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Stop);
            _packetWriter.FinishMessage();
        }
        public void SendTurnNorth() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnNorth);
            _packetWriter.FinishMessage();
        }
        public void SendTurnEast() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnEast);
            _packetWriter.FinishMessage();
        }
        public void SendTurnSouth() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnSouth);
            _packetWriter.FinishMessage();
        }
        public void SendTurnWest() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnWest);
            _packetWriter.FinishMessage();
        }

        public void SendMoveObject(UnityEngine.Vector3Int sourceAbsolute, ushort objectId, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            if (sourceAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.MoveObject);
            message.WritePosition(sourceAbsolute);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)stackPos);
            message.WritePosition(destAbsolute);
            message.WriteUnsignedByte((byte)moveAmount);
            _packetWriter.FinishMessage();
        }
        public void SendInspectInNpcTrade(ushort objectId, int count) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.LookInNpcTrade);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)count);
            _packetWriter.FinishMessage();
        }
        public void SendBuyObject(ushort objectId, int subType, int amount, bool ignoreCapacity, bool buyWithBackpack) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.BuyObject);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)subType);
            message.WriteUnsignedByte((byte)amount);
            message.WriteBoolean(ignoreCapacity);
            message.WriteBoolean(buyWithBackpack);
            _packetWriter.FinishMessage();
        }
        public void SendSellObject(ushort objectId, int subType, int amount, bool ignoreEquiped) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.SellObject);
            message.WriteUnsignedShort(objectId);
            message.WriteUnsignedByte((byte)subType);
            message.WriteUnsignedByte((byte)amount);
            message.WriteBoolean(ignoreEquiped);
            _packetWriter.FinishMessage();
        }
        public void SendCloseNPCTrade() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.CloseNpcTrade);
            _packetWriter.FinishMessage();
        }

        public void SendUseObject(UnityEngine.Vector3Int absolute, uint typeId, int stackPosOrData, int window) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedByte((byte)window); // for containers
            _packetWriter.FinishMessage();
        }

        public void SendUseTwoObjects(UnityEngine.Vector3Int firstAbsolute, uint firstId, int firstData, UnityEngine.Vector3Int secondAbsolute, uint secondId, int secondData) {
            if (firstAbsolute.x != 65535 || secondAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseTwoObjects);
            message.WritePosition(firstAbsolute);
            message.WriteUnsignedShort((ushort)firstId);
            message.WriteUnsignedByte((byte)firstData);
            message.WritePosition(secondAbsolute);
            message.WriteUnsignedShort((ushort)secondId);
            message.WriteUnsignedByte((byte)secondData);
            _packetWriter.FinishMessage();
        }

        public void SendUseOnCreature(UnityEngine.Vector3Int absolute, uint typeId, int stackPosOrData, uint creatureId) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseOnCreature);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendTurnObject(UnityEngine.Vector3Int absolute, uint typeId, int stackPos) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }
        public void SendCloseContainer(int containerId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.CloseContainer);
            message.WriteUnsignedByte((byte)containerId);
            _packetWriter.FinishMessage();
        }
        public void SendUpContainer(int containerId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UpContainer);
            message.WriteUnsignedByte((byte)containerId);
            _packetWriter.FinishMessage();
        }

        public void SendToggleWrapState(UnityEngine.Vector3Int absolute, uint typeId, int stackPos) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }

        public void SendLook(UnityEngine.Vector3Int absolute, uint typeId, int stackPos) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Look);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeId);
            message.WriteUnsignedByte((byte)stackPos);
            _packetWriter.FinishMessage();
        }

        public void SendLookAtCreature(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Look);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendJoinAggression(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinAggression);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendTalk(MessageModeType mode, int channelId, string receiver, string text) {
            var message = _packetWriter.CreateMessage();
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
        public void SendGetChannels() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetChannels);
            _packetWriter.FinishMessage();
        }
        public void SendJoinChannel(int channelId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinChannel);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        public void SendLeaveChannel(int channelId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.LeaveChannel);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }
        public void SendPrivateChannel(string playerName) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PrivateChannel);
            message.WriteString(playerName);
            _packetWriter.FinishMessage();
        }

        public void SendCloseNPCChannel() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.CloseNPCChannel);
            _packetWriter.FinishMessage();
        }

        public void SendSetTactics() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.SetTactics);
            message.WriteUnsignedByte((byte)optionStorage.CombatAttackMode);
            message.WriteUnsignedByte((byte)optionStorage.CombatChaseMode);
            message.WriteUnsignedByte(optionStorage.CombatSecureMode ? (byte)1 : (byte)0);
            message.WriteUnsignedByte((byte)optionStorage.CombatPvPMode);
            _packetWriter.FinishMessage();
        }

        public void SendAttack(uint creatureId) {
            if (creatureId != 0)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Attack);
            message.WriteUnsignedInt(creatureId);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureId); // Sequence
            _packetWriter.FinishMessage();
        }

        public void SendFollow(uint creatureId) {
            if (creatureId != 0)
                Player.StopAutowalk(false);

            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Follow);
            message.WriteUnsignedInt(creatureId);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureId); // Sequence
            _packetWriter.FinishMessage();
        }

        public void SendInviteToParty(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.InviteToParty);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendJoinParty(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinParty);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendRevokeInvitation(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.RevokeInvitation);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendPassLeadership(uint creatureId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PassLeadership);
            message.WriteUnsignedInt(creatureId);
            _packetWriter.FinishMessage();
        }

        public void SendLeaveParty() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.LeaveParty);
            _packetWriter.FinishMessage();
        }

        public void SendShareExperience(bool shared) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ShareExperience);
            message.WriteBoolean(shared);
            _packetWriter.FinishMessage();
        }

        public void SendOpenChannel() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.OpenChannel);
            _packetWriter.FinishMessage();
        }

        public void SendInviteToChannel(string name, int channelId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.InviteToChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }

        public void SendExcludeFromChannel(string name, int channelId) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ExcludeFromChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelId);
            _packetWriter.FinishMessage();
        }

        public void SendCancel() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Cancel);
            _packetWriter.FinishMessage();
        }

        public void SendBrowseField(UnityEngine.Vector3Int absolute) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.BrowseField);
            message.WritePosition(absolute);
            _packetWriter.FinishMessage();
        }

        public void SendGetOutfit() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetOutfit);
            _packetWriter.FinishMessage();
        }

        public void SendSetOutfit(Appearances.OutfitInstance outfit, Appearances.OutfitInstance mount) {
            var message = _packetWriter.CreateMessage();
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

        public void SendMount(bool toggle) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Mount);
            message.WriteBoolean(toggle);
            _packetWriter.FinishMessage();
        }

        public void SendAddBuddyGroup(bool add, string name) {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.AddBuddyGroup);
            message.WriteBoolean(add); // 0 to remove
            message.WriteString(name);
            _packetWriter.FinishMessage();
        }

        public void SendGetQuestLog() {
            var message = _packetWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetQuestLog);
            _packetWriter.FinishMessage();
        }
    }
}
