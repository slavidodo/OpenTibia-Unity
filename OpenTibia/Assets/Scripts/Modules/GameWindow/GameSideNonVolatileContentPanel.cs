using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    class GameSideNonVolatileContentPanel : GameSideContentPanel
    {
        [SerializeField] private RectTransform m_TmpContentPanel = null;
        [SerializeField] private GameSideContentPanel m_SideContentPanel = null;

        public override bool IsNonVolatile() => true;

        protected override void OnRectTransformDimensionsChange() {
            float height = rectTransform.rect.height;
            
            m_SideContentPanel.rectTransform.offsetMax = new Vector2(0, -height);
            m_TmpContentPanel.offsetMax = new Vector2(0, -height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_TmpContentPanel);
        }
    }
}
