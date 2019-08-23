using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    public class SpritesAsset
    {
        public const int AtlasTexture_Width = 512;
        public const int AtlasTexture_Height = 512;

        private static Vector2Int[] s_SpritesAssetSizesRef = new Vector2Int[] {
            new Vector2Int(1, 1),
            new Vector2Int(1, 2),
            new Vector2Int(2, 1),
            new Vector2Int(2, 2)
        };

        private uint _firstSprite_id = 0;
        private uint _lastSprite_id = 0;
        private uint _spriteType = 0;
        private string _fileName = null;

        public uint FirstSprite_id { get => _firstSprite_id; }
        public uint LastSprite_id { get => _lastSprite_id; }
        public uint SpriteType { get => _spriteType; }
        public string FileName { get => _fileName; }

        public SpritesAsset(uint firstSpriteId, uint lastSpriteId, uint spriteType, string filename) {
            _firstSprite_id = firstSpriteId;
            _lastSprite_id = lastSpriteId;
            _spriteType = spriteType;
            _fileName = filename;
        }

        public Rendering.CachedSpriteInformation GetCachedSpriteInformation(uint sprite_id, AssetBundle assetBundle) {
            Texture2D tex2D = assetBundle.LoadAsset<Texture2D>(_fileName);
            if (!tex2D)
                return null;

            var realSpriteSize = s_SpritesAssetSizesRef[_spriteType - 1] * Constants.FieldSize;
            uint real_id = sprite_id - _firstSprite_id;
            int texPerRow = AtlasTexture_Width / realSpriteSize.x;
            int x = (int)((real_id % texPerRow) * realSpriteSize.x);
            int y = (int)(real_id / texPerRow * realSpriteSize.y);
            y = AtlasTexture_Height - y - (int)realSpriteSize.y;
            
            var spriteRect = new Rect(x / (float)AtlasTexture_Width, y / (float)AtlasTexture_Height, realSpriteSize.x / AtlasTexture_Width, realSpriteSize.y / AtlasTexture_Height);

            return new Rendering.CachedSpriteInformation(sprite_id, tex2D, spriteRect, realSpriteSize);
        }

        public static List<SpritesAsset> ParseJsonContents(JArray jArray) {
            List<SpritesAsset> spritesAssets = new List<SpritesAsset>();
            foreach (var @object in jArray.Children<JObject>()) {
                if (!@object.TryGetValue("type", out JToken typeToken) || (string)typeToken != "sprite")
                    continue;

                if (!@object.TryGetValue("file", out JToken fileToken)
                   || !@object.TryGetValue("spritetype", out JToken spriteTypeToken)
                   || !@object.TryGetValue("firstspriteid", out JToken firstSprite_idToken)
                   || !@object.TryGetValue("lastspriteid", out JToken lastSprite_idToken))
                    continue;

                try {
                    spritesAssets.Add(new SpritesAsset(
                        (uint)firstSprite_idToken,
                        (uint)lastSprite_idToken,
                        (uint)spriteTypeToken,
                        (string)fileToken));
                } catch (System.InvalidCastException) { }
            }

            return spritesAssets;
        }

        public static bool operator !(SpritesAsset instance) {
            return instance == null;
        }

        public static bool operator true(SpritesAsset instance) {
            return !!instance;
        }

        public static bool operator false(SpritesAsset instance) {
            return !instance;
        }
    }
}
