using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public sealed class NetworkPacketWriter
    {
        public class PacketFinishedEvent : UnityEvent { }

        private Cryptography.XTEA _xTEA = null;
        private CommunicationStream _messageBuffer = new CommunicationStream();
        private CommunicationStream _outputBuffer = new CommunicationStream();
        private uint _sequenceNumber = 0;

        public PacketFinishedEvent onPacketFinished = new PacketFinishedEvent();

        public Cryptography.XTEA XTEA {
            get => _xTEA;
            set => _xTEA = value;
        }

        public CommunicationStream OutputPacketBuffer {
            get => _outputBuffer;
        }

        public CommunicationStream PrepareStream() {
            // todo; verify caller's client compatability

            // separate the body from the whole message
            // to make it easier to perform actions on the body

            _messageBuffer = new CommunicationStream();
            _outputBuffer = new CommunicationStream();
            return _messageBuffer;
        }

        public void FinishMessage() {
            _outputBuffer = new CommunicationStream();

            int pos = Connection.PacketLengthPos + Connection.PacketLengthSize;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber))
                pos += Connection.SequenceNumberSize;
            else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum))
                pos += Connection.ChecksumSize;
            
            _outputBuffer.Position = pos;
            int messageSize = (int)_messageBuffer.Length;

            _messageBuffer.Position = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameLoginPacketEncryption) && _xTEA != null) {
                _outputBuffer.WriteUnsignedShort((ushort)messageSize);
                _outputBuffer.Write(_messageBuffer, 0, messageSize);
                _xTEA.Encrypt(_outputBuffer, pos, (int)_outputBuffer.Length - pos);
            } else {
                _outputBuffer.Write(_messageBuffer, 0, messageSize);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolSequenceNumber)) {
                _outputBuffer.Position = Connection.SequenceNumberPos;
                _outputBuffer.WriteUnsignedInt(_sequenceNumber++);
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameProtocolChecksum)) {
                uint checksum = Cryptography.Adler32Checksum.CalculateAdler32Checksum(_outputBuffer, pos, (int)_outputBuffer.Length - pos);
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
