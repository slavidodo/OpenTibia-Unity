using System;

namespace OpenTibiaUnity.Core.Communication.Cryptography
{
    internal static class Adler32Checksum
    {
        internal const int ModAdler = 65521;

        internal static uint CalculateAdler32Checksum(Internal.ByteArray byteArray, int offset = 0, int length = 0) {
            if (byteArray == null)
                throw new ArgumentNullException("Adler32Checksum.CalculateAdler32Checksum: Invalid input.");

            if (offset >= byteArray.Length)
                throw new ArgumentOutOfRangeException("Adler32Checksum.CalculateAdler32Checksum: Invalid offset.");

            uint a = 1;
            uint b = 0;
            int i = 0;

            byteArray.Position = offset;
            while (byteArray.BytesAvailable > 0 && (length == 0 || i < length)) {
                a = (a + byteArray.ReadUnsignedByte()) % ModAdler;
                b = (b + a) % ModAdler;
                i++;
            }
            
            a &= 65535;
            b &= 65535;
            return (b << 16) | a;
        }
    }
}
