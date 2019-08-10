namespace OpenTibiaUnity.Core.Compression.Zlib
{
    internal static class ZlibConst
    {
        internal const int Z_OK = 0;
        internal const int Z_STREAM_END = 1;
        internal const int Z_NEED_DICT = 2;
        internal const int Z_STREAM_ERROR = -2;
        internal const int Z_DATA_ERROR = -3;
        internal const int Z_BUF_ERROR = -5;

        internal const int WindowBitsDefault = 15;
        
        internal const int WorkingBufferSizeDefault = 16384;
        internal const int WorkingBufferSizeMin = 1024;
    }
}
