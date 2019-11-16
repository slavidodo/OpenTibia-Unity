using UnityEngine;

namespace OpenTibiaUnity.Core.Input.Mapping
{
    public class Binding
    {
        private bool _ignoreBlocker = true;
        private bool _editable = true;
        private char _charCode = '\0';
        private KeyCode _keyCode = KeyCode.None;
        private EventModifiers _eventModifier = EventModifiers.None;
        private uint _eventMask = 0;
        private IAction _action = null;

        public bool Editable { get => _editable; }
        public bool IgnoreBlocker { get => _ignoreBlocker; }
        public char CharCode { get => _charCode; }
        public KeyCode KeyCode { get => _keyCode; }
        public EventModifiers EventModifier { get => _eventModifier; }
        public uint EventMask { get => _eventMask; }
        public IAction Action { get => _action; }

        public Binding(IAction action, char charCode, KeyCode keyCode, EventModifiers eventModifier, bool ignoreBlocker = true, bool editable = true) {
            _action = action;
            if (action is StaticAction.StaticAction)
                _eventMask = (action as StaticAction.StaticAction).EventMask;
            else
                _eventMask = 0;

            Update(charCode, keyCode, eventModifier, ignoreBlocker);
            _editable = editable;
        }

        public void Update(char charCode, KeyCode keyCode, EventModifiers eventModifier, bool ignoreBlocker = true) {
            if (!_editable)
                return;

            _charCode = charCode;
            _keyCode = keyCode;
            _eventModifier = eventModifier;
            _ignoreBlocker = ignoreBlocker;
        }

        public void Update(Binding other) {
            if (!_editable)
                return;

            _charCode = other.CharCode;
            _keyCode = other.KeyCode;
            _eventModifier = other.EventModifier;
            _ignoreBlocker = other.IgnoreBlocker;
        }

        public bool AppliesTo(uint eventMask, KeyCode keyCode, EventModifiers keyModifer, bool blockerActive) {
            if (!((_eventMask & eventMask) != 0 && _keyCode == keyCode && _eventModifier == keyModifer))
                return false;

            return IgnoreBlocker || !blockerActive;
        }

        public bool Conflicts(Binding other) {
            if (other == null)
                return false;

            bool anyKey = (_eventMask & InputEvent.KeyAny) != 0;
            bool otherAnyKey = (other._eventMask & InputEvent.KeyAny) != 0;

            if (anyKey == otherAnyKey && _keyCode == other._keyCode && _eventModifier == other._eventModifier)
                return true;

            if (anyKey && !otherAnyKey && (_eventModifier == EventModifiers.None || _eventModifier == EventModifiers.Shift))
                return true;

            if (!anyKey && otherAnyKey && (other._eventModifier == EventModifiers.None || other._eventModifier == EventModifiers.Shift))
                return true;

            return false;
        }
        
        public Binding Clone() {
            if (_editable)
                return new Binding(_action.Clone(), _charCode, _keyCode, _eventModifier, _ignoreBlocker, _editable);
            return this;
        }
    }
}
