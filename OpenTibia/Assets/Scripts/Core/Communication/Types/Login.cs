namespace OpenTibiaUnity.Core.Communication.Types
{
    internal enum LoginclientMessageType
    {
        EnterAccount = 1,
    }

    internal enum LoginserverMessageType : byte
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

    internal static class Login
    {
        internal static void WriteEnum(this Internal.ByteArray message, LoginclientMessageType messageType, int offset = 0) {
            message.WriteEnum(messageType, offset);
        }

        internal static LoginserverMessageType ReadLoginType(this Internal.ByteArray message) {
            return message.ReadEnum<LoginserverMessageType>();
        }
    }
}
