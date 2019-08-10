using System.Net.Sockets;
using OpenTibiaUnity.Core.Communication.Types;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Login
{
    internal class ProtocolLogin : Internal.Protocol
    {
        internal class LoginErrorEvent : UnityEvent<string> { }
        internal class LoginTokenErrorEvent : UnityEvent<int> { }
        internal class MessageOfTheDayEvent : UnityEvent<int, string> { }
        internal class UpdateRequiredEvent : UnityEvent { }
        internal class SessionKeyEvent : UnityEvent<string> { }
        internal class CharacterListEvent : UnityEvent<CharacterList> { }

        protected bool m_TokenSuccess = false;
        protected bool m_ExpectingTermination = false;

        internal string EmailAddress { get; set; } = string.Empty;
        internal string AccountName { get; set; } = string.Empty;
        internal string Password { get; set; } = string.Empty;
        internal string Token { get; set; } = string.Empty;

        internal LoginErrorEvent onInternalError { get; } = new LoginErrorEvent();
        internal LoginErrorEvent onLoginError { get; } = new LoginErrorEvent();
        internal LoginTokenErrorEvent onLoginTokenError { get; } = new LoginTokenErrorEvent();
        internal MessageOfTheDayEvent onMessageOfTheDay { get; } = new MessageOfTheDayEvent();
        internal UpdateRequiredEvent onUpdateRequired { get; } = new UpdateRequiredEvent();
        internal SessionKeyEvent onSessionKey { get; } = new SessionKeyEvent();
        internal CharacterListEvent onCharacterList { get; } = new CharacterListEvent();

        protected override void OnConnectionEstablished() {
            SendLogin();
            m_Connection.Receive();
        }

        protected override void OnConnectionTerminated() {
            if (m_ExpectingTermination)
                return;

            OnConnectionSocketError(SocketError.ConnectionRefused, string.Empty);
        }
        
        protected override void OnCommunicationDataReady() {
            LoginserverMessageType prevMessageType = 0;
            LoginserverMessageType lastMessageType = 0;
            while (m_InputBuffer.BytesAvailable > 0) {
                var messageType = m_InputBuffer.ReadLoginType();
                try {
                    ParseMessage(messageType);
                    prevMessageType = lastMessageType;
                    lastMessageType = messageType;
                } catch (System.Exception e) {
                    var err = string.Format("ProtocolLogin.ParsePacket: error: {0}, type: ({1}), last type ({2}), prev type ({2}), unread ({4}), StackTrace: \n{5}.",
                        e.Message,
                        messageType,
                        lastMessageType,
                        prevMessageType,
                        m_InputBuffer.BytesAvailable,
                        e.StackTrace);

                    OnConnectionError(err);
                }
            }
        }

        protected void ParseMessage(Types.LoginserverMessageType messageType) {
            switch (messageType) {
                case LoginserverMessageType.ErrorLegacy:
                case LoginserverMessageType.Error:
                    onLoginError.Invoke(m_InputBuffer.ReadString());
                    m_ExpectingTermination = true;
                    break;

                case LoginserverMessageType.TokenSuccess:
                    m_TokenSuccess = m_InputBuffer.ReadBoolean();
                    break;

                case LoginserverMessageType.TokenError:
                    byte tries = m_InputBuffer.ReadUnsignedByte();
                    onLoginTokenError.Invoke(tries);
                    break;

                case LoginserverMessageType.MessageOfTheDay:
                    string[] motdinfo = m_InputBuffer.ReadString().Split('\n');
                    if (motdinfo.Length == 2 && int.TryParse(motdinfo[0], out int number))
                        onMessageOfTheDay.Invoke(number, motdinfo[1]);
                    break;

                case LoginserverMessageType.UpdateRequired:
                    onUpdateRequired.Invoke();
                    m_ExpectingTermination = true;
                    break;

                case LoginserverMessageType.SessionKey:
                    onSessionKey.Invoke(m_InputBuffer.ReadString());
                    break;

                case LoginserverMessageType.CharacterList:
                    var characterList = new CharacterList();
                    characterList.Parse(m_InputBuffer);
                    onCharacterList.Invoke(characterList);
                    m_ExpectingTermination = true;
                    break;

                default:
                    throw new System.Exception("unknown message type");
            }
        }

        protected override void OnConnectionError(string message, bool _ = false) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                onLoginError.Invoke(message);
            });

            m_ExpectingTermination = true;
            Disconnect();
        }

        protected override void OnConnectionSocketError(SocketError code, string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                if (code == SocketError.ConnectionRefused || code == SocketError.HostUnreachable)
                    onInternalError.Invoke(TextResources.ERRORMSG_10061_LOGIN_HOSTUNREACHABLE);
                else
                    onInternalError.Invoke(string.Format("Error({0}): {1}", code, message));
            });

            m_ExpectingTermination = true;
            Disconnect();
        }
        
        protected void SendLogin() {
            var message = m_PacketWriter.CreateMessage();
            message.WriteEnum(LoginclientMessageType.EnterAccount);
            message.WriteUnsignedShort((ushort)Utility.Utility.GetCurrentOs());

            var gameManager = OpenTibiaUnity.GameManager;

            message.WriteUnsignedShort((ushort)gameManager.ProtocolVersion);

            if (gameManager.GetFeature(GameFeature.GameClientVersion))
                message.WriteUnsignedInt((uint)gameManager.ClientVersion);

            if (gameManager.GetFeature(GameFeature.GameContentRevision)) {
                message.WriteUnsignedShort(OpenTibiaUnity.GetContentRevision(gameManager.ClientVersion, gameManager.BuildVersion));
                message.WriteUnsignedShort(0);
            } else {
                message.WriteUnsignedInt(0); // DatSignature
            }

            message.WriteUnsignedInt(0); // spr signature
            message.WriteUnsignedInt(0); // pic signature

            if (gameManager.GetFeature(GameFeature.GamePreviewState))
                message.WriteUnsignedByte(0x00);
            
            int payloadStart = message.Position;
            var random = new System.Random();
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption)) {
                message.WriteUnsignedByte(0); // first byte must be zero

                m_XTEA.WriteKey(message);
            }

            if (gameManager.GetFeature(GameFeature.GameAccountEmailAddress))
                message.WriteString(EmailAddress);
            else if (gameManager.GetFeature(GameFeature.GameAccountNames))
                message.WriteString(AccountName);
            else if (uint.TryParse(AccountName, out uint accountNumber))
                message.WriteUnsignedInt(accountNumber);
            else
                message.WriteUnsignedInt(0);

            message.WriteString(Password);
            
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption))
                Cryptography.PublicRSA.EncryptMessage(message, payloadStart, Cryptography.PublicRSA.RSABlockSize);

            if (gameManager.GetFeature(GameFeature.GameOGLInformation)) {
                message.WriteUnsignedByte(1);
                message.WriteUnsignedByte(1);

                if (gameManager.ClientVersion >= 1072)
                    message.WriteString(string.Format("{0} {1}", OpenTibiaUnity.GraphicsVendor, OpenTibiaUnity.GraphicsDevice));
                else
                    message.WriteString(OpenTibiaUnity.GraphicsDevice);

                message.WriteString(OpenTibiaUnity.GraphicsVersion);
            }

            if (gameManager.GetFeature(GameFeature.GameAuthenticator)) {
                payloadStart = message.Position;

                message.WriteUnsignedByte(0);
                message.WriteString(Token); // no auth-token

                message.WriteUnsignedByte(0); // stay logged-in for a while

                Cryptography.PublicRSA.EncryptMessage(message, payloadStart, Cryptography.PublicRSA.RSABlockSize);
            }

            m_PacketWriter.FinishMessage();
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption)) {
                m_PacketReader.XTEA = m_XTEA;
                m_PacketWriter.XTEA = m_XTEA;
            }
        }
    }
}
