using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    public class GeneralSpriteProvider
    {
        private SpritesInformation _spritesInformation;
        private AssetBundle _assetBundle;
        private Utils.RingBuffer<Rendering.CachedSpriteInformation> _cachedSprites;

        public GeneralSpriteProvider(AssetBundle assetBundle, string catalogContent) {
            _assetBundle = assetBundle;
            _spritesInformation = new SpritesInformation(catalogContent);
            
            _cachedSprites = new Utils.RingBuffer<Rendering.CachedSpriteInformation>(Constants.MapSizeX * Constants.MapSizeY * Constants.MapSizeZ * Constants.MapSizeW);
        }

        public Rendering.CachedSpriteInformation GetCachedSpriteInformation(uint spriteId) {
            // have we loaded this one recently?
            var cachedSpriteInformation = FindCachedSpriteInformation(spriteId);
            if (!!cachedSpriteInformation)
                return cachedSpriteInformation;

            var asset = _spritesInformation.FindSpritesAsset(spriteId);
            if (!asset)
                return null;

            cachedSpriteInformation = asset.GetCachedSpriteInformation(spriteId, _assetBundle);
            if (!cachedSpriteInformation)
                return null;

            // cache it, and continue
            InsertCachedSpriteInformation(cachedSpriteInformation);
            return cachedSpriteInformation;
        }

        private Rendering.CachedSpriteInformation FindCachedSpriteInformation(uint spriteId) {
            int l = 0, r = _cachedSprites.Length - 1;
            while (l < r) {
                int index = l + r >> 1;
                var other = _cachedSprites.GetItemAt(index);
                if (other.SpriteId > spriteId)
                    l = index + 1;
                else if (other.SpriteId < spriteId)
                    r = index - 1;
                else
                    return other;
            }

            return null;
        }

        private void InsertCachedSpriteInformation(Rendering.CachedSpriteInformation cachedSpriteInformation) {
            int l = 0, r = _cachedSprites.Length - 1;
            while (l <= r) {
                int i = l + (r - l) / 2;
                var other = _cachedSprites.GetItemAt(i);
                if (other.SpriteId < cachedSpriteInformation.SpriteId)
                    l = i + 1;
                else if (other.SpriteId > cachedSpriteInformation.SpriteId)
                    r = i - 1;
                else
                    return;
            }

            _cachedSprites.AddItemAt(cachedSpriteInformation, l);
        }
        
        public static bool operator !(GeneralSpriteProvider instance) {
            return instance == null;
        }

        public static bool operator true(GeneralSpriteProvider instance) {
            return !!instance;
        }

        public static bool operator false(GeneralSpriteProvider instance) {
            return !instance;
        }
    }
}
