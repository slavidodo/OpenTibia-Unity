using UnityEngine;

namespace OpenTibiaUnity.Core.InputManagment.Mapping
{
    public struct MouseBinding
    {
        public MouseButtons MouseButton;
        public KeyCode KeyCode;
        public AppearanceActions Action;

        public MouseBinding(MouseButtons mouseButton, KeyCode keyCode, AppearanceActions action) {
            MouseButton = mouseButton;
            KeyCode = keyCode;
            Action = action;
        }

        public bool AppliesTo(MouseButtons mouseButton, KeyCode keyCode) {
            return MouseButton == mouseButton && KeyCode == keyCode;
        }
    }
}
