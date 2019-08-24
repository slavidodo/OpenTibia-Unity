using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    public class GeneralSpriteProvider
    {
        private SpritesInformation _spritesInformation;
        private AssetBundle _assetBundle;
        private Utils.RingBuffer<Rendering.CachedSpriteInformation> _cachedSpriteInformations;

        public GeneralSpriteProvider(AssetBundle assetBundle, string catalogContent) {
            _assetBundle = assetBundle;
            _spritesInformation = new SpritesInformation(catalogContent);
            
            _cachedSpriteInformations = new Utils.RingBuffer<Rendering.CachedSpriteInformation>(Constants.MapSizeX * Constants.MapSizeY * Constants.MapSizeZ * Constants.MapSizeW);
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
            int lastIndex = _cachedSpriteInformations.Length - 1;
            int index = 0;
            while (index < lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var cachedSpriteInformation = _cachedSpriteInformations.GetItemAt(tmpIndex);
                if (cachedSpriteInformation.SpriteId > spriteId)
                    index = tmpIndex + 1;
                else if (cachedSpriteInformation.SpriteId < spriteId)
                    lastIndex = tmpIndex - 1;
                else
                    return cachedSpriteInformation;
            }

            return null;
        }

        private void InsertCachedSpriteInformation(Rendering.CachedSpriteInformation cachedSpriteInformation) {
            int lastIndex = _cachedSpriteInformations.Length - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundCachedSpriteInformation = _cachedSpriteInformations.GetItemAt(tmpIndex);
                if (foundCachedSpriteInformation.SpriteId < cachedSpriteInformation.SpriteId)
                    index = tmpIndex + 1;
                else if (foundCachedSpriteInformation.SpriteId > cachedSpriteInformation.SpriteId)
                    lastIndex = tmpIndex - 1;
                else
                    return;
            }

            _cachedSpriteInformations.AddItemAt(cachedSpriteInformation, index);
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
