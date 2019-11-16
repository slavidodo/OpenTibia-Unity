using OpenTibiaUnity.Core.Input.Mapping;
using OpenTibiaUnity.Core.Input.StaticAction;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OpenTibiaUnity.Core.Input
{
    public class MappingSet
    {
        public const int ChatModeON = 0;
        public const int ChatModeOFF = 1;

        public const int DefaultSet = 0;

        public static Binding[] ChatModeOffDefaultBindings;
        public static Binding[] ChatModeOnDefaultBindings;

        protected Mapping.Mapping _chatModeOn = null;
        protected Mapping.Mapping _chatModeOff = null;
        protected int _id = 0;
        protected string _name = null;

        public int Id { get => _id; }
        public string Name {
            get { return _name; }
        }

        public Mapping.Mapping ChatModeOnMapping {
            get { return _chatModeOn; }
        }
        public Mapping.Mapping ChatModeOffMapping {
            get { return _chatModeOff; }
        }

        static MappingSet() {
            ChatModeOffDefaultBindings = new Binding[] {
                new Binding(StaticActionList.MiscLogoutCharacter, 'q', KeyCode.Q, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscChangeCharacter, 'g', KeyCode.G, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscToggleFullScreen, 'f', KeyCode.F, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscShowEditHotkeys, 'k', KeyCode.K, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None, false),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.UpArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.Keypad8, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.LeftArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.Keypad4, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.RightArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.Keypad6, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.DownArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.Keypad2, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUpLeft, '\0', KeyCode.Keypad7, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDownLeft, '\0', KeyCode.Keypad1, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUpRight, '\0', KeyCode.Keypad9, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDownRight, '\0', KeyCode.Keypad3, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerTurnUp, '\0', KeyCode.UpArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnLeft, '\0', KeyCode.LeftArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnRight, '\0', KeyCode.RightArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnDown, '\0', KeyCode.DownArrow, EventModifiers.Control, false),
            };

            ChatModeOnDefaultBindings = new Binding[] {
                new Binding(StaticActionList.MiscLogoutCharacter, 'q', KeyCode.Q, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscChangeCharacter, 'g', KeyCode.G, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscToggleFullScreen, 'f', KeyCode.F, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscShowEditHotkeys, 'k', KeyCode.K, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None, false),
                new Binding(StaticActionList.ChatMoveCursorLeft, '\0', KeyCode.LeftArrow, EventModifiers.Shift, false),
                new Binding(StaticActionList.ChatMoveCursorRight, '\0', KeyCode.RightArrow, EventModifiers.Shift, false),
                new Binding(StaticActionList.ChatHistoryPrev, '\0', KeyCode.UpArrow, EventModifiers.Shift, false),
                new Binding(StaticActionList.ChatHistoryNext, '\0', KeyCode.DownArrow, EventModifiers.Shift, false),
                new Binding(StaticActionList.ChatMoveCursorHome, '\0', KeyCode.Home, EventModifiers.None, false),
                new Binding(StaticActionList.ChatMoveCursorEnd, '\0', KeyCode.End, EventModifiers.None, false),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control),
                new Binding(StaticActionList.ChatInsertClipboard, 'v', KeyCode.V, EventModifiers.Control),
                new Binding(StaticActionList.ChatCutSelected, 'x', KeyCode.X, EventModifiers.Control),
                new Binding(StaticActionList.ChatDeletePrev, '\0', KeyCode.Backspace, EventModifiers.None, false),
                new Binding(StaticActionList.ChatDeleteNext, '\0', KeyCode.Delete, EventModifiers.None, false),
                new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.UpArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.Keypad8, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.LeftArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.Keypad4, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.RightArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.Keypad6, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.DownArrow, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.Keypad2, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUpLeft, '\0', KeyCode.Keypad7, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDownLeft, '\0', KeyCode.Keypad1, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveUpRight, '\0', KeyCode.Keypad9, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerMoveDownRight, '\0', KeyCode.Keypad3, EventModifiers.None, false, false),
                new Binding(StaticActionList.PlayerTurnUp, '\0', KeyCode.UpArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnLeft, '\0', KeyCode.LeftArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnRight, '\0', KeyCode.RightArrow, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerTurnDown, '\0', KeyCode.DownArrow, EventModifiers.Control, false),
            };
        }

        public MappingSet(int id, string name = null) {
            _id = id;
            _name = GetSanitizedSetName(id, name);

            _chatModeOff = new Mapping.Mapping();
            _chatModeOn = new Mapping.Mapping();
        }

        public void InitialiseDefaultBindings() {
            _chatModeOn.RemoveAll(false);
            _chatModeOn.AddAll(ChatModeOnDefaultBindings);
            _chatModeOff.RemoveAll(false);
            _chatModeOff.AddAll(ChatModeOffDefaultBindings);
        }

        public static string GetSanitizedSetName(int id, string name) {
            if (name != null) {
                name = Regex.Replace(name, @"\t|\n|\r", "").TrimStart().TrimEnd();
                if (name.Length > 255)
                    name = name.Substring(0, 255);
                return name;
            }
            return string.Format("Set {0}", id + 1);
        }
    }
}
