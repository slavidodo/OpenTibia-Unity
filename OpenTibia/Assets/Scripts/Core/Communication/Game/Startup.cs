using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame
    {
        public ushort BeatDuration = 0;

        public bool BugReportsAllowed = false;

        private void ParseGmActions(Internal.CommunicationStream message) {
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

        private void ParseReadyForSecondaryConnection(Internal.CommunicationStream message) {
            var sessionKey = message.ReadString();
        }

        private void ParseWorldEntered(Internal.CommunicationStream message) {
            bool hasLoginPendingFeature = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPending);
            if ((hasLoginPendingFeature && ConnectionState == ConnectionState.Pending)
                || (!hasLoginPendingFeature && ConnectionState > ConnectionState.Disconnected && ConnectionState != ConnectionState.Game)) {
                MiniMapStorage.Position = Vector3Int.zero;
                WorldMapStorage.Position = Vector3Int.zero;
                WorldMapStorage.ResetMap();
                CreatureStorage.Reset();
                WorldMapStorage.Valid = false;
            }
            
            SetConnectionState(ConnectionState.Game);
        }
        private void ParseLoginError(Internal.CommunicationStream message) {
            string error = message.ReadString();
            onLoginError.Invoke(error);
        }
        private void ParseLoginAdvice(Internal.CommunicationStream message) {
            string advice = message.ReadString();
            onLoginAdvice.Invoke(advice);
        }
        private void ParseLoginWait(Internal.CommunicationStream message) {
            string waitMessage = message.ReadString();
            int waitTime = message.ReadUnsignedByte();
            onLoginWait.Invoke(waitMessage, waitTime);
        }
        private void ParseLoginSuccess(Internal.CommunicationStream message) {
            Player.Id = message.ReadUnsignedInt();

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            string title = string.Format("{0} - {1}", Application.productName, CharacterName);
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

        private void ParseLoginToken(Internal.CommunicationStream message) {
            /*byte unknown = */message.ReadUnsignedByte();
        }

        private void ParseChallange(Internal.CommunicationStream message) {
            uint timestamp = message.ReadUnsignedInt();
            byte challange = message.ReadUnsignedByte();

            SendLogin(timestamp, challange);
            SetConnectionState(ConnectionState.ConnectingStage2, false);
        }
    }
}
