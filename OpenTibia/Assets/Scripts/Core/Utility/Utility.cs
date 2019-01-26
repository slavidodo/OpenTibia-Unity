namespace OpenTibiaUnity.Core.Utility
{
    public sealed class OperatingSystem
    {
        public static readonly byte None = 0;

        public static readonly byte DeprecatedLinux = 1;
        public static readonly byte DeprecatedWindows = 2;
        public static readonly byte DeprecatedFlash = 3;

        public static readonly byte OTClientLinux = 10;
        public static readonly byte OTClientWindows = 11;
        public static readonly byte OTClientMac = 12;

        public static readonly byte UnityLinux = 20;
        public static readonly byte UnityWindows = 21;
        public static readonly byte UnityWindowsUniversal = 22;
        public static readonly byte UnityOSX = 23;
        public static readonly byte UnityIOS = 24;
        public static readonly byte UnityAndroid = 25;

        public static byte GetCurrentOs() {
#if UNITY_STANDALONE_LINUX
            return UnityLinux;
#elif UNITY_STANDALONE_WIN
            return OTClientWindows; // TODO UnityWindows
#elif UNITY_WSA
            return UnityWindowsUniversal;
#elif UNITY_STANDALONE_OSX
            return UnityLinux;
#elif UNITY_IOS
            return UnityIOS;
#elif UNITY_ANDROID
            return UnityAndroid;
#endif
        }
    }
}