using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    class GameSideNonVolatileContentPanel : GameSideContentPanel
    {
        [SerializeField] private RectTransform _tmpContentPanel = null;
        [SerializeField] private GameSideContentPanel _sideContentPanel = null;

        public override bool IsNonVolatile() => true;

        protected override void OnRectTransformDimensionsChange() {
            float height = rectTransform.rect.height;
            
            _sideContentPanel.rectTransform.offsetMax = new Vector2(0, -height);
            _tmpContentPanel.offsetMax = new Vector2(0, -height);

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tmpContentPanel);
        }
    }
}
