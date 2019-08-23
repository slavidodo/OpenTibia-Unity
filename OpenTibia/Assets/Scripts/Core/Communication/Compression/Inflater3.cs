using Ionic.Zlib;

namespace OpenTibiaUnity.Core.Communication.Compression
{
    public class Inflater3
    {
        private static ZlibCodec s_Decompressor;

        public static bool Inflate(byte[] compressed, out byte[] decompressed) {
            byte[] decompressedBytes = new byte[ushort.MaxValue];

            int ret;
            if (s_Decompressor == null) {
                s_Decompressor = new ZlibCodec();
                ret = s_Decompressor.InitializeInflate(ZlibConstants.WindowBitsDefault, false);
                if (ret != ZlibConstants.Z_OK) {
                    s_Decompressor = null;
                    decompressed = null;
                    return false;
                }
            }

            s_Decompressor.InputBuffer = compressed;
            s_Decompressor.NextIn = 0;
            s_Decompressor.AvailableBytesIn = compressed.Length;

            s_Decompressor.OutputBuffer = decompressedBytes;
            s_Decompressor.NextOut = 0;
            s_Decompressor.TotalBytesOut = 0;
            s_Decompressor.AvailableBytesOut = decompressedBytes.Length;
            
            ret = s_Decompressor.Inflate(FlushType.Sync);
            
            decompressed = new byte[s_Decompressor.TotalBytesOut];
            System.Array.Copy(decompressedBytes, decompressed, decompressed.Length);

            s_Decompressor.ResetInflate();
            return true;
        }

        public static void Cleanup() {
            if (s_Decompressor == null)
                return;

            s_Decompressor.EndInflate();
            s_Decompressor = null;
        }
    }
}
