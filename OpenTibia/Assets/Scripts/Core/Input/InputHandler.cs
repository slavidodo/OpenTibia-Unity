using OpenTibiaUnity.Core.Input.Mapping;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityInput = UnityEngine.Input;

namespace OpenTibiaUnity.Core.Input
{
    internal class InputHandler
    {
        internal class ModifierKeyEvent : Utility.EventImpl<char, KeyCode, EventModifiers> { }
        internal class KeyboardEvent : Utility.EventImpl<Event, bool> { }
        internal class MouseEvent : Utility.EventImpl<Event, MouseButton, bool> { }

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

        internal ModifierKeyEvent onModifierKeyEvent = new ModifierKeyEvent();
        private KeyboardEvent onKeyDownEvent = new KeyboardEvent();
        private KeyboardEvent onKeyUpEvent = new KeyboardEvent();
        private MouseEvent onMouseDownEvent = new MouseEvent();
        private MouseEvent onMouseUpEvent = new MouseEvent();
        private MouseEvent onDragEvent = new MouseEvent();
        private MouseEvent onBeginDragEvent = new MouseEvent();
        private MouseEvent onEndDragEvent = new MouseEvent();

        internal InputHandler() {
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnKeyboardRepeatTimer);
            OpenTibiaUnity.GameManager.AddSecondaryTimerListener(OnMouseRepeatTimer);
        }

        internal void Cleanup() {
            OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnKeyboardRepeatTimer);
            OpenTibiaUnity.GameManager.RemoveSecondaryTimerListener(OnMouseRepeatTimer);
        }

        internal void UpdateMapping() {
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

        internal void OnKeyboardRepeatTimer() {
            // Currently this is only useful for movement //
            if (m_CaptureKeyboard && m_MovementBindings != null && !OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf) {
                var ticks = OpenTibiaUnity.TicksMillis;

                EventModifiers modifier = EventModifiers.None;
                if (IsKeyPressed(KeyCode.LeftAlt) || IsKeyPressed(KeyCode.RightAlt)) modifier |= EventModifiers.Alt;
                if (IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl)) modifier |= EventModifiers.Control;
                if (IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift)) modifier |= EventModifiers.Shift;

                bool isBlockerActive = OpenTibiaUnity.GameManager.ActiveBlocker.gameObject.activeSelf;
                foreach (var binding in m_MovementBindings) {
                    if (IsKeyPressed(binding.KeyCode) && m_KeyPressed[(int)binding.KeyCode] + KeyRepeatMinDelay < ticks
                        && binding.AppliesTo((binding.Action as StaticAction.PlayerMove).EventMask, binding.KeyCode, modifier, isBlockerActive)) {
                        binding.Action.Perform(true);
                        break;
                    }
                }
            }
        }

        internal void OnMouseRepeatTimer() {
            // Do something when mouse is held
        }

        internal void ClearPressedKeys() {
            m_KeyPressed = new int[500];
        }

        internal bool IsKeyPressed(KeyCode keyCode) {
            return m_KeyPressed[(int)keyCode] != 0;
        }

        internal bool IsMouseButtonPressed(MouseButton mouseButton) {
            switch (mouseButton) {
                case MouseButton.Left: return m_MousePressed[0] != 0;
                case MouseButton.Right: return m_MousePressed[1] != 0;
                case MouseButton.Middle: return m_MousePressed[2] != 0;
                case MouseButton.Both: return m_MousePressed[0] != 0 && m_MousePressed[1] != 0;
            }
            return false;
        }

        internal bool IsMouseButtonDragged(MouseButton mouseButton) {
            switch (mouseButton) {
                case MouseButton.Left: return m_MouseDragged[0] != 0;
                case MouseButton.Right: return m_MouseDragged[1] != 0;
                case MouseButton.Middle: return m_MouseDragged[2] != 0;
                case MouseButton.Both: return m_MouseDragged[0] != 0 && m_MouseDragged[1] != 0;
            }
            return false;
        }

        internal void OnKeyEvent(Event e) {
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
                        var type = m_KeyPressed[(int)e.keyCode] != 0 ? InputEvent.KeyRepeat : InputEvent.KeyDown;
                        bool handled = m_Mapping.OnKeyInput(type, e.character, e.keyCode, e.modifiers);
                        if (!handled)
                            onKeyDownEvent.InvokeWhile(e, m_KeyPressed[(int)e.keyCode] != 0, () => e.type != EventType.Used);
                        else
                            e.Use();
                    } else if (e.type == EventType.KeyUp) {
                        bool handled = m_Mapping.OnKeyInput(InputEvent.KeyUp, e.character, e.keyCode, e.modifiers);
                        if (!handled)
                            onKeyUpEvent.InvokeWhile(e, false, () => e.type != EventType.Used);
                        else
                            e.Use();
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

        internal void OnMouseEvent(Event e) {
            if (m_MouseHandlerActive || e.button > m_MousePressed.Length)
                return;

            try {
                m_MouseHandlerActive = true;
                var eventType = e.type;
                var rawMouseButton = e.button;
                if (eventType == EventType.MouseUp)
                    m_MousePressed[e.button] = 0;

                if (m_CaptureMouse && !(e.alt && e.control) && e.button != 2) {
                    if (eventType == EventType.MouseDown) {
                        bool leftPressedEarlier = m_MousePressed[0] != 0;
                        bool rightPressedEarlier = m_MousePressed[1] != 0;

                        bool both;
                        if (e.button == 0)
                            both = rightPressedEarlier;
                        else
                            both = leftPressedEarlier;

                        if (both) {
                            m_ignoreNextLeft = m_ignoreNextRight = false;
                            onMouseDownEvent.Invoke(e, MouseButton.Both, leftPressedEarlier && rightPressedEarlier);
                        } else if (e.button == 0) {
                            onMouseDownEvent.Invoke(e, MouseButton.Left, leftPressedEarlier);
                        } else if (e.button == 1) {
                            onMouseDownEvent.Invoke(e, MouseButton.Right, rightPressedEarlier);
                        }
                    } else if (eventType == EventType.MouseUp) {
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

                            var mouseButton = MouseButton.Left;
                            if (both)
                                mouseButton = MouseButton.Both;
                            else if (e.button == 1)
                                mouseButton = MouseButton.Right;

                            if (m_MouseDragged[e.button] != 0) {
                                m_MouseDragged[e.button] = 0;
                                onEndDragEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                            }

                            if (e.type != EventType.Used)
                                onMouseUpEvent.InvokeWhile(e, mouseButton, false, () => e.type != EventType.Used);
                        }
                    }
                }

                if (eventType == EventType.MouseDown)
                    m_MousePressed[e.button] = OpenTibiaUnity.TicksMillis;
            } catch (System.Exception) {
            } finally {
                m_MouseHandlerActive = false;
            }
        }

        internal void OnDragEvent(Event e) {
            if (m_MouseHandlerActive || e.button > m_MousePressed.Length)
                return;

            try {
                bool both = (e.button == 0 ? m_MousePressed[1] : m_MousePressed[0]) != 0;

                MouseButton mouseButton = MouseButton.None;
                if (both)
                    mouseButton = MouseButton.Both;
                else if (e.button == 0)
                    mouseButton = MouseButton.Left;
                else if (e.button == 1)
                    mouseButton = MouseButton.Right;
                else if (e.button == 2)
                    mouseButton = MouseButton.Middle;


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
        
        internal bool IsModifierKeyPressed(Event e = null) {
            if (e != null)
                return e.alt || e.shift || e.control;

            return IsKeyPressed(KeyCode.LeftControl) || IsKeyPressed(KeyCode.RightControl)
                || IsKeyPressed(KeyCode.LeftShift) || IsKeyPressed(KeyCode.RightShift)
                || IsKeyPressed(KeyCode.LeftAlt) || IsKeyPressed(KeyCode.RightAlt);
        }

        internal void AddKeyDownListener(Utility.EventImplPriority priority, System.Action<Event, bool> action) =>
            onKeyDownEvent.AddListener(priority, action);
        internal void RemoveKeyDownListener(System.Action<Event, bool> action) =>
            onKeyDownEvent.RemoveListener(action);
        internal void AddKeyUpListener(Utility.EventImplPriority priority, System.Action<Event, bool> action) =>
           onKeyUpEvent.AddListener(priority, action);
        internal void RemoveKeyUpListener(System.Action<Event, bool> action) =>
            onKeyUpEvent.RemoveListener(action);

        internal void AddMouseDownListener(Utility.EventImplPriority priority, System.Action<Event, MouseButton, bool> action) =>
            onMouseDownEvent.AddListener(priority, action);
        internal void RemoveMouseDownListener(System.Action<Event, MouseButton, bool> action) =>
            onMouseDownEvent.RemoveListener(action);
        internal void AddMouseUpListener(Utility.EventImplPriority priority, System.Action<Event, MouseButton, bool> action) =>
            onMouseUpEvent.AddListener(priority, action);
        internal void RemoveMouseUpListener(System.Action<Event, MouseButton, bool> action) =>
            onMouseUpEvent.RemoveListener(action);

        internal void AddDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButton, bool> action) =>
            onDragEvent.AddListener(priority, action);
        internal void RemoveDragListener(System.Action<Event, MouseButton, bool> action) =>
            onDragEvent.RemoveListener(action);
        internal void AddBeginDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButton, bool> action) =>
            onBeginDragEvent.AddListener(priority, action);
        internal void RemoveBeginDragListener(System.Action<Event, MouseButton, bool> action) =>
            onBeginDragEvent.RemoveListener(action);
        internal void AddEndDragListener(Utility.EventImplPriority priority, System.Action<Event, MouseButton, bool> action) =>
            onEndDragEvent.AddListener(priority, action);
        internal void RemoveEndDragListener(System.Action<Event, MouseButton, bool> action) =>
            onEndDragEvent.RemoveListener(action);

        internal EventModifiers GetRawEventModifiers() {
            EventModifiers eventModifiers = EventModifiers.None;

            if (UnityInput.GetKeyDown(KeyCode.LeftAlt) || UnityInput.GetKeyDown(KeyCode.RightAlt) || UnityInput.GetKey(KeyCode.LeftAlt) || UnityInput.GetKey(KeyCode.RightAlt))
                eventModifiers |= EventModifiers.Alt;

            if (UnityInput.GetKeyDown(KeyCode.LeftControl) || UnityInput.GetKeyDown(KeyCode.RightControl) || UnityInput.GetKey(KeyCode.LeftControl) || UnityInput.GetKey(KeyCode.RightControl))
                eventModifiers |= EventModifiers.Control;

            if (UnityInput.GetKeyDown(KeyCode.LeftShift) || UnityInput.GetKeyDown(KeyCode.RightShift) || UnityInput.GetKey(KeyCode.LeftShift) || UnityInput.GetKey(KeyCode.RightShift))
                eventModifiers |= EventModifiers.Shift;

            return eventModifiers;
        }

        internal static bool IsGameObjectHighlighted(GameObject gameObject) {
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

        internal static bool IsHighlighted(Component component) => IsGameObjectHighlighted(component?.gameObject);
    }
}
