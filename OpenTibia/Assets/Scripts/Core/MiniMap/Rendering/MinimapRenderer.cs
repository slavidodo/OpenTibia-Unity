using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap.Rendering
{
    public sealed class MiniMapRenderer
    {
        private int m_Zoom = 0;
        private float m_ZoomScale = 1f;

        private Rect m_PositionRect = Rect.zero;

        public int m_PositionX = 0;
        public int m_PositionY = 0;
        public int m_PositionZ = 0;

        public int PositionX {
            get { return m_PositionX; }
            set {
                m_PositionX = Mathf.Clamp(value, Constants.MapMinX, Constants.MapMaxX);
            }
        }
        public int PositionY {
            get { return m_PositionY; }
            set {
                m_PositionY = Mathf.Clamp(value, Constants.MapMinY, Constants.MapMaxY);
            }
        }
        public int PositionZ {
            get { return m_PositionZ; }
            set {
                m_PositionZ = Mathf.Clamp(value, Constants.MapMinZ, Constants.MapMaxZ);
            }
        }

        public Vector3Int Position {
            get { return new Vector3Int(PositionX, PositionY, PositionZ); }
            set {
                PositionX = value.x;
                PositionY = value.y;
                PositionZ = value.z;
            }
        }

        private MiniMapStorage MiniMapStorage { get { return OpenTibiaUnity.MiniMapStorage; } }
        private WorldMap.WorldMapStorage WorldMapStorage { get { return OpenTibiaUnity.WorldMapStorage; } }

        public int Zoom {
            get { return m_Zoom; }
            set {
                value = Mathf.Clamp(value, Constants.MiniMapSideBarZoomMin, Constants.MiniMapSideBarZoomMax);
                if (m_Zoom != value) {
                    m_Zoom = value;
                    m_ZoomScale = Mathf.Pow(2, m_Zoom);
                }
            }
        }

        public MiniMapRenderer() {
            MiniMapStorage.onPositionChange.AddListener((_, position, __) => {
                Position = position;
            });
        }

        public RenderError Render(Material material) {
            if (MiniMapStorage == null || !OpenTibiaUnity.GameManager.IsGameRunning || !WorldMapStorage.Valid)
                return RenderError.MiniMapNotValid;

            GL.Clear(false, true, Color.black);

            if (PositionX < Constants.MapMinX || PositionX > Constants.MapMaxX
                || PositionY < Constants.MapMinY || PositionY > Constants.MapMaxY
                || PositionZ < Constants.MapMinZ || PositionZ > Constants.MapMaxZ) {
                return RenderError.PositionNotValid;
            }

            Vector2 screenZoom = new Vector2() {
                x = Screen.width * m_ZoomScale / Constants.MiniMapSideBarViewWidth,
                y = Screen.height * m_ZoomScale / Constants.MiniMapSideBarViewHeight,
            };

            Vector2 zoom = new Vector2() {
                x = Constants.MiniMapSideBarViewWidth / m_ZoomScale,
                y = Constants.MiniMapSideBarViewHeight / m_ZoomScale
            };

            m_PositionRect.x = PositionX - zoom.x / 2;
            m_PositionRect.y = PositionY - zoom.y / 2;
            m_PositionRect.width = zoom.x;
            m_PositionRect.height = zoom.y;
            
            var drawnSectors = new List<MiniMapSector>();
            
            Vector3Int position = Vector3Int.zero;

            var transformationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(m_ZoomScale, m_ZoomScale, 1));
            for (int i = 0; i < 4; i++) {
                position.Set(
                    (int)(m_PositionRect.x + i % 2 * m_PositionRect.width),
                    (int)(m_PositionRect.y + (int)(i / 2) * m_PositionRect.height),
                    PositionZ);
            
                var sector = MiniMapStorage.AcquireSector(position, false);
                if (drawnSectors.IndexOf(sector) == -1) {
                    drawnSectors.Add(sector);

                    var otherRect = new Rect(sector.SectorX, sector.SectorY, Constants.MiniMapSectorSize, Constants.MiniMapSectorSize);
                    var intersectingRect = Intersection(m_PositionRect, otherRect);

                    Rect screenRect = new Rect() {
                        x = (intersectingRect.x - m_PositionRect.x) * screenZoom.x,
                        y = (intersectingRect.y - m_PositionRect.y) * screenZoom.y,
                        width = intersectingRect.width * screenZoom.x,
                        height = intersectingRect.height * screenZoom.y
                    };

                    sector.ApplyPixelChanges();
                    Graphics.DrawTexture(screenRect, sector.Texture2D, OpenTibiaUnity.GameManager.DefaultMaterial);
                }
            }

            return RenderError.None;
        }

        public void TranslatePosition(int x, int y, int z) {
            var renderer = OpenTibiaUnity.MiniMapRenderer;
            var scale = 3 * Mathf.Pow(2, Constants.MiniMapSideBarZoomMax - renderer.Zoom);
            renderer.PositionX += (int)(x * scale);
            renderer.PositionY += (int)(y * scale);
            renderer.PositionZ += z;
        }

        public static Rect Intersection(Rect r1, Rect r2) {
            float r1x = r1.x;
            float r1y = r1.y;
            float r2x = r2.x;
            float r2y = r2.y;
            double r1x2 = r1x; r1x2 += r1.width;
            double r1y2 = r1y; r1y2 += r1.height;
            double r2x2 = r2x; r2x2 += r2.width;
            double r2y2 = r2y; r2y2 += r2.height;
            if (r1x < r2x) r1x = r2x;
            if (r1y < r2y) r1y = r2y;
            if (r1x2 > r2x2) r1x2 = r2x2;
            if (r1y2 > r2y2) r1y2 = r2y2;
            r1x2 -= r1x;
            r1y2 -= r1y;
            // tx2,ty2 will never overflow (they will never be
            // larger than the smallest of the two source w,h)
            // they might underflow, though...
            if (r1x2 < float.MinValue) r1x2 = float.MinValue;
            if (r1y2 < float.MinValue) r1y2 = float.MinValue;
            return new Rect(r1x, r1y, (float)r1x2, (float)r1y2);
        }
    }
}
