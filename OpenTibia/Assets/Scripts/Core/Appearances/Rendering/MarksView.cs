using System.Collections.Generic;
using UnityEngine;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Core.Appearances.Rendering
{
    public class MarksView
    {
        private static Color[] s_FrameColors;

        private const int FrameSizesCount = 15;
        private const int CacheDimention = 10;

        static MarksView() {
            s_FrameColors = new Color[Marks.MarksNumTotal];
            for (int i = 0; i < s_FrameColors.Length; i++)
                s_FrameColors[i] = Colors.ColorFrom8Bit(i);

            s_FrameColors[Marks.MarkAim] = Colors.White;
            s_FrameColors[Marks.MarkAimAttack] = Colors.LightRed;
            s_FrameColors[Marks.MarkAimFollow] = Colors.LightGreen;
            s_FrameColors[Marks.MarkAttack] = Colors.DarkRed;
            s_FrameColors[Marks.MarkFollow] = Colors.DarkGreen;
        }

        private List<MarksViewInformation> _marksViewInformations;

        public uint MarksStartSize { get; set; }

        public MarksView(uint marksStartSize = 0) {
            if (marksStartSize >= FrameSizesCount)
                throw new System.Exception("MarksView.MarksView: Invalid marks start size.");
            MarksStartSize = marksStartSize;
            _marksViewInformations = new List<MarksViewInformation>();
        }

        public void AddMarkToView(MarkType markType, uint thinkness) {
            if (thinkness != Constants.MarkThicknessThin && thinkness != Constants.MarkThicknessBold) {
                throw new System.Exception("MarksView.addMarkToView: Invalid marks thickness: " + thinkness);
            }

            uint size = MarksStartSize;
            foreach (var markInformation in _marksViewInformations)
                size = size + markInformation.MarkThickness;

            if (size + thinkness >= FrameSizesCount)
                throw new System.Exception("MarksView.AddMarkToView: Adding this mark will exceed the maximum frame size");

            MarksViewInformation information = new MarksViewInformation();
            information.MarkType = markType;
            information.MarkThickness = thinkness;

            _marksViewInformations.Add(information);
        }

        public void DrawMarks(CommandBuffer commandBuffer, Marks marks, int screenX, int screenY) {
            var texture = OpenTibiaUnity.GameManager.MarksViewTexture;
            var material = OpenTibiaUnity.GameManager.MarksViewMaterial;

            var position = new Vector2(screenX, screenY);
            var scale = new Vector2(Constants.FieldSize, Constants.FieldSize);
            var transformation = Matrix4x4.TRS(position, Quaternion.Euler(180, 0, 0), scale);

            var size = MarksStartSize;
            foreach (var information in _marksViewInformations) {
                if (!marks.IsMarkSet(information.MarkType))
                    continue;

                uint eightBit = marks.GetMarkColor(information.MarkType);
                if (eightBit > Marks.MarksNumTotal)
                    continue;

                Color color = GetMarksColor(eightBit);

                var uv = new Vector4() {
                    z = size * Constants.FieldSize / (float)texture.width,
                    w = (Constants.MarkThicknessBold - information.MarkThickness) * Constants.FieldSize / (float)texture.height,
                    x = Constants.FieldSize / (float)texture.width,
                    y = Constants.FieldSize / (float)texture.height,
                };

                var props = new MaterialPropertyBlock();
                props.SetTexture("_MainTex", texture);
                props.SetVector("_MainTex_ST", uv);
                props.SetColor("_Color", color);
                Utils.GraphicsUtility.Draw(commandBuffer, transformation, material, props);
            }
        }

        public static Color GetMarksColor(uint eightBit) {
            Color color;
            if (eightBit > Marks.MarkNumColors)
                color = s_FrameColors[(int)eightBit];
            else
                color = Colors.ColorFrom8Bit((int)eightBit);
            return color;
        }
    }

    public class MarksViewInformation
    {
        public MarkType MarkType { get; set; } = MarkType.None;
        public uint MarkThickness { get; set; } = 1;
    }
}
