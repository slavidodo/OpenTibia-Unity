using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.WorldMap
{
    public class OnscreenMessage
    {
        private static int s_NextId = 0;

        protected int _ttl;
        protected int _id;
        protected int _visibleSince;
        protected int _speakerLevel;
        protected MessageModeType _mode;
        protected string _speaker;
        protected string _text;
        protected string _richText = null;
        
        private bool _dirty = true;
        private Vector2 _size = Vector2.zero;
        private Mesh _mesh = null;

        public int VisibleSince { get => _visibleSince; set => _visibleSince = value; }
        public int TTL { get => _ttl; set => _ttl = value; }
        public string Text { get => _text; }
        public string RichText { get => _richText; }

        public Vector2 Size {
            get {
                RebuildCache();
                return _size;
            }
        }

        public float Width { get => Size.x; }
        public float Height { get => Size.y; }
        
        public OnscreenMessage(int statementId, string speaker, int speakerLevel, MessageModeType mode, string text) {
            if (statementId <= 0)
                _id = --s_NextId;
            else
                _id = statementId;

            _speaker = speaker;
            _speakerLevel = speakerLevel;
            _mode = mode;
            _text = text;
            _visibleSince = int.MaxValue;
            _ttl = (30 + _text.Length / 3) * 100;
        }
        
        public void FormatMessage(string text, uint textARGB, uint highlightARGB) {
            _richText = StringHelper.RichTextSpecialChars(_text);
            if (_mode == MessageModeType.NpcFrom)
                _richText = StringHelper.HighlightNpcTalk(_richText, highlightARGB);
            else if (_mode == MessageModeType.Loot && OpenTibiaUnity.GameManager.ClientVersion >= 1200)
                _richText = StringHelper.HighlightLootValue(_richText, (ushort objectId) => {
                    return OpenTibiaUnity.CyclopediaStorage.GetObjectColor(objectId);
                });

            if (text != null)
                _richText = text + _richText;

            _richText = string.Format("<align=center><color=#{0:X6}>{1}</color>", textARGB, _richText);

            _dirty = true;
            RebuildCache();
        }

        private void RebuildCache() {
            // TODO; ensure we are in a render cycle since verticies 
            // to avoid 

            if (_dirty) {
                _dirty = false;
                var textComponent = OpenTibiaUnity.GameManager.LabelOnscreenMessage;
                textComponent.fontSize = 11;
                textComponent.fontStyle = TMPro.FontStyles.Bold;
                textComponent.alignment = TMPro.TextAlignmentOptions.Center;
                textComponent.text = _richText;
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

        public void Draw(CommandBuffer commandBuffer, Vector2 screenPosition) {
            RebuildCache();

            // TRS matrix:
            // 1. The mesh is anchored to the center, and since our calculations is based on the
            // // topleft of the rectangle, we must translate it by half the size (pivot * zoom)
            // 2. Rotate the mesh by PI (rad) on the x-axis
            // 3. Scale the text by the precalculated text zoom to make sure the text size remains
            // // the same no matter what the screen zoom is.
            var material = OpenTibiaUnity.GameManager.LabelOnscreenMessage.materialForRendering;
            var matrix = Matrix4x4.TRS(screenPosition, Quaternion.Euler(180, 0, 0), Vector3.one);
            commandBuffer.DrawMesh(_mesh, matrix, material);
        }
    }
}
