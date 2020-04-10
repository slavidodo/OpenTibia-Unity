using System;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class TileCursor
    {
        private static Texture2D s_texture2D;
        private static int s_cachedFrameCount;

        private int _nextUpdate = 0;
        private int _currentFrame = -1;

        private MaterialPropertyBlock _props;

        public int FrameDuration { get; set; }

        static TileCursor() {
            s_texture2D = OpenTibiaUnity.GameManager.TileCursorTexture;
            s_cachedFrameCount = s_texture2D.width / Constants.FieldSize;
        }

        public void Animate(int ticks) {
            if (ticks < _nextUpdate)
                return;

            _currentFrame = (_currentFrame + 1) % s_cachedFrameCount;
            _nextUpdate = ticks + FrameDuration;

            _props = new MaterialPropertyBlock();
            _props.SetTexture("_MainTex", s_texture2D);
        }

        public void Draw(CommandBuffer commandBuffer, int screenX, int screenY, int ticks) {
            Animate(ticks);

            var position = new Vector3(screenX, screenY, 0);
            var scale = new Vector3(Constants.FieldSize, Constants.FieldSize, 0);
            var uv = new Vector4(1f / s_cachedFrameCount, 1f, (float)_currentFrame / s_cachedFrameCount, 0);
            _props.SetVector("_MainTex_UV", uv);

            Utils.GraphicsUtility.Draw(commandBuffer, position, scale, OpenTibiaUnity.GameManager.AppearanceTypeMaterial, _props);
        }
    }
}
