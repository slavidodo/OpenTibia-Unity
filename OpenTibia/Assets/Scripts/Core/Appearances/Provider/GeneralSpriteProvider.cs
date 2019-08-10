using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    internal class GeneralSpriteProvider
    {
        private SpritesInformation m_SpritesInformation;
        private AssetBundle m_AssetBundle;
        private Utility.RingBuffer<Rendering.CachedSpriteInformation> m_CachedSpriteInformations;

        internal GeneralSpriteProvider(AssetBundle assetBundle, string catalogContent) {
            m_AssetBundle = assetBundle;
            m_SpritesInformation = new SpritesInformation(catalogContent);
            
            m_CachedSpriteInformations = new Utility.RingBuffer<Rendering.CachedSpriteInformation>(Constants.MapSizeX * Constants.MapSizeY * Constants.MapSizeZ * Constants.MapSizeW);
        }

        internal Rendering.CachedSpriteInformation GetCachedSpriteInformation(uint spriteID) {
            // have we loaded this one recently?
            var cachedSpriteInformation = FindCachedSpriteInformation(spriteID);
            if (!!cachedSpriteInformation)
                return cachedSpriteInformation;

            var asset = m_SpritesInformation.FindSpritesAsset(spriteID);
            if (!asset)
                return null;

            cachedSpriteInformation = asset.GetCachedSpriteInformation(spriteID, m_AssetBundle);
            if (!cachedSpriteInformation)
                return null;

            // cache it, and continue
            InsertCachedSpriteInformation(cachedSpriteInformation);
            return cachedSpriteInformation;
        }

        private Rendering.CachedSpriteInformation FindCachedSpriteInformation(uint spriteID) {
            int lastIndex = m_CachedSpriteInformations.Length - 1;
            int index = 0;
            while (index < lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var cachedSpriteInformation = m_CachedSpriteInformations.GetItemAt(tmpIndex);
                if (cachedSpriteInformation.SpriteID > spriteID)
                    index = tmpIndex + 1;
                else if (cachedSpriteInformation.SpriteID < spriteID)
                    lastIndex = tmpIndex - 1;
                else
                    return cachedSpriteInformation;
            }

            return null;
        }

        private void InsertCachedSpriteInformation(Rendering.CachedSpriteInformation cachedSpriteInformation) {
            int lastIndex = m_CachedSpriteInformations.Length - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundCachedSpriteInformation = m_CachedSpriteInformations.GetItemAt(tmpIndex);
                if (foundCachedSpriteInformation.SpriteID < cachedSpriteInformation.SpriteID)
                    index = tmpIndex + 1;
                else if (foundCachedSpriteInformation.SpriteID > cachedSpriteInformation.SpriteID)
                    lastIndex = tmpIndex - 1;
                else
                    return;
            }

            m_CachedSpriteInformations.AddItemAt(cachedSpriteInformation, index);
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
