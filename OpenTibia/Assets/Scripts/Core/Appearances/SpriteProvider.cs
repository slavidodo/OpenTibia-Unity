using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        public ushort SpriteType;
        public uint FirstSpriteId;
        public uint LastSpriteId;
        public uint AtlasId;
        public Texture2D Texture;
    }

    public class CachedSprite
    {
        public uint id { get; set; }
        public Texture2D texture { get; set; }
        public Rect rect { get; set; }
        public Vector2 size { get; set; }
    }

    public sealed class SpritesProvider {
        private static Vector2[] SpriteTypeSizeRefs = new Vector2[] {
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 2.0f),
            new Vector2(2.0f, 1.0f),
            new Vector2(2.0f, 2.0f)
        };

        List<SpriteTypeImpl> _spriteSheet;
        List<CachedSprite> _cachedSprites = new List<CachedSprite>();

        public SpritesProvider(System.IO.Stream stream) {
            using (var reader = new System.IO.BinaryReader(stream)) {
                uint total = reader.ReadUInt32();
                _spriteSheet = new List<SpriteTypeImpl>((int)total);
                for (uint i = 0; i < total; i++) {
                    var atlasId = reader.ReadUInt32();
                    var spriteType = reader.ReadUInt16();
                    var firstSpriteId = reader.ReadUInt32();
                    var lastSpriteId = reader.ReadUInt32();

                    var texture = new Texture2D(1, 1);
                    var size = reader.ReadUInt32();
                    var data = new byte[size];
                    reader.Read(data, 0, (int)size);
                    texture.LoadImage(data);

                    var spriteImpl = new SpriteTypeImpl {
                        AtlasId = atlasId,
                        FirstSpriteId = firstSpriteId,
                        LastSpriteId = lastSpriteId,
                        SpriteType = spriteType,
                        Texture = texture,
                    };

                    _spriteSheet.Add(spriteImpl);
                }
            }
        }

        public void Dispose() {
            _spriteSheet.Clear();
            _cachedSprites.Clear();
        }

        private SpriteLoadingStatus GetSpriteInfo(uint spriteId, out Rect rect, out Vector2 size, out Texture2D texture) {
            SpriteTypeImpl match = _spriteSheet.Find(m => spriteId >= m.FirstSpriteId && spriteId <= m.LastSpriteId);

            var loadingStatus = SpriteLoadingStatus.Failed;
            if (match != null) {
                loadingStatus = SpriteLoadingStatus.Completed;
                texture = match.Texture;
            } else {
                texture = null;
            }

            if (texture == null) {
                rect = Rect.zero;
                size = Vector2.zero;
                return loadingStatus;
            }

            size = SpriteTypeSizeRefs[match.SpriteType - 1] * Constants.FieldSize;
            uint realId = spriteId - match.FirstSpriteId;
            int texPerRow = (int)(texture.width / size.x);

            float x = (realId % texPerRow) * size.x;
            float y = texture.width - (realId / texPerRow * size.y) - size.y;
            rect = new Rect(x / texture.width, y / texture.height, size.x / texture.width, size.y / texture.height);
            return loadingStatus;
        }

        public SpriteLoadingStatus GetSprite(uint spriteId, out CachedSprite cachedSprite) {
            cachedSprite = FindCachedSprite(spriteId);
            if (cachedSprite != null)
                return SpriteLoadingStatus.Completed;

            var loadingStatus = GetSpriteInfo(spriteId, out Rect rect, out Vector2 size, out Texture2D texture);
            if (loadingStatus != SpriteLoadingStatus.Completed)
                return loadingStatus;

            cachedSprite = new CachedSprite {
                id = spriteId,
                rect = rect,
                size = size,
                texture = texture,
            };
            cachedSprite.id = spriteId;
            cachedSprite.rect = rect;
            cachedSprite.size = size;
            cachedSprite.texture = texture;

            InsertCachedSprite(cachedSprite);
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

        private void InsertCachedSprite(CachedSprite cachedSprite) {
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
