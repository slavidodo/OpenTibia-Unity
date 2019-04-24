namespace OpenTibiaUnity.Core.Utility
{
    public sealed class ByteArray
    {
        private byte[] m_Buffer;
        private int m_Position;
        private int m_Size;

        public ByteArray() {
            m_Buffer = new byte[65535];
            m_Position = 0;
            m_Size = 0;
        }

        public ByteArray(byte[] buffer) {
            m_Buffer = buffer ?? throw new System.Exception("ByteArray.ByteArray: Can't assign a null buffer!");
            m_Position = 0;
            m_Size = buffer.Length;
        }

        public bool CanRead(int size) {
            return m_Position + size <= m_Size;
        }

        public void ThrowIfCantRead(int size) {
            if (!CanRead(size))
                throw new System.Exception(string.Format("ByteArray.Read<>: Can't read {0} bytes.", size));
        }

        public byte ReadU8() {
            ThrowIfCantRead(sizeof(byte));
            byte result = m_Buffer[m_Position];
            m_Position += sizeof(byte);
            return result;
        }

        public bool ReadBoolean() {
            return ReadU8() != 0;
        }

        public ushort ReadU16() {
            ThrowIfCantRead(sizeof(ushort));
            ushort result = System.BitConverter.ToUInt16(m_Buffer, m_Position);
            m_Position += sizeof(ushort);
            return result;
        }

        public uint ReadU32() {
            ThrowIfCantRead(sizeof(uint));
            uint result = System.BitConverter.ToUInt32(m_Buffer, m_Position);
            m_Position += sizeof(uint);
            return result;
        }

        public ulong ReadU64() {
            ThrowIfCantRead(sizeof(ulong));
            ulong result = System.BitConverter.ToUInt64(m_Buffer, m_Position);
            m_Position += sizeof(ulong);
            return result;
        }

        public sbyte ReadS8() {
            ThrowIfCantRead(sizeof(sbyte));
            sbyte result = (sbyte)m_Buffer[m_Position];
            m_Position += sizeof(sbyte);
            return result;
        }

        public short ReadS16() {
            ThrowIfCantRead(sizeof(short));
            short result = System.BitConverter.ToInt16(m_Buffer, m_Position);
            m_Position += sizeof(short);
            return result;
        }

        public int ReadS32() {
            ThrowIfCantRead(sizeof(int));
            int result = System.BitConverter.ToInt32(m_Buffer, m_Position);
            m_Position += sizeof(int);
            return result;
        }

        public long ReadS64() {
            ThrowIfCantRead(sizeof(long));
            long result = System.BitConverter.ToInt64(m_Buffer, m_Position);
            m_Position += sizeof(long);
            return result;
        }

        public double ReadDouble() {
            byte precision = ReadU8();
            int v = (int)(ReadU32() - int.MaxValue);
            return v / System.Math.Pow(10f, precision);
        }
    }
}
