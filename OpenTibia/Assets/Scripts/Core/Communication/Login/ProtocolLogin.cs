using System.Net.Sockets;
using OpenTibiaUnity.Core.Communication.Types;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public class ProtocolLogin : Internal.Protocol
    {
        public class LoginErrorEvent : UnityEvent<string> { }
        public class LoginTokenErrorEvent : UnityEvent<int> { }
        public class MessageOfTheDayEvent : UnityEvent<int, string> { }
        public class UpdateRequiredEvent : UnityEvent { }
        public class SessionKeyEvent : UnityEvent<string> { }
        public class CharacterListEvent : UnityEvent<CharacterList> { }

        protected bool _tokenSuccess = false;
        protected bool _expectingTermination = false;

        public string EmailAddress { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        public LoginErrorEvent onInternalError { get; } = new LoginErrorEvent();
        public LoginErrorEvent onLoginError { get; } = new LoginErrorEvent();
        public LoginTokenErrorEvent onLoginTokenError { get; } = new LoginTokenErrorEvent();
        public MessageOfTheDayEvent onMessageOfTheDay { get; } = new MessageOfTheDayEvent();
        public UpdateRequiredEvent onUpdateRequired { get; } = new UpdateRequiredEvent();
        public SessionKeyEvent onSessionKey { get; } = new SessionKeyEvent();
        public CharacterListEvent onCharacterList { get; } = new CharacterListEvent();

        protected override void OnConnectionEstablished() {
            SendLogin();
            _connection.Receive();
        }

        protected override void OnConnectionTerminated() {
            if (_expectingTermination)
                return;

            OnConnectionSocketError(SocketError.ConnectionRefused, string.Empty);
        }
        
        protected override void OnCommunicationDataReady() {
            LoginserverMessageType prevMessageType = 0;
            LoginserverMessageType lastMessageType = 0;
            while (_inputStream.BytesAvailable > 0) {
                var messageType = _inputStream.ReadLoginType();
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
                        _inputStream.BytesAvailable,
                        e.StackTrace);

                    OnConnectionError(err);
                }
            }
        }

        protected void ParseMessage(Types.LoginserverMessageType messageType) {
            switch (messageType) {
                case LoginserverMessageType.ErrorLegacy:
                case LoginserverMessageType.Error:
                    onLoginError.Invoke(_inputStream.ReadString());
                    _expectingTermination = true;
                    break;

                case LoginserverMessageType.TokenSuccess:
                    _tokenSuccess = _inputStream.ReadBoolean();
                    break;

                case LoginserverMessageType.TokenError:
                    byte tries = _inputStream.ReadUnsignedByte();
                    onLoginTokenError.Invoke(tries);
                    break;

                case LoginserverMessageType.MessageOfTheDay:
                    string[] motdinfo = _inputStream.ReadString().Split('\n');
                    if (motdinfo.Length == 2 && int.TryParse(motdinfo[0], out int number))
                        onMessageOfTheDay.Invoke(number, motdinfo[1]);
                    break;

                case LoginserverMessageType.UpdateRequired:
                    onUpdateRequired.Invoke();
                    _expectingTermination = true;
                    break;

                case LoginserverMessageType.SessionKey:
                    onSessionKey.Invoke(_inputStream.ReadString());
                    break;

                case LoginserverMessageType.CharacterList:
                    var characterList = new CharacterList();
                    characterList.Parse(_inputStream);
                    onCharacterList.Invoke(characterList);
                    _expectingTermination = true;
                    break;

                default:
                    throw new System.Exception("unknown message type");
            }
        }

        protected override void OnConnectionError(string message, bool _ = false) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                onLoginError.Invoke(message);
            });

            _expectingTermination = true;
            Disconnect();
        }

        protected override void OnConnectionSocketError(SocketError code, string message) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                if (code == SocketError.ConnectionRefused || code == SocketError.HostUnreachable)
                    onInternalError.Invoke(TextResources.ERRORMSG_10061_LOGIN_HOSTUNREACHABLE);
                else
                    onInternalError.Invoke(string.Format("Error({0}): {1}", code, message));
            });

            _expectingTermination = true;
            Disconnect();
        }
        
        protected void SendLogin() {
            var message = _packetWriter.PrepareStream();
            message.WriteEnum(LoginclientMessageType.EnterAccount);
            message.WriteUnsignedShort((ushort)Utils.Utility.GetCurrentOs());

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
            
            int payloadStart = (int)message.Position;
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption)) {
                message.WriteUnsignedByte(0); // first byte must be zero

                _xTEA.WriteKey(message);
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
                payloadStart = (int)message.Position;

                message.WriteUnsignedByte(0);
                message.WriteString(Token); // no auth-token

                message.WriteUnsignedByte(0); // stay logged-in for a while

                Cryptography.PublicRSA.EncryptMessage(message, payloadStart, Cryptography.PublicRSA.RSABlockSize);
            }

            _packetWriter.FinishMessage();
            if (gameManager.GetFeature(GameFeature.GameLoginPacketEncryption)) {
                _packetReader.XTEA = _xTEA;
                _packetWriter.XTEA = _xTEA;
            }
        }
    }
}
