using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    internal abstract class StaticAction : IAction
    {
        private static List<StaticAction> s_Actions = new List<StaticAction>();

        protected int m_ID = 0;
        protected string m_Label = null;
        protected uint m_EventMask = 0;
        protected bool m_Hidden = true;

        internal int ID {
            get { return m_ID; }
        }

        internal string Label {
            get { return m_Label; }
        }

        internal bool Hidden {
            get { return m_Hidden; }
        }

        internal uint EventMask {
            get { return m_EventMask; }
        }

        internal StaticAction(int id, string label, uint eventMask = 0, bool hidden = false) {
            if (id < 0 || id > 65535)
                throw new System.ArgumentException("StaticAction.StaticAction: ID out of range: " + id);

            m_ID = id;
            m_Label = label;
            m_EventMask = eventMask;
            m_Hidden = hidden;
            StaticAction.RegisterAction(this);
        }

        public abstract bool Perform(bool repeat = false);

        internal virtual bool KeyCallback(uint eventMask, char _, UnityEngine.KeyCode __, UnityEngine.EventModifiers ___) {
            return Perform(eventMask == InputEvent.KeyRepeat);
        }

        internal virtual bool TextCallback(uint eventMask, char _) {
            return Perform(false);
        }

        public abstract IAction Clone();

        internal static StaticAction GetAction(int id) {
            foreach (var action in s_Actions) {
                if (action.ID == id)
                    return action;
            }

            return null;
        }

        internal static void RegisterAction(StaticAction action) {
            s_Actions.Add(action);
        }

        internal static List<StaticAction> GetAllActions() {
            return s_Actions;
        }

        internal static TMPro.TMP_InputField GetSelectedInputField() {
            var gameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            return gameObject?.GetComponent<TMPro.TMP_InputField>();
        }
    }
}
