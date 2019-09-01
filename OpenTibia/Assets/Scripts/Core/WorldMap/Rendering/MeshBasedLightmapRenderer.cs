using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public sealed class MeshBasedLightmapRenderer : LightmapRenderer
    {
        private RenderTexture _renderTexture = new RenderTexture(Constants.WorldMapScreenWidth, Constants.WorldMapScreenHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        private Mesh _lightMesh = new Mesh();
        private MeshRenderer meshRendere;
        private Matrix4x4 _lightTransformationMatrix = new Matrix4x4();
        private Color32[] _colorData = new Color32[(Constants.MapSizeX + 1) * (Constants.MapSizeY + 1)];
        
        private int _cachedScreenWidth = -1;
        private int _cachedScreenHeight = -1;

        public override Color32 this[int index] {
            get => _colorData[index];
            set => _colorData[index] = value;
        }

        public MeshBasedLightmapRenderer() {
            CreateMeshBuffers();

            for (int i = 0; i < _colorData.Length; i++)
                _colorData[i] = new Color32(255, 255, 255, 255);
        }

        private void CreateMeshBuffers() {
            List<Vector3> verticies = new List<Vector3>((Constants.MapSizeX + 1) * (Constants.MapSizeY + 1));
            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                for (int x = 0; x < Constants.MapSizeX + 1; x++) {
                    verticies.Add(new Vector3(x, y));
                }
            }

            List<int> indicies = new List<int>();
            for (int y = 0; y < Constants.MapSizeY; y++) {
                int row = y * (Constants.MapSizeX + 1);
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    int v0 = x + row;
                    int v1 = v0 + 1;
                    int v2 = x + row + Constants.MapSizeX + 1;
                    int v3 = v2 + 1;

                    indicies.AddRange(new int[] { v0, v1, v3, v0, v3, v2 });
                }
            }

            _lightMesh.vertices = verticies.ToArray();
            _lightMesh.triangles = indicies.ToArray();
        }

        public override Texture CreateLightmap() {
            for (int x = 0; x < Constants.MapSizeX + 1; x++)
                _colorData[x] = _colorData[x + Constants.MapSizeX + 1];
            
            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                int destIndex = y * (Constants.MapSizeX + 1);
                _colorData[destIndex] = _colorData[destIndex + 1];
            }

            _lightMesh.colors32 = _colorData;
            if (_cachedScreenHeight != Screen.height || _cachedScreenWidth != Screen.width) {
                _cachedScreenWidth = Screen.width;
                _cachedScreenHeight = Screen.height;

                float zoomX = _cachedScreenWidth / (float)Constants.MapSizeX;
                float zoomY = _cachedScreenHeight / (float)Constants.MapSizeY;

                var scaleVector = new Vector3(zoomX, zoomY, 1);
                _lightTransformationMatrix = Matrix4x4.Scale(scaleVector);
            }

            var previousRenderTexture = RenderTexture.active;
            _renderTexture.Release();
            RenderTexture.active = _renderTexture;

            OpenTibiaUnity.GameManager.InternalColoredMaterial.SetPass(0);
            Graphics.DrawMeshNow(_lightMesh, _lightTransformationMatrix);

            RenderTexture.active = previousRenderTexture;
            for (int z = 0; z < Constants.MapSizeZ; z++)
                CachedLayerBrightnessInfo[z] = null;
            return _renderTexture;
        }

        public override void SetLightSource(int x, int y, int z, uint brightness, Color32 defaultColor32) {
            if (x < 0 || x > Constants.MapSizeX || y < 0 || y > Constants.MapSizeY || brightness <= 0)
                return;

            int nrX = Mathf.Clamp(x - (int)brightness, 0, Constants.MapSizeX);
            int prX = Mathf.Clamp(x + (int)brightness, 0, Constants.MapSizeX);
            int nrY = Mathf.Clamp(y - (int)brightness, 0, Constants.MapSizeY);
            int prY = Mathf.Clamp(y + (int)brightness, 0, Constants.MapSizeY);

            var layerInformation = GetLayerInformation(z);
            
            for (int j = nrY; j < prY; j++) {
                int dY = j + 1 - y;
                dY = dY * dY;
                for (int i = nrX; i < prX; i++) {
                    int dX = i + 1 - x;
                    dX = dX * dX;

                    float magnitude = (brightness - Mathf.Sqrt(dX + dY)) / 5f;
                    if (magnitude >= 0) {
                        var color32 = Utils.Utility.MulColor32(defaultColor32, Mathf.Min(magnitude, 1f));
                        int index = j * Constants.MapSizeX + i;
                        if (layerInformation[index])
                            color32 = Utils.Utility.MulColor32(color32, OpenTibiaUnity.OptionStorage.FixedLightLevelSeparator / 100f);

                        var colorIndex = ToColorIndex(i, j);
                        var currentColor32 = _colorData[colorIndex];

                        if (color32.r > currentColor32.r || color32.g > currentColor32.g || color32.b > currentColor32.b) {
                            currentColor32.r = System.Math.Max(currentColor32.r, color32.r);
                            currentColor32.g = System.Math.Max(currentColor32.g, color32.g);
                            currentColor32.b = System.Math.Max(currentColor32.b, color32.b);
                            _colorData[colorIndex] = currentColor32;
                        }
                    }
                }
            }
        }

        public override void SetFieldBrightness(int x, int y, int brightness, bool aboveGround) {
            brightness = Mathf.Clamp(brightness, 0, 255);

            Color color = Utils.Utility.MulColor32(OpenTibiaUnity.WorldMapStorage.AmbientCurrentColor, brightness / 255f);
            Color staticColor = aboveGround ? Constants.ColorAboveGround : Constants.ColorBelowGround;

            color += staticColor * (OpenTibiaUnity.OptionStorage.AmbientBrightness / 100f) * (1f - color.r);

            int index = ToColorIndex(x, y);
            _colorData[index] = color;
        }

        public override int GetFieldBrightness(int x, int y) {
            if (x >= 0 && x < Constants.MapSizeX && y >= 0 && y < Constants.MapSizeY) {
                var color32 = _colorData[ToColorIndex(x, y)];
                return (color32.r + color32.g + color32.b) / 3;
            }

            return int.MaxValue;
        }

        public override int ToColorIndex(int x, int y) {
            return (y + 1) * (Constants.MapSizeX + 1) + (x + 1);
        }

        public override float CalculateCreatureBrightnessFactor(Creatures.Creature creature, bool isLocalPlayer) {
            var mapPosition = OpenTibiaUnity.WorldMapStorage.ToMapClosest(creature.Position);
            var brightness = GetFieldBrightness(mapPosition.x, mapPosition.y);
            if (isLocalPlayer)
                brightness = Mathf.Max(brightness, Mathf.Max(2, Mathf.Min(creature.Brightness, 5)) * 51);

            return brightness / 255f;
        }
    }
}
