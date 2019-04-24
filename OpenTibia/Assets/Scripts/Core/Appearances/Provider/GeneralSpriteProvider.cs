using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    public class GeneralSpriteProvider
    {
        private SpritesInformation m_SpritesInformation;
        private AssetBundle m_AssetBundle;
        private Utility.RingBuffer<Rendering.CachedSpriteInformation> m_CachedSpriteInformations;

        public GeneralSpriteProvider(AssetBundle assetBundle, string catalogContent) {
            m_AssetBundle = assetBundle;
            m_SpritesInformation = new SpritesInformation(catalogContent);
            
            // cache up to 20160 at a single time
            m_CachedSpriteInformations = new Utility.RingBuffer<Rendering.CachedSpriteInformation>(Constants.MapSizeX * Constants.MapSizeY * Constants.MapSizeZ * Constants.MapSizeW);
        }

        public Rendering.CachedSpriteInformation GetCachedSpriteInformation(uint spriteID) {
            // have we loaded this one recently?
            var cachedSpriteInformation = FindCachedSpriteInformation(spriteID);
            if (cachedSpriteInformation != null)
                return cachedSpriteInformation;

            var asset = m_SpritesInformation.FindSpritesAsset(spriteID);


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
    }
}
