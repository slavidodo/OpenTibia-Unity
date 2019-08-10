using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class SpriteTypeImpl
    {
        public string File;
        public int SpriteType;
        public uint FirstSpriteID;
        public uint LastSpriteID;
    }

    public class CachedSpriteInformation
    {
        public uint id = 0;
        public Texture2D texture = null;
        public Rect rect = Rect.zero;
        public Vector2 spriteSize = Vector2.zero;
    }

    public sealed class SpritesProvider {
        AssetBundle m_SpritesAssetBundle;

        List<SpriteTypeImpl> m_SpriteSheet = new List<SpriteTypeImpl>();
        Dictionary<string, Texture2D> m_CachedTextures = new Dictionary<string, Texture2D>();
        List<CachedSpriteInformation> m_SpriteCachedInformation = new List<CachedSpriteInformation>();

        private static Vector2[] s_SpriteTypesSizesRef = new Vector2[] {
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 2.0f),
            new Vector2(2.0f, 1.0f),
            new Vector2(2.0f, 2.0f)
        };
        
        public SpritesProvider(AssetBundle spritesBundle, string catalogJson) {
            var catalogObjects = (JArray)JsonConvert.DeserializeObject(catalogJson);
            if (catalogObjects == null)
                throw new System.Exception("SpriteProvider.SpritesProvider: Invalid catalog-content JSON");
            
            m_SpritesAssetBundle = spritesBundle;
            foreach (var @object in catalogObjects.Children<JObject>()) {
                var typeProperty = @object.Property("type");
                if (typeProperty == null || typeProperty.Value.ToString() != "sprite")
                    continue;
                
                if (!@object.TryGetValue("file", out JToken fileToken)
                    || !@object.TryGetValue("spritetype", out JToken spriteTypeToken)
                    || !@object.TryGetValue("firstspriteid", out JToken firstSpriteIDToken)
                    || !@object.TryGetValue("lastspriteid", out JToken lastSpriteIDToken))
                    continue;

                try {
                    m_SpriteSheet.Add(new SpriteTypeImpl() {
                        File = (string)fileToken,
                        SpriteType = (int)spriteTypeToken,
                        FirstSpriteID = (uint)firstSpriteIDToken,
                        LastSpriteID = (uint)lastSpriteIDToken
                    });
                } catch (System.InvalidCastException) {}
            }
        }

        public void Unload() {
            m_SpritesAssetBundle?.Unload(true);
            m_SpritesAssetBundle = null;

            m_SpriteSheet.Clear();
            m_CachedTextures.Clear();
            m_SpriteCachedInformation.Clear();
        }

        private bool GetSpriteInfo(uint spriteID, out string filename, out Rect spriteRect, out Vector2 realSpriteSize, out Texture2D tex2D) {
            SpriteTypeImpl match = m_SpriteSheet.Find(m => spriteID >= m.FirstSpriteID && spriteID <= m.LastSpriteID);
            tex2D = match != null ? GetOrLoadTexture(match.File) : null;

            if (match == null || match.SpriteType < 1 || match.SpriteType > 4 || tex2D == null) {
                filename = null;
                spriteRect = Rect.zero;
                realSpriteSize = Vector2.zero;
                tex2D = null;
                return false;
            }

            filename = match.File;
            realSpriteSize = s_SpriteTypesSizesRef[match.SpriteType - 1] * Constants.FieldSize;
            uint realID = spriteID - match.FirstSpriteID;
            int texPerRow = (int)(tex2D.width / realSpriteSize.x);

            float x = (realID % texPerRow) * realSpriteSize.x;
            float y = tex2D.width - (realID / texPerRow * realSpriteSize.y) - realSpriteSize.y;
            spriteRect = new Rect(x / tex2D.width, y / tex2D.height, realSpriteSize.x / tex2D.width, realSpriteSize.y / tex2D.height);
            return true;
        }

        public CachedSpriteInformation GetSprite(uint spriteID) {
            var cachedInformation = FindCachedInformation(spriteID);
            if (cachedInformation != null)
                return cachedInformation;

            string filename;
            Rect spriteRect;
            Vector2 realSize;
            Texture2D tex2D;
            if (!GetSpriteInfo(spriteID, out filename, out spriteRect, out realSize, out tex2D))
                return null;
            
            cachedInformation = new CachedSpriteInformation();
            cachedInformation.id = spriteID;
            cachedInformation.rect = spriteRect;
            cachedInformation.spriteSize = realSize;
            cachedInformation.texture = tex2D;

            BinaryInsert(cachedInformation);
            return cachedInformation;
        }

        private Texture2D GetOrLoadTexture(string filename) {
            Texture2D tex2D;
            if (m_CachedTextures.TryGetValue(filename, out tex2D))
                return tex2D;

            tex2D = m_SpritesAssetBundle.LoadAsset<Texture2D>(filename);
            if (tex2D)
                m_CachedTextures.Add(filename, tex2D);

            return tex2D;
        }

        private CachedSpriteInformation FindCachedInformation(uint spriteID) {
            int lastIndex = m_SpriteCachedInformation.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var cachedInformation = m_SpriteCachedInformation[tmpIndex];
                if (cachedInformation.id > spriteID)
                    index = tmpIndex + 1;
                else if (cachedInformation.id < spriteID)
                    lastIndex = tmpIndex - 1;
                else
                    return cachedInformation;
            }

            return null;
        }

        private void BinaryInsert(CachedSpriteInformation cachedInformation) {
            int index = 0;
            int lastIndex = m_SpriteCachedInformation.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundCache = m_SpriteCachedInformation[tmpIndex];
                if (foundCache.id < cachedInformation.id)
                    index = tmpIndex + 1;
                else if (foundCache.id > cachedInformation.id)
                    lastIndex = tmpIndex - 1;
                else
                    return;
            }

            m_SpriteCachedInformation.Insert(index, cachedInformation);
        }
    }
}
