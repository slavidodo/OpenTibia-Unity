using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Rendering
{
    public class CachedSpriteInformation : Utils.Texture2DPart
    {
        private uint _spriteId;
        private Vector2Int _size;
        
        public uint SpriteId { get => _spriteId; }

        public CachedSpriteInformation(uint spriteId, Texture2D tex2D, Rect rect, Vector2Int size) : base(null, Rect.zero) {
            _spriteId = spriteId;
            _size = size;
        }

        public void SetCachedSpriteInformationTo(uint spriteId, Texture2D tex2D, Rect rect) {
            SetTexture2DPartTo(tex2D, rect);
            _spriteId = spriteId;
        }

        public void Reset() {
            _spriteId = 0;
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
