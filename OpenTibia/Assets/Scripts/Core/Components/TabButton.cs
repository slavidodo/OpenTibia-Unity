using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Button), typeof(LayoutElement))]
    public class TabButton : Base.AbstractComponent
    {

        private LayoutElement _layoutElement;
        public LayoutElement layoutElement {
            get {
                if (!_layoutElement)
                    _layoutElement = GetComponent<LayoutElement>();
                return _layoutElement;
            }
        }
    }
}
