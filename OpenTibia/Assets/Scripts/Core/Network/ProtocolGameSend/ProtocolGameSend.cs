using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private int m_PingSent = 0;
        private int m_Ping = -1;
        private System.Diagnostics.Stopwatch m_PingStopwatch = new System.Diagnostics.Stopwatch();

        public int Ping {
            get { return m_Ping; }
        }

        // TODO: use a pool instead
        // and confirm the encryption of the packet ONLY on a successful sent
        // This is working, but using a pool is more of a robust

        public void SendLoginPacket(uint challengeTimestamp, byte challengeRandom) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.PendingGame);
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
        private void SendEnterGame() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.EnterGame);
            WriteToOutput(message);
        }
        public void SendLeaveGame() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.LeaveGame);
            WriteToOutput(message);
        }
        public void SendPing() {
            m_Ping = (int)m_PingStopwatch.ElapsedMilliseconds;

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Ping);
            WriteToOutput(message);

            m_PingSent++;
            m_PingStopwatch.Restart();
        }
        public void SendPingBack() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.PingBack);
            WriteToOutput(message);
        }
        

        public void SendGo(List<int> pathSteps) {
            if (pathSteps == null)
                return;
            
            m_CreatureStorage.ClearTargets();
            OutputMessage message = new OutputMessage();
            if (pathSteps.Count == 1) {
                switch ((PathDirection)(pathSteps[0] & 65535)) {
                    case PathDirection.East: message.AddU8(ClientServerOpCodes.GoEast); break;
                    case PathDirection.NorthEast: message.AddU8(ClientServerOpCodes.GoNorthEast); break;
                    case PathDirection.North: message.AddU8(ClientServerOpCodes.GoNorth); break;
                    case PathDirection.NorthWest: message.AddU8(ClientServerOpCodes.GoNorthWest); break;
                    case PathDirection.West: message.AddU8(ClientServerOpCodes.GoWest); break;
                    case PathDirection.SouthWest: message.AddU8(ClientServerOpCodes.GoSouthWest); break;
                    case PathDirection.South: message.AddU8(ClientServerOpCodes.GoSouth); break;
                    case PathDirection.SouthEast: message.AddU8(ClientServerOpCodes.GoSouthEast); break;
                    default: return;
                }
            } else {
                int pathMaxSteps = (byte)System.Math.Min(byte.MaxValue, pathSteps.Count);

                message.AddU8(ClientServerOpCodes.GoPath);
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
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Stop);
            WriteToOutput(message);
        }
        public void SendTurnNorth() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.TurnNorth);
            WriteToOutput(message);
        }
        public void SendTurnEast() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.TurnEast);
            WriteToOutput(message);
        }
        public void SendTurnSouth() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.TurnSouth);
            WriteToOutput(message);
        }
        public void SendTurnWest() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.TurnWest);
            WriteToOutput(message);
        }

        public void SendUseObject(UnityEngine.Vector3Int position, uint typeID, int positionOrData, int window) {
            if (position.x != 65535)
                m_Player.StopAutowalk(false);

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.UseObject);
            message.AddPosition(position);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)positionOrData);
            message.AddU8((byte)window); // for containers
            WriteToOutput(message);
        }

        public void SendUseTwoObjects(UnityEngine.Vector3Int firstPosition, uint firstID, int firstData, UnityEngine.Vector3Int secondPosition, uint secondID, int secondData) {
            if (firstPosition.x != 65535 || secondPosition.x != 65535)
                m_Player.StopAutowalk(false);

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.UseTwoObject);
            message.AddPosition(firstPosition);
            message.AddU16((ushort)firstID);
            message.AddU8((byte)firstData);
            message.AddPosition(secondPosition);
            message.AddU16((ushort)secondID);
            message.AddU8((byte)secondData);
            WriteToOutput(message);
        }

        public void SendUseOnCreature(UnityEngine.Vector3Int position, uint typeID, int positionOrData, uint creatureID) {
            if (position.x != 65535)
                m_Player.StopAutowalk(false);

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.UseObject);
            message.AddPosition(position);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)positionOrData);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendLook(UnityEngine.Vector3Int position, uint typeID, int stackPosition) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Look);
            message.AddPosition(position);
            message.AddU16((ushort)typeID);
            message.AddU8((byte)stackPosition);
            WriteToOutput(message);
        }

        public void SendLookAtCreature(uint creatureID) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Look);
            message.AddU32(creatureID);
            WriteToOutput(message);
        }

        public void SendTalk(MessageModes mode, params object[] rest) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Talk);
            message.AddU8((byte)mode);
            
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
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.GetChannels);
            WriteToOutput(message);
        }
        public void SendJoinChannel(int channelID) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.JoinChannel);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }
        public void SendLeaveChannel(int channelID) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.LeaveChannel);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }

        public void SendCloseNPCChannel() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.CloseNPCChannel);
            WriteToOutput(message);
        }

        public void SendSetTactics() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.SetTactics);
            message.AddU8((byte)optionStorage.CombatAttackMode);
            message.AddU8((byte)optionStorage.CombatChaseMode);
            message.AddU8(optionStorage.CombatSecureMode ? (byte)1 : (byte)0);
            message.AddU8((byte)optionStorage.CombatPvPMode);
            WriteToOutput(message);
        }

        public void SendAttack(uint creatureID) {
            if (creatureID != 0) {
                m_Player.StopAutowalk(false);
            }

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Attack);
            message.AddU32(creatureID);
            message.AddU32(creatureID); // Sequence
            WriteToOutput(message);
        }

        public void SendFollow(uint creatureID) {
            if (creatureID != 0) {
                m_Player.StopAutowalk(false);
            }

            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Follow);
            message.AddU32(creatureID);
            message.AddU32(creatureID); // Sequence
            WriteToOutput(message);
        }

        public void SendInviteToChannel(string name, int channelID) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.InviteToChannel);
            message.AddString(name);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }
        public void SendExcludeFromChannel(string name, int channelID) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.ExcludeFromChannel);
            message.AddString(name);
            message.AddU16((ushort)channelID);
            WriteToOutput(message);
        }

        public void SendCancel() {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Cancel);
            WriteToOutput(message);
        }

        public void SendMount(bool toggle) {
            OutputMessage message = new OutputMessage();
            message.AddU8(ClientServerOpCodes.Mount);
            message.AddBool(toggle);
            WriteToOutput(message);
        }
    }
}
