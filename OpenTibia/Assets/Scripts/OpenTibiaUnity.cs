namespace OpenTibiaUnity
{
    public static class OpenTibiaUnity
    {
        public static int TicksMillis { get { return (int)(UnityEngine.Time.time * 1000); } }
        public static int TicksSeconds { get { return (int)UnityEngine.Time.time; } }
        public static System.Threading.Thread MainThread { get; internal set; }
        public static string GraphicsVendor { get; internal set; }
        public static string GraphicsDevice { get; internal set; }
        public static string GraphicsVersion { get; internal set; }
        public static int StartupTimeMillis { get; internal set; } = 0;
        public static int DeltaTimeMillis { get { return (int)(UnityEngine.Time.deltaTime * 1000); } }
        public static bool Quiting { get; internal set; }

        public static Core.GameManager GameManager { get {return Core.GameManager.Instance; } }
        public static Core.Network.ProtocolGame ProtocolGame {
            get { return GameManager?.ProtocolGame; }
            set { if (GameManager != null) GameManager.ProtocolGame = value; }
        }
        public static Core.Creatures.CreatureStorage CreatureStorage { get { return GameManager?.CreatureStorage; } }
        public static Core.Creatures.Player Player { get { return CreatureStorage?.Player; } }
        public static Core.Appearances.AppearanceStorage AppearanceStorage { get { return GameManager?.AppearanceStorage; } }
        public static Core.WorldMap.WorldMapStorage WorldMapStorage { get { return GameManager?.WorldMapStorage; } }
        public static Core.WorldMap.Rendering.WorldMapRenderer WorldMapRenderer { get { return GameManager?.WorldMapRenderer; } }
        public static Core.MiniMap.MiniMapStorage MiniMapStorage { get { return GameManager?.MiniMapStorage; } }
        public static Core.MiniMap.Rendering.MiniMapRenderer MiniMapRenderer { get { return GameManager?.MiniMapRenderer; } }
        public static Core.Options.OptionStorage OptionStorage { get { return GameManager?.OptionStorage; } }
        public static Core.InputManagment.InputHandler InputHandler { get { return GameManager?.InputHandler; } }
        public static Core.Chat.ChatStorage ChatStorage { get { return GameManager?.ChatStorage; } }
        public static Core.Chat.MessageStorage MessageStorage { get { return GameManager?.MessageStorage; } }
        public static Core.Container.ContainerStorage ContainerStorage { get { return GameManager?.ContainerStorage; } }

        public static int[] GetSupportedVersions() => SupportedVersions;
        private static int[] SupportedVersions => new int[] {
                740, 741, 750, 760, 770, 772,
                780, 781, 782, 790, 792,
            
                800, 810, 811, 820, 821, 822,
                830, 831, 840, 842, 850, 853,
                854, 855, 857, 860, 861, 862,
                870, 871,
            
                900, 910, 920, 931, 940, 943,
                944, 951, 952, 953, 954, 960,
                961, 963, 970, 971, 972, 973,
                980, 981, 982, 983, 984, 985,
                986,
            
                1000, 1001, 1002, 1010, 1011,
                1012, 1013, 1020, 1021, 1022,
                1030, 1031, 1032, 1033, 1034,
                1035, 1036, 1037, 1038, 1039,
                1040, 1041, 1050, 1051, 1052,
                1053, 1054, 1055, 1056, 1057,
                1058, 1059, 1060, 1061, 1062,
                1063, 1064, 1070, 1071, 1072,
                1073, 1074, 1075, 1076, 1080,
                1081, 1082, 1090, 1091, 1092,
                1093, 1094, 1095, 1096, 1097,
                1098, 1099
        };

        public static bool IsRunningInEditor {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }
    }
}
