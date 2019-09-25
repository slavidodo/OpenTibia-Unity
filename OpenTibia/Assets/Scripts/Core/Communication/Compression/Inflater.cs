using System.IO;

namespace OpenTibiaUnity.Core.Communication.Compression
{
    public class Inflater
    {
        private static Ionic.Zlib.DeflateStream _deflateStream = null;
        private static MemoryStream _outputStream;
        private static long _lastOutput = 0;

        public static bool Inflate(byte[] compressed, out byte[] decompressed) {
            CreateStreamsIfNotCreated();

            byte[] compressedAligned = new byte[compressed.Length + 4];
            System.Array.Copy(compressed, compressedAligned, compressed.Length);
            compressedAligned[compressedAligned.Length - 1] = 0xFF;
            compressedAligned[compressedAligned.Length - 2] = 0xFF;
            compressedAligned[compressedAligned.Length - 3] = 0;
            compressedAligned[compressedAligned.Length - 4] = 0;
            
            _deflateStream.FlushMode = Ionic.Zlib.FlushType.Sync;
            _deflateStream.Write(compressedAligned, 0, compressedAligned.Length);
            _deflateStream.Flush();

            long totalOutput = _deflateStream.TotalOut;
            decompressed = new byte[totalOutput - _lastOutput];
            _outputStream.Position -= decompressed.Length;
            _outputStream.Read(decompressed, 0, decompressed.Length);
            _outputStream.SetLength(0);
            
            _lastOutput = totalOutput;
            return true;
        }

        private static void CreateStreamsIfNotCreated() {
            if (_outputStream != null && _deflateStream != null)
                return;

            _outputStream = new MemoryStream();
            _deflateStream = new Ionic.Zlib.DeflateStream(_outputStream, Ionic.Zlib.CompressionMode.Decompress, true);
            _lastOutput = 0;
        }

        public static void Cleanup() {
            if (_outputStream != null) {
                _outputStream.Dispose();
                _outputStream = null;
            }

            if (_deflateStream != null) {
                _deflateStream.Dispose();
                _deflateStream = null;
            }
        }
    }
}
