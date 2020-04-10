namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public static class StaticActionList
    {
        // Misc
        public static ShowDialog MiscShowEditHotkeys = new ShowDialog(4, "MISC_SHOW_EDIT_HOTKEYS", InputEvent.KeyDown, DialogType.OptionsHotkey);

        public static LogoutCharacter MiscLogoutCharacter = new LogoutCharacter(11, "MISC_LOGOUT_CHARACTER", InputEvent.KeyDown);
        public static ChangeCharacter MiscChangeCharacter = new ChangeCharacter(12, "MISC_CHANGE_CHARACTER", InputEvent.KeyDown);

        public static ShowDialog MiscShowOutfit = new ShowDialog(18, "MISC_SHOW_OUTFIT", InputEvent.KeyDown, DialogType.CharacterOutfit);
        public static ToggleFullScreen MiscToggleFullScreen = new ToggleFullScreen(19, "MISC_TOGGLE_FULL_SCREEN", InputEvent.KeyDown);

        // Chat
        public static ChatMoveCursorLeft ChatMoveCursorLeft = new ChatMoveCursorLeft(257, "CHAT_MOVE_CURSOR_LEFT", InputEvent.KeyDown);
        public static ChatMoveCursorRight ChatMoveCursorRight = new ChatMoveCursorRight(258, "CHAT_MOVE_CURSOR_RIGHT", InputEvent.KeyDown);
        public static ChatMoveCursorHome ChatMoveCursorHome = new ChatMoveCursorHome(259, "CHAT_MOVE_CURSOR_HOME", InputEvent.KeyDown);
        public static ChatMoveCursorEnd ChatMoveCursorEnd = new ChatMoveCursorEnd(260, "CHAT_MOVE_CURSOR_END", InputEvent.KeyDown);
        public static ChatDeletePrev ChatDeletePrev = new ChatDeletePrev(261, "CHAT_DELETE_PREV", InputEvent.KeyDown | InputEvent.KeyRepeat);
        public static ChatDeleteNext ChatDeleteNext = new ChatDeleteNext(262, "CHAT_DELETE_NEXT", InputEvent.KeyDown | InputEvent.KeyRepeat);
        public static ChatSelectAll ChatSelectAll = new ChatSelectAll(263, "CHAT_SELECT_ALL", InputEvent.KeyDown);
        public static ChatCopySelected ChatCopySelected = new ChatCopySelected(264, "CHAT_COPY_SELECTED", InputEvent.KeyDown);
        public static ChatInsertClipboard ChatInsertClipboard = new ChatInsertClipboard(265, "CHAT_INSERT_CLIPBOARD", InputEvent.KeyDown);
        public static ChatCutSelected ChatCutSelected = new ChatCutSelected(266, "CHAT_CUT_SELECTED", InputEvent.KeyDown);
        public static ChatHistoryPrev ChatHistoryPrev = new ChatHistoryPrev(267, "CHAT_HISTORY_PREV", InputEvent.KeyDown);
        public static ChatHistoryNext ChatHistoryNext = new ChatHistoryNext(268, "CHAT_HISTORY_NEXT", InputEvent.KeyDown);
        public static ChatSendText ChatSendText = new ChatSendText(269, "CHAT_SEND_TEXT", InputEvent.KeyDown);
        public static ShowDialog ChatChannelOpen = new ShowDialog(270, "CHAT_CHANNEL_OPEN", InputEvent.KeyDown, DialogType.ChatChannelSelection);

        // Player
        public static PlayerMove PlayerMoveRight = new PlayerMove(512, "PLAYER_MOVE_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.East);
        public static PlayerMove PlayerMoveUpRight = new PlayerMove(513, "PLAYER_MOVE_UP_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.NorthEast);
        public static PlayerMove PlayerMoveUp = new PlayerMove(514, "PLAYER_MOVE_UP", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.North);
        public static PlayerMove PlayerMoveUpLeft = new PlayerMove(515, "PLAYER_MOVE_UP_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.NorthWest);
        public static PlayerMove PlayerMoveLeft = new PlayerMove(516, "PLAYER_MOVE_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.West);
        public static PlayerMove PlayerMoveDownLeft = new PlayerMove(517, "PLAYER_MOVE_DOWN_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.SouthWest);
        public static PlayerMove PlayerMoveDown = new PlayerMove(518, "PLAYER_MOVE_DOWN", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.South);
        public static PlayerMove PlayerMoveDownRight = new PlayerMove(519, "PLAYER_MOVE_DOWN_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.SouthEast);
        public static PlayerTurn PlayerTurnRight = new PlayerTurn(520, "PLAYER_TURN_RIGHT", InputEvent.KeyDown, Direction.East);
        public static PlayerTurn PlayerTurnUp = new PlayerTurn(521, "PLAYER_TURN_UP", InputEvent.KeyDown, Direction.North);
        public static PlayerTurn PlayerTurnLeft = new PlayerTurn(522, "PLAYER_TURN_LEFT", InputEvent.KeyDown, Direction.West);
        public static PlayerTurn PlayerTurnDown = new PlayerTurn(523, "PLAYER_TURN_DOWN", InputEvent.KeyDown, Direction.South);
        public static PlayerCancel PlayerCancel = new PlayerCancel(524, "PLAYER_CANCEL", InputEvent.KeyDown);
        public static PlayerMount PlayerMount = new PlayerMount(525, "PLAYER_MOUNT", InputEvent.KeyDown);

    }
}
