using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public class NetworkPacketReader
    {
        public class PacketReadyEvent : UnityEvent { }

        private Cryptography.XTEA _xTEA = null;
        private CommunicationStream _inputStream = null;

        private bool _compressed = false;

        public PacketReadyEvent onPacketReady = new PacketReadyEvent();

        public Cryptography.XTEA XTEA {
            get => _xTEA;
            set => _xTEA = value;
        }

        public bool Compressed {
            get => _compressed;
        }

        public NetworkPacketReader(CommunicationStream stream) {
            _inputStream = stream;
        }

        public bool BytesAvailable(int bytes) {
            return _inputStream.BytesAvailable >= bytes;
        }

        public void PreparePacket() {
            int payloadOffset = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                uint recvCompression = _inputStream.ReadUnsignedInt();
                payloadOffset = (int)_inputStream.Position;

                _compressed = (recvCompression & 1U << 31) != 0;
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                var recvChecksum = _inputStream.ReadUnsignedInt();
                payloadOffset = (int)_inputStream.Position;
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(_inputStream, payloadOffset, (int)_inputStream.Length - payloadOffset);
                if (recvChecksum != checksum)
                    throw new System.Exception($"Received checksum doesn't match the expected one ({recvChecksum} != {checksum})");

                // TODO; is this really neseccary?
                _inputStream.Position = payloadOffset;
                _compressed = false;
            }

            if (_xTEA != null) {
                int length = (int)_inputStream.Length - payloadOffset;
                if (_xTEA.Decrypt(_inputStream, payloadOffset, length) == 0)
                    throw new System.Exception("failed to decrypt XTEA");
            }
            
            onPacketReady.Invoke();
        }
    }
}
