using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Appearances.Provider
{
    public class SpritesAsset
    {
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

        public static List<SpritesAsset> ParseJsonContents(JArray jArray) {
            List<SpritesAsset> spritesAssets = new List<SpritesAsset>();
            foreach (var jObject in jArray.Children<JObject>()) {
                var typeProperty = jObject.Property("type");
                if (typeProperty == null || typeProperty.Value.ToString() != "sprite")
                    continue;

                var fileProperty = jObject.Property("file");
                var firstIDProperty = jObject.Property("firstspriteid");
                var lastIDProperty = jObject.Property("lastspriteid");
                var spriteTypeProperty = jObject.Property("spritetype");
                if (fileProperty == null || firstIDProperty == null || lastIDProperty == null || spriteTypeProperty == null)
                    continue;

                SpritesAsset asset = new SpritesAsset((uint)firstIDProperty.Value,
                    (uint)lastIDProperty.Value,
                    (uint)spriteTypeProperty.Value,
                    fileProperty.Value.ToString());

                spritesAssets.Add(asset);
            }

            return spritesAssets;
        }
    }
}
