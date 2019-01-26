using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public abstract class ILightmapRenderer
    {
        public static Color32 ColorAboveGround = new Color32(200, 200, 255, 255);
        public static Color32 ColorBelowGround = new Color32(255, 255, 255, 255);

        protected List<bool>[] m_CachedLayerBrightnessInfo = new List<bool>[Constants.MapSizeZ];

        public abstract Color32 this[int index] { get; set; }

        public abstract Texture CreateLightmap();

        public abstract void SetLightSource(int x, int y, int z, uint brightness, Color32 color32);

        public abstract void SetFieldBrightness(int x, int y, int brightness, bool aboveGround);

        public abstract int GetFieldBrightness(int x, int y);

        public abstract float CalculateCreatureBrightnessFactor(Creatures.Creature creature, bool isLocalPlayer);

        public abstract int ToColorIndex(int x, int y);

        protected List<bool> GetLayerInformation(int z) {
            if (m_CachedLayerBrightnessInfo[z] == null)
                m_CachedLayerBrightnessInfo[z] = OpenTibiaUnity.WorldMapStorage.GetLightBlockingTilesForZLayer(z);
            return m_CachedLayerBrightnessInfo[z];
        }

        public static Color32 MulColor32(Color32 c, byte f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        public static Color32 MulColor32(Color32 c, float f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        public static Color32 MulColor32(Color32 c, int f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }
    }
}
