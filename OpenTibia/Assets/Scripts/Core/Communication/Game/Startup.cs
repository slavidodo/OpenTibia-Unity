using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    internal partial class ProtocolGame : Internal.Protocol
    {
        internal ushort BeatDuration = 0;

        internal bool BugReportsAllowed = false;

        private void ParseGmActions(Internal.ByteArray message) {
            int numViolationReasons = 20;
            var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;
            if (clientVersion >= 850)
                numViolationReasons = 20;
            else if (clientVersion >= 840)
                numViolationReasons = 23;
            else
                numViolationReasons = 32;

            List<byte> actions = new List<byte>();
            for (int i = 0; i < numViolationReasons; i++) {
                actions.Add(message.ReadUnsignedByte());
            }
        }

        private void ParseReadyForSecondaryConnection(Internal.ByteArray message) {
            var sessionKey = message.ReadString();
        }

        private void ParseWorldEntered(Internal.ByteArray message) {
            bool hasLoginPendingFeature = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPending);
            if ((hasLoginPendingFeature && m_ConnectionState == ConnectionState.Pending)
                || (!hasLoginPendingFeature && m_ConnectionState > ConnectionState.Disconnected && m_ConnectionState != ConnectionState.Game)) {
                MiniMapStorage.Position = Vector3Int.zero;
                WorldMapStorage.Position = Vector3Int.zero;
                WorldMapStorage.ResetMap();
                CreatureStorage.Reset();
                WorldMapStorage.Valid = false;
            }
            
            SetConnectionState(ConnectionState.Game);
        }
        private void ParseLoginError(Internal.ByteArray message) {
            string error = message.ReadString();
            onLoginError.Invoke(error);
        }
        private void ParseLoginAdvice(Internal.ByteArray message) {
            string advice = message.ReadString();
            onLoginAdvice.Invoke(advice);
        }
        private void ParseLoginWait(Internal.ByteArray message) {
            string waitMessage = message.ReadString();
            int waitTime = message.ReadUnsignedByte();
            onLoginWait.Invoke(waitMessage, waitTime);
        }
        private void ParseLoginSuccess(Internal.ByteArray message) {
            Player.ID = message.ReadUnsignedInt();
#if !UNITY_EDITOR
            string title = string.Format("{0} - {1}", Application.productName, Player.Name);
            OpenTibiaUnity.GameManager.SetApplicationTitle(title);
#endif

            BeatDuration = message.ReadUnsignedShort();
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewSpeedLaw)) {
                Creatures.Creature.SpeedA = message.ReadDouble();
                Creatures.Creature.SpeedB = message.ReadDouble();
                Creatures.Creature.SpeedC = message.ReadDouble();
            }

            BugReportsAllowed = message.ReadBoolean();
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1054) {
                bool canChangePvPFrameRate = message.ReadBoolean();
            }

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1058) {
                bool exportPvPEnabled = message.ReadBoolean();
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStore)) {
                string storeLink = message.ReadString();
                ushort storePackageSize = message.ReadUnsignedShort();
            }

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1149 && OpenTibiaUnity.GameManager.BuildVersion >= 6018) {
                bool exivaRestrictions = message.ReadBoolean();
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameTournament)) {
                bool tournamentActivated = message.ReadBoolean();
            }
        }

        private void ParseLoginToken(Internal.ByteArray message) {
            /*byte unknown = */message.ReadUnsignedByte();
        }

        private void ParseChallange(Internal.ByteArray message) {
            uint timestamp = message.ReadUnsignedInt();
            byte challange = message.ReadUnsignedByte();

            SendLogin(timestamp, challange);
            SetConnectionState(ConnectionState.ConnectingStage2, false);
        }
    }
}
