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
        public uint FirstSpriteId;
        public uint LastSpriteId;
    }

    public class CachedSpriteInformation
    {
        public uint id = 0;
        public Texture2D texture = null;
        public Rect rect = Rect.zero;
        public Vector2 spriteSize = Vector2.zero;
    }

    public sealed class SpritesProvider {
        AssetBundle _spritesAssetBundle;

        List<SpriteTypeImpl> _spriteSheet = new List<SpriteTypeImpl>();
        Dictionary<string, Texture2D> _cachedTextures = new Dictionary<string, Texture2D>();
        List<CachedSpriteInformation> _spriteCachedInformation = new List<CachedSpriteInformation>();

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
            
            _spritesAssetBundle = spritesBundle;
            foreach (var @object in catalogObjects.Children<JObject>()) {
                var typeProperty = @object.Property("type");
                if (typeProperty == null || typeProperty.Value.ToString() != "sprite")
                    continue;
                
                if (!@object.TryGetValue("file", out JToken fileToken)
                    || !@object.TryGetValue("spritetype", out JToken spriteTypeToken)
                    || !@object.TryGetValue("firstspriteid", out JToken firstSpriteIdToken)
                    || !@object.TryGetValue("lastspriteid", out JToken lastSpriteIdToken))
                    continue;

                try {
                    _spriteSheet.Add(new SpriteTypeImpl() {
                        File = (string)fileToken,
                        SpriteType = (int)spriteTypeToken,
                        FirstSpriteId = (uint)firstSpriteIdToken,
                        LastSpriteId = (uint)lastSpriteIdToken
                    });
                } catch (System.InvalidCastException) {}
            }
        }

        public void Unload() {
            _spritesAssetBundle?.Unload(true);
            _spritesAssetBundle = null;

            _spriteSheet.Clear();
            _cachedTextures.Clear();
            _spriteCachedInformation.Clear();
        }

        private bool GetSpriteInfo(uint spriteId, out string filename, out Rect spriteRect, out Vector2 realSpriteSize, out Texture2D tex2D) {
            SpriteTypeImpl match = _spriteSheet.Find(m => spriteId >= m.FirstSpriteId && spriteId <= m.LastSpriteId);
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
            uint realId = spriteId - match.FirstSpriteId;
            int texPerRow = (int)(tex2D.width / realSpriteSize.x);

            float x = (realId % texPerRow) * realSpriteSize.x;
            float y = tex2D.width - (realId / texPerRow * realSpriteSize.y) - realSpriteSize.y;
            spriteRect = new Rect(x / tex2D.width, y / tex2D.height, realSpriteSize.x / tex2D.width, realSpriteSize.y / tex2D.height);
            return true;
        }

        public CachedSpriteInformation GetSprite(uint spriteId) {
            var cachedInformation = FindCachedInformation(spriteId);
            if (cachedInformation != null)
                return cachedInformation;

            string filename;
            Rect spriteRect;
            Vector2 realSize;
            Texture2D tex2D;
            if (!GetSpriteInfo(spriteId, out filename, out spriteRect, out realSize, out tex2D))
                return null;
            
            cachedInformation = new CachedSpriteInformation();
            cachedInformation.id = spriteId;
            cachedInformation.rect = spriteRect;
            cachedInformation.spriteSize = realSize;
            cachedInformation.texture = tex2D;

            BinaryInsert(cachedInformation);
            return cachedInformation;
        }

        private Texture2D GetOrLoadTexture(string filename) {
            Texture2D tex2D;
            if (_cachedTextures.TryGetValue(filename, out tex2D))
                return tex2D;

            tex2D = _spritesAssetBundle.LoadAsset<Texture2D>(filename);
            if (tex2D)
                _cachedTextures.Add(filename, tex2D);

            return tex2D;
        }

        private CachedSpriteInformation FindCachedInformation(uint spriteId) {
            int lastIndex = _spriteCachedInformation.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var cachedInformation = _spriteCachedInformation[tmpIndex];
                if (cachedInformation.id > spriteId)
                    index = tmpIndex + 1;
                else if (cachedInformation.id < spriteId)
                    lastIndex = tmpIndex - 1;
                else
                    return cachedInformation;
            }

            return null;
        }

        private void BinaryInsert(CachedSpriteInformation cachedInformation) {
            int index = 0;
            int lastIndex = _spriteCachedInformation.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundCache = _spriteCachedInformation[tmpIndex];
                if (foundCache.id < cachedInformation.id)
                    index = tmpIndex + 1;
                else if (foundCache.id > cachedInformation.id)
                    lastIndex = tmpIndex - 1;
                else
                    return;
            }

            _spriteCachedInformation.Insert(index, cachedInformation);
        }
    }
}
