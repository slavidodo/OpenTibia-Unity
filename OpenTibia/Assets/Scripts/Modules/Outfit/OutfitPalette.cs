using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;
using CommandBuffer = UnityEngine.Rendering.CommandBuffer;
using GraphicsUtility = OpenTibiaUnity.Core.Utils.GraphicsUtility;

namespace OpenTibiaUnity.Modules.Outfit
{
    public class OutfitPalette : UI.Legacy.BasicElement,
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public class ColorChangeEvent : UnityEvent<int, int> {}

        // static fields
        private static Vector2Int margin = new Vector2Int(1, 2);
        private static Vector2Int padding = new Vector2Int(2, 2);

        // serialized fields
        [SerializeField]
        private Texture _colorTexture = null;
        [SerializeField]
        private Texture _colorTextureOn = null;
        [SerializeField]
        private UnityUI.RawImage _image = null;

        // fields
        private bool _mustRedraw = true;
        private bool _cancelHighlight = false;
        private int _activeColor = 15;
        private int _highlightedColor = -1;
        private RenderTexture _renderTexture;

        // events
        public ColorChangeEvent onColorChanged = new ColorChangeEvent();

        protected override void Start() {
            base.Start();

            _mustRedraw = true;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            if (_renderTexture != null) {
                _renderTexture.Release();
                _renderTexture = null;
            }
        }

        private void OnGUI() {
            Event e = Event.current;
            if (!_mustRedraw || e.type != EventType.Repaint)
                return;

            if (_renderTexture == null) {
                int width = Core.Colors.HSI_H_STEPS * _colorTexture.width + (Core.Colors.HSI_H_STEPS - 1) * margin.x;
                int height = Core.Colors.HSI_SI_VALUES * _colorTexture.height + (Core.Colors.HSI_SI_VALUES - 1) * margin.y;
                _renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                _renderTexture.filterMode = FilterMode.Point;
                _image.texture = _renderTexture;
            }

            int totalColors = Core.Colors.HSI_H_STEPS * Core.Colors.HSI_SI_VALUES;
            var texturePositions = new Vector2[totalColors];
            var colorPositions = new Vector2[totalColors];
            var colors = new Vector4[totalColors];
            var uvs = new Vector4[totalColors];

            int index = 0;
            for (int j = 0; j < Core.Colors.HSI_SI_VALUES; j++) {
                for (int i = 0; i < Core.Colors.HSI_H_STEPS; i++) {
                    int colorIndex = j * Core.Colors.HSI_H_STEPS + i;
                    if (colorIndex == _activeColor || colorIndex == _highlightedColor) {
                        totalColors--;
                        continue;
                    }

                    var position = new Vector2 {
                        x = i * (margin.x + _colorTexture.width),
                        y = j * (margin.y + _colorTexture.height)
                    };

                    texturePositions[index] = position;
                    colorPositions[index] = position + padding;
                    colors[index] = Core.Colors.ColorFromHSI(colorIndex);
                    uvs[index] = new Vector4(1, 1, 0, 0);
                    index++;
                }
            }

            var zoom = new Vector2 {
                x = (float)Screen.width / _renderTexture.width,
                y = (float)Screen.height / _renderTexture.height
            };

            using (CommandBuffer commandBuffer = new CommandBuffer()) {
                commandBuffer.SetRenderTarget(_renderTexture);
                commandBuffer.ClearRenderTarget(false, true, GraphicsUtility.TransparentColor);
                commandBuffer.SetViewMatrix(
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                    OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

                DrawColorItems(commandBuffer, totalColors, texturePositions, colorPositions, uvs, colors, padding);

                if (_activeColor != -1)
                    DrawColorItem(commandBuffer, _activeColor, margin, padding);

                if (_highlightedColor != -1)
                    DrawColorItem(commandBuffer, _highlightedColor, margin, padding);

                Graphics.ExecuteCommandBuffer(commandBuffer);
            }

            _mustRedraw = false;
        }

        public void OnPointerDown(PointerEventData eventData) {
            _highlightedColor = FindItemUnderMouse(eventData.position);
            _cancelHighlight = false;
            _mustRedraw = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            int index = FindItemUnderMouse(eventData.position);
            if (index == _highlightedColor && index != -1)
                SetActiveColor(index);

            _highlightedColor = -1;
            _mustRedraw = true;
        }

        public void OnDrag(PointerEventData eventData) {
            if (_highlightedColor != -1) {
                int index = FindItemUnderMouse(eventData.position);
                bool cancelHighlight = index != _highlightedColor;
                _mustRedraw = cancelHighlight != _cancelHighlight;
                _cancelHighlight = cancelHighlight;
            }
        }

        private int FindItemUnderMouse(Vector2 mousePosition) {
            var relativePosition = CalculateAbsoluteMousePosition(mousePosition);
            for (int i = 0; i < Core.Colors.HSI_H_STEPS; i++) {
                for (int j = 0; j < Core.Colors.HSI_SI_VALUES; j++) {
                    int startx = i * (margin.x + _colorTexture.width);
                    int endx = startx + _colorTexture.width;
                    int starty = j * (margin.y + _colorTexture.height);
                    int endy = starty + _colorTexture.width;

                    if (relativePosition.x >= startx && relativePosition.x <= endx && relativePosition.y >= starty && relativePosition.y <= endy)
                        return j * Core.Colors.HSI_H_STEPS + i;
                }
            }

            return -1;
        }

        public override Vector2 CalculateAbsoluteMousePosition(Vector2 mousePosition) {
            var relativePosition = base.CalculateAbsoluteMousePosition(mousePosition);
            relativePosition.x = relativePosition.x - transform.position.x;
            relativePosition.y = transform.position.y - relativePosition.y;
            return relativePosition;
        }

        public void SetActiveColor(int index, bool cancelDispatch = false) {
            if (_activeColor != index) {
                int oldColor = _activeColor;
                _activeColor = index;

                if (!cancelDispatch)
                    onColorChanged.Invoke(oldColor, _activeColor);
            }
        }

        private void DrawColorItems(CommandBuffer commandBuffer, int count, Vector2[] texPositions, Vector2[] colorPositions, Vector4[] uvs, Vector4[] colors, Vector2 padding) {
            var texScale = new Vector2(_colorTexture.width, _colorTexture.height);
            var colorScale = texScale - padding * 2; 
            
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetTexture("_MainTex", _colorTexture);
            props.SetVectorArray("_MainTex_UV", uvs);
            GraphicsUtility.DrawInstanced(commandBuffer, texPositions, texScale, count, OpenTibiaUnity.GameManager.AppearanceTypeMaterial, props);

            props = new MaterialPropertyBlock();
            props.SetVectorArray("_Color", colors);
            GraphicsUtility.DrawInstanced(commandBuffer, colorPositions, colorScale, count, OpenTibiaUnity.GameManager.ColoredMaterial, props);
        }

        private void DrawColorItem(CommandBuffer commandBuffer, int index, Vector2 margin, Vector2 padding) {
            var texScale = new Vector2(_colorTexture.width, _colorTexture.height);
            var colorScale = texScale - padding * 2;

            Texture texture;
            if (index == _activeColor || (index == _highlightedColor && !_cancelHighlight))
                texture = _colorTextureOn;
            else
                texture = _colorTexture;

            int i = index % Core.Colors.HSI_H_STEPS;
            int j = index / Core.Colors.HSI_H_STEPS;
            var position = new Vector2 {
                x = i * (margin.x + _colorTexture.width),
                y = j * (margin.y + _colorTexture.height)
            };

            var props = new MaterialPropertyBlock();
            props.SetTexture("_MainTex", texture);
            props.SetVector("_MainTex_UV", new Vector4(1, 1, 0, 0));
            GraphicsUtility.Draw(commandBuffer, position, texScale, OpenTibiaUnity.GameManager.AppearanceTypeMaterial, props);

            position += padding;
            props = new MaterialPropertyBlock();
            props.SetVector("_Color", Core.Colors.ColorFromHSI(index));
            GraphicsUtility.Draw(commandBuffer, position, colorScale, OpenTibiaUnity.GameManager.ColoredMaterial, props);
        }
    }
}
