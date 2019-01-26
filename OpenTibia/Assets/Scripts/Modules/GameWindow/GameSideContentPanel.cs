using UnityEngine;

namespace OpenTibiaUnity.Modules.GameWindow
{
    [DisallowMultipleComponent]
    class GameSideContentPanel : Core.Components.Base.AbstractComponent
    {
        public virtual bool IsNonVolatile() => false;
    }
}
