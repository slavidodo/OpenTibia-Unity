namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal static class StaticActionList
    {
        // Misc
        internal static ShowDialog MiscShowEditHotkeys = new ShowDialog(4, "MISC_SHOW_EDIT_HOTKEYS", InputEvent.KeyDown, DialogType.OptionsHotkey);

        internal static LogoutCharacter MiscLogoutCharacter = new LogoutCharacter(11, "MISC_LOGOUT_CHARACTER", InputEvent.KeyDown);
        internal static ChangeCharacter MiscChangeCharacter = new ChangeCharacter(12, "MISC_CHANGE_CHARACTER", InputEvent.KeyDown);

        internal static ShowDialog MiscShowOutfit = new ShowDialog(18, "MISC_SHOW_OUTFIT", InputEvent.KeyDown, DialogType.CharacterOutfit);

        // Chat
        internal static ChatMoveCursorHome ChatMoveCursorHome = new ChatMoveCursorHome(259, "CHAT_MOVE_CURSOR_HOME", InputEvent.KeyDown);
        internal static ChatMoveCursorEnd ChatMoveCursorEnd = new ChatMoveCursorEnd(260, "CHAT_MOVE_CURSOR_END", InputEvent.KeyDown);
        internal static ChatDeletePrev ChatDeletePrev = new ChatDeletePrev(261, "CHAT_DELETE_PREV", InputEvent.KeyDown | InputEvent.KeyRepeat);
        internal static ChatDeleteNext ChatDeleteNext = new ChatDeleteNext(262, "CHAT_DELETE_NEXT", InputEvent.KeyDown | InputEvent.KeyRepeat);
        internal static ChatSelectAll ChatSelectAll = new ChatSelectAll(263, "CHAT_SELECT_ALL", InputEvent.KeyDown);
        internal static ChatCopySelected ChatCopySelected = new ChatCopySelected(264, "CHAT_COPY_SELECTED", InputEvent.KeyDown);
        internal static ChatInsertClipboard ChatInsertClipboard = new ChatInsertClipboard(265, "CHAT_INSERT_CLIPBOARD", InputEvent.KeyDown);
        internal static ChatCutSelected ChatCutSelected = new ChatCutSelected(266, "CHAT_CUT_SELECTED", InputEvent.KeyDown);

        // Player
        internal static PlayerMove PlayerMoveRight = new PlayerMove(512, "PLAYER_MOVE_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.East);
        internal static PlayerMove PlayerMoveUpRight = new PlayerMove(513, "PLAYER_MOVE_UP_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.NorthEast);
        internal static PlayerMove PlayerMoveUp = new PlayerMove(514, "PLAYER_MOVE_UP", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.North);
        internal static PlayerMove PlayerMoveUpLeft = new PlayerMove(515, "PLAYER_MOVE_UP_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.NorthWest);
        internal static PlayerMove PlayerMoveLeft = new PlayerMove(516, "PLAYER_MOVE_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.West);
        internal static PlayerMove PlayerMoveDownLeft = new PlayerMove(517, "PLAYER_MOVE_DOWN_LEFT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.SouthWest);
        internal static PlayerMove PlayerMoveDown = new PlayerMove(518, "PLAYER_MOVE_DOWN", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.South);
        internal static PlayerMove PlayerMoveDownRight = new PlayerMove(519, "PLAYER_MOVE_DOWN_RIGHT", InputEvent.KeyDown | InputEvent.KeyRepeat, Direction.SouthEast);
        internal static PlayerTurn PlayerTurnRight = new PlayerTurn(520, "PLAYER_TURN_RIGHT", InputEvent.KeyDown, Direction.East);
        internal static PlayerTurn PlayerTurnUp = new PlayerTurn(521, "PLAYER_TURN_UP", InputEvent.KeyDown, Direction.North);
        internal static PlayerTurn PlayerTurnLeft = new PlayerTurn(522, "PLAYER_TURN_LEFT", InputEvent.KeyDown, Direction.West);
        internal static PlayerTurn PlayerTurnDown = new PlayerTurn(523, "PLAYER_TURN_DOWN", InputEvent.KeyDown, Direction.South);
        internal static PlayerCancel PlayerCancel = new PlayerCancel(524, "PLAYER_CANCEL", InputEvent.KeyDown);
        internal static PlayerMount PlayerMount = new PlayerMount(525, "PLAYER_MOUNT", InputEvent.KeyDown);

    }
}
