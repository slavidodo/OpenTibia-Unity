namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class MoveActionImpl : IActionImpl
    {
        public const int MoveAll = -2;
        public const int MoveAsk = -1;

        private UnityEngine.Vector3Int _sourceAbsolute;
        private UnityEngine.Vector3Int _destAbsolute;
        private Appearances.AppearanceType _objectType;

        private int _stackPos;
        private int _objectAmount;
        private int _moveAmount;

        public MoveActionImpl(UnityEngine.Vector3Int sourseAbsolute, Appearances.ObjectInstance @object, int stackPos, UnityEngine.Vector3Int destAbsolute, int moveAmount) {
            _sourceAbsolute = sourseAbsolute;
            _objectType = @object.Type;
            _stackPos = stackPos;
            _destAbsolute = destAbsolute;

            if (_objectType.IsStackable)
                _objectAmount = (int)@object.Data;
            else
                _objectAmount = 1;

            if (moveAmount < MoveActionImpl.MoveAll || moveAmount > 100)
                throw new System.ArgumentException("MoveActionImpl.MoveActionImpl: Invalid amount.");

            _moveAmount = moveAmount;
        }

        public void Perform(bool repeat = false) {
            if (_sourceAbsolute == _destAbsolute)
                return;
            
            if (_objectType.IsUnmovable) {
                OpenTibiaUnity.WorldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.GAME_MOVE_UNMOVEABLE);
            } else if (_objectType.IsStackable && _objectAmount > 1 && _moveAmount == MoveActionImpl.MoveAsk) {
                var gameManager = OpenTibiaUnity.GameManager;
                var splitStackWindow = UnityEngine.Object.Instantiate(gameManager.SplitStackWindowPrefab, gameManager.ActiveCanvas.transform);
                
                splitStackWindow.ObjectType = _objectType;
                splitStackWindow.ObjectAmount = _objectAmount;
                splitStackWindow.SelectedAmount = _objectAmount;
                splitStackWindow.onOk.AddListener(OnSplitStackWindowOk);
                splitStackWindow.Show();
            } else {
                InternalPerform(_moveAmount);
            }
        }

        protected void InternalPerform(int moveAmount) {
            if (moveAmount == MoveActionImpl.MoveAll)
                moveAmount = _objectAmount;
            else if (moveAmount <= 0 || moveAmount > _objectAmount)
                moveAmount = 1;

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!!protocolGame && protocolGame.IsGameRunning)
                protocolGame.SendMoveObject(_sourceAbsolute, (ushort)_objectType.Id, _stackPos, _destAbsolute, moveAmount);
        }

        protected void OnSplitStackWindowOk(UI.Legacy.SplitStackWidget splitStackWindow) {
            InternalPerform(splitStackWindow.SelectedAmount);
        }
    }
}
