using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.Input.Mapping
{
    public class Mapping
    {
        private List<Binding> _bindings;

        public List<Binding> Bindings {
            get { return _bindings; }
        }

        public Mapping() {
            _bindings = new List<Binding>();
        }

        public void RemoveAll(bool param1 = true) {
            if (param1) {
                _bindings.Clear();
            } else {
                int length = _bindings.Count - 1;
                int index = length;
                while (index > 0) {
                    if (_bindings[index] == null || _bindings[index].Editable) {
                        length--;
                        var binding = _bindings[length];
                        _bindings[length] = _bindings[index];
                        _bindings[index] = binding;
                    }
                    
                    index--;
                }
                
                if (length >= 0 && (_bindings.Count - length + 1) > 0) {
                    _bindings.RemoveRange(length, _bindings.Count - length + 1);
                }
            }
        }

        public bool AddAll(Binding[] bindings) {
            var newLen = bindings.Length;
            var currentLen = _bindings.Count;

            for (int i = 0; i < bindings.Length; i++) {
                if (bindings[i] == null)
                    throw new ArgumentException("Mapping.AddAll: Invalid input.");

                var conflictingBinding = GetConflictingBinding(bindings[i]);
                if (conflictingBinding != null) {
                    if (currentLen == 0)
                        _bindings.Clear();
                    else
                        _bindings.RemoveRange(currentLen - 1, i + 1);
                    return false;
                }

                _bindings.Add(bindings[i].Clone());
            }

            return true;
        }

        public bool OnKeyInput(InputEvent eventMask, char character, KeyCode keyCode, EventModifiers rawModifiers) {
            EventModifiers eventModifiers = EventModifiers.None;
            if ((rawModifiers & EventModifiers.Shift) != 0) eventModifiers |= EventModifiers.Shift;
            if ((rawModifiers & EventModifiers.Control) != 0) eventModifiers |= EventModifiers.Control;
            if ((rawModifiers & EventModifiers.Alt) != 0) eventModifiers |= EventModifiers.Alt;

            bool isBlockerActive = OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf;
            foreach (var binding in _bindings) {
                if (binding.AppliesTo(eventMask, keyCode, eventModifiers)) {
                    if (binding.Action is StaticAction.StaticAction staticAction)
                        return staticAction.KeyCallback(eventMask, character, keyCode, eventModifiers);
                    else
                        return binding.Action.Perform(eventMask == InputEvent.KeyRepeat);
                }
            }

            return false;
        }

        public void OnTextInput(InputEvent eventMask, char character) {
            foreach (var binding in _bindings) {
                if (binding.AppliesTo(eventMask, KeyCode.None, EventModifiers.None)) {
                    if (binding.Action is StaticAction.StaticAction) {
                        var staticAction = binding.Action as StaticAction.StaticAction;
                        staticAction.TextCallback(eventMask, character);
                    } else {
                        binding.Action.Perform(eventMask == InputEvent.KeyRepeat);
                    }

                    break;
                }
            }
        }

        public List<Binding> GetBindingsByAction(IAction action) {
            var bindings = new List<Binding>();
            foreach (var binding in _bindings) {
                if (binding.Action.Equals(action))
                    bindings.Add(binding);
            }

            return bindings;
        }

        public Binding GetConflictingBinding(Binding binding) {
            foreach (var other in _bindings) {
                if (other.Conflicts(binding)) {
                    return other;
                }
            }

            return null;
        }

        public bool AddItem(Binding binding) {
            if (binding == null)
                throw new ArgumentException("Mapping.AddItem: invalid argument");

            if (GetConflictingBinding(binding) != null)
                return false;
            
            _bindings.Add(binding);
            return true;
        }

        public void RemoveItem(Binding binding) {
            if (binding == null)
                throw new ArgumentException("Mapping.RemoveItem: invalid argument");

            _bindings.Remove(binding);
        }
    }
}
