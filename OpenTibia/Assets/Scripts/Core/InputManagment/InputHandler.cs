using OpenTibiaUnity.Core.InputManagment.Mapping;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.InputManagment
{
    public class InputHandler
    {
        public class ModifierKeyEvent : UnityEvent<char, KeyCode, EventModifiers> { }
        public class CustomEvent : UnityEvent<Event, bool> { }

        private const int KeyRepeatMinDelay = 250;

        private bool m_CaptureKeyboard = true;
        private bool m_CaptureMouse = true;
        private bool m_KeyboardHandlerActive = false;
        private bool m_MouseHandlerActive = false;
        private bool m_Numlock = false;
        private KeyCode m_KeyCode = KeyCode.None;

        private Mapping.Mapping m_Mapping;

        private List<Binding> m_MovementBindings = null;
        private int[] m_KeyPressed;
        private int[] m_MousePressed;

        public ModifierKeyEvent onModifierKeyEvent = new ModifierKeyEvent();
        private CustomEvent onKeyDownEvent = new CustomEvent();
        private CustomEvent onKeyUpEvent = new CustomEvent();
        private CustomEvent onLeftMouseDownEvent = new CustomEvent();
        private CustomEvent onLeftMouseUpEvent = new CustomEvent();
        private CustomEvent onRightMouseDownEvent = new CustomEvent();
        private CustomEvent onRightMouseUpEvent = new CustomEvent();

        public InputHandler() {
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnKeyboardRepeatTimer);
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnMouseRepeatTimer);
            m_KeyPressed = new int[500];
            m_MousePressed = new int[3]; // left, right, middle
        }

        public void Cleanup() {
            OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnKeyboardRepeatTimer);
            OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnMouseRepeatTimer);
        }

        public void UpdateMapping() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            Mapping.Mapping mapping = null;
            MappingSet mappingSet = optionStorage.GetMappingSet(optionStorage.GeneralInputSetID);
            if (mappingSet != null) {
                if (optionStorage.GeneralInputSetMode == MappingSet.ChatModeOFF)
                    mapping = mappingSet.ChatModeOffMapping;
                else
                    mapping = mappingSet.ChatModeOnMapping;
            }
            
            if (mapping != null && mapping.Bindings != null) {
                m_MovementBindings = new List<Binding>();
                foreach (var binding in mapping.Bindings) {
                    if (binding.Action != null && (binding.Action as StaticAction.PlayerMove) != null)
                        m_MovementBindings.Add(binding);
                }
            }

            m_Mapping = mapping;
        }

        public void OnKeyboardRepeatTimer() {
            // Currently this is only useful for movement //
            if (m_CaptureKeyboard && m_MovementBindings != null && !OpenTibiaUnity.GameManager.ActiveOverlay.gameObject.activeSelf) {
                var ticks = OpenTibiaUnity.TicksMillis;

                EventModifiers modifier = EventModifiers.None;
                if (IsKeyPressed(KeyCode.LeftAlt) || IsKeyPressed(KeyCode.RightAlt)) modifier |= EventModifiers.Alt;
                if (IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl)) modifier |= EventModifiers.Control;
                if (IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift)) modifier |= EventModifiers.Shift;

                foreach (var binding in m_MovementBindings) {
                    if (IsKeyPressed(binding.KeyCode) && m_KeyPressed[(int)binding.KeyCode] + KeyRepeatMinDelay < ticks
                        && binding.AppliesTo((binding.Action as StaticAction.PlayerMove).EventMask, binding.KeyCode, modifier)) {
                        binding.Action.Perform(true);
                        break;
                    }
                }
            }
        }

        public void OnMouseRepeatTimer() {
            // Do something when mouse is held
        }

        public void ClearPressedKeys() {
            m_KeyPressed = new int[500];
        }

        private bool IsKeyPressed(KeyCode keyCode) {
            return m_KeyPressed[(int)keyCode] != 0;
        }

        public void OnKeyEvent(Event e) {
            if (m_KeyboardHandlerActive || (int)e.keyCode >= m_KeyPressed.Length)
                return;
            
            try {
                m_KeyboardHandlerActive = true;
                var keyModified = false;
                if (e.type == EventType.KeyUp) {
                    keyModified = m_KeyPressed[(int)e.keyCode] != 0;
                    m_KeyPressed[(int)e.keyCode] = 0;

                    bool numlock = (e.modifiers & EventModifiers.Numeric) != 0;
                    if (numlock != m_Numlock) {
                        ClearPressedKeys();
                        m_Numlock = numlock;
                    }
                }

                if (m_CaptureKeyboard && m_Mapping != null && !(e.alt && e.control)) {
                    if (e.type == EventType.KeyDown) {
                        if (!OpenTibiaUnity.GameManager.ActiveOverlay.gameObject.activeSelf) {
                            var type = m_KeyPressed[(int)e.keyCode] != 0 ? InputEvent.KeyRepeat : InputEvent.KeyDown;
                            m_Mapping.OnKeyInput(type, e.character, e.keyCode, e.modifiers);
                        }

                        onKeyDownEvent.Invoke(e, m_KeyPressed[(int)e.keyCode] != 0);
                    } else if (e.type == EventType.KeyUp) {
                        if (!OpenTibiaUnity.GameManager.ActiveOverlay.gameObject.activeSelf)
                            m_Mapping.OnKeyInput(InputEvent.KeyUp, e.character, e.keyCode, e.modifiers);

                        onKeyUpEvent.Invoke(e, false);
                    }
                }

                if (e.type == EventType.KeyDown) {
                    m_KeyCode = e.keyCode;
                    keyModified = m_KeyPressed[(int)e.keyCode] != 0;
                    m_KeyPressed[(int)e.keyCode] = OpenTibiaUnity.TicksMillis;
                }

                if (keyModified)
                    onModifierKeyEvent.Invoke(e.character, e.keyCode, e.modifiers);

            } catch (System.Exception) {
            } finally {
                m_KeyboardHandlerActive = false;
            }
        }

        public void OnMouseDown(Event e) {
            if (m_MouseHandlerActive || e.button > m_MousePressed.Length)
                return;

            try {
                m_MouseHandlerActive = true;
                if (e.type == EventType.MouseUp) {
                    m_MousePressed[e.button] = 0;
                }

                if (m_CaptureMouse && !(e.alt && e.control)) {
                    if (e.type == EventType.MouseDown) {
                        if (e.button == 0)
                            onLeftMouseDownEvent.Invoke(e, m_MousePressed[e.button] != 0);
                        else if (e.button == 1)
                            onRightMouseDownEvent.Invoke(e, m_MousePressed[e.button] != 0);
                    } else if (e.type == EventType.MouseUp) {
                        if (e.button == 0)
                            onLeftMouseUpEvent.Invoke(e, false);
                        else if (e.button == 1)
                            onRightMouseUpEvent.Invoke(e, false);
                    }
                }

                if (e.type == EventType.MouseDown) {
                    m_MousePressed[e.button] = OpenTibiaUnity.TicksMillis;
                }
            } catch (System.Exception) {

            } finally {
                m_MouseHandlerActive = false;
            }

        }

        public void OnMouseUp(Event e) {

        }

        public bool IsModifierKeyPressed(Event e = null) {
            if (e != null)
                return e.alt || e.shift || e.control;

            return IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl)
                || IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift)
                || IsKeyPressed(KeyCode.LeftAlt) || IsKeyPressed(KeyCode.RightAlt);
        }

        public void AddKeyDownListener(UnityAction<Event, bool> action) => onKeyDownEvent.AddListener(action);
        public void AddKeyUpListener(UnityAction<Event, bool> action) => onKeyUpEvent.AddListener(action);
        public void RemoveKeyDownListener(UnityAction<Event, bool> action) => onKeyDownEvent.RemoveListener(action);
        public void RemoveKeyUpListener(UnityAction<Event, bool> action) => onKeyUpEvent.RemoveListener(action);

        public void AddLeftMouseUpListener(UnityAction<Event, bool> action) => onLeftMouseUpEvent.AddListener(action);
        public void AddLeftMouseDownListener(UnityAction<Event, bool> action) => onLeftMouseDownEvent.AddListener(action);
        public void RemoveLeftMouseUpListener(UnityAction<Event, bool> action) => onLeftMouseUpEvent.RemoveListener(action);
        public void RemoveLeftMouseDownListener(UnityAction<Event, bool> action) => onLeftMouseDownEvent.RemoveListener(action);

        public void AddRightMouseUpListener(UnityAction<Event, bool> action) => onRightMouseUpEvent.AddListener(action);
        public void AddRightMouseDownListener(UnityAction<Event, bool> action) => onRightMouseDownEvent.AddListener(action);
        public void RemoveRightMouseUpListener(UnityAction<Event, bool> action) => onRightMouseUpEvent.RemoveListener(action);
        public void RemoveRightMouseDownListener(UnityAction<Event, bool> action) => onRightMouseDownEvent.RemoveListener(action);

        public static bool IsGameObjectHighlighted(GameObject gameObject) {
            if (!gameObject)
                return true;
            else if (!gameObject.activeSelf)
                return false;

            var selectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (!selectedGameObject)
                return false;

            var root = selectedGameObject.transform.root;
            var parent = selectedGameObject.transform;
            while (parent != root) {
                if (parent.gameObject == gameObject)
                    return true;

                parent = parent.parent;
            }

            return false;
        }

        public static bool IsHighlighted(Component component) => IsGameObjectHighlighted(component?.gameObject);
    }
}
