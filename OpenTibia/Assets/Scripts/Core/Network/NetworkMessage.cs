using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Network
{
    public class InputMessage
    {
        public InputMessage(byte[] buffer) {
            m_Buffer = buffer;
        }

        public bool GetBool() {
            return GetU8() != 0;
        }

        public bool PeekBool() {
            return PeekU8() != 0;
        }

        public byte GetU8() {
            if (!CanRead(1)) ThrowCantRead(1);
            return m_Buffer[m_Position++];
        }

        // TODO: test if this is correct
        public T GetU8<T>() where T : Enum {
            if (!CanRead(1)) ThrowCantRead(1);
            return (T)(object)m_Buffer[m_Position++];
        }

        public byte PeekU8() {
            byte res = GetU8();
            m_Position--;
            return res;
        }

        public ushort GetU16() {
            int size = sizeof(ushort);
            if (!CanRead(size)) ThrowCantRead(size);
            ushort res = BitConverter.ToUInt16(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public ushort PeekU16() {
            ushort res = GetU16();
            m_Position -= sizeof(ushort);
            return res;
        }

        public uint GetU32() {
            int size = sizeof(uint);
            if (!CanRead(size)) ThrowCantRead(size);
            uint res = BitConverter.ToUInt32(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public uint PeekU32() {
            uint res = GetU32();
            m_Position -= sizeof(uint);
            return res;
        }

        public ulong GetU64() {
            int size = sizeof(ulong);
            if (!CanRead(size)) ThrowCantRead(size);
            ulong res = BitConverter.ToUInt64(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public ulong PeekU64() {
            ulong res = GetU64();
            m_Position -= sizeof(ulong);
            return res;
        }

        public sbyte GetS8() {
            if (!CanRead(1)) ThrowCantRead(1);
            return (sbyte)m_Buffer[m_Position++];
        }

        public sbyte PeekS8() {
            sbyte res = GetS8();
            m_Position--;
            return res;
        }

        public short GetS16() {
            int size = sizeof(short);
            if (!CanRead(size)) ThrowCantRead(size);
            short res = BitConverter.ToInt16(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public short PeekS16() {
            short res = GetS16();
            m_Position -= sizeof(short);
            return res;
        }

        public int GetS32() {
            int size = sizeof(int);
            if (!CanRead(size)) ThrowCantRead(size);
            int res = BitConverter.ToInt32(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public int PeekS32() {
            int res = GetS32();
            m_Position -= sizeof(int);
            return res;
        }

        public long GetS64() {
            int size = sizeof(long);
            if (!CanRead(size)) ThrowCantRead(size);
            long res = BitConverter.ToInt64(m_Buffer, (int)m_Position);
            m_Position += size;
            return res;
        }

        public long PeekS64() {
            long res = GetS64();
            m_Position -= sizeof(long);
            return res;
        }

        public string GetString() {
            ushort length = GetU16();
            if (length == 0)
                return string.Empty;
            else if (!CanRead(length))
                ThrowCantRead(length);

            byte[] str = new byte[length];
            Array.Copy(m_Buffer, m_Position, str, 0, length);
            m_Position += length;

            return Encoding.ASCII.GetString(str);
        }

        public double GetDouble() {
            byte precision = GetU8();
            int v = (int)(GetU32() - int.MaxValue);
            return (v / Math.Pow((float)10, precision));
        }

        public Vector3Int GetPosition() {
            int x = GetU16();
            int y = GetU16();
            int z = GetU8();
            return new Vector3Int(x, y, z);
        }

        public Vector3Int GetPosition(int x) {
            int y = GetU16();
            int z = GetU8();
            return new Vector3Int(x, y, z);
        }

        public void SkipBytes(int skip) {
            if (!CanRead(skip)) ThrowCantRead(skip);
            m_Position += skip;
        }

        public void ThrowCantRead(int n) {
            throw new Exception(string.Format("Trying to read after EOF has reached (n = {0}, total = {1}).", n, GetUnreadSize()));
        }

        public bool CanRead(int n) {
            return m_Buffer.Length >= m_Position + n;
        }

        public int GetUnreadSize() {
            return GetBufferLength() - GetReadPos();
        }

        public void SetBuffer(byte[] buffer, int offset = 0) {
            m_Buffer = buffer;
            m_Position = offset;
        }

        public byte[] GetBuffer() {
            return m_Buffer;
        }

        public byte[] GetUnreadBuffer() {
            return m_Buffer.Skip((int)m_Position).ToArray();
        }

        public void ReplaceUnreadBuffer(byte[] buffer) {
            List<byte> list = new List<byte>(m_Buffer.Take((int)m_Position));
            list.AddRange(buffer);

            m_Buffer = list.ToArray();
        }

        public bool ReadChecksum() {
            var u32 = GetU32();
            var checksum = Protocol.Adler32(GetUnreadBuffer(), (uint)GetUnreadSize());
            return u32 == checksum;
        }

        public int GetBufferLength() {
            return m_Buffer.Length;
        }

        public int GetReadPos() {
            return (int)m_Position;
        }

        public int Tell() {
            return GetReadPos();
        }

        public void Seek(int position) {
            m_Position = Mathf.Clamp(position, 0, m_Buffer.Length - 1);
        }

        byte[] m_Buffer;
        long m_Position = 0;
    }

    public class OutputMessage
    {
        public void AddBool(bool value) {
            AddU8(value ? (byte)1 : (byte)0);
        }

        public void AddU8(byte u8) {
            m_Buffer.Insert(m_Position++, u8);
            m_MessageSize++;
        }

        public void AddU16(ushort u16) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u16));
            m_Position += sizeof(ushort);
            m_MessageSize += sizeof(ushort);
        }

        public void AddU32(uint u32) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u32));
            m_Position += sizeof(uint);
            m_MessageSize += sizeof(uint);
        }

        public void AddU64(ulong u64) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u64));
            m_Position += sizeof(ulong);
            m_MessageSize += sizeof(ulong);
        }

        public void AddS8(sbyte s8) {
            m_Buffer.Insert(m_Position++, (byte)s8);
            m_MessageSize++;
        }

        public void AddS16(short u16) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u16));
            m_Position += sizeof(short);
            m_MessageSize += sizeof(short);
        }

        public void AddS32(int u32) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u32));
            m_Position += sizeof(int);
            m_MessageSize += sizeof(int);
        }

        public void AddS64(long u64) {
            m_Buffer.InsertRange(m_Position, BitConverter.GetBytes(u64));
            m_Position += sizeof(long);
            m_MessageSize += sizeof(long);
        }

        public void AddString(string str, bool raw = false) {
            ushort length = (ushort)str.Length;
            if (!raw)
                AddU16(length);

            m_Buffer.AddRange(Encoding.ASCII.GetBytes(str));
            m_Position += length;
            m_MessageSize += length;
        }

        public void AddPosition(UnityEngine.Vector3Int position) {
            AddU16((ushort)position.x);
            AddU16((ushort)position.y);
            AddU8((byte)position.z);
        }

        public void AddPaddingBytes(int size, byte ubyte = 0) {
            m_Buffer.AddRange(Enumerable.Repeat(ubyte, size));
            m_Position += size;
            m_MessageSize += size;
        }

        public void IncreaseHeaderLength(int size) {
            m_HeaderLength += size;
        }

        public int GetHeaderLength() {
            return m_HeaderLength;
        }

        public List<byte> GetBuffer() {
            return m_Buffer;
        }

        public byte[] GetBufferArray() {
            return m_Buffer.ToArray();
        }

        public byte[] GetHeaderBuffer() {
            var rng = m_Buffer.GetRange(m_HeaderLength, m_Buffer.Count - m_HeaderLength);
            return rng.ToArray();
        }

        public int GetMessageSize() {
            return m_MessageSize;
        }

        public int GetHeaderSize() {
            return m_MessageSize - m_HeaderLength;
        }

        // RSA
        public byte[] GetIndexedBuffer(int size) {
            if (size > GetBufferLength()) return null;

            byte[] buffer = new byte[size];
            m_Buffer.CopyTo(GetBufferLength() - size, buffer, 0, size);
            return buffer;
        }

        public void ReplaceIndexedBuffer(byte[] buffer) {
            if (buffer.Length > GetBufferLength()) {
                m_Buffer = new List<byte>(buffer);
                m_Position = GetBufferLength();
                return;
            };

            m_Buffer.RemoveRange(GetBufferLength() - buffer.Length, buffer.Length);
            m_Buffer.AddRange(buffer);
        }

        public int GetBufferLength() {
            return m_Buffer.Count;
        }

        public int GetWritePos() {
            return m_Position;
        }

        public int Tell() {
            return GetWritePos();
        }

        public void Seek(int position) {
            m_Position = Mathf.Clamp(position, 0, m_Buffer.Count - 1);
        }
        
        public void SetBuffer(IEnumerable<byte> buffer) {
            m_Buffer = new List<byte>(buffer);
            m_Position = 0;
            m_HeaderLength = 0;
        }

        List<byte> m_Buffer = new List<byte>();
        int m_Position = 0;
        int m_HeaderLength = 0;
        int m_MessageSize = 0;
    }
}