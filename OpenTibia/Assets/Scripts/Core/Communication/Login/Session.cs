namespace OpenTibiaUnity.Core.Communication.Login
{
    internal class Session
    {
        internal uint LastLoginTime = 0;
        internal uint PremiumUntil = 0;

        internal string SessionKey = string.Empty;
        internal string Status = string.Empty;

        internal bool IsPremium = false;
        internal bool FpsTracking = false;
        internal bool IsReturner = false;
        internal bool ReturnerNotification = false;
        internal bool ShowRewardNews = false;
        internal bool OptionTracking = false;
    }
}
