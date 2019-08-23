using System;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class TileCursor
    {
        private int _cachedFrameCount = 0;
        private int _nextUpdate = 0;
        private int _currentFrame = -1;
        private Texture2D _texture2D;
        private Rect _textureRect;

        public int FrameDuration { get; set; }

        public TileCursor() {
            _texture2D = OpenTibiaUnity.GameManager.TileCursorTexture;
            _cachedFrameCount = _texture2D.width / Constants.FieldSize;

            _textureRect = new Rect() {
                x = 0,
                y = 0,
                width = 1f / _cachedFrameCount,
                height = 1f,
            };
        }

        public void Animate(int ticks) {
            if (ticks < _nextUpdate)
                return;

            _currentFrame = (_currentFrame + 1) % _cachedFrameCount;
            _nextUpdate = ticks + FrameDuration;
        }

        public void DrawTo(float screenX, float screenY, Vector2 zoom, int ticks) {
            Animate(ticks);

            Rect screenRect = new Rect() {
                x = screenX * zoom.x,
                y = screenY * zoom.y,
                width = Constants.FieldSize * zoom.x,
                height = Constants.FieldSize * zoom.y,
            };

            _textureRect.x = (float)_currentFrame / _cachedFrameCount;
            Graphics.DrawTexture(screenRect, _texture2D, _textureRect, 0, 0, 0, 0);
        }
    }
}
