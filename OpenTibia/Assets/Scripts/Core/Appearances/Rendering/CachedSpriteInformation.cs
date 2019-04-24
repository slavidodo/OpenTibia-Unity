using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Rendering
{
    public class CachedSpriteInformation : Utility.Texture2DPart
    {
        private uint m_SpriteID = 0;
        
        public uint SpriteID { get => m_SpriteID; }

        public CachedSpriteInformation(uint spriteId, Texture2D tex2D, Rect rect) : base(null, Rect.zero) {
            SetCachedSpriteInformationTo(spriteId, tex2D, rect);
        }

        public void SetCachedSpriteInformationTo(uint spriteId, Texture2D tex2D, Rect rect) {
            SetTexture2DPartTo(tex2D, rect);
            m_SpriteID = spriteId;
        }

        public void Reset() {
            m_SpriteID = 0;
            SetTexture2DPartTo(null, null);
        }
    }
}
