using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances
{
    public enum SpriteLoadingStatus
    {
        Loading,
        Failed,
        Completed,
    }

    public class SpriteTypeImpl
    {
        public string File;
        public int SpriteType;
        public uint FirstSpriteId;
        public uint LastSpriteId;
    }

    public class CachedSprite
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
        Dictionary<string, AssetBundleRequest> _activeTextureRequests = new Dictionary<string, AssetBundleRequest>();
        List<CachedSprite> _cachedSprites = new List<CachedSprite>();

        private static Vector2[] SpriteTypeSizeRefs = new Vector2[] {
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
            _activeTextureRequests.Clear();
            _cachedSprites.Clear();
        }

        private SpriteLoadingStatus GetSpriteInfo(uint spriteId, out string filename, out Rect spriteRect, out Vector2 realSpriteSize, out Texture2D tex2D) {
            SpriteTypeImpl match = _spriteSheet.Find(m => spriteId >= m.FirstSpriteId && spriteId <= m.LastSpriteId);

            SpriteLoadingStatus loadingStatus = SpriteLoadingStatus.Failed;
            if (match != null)
                loadingStatus = GetOrLoadTexture(match.File, out tex2D);
            else
                tex2D = null;

            if (match == null || match.SpriteType < 1 || match.SpriteType > 4 || tex2D == null) {
                filename = null;
                spriteRect = Rect.zero;
                realSpriteSize = Vector2.zero;
                return loadingStatus;
            }

            filename = match.File;
            realSpriteSize = SpriteTypeSizeRefs[match.SpriteType - 1] * Constants.FieldSize;
            uint realId = spriteId - match.FirstSpriteId;
            int texPerRow = (int)(tex2D.width / realSpriteSize.x);

            float x = (realId % texPerRow) * realSpriteSize.x;
            float y = tex2D.width - (realId / texPerRow * realSpriteSize.y) - realSpriteSize.y;
            spriteRect = new Rect(x / tex2D.width, y / tex2D.height, realSpriteSize.x / tex2D.width, realSpriteSize.y / tex2D.height);
            return loadingStatus;
        }

        public SpriteLoadingStatus GetSprite(uint spriteId, out CachedSprite cachedSprite) {
            cachedSprite = FindCachedSprite(spriteId);
            if (cachedSprite != null)
                return SpriteLoadingStatus.Completed;

            string filename;
            Rect spriteRect;
            Vector2 realSize;
            Texture2D tex2D;

            var loadingStatus = GetSpriteInfo(spriteId, out filename, out spriteRect, out realSize, out tex2D);
            if (loadingStatus != SpriteLoadingStatus.Completed)
                return loadingStatus;
            
            cachedSprite = new CachedSprite();
            cachedSprite.id = spriteId;
            cachedSprite.rect = spriteRect;
            cachedSprite.spriteSize = realSize;
            cachedSprite.texture = tex2D;

            BinaryInsert(cachedSprite);
            return SpriteLoadingStatus.Completed;
        }

        private SpriteLoadingStatus GetOrLoadTexture(string filename, out Texture2D tex2D) {
            if (_cachedTextures.TryGetValue(filename, out tex2D))
                return SpriteLoadingStatus.Completed;

            AssetBundleRequest asyncRequest;
            if (!_activeTextureRequests.TryGetValue(filename, out asyncRequest)) {
                asyncRequest = _spritesAssetBundle.LoadAssetAsync<Texture2D>(filename);
                _activeTextureRequests.Add(filename, asyncRequest);
            }

            if (!asyncRequest.isDone) {
                return SpriteLoadingStatus.Loading;
            } else if (asyncRequest.asset == null) {
                _activeTextureRequests.Remove(filename);
                return SpriteLoadingStatus.Failed;
            }

            tex2D = asyncRequest.asset as Texture2D;
            _activeTextureRequests.Remove(filename);
            _cachedTextures.Add(filename, tex2D);
            return SpriteLoadingStatus.Completed;
        }

        private CachedSprite FindCachedSprite(uint spriteId) {
            int lastIndex = _cachedSprites.Count - 1;
            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var cachedSprite = _cachedSprites[tmpIndex];
                if (cachedSprite.id > spriteId)
                    index = tmpIndex + 1;
                else if (cachedSprite.id < spriteId)
                    lastIndex = tmpIndex - 1;
                else
                    return cachedSprite;
            }

            return null;
        }

        private void BinaryInsert(CachedSprite cachedSprite) {
            int index = 0;
            int lastIndex = _cachedSprites.Count - 1;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var foundCache = _cachedSprites[tmpIndex];
                if (foundCache.id < cachedSprite.id)
                    index = tmpIndex + 1;
                else if (foundCache.id > cachedSprite.id)
                    lastIndex = tmpIndex - 1;
                else
                    return;
            }

            _cachedSprites.Insert(index, cachedSprite);
        }
    }
}
