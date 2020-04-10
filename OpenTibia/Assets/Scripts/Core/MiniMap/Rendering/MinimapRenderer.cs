using System.Collections.Generic;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.MiniMap.Rendering
{
    public sealed class MiniMapRenderer
    {
        private int _zoom = 0;
        private float _zoomScale = 1f;

        private RectInt _positionRect = new RectInt(0, 0, 0, 0);

        public int _positionX = 0;
        public int _positionY = 0;
        public int _positionZ = 0;

        public int PositionX {
            get { return _positionX; }
            set {
                if (_positionX != value) {
                    _positionX = Mathf.Clamp(value, Constants.MapMinX, Constants.MapMaxX);
                    UpdatePositionRect();
                }
            }
        }
        public int PositionY {
            get { return _positionY; }
            set {
                if (_positionY != value) {
                    _positionY = Mathf.Clamp(value, Constants.MapMinY, Constants.MapMaxY);
                    UpdatePositionRect();
                }
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
                if (_positionX != value.x || _positionY != value.y) {
                    _positionX = value.x;
                    _positionY = value.y;
                    UpdatePositionRect();
                }

                _positionZ = value.z;
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

                    UpdatePositionRect();
                }
            }
        }

        public MiniMapRenderer() {
            MiniMapStorage.onPositionChange.AddListener((_, position, __) => {
                Position = position;
            });
        }

        private void UpdatePositionRect() {
            var rectSize = new Vector2(Constants.MiniMapSideBarViewWidth / _zoomScale, Constants.MiniMapSideBarViewHeight / _zoomScale);
            _positionRect = new RectInt {
                x = _positionX - (int)(rectSize.x / 2),
                y = _positionY - (int)(rectSize.y / 2),
                width = (int)rectSize.x,
                height = (int)rectSize.y
            };
        }

        public RenderError Render(RenderTexture renderTarget) {
            if (MiniMapStorage == null || !OpenTibiaUnity.GameManager.IsGameRunning || !WorldMapStorage.Valid)
                return RenderError.MiniMapNotValid;

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(renderTarget);
            commandBuffer.ClearRenderTarget(false, true, Color.black);

            var zoom = new Vector2() {
                x = Screen.width / (float)Constants.MiniMapSideBarViewWidth,
                y = Screen.height / (float)Constants.MiniMapSideBarViewHeight,
            };

            zoom *= _zoomScale;

            var drawnSectors = new List<MiniMapSector>();

            // 4 is the maximum number of sectors to be drawn (pre-calculated)
            // todo; provide a function to calculate it (if someone plans to use minimap differently)
            Vector3Int curPosition = new Vector3Int(0, 0, PositionZ);
            for (int i = 0; i < 4; i++) {
                curPosition.x = _positionRect.x + i % 2 * _positionRect.width;
                curPosition.y = _positionRect.y + (i / 2) * _positionRect.height;

                var sector = MiniMapStorage.AcquireSector(curPosition, false);
                if (sector == null || drawnSectors.IndexOf(sector) != -1)
                    continue;

                drawnSectors.Add(sector);

                var sectorRect = new RectInt(sector.SectorX, sector.SectorY, Constants.MiniMapSectorSize, Constants.MiniMapSectorSize);
                var translation = sectorRect.position - _positionRect.position;

                Matrix4x4 transformation = Matrix4x4.TRS(translation * zoom, Quaternion.Euler(180, 0, 0), Constants.MiniMapSectorSize * zoom);

                var props = new MaterialPropertyBlock();
                props.SetTexture("_MainTex", sector.SafeDrawTexture);
                props.SetVector("_MainTex_UV", new Vector4(1, 1, 0, 0));
                props.SetFloat("_HighlightOpacity", 0);
                Utils.GraphicsUtility.Draw(commandBuffer, transformation, OpenTibiaUnity.GameManager.AppearanceTypeMaterial, props);
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();

            return RenderError.None;

        }

        public void TranslatePosition(int x, int y, int z) {
            var scale = 3 * Mathf.Pow(2, Constants.MiniMapSideBarZoomMax - _zoom);
            _positionX += (int)(x * scale);
            _positionY += (int)(y * scale);
            _positionZ += z;
            UpdatePositionRect();
        }
    }
}
