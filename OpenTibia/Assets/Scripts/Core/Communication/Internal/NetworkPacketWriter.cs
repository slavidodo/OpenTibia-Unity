using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public sealed class NetworkPacketWriter
    {
        public class PacketFinishedEvent : UnityEvent { }

        private Cryptography.XTEA _xTEA = null;
        private ByteArray _messageBuffer = new ByteArray();
        private ByteArray _outputBuffer = new ByteArray();
        private uint _sequenceNumber = 0;

        public PacketFinishedEvent onPacketFinished = new PacketFinishedEvent();

        public Cryptography.XTEA XTEA {
            get => _xTEA;
            set => _xTEA = value;
        }

        public ByteArray OutputPacketBuffer {
            get => _outputBuffer;
        }

        public ByteArray CreateMessage() {
            // separate the body from the whole message
            // to make it easier to perform actions on the body

            _messageBuffer.Length = 0;
            _messageBuffer.Position = 0;
            _outputBuffer.Length = 0;
            _outputBuffer.Position = 0;
            return _messageBuffer;
        }

        public void FinishMessage() {
            _outputBuffer.Length = 0;
            _outputBuffer.Position = 0;

            int pos = Connection.PacketLengthPos + Connection.PacketLengthSize;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber))
                pos += Connection.SequenceNumberSize;
            else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum))
                pos += Connection.ChecksumSize;
            
            _outputBuffer.Position = pos;
            int messageSize = _messageBuffer.Position;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPacketEncryption) && _xTEA != null) {
                _outputBuffer.WriteUnsignedShort((ushort)messageSize);
                _outputBuffer.WriteBytes(_messageBuffer, 0, messageSize);
                _xTEA.Encrypt(_outputBuffer, pos, _outputBuffer.Length - pos);
            } else {
                _outputBuffer.WriteBytes(_messageBuffer, 0, messageSize);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                _outputBuffer.Position = Connection.SequenceNumberPos;
                _outputBuffer.WriteUnsignedInt(_sequenceNumber++);
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(_outputBuffer, pos, _outputBuffer.Length - pos);
                _outputBuffer.Position = Connection.ChecksumPos;
                _outputBuffer.WriteUnsignedInt(checksum);
            }
            
            _outputBuffer.Position = Connection.PacketLengthPos;
            _outputBuffer.WriteShort((short)(_outputBuffer.Length - Connection.PacketLengthSize));
            _outputBuffer.Position = 0;

            onPacketFinished.Invoke();
        }
    }
}
