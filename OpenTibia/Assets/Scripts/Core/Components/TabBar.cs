using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    [RequireComponent(typeof(UnityUI.HorizontalOrVerticalLayoutGroup))]
    public class TabBar : Core.Components.Base.AbstractComponent
    {
        private UnityUI.HorizontalOrVerticalLayoutGroup _layoutGroup = null;

        protected override void Awake() {
            base.Awake();

            _layoutGroup = GetComponent<UnityUI.HorizontalOrVerticalLayoutGroup>();
        }
        
        public void AddTabButton(TabButton button) {

        }
    }
}
