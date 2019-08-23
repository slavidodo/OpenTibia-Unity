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
                return x.FirstSprite_id.CompareTo(y.FirstSprite_id);
            });
        }
        
        public SpritesAsset FindSpritesAsset(uint sprite_id) {
            int lastIndex = _spritesAssetInformations.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var asset = _spritesAssetInformations[tmpIndex];
                if (asset.FirstSprite_id > sprite_id)
                    index = tmpIndex + 1;
                else if (asset.LastSprite_id < sprite_id)
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