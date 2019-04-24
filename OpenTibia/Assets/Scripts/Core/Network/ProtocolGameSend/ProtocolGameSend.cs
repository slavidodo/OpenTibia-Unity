using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        // TODO: use a pool instead
        // and confirm the encryption of the packet ONLY on a successful sent
        // This is working, but using a pool is more of a robust

        public void SendLoginPacket(uint challengeTimestamp, byte challengeRandom) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.PendingGame);
            message.AddU16(Utility.OperatingSystem.GetCurrentOs());

            var gameManager = OpenTibiaUnity.GameManager;

            message.AddU16((ushort)gameManager.ProtocolVersion);
            if (gameManager.GetFeature(GameFeatures.GameClientVersion))
                message.AddU32((uint)gameManager.ClientVersion);

            if (gameManager.GetFeature(GameFeatures.GameContentRevision))
                message.AddU16(Constants.ContentRevision);

            if (gameManager.GetFeature(GameFeatures.GamePreviewState))
                message.AddU8(0);
            
            int offset = message.Tell();
            message.AddU8(0); // first byte must be zero;

            var random = new System.Random();

            if (gameManager.GetFeature(GameFeatures.GameLoginPacketEncryption)) {
                GenerateXteaKey(random);
                AddXteaKey(message);
                message.AddU8(0x00); // gm
            }

            if (gameManager.GetFeature(GameFeatures.GameSessionKey)) {
                message.AddString(SessionKey);
                message.AddString(CharacterName);
            } else {
                if (gameManager.GetFeature(GameFeatures.GameAccountNames))
                    message.AddString(AccountName);
                else
                    message.AddU32(uint.Parse(AccountName));

                message.AddString(CharacterName);
                message.AddString(Password);

                if (gameManager.GetFeature(GameFeatures.GameAuthenticator))
                    message.AddString(AuthenticatorToken);
            }

            if (gameManager.GetFeature(GameFeatures.GameChallengeOnLogin)) {
                message.AddU32(challengeTimestamp);
                message.AddU8(challengeRandom);
            }

            int paddingBytes = Crypto.RSA.GetRsaSize() - (message.Tell() - offset);
            for (int i = 0; i < paddingBytes; i++) {
                message.AddU8((byte)random.Next(0xFF));
            }

            if (gameManager.GetFeature(GameFeatures.GameLoginPacketEncryption))
                Crypto.RSA.EncryptMessage(message);

            if (gameManager.GetFeature(GameFeatures.GameProtocolChecksum))
                ChecksumEnabled = true;

            WriteToOutput(message);

            if (gameManager.GetFeature(GameFeatures.GameLoginPacketEncryption))
                XteaEnabled = true;
        }
        public void SendEnterGame() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.EnterGame);
            WriteToOutput(message);
        }
        public void SendLeaveGame() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.LeaveGame);
            WriteToOutput(message);
        }
        public void SendPing() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Ping);
            WriteToOutput(message);
        }
        public void SendPingBack() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.PingBack);
            WriteToOutput(message);
        }
        
        public void SendGo(List<int> pathSteps) {
            if (pathSteps == null)
                return;
            
            m_CreatureStorage.ClearTargets();
            var message = new OutputMessage();
            if (pathSteps.Count == 1) {
                switch ((PathDirection)(pathSteps[0] & 65535)) {
                    case PathDirection.East: message.AddU8(GameClientOpCodes.GoEast); break;
                    case PathDirection.NorthEast: message.AddU8(GameClientOpCodes.GoNorthEast); break;
                    case PathDirection.North: message.AddU8(GameClientOpCodes.GoNorth); break;
                    case PathDirection.NorthWest: message.AddU8(GameClientOpCodes.GoNorthWest); break;
                    case PathDirection.West: message.AddU8(GameClientOpCodes.GoWest); break;
                    case PathDirection.SouthWest: message.AddU8(GameClientOpCodes.GoSouthWest); break;
                    case PathDirection.South: message.AddU8(GameClientOpCodes.GoSouth); break;
                    case PathDirection.SouthEast: message.AddU8(GameClientOpCodes.GoSouthEast); break;
                    default: return;
                }
            } else {
                int pathMaxSteps = (byte)System.Math.Min(byte.MaxValue, pathSteps.Count);

                message.AddU8(GameClientOpCodes.GoPath);
                message.AddU8((byte)pathMaxSteps);
                int i = 0;
                while (i < pathMaxSteps) {
                    message.AddU8((byte)(pathSteps[i] & 65535));
                    i++;
                }
            }

            WriteToOutput(message);
        }
        public void SendStop() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Stop);
            WriteToOutput(message);
        }
        public void SendTurnNorth() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.TurnNorth);
            WriteToOutput(message);
        }
        public void SendTurnEast() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.TurnEast);
            WriteToOutput(message);
        }
        public void SendTurnSouth() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.TurnSouth);
            WriteToOutput(message);
        }
        public void SendTurnWest() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.TurnWest);
            WriteToOutput(message);
        }

        public void SendMoveObject(UnityEngine.Vector3Int sourceAbsolute, uint typeID, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            if (sourceAbsolute.x != 65535)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.MoveObject);
            message.AddPosition(sourceAbsolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPos);
            message.AddPosition(destAbsolute);
            message.AddU8((byte)moveAmount);
            WriteToOutput(message);
        }

        public void SendUseObject(UnityEngine.Vector3Int absolute, uint typeID, int stackPosOrData, int window) {
            if (absolute.x != 65535)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.UseObject);
            message.AddPosition(absolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPosOrData);
            message.AddU8((byte)window); // for containers
            WriteToOutput(message);
        }

        public void SendUseTwoObjects(UnityEngine.Vector3Int firstAbsolute, uint firstID, int firstData, UnityEngine.Vector3Int secondAbsolute, uint secondID, int secondData) {
            if (firstAbsolute.x != 65535 || secondAbsolute.x != 65535)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.UseTwoObjects);
            message.AddPosition(firstAbsolute);
            message.AddU16((ushort)firstID);
            message.AddU8((byte)firstData);
            message.AddPosition(secondAbsolute);
            message.AddU16((ushort)secondID);
            message.AddU8((byte)secondData);
            WriteToOutput(message);
        }

        public void SendUseOnCreature(UnityEngine.Vector3Int absolute, uint typeID, int stackPosOrData, uint creatureID) {
            if (absolute.x != 65535)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.UseOnCreature);
            message.AddPosition(absolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPosOrData);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendTurnObject(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.TurnObject);
            message.AddPosition(absolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPos);
            WriteToOutput(message);
        }

        public void SendToggleWrapState(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.ToggleWrapState);
            message.AddU8(GameClientOpCodes.ToggleWrapState);
            message.AddPosition(absolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPos);
            WriteToOutput(message);
        }

        public void SendLook(UnityEngine.Vector3Int absolute, uint typeID, int stackPos) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Look);
            message.AddPosition(absolute);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPos);
            WriteToOutput(message);
        }

        public void SendLookAtCreature(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Look);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendJoinAggression(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.JoinAggression);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendTalk(MessageModes mode, params object[] rest) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Talk);
            message.AddU8(TranslateMessageModeToServer(mode));
            
            if (rest.Length == 1 && rest[0] is string) {
                message.AddString(rest[0] as string);
            } else if (rest.Length == 2 && rest[0] is string && rest[1] is string) {
                message.AddString(rest[0] as string);
                message.AddString(rest[1] as string);
            } else if (rest.Length == 2 && rest[0] is int && rest[1] is string) {
                int channelID = (int)rest[0];
                message.AddU16((ushort)channelID);
                message.AddString(rest[1] as string);
            }

            WriteToOutput(message);
        }
        public void SendGetChannels() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.GetChannels);
            WriteToOutput(message);
        }
        public void SendJoinChannel(int channelID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.JoinChannel);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }
        public void SendLeaveChannel(int channelID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.LeaveChannel);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }

        public void SendCloseNPCChannel() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.CloseNPCChannel);
            WriteToOutput(message);
        }

        public void SendSetTactics() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.SetTactics);
            message.AddU8((byte)optionStorage.CombatAttackMode);
            message.AddU8((byte)optionStorage.CombatChaseMode);
            message.AddU8(optionStorage.CombatSecureMode ? (byte)1 : (byte)0);
            message.AddU8((byte)optionStorage.CombatPvPMode);
            WriteToOutput(message);
        }

        public void SendAttack(uint creatureID) {
            if (creatureID != 0)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Attack);
            message.AddU32(creatureID);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameAttackSeq))
                message.AddU32(creatureID); // Sequence
            WriteToOutput(message);
        }

        public void SendFollow(uint creatureID) {
            if (creatureID != 0)
                m_Player.StopAutowalk(false);

            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Follow);
            message.AddU32(creatureID);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameAttackSeq))
                message.AddU32(creatureID); // Sequence
            WriteToOutput(message);
        }

        public void SendInviteToParty(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.InviteToParty);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendJoinParty(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.JoinParty);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendRevokeInvitation(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.RevokeInvitation);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendPassLeadership(uint creatureID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.PassLeadership);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendLeaveParty() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.LeaveParty);
            WriteToOutput(message);
        }

        public void SendShareExperience(bool shared) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.ShareExperience);
            message.AddBool(shared);
            WriteToOutput(message);
        }

        public void SendInviteToChannel(string name, int channelID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.InviteToChannel);
            message.AddString(name);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }

        public void SendExcludeFromChannel(string name, int channelID) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.ExcludeFromChannel);
            message.AddString(name);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }

        public void SendCancel() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Cancel);
            WriteToOutput(message);
        }

        public void SendBrowseField(UnityEngine.Vector3Int absolute) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.BrowseField);
            message.AddPosition(absolute);
            WriteToOutput(message);
        }

        public void SendGetOutfit() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.GetOutfit);
            WriteToOutput(message);
        }

        public void SendMount(bool toggle) {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.Mount);
            message.AddBool(toggle);
            WriteToOutput(message);
        }

        public void SendGetQuestLog() {
            var message = new OutputMessage();
            message.AddU8(GameClientOpCodes.GetQuestLog);
            WriteToOutput(message);
        }
    }
}
