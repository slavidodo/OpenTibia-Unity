namespace OpenTibiaUnity.Core.Communication.Types
{
    public enum LoginclientMessageType
    {
        EnterAccount = 1,
    }

    public enum LoginserverMessageType : byte
    {
        Retry = 10,
        Error = 11,

        ErrorLegacy = 10,
        TokenSuccess = 12,
        TokenError = 13,
        MessageOfTheDay = 20,
        UpdateRequired = 30,
        SessionKey = 40,
        CharacterList = 100,
    }

    public static class Login
    {
        public static void WriteEnum(this Internal.CommunicationStream message, LoginclientMessageType messageType, int offset = 0) {
            message.WriteEnum(messageType, offset);
        }

        public static LoginserverMessageType ReadLoginType(this Internal.CommunicationStream message) {
            return message.ReadEnum<LoginserverMessageType>();
        }
    }
}
