using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public class PlayData
    {
        public class PlayDataSession
        {
            public uint LastLoginTime = 0;
            public uint PremiumUntil = 0;

            public string Key = string.Empty;
            public string Status = string.Empty;

            // legay strings
            public string AccountName = string.Empty;
            public string Password = string.Empty;
            public string Token = string.Empty;

            public bool IsPremium = false;
            public bool InfinitePremium = false;
            public bool FpsTracking = false;
            public bool IsReturner = false;
            public bool ReturnerNotification = false;
            public bool ShowRewardNews = false;
            public bool OptionTracking = false;
        }

        public class PlayDataWorld
        {
            public int Id = 0;

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

            public bool AntiCheatProtection = false;
            public bool IsTournamentActive = false;
            public bool IsTournamentWorld = false;
            public bool IsMainCharacter = false;
            public bool RestrictStore = false;

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

        public class PlayDataCharacter
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

        public PlayDataSession Session = new PlayDataSession();
        public List<PlayDataWorld> Worlds = new List<PlayDataWorld>();
        public List<PlayDataCharacter> Characters = new List<PlayDataCharacter>();

        public PlayDataWorld FindWorld(int id) {
            return Worlds.Find((x) => x.Id == id);
        }

        public PlayDataWorld FindWorld(string name) {
            return Worlds.Find((x) => x.Name == name);
        }
    }
}
