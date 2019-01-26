using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private bool m_BugreportsAllowed = false;

        private void ParseGmActions(InputMessage message) {
            int numViolations = 20;

            List<byte> actions = new List<byte>();
            for (int i = 0; i < numViolations; i++) {
                actions.Add(message.GetU8());
            }
        }
        private void ParseWorldEntered(InputMessage message) {
            m_MiniMapStorage.Position = new Vector3Int(0, 0, 0);
            m_WorldMapStorage.Position = new Vector3Int(0, 0, 0);
            m_WorldMapStorage.ResetMap();
            m_CreatureStorage.Reset();
            m_WorldMapStorage.Valid = false;
        }
        private void ParseLoginError(InputMessage message) {
            string error = message.GetString();
        }
        private void ParseLoginAdvice(InputMessage message) {
            string messageStr = message.GetString();
        }
        private void ParseLoginWait(InputMessage message) {
            string waitMessage = message.GetString();
            int time = message.GetU8();
        }
        private void ParseLoginSuccess(InputMessage message) {
            m_Player.ID = message.GetU32();
            m_Player.Name = CharacterName;

#if !UNITY_EDITOR
            string title = string.Format("OpenTibiaUnity - {0}", m_Player.Name);
            OpenTibiaUnity.GameManager.SetApplicationTitle(title);
#endif

            BeatDuration = message.GetU16();
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameNewSpeedLaw)) {
                Creatures.Creature.SpeedA = message.GetDouble();
                Creatures.Creature.SpeedB = message.GetDouble();
                Creatures.Creature.SpeedC = message.GetDouble();
            }

            m_BugreportsAllowed = message.GetBool();

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1054) {
                bool canChangePvPFrameRate = message.GetBool();
            }

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1058) {
                bool exportPvPEnabled = message.GetBool();
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameIngameStore)) {
                string storeLink = message.GetString();
                ushort storePackageSize = message.GetU16();
            }
        }

        private void ParseLoginToken(InputMessage message) {
            /*byte unknown = */message.GetU8();
        }

        private void ParseChallange(InputMessage message) {
            uint timestamp = message.GetU32();
            byte challange = message.GetU8();

            SendLoginPacket(timestamp, challange);
        }
    }
}
