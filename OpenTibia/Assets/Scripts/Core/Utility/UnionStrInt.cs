using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utility
{
    public class UnionStrInt : IEquatable<UnionStrInt>, IComparable<UnionStrInt>, IComparable
    {
        private bool m_IsInt = false;
        private bool m_IsString = true;

        private int? m_IntValue = null;
        private string m_StringValue = null;

        public bool IsInt { get => m_IsInt; }
        public bool IsString { get => m_IsString; }

        public UnionStrInt(int id) {
            m_IsInt = true;
            m_IntValue = id;
        }

        public UnionStrInt(string value) {
            m_IsString = true;
            m_StringValue = value ?? throw new System.ArgumentNullException("ChannelID.ChannelID: (string) value can't be null.");
        }

        public override bool Equals(object obj) {
            if (!(obj is UnionStrInt))
                return false;

            UnionStrInt other = obj as UnionStrInt;
            if (m_IsInt)
                return other.m_IsInt && m_IntValue == other.m_IntValue;

            return other.m_IsString && m_StringValue == other.m_StringValue;
        }

        public bool Equals(UnionStrInt other) {
            if ((object)other == null)
                return false;

            if (m_IsInt)
                return other.m_IsInt && m_IntValue == other.m_IntValue;

            return other.m_IsString && m_StringValue == other.m_StringValue;
        }

        public override int GetHashCode() {
            var hashCode = -632354996;
            hashCode = hashCode * -1521134295 + m_IsInt.GetHashCode();
            hashCode = hashCode * -1521134295 + m_IsString.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(m_IntValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(m_StringValue);
            hashCode = hashCode * -1521134295 + IsInt.GetHashCode();
            hashCode = hashCode * -1521134295 + IsString.GetHashCode();
            return hashCode;
        }

        public int CompareTo(object obj) {
            if (obj is UnionStrInt)
                return CompareTo(obj as UnionStrInt);
            return -1;
        }

        public int CompareTo(UnionStrInt other) {
            if (m_IsInt) {
                if (!other.m_IsInt)
                    return -1;

                if (m_IntValue < other.m_IntValue)
                    return -1;
                else if (m_IntValue > other.m_IntValue)
                    return 1;

                return 0;
            }

            if (!other.m_IsString)
                return 1;

            return m_StringValue.CompareTo(other.m_StringValue);
        }

        public static bool operator ==(UnionStrInt cid1, UnionStrInt cid2) {
            if ((object)cid1 != null)
                return cid1.Equals(cid2);
            else if ((object)cid2 != null)
                return cid2.Equals(cid1);

            return true;
        }
        public static bool operator !=(UnionStrInt cid1, UnionStrInt cid2) {
            if ((object)cid1 != null)
                return !cid1.Equals(cid2);
            else if ((object)cid2 != null)
                return !cid2.Equals(cid1);

            return false;
        }

        public static bool operator ==(UnionStrInt cid, string value) => (object)cid != null && cid.m_IsString && cid.m_StringValue == value;
        public static bool operator !=(UnionStrInt cid, string value) => (object)cid != null && (!cid.m_IsString || cid.m_StringValue != value);

        public static bool operator ==(UnionStrInt cid, int value) => (object)cid != null &&cid.m_IsInt && cid.m_IntValue == value;
        public static bool operator !=(UnionStrInt cid, int value) => (object)cid != null && (!cid.m_IsInt || cid.m_IntValue != value);

        public static bool operator >=(UnionStrInt cid, int value) => (object)cid != null && cid.m_IsInt && cid.m_IntValue >= value;
        public static bool operator <=(UnionStrInt cid, int value) => (object)cid != null && cid.m_IsInt && cid.m_IntValue <= value;

        public static implicit operator int(UnionStrInt cid) => (object)cid != null && cid.m_IsInt ? cid.m_IntValue.Value : 0;
        public static implicit operator string(UnionStrInt cid) => (object)cid != null && cid.m_IsString ? cid.m_StringValue : string.Empty;

        public static implicit operator UnionStrInt(int id) => new UnionStrInt(id);
        public static implicit operator UnionStrInt(string id) => new UnionStrInt(id);
    }
}
