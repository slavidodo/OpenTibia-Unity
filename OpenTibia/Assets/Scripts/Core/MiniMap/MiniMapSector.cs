using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.MiniMap
{
    public class MiniMapSector
    {
        private int[] m_Cost = new int[Constants.MiniMapSectorSize * Constants.MiniMapSectorSize];

        public bool UncommittedPixelChanges { get; set; } = false;
        public bool Dirty { get; set; } = false;
        public int SectorX { get; }
        public int SectorY { get; }
        public int SectorZ { get; }
        public int MinCost { get; private set; } = 255;
        public UnityEngine.Texture2D Texture2D { get; private set; }
        private UnityEngine.Texture2D m_WaypointsTexture2D;

        public MiniMapSector(int x, int y, int z) {
            SectorX = x;
            SectorY = y;
            SectorZ = z;

            Texture2D = new Texture2D(Constants.MiniMapSectorSize, Constants.MiniMapSectorSize, TextureFormat.ARGB32, true) {
                filterMode = FilterMode.Point
            };

            Texture2D.SetPixels(Enumerable.Repeat(Color.black, Constants.MiniMapSectorSize * Constants.MiniMapSectorSize).ToArray());
            Texture2D.Apply();

            m_WaypointsTexture2D = new Texture2D(Constants.MiniMapSectorSize, Constants.MiniMapSectorSize, TextureFormat.ARGB32, true) {
                filterMode = FilterMode.Point
            };

            m_WaypointsTexture2D.SetPixels(Enumerable.Repeat(Colors.ColorFrom8Bit(Constants.PathCostUndefined), Constants.MiniMapSectorSize * Constants.MiniMapSectorSize).ToArray());
            m_WaypointsTexture2D.Apply();

            for (int i = 0; i < m_Cost.Length; i++)
                m_Cost[i] = Constants.PathCostUndefined;
        }

        public static string GetSectorName(object obj, int sectorY = 0, int sectorZ = 0) {
            int sectorX;

            if (obj is MiniMapSector sector) {
                sectorX = sector.SectorX;
                sectorY = sector.SectorY;
                sectorZ = sector.SectorZ;
            } else if (obj is int) {
                sectorX = (int)obj;
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

            Texture2D.SetPixel(x, Constants.MiniMapSectorSize - y - 1, Colors.ColorFromARGB(color));
            m_WaypointsTexture2D.SetPixel(x, Constants.MiniMapSectorSize - y - 1, Colors.ColorFrom8Bit(cost));

            m_Cost[y * Constants.MiniMapSectorSize + x] = cost;
            MinCost = System.Math.Min(MinCost, cost);
            UncommittedPixelChanges = true;
            Dirty = true;
        }

        public int GetCost(int x, int y, int _) {
            x = x % Constants.MiniMapSectorSize;
            y = y % Constants.MiniMapSectorSize;
            
            return m_Cost[y * Constants.MiniMapSectorSize + x];
        }

        public uint GetColour(int x, int y, int _) {
            x %= Constants.MiniMapSectorSize;
            y = Constants.MiniMapSectorSize - (y % Constants.MiniMapSectorSize ) - 1;
            var color = Texture2D.GetPixel(x, y);
            return Colors.ARGBFromColor(color);
        }

        public void ApplyPixelChanges() {
            if (UncommittedPixelChanges) {
                UncommittedPixelChanges = false;
                Texture2D.Apply();
                m_WaypointsTexture2D.Apply();
            }
        }

        
        public bool LoadSharedObject() {
            var sectorName = GetSectorName(this);

            var colorPath = Path.Combine(Application.persistentDataPath, "MiniMap/Colors_" + sectorName + ".png");
            var waypointPath = Path.Combine(Application.persistentDataPath, "MiniMap/Waypoints_" + sectorName + ".png");
            if (!File.Exists(colorPath) || !File.Exists(waypointPath))
                return false;

            var colorBytes = File.ReadAllBytes(colorPath);
            var waypointBytes = File.ReadAllBytes(waypointPath);
            if (colorBytes == null || waypointBytes == null)
                return false;

            if (!Texture2D.LoadImage(colorBytes))
                return false;

            if (!m_WaypointsTexture2D.LoadImage(waypointBytes)) {
                Texture2D.SetPixels(Enumerable.Repeat(Color.black, Constants.MiniMapSectorSize * Constants.MiniMapSectorSize).ToArray());
                Texture2D.Apply();
                UncommittedPixelChanges = false;
                return false;
            }

            for (int x = 0; x < Constants.MiniMapSectorSize; x++) {
                for (int y = 0; y < Constants.MiniMapSectorSize; y++) {
                    var cost = Colors.EightBitFromColor(m_WaypointsTexture2D.GetPixel(x, y));
                    MinCost = Mathf.Min(MinCost, cost);
                    m_Cost[(Constants.MiniMapSectorSize - y - 1) * Constants.MiniMapSectorSize + x] = cost;
                }
            }
            
            Texture2D.Apply();
            UncommittedPixelChanges = false;
            return true;
        }

        public void SaveSharedObject() {
            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "MiniMap")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "MiniMap"));

            byte[] colorTexBytes = Texture2D.EncodeToPNG();
            byte[] waypointsTexBytes = m_WaypointsTexture2D.EncodeToPNG();

            var sectorName = GetSectorName(this);
            var colorPath = Path.Combine(Application.persistentDataPath, "MiniMap/Colors_" + sectorName + ".png");
            var waypointsPath = Path.Combine(Application.persistentDataPath, "MiniMap/Waypoints_" + sectorName + ".png");
            
            File.WriteAllBytes(colorPath, colorTexBytes);
            File.WriteAllBytes(waypointsPath, waypointsTexBytes);

            //Debug.Log(colorPath);
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
