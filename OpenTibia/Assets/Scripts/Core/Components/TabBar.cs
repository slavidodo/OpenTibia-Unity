using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core.Components
{
    [RequireComponent(typeof(HorizontalOrVerticalLayoutGroup))]
    public class TabBar : Base.AbstractComponent
    {
        HorizontalOrVerticalLayoutGroup _layoutGroup;

        protected override void Awake() {
            base.Awake();

            _layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }
        
        public void AddTabButton(TabButton button) {

        }
    }
}
