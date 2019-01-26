using System.Collections.Generic;

namespace OpenTibiaUnity.Core.InputManagment.StaticAction
{
    public abstract class StaticAction : IAction
    {
        private static List<StaticAction> s_Actions = new List<StaticAction>();

        protected int m_ID = 0;
        protected string m_Label = null;
        protected uint m_EventMask = 0;
        protected bool m_Hidden = true;

        public int ID {
            get { return m_ID; }
        }

        public string Label {
            get { return m_Label; }
        }

        public bool Hidden {
            get { return m_Hidden; }
        }

        public uint EventMask {
            get { return m_EventMask; }
        }

        public StaticAction(int id, string label, uint eventMask = 0, bool hidden = false) {
            if (id < 0 || id > 65535)
                throw new System.ArgumentException("StaticAction.StaticAction: ID out of range: " + id);

            m_ID = id;
            m_Label = label;
            m_EventMask = eventMask;
            m_Hidden = hidden;
            StaticAction.RegisterAction(this);
        }

        public abstract bool Perform(bool repeat = false);

        public virtual bool KeyCallback(uint eventMask, char _, UnityEngine.KeyCode __, UnityEngine.EventModifiers ___) {
            return Perform(eventMask == InputEvent.KeyRepeat);
        }

        public virtual bool TextCallback(uint eventMask, char _) {
            return Perform(false);
        }

        public IAction Clone() {
            return this;
        }

        public static StaticAction GetAction(int id) {
            foreach (var action in s_Actions) {
                if (action.ID == id)
                    return action;
            }

            return null;
        }

        public static void RegisterAction(StaticAction action) {
            s_Actions.Add(action);
        }

        public static List<StaticAction> GetAllActions() {
            return s_Actions;
        }

        public static TMPro.TMP_InputField GetSelectedInputField() {
            var gameObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            return gameObject?.GetComponent<TMPro.TMP_InputField>();
        }
    }
}
