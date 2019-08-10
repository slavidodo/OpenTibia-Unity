using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Login
{
    internal class Playdata
    {
        internal class World
        {
            internal int ID = 0;

            internal string Name = string.Empty;
            internal string ExternalAddress = string.Empty;
            internal string ExternalAddressProtected = string.Empty;
            internal string ExternalAddressUnprotected = string.Empty;

            internal int PreviewState = 0;
            internal int PvpType = 0;

            internal int ExternalPort = 0;
            internal int ExternalPortProtected = 0;
            internal int ExternalPortUnprotected = 0;

            internal int CurrentTournamentPhase = 0;

            internal bool AntiCheatProtection = true;
            internal bool IsTournamentActive = true;
            internal bool IsTournamentWorld = true;
            internal bool RestrictStore = true;

            internal string GetAddress(int clientVersion, int buildVersion) {
                if (clientVersion >= 1148) {
                    if (AntiCheatProtection)
                        return ExternalAddressProtected;
                    else if (clientVersion >= 1149 && buildVersion >= 5921)
                        return ExternalAddressProtected;
                }

                return ExternalAddress;
            }

            internal int GetPort(int clientVersion, int buildVersion) {
                if (clientVersion >= 1148) {
                    if (AntiCheatProtection)
                        return ExternalPortProtected;
                    else if (clientVersion >= 1149 && buildVersion >= 5921)
                        return ExternalPortUnprotected;
                }

                return ExternalPort;
            }

            internal string GetPvPTypeDescription() {
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

        internal class Character
        {
            internal int WorldID = 0;
            internal int Level = 0;
            internal int OutfitID = 0;
            internal int HeadColor = 0;
            internal int TorsoColor = 0;
            internal int LegsColor = 0;
            internal int DetailColor = 0;
            internal int AddonsFlags = 0;
            internal int RemainingDailyTournamentPlaytime = 0;

            internal string Name = string.Empty;
            internal string Vocation = string.Empty;

            internal bool IsMale = false;
            internal bool Tutorial = false;
            internal bool IsHidden = false;
            internal bool IsTournamentParticipant = false;
        }

        internal List<World> Worlds = new List<World>();
        internal List<Character> Characters = new List<Character>();

        internal World FindWorld(int id) {
            return Worlds.Find((x) => x.ID == id);
        }

        internal World FindWorld(string name) {
            return Worlds.Find((x) => x.Name == name);
        }
    }
}
