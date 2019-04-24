using OpenTibiaUnity.Core.InputManagment.Mapping;
using OpenTibiaUnity.Core.InputManagment.StaticAction;
using System.Text.RegularExpressions;
using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment
{
    public class MappingSet
    {
        public const int ChatModeON = 0;
        public const int ChatModeOFF = 1;

        public const int DefaultSet = 0;

        public static Binding[] ChatModeOffDefaultBindings;
        public static Binding[] ChatModeOnDefaultBindings;

        protected Mapping.Mapping m_ChatModeOn = null;
        protected Mapping.Mapping m_ChatModeOff = null;
        protected int m_ID = 0;
        protected string m_Name = null;

        public int ID {
            get { return m_ID; }
        }
        public string Name {
            get { return m_Name; }
        }

        public Mapping.Mapping ChatModeOnMapping {
            get { return m_ChatModeOn; }
        }
        public Mapping.Mapping ChatModeOffMapping {
            get { return m_ChatModeOff; }
        }

        static MappingSet() {
            ChatModeOffDefaultBindings = new Binding[] {
                new Binding(StaticActionList.MiscLogoutCharacter, 'q', KeyCode.Q, EventModifiers.Control),
                new Binding(StaticActionList.MiscChangeCharacter, 'g', KeyCode.G, EventModifiers.Control),
                //new Binding(StaticActionList.MiscEditOptions, 'z', KeyCode.Z, EventModifiers.Control),
                //new Binding(StaticActionList.MiscEditOptions, 'k', KeyCode.K, EventModifiers.Control),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None),
                //new Binding(StaticActionList.MiscToggleMappingMode, '\0', KeyCode.Return, EventModifiers.Control),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control, false),
                //new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.UpArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.Alpha8, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.LeftArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.Alpha4, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.RightArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.Alpha6, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.DownArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.Alpha2, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUpLeft, '\0', KeyCode.Alpha7, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDownLeft, '\0', KeyCode.Alpha1, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUpRight, '\0', KeyCode.Alpha9, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDownRight, '\0', KeyCode.Alpha3, EventModifiers.None),
                new Binding(StaticActionList.PlayerTurnUp, '\0', KeyCode.UpArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnLeft, '\0', KeyCode.LeftArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnRight, '\0', KeyCode.RightArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnDown, '\0', KeyCode.DownArrow, EventModifiers.Control),
            };

            ChatModeOnDefaultBindings = new Binding[] {
                new Binding(StaticActionList.MiscLogoutCharacter, 'q', KeyCode.Q, EventModifiers.Control),
                new Binding(StaticActionList.MiscChangeCharacter, 'g', KeyCode.G, EventModifiers.Control),
                //new Binding(StaticActionList.MiscEditOptions, 'z', KeyCode.Z, EventModifiers.Control),
                //new Binding(StaticActionList.MiscEditOptions, 'k', KeyCode.K, EventModifiers.Control),
                new Binding(StaticActionList.PlayerMount, 'r', KeyCode.R, EventModifiers.Control),
                new Binding(StaticActionList.PlayerCancel, '\0', KeyCode.Escape, EventModifiers.None),
                //new Binding(StaticActionList.MiscToggleMappingMode, '\0', KeyCode.Return, EventModifiers.Control),
                //new Binding(StaticActionList.ChatMoveCursorLeft, '\0', KeyCode.LeftArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatMoveCursorRight, '\0', KeyCode.RightArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatHistoryPrev, '\0', KeyCode.UpArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatHistoryNext, '\0', KeyCode.DownArrow, EventModifiers.Shift, false),
                //new Binding(StaticActionList.ChatTextInput, '\0', KeyCode.None, EventModifiers.None, false),
                new Binding(StaticActionList.ChatMoveCursorHome, '\0', KeyCode.Home, EventModifiers.None, false),
                new Binding(StaticActionList.ChatMoveCursorEnd, '\0', KeyCode.End, EventModifiers.None, false),
                new Binding(StaticActionList.ChatSelectAll, 'a', KeyCode.A, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatCopySelected, 'c', KeyCode.C, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatInsertClipboard, 'v', KeyCode.V, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatCutSelected, 'x', KeyCode.X, EventModifiers.Control, false),
                new Binding(StaticActionList.ChatDeletePrev, '\0', KeyCode.Backspace, EventModifiers.None, true),
                new Binding(StaticActionList.ChatDeleteNext, '\0', KeyCode.Delete, EventModifiers.None, true),
                //new Binding(StaticActionList.ChatSendText, '\0', KeyCode.Return, EventModifiers.None, false),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.UpArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUp, '\0', KeyCode.Alpha8, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.LeftArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveLeft, '\0', KeyCode.Alpha4, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.RightArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveRight, '\0', KeyCode.Alpha6, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.DownArrow, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDown, '\0', KeyCode.Alpha2, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUpLeft, '\0', KeyCode.Alpha7, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDownLeft, '\0', KeyCode.Alpha1, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveUpRight, '\0', KeyCode.Alpha9, EventModifiers.None),
                new Binding(StaticActionList.PlayerMoveDownRight, '\0', KeyCode.Alpha3, EventModifiers.None),
                new Binding(StaticActionList.PlayerTurnUp, '\0', KeyCode.UpArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnLeft, '\0', KeyCode.LeftArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnRight, '\0', KeyCode.RightArrow, EventModifiers.Control),
                new Binding(StaticActionList.PlayerTurnDown, '\0', KeyCode.DownArrow, EventModifiers.Control),
            };
        }

        public MappingSet(int id, string name = null) {
            m_ID = id;
            m_Name = GetSanitizedSetName(id, name);

            m_ChatModeOff = new Mapping.Mapping();
            m_ChatModeOn = new Mapping.Mapping();
        }

        public void InitialiseDefaultBindings() {
            m_ChatModeOn.RemoveAll(false);
            m_ChatModeOn.AddAll(ChatModeOnDefaultBindings);
            m_ChatModeOff.RemoveAll(false);
            m_ChatModeOff.AddAll(ChatModeOffDefaultBindings);
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
