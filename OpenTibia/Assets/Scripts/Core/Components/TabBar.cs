using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class TabBar : Base.AbstractComponent
    {
        HorizontalOrVerticalLayoutGroup m_LayoutGroup;

        protected override void Awake() {
            base.Awake();

            m_LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }
        
        public void AddTabButton(TabButton button) {

        }
    }
}
