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

        private uint m_FirstSpriteID = 0;
        private uint m_LastSpriteID = 0;
        private uint m_SpriteType = 0;
        private string m_FileName = null;

        public uint FirstSpriteID { get => m_FirstSpriteID; }
        public uint LastSpriteID { get => m_LastSpriteID; }
        public uint SpriteType { get => m_SpriteType; }
        public string FileName { get => m_FileName; }

        public SpritesAsset(uint firstSpriteId, uint lastSpriteId, uint spriteType, string filename) {
            m_FirstSpriteID = firstSpriteId;
            m_LastSpriteID = lastSpriteId;
            m_SpriteType = spriteType;
            m_FileName = filename;
        }

        public Rendering.CachedSpriteInformation GetCachedSpriteInformation(uint spriteID, AssetBundle assetBundle) {
            Texture2D tex2D = assetBundle.LoadAsset<Texture2D>(m_FileName);
            if (!tex2D)
                return null;

            var realSpriteSize = s_SpritesAssetSizesRef[m_SpriteType - 1] * Constants.FieldSize;
            uint realID = spriteID - m_FirstSpriteID;
            int texPerRow = AtlasTexture_Width / realSpriteSize.x;
            int x = (int)((realID % texPerRow) * realSpriteSize.x);
            int y = (int)(realID / texPerRow * realSpriteSize.y);
            y = AtlasTexture_Height - y - (int)realSpriteSize.y;
            
            var spriteRect = new Rect(x / (float)AtlasTexture_Width, y / (float)AtlasTexture_Height, realSpriteSize.x / AtlasTexture_Width, realSpriteSize.y / AtlasTexture_Height);

            return new Rendering.CachedSpriteInformation(spriteID, tex2D, spriteRect, realSpriteSize);
        }

        public static List<SpritesAsset> ParseJsonContents(JArray jArray) {
            List<SpritesAsset> spritesAssets = new List<SpritesAsset>();
            foreach (var @object in jArray.Children<JObject>()) {
                if (!@object.TryGetValue("type", out JToken typeToken) || (string)typeToken != "sprite")
                    continue;

                if (!@object.TryGetValue("file", out JToken fileToken)
                   || !@object.TryGetValue("spritetype", out JToken spriteTypeToken)
                   || !@object.TryGetValue("firstspriteid", out JToken firstSpriteIDToken)
                   || !@object.TryGetValue("lastspriteid", out JToken lastSpriteIDToken))
                    continue;

                try {
                    spritesAssets.Add(new SpritesAsset(
                        (uint)firstSpriteIDToken,
                        (uint)lastSpriteIDToken,
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
