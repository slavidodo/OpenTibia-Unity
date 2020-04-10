using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy.Tables
{
    [UnityEngine.RequireComponent(typeof(UnityUI.LayoutElement))]
    public class TableColumn : BasicElement
    {
        public UnityUI.RawImage graphic = null;
        public TMPro.TextMeshProUGUI textComponent = null;

        private UnityUI.LayoutElement _layoutElement;
        public UnityUI.LayoutElement layoutElement {
            get {
                if (!_layoutElement)
                    _layoutElement = GetComponent<UnityUI.LayoutElement>();
                return _layoutElement;
            }
        }

        public object sortValue = null;
    }
}
