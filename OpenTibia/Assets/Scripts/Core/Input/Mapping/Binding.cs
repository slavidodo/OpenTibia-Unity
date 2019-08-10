using UnityEngine;

namespace OpenTibiaUnity.Core.Input.Mapping
{
    internal class Binding
    {
        private bool m_IgnoreBlocker = true;
        private bool m_Editable = true;
        private IAction m_Action = null;
        private char m_CharCode = '\0';
        private KeyCode m_KeyCode = KeyCode.None;
        private EventModifiers m_EventModifier = EventModifiers.None;
        private uint m_EventMask = 0;

        internal char CharCode {
            get { return m_CharCode; }
        }

        internal KeyCode KeyCode {
            get { return m_KeyCode; }
        }

        internal EventModifiers EventModifier {
            get { return m_EventModifier; }
        }

        internal IAction Action {
            get { return m_Action; }
        }

        internal bool Editable {
            get { return m_Editable; }
        }

        internal uint EventMask {
            get { return m_EventMask; }
        }

        internal bool IgnoreBlocker {
            get { return m_IgnoreBlocker; }
        }

        internal Binding(IAction action, char charCode, KeyCode keyCode, EventModifiers eventModifier, bool ignoreBlocker = true, bool editable = true) {
            m_Action = action;
            if (action is StaticAction.StaticAction)
                m_EventMask = (action as StaticAction.StaticAction).EventMask;
            else
                m_EventMask = 0;

            Update(charCode, keyCode, eventModifier, ignoreBlocker, editable);
        }

        internal void Update(char charCode, KeyCode keyCode, EventModifiers eventModifier, bool ignoreBlocker = true, bool editable = true) {
            if (!m_Editable)
                return;

            m_CharCode = charCode;
            m_KeyCode = keyCode;
            m_EventModifier = eventModifier;
            m_IgnoreBlocker = ignoreBlocker;
            m_Editable = editable;
        }

        internal bool AppliesTo(uint eventMask, KeyCode keyCode, EventModifiers keyModifer, bool blockerActive) {
            if (!((m_EventMask & eventMask) != 0 && m_KeyCode == keyCode && m_EventModifier == keyModifer))
                return false;

            return IgnoreBlocker || !blockerActive;
        }

        internal bool Conflicts(Binding other) {
            if (other == null)
                return false;

            bool anyKey = (m_EventMask & InputEvent.KeyAny) != 0;
            bool otherAnyKey = (other.m_EventMask & InputEvent.KeyAny) != 0;

            if (anyKey == otherAnyKey && m_KeyCode == other.m_KeyCode && m_EventModifier == other.m_EventModifier)
                return true;

            if (anyKey && !otherAnyKey && (m_EventModifier == EventModifiers.None || m_EventModifier == EventModifiers.Shift))
                return true;

            if (!anyKey && otherAnyKey && (other.m_EventModifier == EventModifiers.None || other.m_EventModifier == EventModifiers.Shift))
                return true;

            return false;
        }
        
        internal Binding Clone() {
            if (m_Editable)
                return new Binding(m_Action.Clone(), m_CharCode, m_KeyCode, m_EventModifier, m_Editable);
            return this;
        }
    }
}
