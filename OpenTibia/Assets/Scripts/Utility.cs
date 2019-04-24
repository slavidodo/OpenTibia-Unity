namespace OpenTibiaUnity
{
    public static class Utility
    {
        public static int ManhattanDistance(UnityEngine.Vector3Int position, UnityEngine.Vector3Int other) {
            return System.Math.Abs(position.x - other.x) + System.Math.Abs(position.y - other.y);
        }

        public static float Distance(UnityEngine.Vector3Int position, UnityEngine.Vector3Int other) {
            return (float)System.Math.Sqrt(System.Math.Pow(position.x - other.x, 2) + System.Math.Pow(position.y - other.y, 2));
        }
    }
}
