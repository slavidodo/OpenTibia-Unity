using System.Linq;
using UnityEngine;

namespace OpenTibiaUnity.Core.Utils
{
    public static class GraphicsUtility {
        private static Mesh _mesh;

        static GraphicsUtility() {
            _mesh = new Mesh();
            _mesh.MarkDynamic();
        }

        public static void ClearWithTransparency() {
            GL.Clear(false, true, new Color(0, 0, 0, 0));
        }

        public static void DrawRect(Rect rect, Vector3 scale, Color color) {
            _mesh.vertices = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(rect.width, 0, 0),
                new Vector3(rect.width, rect.height, 0),
                new Vector3(0, rect.height, 0),
            };

            _mesh.colors = Enumerable.Repeat(color, 4).ToArray();
            _mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);
            _mesh.Optimize();

            if (!OpenTibiaUnity.GameManager.InternalColoredMaterial.SetPass(0))
                return;

            Matrix4x4 matrix = Matrix4x4.TRS(rect.position, Quaternion.Euler(180, 0, 0), scale);
            Graphics.DrawMeshNow(_mesh, matrix);
        }
    }
}
