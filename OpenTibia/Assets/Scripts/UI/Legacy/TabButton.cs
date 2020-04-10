using UnityEngine;
using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.LayoutElement))]
    public class TabButton : Button
    {
        private UnityUI.LayoutElement _layoutElement;
        public UnityUI.LayoutElement layoutElement {
            get {
                if (!_layoutElement)
                    _layoutElement = GetComponent<UnityUI.LayoutElement>();
                return _layoutElement;
            }
        }
    }
}
