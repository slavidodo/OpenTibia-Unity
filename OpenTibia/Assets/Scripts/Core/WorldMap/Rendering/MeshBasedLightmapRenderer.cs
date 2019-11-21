using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public sealed class MeshBasedLightmapRenderer : LightmapRenderer
    {
        private const int VertexCount = (Constants.MapSizeX + 1) * (Constants.MapSizeY + 1);

        private Mesh _lightMesh = new Mesh();
        private Color32[] _colorData = new Color32[VertexCount];

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
            var vertices = new List<Vector3>(VertexCount);
            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                for (int x = 0; x < Constants.MapSizeX + 1; x++)
                    vertices.Add(new Vector3(x, y));
            }

            var indices = new List<int>(Constants.MapSizeX * Constants.MapSizeY * 6);
            for (int y = 0; y < Constants.MapSizeY; y++) {
                int rowsize = Constants.MapSizeX + 1;
                int row = y * rowsize;
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    int v0 = x + row;
                    int v1 = v0 + 1;
                    int v2 = v0 + rowsize;
                    int v3 = v2 + 1;
                    indices.AddRange(new int[] { v0, v1, v3, v0, v2, v3 });
                }
            }

            _lightMesh.vertices = vertices.ToArray();
            _lightMesh.triangles = indices.ToArray();
        }

        public override Mesh CreateLightmap() {
            // lerp inner colormaps
            for (int x = 0; x < Constants.MapSizeX + 1; x++)
                _colorData[x] = _colorData[x + Constants.MapSizeX + 1];

            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                int destIndex = y * (Constants.MapSizeX + 1);
                _colorData[destIndex] = _colorData[destIndex + 1];
            }

            for (int z = 0; z < Constants.MapSizeZ; z++)
                CachedLayerBrightnessInfo[z] = null;

            _lightMesh.colors32 = _colorData;
            return _lightMesh;
        }

        public override void SetLightSource(int x, int y, int z, uint brightness, Color32 defaultColor32) {
            if (x < 0 || x > Constants.MapSizeX || y < 0 || y > Constants.MapSizeY || brightness <= 0)
                return;

            var layerInformation = GetLayerInformation(z);

            int nrX = Mathf.Max(x - (int)brightness, 0);
            int prX = Mathf.Min(x + (int)brightness, Constants.MapSizeX);
            int nrY = Mathf.Max(y - (int)brightness, 0);
            int prY = Mathf.Min(y + (int)brightness, Constants.MapSizeY);
            for (int j = nrY; j < prY; j++) {
                int dY = j + 1 - y;
                dY = dY * dY;
                for (int i = nrX; i < prX; i++) {
                    int dX = i + 1 - x;
                    dX = dX * dX;

                    float magnitude = (brightness - Mathf.Sqrt(dX + dY)) / 5f;
                    if (magnitude >= 0) {
                        if (magnitude > 1)
                            magnitude = 1;

                        var color32 = Utils.Utility.MulColor32(defaultColor32, magnitude);
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
            if (x < Constants.MapSizeX && y < Constants.MapSizeY) {
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
