using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Rendering
{
    public class CachedSpriteInformation : Utility.Texture2DPart
    {
        private uint m_SpriteID;
        private Vector2Int m_Size;
        
        public uint SpriteID { get => m_SpriteID; }

        public CachedSpriteInformation(uint spriteId, Texture2D tex2D, Rect rect, Vector2Int size) : base(null, Rect.zero) {
            m_SpriteID = spriteId;
            m_Size = size;
        }

        public void SetCachedSpriteInformationTo(uint spriteId, Texture2D tex2D, Rect rect) {
            SetTexture2DPartTo(tex2D, rect);
            m_SpriteID = spriteId;
        }

        public void Reset() {
            m_SpriteID = 0;
            SetTexture2DPartTo(null, null);
        }

        public static bool operator !(CachedSpriteInformation instance) {
            return instance == null;
        }

        public static bool operator true(CachedSpriteInformation instance) {
            return !!instance;
        }

        public static bool operator false(CachedSpriteInformation instance) {
            return !instance;
        }
    }
}
