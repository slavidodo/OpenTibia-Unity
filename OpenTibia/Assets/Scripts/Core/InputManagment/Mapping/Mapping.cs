using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.Mapping
{
    public class Mapping
    {
        private List<Binding> m_Bindings;

        public List<Binding> Bindings {
            get { return m_Bindings; }
        }

        public Mapping() {
            m_Bindings = new List<Binding>();
        }

        public void RemoveAll(bool param1 = true) {
            if (param1) {
                m_Bindings.Clear();
            } else {
                var length = m_Bindings.Count - 1;
                var index = length;
                while (index > 0) {
                    if (m_Bindings[index] == null || m_Bindings[index].Editable) {
                        length--;
                        var binding = m_Bindings[length];
                        m_Bindings[length] = m_Bindings[index];
                        m_Bindings[index] = binding;
                    }
                    
                    index--;
                }
                
                if (length >= 0 && (m_Bindings.Count - length + 1) > 0) {
                    m_Bindings.RemoveRange(length, m_Bindings.Count - length + 1);
                }
            }
        }

        public bool AddAll(Binding[] bindings) {
            var newLen = bindings.Length;
            var currentLen = m_Bindings.Count;

            for (int i = 0; i < bindings.Length; i++) {
                if (bindings[i] == null)
                    throw new ArgumentException("Mapping.AddAll: Invalid input.");

                var conflictingBinding = GetConflictingBinding(bindings[i]);
                if (conflictingBinding != null) {
                    if (currentLen == 0)
                        m_Bindings.Clear();
                    else
                        m_Bindings.RemoveRange(currentLen - 1, i + 1);
                    return false;
                }

                m_Bindings.Add(bindings[i].Clone());
            }

            return true;
        }

        public void OnKeyInput(uint eventMask, char character, KeyCode keyCode, EventModifiers rawModifiers) {
            EventModifiers eventModifiers = EventModifiers.None;
            if ((rawModifiers & EventModifiers.Shift) != 0) eventModifiers |= EventModifiers.Shift;
            if ((rawModifiers & EventModifiers.Control) != 0) eventModifiers |= EventModifiers.Control;
            if ((rawModifiers & EventModifiers.Alt) != 0) eventModifiers |= EventModifiers.Alt;

            foreach (var binding in m_Bindings) {
                if (binding.AppliesTo(eventMask, keyCode, eventModifiers)) {
                    if (binding.Action is StaticAction.StaticAction) {
                        var staticAction = binding.Action as StaticAction.StaticAction;
                        staticAction.KeyCallback(eventMask, character, keyCode, eventModifiers);
                    } else {
                        binding.Action.Perform(eventMask == InputEvent.KeyRepeat);
                    }

                    break;
                }
            }
        }

        public void OnTextInput(uint eventMask, char character) {
            foreach (var binding in m_Bindings) {
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
            foreach (var binding in m_Bindings) {
                if (binding.Action.Equals(action))
                    bindings.Add(binding);
            }

            return bindings;
        }

        public Binding GetConflictingBinding(Binding binding) {
            foreach (var other in m_Bindings) {
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


            m_Bindings.Add(binding);
            return true;
        }

        public void RemoveItem(Binding binding) {
            if (binding == null)
                throw new ArgumentException("Mapping.RemoveItem: invalid argument");

            m_Bindings.Remove(binding);
        }
    }
}
