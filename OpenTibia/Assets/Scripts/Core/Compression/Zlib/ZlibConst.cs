namespace OpenTibiaUnity.Core.Compression.Zlib
{
    public static class ZlibConst
    {
        public const int Z_OK = 0;
        public const int Z_STREA_eND = 1;
        public const int Z_NEED_DICT = 2;
        public const int Z_STREA_eRROR = -2;
        public const int Z_DATA_ERROR = -3;
        public const int Z_BUF_ERROR = -5;

        public const int WindowBitsDefault = 15;
        
        public const int WorkingBufferSizeDefault = 16384;
        public const int WorkingBufferSizeMin = 1024;
    }
}
