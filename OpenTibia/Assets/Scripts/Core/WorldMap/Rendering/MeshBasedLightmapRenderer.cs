using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public sealed class MeshBasedLightmapRenderer : ILightmapRenderer
    {
        private RenderTexture m_RenderTexture = new RenderTexture(Constants.WorldMapScreenWidth, Constants.WorldMapScreenHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        private Mesh m_LightMesh = new Mesh();
        private Material m_LightMaterial;
        private Matrix4x4 m_LightTransformationMatrix = new Matrix4x4();
        private Color32[] m_ColorData = new Color32[(Constants.MapSizeX + 1) * (Constants.MapSizeY + 1)];
        
        private int m_CachedScreenWidth = -1;
        private int m_CachedScreenHeight = -1;

        public override Color32 this[int index] {
            get => m_ColorData[index];
            set => m_ColorData[index] = value;
        }
        
        public MeshBasedLightmapRenderer() {
            CreateMeshBuffers();
            m_LightMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            for (int i = 0; i < m_ColorData.Length; i++)
                m_ColorData[i] = new Color32(255, 255, 255, 255);
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

            m_LightMesh.vertices = verticies.ToArray();
            m_LightMesh.triangles = indicies.ToArray();
        }

        public override Texture CreateLightmap() {
            for (int x = 0; x < Constants.MapSizeX + 1; x++)
                m_ColorData[x] = m_ColorData[x + Constants.MapSizeX + 1];
            
            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                int destIndex = y * (Constants.MapSizeX + 1);
                m_ColorData[destIndex] = m_ColorData[destIndex + 1];
            }

            m_LightMesh.colors32 = m_ColorData;
            if (m_CachedScreenHeight != Screen.height || m_CachedScreenWidth != Screen.width) {
                m_CachedScreenWidth = Screen.width;
                m_CachedScreenHeight = Screen.height;

                float zoomX = m_CachedScreenWidth / (float)Constants.MapSizeX;
                float zoomY = m_CachedScreenHeight / (float)Constants.MapSizeY;

                var scaleVector = new Vector3(zoomX, zoomY, 1);
                m_LightTransformationMatrix = Matrix4x4.Scale(scaleVector);
            }

            var previousRenderTexture = RenderTexture.active;
            m_RenderTexture.Release();
            RenderTexture.active = m_RenderTexture;

            m_LightMaterial.SetPass(0);
            Graphics.DrawMeshNow(m_LightMesh, m_LightTransformationMatrix);

            RenderTexture.active = previousRenderTexture;
            for (int z = 0; z < Constants.MapSizeZ; z++)
                m_CachedLayerBrightnessInfo[z] = null;
            return m_RenderTexture;
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
                        var color32 = MulColor32(defaultColor32, Mathf.Min(magnitude, 1f));
                        int index = j * Constants.MapSizeX + i;
                        if (layerInformation[index])
                            color32 = MulColor32(color32, OpenTibiaUnity.OptionStorage.FixedLightLevelSeparator / 100f);

                        var colorIndex = ToColorIndex(i, j);
                        var currentColor32 = m_ColorData[colorIndex];

                        if (color32.r > currentColor32.r || color32.g > currentColor32.g || color32.b > currentColor32.b) {
                            currentColor32.r = System.Math.Max(currentColor32.r, color32.r);
                            currentColor32.g = System.Math.Max(currentColor32.g, color32.g);
                            currentColor32.b = System.Math.Max(currentColor32.b, color32.b);
                            m_ColorData[colorIndex] = currentColor32;
                        }
                    }
                }
            }
        }

        public override void SetFieldBrightness(int x, int y, int brightness, bool aboveGround) {
            Color ambient = OpenTibiaUnity.WorldMapStorage.AmbientCurrentColor;
            brightness = Mathf.Clamp(brightness, 0, 255);

            Color32 color32 = MulColor32(ambient, brightness / 255f);
            Color32 staticColor = aboveGround ? Constants.ColorAboveGround : Constants.ColorBelowGround;

            float internalFactor = (OpenTibiaUnity.OptionStorage.AmbientBrightness / 100f) * ((255f - color32.r) / 255f);
            color32 = (Color)color32 + MulColor32(staticColor, internalFactor);

            int index = ToColorIndex(x, y);
            m_ColorData[index] = color32;
        }

        public override int GetFieldBrightness(int x, int y) {
            if (x >= 0 && x < Constants.MapSizeX && y >= 0 && y < Constants.MapSizeY) {
                var color32 = m_ColorData[ToColorIndex(x, y)];
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
