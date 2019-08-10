using OpenTibiaUnity.Core.Communication.Exceptions;
using System;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public sealed class ByteArray
    {
        internal const int MaxByteArraySize = ushort.MaxValue;
        
        private int m_Length;
        private int m_Position = 0;

        private byte[] m_Buffer;

        internal int Length {
            get => m_Length;
            set => m_Length = value;
        }

        internal int Position {
            get => m_Position;
            set => m_Position = value;
        }

        internal int BytesAvailable {
            get => m_Length - m_Position;
        }
        
        internal byte[] Buffer {
            get => m_Buffer;
        }

        internal byte this[int index] {
            get => m_Buffer[index];
            set => m_Buffer[index] = value;
        }

        internal ByteArray(byte[] buffer = null) {
            if (buffer != null) { // initialized for read //
                m_Buffer = buffer;
                m_Length = buffer.Length;
            } else {
                m_Buffer = new byte[MaxByteArraySize];
                m_Length = 0;
            }
        }

        #region ByteArray: InputStreamFns
        internal int ReadBytes(byte[] buffer, int offset, int length) {
            if (buffer == null)
                throw new ArgumentNullException("ByteArray.ReadBytes: Destination ByteArray can't be null.");
            else if (offset < 0 || length < 0 || length > buffer.Length - offset)
                throw new ArgumentOutOfRangeException("ByteArray.ReadBytes: Data to be read exceeds the size of the array.");

            if (m_Position >= m_Length)
                throw new ByteArrayEOFException("ByteArray.ReadBytes: EOF reached.");

            int available = m_Length - m_Position;
            if (length > available)
                length = available;

            if (length <= 0)
                return 0;

            Array.Copy(m_Buffer, Position, buffer, offset, length);
            m_Position += length;
            return length;
        }

        internal int PeekBytes(byte[] buffer, int offset, int length) {
            length = ReadBytes(buffer, offset, length);
            m_Position -= length;
            return length;
        }

        internal int ReadBytes(ByteArray other, int offset, int length) {
            return ReadBytes(other.Buffer, offset, length);
        }

        internal int PeekBytes(ByteArray other, int offset, int length) {
            return PeekBytes(other.Buffer, offset, length);
        }

        internal byte ReadUnsignedByte() {
            if (m_Position >= m_Length)
                throw new ByteArrayEOFException("ByteArray.ReadU8: EOF reached.");
            return m_Buffer[m_Position++];
        }

        internal byte PeekUnsignedByte() {
            byte res = ReadUnsignedByte();
            m_Position--;
            return res;
        }

        internal bool ReadBoolean() {
            return ReadUnsignedByte() != 0;
        }

        internal bool PeekBoolean() {
            return PeekUnsignedByte() != 0;
        }

        /// <summary>
        /// Reading a byte and checks if it's defined in the enumeration
        /// </summary>
        /// <typeparam name="T">the enumeration template class</typeparam>
        /// <returns>enum value of type (T)</returns>
        /// <exception cref="ByteArrayEOFException">the message has no available bytes to read</exception>
        /// <exception cref="ByteArrayInvalidEnumValue">the read value isn't defined in the enum</exception>
        internal T ReadEnum<T>() where T : Enum {
            var type = typeof(T);
            var value = ReadUnsignedByte();
            if (!Enum.IsDefined(type, value))
                throw new ByteArrayInvalidEnumValue("ByteArray.ReadEnum: invalid enum value (" + value + ")");
            return (T)Enum.ToObject(type, value);
        }
        
        internal T PeekEnum<T>() where T : Enum {
            var value = ReadEnum<T>();
            m_Position--;
            return value;
        }
        
        internal sbyte ReadSignedByte() {
            if (m_Position >= m_Length)
                throw new ByteArrayEOFException("ByteArray.ReadSignedByte: EOF reached.");
            return (sbyte)m_Buffer[m_Position++];
        }

        internal sbyte PeekSignedByte() {
            sbyte res = ReadSignedByte();
            m_Position -= sizeof(sbyte);
            return res;
        }

        internal ushort ReadUnsignedShort() {
            byte[] buf = new byte[sizeof(ushort)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedShort: EOF reached.");
            return BitConverter.ToUInt16(buf, 0);
        }

        internal ushort PeekUnsignedShort() {
            ushort res = ReadUnsignedShort();
            m_Position -= sizeof(ushort);
            return res;
        }

        internal short ReadShort() {
            byte[] buf = new byte[sizeof(short)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadShort: EOF reached.");
            return BitConverter.ToInt16(buf, 0);
        }

        internal short PeekShort() {
            short res = ReadShort();
            m_Position -= sizeof(short);
            return res;
        }

        internal uint ReadUnsignedInt() {
            byte[] buf = new byte[sizeof(uint)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedInt: EOF reached.");
            return BitConverter.ToUInt32(buf, 0);
        }

        internal uint PeekUnsignedInt() {
            uint res = ReadUnsignedInt();
            m_Position -= sizeof(uint);
            return res;
        }

        internal int ReadInt() {
            byte[] buf = new byte[sizeof(int)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadInt: EOF reached.");
            return BitConverter.ToInt32(buf, 0);
        }

        internal int PeekInt() {
            int res = ReadInt();
            m_Position -= sizeof(int);
            return res;
        }

        internal ulong ReadUnsignedLong() {
            byte[] buf = new byte[sizeof(ulong)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedLong: EOF reached.");
            return BitConverter.ToUInt64(buf, 0);
        }

        internal ulong PeekUnsignedLong() {
            ulong res = ReadUnsignedLong();
            m_Position -= sizeof(ulong);
            return res;
        }

        internal long ReadLong() {
            byte[] buf = new byte[sizeof(long)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedLong: EOF reached.");
            return BitConverter.ToInt64(buf, 0);
        }

        internal long PeekLong() {
            long res = ReadLong();
            m_Position -= sizeof(long);
            return res;
        }

        internal double ReadDouble() {
            byte precision = ReadUnsignedByte();
            int v = ReadInt() - int.MaxValue;
            return v / Math.Pow(10f, precision);
        }

        internal string ReadString(int length = -1) {
            if (length == -1)
                length = ReadUnsignedShort();

            if (length == 0)
                return string.Empty;

            byte[] buf = new byte[length];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadString: EOF reached.");
            return System.Text.Encoding.ASCII.GetString(buf);
        }

        internal string PeekString() {
            string str = ReadString();
            m_Position -= str.Length + sizeof(ushort);
            return str;
        }

        internal UnityEngine.Vector3Int ReadPosition(int x = -1) {
            if (x == -1)
                x = ReadUnsignedShort();
            int y = ReadUnsignedShort();
            int z = ReadUnsignedByte();

            return new UnityEngine.Vector3Int(x, y, z);
        }

        internal UnityEngine.Vector3Int PeekPosition(int x = -1) {
            var position = ReadPosition(x);
            m_Position -= 2 * sizeof(ushort) + 1;
            return position;
        }

        internal int Skip(int n) {
            if (m_Position + n > m_Length)
                n = m_Length - m_Position;

            m_Position += n;
            return n;
        }
        #endregion

        #region OutputStreamFns
        internal void WriteBytes(ByteArray byteArray, int offset = 0, int length = -1) {
            WriteBytes(byteArray.Buffer, offset, length);
        }

        internal void WriteBytes(byte[] buffer, int offset = 0, int length = -1) {
            if (length == -1)
                length = buffer.Length;

            Array.Copy(buffer, offset, m_Buffer, m_Position, length);
            m_Position += length;

            if (m_Position > m_Length)
                m_Length = m_Position;
        }
        
        internal void WriteUnsignedByte(byte value, int offset = 0) {
            WriteBytes(new byte[] { value }, offset);
        }
        
        internal void WriteSignedByte(sbyte value, int offset = 0) {
            WriteBytes(new byte[] { (byte)value }, offset);
        }

        internal void WriteBoolean(bool value, int offset = 0) {
            WriteUnsignedByte(value ? (byte)1 : (byte)0, offset);
        }
        
        internal void WriteEnum<T>(T value, int offset = 0) where T : System.Enum {
            WriteUnsignedByte(Convert.ToByte(value), offset);
        }

        internal void WriteUnsignedShort(ushort value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteShort(short value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteUnsignedInt(uint value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteInt(int value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteUnsignedLong(ulong value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteLong(long value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        internal void WriteString(string value, int offset = 0, bool raw = false) {
            if (!raw)
                WriteUnsignedShort((ushort)value.Length, offset);
            WriteBytes(System.Text.Encoding.ASCII.GetBytes(value), offset);
        }

        internal void WritePosition(UnityEngine.Vector3Int value, int offset = 0) {
            WriteUnsignedShort((ushort)value.x, offset);
            WriteUnsignedShort((ushort)value.y, offset);
            WriteUnsignedByte((byte)value.z, offset);
        }
        #endregion
        
        internal ByteArray Clone() {
            var clone = new ByteArray(m_Buffer.Clone() as byte[]);
            clone.m_Position = m_Position;
            clone.m_Length = m_Length;
            return clone;
        }
    }
}
