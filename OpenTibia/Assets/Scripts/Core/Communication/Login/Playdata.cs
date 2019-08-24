using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public class Playdata
    {
        public class World
        {
            public int _id = 0;

            public string Name = string.Empty;
            public string ExternalAddress = string.Empty;
            public string ExternalAddressProtected = string.Empty;
            public string ExternalAddressUnprotected = string.Empty;

            public int PreviewState = 0;
            public int PvpType = 0;

            public int ExternalPort = 0;
            public int ExternalPortProtected = 0;
            public int ExternalPortUnprotected = 0;

            public int CurrentTournamentPhase = 0;

            public bool AntiCheatProtection = true;
            public bool IsTournamentActive = true;
            public bool IsTournamentWorld = true;
            public bool RestrictStore = true;

            public string GetAddress(int clientVersion, int buildVersion) {
                if (clientVersion >= 1148) {
                    if (AntiCheatProtection)
                        return ExternalAddressProtected;
                    else if (clientVersion >= 1149 && buildVersion >= 5921)
                        return ExternalAddressProtected;
                }

                return ExternalAddress;
            }

            public int GetPort(int clientVersion, int buildVersion) {
                if (clientVersion >= 1148) {
                    if (AntiCheatProtection)
                        return ExternalPortProtected;
                    else if (clientVersion >= 1149 && buildVersion >= 5921)
                        return ExternalPortUnprotected;
                }

                return ExternalPort;
            }

            public string GetPvPTypeDescription() {
                switch (PvpType) {
                    case 0: return "Open pvp";
                    case 1: return "Optional pvp";
                    case 2: return "Harcore pvp";
                    case 3: return "Retro Open pvp";
                    case 4: return "Retro Hardcore pvp";
                    default: return "unknown pvp type";
                }
            }
        }

        public class Character
        {
            public int WorldId = 0;
            public int Level = 0;
            public int OutfitId = 0;
            public int HeadColor = 0;
            public int TorsoColor = 0;
            public int LegsColor = 0;
            public int DetailColor = 0;
            public int AddonsFlags = 0;
            public int RemainingDailyTournamentPlaytime = 0;

            public string Name = string.Empty;
            public string Vocation = string.Empty;

            public bool IsMale = false;
            public bool Tutorial = false;
            public bool IsHidden = false;
            public bool IsTournamentParticipant = false;
        }

        public List<World> Worlds = new List<World>();
        public List<Character> Characters = new List<Character>();

        public World FindWorld(int id) {
            return Worlds.Find((x) => x._id == id);
        }

        public World FindWorld(string name) {
            return Worlds.Find((x) => x.Name == name);
        }
    }
}
