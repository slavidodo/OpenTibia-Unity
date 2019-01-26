using System.Collections.Generic;
using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.Mapping
{
    public class MouseMapping {
        public static MouseMapping ClassicMapping;
        public static MouseMapping RefularMapping;
        public static MouseMapping LeftSmartMapping;

        static MouseMapping() {
            MouseBinding[] classicMouseBindings = new MouseBinding[] {
                new MouseBinding(MouseButtons.Left, KeyCode.None, AppearanceActions.AutoWalk),
                new MouseBinding(MouseButtons.Left, KeyCode.LeftShift, AppearanceActions.Look),
                new MouseBinding(MouseButtons.Both, KeyCode.None, AppearanceActions.Look),

                new MouseBinding(MouseButtons.Left, KeyCode.LeftAlt, AppearanceActions.AttackOrTalk),
                new MouseBinding(MouseButtons.Right, KeyCode.None, AppearanceActions.Attack),
                new MouseBinding(MouseButtons.Right, KeyCode.LeftAlt, AppearanceActions.AttackOrTalk),

                new MouseBinding(MouseButtons.Left, KeyCode.LeftControl, AppearanceActions.ContextMenu),
                new MouseBinding(MouseButtons.Right, KeyCode.LeftControl, AppearanceActions.ContextMenu),
            };

            MouseBinding[] regularMouseBindings = new MouseBinding[] {
                new MouseBinding(MouseButtons.Left, KeyCode.None, AppearanceActions.AutoWalk),
                new MouseBinding(MouseButtons.Left, KeyCode.LeftShift, AppearanceActions.Look),
                new MouseBinding(MouseButtons.Left, KeyCode.LeftAlt, AppearanceActions.AttackOrTalk),
                new MouseBinding(MouseButtons.Right, KeyCode.None, AppearanceActions.ContextMenu),
                new MouseBinding(MouseButtons.Right, KeyCode.LeftAlt, AppearanceActions.AttackOrTalk),

                new MouseBinding(MouseButtons.Left, KeyCode.LeftControl, AppearanceActions.UseOrOpen),
                new MouseBinding(MouseButtons.Right, KeyCode.LeftControl, AppearanceActions.UseOrOpen),
            };

            MouseBinding[] smartMouseBindings = new MouseBinding[] {
                new MouseBinding(MouseButtons.Left, KeyCode.None, AppearanceActions.SmartClick),
                new MouseBinding(MouseButtons.Left, KeyCode.LeftShift, AppearanceActions.Look),
                new MouseBinding(MouseButtons.Right, KeyCode.None, AppearanceActions.ContextMenu),
            };

            ClassicMapping = new MouseMapping(classicMouseBindings);
            RefularMapping = new MouseMapping(regularMouseBindings);
            LeftSmartMapping = new MouseMapping(smartMouseBindings);
        }

        private List<MouseBinding> m_MouseBindings;

        public MouseMapping(MouseBinding[] bindings = null) {
            if (bindings != null)
                m_MouseBindings = new List<MouseBinding>(bindings);
            else
                m_MouseBindings = new List<MouseBinding>();
        }

        public MouseBinding? DetermineBinding() {
            var mouseButton = InternalGetMouseButton();
            var keyCode = InternalGetInputKeyCode();

            if (mouseButton == MouseButtons.None)
                return null;
            
            foreach (var binding in m_MouseBindings) {
                if (binding.AppliesTo(mouseButton, keyCode))
                    return binding;
            }

            return null;
        }

        private KeyCode InternalGetInputKeyCode() {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftControl)
                || Input.GetKey(KeyCode.RightControl) || Input.GetKeyUp(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.RightControl)) {
                return KeyCode.LeftControl;
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.LeftShift)
                || Input.GetKey(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.RightShift)) {
                return KeyCode.LeftShift;
            }

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.LeftAlt)
                || Input.GetKey(KeyCode.RightAlt) || Input.GetKeyUp(KeyCode.RightAlt) || Input.GetKeyDown(KeyCode.RightAlt)) {
                return KeyCode.LeftAlt;
            }

            return KeyCode.None;
        }

        private MouseButtons InternalGetMouseButton() {
            bool leftUp = Input.GetMouseButtonUp(0);
            bool leftDown = Input.GetMouseButtonDown(0);
            bool leftHeld = Input.GetMouseButton(0);

            bool rightUp = Input.GetMouseButtonUp(1);
            bool rightDown = Input.GetMouseButtonDown(1);
            bool rightHeld = Input.GetMouseButton(1);

            if ((leftUp && (rightUp || rightDown || rightHeld)) || (rightUp && (leftUp || leftDown || leftHeld)))
                return MouseButtons.Both; // one is up, the other is anything
            else if (leftUp)
                return MouseButtons.Left;
            else if (rightUp)
                return MouseButtons.Right;
            else if (Input.GetMouseButtonUp(2))
                return MouseButtons.Middle;

            return MouseButtons.None;
        }
    }
}
