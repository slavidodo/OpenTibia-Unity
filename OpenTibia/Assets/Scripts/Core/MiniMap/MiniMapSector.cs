using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap
{
    public class MiniMapSector
    {
        private int[] _cost = new int[Constants.MiniMapSectorSize * Constants.MiniMapSectorSize];

        public bool UncommittedPixelChanges { get; set; } = false;
        public bool Dirty { get; set; } = false;
        public int SectorX { get; }
        public int SectorY { get; }
        public int SectorZ { get; }
        public int MinCost { get; private set; } = 255;
        public Texture2D Texture2D { get; private set; }
        public Texture2D WaypointsTexture2D { get; private set; }

        public Texture2D SafeDrawTexture {
            get {
                ApplyPixelChanges();
                return Texture2D;
            }
        }

        public MiniMapSector(int x, int y, int z) {
            SectorX = x;
            SectorY = y;
            SectorZ = z;

            Texture2D = new Texture2D(Constants.MiniMapSectorSize, Constants.MiniMapSectorSize, TextureFormat.ARGB32, true) {
                filterMode = FilterMode.Point
            };

            WaypointsTexture2D = new Texture2D(Constants.MiniMapSectorSize, Constants.MiniMapSectorSize, TextureFormat.ARGB32, true) {
                filterMode = FilterMode.Point
            };

            var blackColorArray = Enumerable.Repeat(Color.black, Constants.MiniMapSectorSize * Constants.MiniMapSectorSize).ToArray();
            Texture2D.SetPixels(blackColorArray);
            WaypointsTexture2D.SetPixels(blackColorArray);
            Texture2D.Apply();
            WaypointsTexture2D.Apply();

            for (int i = 0; i < _cost.Length; i++)
                _cost[i] = Constants.PathCostUndefined;
        }

        public static string GetSectorName(object @object, int sectorY = 0, int sectorZ = 0) {
            int sectorX;

            if (@object is MiniMapSector sector) {
                sectorX = sector.SectorX;
                sectorY = sector.SectorY;
                sectorZ = sector.SectorZ;
            } else if (@object is int) {
                sectorX = (int)@object;
            } else {
                throw new System.ArgumentException("MiniMapSector.s_GetSectorName: Invalid argument (must be MiniMapSector or int).");
            }

            sectorX = (int)System.Math.Floor(sectorX / (float)Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;
            sectorY = (int)System.Math.Floor(sectorY / (float)Constants.MiniMapSectorSize) * Constants.MiniMapSectorSize;

            return sectorX.ToString() + "_" + sectorY.ToString() + "_" + sectorZ.ToString();
        }

        public static int GetWaypointsSafe(int cost) {
            if (cost >= 0 && cost <= Constants.PathCostMax || cost == Constants.PathCostObstacle) {
                return cost;
            }
            return Constants.PathCostUndefined;
        }
        
        public void UpdateField(UnityEngine.Vector3Int position, uint color, int cost) {
            UpdateField(position.x, position.y, position.z, color, cost);
        }

        public void UpdateField(int x, int y, int _, uint color, int cost) {
            x %= Constants.MiniMapSectorSize;
            y %= Constants.MiniMapSectorSize;
            cost = GetWaypointsSafe(cost);

            int texelX = x, texelY = Constants.MiniMapSectorSize - y - 1;
            Texture2D.SetPixel(texelX, texelY, Colors.ColorFromARGB(color));

            byte costb = (byte)cost;
            if (costb > 210)
                costb = 210;
            WaypointsTexture2D.SetPixel(texelX, texelY, new Color32 { r = costb, g = costb, b = costb, a = 255 });

            _cost[y * Constants.MiniMapSectorSize + x] = cost;
            MinCost = System.Math.Min(MinCost, cost);
            UncommittedPixelChanges = true;
            Dirty = true;
        }

        public int GetCost(int x, int y, int _) {
            x = x % Constants.MiniMapSectorSize;
            y = y % Constants.MiniMapSectorSize;
            
            return _cost[y * Constants.MiniMapSectorSize + x];
        }

        public uint GetColour(int x, int y, int _) {
            x %= Constants.MiniMapSectorSize;
            y = Constants.MiniMapSectorSize - (y % Constants.MiniMapSectorSize ) - 1;
            var color = SafeDrawTexture.GetPixel(x, y);
            return Colors.ARGBFromColor(color);
        }

        public void ApplyPixelChanges() {
            if (UncommittedPixelChanges) {
                UncommittedPixelChanges = false;
                Texture2D.Apply();
                WaypointsTexture2D.Apply();
            }
        }

        
        public bool LoadSharedObject() {
            var sectorName = GetSectorName(this);

            var colorPath = Path.Combine(Application.persistentDataPath, "MiniMap/MiniMap_Color_" + sectorName + ".png");
            var waypointPath = Path.Combine(Application.persistentDataPath, "MiniMap/MiniMap_WaypointCost_" + sectorName + ".png");
            if (!File.Exists(colorPath) && !File.Exists(waypointPath))
                return false;

            if (File.Exists(colorPath)) {
                var colorBytes = File.ReadAllBytes(colorPath);
                if (!Texture2D.LoadImage(colorBytes))
                    return false;

                Texture2D.Apply();
            }

            if (File.Exists(waypointPath)) {
                var waypointBytes = File.ReadAllBytes(waypointPath);
                if (!WaypointsTexture2D.LoadImage(waypointBytes))
                    return false;

                WaypointsTexture2D.Apply();
                for (int x = 0; x < Constants.MiniMapSectorSize; x++) {
                    for (int y = 0; y < Constants.MiniMapSectorSize; y++) {
                        Color32 c = WaypointsTexture2D.GetPixel(x, y);
                        int cost = c.r;
                        if (cost >= 210)
                            cost = Constants.PathCostObstacle;

                        MinCost = Mathf.Min(MinCost, c.r);
                        _cost[(Constants.MiniMapSectorSize - y - 1) * Constants.MiniMapSectorSize + x] = c.r;
                    }
                }
            }

            UncommittedPixelChanges = false;
            return true;
        }

        public void SaveSharedObject() {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "MiniMap")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "MiniMap"));

            byte[] colorTexBytes = Texture2D.EncodeToPNG();
            byte[] waypointsTexBytes = WaypointsTexture2D.EncodeToPNG();

            var sectorName = GetSectorName(this);
            var colorPath = Path.Combine(Application.persistentDataPath, "MiniMap/MiniMap_Color_" + sectorName + ".png");
            var waypointsPath = Path.Combine(Application.persistentDataPath, "MiniMap/MiniMap_WaypointCost_" + sectorName + ".png");

            File.WriteAllBytes(colorPath, colorTexBytes);
            File.WriteAllBytes(waypointsPath, waypointsTexBytes);
            Dirty = false;
        }

        public bool Equals(MiniMapSector sector) {
            return Equals(sector.SectorX, sector.SectorY, sector.SectorZ);
        }

        public override string ToString() {
            return GetSectorName(this);
        }

        public bool Equals(int x, int y, int z) {
            return SectorX == x && SectorY == y && SectorZ == z;
        }

        public static bool operator !(MiniMapSector sector) {
            return sector == null;
        }

        public static bool operator true(MiniMapSector sector) {
            return !!sector;
        }

        public static bool operator false(MiniMapSector sector) {
            return !sector;
        }
    }
}
