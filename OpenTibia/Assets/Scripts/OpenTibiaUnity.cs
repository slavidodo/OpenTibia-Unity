namespace OpenTibiaUnity
{
    internal static class OpenTibiaUnity
    {
        internal static int TicksMillis { get => (int)(UnityEngine.Time.time * 1000); }
        internal static int TicksSeconds { get => (int)UnityEngine.Time.time; }
        internal static System.Threading.Thread MainThread { get; set; }
        internal static string GraphicsVendor { get; set; }
        internal static string GraphicsDevice { get; set; }
        internal static string GraphicsVersion { get; set; }
        internal static int StartupTimeMillis { get; set; } = 0;
        internal static int DeltaTimeMillis { get => (int)(UnityEngine.Time.deltaTime * 1000); }
        internal static bool Quiting { get; set; }

        internal static Core.GameManager GameManager { get => Core.GameManager.Instance; }
        internal static Core.Communication.Game.ProtocolGame ProtocolGame {
            get => GameManager?.ProtocolGame;
            set { if (GameManager != null) GameManager.ProtocolGame = value; }
        }
        internal static UnityEngine.UI.GraphicRaycaster ActiveRaycaster { get => GameManager?.ActiveRaycaster; }
        internal static UnityEngine.EventSystems.EventSystem EventSystem { get => GameManager?.EventSystem; }
        internal static Core.Creatures.CreatureStorage CreatureStorage { get => GameManager?.CreatureStorage; }
        internal static Core.Creatures.Player Player { get => CreatureStorage?.Player; }
        internal static Core.Appearances.AppearanceStorage AppearanceStorage { get => GameManager?.AppearanceStorage; }
        internal static Core.WorldMap.WorldMapStorage WorldMapStorage { get => GameManager?.WorldMapStorage; }
        internal static Core.WorldMap.Rendering.WorldMapRenderer WorldMapRenderer { get => GameManager?.WorldMapRenderer; }
        internal static Core.MiniMap.MiniMapStorage MiniMapStorage { get => GameManager?.MiniMapStorage; }
        internal static Core.MiniMap.Rendering.MiniMapRenderer MiniMapRenderer { get => GameManager?.MiniMapRenderer; }
        internal static Core.Options.OptionStorage OptionStorage { get => GameManager?.OptionStorage; }
        internal static Core.Input.InputHandler InputHandler { get => GameManager?.InputHandler; }
        internal static Core.Chat.ChatStorage ChatStorage { get => GameManager?.ChatStorage; }
        internal static Core.Chat.MessageStorage MessageStorage { get => GameManager?.MessageStorage; }
        internal static Core.Container.ContainerStorage ContainerStorage { get => GameManager?.ContainerStorage; }
        internal static Core.Magic.SpellStorage SpellStorage { get => GameManager?.SpellStorage; }

        internal static int[] GetClientVersions() {
            return new int[] {
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
            1098, 1099,

            1149,
            1150,
            1200, 1201, 1202, 1203, 1205,
            1206, 1207, 1208, 1209, 1210,
            1211, 1212, 1215, 1220

            // 1146 / p: 1145
            // 1148 / p: 1148
            // 1149 / p: 1148
            // 1150 / p: 1150
            // 1156 / p: 1150
        };
        }

        internal static int GetMinimumClientVersion() => 740;
        internal static int GetMaximumClientVersion() => 1215;

        internal static int GetProtocolVersion(int clientVersion, int buildVersion) {
            switch (clientVersion) {
                case 980: return 971;
                case 981: return 973;
                case 982: return 974;
                case 983: return 975;
                case 984: return 976;
                case 985: return 977;
                case 986: return 978;
                case 1001: return 979;
                case 1002: return 980;
                case 1101: return 1100;
                case 1102:
                case 1103:
                case 1104: return 1101;
                case 1121: return 1120;
                case 1141:
                case 1142: return 1140;
                case 1143:
                case 1144: return 1141;
                case 1146:
                case 1147: return 1145;
                case 1149: return 1148;
                case 1156: return 1150;
                /* TODO until tibia 12.00, it's unknown */
                default: return clientVersion;
            }
        }

        internal static int GetMinimumProtocolVersion() => 740;
        internal static int GetMaximumProtocolVersion() => 1215;

        internal static int[] GetBuildVersions(int clientVersion) {
            if (clientVersion < 1100)
                return null;

            switch (clientVersion) {
                case 1100: return new int[] { 3768, 3787, 3813, 3831, 3839, 3862, 3878, 3913, 3947, 3953 };
                case 1101: return new int[] { 3987, 4015, 4078, 4098 };
                case 1102: return new int[] { 4128, 4171, 4180 };
                case 1103: return new int[] { 4221, 4245, 4246 };
                case 1104: return new int[] { 4312, 4320, 4347, 4363, 4366, 4384, 4391 };
                case 1110: return new int[] { 4427, 4445, 4472, 4527, 4562 };
                case 1111: return new int[] { 4601 };
                case 1120: return new int[] { 4691, 4732, 4789 };
                case 1121: return new int[] { 4812 };
                case 1130: return new int[] { 4868, 4876, 4898, 4948, 5036 };
                case 1131: return new int[] { 5138 };
                case 1132: return new int[] { 5206, 5229, 5246, 5287, 5341 };
                case 1134: return new int[] { 5384 };
                case 1135: return new int[] { 5388 };
                case 1140: return new int[] { 5409 };
                case 1141: return new int[] { 5435 };
                case 1142: return new int[] { 5478 };
                case 1143: return new int[] { 5504 };
                case 1144: return new int[] { 5516 };
                case 1145: return new int[] { 5528 };
                case 1146: return new int[] { 5537, 5556 };
                case 1147: return new int[] { 5602, 5620, 5640, 5674 };
                case 1148: return new int[] { 5712, 5753 };
                case 1149: return new int[] { 5813, 5884, 5921, 5983, 6018, 6030 };
                case 1150: return new int[] { 6055 };
                case 1151: return new int[] { 6099 };
                case 1152: return new int[] { 6104 };
                case 1153: return new int[] { 6154 };
                case 1154: return new int[] { 6216 };
                case 1155: return new int[] { 6239 };
                case 1156: return new int[] { 6239 };
                case 1157: return new int[] { 6239 };
                case 1158: return new int[] { 6239 };
                case 1159: return new int[] { 6413, 6424 };
                case 1160: return new int[] { 6457 };
                case 1165: return new int[] { 6492, 6507 };
                case 1166: return new int[] { 6516 };
                case 1170: return new int[] { 6521, 6535, 6543, 6548, 6555 };
                case 1171: return new int[] { 6562, 6571 };
                case 1172: return new int[] { 6598, 6646, 6698 };
                case 1173: return new int[] { 6703 };
                case 1174: return new int[] { 6781 };
                case 1175: return new int[] { 6785, 6829, 6837, 6922, 6942, 6969, 6980 };
                case 1180: return new int[] { 7048, 7086 };
                case 1181: return new int[] { 7107, 7119 };
                case 1182: return new int[] { 7127 };
                case 1183: return new int[] { 7129 };
                case 1185: return new int[] { 7148 };
                case 1186: return new int[] { 7197, 7234, 7253 };
                case 1187: return new int[] { 7266 };
                case 1188: return new int[] { 7288 };
                case 1190: return new int[] { 7293 };
                case 1191: return new int[] { 7346, 7423 };
                case 1192: return new int[] { 7441 };
                case 1193: return new int[] { 7495 };
                case 1194: return new int[] { 7495, 7595, 7611 };

                // Tibia 12
                case 1200: return new int[] { 7695 };
                case 1201: return new int[] { 7724 };
                case 1202: return new int[] { 7746 };
                case 1203: return new int[] { 7790, 7844 };
                case 1205: return new int[] { 7904 };
                case 1206: return new int[] { 7941, 7958 };
                case 1207: return new int[] { 7973 };
                case 1208: return new int[] { 7995, 8034, 8046 };
                case 1209: return new int[] { 8053 };
                case 1210: return new int[] { 8084 };
                case 1211: return new int[] { 8154 };
                case 1212: return new int[] { 8202, 8266, 8334, 8413 };
                case 1215: return new int[] { 8493, 8554, 8610, 8659, 8706, 8721, 8762, 8768, 8786, 8788, 8794, 8795, 8800, 8802, 8823 };
                case 1220: return new int[] { };
                default: return null;
            }
        }

        internal static int GetMinimumBuildVersion() => 3768;
        internal static int GetMaximumBuildVersion() => 8823;

        internal static ushort GetContentRevision(int clientVersion, int buildVersion) {
            switch (clientVersion) {
                case 1104: return 17718;
                case 1110: return 17782;
                case 1132: return 18492;
                case 1143: return 18711;
                case 1144: return 18736;
                case 1146: return 18774;
                case 1147: {
                    if (buildVersion == 5620)
                        return 19299;
                    return 18774;
                }
                case 1148: return 18774;
                case 1149: return 18960;
                case 1150: return 19304;
                case 1200: return 41880;
                case 1212: return 22097;
                case 1215:
                    switch (buildVersion) {
                        case 8493: return 22268;
                        case 8554: case 8610: return 22362;
                        case 8659: return 22463;
                        case 8706: return 22570;
                        case 8721: return 22632;
                        case 8762: return 22644;
                        case 8768: return 22655;
                        case 8786: return 22700;
                        case 8788: case 8794: return 22644;
                        case 8795: return 22807;
                        default: return 0;
                    }
            }

            return 0;
        }

        internal static bool IsTestBuildVersion(int buildVersion) {
            switch (buildVersion) {
                case 8768:
                case 8786:
                case 8800:
                case 8802:
                case 8823:
                    return true;

                default:
                    return false;
            }
        }

        internal static bool IsRunningInEditor {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        internal static Core.Components.ObjectContextMenu CreateObjectContextMenu(UnityEngine.Vector3Int absolute,
            Core.Appearances.ObjectInstance lookObject, int lookObjectStackPos,
            Core.Appearances.ObjectInstance useObject, int useObjectStackPos,
            Core.Creatures.Creature creature) {

            var canvas = GameManager.ActiveCanvas;
            var gameObject = UnityEngine.Object.Instantiate(GameManager.ContextMenuBasePrefab, canvas.transform);

            var objectContextMenu = gameObject.AddComponent<Core.Components.ObjectContextMenu>();
            objectContextMenu.Set(absolute, lookObject, lookObjectStackPos, useObject, useObjectStackPos, creature);
            return objectContextMenu;
        }
    }
}
