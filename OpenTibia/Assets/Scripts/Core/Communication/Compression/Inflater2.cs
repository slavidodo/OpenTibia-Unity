using Ionic.Zlib;
using System.IO;

namespace OpenTibiaUnity.Core.Communication.Compression
{
    public class Inflater2
    {
        private static MemoryStream s_OutputStream;

        public static bool Inflate(byte[] compressed, out byte[] decompressed) {
            var outputStream = GetOrCreateOutputStream();
            
            using (var deflateStream = new DeflateStream(outputStream, CompressionMode.Decompress, true)) {
                deflateStream.FlushMode = FlushType.Sync;
                
                deflateStream.Write(compressed, 0, compressed.Length);

                decompressed = new byte[deflateStream.TotalOut];
                
                s_OutputStream.Position -= decompressed.Length;
                s_OutputStream.Read(decompressed, 0, decompressed.Length);

                //// not usable in sync
                //if (decompressed.Length != s_OutputStream.Length) {
                //    s_OutputStream.Position -= decompressed.Length;
                //}
            }

            return true;
        }

        public static MemoryStream GetOrCreateOutputStream() {
            if (s_OutputStream != null)
                return s_OutputStream;

            s_OutputStream = new MemoryStream();
            return s_OutputStream;
        }

        public static void Cleanup() {
            if (s_OutputStream == null)
                return;

            s_OutputStream.Dispose();
            s_OutputStream = null;
        }
    }
}
