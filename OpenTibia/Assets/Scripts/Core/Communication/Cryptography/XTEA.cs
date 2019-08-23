using System;

namespace OpenTibiaUnity.Core.Communication.Cryptography
{
    public class XTEA
    {
        public const int BlockSize = 2 * sizeof(uint);

        Random _random = new Random();
        uint[] _key = new uint[4];

        public XTEA() {
            GenerateKey();
        }

        private void GenerateKey() {
            _key[0] = (uint)_random.Next();
            _key[1] = (uint)_random.Next();
            _key[2] = (uint)_random.Next();
            _key[3] = (uint)_random.Next();
        }

        public void WriteKey(Internal.ByteArray message) {
            message.WriteUnsignedInt(_key[0]);
            message.WriteUnsignedInt(_key[1]);
            message.WriteUnsignedInt(_key[2]);
            message.WriteUnsignedInt(_key[3]);
        }

        public int Encrypt(Internal.ByteArray message, int offset = 0, int length = int.MaxValue) {
            length = Math.Min(length, message.Length - offset);
            message.Position = offset + length;

            int encryptedLength = (int)(Math.Floor((length + BlockSize - 1) / (double)BlockSize) * BlockSize);
            if (encryptedLength > length) {
                byte[] tmp = new byte[encryptedLength - length];
                _random.NextBytes(tmp);
                message.WriteBytes(tmp);
                length = encryptedLength;
            }

            int i = offset;
            while (i < offset + length) {
                message.Position = i;
                uint v0 = message.ReadUnsignedInt();
                uint v1 = message.ReadUnsignedInt();
                uint delta = 0x61C88647;
                uint sum = 0;
                for (int r = 0; r < 32; r++) {
                    v0 += (v1 << 4 ^ v1 >> 5) + v1 ^ sum + _key[sum & 3];
                    sum -= delta;
                    v1 += (v0 << 4 ^ v0 >> 5) + v0 ^ sum + _key[sum >> 11 & 3];
                }

                message.Position -= BlockSize;
                message.WriteUnsignedInt(v0);
                message.WriteUnsignedInt(v1);

                i += BlockSize;
            }

            return length;
        }

        public int Decrypt(Internal.ByteArray message, int offset = 0, int length = int.MaxValue) {
            length = Math.Min(length, message.Length - offset);
            length -= length % BlockSize;
            int i = offset;
            while (i < offset + length) {
                message.Position = i;
                uint v0 = message.ReadUnsignedInt();
                uint v1 = message.ReadUnsignedInt();
                uint delta = 0x61C88647;
                uint sum = 0xC6EF3720;
                for (int r = 0; r < 32; r++) {
                    v1 -= (v0 << 4 ^ v0 >> 5) + v0 ^ sum + _key[sum >> 11 & 3];
                    sum += delta;
                    v0 -= (v1 << 4 ^ v1 >> 5) + v1 ^ sum + _key[sum & 3];
                }
                message.Position -= BlockSize;
                message.WriteUnsignedInt(v0);
                message.WriteUnsignedInt(v1);
                i += BlockSize;
            }
            
            message.Position = offset;
            int payloadLength = message.ReadUnsignedShort();
            int messageLength = payloadLength + Internal.Connection.PacketLengthSize;

            int lengthDelta = messageLength - length;
            if (lengthDelta > 0 || -lengthDelta > length)
                return 0;

            message.Length += lengthDelta;
            return length;
        }
    }
}
