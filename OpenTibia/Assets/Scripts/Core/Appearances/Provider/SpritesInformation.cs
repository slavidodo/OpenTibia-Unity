using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    class SpritesInformation
    {
        private List<SpritesAsset> m_SpritesAssetInformations;

        public SpritesInformation(string catalogContent) {
            if (JsonConvert.DeserializeObject(catalogContent) is JArray jArray)
                m_SpritesAssetInformations = SpritesAsset.ParseJsonContents(jArray);
            else
                throw new Exception("SpritesInformation.SpritesInformation: Invalid catalog-content.json");

            m_SpritesAssetInformations.Sort((SpritesAsset x, SpritesAsset y) => {
                return x.FirstSpriteID.CompareTo(y.FirstSpriteID);
            });
        }
        
        public SpritesAsset FindSpritesAsset(uint spriteID) {
            int lastIndex = m_SpritesAssetInformations.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var asset = m_SpritesAssetInformations[tmpIndex];
                if (asset.FirstSpriteID > spriteID)
                    index = tmpIndex + 1;
                else if (asset.LastSpriteID < spriteID)
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