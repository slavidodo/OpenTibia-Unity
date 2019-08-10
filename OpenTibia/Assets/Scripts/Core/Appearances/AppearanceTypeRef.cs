namespace OpenTibiaUnity.Core.Appearances
{
    internal class AppearanceTypeRef
    {
        protected int m_ID;
        protected int m_Data;
        private int m_Key;

        internal AppearanceTypeRef(int id, int data) {
            m_ID = id;
            m_Data = data;
            m_Key = (m_ID & 65535) << 8 | m_Data & 255;
        }

        internal bool Equals(AppearanceTypeRef other) {
            return Equals(this, other);
        }

        internal bool Equals(int id, int data) {
            return Equals(this, id, data);
        }

        internal int Compare(AppearanceTypeRef other) {
            return Compare(this, other);
        }

        internal int Compare(int id, int data) {
            return Compare(this, id, data);
        }

        internal static int Compare(AppearanceTypeRef a, AppearanceTypeRef other) {
            if (a != null && other != null)
                return a.m_Key - other.m_Key;
            return -1;
        }

        internal static int Compare(AppearanceTypeRef a, int id, int data) {
            if (a != null)
                return a.m_Key - (id & 65535) << 8 | data & 255;
            return -1;
        }

        internal static bool Equals(AppearanceTypeRef a, AppearanceTypeRef other) {
            return a != null && other != null && a.m_Key == other.m_Key;
        }

        internal static bool Equals(AppearanceTypeRef a, int id, int data) {
            return a != null && a.m_Key == ((id & 65535) << 8 | data & 255);
        }

        internal virtual AppearanceTypeRef Clone() {
            return new AppearanceTypeRef(m_ID, m_Data);
        }
    }
}
