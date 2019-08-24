using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    class SpritesInformation
    {
        private List<SpritesAsset> _spritesAssetInformations;

        public SpritesInformation(string catalogContent) {
            if (JsonConvert.DeserializeObject(catalogContent) is JArray jArray)
                _spritesAssetInformations = SpritesAsset.ParseJsonContents(jArray);
            else
                throw new Exception("SpritesInformation.SpritesInformation: Invalid catalog-content.json");

            _spritesAssetInformations.Sort((SpritesAsset x, SpritesAsset y) => {
                return x.FirstSpriteId.CompareTo(y.FirstSpriteId);
            });
        }
        
        public SpritesAsset FindSpritesAsset(uint spriteId) {
            int lastIndex = _spritesAssetInformations.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var asset = _spritesAssetInformations[tmpIndex];
                if (asset.FirstSpriteId > spriteId)
                    index = tmpIndex + 1;
                else if (asset.LastSpriteId < spriteId)
                    lastIndex = tmpIndex - 1;
                else // first < id < last
                    return asset;
            }

            return null;
        }

        public static bool operator !(SpritesInformation instance) {
            return instance == null;
        }

        public static bool operator true(SpritesInformation instance) {
            return !!instance;
        }

        public static bool operator false(SpritesInformation instance) {
            return !instance;
        }
    }
}