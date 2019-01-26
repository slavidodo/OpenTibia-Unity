using UnityEngine;

namespace OpenTibiaUnity.Core.Components.Base
{
    public class MiniWindow : AbstractComponent
    {
        public const int MinimumStaticHeight = 50;

        [SerializeField] protected bool m_Resizable = false;
        [SerializeField] protected int m_InitialHeight = 50;
        [SerializeField] protected int m_PreferredHeight = 50;

        protected int m_MinimumSize = -1;
        protected int m_MaximumSize = -1;
    }
}