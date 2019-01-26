namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public static class StaticActionList
    {
        // Misc
        public static LogoutCharacter MiscLogoutCharacter = new LogoutCharacter(11, "MISC_LOGOUT_CHARACTER", InputEvent.KeyDown);
        public static ChangeCharacter MiscChangeCharacter = new ChangeCharacter(12, "MISC_CHANGE_CHARACTER", InputEvent.KeyDown);

        // Chat
        public static ChatMoveCursorHome ChatMoveCursorHome = new ChatMoveCursorHome(259, "CHAT_MOVE_CURSOR_HOME", InputEvent.KeyDown);
        public static ChatMoveCursorEnd ChatMoveCursorEnd = new ChatMoveCursorEnd(260, "CHAT_MOVE_CURSOR_END", InputEvent.KeyDown);
        public static ChatDeletePrev ChatDeletePrev = new ChatDeletePrev(261, "CHAT_DELETE_PREV", InputEvent.KeyDown | InputEvent.KeyRepeat);
        public static ChatDeleteNext ChatDeleteNext = new ChatDeleteNext(262, "CHAT_DELETE_NEXT", InputEvent.KeyDown | InputEvent.KeyRepeat);
        public static ChatSelectAll ChatSelectAll = new ChatSelectAll(263, "CHAT_SELECT_ALL", InputEvent.KeyDown);
        public static ChatCopySelected ChatCopySelected = new ChatCopySelected(264, "CHAT_COPY_SELECTED", InputEvent.KeyDown);
        public static ChatInsertClipboard ChatInsertClipboard = new ChatInsertClipboard(265, "CHAT_INSERT_CLIPBOARD", InputEvent.KeyDown);
        public static ChatCutSelected ChatCutSelected = new ChatCutSelected(266, "CHAT_CUT_SELECTED", InputEvent.KeyDown);

        // Player
        public static PlayerMove PlayerMoveRight = new PlayerMove(512, "PLAYER_MOVE_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.East);
        public static PlayerMove PlayerMoveUpRight = new PlayerMove(513, "PLAYER_MOVE_UP_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.NorthEast);
        public static PlayerMove PlayerMoveUp = new PlayerMove(514, "PLAYER_MOVE_UP", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.North);
        public static PlayerMove PlayerMoveUpLeft = new PlayerMove(515, "PLAYER_MOVE_UP_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.NorthWest);
        public static PlayerMove PlayerMoveLeft = new PlayerMove(516, "PLAYER_MOVE_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.West);
        public static PlayerMove PlayerMoveDownLeft = new PlayerMove(517, "PLAYER_MOVE_DOWN_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.SouthWest);
        public static PlayerMove PlayerMoveDown = new PlayerMove(518, "PLAYER_MOVE_DOWN", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.South);
        public static PlayerMove PlayerMoveDownRight = new PlayerMove(519, "PLAYER_MOVE_DOWN_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Directions.SouthEast);
        public static PlayerTurn PlayerTurnRight = new PlayerTurn(520, "PLAYER_TURN_RIGHT", InputEvent.KeyDown, Directions.East);
        public static PlayerTurn PlayerTurnUp = new PlayerTurn(521, "PLAYER_TURN_UP", InputEvent.KeyDown, Directions.North);
        public static PlayerTurn PlayerTurnLeft = new PlayerTurn(522, "PLAYER_TURN_LEFT", InputEvent.KeyDown, Directions.West);
        public static PlayerTurn PlayerTurnDown = new PlayerTurn(523, "PLAYER_TURN_DOWN", InputEvent.KeyDown, Directions.South);
        public static PlayerCancel PlayerCancel = new PlayerCancel(524, "PLAYER_CANCEL", InputEvent.KeyDown);
        public static PlayerMount PlayerMount = new PlayerMount(525, "PLAYER_MOUNT", InputEvent.KeyDown);

    }
}
