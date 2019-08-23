namespace OpenTibiaUnity.Core.Utils
{
    public sealed class ByteArray
    {
        private byte[] _buffer;
        private int _position;
        private int _size;

        public ByteArray() {
            _buffer = new byte[65535];
            _position = 0;
            _size = 0;
        }

        public ByteArray(byte[] buffer) {
            _buffer = buffer ?? throw new System.Exception("ByteArray.ByteArray: Can't assign a null buffer!");
            _position = 0;
            _size = buffer.Length;
        }

        public bool CanRead(int size) {
            return _position + size <= _size;
        }

        public void ThrowIfCantRead(int size) {
            if (!CanRead(size))
                throw new System.Exception(string.Format("ByteArray.Read<>: Can't read {0} bytes.", size));
        }

        public byte ReadU8() {
            ThrowIfCantRead(sizeof(byte));
            byte result = _buffer[_position];
            _position += sizeof(byte);
            return result;
        }

        public bool ReadBoolean() {
            return ReadU8() != 0;
        }

        public ushort ReadU16() {
            ThrowIfCantRead(sizeof(ushort));
            ushort result = System.BitConverter.ToUInt16(_buffer, _position);
            _position += sizeof(ushort);
            return result;
        }

        public uint ReadU32() {
            ThrowIfCantRead(sizeof(uint));
            uint result = System.BitConverter.ToUInt32(_buffer, _position);
            _position += sizeof(uint);
            return result;
        }

        public ulong ReadU64() {
            ThrowIfCantRead(sizeof(ulong));
            ulong result = System.BitConverter.ToUInt64(_buffer, _position);
            _position += sizeof(ulong);
            return result;
        }

        public sbyte ReadS8() {
            ThrowIfCantRead(sizeof(sbyte));
            sbyte result = (sbyte)_buffer[_position];
            _position += sizeof(sbyte);
            return result;
        }

        public short ReadS16() {
            ThrowIfCantRead(sizeof(short));
            short result = System.BitConverter.ToInt16(_buffer, _position);
            _position += sizeof(short);
            return result;
        }

        public int ReadS32() {
            ThrowIfCantRead(sizeof(int));
            int result = System.BitConverter.ToInt32(_buffer, _position);
            _position += sizeof(int);
            return result;
        }

        public long ReadS64() {
            ThrowIfCantRead(sizeof(long));
            long result = System.BitConverter.ToInt64(_buffer, _position);
            _position += sizeof(long);
            return result;
        }

        public double ReadDouble() {
            byte precision = ReadU8();
            int v = (int)(ReadU32() - int.MaxValue);
            return v / System.Math.Pow(10f, precision);
        }
    }
}
