using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Appearances
{
    public class TextualEffectInstance : AppearanceInstance
    {
        private const int PhaseDuration = 100;
        private const int PhaseCount = 10;

        private int _phase = 0;
        private int _lastPhaseChange;

        private int _value;
        private string _text;
        private int _rawColor;
        private Color _color;

        private bool _integral = false;
        private bool _dirty = true;

        // dirty; should only be accessed if cache is built
        private Vector2 _size = Vector2.zero;
        private Mesh _mesh = null;
        
        public float Width {
            get {
                RebuildCache();
                return _size.x;
            }
        }

        public float Height {
            get {
                RebuildCache();
                return _size.y;
            }
        }

        public override int Phase { get => _phase; }
        
        public TextualEffectInstance(int color, string text, int value = -1) : base(0, null) {
            _rawColor = color;
            _color = Colors.ColorFrom8Bit(color);
            _lastPhaseChange = OpenTibiaUnity.TicksMillis;

            if (text == null) {
                text = value.ToString();
                _integral = true;
            }

            _value = value;
            _text = text;
        }

        public override void Draw(CommandBuffer commandBuffer, Vector2Int screenPosition, int patternX, int patternY, int patternZ, bool highlighted = false, float highlightOpacity = 0) {
            RebuildCache();
            var material = OpenTibiaUnity.GameManager.LabelOnscreenText.materialForRendering;
            var matrix = Matrix4x4.TRS(new Vector2(screenPosition.x, screenPosition.y), Quaternion.Euler(180, 0, 0), Vector3.one);
            commandBuffer.DrawMesh(_mesh, matrix, material);
        }

        public bool Merge(AppearanceInstance other) {
            var textualEffect = other as TextualEffectInstance;
            if (_integral && !!textualEffect && textualEffect._integral && textualEffect._phase <= 0 && _phase <= 0 && _rawColor == textualEffect._rawColor) {
                _value += textualEffect._value;
                _text = _value.ToString();
                _dirty = true;
                return true;
            }

            return false;
        }

        public override bool Animate(int ticks, int delay = 0) {
            var timeChange = Mathf.Abs(ticks - _lastPhaseChange);
            int phaseChange = timeChange / PhaseDuration;
            _phase += phaseChange;
            _lastPhaseChange += phaseChange * PhaseDuration;
            return _phase < PhaseCount;
        }

        public override AppearanceInstance Clone() {
            return new TextualEffectInstance(_rawColor, _text, _value);
        }

        private void RebuildCache() {
            if (_dirty) {
                _dirty = false;

                var textComponent = OpenTibiaUnity.GameManager.LabelOnscreenText; // an instance of a textmeshpro (ugui since we are in canvas)
                textComponent.color = _color;
                textComponent.text = _text;
                textComponent.ForceMeshUpdate(true); // force update now to obtain mesh data
                
                if (_mesh == null)
                    _mesh = new Mesh();
                
                var textMesh = textComponent.textInfo.meshInfo[0]; // clearly, there are no submeshes
                _mesh.vertices = textMesh.vertices;
                _mesh.normals = textMesh.normals;
                _mesh.uv = textMesh.uvs0;
                _mesh.uv2 = textMesh.uvs2;
                _mesh.triangles = textMesh.triangles;
                _mesh.tangents = textMesh.tangents;
                _mesh.colors32 = textMesh.colors32;
                _mesh.Optimize(); // optimize the mesh for rendering

                _size = textComponent.GetPreferredValues();
            }
        }
    }
}
