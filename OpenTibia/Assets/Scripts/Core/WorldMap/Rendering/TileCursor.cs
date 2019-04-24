using System;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public class TileCursor
    {
        private int m_CachedFrameCount = 0;
        private int m_NextUpdate = 0;
        private int m_CurrentFrame = -1;
        private Texture2D m_Texture2D;
        private Rect m_TextureRect;

        public int FrameDuration { get; set; }

        public TileCursor() {
            m_Texture2D = OpenTibiaUnity.GameManager.TileCursorTexture;
            m_CachedFrameCount = m_Texture2D.width / Constants.FieldSize;

            m_TextureRect = new Rect() {
                x = 0,
                y = 0,
                width = 1f / m_CachedFrameCount,
                height = 1f,
            };
        }

        public void Animate(int ticks) {
            if (ticks < m_NextUpdate)
                return;

            m_CurrentFrame = (m_CurrentFrame + 1) % m_CachedFrameCount;
            m_NextUpdate = ticks + FrameDuration;
        }

        public void DrawTo(float screenX, float screenY, Vector2 zoom, int ticks) {
            Animate(ticks);

            Rect screenRect = new Rect() {
                x = screenX * zoom.x,
                y = screenY * zoom.y,
                width = Constants.FieldSize * zoom.x,
                height = Constants.FieldSize * zoom.y,
            };

            m_TextureRect.x = (float)m_CurrentFrame / m_CachedFrameCount;
            Graphics.DrawTexture(screenRect, m_Texture2D, m_TextureRect, 0, 0, 0, 0);
        }
    }
}
