using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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
        public Vector4 uv { get; set; }
        public Vector2 size { get; set; }
        public MaterialPropertyBlock materialProperyBlock { get; set; }

        public void GenerateMaterialProps(MaterialPropertyBlock props) {
            props.SetTexture("_MainTex", texture);
            props.SetVector("_MainTex_UV", uv);
        }

        public void GenerateChannelsMaterialProps(MaterialPropertyBlock props) {
            props.SetTexture("_ChannelsTex", texture);
            props.SetVector("_ChannelsTex_UV", uv);
        }
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

        public IEnumerator Parse(System.IO.Stream stream) {
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

                    // load a batch of 5 textures at once
                    if (i % 5 == 0)
                        yield return null;
                }
            }
        }

        public void Dispose() {
            _spriteSheet.Clear();
            _cachedSprites.Clear();
        }

        private SpriteLoadingStatus GetSpriteInfo(uint spriteId, out Vector4 uv, out Vector2 size, out Texture2D texture) {
            SpriteTypeImpl match = _spriteSheet.Find(m => spriteId >= m.FirstSpriteId && spriteId <= m.LastSpriteId);

            var loadingStatus = SpriteLoadingStatus.Failed;
            if (match != null) {
                loadingStatus = SpriteLoadingStatus.Completed;
                texture = match.Texture;
            } else {
                texture = null;
            }

            if (texture == null) {
                uv = Vector4.zero;
                size = Vector2.zero;
                return loadingStatus;
            }

            size = SpriteTypeSizeRefs[match.SpriteType - 1] * Constants.FieldSize;
            uint realId = spriteId - match.FirstSpriteId;
            int texPerRow = (int)(texture.width / size.x);

            float x = (realId % texPerRow) * size.x;
            float y = texture.width - (realId / texPerRow * size.y) - size.y;
            uv = new Vector4(size.x / texture.width, size.y / texture.height, x / texture.width, y / texture.height);
            return loadingStatus;
        }

        public SpriteLoadingStatus GetSprite(uint spriteId, out CachedSprite cachedSprite) {
            cachedSprite = FindCachedSprite(spriteId);
            if (cachedSprite != null)
                return SpriteLoadingStatus.Completed;

            var loadingStatus = GetSpriteInfo(spriteId, out Vector4 uv, out Vector2 size, out Texture2D texture);
            if (loadingStatus != SpriteLoadingStatus.Completed)
                return loadingStatus;

            cachedSprite = new CachedSprite {
                id = spriteId,
                uv = uv,
                size = size,
                texture = texture,
                materialProperyBlock = new MaterialPropertyBlock(),
            };

            cachedSprite.GenerateMaterialProps(cachedSprite.materialProperyBlock);

            InsertCachedSprite(cachedSprite);
            return SpriteLoadingStatus.Completed;
        }

        private CachedSprite FindCachedSprite(uint spriteId) {
            int l = 0, r = _cachedSprites.Count - 1;
            while (l <= r) {
                int i = l + (r - l) / 2;
                var other = _cachedSprites[i];
                if (other.id > spriteId)
                    l = i + 1;
                else if (other.id < spriteId)
                    r = i - 1;
                else
                    return other;
            }

            return null;
        }

        private void InsertCachedSprite(CachedSprite sprite) {
            int l = 0, r = _cachedSprites.Count - 1;
            while (l <= r) {
                int i = l + (r - l) / 2;
                var other = _cachedSprites[i];
                if (other.id < sprite.id)
                    l = i + 1;
                else if (other.id > sprite.id)
                    r = i - 1;
                else
                    return;
            }

            _cachedSprites.Insert(l, sprite);
        }
    }
}
