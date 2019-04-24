using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public class SpriteType
    {
        public string file;
        public int spritetype;
        public uint firstspriteid;
        public uint lastspriteid;
    }

    public class CachedSpriteInformation
    {
        public uint id = 0;
        public Texture2D texture = null;
        public Rect rect = Rect.zero;
        public Vector2 spriteSize = Vector2.zero;
    }

    public sealed class SpritesProvider
    {
        public const int AtlasTexture_Width = 512;
        public const int AtlasTexture_Height = 512;

        AssetBundle m_SpritesAssetBundle;

        List<SpriteType> m_SpriteSheet = new List<SpriteType>();
        Dictionary<string, Texture2D> m_CachedTextures = new Dictionary<string, Texture2D>();
        List<CachedSpriteInformation> m_SpriteCachedInformation = new List<CachedSpriteInformation>();

        private static Vector2Int[] s_SpriteTypesSizesRef = new Vector2Int[] {
            new Vector2Int(1, 1),
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, 2)
        };
        
        public SpritesProvider(AssetBundle spritesBundle, string catalogJson) {
            var jsonDeserializedObj = JsonConvert.DeserializeObject(catalogJson);
            JArray jArray = (JArray)jsonDeserializedObj;
            if (jArray == null)
                throw new System.Exception("SpriteProvider.CSOR: Invalid catalog-content JSON");
            
            m_SpritesAssetBundle = spritesBundle;
            foreach (var obj in jArray.Children<JObject>()) {
                var typeProperty = obj.Property("type");
                if (typeProperty == null || typeProperty.Value.ToString() != "sprite")
                    continue;
            
                SpriteType spriteType = new SpriteType();
                bool a0 = false, a1 = false, a2 = false, a3 = false;
                foreach (var property in obj.Properties()) {
                    string name = property.Name;
                    JToken value = property.Value;
                    if (name == "file") {
                        if (a0)
                            continue;
            
                        spriteType.file = (string)value;
                        a0 = true;
                    } else if (name == "spritetype") {
                        if (a1)
                            continue;
            
                        int? spritetype = (int)value;
                        if (spritetype != null) {
                            spriteType.spritetype = spritetype.Value;
                            a1 = true;
                        }
                    } else if (name == "firstspriteid") {
                        if (a2)
                            continue;
            
                        uint? firstspriteid = (uint)value;
                        if (firstspriteid != null) {
                            spriteType.firstspriteid = firstspriteid.Value;
                            a2 = true;
                        }
                    } else if (name == "lastspriteid") {
                        if (a3)
                            continue;
            
                        uint? lastspriteid = (uint)value;
                        if (lastspriteid != null) {
                            spriteType.lastspriteid = lastspriteid.Value;
                            a3 = true;
                        }
                    }
                }
            
                if (a0 && a1 && a2 && a3)
                    m_SpriteSheet.Add(spriteType);
            }
        }

        public void Unload() {
            m_SpritesAssetBundle?.Unload(true);
            m_SpritesAssetBundle = null;

            m_SpriteSheet.Clear();
            m_CachedTextures.Clear();
            m_SpriteCachedInformation.Clear();
        }

        private bool GetSpriteInfo(uint spriteID, out string filename, out Rect spriteRect, out Vector2 realSpriteSize) {
            SpriteType match = m_SpriteSheet.Find(m => spriteID >= m.firstspriteid && spriteID <= m.lastspriteid);
            if (match == null || match.spritetype < 1 || match.spritetype > 4) {
                filename = null;
                spriteRect = Rect.zero;
                realSpriteSize = Vector2.zero;
                return false;
            }
            
            realSpriteSize = s_SpriteTypesSizesRef[match.spritetype - 1] * Constants.FieldSize;
            uint realID = spriteID - match.firstspriteid;
            int texPerRow = (int)(AtlasTexture_Width / realSpriteSize.x);
            int x = (int)((realID % texPerRow) * realSpriteSize.x);
            int y = (int)(realID / texPerRow * realSpriteSize.y);
            y = AtlasTexture_Height - y - (int)realSpriteSize.y;

            filename = match.file;
            spriteRect = new Rect(x / (float)AtlasTexture_Width, y / (float)AtlasTexture_Height, realSpriteSize.x / AtlasTexture_Width, realSpriteSize.y / AtlasTexture_Height);
            return true;
        }

        public CachedSpriteInformation GetSprite(uint spriteID) {
            CachedSpriteInformation cachedInformation = FindCachedInformation(spriteID);
            if (cachedInformation != null)
                return cachedInformation;

            string filename;
            Rect spriteRect;
            Vector2 realSize;
            if (!GetSpriteInfo(spriteID, out filename, out spriteRect, out realSize))
                return null;
                

            Texture2D tex2D;
            if (!m_CachedTextures.TryGetValue(filename, out tex2D))
                tex2D = LoadTexture(filename);

            if (tex2D == null)
                return null;
            
            cachedInformation = new CachedSpriteInformation();
            cachedInformation.id = spriteID;
            cachedInformation.rect = spriteRect;
            cachedInformation.spriteSize = realSize;
            cachedInformation.texture = tex2D;

            BinaryInsert(cachedInformation);
            return cachedInformation;
        }

        private Texture2D LoadTexture(string filename) {
            Texture2D tex2D = m_SpritesAssetBundle.LoadAsset<Texture2D>(filename);
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
