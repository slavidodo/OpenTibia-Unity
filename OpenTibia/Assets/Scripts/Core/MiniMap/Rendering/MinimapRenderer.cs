using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap.Rendering
{
    public sealed class MiniMapRenderer
    {
        private int _zoom = 0;
        private float _zoomScale = 1f;

        private Rect _positionRect = Rect.zero;

        public int _positionX = 0;
        public int _positionY = 0;
        public int _positionZ = 0;

        public int PositionX {
            get { return _positionX; }
            set {
                _positionX = Mathf.Clamp(value, Constants.MapMinX, Constants.MapMaxX);
            }
        }
        public int PositionY {
            get { return _positionY; }
            set {
                _positionY = Mathf.Clamp(value, Constants.MapMinY, Constants.MapMaxY);
            }
        }
        public int PositionZ {
            get { return _positionZ; }
            set {
                _positionZ = Mathf.Clamp(value, Constants.MapMinZ, Constants.MapMaxZ);
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
            get { return _zoom; }
            set {
                value = Mathf.Clamp(value, Constants.MiniMapSideBarZoomMin, Constants.MiniMapSideBarZoomMax);
                if (_zoom != value) {
                    _zoom = value;
                    _zoomScale = Mathf.Pow(2, _zoom);
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
                x = Screen.width * _zoomScale / Constants.MiniMapSideBarViewWidth,
                y = Screen.height * _zoomScale / Constants.MiniMapSideBarViewHeight,
            };

            Vector2 zoom = new Vector2() {
                x = Constants.MiniMapSideBarViewWidth / _zoomScale,
                y = Constants.MiniMapSideBarViewHeight / _zoomScale
            };

            _positionRect.x = PositionX - zoom.x / 2;
            _positionRect.y = PositionY - zoom.y / 2;
            _positionRect.width = zoom.x;
            _positionRect.height = zoom.y;
            
            var drawnSectors = new List<MiniMapSector>();
            
            Vector3Int position = Vector3Int.zero;

            var transformationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_zoomScale, _zoomScale, 1));
            for (int i = 0; i < 4; i++) {
                position.Set(
                    (int)(_positionRect.x + i % 2 * _positionRect.width),
                    (int)(_positionRect.y + (int)(i / 2) * _positionRect.height),
                    PositionZ);
            
                var sector = MiniMapStorage.AcquireSector(position, false);
                if (drawnSectors.IndexOf(sector) == -1) {
                    drawnSectors.Add(sector);

                    var otherRect = new Rect(sector.SectorX, sector.SectorY, Constants.MiniMapSectorSize, Constants.MiniMapSectorSize);
                    var intersectingRect = Intersection(_positionRect, otherRect);

                    Rect screenRect = new Rect() {
                        x = (intersectingRect.x - _positionRect.x) * screenZoom.x,
                        y = (intersectingRect.y - _positionRect.y) * screenZoom.y,
                        width = intersectingRect.width * screenZoom.x,
                        height = intersectingRect.height * screenZoom.y
                    };

                    sector.ApplyPixelChanges();
                    Graphics.DrawTexture(screenRect, sector.Texture2D);
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
