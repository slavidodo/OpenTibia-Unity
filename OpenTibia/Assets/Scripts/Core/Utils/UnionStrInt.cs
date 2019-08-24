using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Utils
{
    public class UnionStrInt : IEquatable<UnionStrInt>, IComparable<UnionStrInt>, IComparable
    {
        private bool _isInt = false;
        private bool _isString = false;

        private int? _intValue = null;
        private string _stringValue = null;

        public bool IsInt { get => _isInt; }
        public bool IsString { get => _isString; }

        public UnionStrInt(int id) {
            _isInt = true;
            _intValue = id;
        }

        public UnionStrInt(string value) {
            _isString = true;
            _stringValue = value ?? throw new ArgumentNullException("UnionStrInt.UnionStrInt: (string) value can't be null.");
        }

        public override bool Equals(object @object) {
            if (!(@object is UnionStrInt))
                return false;

            UnionStrInt other = @object as UnionStrInt;
            if (_isInt)
                return other._isInt && _intValue == other._intValue;

            return other._isString && _stringValue == other._stringValue;
        }

        public bool Equals(UnionStrInt other) {
            if ((object)other == null)
                return false;

            if (_isInt)
                return other._isInt && _intValue == other._intValue;

            return other._isString && _stringValue == other._stringValue;
        }

        public override int GetHashCode() {
            var hashCode = -632354996;
            hashCode = hashCode * -1521134295 + _isInt.GetHashCode();
            hashCode = hashCode * -1521134295 + _isString.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(_intValue);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_stringValue);
            hashCode = hashCode * -1521134295 + IsInt.GetHashCode();
            hashCode = hashCode * -1521134295 + IsString.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            if (_isString)
                return _stringValue;
            else if (_isInt)
                return _intValue.Value.ToString();
            return null;
        }

        public int CompareTo(object @object) {
            if (@object is UnionStrInt)
                return CompareTo(@object as UnionStrInt);
            return -1;
        }

        public int CompareTo(UnionStrInt other) {
            if (_isInt) {
                if (!other._isInt)
                    return -1;

                if (_intValue < other._intValue)
                    return -1;
                else if (_intValue > other._intValue)
                    return 1;

                return 0;
            }

            if (!other._isString)
                return 1;

            return _stringValue.CompareTo(other._stringValue);
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

        public static bool operator ==(UnionStrInt cid, string value) => (object)cid != null && cid._isString && cid._stringValue == value;
        public static bool operator !=(UnionStrInt cid, string value) => (object)cid != null && (!cid._isString || cid._stringValue != value);

        public static bool operator ==(UnionStrInt cid, int value) => (object)cid != null &&cid._isInt && cid._intValue == value;
        public static bool operator !=(UnionStrInt cid, int value) => (object)cid != null && (!cid._isInt || cid._intValue != value);

        public static bool operator >=(UnionStrInt cid, int value) => (object)cid != null && cid._isInt && cid._intValue >= value;
        public static bool operator <=(UnionStrInt cid, int value) => (object)cid != null && cid._isInt && cid._intValue <= value;

        public static implicit operator int(UnionStrInt cid) => (object)cid != null && cid._isInt ? cid._intValue.Value : 0;
        public static implicit operator string(UnionStrInt cid) => (object)cid != null && cid._isString ? cid._stringValue : string.Empty;

        public static implicit operator UnionStrInt(int id) => new UnionStrInt(id);
        public static implicit operator UnionStrInt(string id) => new UnionStrInt(id);
    }
}
