using OpenTibiaUnity.Core.Communication.Types;
using System.Net.Sockets;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Login
{
    public sealed class ProtocolLogin : Internal.Protocol
    {
        public class LoginErrorEvent : UnityEvent<string> { }
        public class LoginTokenErrorEvent : UnityEvent<int> { }
        public class MessageOfTheDayEvent : UnityEvent<int, string> { }
        public class UpdateRequiredEvent : UnityEvent { }
        public class SessionKeyEvent : UnityEvent<string> { }
        public class PlayDataEvent : UnityEvent<PlayData> { }

        private bool _tokenSuccess = false;
        private bool _expectingTermination = false;
        private string _sessionKey = string.Empty;

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
        public PlayDataEvent onPlayData { get; } = new PlayDataEvent();

        protected override void OnConnectionEstablished() {
            // login should be sent on the main thread
            // since it makes use of UnityEngine.Random
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => SendLogin());
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

        private void ParseMessage(LoginserverMessageType messageType) {
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
                    _sessionKey = _inputStream.ReadString();
                    break;

                case LoginserverMessageType.CharacterList:
                    var playData = ReadPlayData(_inputStream);

                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSessionKey)) {
                        playData.Session.Key = _sessionKey;
                    } else {
                        playData.Session.AccountName = AccountName;
                        playData.Session.Password = Password;
                    }
                    onPlayData.Invoke(playData);
                    _expectingTermination = true;
                    break;

                default:
                    throw new System.Exception("unknown message type");
            }
        }

        private PlayData ReadPlayData(Internal.CommunicationStream stream) {
            var playData = new PlayData();

            if (OpenTibiaUnity.GameManager.ClientVersion >= 1010) {
                byte worldCount = stream.ReadUnsignedByte();
                for (int i = 0; i < worldCount; i++) {
                    int id = stream.ReadUnsignedByte();
                    string name = stream.ReadString();
                    string hostname = stream.ReadString();
                    ushort port = stream.ReadUnsignedShort();
                    bool preview = stream.ReadBoolean();

                    playData.Worlds.Add(new PlayData.PlayDataWorld {
                        Id = id,
                        Name = name,
                        ExternalAddress = hostname,
                        ExternalPort = port,
                        PreviewState = preview ? 1 : 0
                    });
                }

                byte characterCount = stream.ReadUnsignedByte();
                for (int i = 0; i < characterCount; i++) {
                    var worldId = stream.ReadUnsignedByte();
                    string name = stream.ReadString();

                    playData.Characters.Add(new PlayData.PlayDataCharacter {
                        WorldId = worldId,
                        Name = name
                    });
                }
            } else {
                int characterCount = stream.ReadUnsignedByte();
                for (int i = 0; i < characterCount; i++) {
                    string characterName = stream.ReadString();
                    string worldName = stream.ReadString();
                    uint worldIpLong = stream.ReadUnsignedInt();
                    ushort worldPort = stream.ReadUnsignedShort();

                    int worldId = 0;
                    int index = playData.Worlds.FindIndex(x => x.Name == worldName);
                    if (index == -1) {
                        worldId = playData.Worlds.Count;
                        playData.Worlds.Add(new PlayData.PlayDataWorld {
                            Id = worldId,
                            Name = worldName,
                            ExternalAddress = new System.Net.IPAddress((long)worldIpLong).ToString(),
                            ExternalPort = worldPort
                        });
                    } else {
                        worldId = playData.Worlds[index].Id;
                    }

                    playData.Characters.Add(new PlayData.PlayDataCharacter {
                        Name = characterName,
                        WorldId = worldId
                    });
                }
            }

            uint now = (uint)System.DateTime.Now.Second;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1077) {
                stream.ReadUnsignedByte(); // todo map accountState to strings
                playData.Session.IsPremium = stream.ReadBoolean();
                playData.Session.PremiumUntil = stream.ReadUnsignedInt();
                playData.Session.InfinitePremium = playData.Session.IsPremium && playData.Session.PremiumUntil == 0;
            } else {
                uint premiumDays = stream.ReadUnsignedShort();
                playData.Session.IsPremium = premiumDays > 0;
                playData.Session.PremiumUntil = premiumDays > 0 ? (now + premiumDays * 86400U) : 0;
                playData.Session.InfinitePremium = premiumDays == ushort.MaxValue;
            }
            return playData;
        }

        protected override void OnConnectionError(string message, bool _ = false) {
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() => {
                onLoginError.Invoke(message);
            });

            _expectingTermination = true;
            Disconnect();
        }

        protected override void OnConnectionSocketError(SocketError code, string message) {
            onInternalError.Invoke($"{TextResources.ERRORMSG_HEADER_LOGIN}Error: {message} ({code}){TextResources.ERRORMSG_FOOTER}");
            _expectingTermination = true;
            Disconnect();
        }

        private void SendLogin() {
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
