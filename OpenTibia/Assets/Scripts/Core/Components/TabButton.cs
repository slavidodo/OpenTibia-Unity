using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(Button), typeof(LayoutElement))]
    public class TabButton : Base.AbstractComponent
    {

        private LayoutElement m_LayoutElement;
        public LayoutElement layoutElement {
            get {
                if (!m_LayoutElement)
                    m_LayoutElement = GetComponent<LayoutElement>();
                return m_LayoutElement;
            }
        }
    }
}
