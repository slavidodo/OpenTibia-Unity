using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Input.StaticAction
{
    public abstract class StaticAction : IAction
    {
        private static List<StaticAction> s_Actions = new List<StaticAction>();

        protected int _id = 0;
        protected string _label = null;
        protected InputEvent _eventMask = 0;
        protected bool _hidden = true;

        public int Id { get => _id; }
        public string Label { get => _label; }
        public bool Hidden { get => _hidden; }
        public InputEvent EventMask { get => _eventMask; }

        public StaticAction(int id, string label, InputEvent eventMask = 0, bool hidden = false) {
            if (id < 0 || id > 65535)
                throw new System.ArgumentException("StaticAction.StaticAction: _id out of range: " + id);

            _id = id;
            _label = label;
            _eventMask = eventMask;
            _hidden = hidden;
            StaticAction.RegisterAction(this);
        }

        public abstract bool Perform(bool repeat = false);

        public virtual bool KeyCallback(InputEvent eventMask, char _, UnityEngine.KeyCode __, UnityEngine.EventModifiers ___) {
            return Perform((eventMask & InputEvent.KeyRepeat) != 0);
        }

        public virtual bool TextCallback(InputEvent _, char __) {
            return Perform(false);
        }

        public abstract IAction Clone();

        public static StaticAction GetAction(int id) {
            foreach (var action in s_Actions) {
                if (action._id == id)
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
