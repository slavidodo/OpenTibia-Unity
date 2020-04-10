using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy.Tables
{
    public class TableRow : BasicElement
    {
        public UnityUI.RawImage graphic = null;

        public UnityEngine.Color evenColor = new UnityEngine.Color32(72, 72, 72, 255);
        public UnityEngine.Color oddColor = new UnityEngine.Color32(72, 72, 72, 255); // (strips: 65 65 65 255)

        public void UpdateBackgroundColor() {
            bool isEven = (rectTransform.GetSiblingIndex() & 1) == 0;
            if (isEven)
                graphic.color = evenColor;
            else
                graphic.color = oddColor;
        }
    }
}
