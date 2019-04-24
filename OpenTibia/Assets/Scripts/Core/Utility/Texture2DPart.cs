using UnityEngine;

namespace OpenTibiaUnity.Core.Utility
{
    public class Texture2DPart
    {
        protected Texture2D m_Texture2D;
        protected Rect m_Rect;

        public Texture2D Texture2D { get => m_Texture2D; }
        public Rect Rect { get => m_Rect; }

        public Texture2DPart(Texture2D tex2D, Rect rect) {
            m_Rect = new Rect();
            SetTexture2DPartTo(tex2D, rect);
        }

        public void CopyFrom(Texture2DPart tex2DPart) {
            if (tex2DPart != null) {
                m_Texture2D = tex2DPart.Texture2D;
                m_Rect = tex2DPart.Rect;
            } else {
                SetTexture2DPartTo(null, null);
            }
        }

        public void SetTexture2DPartTo(Texture2D tex2D, Rect? rect) {
            m_Rect = rect.HasValue ? rect.Value : Rect.zero;
            m_Texture2D = tex2D;
        }
    }
}
