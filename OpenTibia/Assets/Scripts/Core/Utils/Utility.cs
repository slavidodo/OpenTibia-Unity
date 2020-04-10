using UnityEngine;

namespace OpenTibiaUnity.Core.Utils
{
    public enum OperatingSystem
    {
        None = 0,
        DeprecatedLinux = 1,
        DeprecatedWindows = 2,
        DeprecatedFlash = 3,

        CipsoftLinux = 4,
        CipsoftWindows = 5,
        CipsoftMacOS = 6,

        OTClientLinux = 10,
        OTClientWindows = 11,
        OTClientMax = 12,

        UnityLinux = 20,
        UnityWindows = 21,
        UnityWindowsUniversal = 22,
        UnityOSX = 23,
        UnityIOS = 24,
        UnityAndroid = 25,
        UnityUnknown = 26,
    }
    
    public static class Utility
    {
        public static int ManhattanDistance(Vector3Int position, Vector3Int other) {
            return System.Math.Abs(position.x - other.x) + System.Math.Abs(position.y - other.y);
        }

        public static float Distance(Vector3Int position, Vector3Int other) {
            return (float)System.Math.Sqrt(System.Math.Pow(position.x - other.x, 2) + System.Math.Pow(position.y - other.y, 2));
        }

        public static Color32 MulColor32(Color32 c, byte f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        public static Color32 MulColor32(Color32 c, float f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        public static Color32 MulColor32(Color32 c, int f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }


        public static Rect Intersect(Rect a, Rect b) {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMax = Mathf.Min(a.yMax, b.yMax);
            if (xMax > xMin && yMax > yMin)
                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            return Rect.zero;
        }

        public static bool IntersectsWith(Rect a, Rect b) {
            if (b.xMin < a.xMax && a.xMin < b.x + b.width && b.y < a.y + a.height)
                return a.y < b.y + b.height;
            return false;
        }

        public static string Commafy(long value) {
            if (value == 0)
                return "0";
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0,0}", value);
        }

        public static OperatingSystem GetCurrentOs() {
            if (OpenTibiaUnity.GameManager.IsRealTibia)
                return GetRealTibiaOS();

#if UNITY_STANDALONE_LINUX
            return OperatingSystem.UnityLinux;
#elif UNITY_STANDALONE_WIN
            return OperatingSystem.UnityWindows;
#elif UNITY_WSA
            return OperatingSystem.UnityWindowsUniversal;
#elif UNITY_STANDALONE_OSX
            return OperatingSystem.UnityLinux;
#elif UNITY_IOS
            return OperatingSystem.UnityIOS;
#elif UNITY_ANDROId
            return OperatingSystem.UnityAndroid;
#else
            return OperatingSystem.UnityUnknown;
#endif
        }

        private static OperatingSystem GetRealTibiaOS() {
            if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                return GetDeprecatedRealTibiaOS();

#if UNITY_STANDALONE_LINUX
            return OperatingSystem.CipsoftLinux;
#elif UNITY_STANDALONE_WIN
            return OperatingSystem.CipsoftWindows;
#else
            return OperatingSystem.CipsoftMacOS;
#endif
        }

        private static OperatingSystem GetDeprecatedRealTibiaOS() {
#if UNITY_STANDALONE_LINUX
            return OperatingSystem.DeprecatedLinux;
#elif UNITY_STANDALONE_WIN
            return OperatingSystem.DeprecatedWindows;
#else
            return OperatingSystem.DeprecatedFlash;
#endif
        }
    }
}