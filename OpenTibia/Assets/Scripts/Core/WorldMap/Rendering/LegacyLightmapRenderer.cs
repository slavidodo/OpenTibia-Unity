using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public sealed class LegacyLightmapRenderer : ILightmapRenderer
    {
        public const int ColorValuesPerVertex = 4;
        public const int ColorValues = (Constants.MapSizeX + 1) * (Constants.MapSizeY + 1) * ColorValuesPerVertex;

        private byte[] m_CurrentColorData = new byte[ColorValues];
        private byte[] m_OldColorData = new byte[ColorValues];
        private List<int> m_IndexData = new List<int>();
        private List<float> m_CoordinatesData = new List<float>();
        private Material m_LightMaterial;
        private Matrix4x4 m_LightMatrix;
        private RenderTexture m_RenderTexture;
        private Mesh m_LightMesh;

        public override Color32 this[int index] {
            get {
                return new Color32(m_CurrentColorData[index], m_CurrentColorData[index + 1], m_CurrentColorData[index + 2], m_CurrentColorData[index + 3]);
            }

            set {
                m_CurrentColorData[index] = value.r;
                m_CurrentColorData[index + 1] = value.g;
                m_CurrentColorData[index + 2] = value.b;
                m_CurrentColorData[index + 3] = value.a;
            }
        }
        
        public LegacyLightmapRenderer() {
            CreateBufferData();

            m_LightMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            var scaleVector = new Vector3(1f / (Constants.MapSizeX + 1), 1f / (Constants.MapSizeY + 1), 1);
            m_LightMatrix = Matrix4x4.Scale(scaleVector);

            m_RenderTexture = new RenderTexture(Constants.WorldMapScreenWidth, Constants.WorldMapScreenHeight, 0);
        }

        private void CreateBufferData() {
            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                for (int x = 0; x < Constants.MapSizeX + 1; x++) {
                    float scaledX = x / (Constants.MapSizeX + 1f);
                    float scaledY = y / (Constants.MapSizeY + 1f);
                    m_CoordinatesData.AddRange(new float[] { scaledX, scaledY });
                }
            }
            
            for (int y = 0; y < Constants.MapSizeY; y++) {
                int r = y * (Constants.MapSizeX + 1);
                for (int x = 0; x < Constants.MapSizeX; x++) {
                    int v0 = r + x;
                    int v1 = v0 + 1;
                    int v2 = v0 + Constants.MapSizeX + 1;
                    int v3 = v2 + 1;
                    
                    m_IndexData.AddRange(new int[] { v0, v1, v3, v0, v3, v2 });
                }
            }

            for (int i = 0; i < ColorValues; i++) {
                m_CurrentColorData[i] = 255;
                m_OldColorData[i] = 255;
            }
        }

        public override Texture CreateLightmap() {
            for (int x = 0; x < Constants.MapSizeX + 1; x++) {
                int destIndex = x * ColorValuesPerVertex;
                int sourceIndex = (x + Constants.MapSizeX + 1) * ColorValuesPerVertex;
                m_CurrentColorData[destIndex] = m_CurrentColorData[sourceIndex];
                m_CurrentColorData[destIndex + 1] = m_CurrentColorData[sourceIndex + 1];
                m_CurrentColorData[destIndex + 2] = m_CurrentColorData[sourceIndex + 2];
            }

            for (int y = 0; y < Constants.MapSizeY + 1; y++) {
                int destIndex = y * (Constants.MapSizeX + 1) * ColorValuesPerVertex;
                int sourceIndex = destIndex + ColorValuesPerVertex;
                m_CurrentColorData[destIndex] = m_CurrentColorData[sourceIndex];
                m_CurrentColorData[destIndex + 1] = m_CurrentColorData[sourceIndex + 1];
                m_CurrentColorData[destIndex + 2] = m_CurrentColorData[sourceIndex + 2];
            }

            var texture = InternalGenerateLightmapTexture();

            for (int z = 0; z < Constants.MapSizeZ; z++)
                m_CachedLayerBrightnessInfo[z] = null;

            return texture;
        }

        private Texture InternalGenerateLightmapTexture() {
            // backup state
            var previousRT = RenderTexture.active;
            
            // draw our lightmap
            RenderTexture.active = m_RenderTexture;

            GL.PushMatrix();
            //GL.MultMatrix(m_LightMatrix);
            GL.LoadOrtho();
            m_LightMaterial.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            GL.Clear(false, true, Color.white);
            
            foreach (int index in m_IndexData) {
                int colorIndex = index * 4;
                int vertexIndex = index * 2;

                byte r = m_CurrentColorData[colorIndex];
                byte g = m_CurrentColorData[colorIndex + 1];
                byte b = m_CurrentColorData[colorIndex + 2];
                byte a = m_CurrentColorData[colorIndex + 3];
                
                GL.Color(new Color32(r, g, b, a));
                GL.Vertex3(m_CoordinatesData[vertexIndex], 1f - m_CoordinatesData[vertexIndex + 1], 0);
            }
            
            GL.End();
            GL.PopMatrix();
            
            // restore state
            RenderTexture.active = previousRT;
            return m_RenderTexture;
        }
        
        public override void SetLightSource(int x, int y, int z, uint brightness, Color32 color32) {
            if (x < 0 || x > Constants.MapSizeX || y < 0 || y > Constants.MapSizeY || brightness <= 0 || true)
                return;
            
            int nrX = Mathf.Clamp(x - (int)brightness, 0, Constants.MapSizeX);
            int prX = Mathf.Clamp(x + (int)brightness, 0, Constants.MapSizeX);
            int nrY = Mathf.Clamp(y - (int)brightness, 0, Constants.MapSizeY);
            int prY = Mathf.Clamp(y + (int)brightness, 0, Constants.MapSizeY);

            var layerInformation = GetLayerInformation(z);

            var r = color32.r;
            var g = color32.g;
            var b = color32.b;

            var optionStorage = OpenTibiaUnity.OptionStorage;

            for (int j = nrY; j < prY; j++) {
                int dY = j + 1 - y;
                dY = dY * dY;
                for (int i = nrX; i < prX; i++) {
                    int dX = i + 1 - x;
                    dX = dX * dX;

                    float magnitude = (brightness - Mathf.Sqrt(dX * dY)) / 5f;
                    if (magnitude >= 0) {
                        if (magnitude > 1)
                            magnitude = 1;

                        int newRed = (int)(r * magnitude);
                        int newGreen = (int)(g * magnitude);
                        int newBlue = (int)(b * magnitude);
                        int index = j * Constants.MapSizeX + i;
                        if (layerInformation[index]) {
                            float levelSeparatorFactor = optionStorage.LevelSeparator / 100f;
                            newRed = (int)(newRed * levelSeparatorFactor);
                            newGreen = (int)(newGreen * levelSeparatorFactor);
                            newBlue = (int)(newBlue * levelSeparatorFactor);
                        }

                        var colorIndex = ToColorIndex(i, j);
                        
                        if (newRed > m_CurrentColorData[colorIndex])
                            m_CurrentColorData[colorIndex] = (byte)newRed;

                        if (newGreen > m_CurrentColorData[++colorIndex])
                            m_CurrentColorData[colorIndex] = (byte)newGreen;

                        if (newBlue > m_CurrentColorData[++colorIndex])
                            m_CurrentColorData[colorIndex] = (byte)newBlue;
                    }
                }
            }
        }

        public override void SetFieldBrightness(int x, int y, int brightness, bool aboveGround) {
            var ambientColor = OpenTibiaUnity.WorldMapStorage.AmbientCurrentColor;
            float brightnessF = Mathf.Clamp(brightness / 255f, 0, 1f);

            int r = (int)(ambientColor.r * brightnessF);
            int g = (int)(ambientColor.g * brightnessF);
            int b = (int)(ambientColor.b * brightnessF);

            Color32 staticColor;
            if (aboveGround)
                staticColor = ColorAboveGround;
            else
                staticColor = ColorBelowGround;

            float internalFactor = (OpenTibiaUnity.OptionStorage.AmbientBrightness / 100f) * ((255f - r) / 255f);

            r = Mathf.Clamp(r + (int)(staticColor.r * internalFactor), 0, 255);
            g = Mathf.Clamp(g + (int)(staticColor.g * internalFactor), 0, 255);
            b = Mathf.Clamp(b + (int)(staticColor.b * internalFactor), 0, 255);

            var index = ToColorIndex(x, y);
            m_CurrentColorData[index] = (byte)r;
            m_CurrentColorData[index + 1] = (byte)g;
            m_CurrentColorData[index + 2] = (byte)b;
        }

        public override int GetFieldBrightness(int x, int y) {
            if (x >= 0 && x < Constants.MapSizeX && y >= 0 && y < Constants.MapSizeY) {
                int index = ToColorIndex(x, y);
                return (m_CurrentColorData[index] + m_CurrentColorData[index + 1] + m_CurrentColorData[index + 2]) / 3;
            }

            return int.MaxValue;
        }
        
        public override int ToColorIndex(int x, int y) {
            return ((y + 1) * (Constants.MapSizeX + 1) + (x + 1)) * ColorValuesPerVertex;
        }

        public override float CalculateCreatureBrightnessFactor(Creatures.Creature creature, bool isLocalPlayer) {
            var mapPosition = OpenTibiaUnity.WorldMapStorage.ToMapClosest(creature.Position);
            var brightness = GetFieldBrightness(mapPosition.x, mapPosition.y);
            if (isLocalPlayer)
                brightness = Mathf.Max(brightness, Mathf.Max(2, Mathf.Min(creature.Brightness, 5)) * 51);

            return brightness / 255;
        }
    }
}
