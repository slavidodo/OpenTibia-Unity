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
        public class ModifierKeyEvent : Utility.EventImpl<char, KeyCode, EventModifiers> { }
        public class KeyboardEvent : Utility.EventImpl<Event, bool> { }
        public class MouseEvent : Utility.EventImpl<Event, MouseButtons, bool> { }

        private const int KeyRepeatMinDelay = 250;

        private bool m_CaptureKeyboard = true;
        private bool m_CaptureMouse = true;
        private bool m_KeyboardHandlerActive = false;
        private bool m_MouseHandlerActive = false;
        private bool m_ignoreNextLeft = false;
        private bool m_ignoreNextRight = false;
        private bool m_Numlock = false;
        private KeyCode m_KeyCode = KeyCode.None;

        private Mapping.Mapping m_Mapping;

        private List<Binding> m_MovementBindings = null;
        private int[] m_KeyPressed = new int[500];
        private int[] m_MousePressed = new int[3];
        private int[] m_MouseDragged = new int[3];

        public ModifierKeyEvent onModifierKeyEvent = new ModifierKeyEvent();
        private KeyboardEvent onKeyDownEvent = new KeyboardEvent();
        private KeyboardEvent onKeyUpEvent = new KeyboardEvent();
        private MouseEvent onMouseDownEvent = new MouseEvent();
        private MouseEvent onMouseUpEvent = new MouseEvent();
        private MouseEvent onDragEvent = new MouseEvent();
        private MouseEvent onBeginDragEvent = new MouseEvent();
        private MouseEvent onEndDragEvent = new MouseEvent();

        public InputHandler() {
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnKeyboardRepeatTimer);
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnMouseRepeatTimer);
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
            if (m_CaptureKeyboard && m_MovementBindings != null && !OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf) {
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

        public bool IsKeyPressed(KeyCode keyCode) {
            return m_KeyPressed[(int)keyCode] != 0;
        }

        public bool IsMouseButtonPressed(MouseButtons mouseButton) {
            switch (mouseButton) {
                case MouseButtons.Left: return m_MousePressed[0] != 0;
                case MouseButtons.Right: return m_MousePressed[1] != 0;
                case MouseButtons.Middle: return m_MousePressed[2] != 0;
                case MouseButtons.Both: return m_MousePressed[0] != 0 && m_MousePressed[1] != 0;
            }
            return false;
        }

        public bool IsMouseButtonDragged(MouseButtons mouseButton) {
            switch (mouseButton) {
                case MouseButtons.Left: return m_MouseDragged[0] != 0;
                case MouseButtons.Right: return m_MouseDragged[1] != 0;
                case MouseButtons.Middle: return m_MouseDragged[2] != 0;
                case MouseButtons.Both: return m_MouseDragged[0] != 0 && m_MouseDragged[1] != 0;
            }
            return false;
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
                        if (!OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf) {
                            var type = m_KeyPressed[(int)e.keyCode] != 0 ? InputEvent.KeyRepeat : InputEvent.KeyDown;
                            m_Mapping.OnKeyInput(type, e.character, e.keyCode, e.modifiers);
                        }

                        onKeyDownEvent.InvokeWhile(e, m_KeyPressed[(int)e.keyCode] != 0, () => e.type != EventType.Used);
                    } else if (e.type == EventType.KeyUp) {
                        if (!OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf)
                            m_Mapping.OnKeyInput(InputEvent.KeyUp, e.character, e.keyCode, e.modifiers);

                        onKeyUpEvent.InvokeWhile(e, false, () => e.type != EventType.Used);
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

        public void OnMouseEvent(Event e) {
            if (m_MouseHandlerActive || e.button > m_MousePressed.Length)
                return;

            try {
                m_MouseHandlerActive = true;
                if (e.type == EventType.MouseUp)
                    m_MousePressed[e.button] = 0;

                if (m_CaptureMouse && !(e.alt && e.control) && e.button != 2) {
                    if (e.type == EventType.MouseDown) {
                        bool leftPressedEarlier = m_MousePressed[0] != 0;
                        bool rightPressedEarlier = m_MousePressed[1] != 0;

                        bool both;
                        if (e.button == 0) {
                            both = rightPressedEarlier;
                        } else {
                            both = leftPressedEarlier;
                        }

                        if (both) {
                            m_ignoreNextLeft = m_ignoreNextRight = false;
                            onMouseDownEvent.Invoke(e, MouseButtons.Both, leftPressedEarlier && rightPressedEarlier);
                        } else if (e.button == 0) {
                            onMouseDownEvent.Invoke(e, MouseButtons.Left, leftPressedEarlier);
                        } else if (e.button == 1) {
                            onMouseDownEvent.Invoke(e, MouseButtons.Right, rightPressedEarlier);
                        }
                    } else if (e.type == EventType.MouseUp) {
                        if (m_ignoreNextLeft && e.button == 0) {
                            m_ignoreNextLeft = false;
                        } else if (m_ignoreNextRight && e.button == 1) {
                            m_ignoreNextRight = false;
                        } else {
                            bool both;
                            if (e.button == 0) {
                                both = m_MousePressed[1] != 0;
                                m_ignoreNextRight = both && m_MouseDragged[1] == 0;
                            } else {
                                both = m_MousePressed[0] != 0;
                                m_ignoreNextLeft = both && m_MouseDragged[0] == 0;
                            }

                            var mouseButton = MouseButtons.Left;
                            if (both)
                                mouseButton = MouseButtons.Both;
                            else if (e.button == 1)
                                mouseButton = MouseButtons.Right;
                            
                            if (m_MouseDragged[e.button] != 0) {
                                m_MouseDragged[e.button] = 0;
                                onEndDragEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                                if (e.type != EventType.Used)
                                    onMouseUpEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                            } else {
                                onMouseUpEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                            }
                        }
                    }
                }

                if (e.type == EventType.MouseDown)
                    m_MousePressed[e.button] = OpenTibiaUnity.TicksMillis;
            } catch (System.Exception) {
            } finally {
                m_MouseHandlerActive = false;
            }
        }

        public void OnDragEvent(Event e) {
            if (m_MouseHandlerActive || e.button > m_MousePressed.Length)
                return;

            try {
                bool both = (e.button == 0 ? m_MousePressed[1] : m_MousePressed[0]) != 0;

                MouseButtons mouseButton = MouseButtons.None;
                if (both)
                    mouseButton = MouseButtons.Both;
                else if (e.button == 0)
                    mouseButton = MouseButtons.Left;
                else if (e.button == 1)
                    mouseButton = MouseButtons.Right;
                else if (e.button == 2)
                    mouseButton = MouseButtons.Middle;


                if (m_MouseDragged[e.button] == 0)
                    onBeginDragEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                else
                    onDragEvent.InvokeWhile(e, mouseButton, true, () => e.type != EventType.Used);

                m_MouseDragged[e.button] = OpenTibiaUnity.TicksMillis;
            } catch (System.Exception) {
            } finally {
                m_MouseHandlerActive = false;
            }
        }
        
        public bool IsModifierKeyPressed(Event e = null) {
            if (e != null)
                return e.alt || e.shift || e.control;

            return IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl)
                || IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift)
                || IsKeyPressed(KeyCode.LeftAlt) || IsKeyPressed(KeyCode.RightAlt);
        }

        public void AddKeyDownListener(Utility.EventImplPriority priority, System.Action<Event, bool> action) =>
            onKeyDownEvent.AddListener(priority, action);
        public void RemoveKeyDownListener(System.Action<Event, bool> action) =>
            onKeyDownEvent.RemoveListener(action);
        public void AddKeyUpListener(Utility.EventImplPriority priority, System.Action<Event, bool> action) =>
           onKeyUpEvent.AddListener(priority, action);
        public void RemoveKeyUpListener(System.Action<Event, bool> action) =>
            onKeyUpEvent.RemoveListener(action);

        public void AddMouseDownListener(Utility.EventImplPriority priority, System.Action<Event, MouseButtons, bool> action) =>
            onMouseDownEvent.AddListener(priority, action);
        public void RemoveMouseDownListener(System.Action<Event, MouseButtons, bool> action) =>
            onMouseDownEvent.RemoveListener(action);
        public void AddMouseUpListener(Utility.EventImplPriority priority, System.Action<Event, MouseButtons, bool> action) =>
            onMouseUpEvent.AddListener(priority, action);
        public void RemoveMouseUpListener(System.Action<Event, MouseButtons, bool> action) =>
            onMouseUpEvent.RemoveListener(action);

        public void AddDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButtons, bool> action) =>
            onDragEvent.AddListener(priority, action);
        public void RemoveDragListener(System.Action<Event, MouseButtons, bool> action) =>
            onDragEvent.RemoveListener(action);
        public void AddBeginDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButtons, bool> action) =>
            onBeginDragEvent.AddListener(priority, action);
        public void RemoveBeginDragListener(System.Action<Event, MouseButtons, bool> action) =>
            onBeginDragEvent.RemoveListener(action);
        public void AddEndDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButtons, bool> action) =>
            onEndDragEvent.AddListener(priority, action);
        public void RemoveEndDragListener(System.Action<Event, MouseButtons, bool> action) =>
            onEndDragEvent.RemoveListener(action);

        public EventModifiers GetRawEventModifiers() {
            EventModifiers eventModifiers = EventModifiers.None;

            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                eventModifiers |= EventModifiers.Alt;

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                eventModifiers |= EventModifiers.Control;

            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                eventModifiers |= EventModifiers.Shift;

            return eventModifiers;
        }

        public static bool IsGameObjectHighlighted(GameObject gameObject) {
            if (!gameObject)
                return true;
            else if (!gameObject.activeSelf)
                return false;

            var selectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (!selectedGameObject)
                return false;

            var currentTransform = selectedGameObject.transform;
            var rootTransform = currentTransform.root;
            while (currentTransform != rootTransform) {
                if (currentTransform.gameObject == gameObject)
                    return true;

                currentTransform = currentTransform.parent;
            }

            return false;
        }

        public static bool IsHighlighted(Component component) => IsGameObjectHighlighted(component?.gameObject);
    }
}
