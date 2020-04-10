using Newtonsoft.Json.Linq;

namespace OpenTibiaUnity.Modules.Options
{
    public abstract class HotkeyAction
    {
        public abstract void Apply();

        public abstract JObject Serialize();

        public override string ToString() {
            return Serialize().ToString();
        }

        public static HotkeyAction Unserialize(JObject data) {
            string typeToken = data["type"]?.ToString();
            if (typeToken == null)
                return null;

            if (typeToken == "text") {
                string text = data["text"]?.ToString();
                bool autoSend = (bool)data["autoSend"];
                return new HotkeyTextAction(text, autoSend);
            } else if (typeToken == "use") {
                var objectIdToken = data["objectId"];
                ushort objectId = 0;
                if (objectIdToken == null || !ushort.TryParse(objectIdToken.ToString(), out objectId))
                    return null;

                var useTargetToken = data["useTarget"]?.ToString();
                var useTarget = UseActionTarget.CrossHair;
                if (useTargetToken != null) {
                    if (useTargetToken == "self")
                        useTarget = UseActionTarget.Self;
                    else if (useTargetToken == "target")
                        useTarget = UseActionTarget.Target;
                    else if (useTargetToken == "none")
                        useTarget = UseActionTarget.Auto;
                }

                return new HotkeyObjectAction(objectId, useTarget);
            }

            return null;
        }
    }

    public class HotkeyTextAction : HotkeyAction
    {
        public string Text { get; set; }
        public bool AutoSend { get; set; }

        public HotkeyTextAction(string text, bool autoSend = false) {
            Text = text;
            AutoSend = autoSend;
        }

        public override void Apply() {
            new Core.Input.GameAction.TalkActionImpl(Text, AutoSend).Perform();
        }

        public override JObject Serialize() {
            var jobject = new JObject();
            jobject.Add("type", "text");
            jobject.Add("text", Text);
            jobject.Add("autoSend", AutoSend);
            return jobject;
        }
    }

    public class HotkeyObjectAction : HotkeyAction
    {
        public Core.Appearances.AppearanceType AppearanceType { get; set; }
        public UseActionTarget ActionTarget { get; set; }

        public HotkeyObjectAction(ushort objectId, UseActionTarget actionTarget)
            : this(OpenTibiaUnity.AppearanceStorage.GetObjectType(objectId), actionTarget) { }

        public HotkeyObjectAction(Core.Appearances.ObjectInstance @object, UseActionTarget actionTarget)
           : this(@object?.Type, actionTarget) { }

        public HotkeyObjectAction(Core.Appearances.AppearanceType appearanceType, UseActionTarget actionTarget) {
            if (!appearanceType)
                throw new System.ArgumentNullException("HotkeyObjectAction.HotkeyObjectAction: invalid appearance type");

            if (appearanceType.Category != AppearanceCategory.Object)
                throw new System.Exception("HotkeyObjectAction.HotkeyObjectAction: invalid appearance type.");

            AppearanceType = appearanceType;
            if (AppearanceType.IsMultiUse && actionTarget == UseActionTarget.Auto)
                actionTarget = UseActionTarget.CrossHair;
            else if (!AppearanceType.IsMultiUse && actionTarget != UseActionTarget.Auto)
                actionTarget = UseActionTarget.Auto;

            ActionTarget = actionTarget;
        }

        public override void Apply() {
            var absolutePosition = new UnityEngine.Vector3Int(65535, 0, 0);
            if (ActionTarget == UseActionTarget.CrossHair || (ActionTarget == UseActionTarget.Target && !OpenTibiaUnity.CreatureStorage.AttackTarget)) {
                var @object = new Core.Appearances.ObjectInstance(AppearanceType.Id, AppearanceType, 0);
                Core.Game.ObjectMultiUseHandler.Activate(absolutePosition, @object, 0);
                return;
            }

            // this is guaranteed to only be "self/target"
            // if the item is not multiuse (i.e a rune that can only be used on yourself, it will be only used)
            new Core.Input.GameAction.UseActionImpl(absolutePosition, AppearanceType, 0, UnityEngine.Vector3Int.zero, null, 0, ActionTarget).Perform();
        }

        public override JObject Serialize() {
            var jobject = new JObject();
            jobject.Add("type", "use");
            jobject.Add("objectId", AppearanceType.Id);
            switch (ActionTarget) {
                case UseActionTarget.Self: jobject.Add("useTarget", "self"); break;
                case UseActionTarget.Target: jobject.Add("useTarget", "target"); break;
                case UseActionTarget.CrossHair: jobject.Add("useTarget", "crosshair"); break;
                case UseActionTarget.Auto: jobject.Add("useTarget", "none"); break;
            }
            return jobject;
        }
    }

    public class HotkeyEquipAction : HotkeyAction
    {
        public override void Apply() {
            throw new System.NotImplementedException();
        }

        public override JObject Serialize() {
            throw new System.NotImplementedException();
        }
    }
}
