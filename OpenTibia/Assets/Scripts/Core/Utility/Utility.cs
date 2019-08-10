namespace OpenTibiaUnity.Core.Utility
{
    internal enum OperatingSystem
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
    
    internal static class Utility
    {
        internal static OperatingSystem GetCurrentOs() {
            if (OpenTibiaUnity.GameManager.IsRealTibia || OpenTibiaUnity.GameManager.IsOpenTibia)
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
#elif UNITY_ANDROID
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