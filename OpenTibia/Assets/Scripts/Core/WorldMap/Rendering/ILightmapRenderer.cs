using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    internal abstract class ILightmapRenderer
    {
        protected List<bool>[] m_CachedLayerBrightnessInfo = new List<bool>[Constants.MapSizeZ];

        internal abstract Color32 this[int index] { get; set; }

        internal abstract Texture CreateLightmap();

        internal abstract void SetLightSource(int x, int y, int z, uint brightness, Color32 color32);

        internal abstract void SetFieldBrightness(int x, int y, int brightness, bool aboveGround);

        internal abstract int GetFieldBrightness(int x, int y);

        internal abstract float CalculateCreatureBrightnessFactor(Creatures.Creature creature, bool isLocalPlayer);

        internal abstract int ToColorIndex(int x, int y);

        protected List<bool> GetLayerInformation(int z) {
            if (m_CachedLayerBrightnessInfo[z] == null)
                m_CachedLayerBrightnessInfo[z] = OpenTibiaUnity.WorldMapStorage.GetLightBlockingTilesForZLayer(z);
            return m_CachedLayerBrightnessInfo[z];
        }

        internal static Color32 MulColor32(Color32 c, byte f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        internal static Color32 MulColor32(Color32 c, float f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }

        internal static Color32 MulColor32(Color32 c, int f) {
            c.r = (byte)Mathf.Clamp(c.r * f, 0, 255);
            c.g = (byte)Mathf.Clamp(c.g * f, 0, 255);
            c.b = (byte)Mathf.Clamp(c.b * f, 0, 255);
            return c;
        }
    }
}
