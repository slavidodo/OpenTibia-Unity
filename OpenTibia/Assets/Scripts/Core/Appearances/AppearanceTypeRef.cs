namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceTypeRef
    {
        protected ushort _id;
        protected int _data;
        private int _key;

        public ushort Id { get => _id; }
        public int Data { get => _data; }
        public int Key { get => _key; }

        public AppearanceTypeRef(ushort id, int data) {
            _id = id;
            _data = data;
            _key = (_id & 65535) << 8 | _data & 255;
        }

        public bool Equals(AppearanceTypeRef other) {
            return Equals(this, other);
        }

        public bool Equals(int id, int data) {
            return Equals(this, id, data);
        }

        public int Compare(AppearanceTypeRef other) {
            return Compare(this, other);
        }

        public int Compare(int id, int data) {
            return Compare(this, id, data);
        }

        public static int Compare(AppearanceTypeRef a, AppearanceTypeRef other) {
            if (a != null && other != null)
                return a._key - other._key;
            return -1;
        }

        public static int Compare(AppearanceTypeRef a, int id, int data) {
            if (a != null)
                return a._key - (id & 65535) << 8 | data & 255;
            return -1;
        }

        public static bool Equals(AppearanceTypeRef a, AppearanceTypeRef other) {
            return a != null && other != null && a._key == other._key;
        }

        public static bool Equals(AppearanceTypeRef a, int id, int data) {
            return a != null && a._key == ((id & 65535) << 8 | data & 255);
        }

        public virtual AppearanceTypeRef Clone() {
            return new AppearanceTypeRef(_id, _data);
        }
    }
}
