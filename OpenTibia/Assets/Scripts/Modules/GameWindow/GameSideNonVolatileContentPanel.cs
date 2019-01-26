using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    class GameSideNonVolatileContentPanel : GameSideContentPanel
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RectTransform m_SideContentPanel;
#pragma warning restore CS0649 // never assigned to

        public override bool IsNonVolatile() => true;

        protected override void OnRectTransformDimensionsChange() {
            float height = (transform as RectTransform).rect.height;

            var rectTransform = m_SideContentPanel.transform as RectTransform;
            rectTransform.offsetMax = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
