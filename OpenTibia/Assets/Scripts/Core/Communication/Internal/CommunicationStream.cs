using OpenTibiaUnity.Core.Communication.Exceptions;
using System;

namespace OpenTibiaUnity.Core.Communication.Internal
{
    public sealed class CommunicationStream : System.IO.MemoryStream
    {

        public int BytesAvailable {
            get => (int)Length - (int)Position;
        }

        public CommunicationStream() {
            Capacity = 65535;
        }

        public CommunicationStream(byte[] buffer) : this() {
            Write(buffer, 0, buffer.Length);
            Position = 0;
        }

        public CommunicationStream(CommunicationStream other) : this() {
            other.CopyTo(this);
            Position = 0;
        }

        #region Input Functions
        public byte ReadUnsignedByte() {
            int res = ReadByte();
            if (res == -1)
                throw new MemoryStreamEOFException("MemoryStream.ReadUnsignedByte: EOF reached");
            return (byte)res;
        }
        public byte PeekUnsignedByte() {
            byte res = ReadUnsignedByte();
            Position--;
            return res;
        }
        public sbyte ReadSignedByte() {
            return (sbyte)ReadUnsignedByte();
        }
        public sbyte PeekSignedByte() {
            sbyte res = ReadSignedByte();
            Position--;
            return res;
        }
        public bool ReadBoolean() {
            return ReadUnsignedByte() != 0;
        }
        public bool PeekBoolean() {
            return PeekUnsignedByte() != 0;
        }
        public T ReadEnum<T>() where T : Enum {
            var type = typeof(T);
            var value = ReadUnsignedByte();
            if (!Enum.IsDefined(type, value))
                throw new MemoryStreamInvalidEnumValue("MemoryStream.ReadEnum: invalid enum value (" + value + ")");
            return (T)Enum.ToObject(type, value);
        }

        public T PeekEnum<T>() where T : Enum {
            var value = ReadEnum<T>();
            Position--;
            return value;
        }
        public ushort ReadUnsignedShort() {
            byte[] buf = new byte[sizeof(ushort)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadUnsignedShort: EOF reached");
            return BitConverter.ToUInt16(buf, 0);
        }
        public ushort PeekUnsignedShort() {
            ushort res = ReadUnsignedShort();
            Position -= sizeof(ushort);
            return res;
        }
        public short ReadShort() {
            byte[] buf = new byte[sizeof(short)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadShort: EOF reached");
            return BitConverter.ToInt16(buf, 0);
        }
        public short PeekShort() {
            short res = ReadShort();
            Position -= sizeof(short);
            return res;
        }
        public uint ReadUnsignedInt() {
            byte[] buf = new byte[sizeof(uint)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadUnsignedInt: EOF reached");
            return BitConverter.ToUInt32(buf, 0);
        }
        public uint PeekUnsignedInt() {
            uint res = ReadUnsignedInt();
            Position -= sizeof(uint);
            return res;
        }
        public int ReadInt() {
            byte[] buf = new byte[sizeof(int)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadInt: EOF reached");
            return BitConverter.ToInt32(buf, 0);
        }
        public int PeekInt() {
            int res = ReadInt();
            Position -= sizeof(int);
            return res;
        }
        public ulong ReadUnsignedLong() {
            byte[] buf = new byte[sizeof(ulong)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadUnsignedLong: EOF reached");
            return BitConverter.ToUInt64(buf, 0);
        }
        public ulong PeekUnsignedLong() {
            ulong res = ReadUnsignedLong();
            Position -= sizeof(ulong);
            return res;
        }
        public long ReadLong() {
            byte[] buf = new byte[sizeof(long)];
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadUnsignedLong: EOF reached");
            return BitConverter.ToInt64(buf, 0);
        }
        public long PeekLong() {
            long res = ReadLong();
            Position -= sizeof(long);
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
            if (Read(buf, 0, buf.Length) != buf.Length)
                throw new MemoryStreamEOFException("MemoryStream.ReadString: EOF reached (req length = " + length + ")");
            return System.Text.Encoding.ASCII.GetString(buf);
        }
        public string PeekString() {
            string str = ReadString();
            Position -= str.Length + sizeof(ushort);
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
            Position -= 2 * sizeof(ushort) + 1;
            return position;
        }
        public void Skip(int n) {
            Position += n;
        }
        #endregion

        #region Output Functions
        public void WriteUnsignedByte(byte v) {
            WriteByte(v);
        }
        public void WriteSignedByte(sbyte v) {
            WriteByte((byte)v);
        }
        public void WriteBoolean(bool v) {
            WriteUnsignedByte(v ? (byte)1 : (byte)0);
        }
        public void WriteEnum<T>(T value) where T : Enum {
            WriteUnsignedByte(Convert.ToByte(value));
        }
        public void WriteUnsignedShort(ushort value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(ushort));
        }

        public void WriteShort(short value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(short));
        }

        public void WriteUnsignedInt(uint value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(uint));
        }

        public void WriteInt(int value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public void WriteUnsignedLong(ulong value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(ulong));
        }

        public void WriteLong(long value) {
            Write(BitConverter.GetBytes(value), 0, sizeof(long));
        }

        public void WriteString(string value, bool raw = false) {
            if (!raw)
                WriteUnsignedShort((ushort)value.Length);
            Write(System.Text.Encoding.ASCII.GetBytes(value), 0, value.Length);
        }

        public void WritePosition(UnityEngine.Vector3Int value) {
            WriteUnsignedShort((ushort)value.x);
            WriteUnsignedShort((ushort)value.y);
            WriteUnsignedByte((byte)value.z);
        }
        public void Write(CommunicationStream stream, int offset, int count) {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            Write(buffer, offset, count);
        }
        #endregion
    }
}
