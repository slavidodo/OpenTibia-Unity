using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    class GameSideNonVolatileContentPanel : GameSideContentPanel
    {
        [SerializeField]
        private RectTransform _content = null;
        [SerializeField]
        private RectTransform _tmpContent = null;

        public override bool IsNonVolatile() => true;

        protected override void OnRectTransformDimensionsChange() {
            float height = rectTransform.rect.height;
            _content.offsetMax = new Vector2(0, -height);
            _tmpContent.offsetMax = new Vector2(0, -height);

            UnityUI.LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            UnityUI.LayoutRebuilder.MarkLayoutForRebuild(_tmpContent);
        }
    }
}
