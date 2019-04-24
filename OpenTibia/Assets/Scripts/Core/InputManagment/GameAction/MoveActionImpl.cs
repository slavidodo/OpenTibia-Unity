namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class MoveActionImpl : IActionImpl
    {
        public const int MoveAll = -2;
        public const int MoveAsk = -1;

        private UnityEngine.Vector3Int m_SourceAbsolute;
        private UnityEngine.Vector3Int m_DestAbsolute;
        private Appearances.AppearanceType m_ObjectType;

        private int m_StackPos;
        private int m_ObjectAmount;
        private int m_MoveAmount;

        public MoveActionImpl(UnityEngine.Vector3Int sourseAbsolute, Appearances.ObjectInstance obj, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            m_SourceAbsolute = sourseAbsolute;
            m_ObjectType = obj.Type;
            m_StackPos = stackPos;
            m_DestAbsolute = destAbsolute;

            if (m_ObjectType.IsStackable)
                m_ObjectAmount = (int)obj.Data;
            else
                m_ObjectAmount = 1;

            if (moveAmount < MoveActionImpl.MoveAll || moveAmount > 100)
                throw new System.ArgumentException("MoveActionImpl.MoveActionImpl: Invalid amount.");

            m_MoveAmount = moveAmount;
        }

        public void Perform(bool repeat = false) {
            if (m_SourceAbsolute == m_DestAbsolute)
                return;
            
            if (m_ObjectType.IsUnmovable) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.GAME_MOVE_UNMOVEABLE);
            } else if (m_ObjectType.IsStackable && m_ObjectAmount > 1 && m_MoveAmount == MoveActionImpl.MoveAsk) {
                // TODO: SplitStackWidget
                var splitStackWindow = UnityEngine.Object.Instantiate(OpenTibiaUnity.GameManager.SplitStackWindowPrefab,
                    OpenTibiaUnity.GameManager.ActiveCanvas.transform);

                splitStackWindow.ObjectType = m_ObjectType;
                splitStackWindow.ObjectAmount = m_ObjectAmount;
                splitStackWindow.SelectedAmount = m_ObjectAmount;
                splitStackWindow.onOk.AddListener(OnSplitStackWindowOk);
            } else {
                PerformInternal(m_MoveAmount);
            }
        }

        protected void PerformInternal(int moveAmount) {
            if (moveAmount == MoveActionImpl.MoveAll)
                moveAmount = m_ObjectAmount;
            else if (moveAmount <= 0 || moveAmount > m_ObjectAmount)
                moveAmount = 1;

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendMoveObject(m_SourceAbsolute, m_ObjectType.ID, m_StackPos, m_DestAbsolute, moveAmount);
        }

        protected void OnSplitStackWindowOk(Components.SplitStackWindow splitStackWindow) {
            PerformInternal(splitStackWindow.SelectedAmount);
        }
    }
}
