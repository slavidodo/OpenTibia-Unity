namespace OpenTibiaUnity.Core.Appearances
{
    public class AppearanceTypeRef
    {
        protected int m_ID;
        protected int m_Data;
        private int m_Key;

        public AppearanceTypeRef(int id, int data) {
            m_ID = id;
            m_Data = data;
            m_Key = (m_ID & 65535) << 8 | m_Data & 255;
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
                return a.m_Key - other.m_Key;
            return -1;
        }

        public static int Compare(AppearanceTypeRef a, int id, int data) {
            if (a != null)
                return a.m_Key - (id & 65535) << 8 | data & 255;
            return -1;
        }

        public static bool Equals(AppearanceTypeRef a, AppearanceTypeRef other) {
            return a != null && other != null && a.m_Key == other.m_Key;
        }

        public static bool Equals(AppearanceTypeRef a, int id, int data) {
            return a != null && a.m_Key == ((id & 65535) << 8 | data & 255);
        }

        public AppearanceTypeRef Clone() {
            return new AppearanceTypeRef(m_ID, m_Data);
        }
    }
}
