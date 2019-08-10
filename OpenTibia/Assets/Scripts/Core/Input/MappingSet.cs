using OpenTibiaUnity.Core.Input.Mapping;
using OpenTibiaUnity.Core.Input.StaticAction;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OpenTibiaUnity.Core.Input
{
    internal class MappingSet
    {
        internal const int ChatModeON = 0;
        internal const int ChatModeOFF = 1;

        internal const int DefaultSet = 0;

        internal static Binding[] ChatModeOffDefaultBindings;
        internal static Binding[] ChatModeOnDefaultBindings;

        protected Mapping.Mapping m_ChatModeOn = null;
        protected Mapping.Mapping m_ChatModeOff = null;
        protected int m_ID = 0;
        protected string m_Name = null;

        internal int ID {
            get { return m_ID; }
        }
        internal string Name {
            get { return m_Name; }
        }

        internal Mapping.Mapping ChatModeOnMapping {
            get { return m_ChatModeOn; }
        }
        internal Mapping.Mapping ChatModeOffMapping {
            get { return m_ChatModeOff; }
        }

        static MappingSet() {
            ChatModeOffDefaultBindings = new Binding[] {
                new Binding(StaticActionList.MiscLogoutCharacter, 'q', KeyCode.Q, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscChangeCharacter, 'g', KeyCode.G, EventModifiers.Control, false),
                //new Binding(StaticActionList.MiscEditOptions, 'z', KeyCode.Z, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscShowEditHotkeys, 'k', KeyCode.K, EventModifiers.Control, false),
                //new Binding(StaticActionList.MiscToggleMappingMode, '\0', KeyCode.Return, EventModifiers.Control),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None, false),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control, false),
                //new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false, false),
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
                //new Binding(StaticActionList.MiscEditOptions, 'z', KeyCode.Z, EventModifiers.Control, false),
                new Binding(StaticActionList.MiscShowEditHotkeys, 'k', KeyCode.K, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control, false),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None, false),
                //new Binding(StaticActionList.MiscToggleMappingMode, '\0', KeyCode.Return, EventModifiers.Control),
                //new Binding(StaticActionList.ChatMoveCursorLeft, '\0', KeyCode.LeftArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatMoveCursorRight, '\0', KeyCode.RightArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatHistoryPrev, '\0', KeyCode.UpArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatHistoryNext, '\0', KeyCode.DownArrow, EventModifiers.Shift, false),
                new Binding(StaticActionList.ChatMoveCursorHome, '\0', KeyCode.Home, EventModifiers.None),
                new Binding(StaticActionList.ChatMoveCursorEnd, '\0', KeyCode.End, EventModifiers.None),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control),
                new Binding(StaticActionList.ChatInsertClipboard, 'v', KeyCode.V, EventModifiers.Control),
                new Binding(StaticActionList.ChatCutSelected, 'x', KeyCode.X, EventModifiers.Control),
                new Binding(StaticActionList.ChatDeletePrev, '\0', KeyCode.Backspace, EventModifiers.None),
                new Binding(StaticActionList.ChatDeleteNext, '\0', KeyCode.Delete, EventModifiers.None),
                //new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false),
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

        internal MappingSet(int id, string name = null) {
            m_ID = id;
            m_Name = GetSanitizedSetName(id, name);

            m_ChatModeOff = new Mapping.Mapping();
            m_ChatModeOn = new Mapping.Mapping();
        }

        internal void InitialiseDefaultBindings() {
            m_ChatModeOn.RemoveAll(false);
            m_ChatModeOn.AddAll(ChatModeOnDefaultBindings);
            m_ChatModeOff.RemoveAll(false);
            m_ChatModeOff.AddAll(ChatModeOffDefaultBindings);
        }

        internal static string GetSanitizedSetName(int id, string name) {
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
