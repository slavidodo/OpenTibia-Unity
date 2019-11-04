using System.Linq;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Utils
{
    public static class GraphicsUtility {
        private static Mesh _mesh;
        private static Mesh s_texMesh;

        public static Color TransparentColor = new Color(0, 0, 0, 0);

        static GraphicsUtility() {
            _mesh = new Mesh();
            _mesh.MarkDynamic();

            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            s_texMesh = gameObject.GetComponent<MeshFilter>().mesh;
            Object.Destroy(gameObject);

            s_texMesh.vertices = new Vector3[] {
                new Vector3(0, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
            };
        }

        public static void ClearWithTransparency() {
            ClearColor(TransparentColor);
        }

        public static void ClearColor(Color color) {
            GL.Clear(false, true, color);
        }

        public static void DrawRect(CommandBuffer commandBuffer, Rect rect, Vector3 scale, Color color) {
            _mesh.vertices = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(rect.width, 0, 0),
                new Vector3(rect.width, rect.height, 0),
                new Vector3(0, rect.height, 0),
            };

            _mesh.colors = Enumerable.Repeat(color, 4).ToArray();
            _mesh.SetIndices(new int[] { 0, 1, 2, 3 }, MeshTopology.Quads, 0);

            Matrix4x4 matrix = Matrix4x4.TRS(rect.position, Quaternion.Euler(180, 0, 0), scale);
            commandBuffer.DrawMesh(_mesh, matrix, OpenTibiaUnity.GameManager.InternalColoredMaterial);
        }

        public static void DrawTexture(CommandBuffer commandBuffer, Vector2 position, Vector3 scale, Material mat) {
            var matrix = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), scale);
            DrawTexture(commandBuffer, matrix, mat);
        }

        public static void DrawTexture(CommandBuffer commandBuffer, Vector2 position, Vector3 scale, Material mat, MaterialPropertyBlock props) {
            var matrix = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), scale);
            DrawTexture(commandBuffer, matrix, mat, props);
        }

        public static void DrawTexture(CommandBuffer commandBuffer, Matrix4x4 transformation, Material mat) {
            commandBuffer.DrawMesh(s_texMesh, transformation, mat, 0);
        }

        public static void DrawTexture(CommandBuffer commandBuffer, Matrix4x4 transformation, Material mat, MaterialPropertyBlock props) {
            commandBuffer.DrawMesh(s_texMesh, transformation, mat, 0, 0, props);
        }

        public static void DrawTextureInstanced(CommandBuffer commandBuffer, Matrix4x4[] matricies, int count, Material mat) {
            commandBuffer.DrawMeshInstanced(s_texMesh, 0, mat, 0, matricies, count);
        }

        public static void DrawTextureInstanced(CommandBuffer commandBuffer, Matrix4x4[] matricies, int count, Material mat, MaterialPropertyBlock props) {
            commandBuffer.DrawMeshInstanced(s_texMesh, 0, mat, 0, matricies, count, props);
        }
    }
}
