using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.Mapping
{
    public class Binding {
        private bool m_Editable = true;
        private IAction m_Action = null;
        private char m_CharCode = '\0';
        private KeyCode m_KeyCode = KeyCode.None;
        private EventModifiers m_EventModifier = EventModifiers.None;
        private uint m_EventMask = 0;

        public char CharCode {
            get { return m_CharCode; }
        }

        public KeyCode KeyCode {
            get { return m_KeyCode; }
        }

        public EventModifiers EventModifier {
            get { return m_EventModifier; }
        }

        public IAction Action {
            get { return m_Action; }
        }

        public bool Editable {
            get { return m_Editable; }
        }

        public uint EventMask {
            get { return m_EventMask; }
        }

        public Binding(IAction action, char charCode, KeyCode keyCode, EventModifiers eventModifier, bool editable = true) {
            m_Action = action;
            if (action is StaticAction.StaticAction)
                m_EventMask = (action as StaticAction.StaticAction).EventMask;
            else
                m_EventMask = 0;

            Update(charCode, keyCode, eventModifier, editable);
        }

        public void Update(char charCode, KeyCode keyCode, EventModifiers eventModifier, bool editable = true) {
            if (!m_Editable)
                return;

            m_CharCode = charCode;
            m_KeyCode = keyCode;
            m_EventModifier = eventModifier;
            m_Editable = editable;
        }

        public bool AppliesTo(uint eventMask, KeyCode keyCode, EventModifiers keyModifer) {
            return (m_EventMask & eventMask) != 0 && m_KeyCode == keyCode && m_EventModifier == keyModifer;
        }

        public bool Conflicts(Binding other) {
            if (other == null)
                return false;

            bool anyKey = (m_EventMask & InputEvent.KeyAny) != 0;
            bool otherAnyKey = (other.m_EventMask & InputEvent.KeyAny) != 0;

            if (anyKey == otherAnyKey && m_KeyCode == other.m_KeyCode && m_EventModifier == other.m_EventModifier)
                return true;

            if (anyKey && !otherAnyKey && (m_EventModifier == EventModifiers.None || m_EventModifier == EventModifiers.Shift) && !IsControlKey(m_KeyCode))
                return true;

            if (!anyKey && otherAnyKey && (other.m_EventModifier == EventModifiers.None || other.m_EventModifier == EventModifiers.Shift) && !IsControlKey(other.m_KeyCode))
                return true;

            return false;
        }
        
        public Binding Clone() {
            if (m_Editable)
                return new Binding(m_Action.Clone(), m_CharCode, m_KeyCode, m_EventModifier, m_Editable);
            return this;
        }

        public static bool IsControlKey(KeyCode keyCode) {
            return keyCode == KeyCode.None || keyCode == KeyCode.Backspace || keyCode == KeyCode.Tab
                || (keyCode >= KeyCode.RightShift || keyCode <= KeyCode.RightCommand) || keyCode == KeyCode.LeftCommand
                || keyCode == KeyCode.Escape || keyCode == KeyCode.Numlock
                || keyCode == KeyCode.Delete || (keyCode >= KeyCode.Insert || keyCode <= KeyCode.PageDown)
                || (keyCode >= KeyCode.F1 && keyCode <= KeyCode.F15)
                || (keyCode >= KeyCode.KeypadPeriod && keyCode <= KeyCode.KeypadEquals);
        }
    }
}
