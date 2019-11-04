using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.WorldMap.Rendering
{
    public abstract class LightmapRenderer
    {
        protected List<bool>[] CachedLayerBrightnessInfo = new List<bool>[Constants.MapSizeZ];

        public abstract Color32 this[int index] { get; set; }

        public abstract Mesh CreateLightmap();

        public abstract void SetLightSource(int x, int y, int z, uint brightness, Color32 color32);

        public abstract void SetFieldBrightness(int x, int y, int brightness, bool aboveGround);

        public abstract int GetFieldBrightness(int x, int y);

        public abstract float CalculateCreatureBrightnessFactor(Creatures.Creature creature, bool isLocalPlayer);

        public abstract int ToColorIndex(int x, int y);

        protected List<bool> GetLayerInformation(int z) {
            if (CachedLayerBrightnessInfo[z] == null)
                CachedLayerBrightnessInfo[z] = OpenTibiaUnity.WorldMapStorage.GetLightBlockingTilesForZLayer(z);
            return CachedLayerBrightnessInfo[z];
        }
    }
}
