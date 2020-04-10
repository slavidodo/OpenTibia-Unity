using System;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class CreatureStatus : IEquatable<CreatureStatus>
    {
        private string _name;
        private Color32 _color;

        private Vector2 _size;
        private Mesh _mesh;
        private bool _dirty = true;

        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    _dirty = true;
                }
            }
        }

        public Color32 Color {
            get => _color;
            set {
                if (_color.r != value.r || _color.g != value.g || _color.b != value.b || _color.a != value.a) {
                    _color = value;
                    _dirty = true;
                }
            }
        }

        public Vector2 Size {
            get {
                RebuildCache();
                return _size;
            }
        }

        public float Width { get => Size.x; }
        public float Height { get => Size.y; }

        public CreatureStatus(string name, Color32 color) {
            _name = name;
            _color = color;
            _dirty = true;
        }

        public void Draw(CommandBuffer commandBuffer, Vector2 screenPosition) {
            RebuildCache();
            var material = OpenTibiaUnity.GameManager.LabelOnscreenText.materialForRendering;
            var matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), Vector3.one);
            commandBuffer.DrawMesh(_mesh, matrix, material);
        }

        private void RebuildCache() {
            if (_dirty) {
                _dirty = false;
                var textComponent = OpenTibiaUnity.GameManager.LabelOnscreenText;
                textComponent.fontSize = 12;
                textComponent.fontStyle = TMPro.FontStyles.Normal;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;
                textComponent.text = _name;
                textComponent.color = _color;
                textComponent.ForceMeshUpdate(true);

                _size = textComponent.GetRenderedValues();
                if (_mesh == null)
                    _mesh = new Mesh();

                var textMesh = textComponent.textInfo.meshInfo[0];
                _mesh.vertices = textMesh.vertices;
                _mesh.normals = textMesh.normals;
                _mesh.uv = textMesh.uvs0;
                _mesh.uv2 = textMesh.uvs2;
                _mesh.triangles = textMesh.triangles;
                _mesh.tangents = textMesh.tangents;
                _mesh.colors32 = textMesh.colors32;
                _mesh.Optimize();
            }
        }

        public bool Equals(CreatureStatus other) {
            return _name == other._name &&
                _color.r == other._color.r &&
                _color.g == other._color.g &&
                _color.b == other._color.b &&
                _color.a == other._color.a;
        }
    }
}
