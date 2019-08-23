using OpenTibiaUnity.Core.Communication.Exceptions;
using System;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public sealed class ByteArray
    {
        public const int MaxByteArraySize = ushort.MaxValue;
        
        private int _length;
        private int _position = 0;

        private byte[] _buffer;

        public int Length {
            get => _length;
            set => _length = value;
        }

        public int Position {
            get => _position;
            set => _position = value;
        }

        public int BytesAvailable {
            get => _length - _position;
        }
        
        public byte[] Buffer {
            get => _buffer;
        }

        public byte this[int index] {
            get => _buffer[index];
            set => _buffer[index] = value;
        }

        public ByteArray(byte[] buffer = null) {
            if (buffer != null) { // initialized for read //
                _buffer = buffer;
                _length = buffer.Length;
            } else {
                _buffer = new byte[MaxByteArraySize];
                _length = 0;
            }
        }

        #region ByteArray: InputStreamFns
        public int ReadBytes(byte[] buffer, int offset, int length) {
            if (buffer == null)
                throw new ArgumentNullException("ByteArray.ReadBytes: Destination ByteArray can't be null.");
            else if (offset < 0 || length < 0 || length > buffer.Length - offset)
                throw new ArgumentOutOfRangeException("ByteArray.ReadBytes: Data to be read exceeds the size of the array.");

            if (_position >= _length)
                throw new ByteArrayEOFException("ByteArray.ReadBytes: EOF reached.");

            int available = _length - _position;
            if (length > available)
                length = available;

            if (length <= 0)
                return 0;

            Array.Copy(_buffer, Position, buffer, offset, length);
            _position += length;
            return length;
        }

        public int PeekBytes(byte[] buffer, int offset, int length) {
            length = ReadBytes(buffer, offset, length);
            _position -= length;
            return length;
        }

        public int ReadBytes(ByteArray other, int offset, int length) {
            return ReadBytes(other.Buffer, offset, length);
        }

        public int PeekBytes(ByteArray other, int offset, int length) {
            return PeekBytes(other.Buffer, offset, length);
        }

        public byte ReadUnsignedByte() {
            if (_position >= _length)
                throw new ByteArrayEOFException("ByteArray.ReadU8: EOF reached.");
            return _buffer[_position++];
        }

        public byte PeekUnsignedByte() {
            byte res = ReadUnsignedByte();
            _position--;
            return res;
        }

        public bool ReadBoolean() {
            return ReadUnsignedByte() != 0;
        }

        public bool PeekBoolean() {
            return PeekUnsignedByte() != 0;
        }

        /// <summary>
        /// Reading a byte and checks if it's defined in the enumeration
        /// </summary>
        /// <typeparam name="T">the enumeration template class</typeparam>
        /// <returns>enum value of type (T)</returns>
        /// <exception cref="ByteArrayEOFException">the message has no available bytes to read</exception>
        /// <exception cref="ByteArrayInvalidEnumValue">the read value isn't defined in the enum</exception>
        public T ReadEnum<T>() where T : Enum {
            var type = typeof(T);
            var value = ReadUnsignedByte();
            if (!Enum.IsDefined(type, value))
                throw new ByteArrayInvalidEnumValue("ByteArray.ReadEnum: invalid enum value (" + value + ")");
            return (T)Enum.ToObject(type, value);
        }
        
        public T PeekEnum<T>() where T : Enum {
            var value = ReadEnum<T>();
            _position--;
            return value;
        }
        
        public sbyte ReadSignedByte() {
            if (_position >= _length)
                throw new ByteArrayEOFException("ByteArray.ReadSignedByte: EOF reached.");
            return (sbyte)_buffer[_position++];
        }

        public sbyte PeekSignedByte() {
            sbyte res = ReadSignedByte();
            _position -= sizeof(sbyte);
            return res;
        }

        public ushort ReadUnsignedShort() {
            byte[] buf = new byte[sizeof(ushort)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedShort: EOF reached.");
            return BitConverter.ToUInt16(buf, 0);
        }

        public ushort PeekUnsignedShort() {
            ushort res = ReadUnsignedShort();
            _position -= sizeof(ushort);
            return res;
        }

        public short ReadShort() {
            byte[] buf = new byte[sizeof(short)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadShort: EOF reached.");
            return BitConverter.ToInt16(buf, 0);
        }

        public short PeekShort() {
            short res = ReadShort();
            _position -= sizeof(short);
            return res;
        }

        public uint ReadUnsignedInt() {
            byte[] buf = new byte[sizeof(uint)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedInt: EOF reached.");
            return BitConverter.ToUInt32(buf, 0);
        }

        public uint PeekUnsignedInt() {
            uint res = ReadUnsignedInt();
            _position -= sizeof(uint);
            return res;
        }

        public int ReadInt() {
            byte[] buf = new byte[sizeof(int)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadInt: EOF reached.");
            return BitConverter.ToInt32(buf, 0);
        }

        public int PeekInt() {
            int res = ReadInt();
            _position -= sizeof(int);
            return res;
        }

        public ulong ReadUnsignedLong() {
            byte[] buf = new byte[sizeof(ulong)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedLong: EOF reached.");
            return BitConverter.ToUInt64(buf, 0);
        }

        public ulong PeekUnsignedLong() {
            ulong res = ReadUnsignedLong();
            _position -= sizeof(ulong);
            return res;
        }

        public long ReadLong() {
            byte[] buf = new byte[sizeof(long)];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadUnsignedLong: EOF reached.");
            return BitConverter.ToInt64(buf, 0);
        }

        public long PeekLong() {
            long res = ReadLong();
            _position -= sizeof(long);
            return res;
        }

        public double ReadDouble() {
            byte precision = ReadUnsignedByte();
            int v = ReadInt() - int.MaxValue;
            return v / Math.Pow(10f, precision);
        }

        public string ReadString(int length = -1) {
            if (length == -1)
                length = ReadUnsignedShort();

            if (length == 0)
                return string.Empty;

            byte[] buf = new byte[length];
            if (ReadBytes(buf, 0, buf.Length) != buf.Length)
                throw new ByteArrayEOFException("ByteArray.ReadString: EOF reached.");
            return System.Text.Encoding.ASCII.GetString(buf);
        }

        public string PeekString() {
            string str = ReadString();
            _position -= str.Length + sizeof(ushort);
            return str;
        }

        public UnityEngine.Vector3Int ReadPosition(int x = -1) {
            if (x == -1)
                x = ReadUnsignedShort();
            int y = ReadUnsignedShort();
            int z = ReadUnsignedByte();

            return new UnityEngine.Vector3Int(x, y, z);
        }

        public UnityEngine.Vector3Int PeekPosition(int x = -1) {
            var position = ReadPosition(x);
            _position -= 2 * sizeof(ushort) + 1;
            return position;
        }

        public int Skip(int n) {
            if (_position + n > _length)
                n = _length - _position;

            _position += n;
            return n;
        }
        #endregion

        #region OutputStreamFns
        public void WriteBytes(ByteArray byteArray, int offset = 0, int length = -1) {
            WriteBytes(byteArray.Buffer, offset, length);
        }

        public void WriteBytes(byte[] buffer, int offset = 0, int length = -1) {
            if (length == -1)
                length = buffer.Length;

            Array.Copy(buffer, offset, _buffer, _position, length);
            _position += length;

            if (_position > _length)
                _length = _position;
        }
        
        public void WriteUnsignedByte(byte value, int offset = 0) {
            WriteBytes(new byte[] { value }, offset);
        }
        
        public void WriteSignedByte(sbyte value, int offset = 0) {
            WriteBytes(new byte[] { (byte)value }, offset);
        }

        public void WriteBoolean(bool value, int offset = 0) {
            WriteUnsignedByte(value ? (byte)1 : (byte)0, offset);
        }
        
        public void WriteEnum<T>(T value, int offset = 0) where T : System.Enum {
            WriteUnsignedByte(Convert.ToByte(value), offset);
        }

        public void WriteUnsignedShort(ushort value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteShort(short value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteUnsignedInt(uint value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteInt(int value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteUnsignedLong(ulong value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteLong(long value, int offset = 0) {
            WriteBytes(BitConverter.GetBytes(value), offset);
        }

        public void WriteString(string value, int offset = 0, bool raw = false) {
            if (!raw)
                WriteUnsignedShort((ushort)value.Length, offset);
            WriteBytes(System.Text.Encoding.ASCII.GetBytes(value), offset);
        }

        public void WritePosition(UnityEngine.Vector3Int value, int offset = 0) {
            WriteUnsignedShort((ushort)value.x, offset);
            WriteUnsignedShort((ushort)value.y, offset);
            WriteUnsignedByte((byte)value.z, offset);
        }
        #endregion
        
        public ByteArray Clone() {
            var clone = new ByteArray(_buffer.Clone() as byte[]);
            clone._position = _position;
            clone._length = _length;
            return clone;
        }
    }
}
