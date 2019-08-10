using OpenTibiaUnity.Core.Communication.Types;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        private void SendProxyWorldNameIdentification() {
            var message = new Internal.ByteArray();
            message.WriteString(WorldName + "\n", 0, true);
            m_Connection.Send(message);
        }

        internal void SendLogin(uint challengeTimestamp, byte challengeRandom) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PendingGame);
            message.WriteUnsignedShort((ushort)Utility.Utility.GetCurrentOs());

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
                m_XTEA.WriteKey(message);
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
            
            m_PacketWriter.FinishMessage();
        }
        internal void SendEnterGame() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.EnterWorld);
            m_PacketWriter.FinishMessage();
        }
        internal void SendQuitGame() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.QuitGame);
            m_PacketWriter.FinishMessage();
        }
        private void InternalSendPing() {
            // this function should only be called from protocolgame
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Ping);
            m_PacketWriter.FinishMessage();
        }
        internal void InternalSendPingBack() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PingBack);
            m_PacketWriter.FinishMessage();
        }
        
        internal void SendGo(List<int> pathSteps) {
            if (pathSteps == null)
                return;
            
            CreatureStorage.ClearTargets();
            var message = m_PacketWriter.CreateMessage();
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

            m_PacketWriter.FinishMessage();
        }
        internal void SendStop() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Stop);
            m_PacketWriter.FinishMessage();
        }
        internal void SendTurnNorth() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnNorth);
            m_PacketWriter.FinishMessage();
        }
        internal void SendTurnEast() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnEast);
            m_PacketWriter.FinishMessage();
        }
        internal void SendTurnSouth() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnSouth);
            m_PacketWriter.FinishMessage();
        }
        internal void SendTurnWest() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnWest);
            m_PacketWriter.FinishMessage();
        }

        internal void SendMoveObject(UnityEngine.Vector3Int sourceAbsolute, uint typeID, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            if (sourceAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.MoveObject);
            message.WritePosition(sourceAbsolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPos);
            message.WritePosition(destAbsolute);
            message.WriteUnsignedByte((byte)moveAmount);
            m_PacketWriter.FinishMessage();
        }

        internal void SendUseObject(UnityEngine.Vector3Int absolute, uint typeID, int stackPosOrData, int window) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedByte((byte)window); // for containers
            m_PacketWriter.FinishMessage();
        }

        internal void SendUseTwoObjects(UnityEngine.Vector3Int firstAbsolute, uint firstID, int firstData, UnityEngine.Vector3Int secondAbsolute, uint secondID, int secondData) {
            if (firstAbsolute.x != 65535 || secondAbsolute.x != 65535)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseTwoObjects);
            message.WritePosition(firstAbsolute);
            message.WriteUnsignedShort((ushort)firstID);
            message.WriteUnsignedByte((byte)firstData);
            message.WritePosition(secondAbsolute);
            message.WriteUnsignedShort((ushort)secondID);
            message.WriteUnsignedByte((byte)secondData);
            m_PacketWriter.FinishMessage();
        }

        internal void SendUseOnCreature(UnityEngine.Vector3Int absolute, uint typeID, int stackPosOrData, uint creatureID) {
            if (absolute.x != 65535)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.UseOnCreature);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPosOrData);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendTurnObject(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.TurnObject);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPos);
            m_PacketWriter.FinishMessage();
        }

        internal void SendToggleWrapState(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WriteEnum(GameclientMessageType.ToggleWrapState);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPos);
            m_PacketWriter.FinishMessage();
        }

        internal void SendLook(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Look);
            message.WritePosition(absolute);
            message.WriteUnsignedShort((ushort)typeID);
            message.WriteUnsignedByte((byte)stackPos);
            m_PacketWriter.FinishMessage();
        }

        internal void SendLookAtCreature(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Look);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendJoinAggression(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinAggression);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendTalk(MessageModeType mode, int channelID, string receiver, string text) {
            var message = m_PacketWriter.CreateMessage();
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
                    message.WriteUnsignedShort((ushort)channelID);
                    break;
                default:
                    break;
            }

            message.WriteString(text);
            m_PacketWriter.FinishMessage();
        }
        internal void SendGetChannels() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetChannels);
            m_PacketWriter.FinishMessage();
        }
        internal void SendJoinChannel(int channelID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinChannel);
            message.WriteUnsignedShort((ushort)channelID);
            m_PacketWriter.FinishMessage();
        }
        internal void SendLeaveChannel(int channelID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.LeaveChannel);
            message.WriteUnsignedShort((ushort)channelID);
            m_PacketWriter.FinishMessage();
        }
        internal void SendPrivateChannel(string playerName) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PrivateChannel);
            message.WriteString(playerName);
            m_PacketWriter.FinishMessage();
        }

        internal void SendCloseNPCChannel() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.CloseNPCChannel);
            m_PacketWriter.FinishMessage();
        }

        internal void SendSetTactics() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.SetTactics);
            message.WriteUnsignedByte((byte)optionStorage.CombatAttackMode);
            message.WriteUnsignedByte((byte)optionStorage.CombatChaseMode);
            message.WriteUnsignedByte(optionStorage.CombatSecureMode ? (byte)1 : (byte)0);
            message.WriteUnsignedByte((byte)optionStorage.CombatPvPMode);
            m_PacketWriter.FinishMessage();
        }

        internal void SendAttack(uint creatureID) {
            if (creatureID != 0)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Attack);
            message.WriteUnsignedInt(creatureID);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureID); // Sequence
            m_PacketWriter.FinishMessage();
        }

        internal void SendFollow(uint creatureID) {
            if (creatureID != 0)
                Player.StopAutowalk(false);

            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Follow);
            message.WriteUnsignedInt(creatureID);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                message.WriteUnsignedInt(creatureID); // Sequence
            m_PacketWriter.FinishMessage();
        }

        internal void SendInviteToParty(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.InviteToParty);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendJoinParty(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.JoinParty);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendRevokeInvitation(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.RevokeInvitation);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendPassLeadership(uint creatureID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.PassLeadership);
            message.WriteUnsignedInt(creatureID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendLeaveParty() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.LeaveParty);
            m_PacketWriter.FinishMessage();
        }

        internal void SendShareExperience(bool shared) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ShareExperience);
            message.WriteBoolean(shared);
            m_PacketWriter.FinishMessage();
        }

        internal void SendOpenChannel() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.OpenChannel);
            m_PacketWriter.FinishMessage();
        }

        internal void SendInviteToChannel(string name, int channelID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.InviteToChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendExcludeFromChannel(string name, int channelID) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.ExcludeFromChannel);
            message.WriteString(name);
            message.WriteUnsignedShort((ushort)channelID);
            m_PacketWriter.FinishMessage();
        }

        internal void SendCancel() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Cancel);
            m_PacketWriter.FinishMessage();
        }

        internal void SendBrowseField(UnityEngine.Vector3Int absolute) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.BrowseField);
            message.WritePosition(absolute);
            m_PacketWriter.FinishMessage();
        }

        internal void SendGetOutfit() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetOutfit);
            m_PacketWriter.FinishMessage();
        }

        internal void SendSetOutfit(Appearances.OutfitInstance outfit, Appearances.OutfitInstance mount) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.SetOutfit);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameOutfitIDU16))
                message.WriteUnsignedShort((ushort)outfit.ID);
            else
                message.WriteUnsignedByte((byte)outfit.ID);
            message.WriteUnsignedByte((byte)outfit.Head);
            message.WriteUnsignedByte((byte)outfit.Torso);
            message.WriteUnsignedByte((byte)outfit.Legs);
            message.WriteUnsignedByte((byte)outfit.Detail);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons))
                message.WriteUnsignedByte((byte)outfit.AddOns);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                message.WriteUnsignedShort((ushort)mount?.ID);

            m_PacketWriter.FinishMessage();
        }

        internal void SendMount(bool toggle) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.Mount);
            message.WriteBoolean(toggle);
            m_PacketWriter.FinishMessage();
        }

        internal void SendAddBuddyGroup(bool add, string name) {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.AddBuddyGroup);
            message.WriteBoolean(add); // 0 to remove
            message.WriteString(name);
            m_PacketWriter.FinishMessage();
        }

        internal void SendGetQuestLog() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(GameclientMessageType.GetQuestLog);
            m_PacketWriter.FinishMessage();
        }
    }
}
