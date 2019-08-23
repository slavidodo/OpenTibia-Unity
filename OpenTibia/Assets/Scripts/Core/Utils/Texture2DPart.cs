using UnityEngine;

namespace OpenTibiaUnity.Core.Utils
{
    public class Texture2DPart
    {
        protected Texture2D _texture2D;
        protected Rect _rect;

        public Texture2D Texture2D { get => _texture2D; }
        public Rect Rect { get => _rect; }

        public Texture2DPart(Texture2D tex2D, Rect rect) {
            _rect = new Rect();
            SetTexture2DPartTo(tex2D, rect);
        }

        public void CopyFrom(Texture2DPart tex2DPart) {
            if (tex2DPart != null) {
                _texture2D = tex2DPart.Texture2D;
                _rect = tex2DPart.Rect;
            } else {
                SetTexture2DPartTo(null, null);
            }
        }

        public void SetTexture2DPartTo(Texture2D tex2D, Rect? rect) {
            _rect = rect.HasValue ? rect.Value : Rect.zero;
            _texture2D = tex2D;
        }
    }
}
