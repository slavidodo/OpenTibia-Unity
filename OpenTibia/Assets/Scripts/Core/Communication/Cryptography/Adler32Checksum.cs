using System;

namespace OpenTibiaUnity.Core.Communication.Cryptography
{
    public static class Adler32Checksum
    {
        public const int ModAdler = 65521;

        public static uint CalculateAdler32Checksum(Internal.CommunicationStream stream, int offset = 0, int length = 0) {
            if (stream == null)
                throw new ArgumentNullException("Adler32Checksum.CalculateAdler32Checksum: Invalid input.");

            if (offset >= stream.Length)
                throw new ArgumentOutOfRangeException("Adler32Checksum.CalculateAdler32Checksum: Invalid offset.");

            uint a = 1;
            uint b = 0;
            int i = 0;

            stream.Position = offset;
            while (stream.BytesAvailable > 0 && (length == 0 || i < length)) {
                a = (a + stream.ReadUnsignedByte()) % ModAdler;
                b = (b + a) % ModAdler;
                i++;
            }
            
            a &= 65535;
            b &= 65535;
            return (b << 16) | a;
        }
    }
}
