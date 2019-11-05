using UnityEngine;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class GamePanelContainer : Core.Components.Base.Module
    {
        protected override void OnRectTransformDimensionsChange() {
            base.OnRectTransformDimensionsChange();

            UpdateLayout();
        }

        protected void UpdateLayout() {
            var parent = transform.parent;
            var gameWindowLayout = parent.GetComponent<GameInterface>();
            if (!!gameWindowLayout)
                gameWindowLayout.UpdateLayout();
        }
    }
}
