namespace OpenTibiaUnity.Core.Communication.Login
{
    public class Session
    {
        public uint LastLoginTime = 0;
        public uint PremiumUntil = 0;

        public string SessionKey = string.Empty;
        public string Status = string.Empty;

        public bool IsPremium = false;
        public bool FpsTracking = false;
        public bool IsReturner = false;
        public bool ReturnerNotification = false;
        public bool ShowRewardNews = false;
        public bool OptionTracking = false;
    }
}
