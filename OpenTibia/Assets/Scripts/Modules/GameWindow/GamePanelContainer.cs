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
            GameInterface gameInterface;
            if (OpenTibiaUnity.GameManager != null)
                gameInterface = OpenTibiaUnity.GameManager.GetModule<GameInterface>();
            else
                gameInterface = rectTransform.parent.GetComponent<GameInterface>();

            if (gameInterface)
                gameInterface.UpdateLayout(this);
        }
    }
}
