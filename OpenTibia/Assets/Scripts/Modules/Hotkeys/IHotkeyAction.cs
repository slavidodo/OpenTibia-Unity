namespace OpenTibiaUnity.Modules.Hotkeys
{
    internal interface IHotkeyAction
    {
        void Apply();
    }

    internal class HotkeyTextAction : IHotkeyAction
    {
        internal string Text { get; set; }
        internal bool AutoSend { get; set; }

        internal HotkeyTextAction(string text, bool autoSend = false) {
            Text = text;
            AutoSend = autoSend;
        }

        public void Apply() {
            Core.Input.GameAction.GameActionFactory.CreateTalkAction(Text, AutoSend).Perform();
        }
    }

    internal class HotkeyObjectAction : IHotkeyAction
    {
        internal Core.Appearances.AppearanceType AppearanceType { get; set; }
        internal UseActionTarget ActionTarget { get; set; }

        internal HotkeyObjectAction(ushort objectID, UseActionTarget actionTarget) {
            var appearanceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectID);
            if (!appearanceType)
                throw new System.Exception("HotkeyObjectAction.HotkeyObjectAction: invalid object id.");

            AppearanceType = appearanceType;
            ActionTarget = actionTarget;
        }

        internal HotkeyObjectAction(Core.Appearances.AppearanceType appearanceType, UseActionTarget actionTarget) {
            if (!appearanceType)
                throw new System.ArgumentNullException("HotkeyObjectAction.HotkeyObjectAction: invalid appearance type");

            if (appearanceType.Category != AppearanceCategory.Object)
                throw new System.Exception("HotkeyObjectAction.HotkeyObjectAction: invalid appearance type.");

            AppearanceType = appearanceType;
            ActionTarget = actionTarget;
        }

        internal HotkeyObjectAction(Core.Appearances.ObjectInstance @object, UseActionTarget actionTarget) {
            if (!@object)
                throw new System.ArgumentNullException("HotkeyObjectAction.HotkeyObjectAction: invalid object");

            AppearanceType = @object.Type;
            ActionTarget = actionTarget;
        }

        public void Apply() {
            var absolutePosition = new UnityEngine.Vector3Int(65535, 0, 0);
            if (ActionTarget == UseActionTarget.CrossHair) {
                var @object = new Core.Appearances.ObjectInstance(AppearanceType.ID, AppearanceType, 0);
                Core.Game.ObjectMultiUseHandler.Activate(absolutePosition, @object, 0);
                return;
            }
            
            // this is guaranteed to only be "self/target"
            // if the item is not multiuse (i.e a rune that can only be used on yourself, it will be only used)
            Core.Input.GameAction.GameActionFactory.CreateUseAction(absolutePosition, AppearanceType, 0, UnityEngine.Vector3Int.zero, null, 0, ActionTarget).Perform();
        }
    }

    internal class HotkeyEquipAction : IHotkeyAction
    {
        public void Apply() {
            
        }
    }
}
