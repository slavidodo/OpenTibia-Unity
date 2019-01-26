namespace OpenTibiaUnity.Core.InputManagment
{
    public class MouseActionHelper
    {
        public static AppearanceActions ResolveActionForAppearanceOrCreature(AppearanceActions action, object obj) {
            if (obj == null)
                throw new System.ArgumentNullException("MouseActionHelper.ResolveActionForAppearanceOrCreature: obj must be ObjectInstance or Creature");

            Appearances.ObjectInstance objectInstance = null;
            Creatures.Creature creature = null;
            if (obj is Appearances.ObjectInstance) {
                objectInstance = obj as Appearances.ObjectInstance;
                if (objectInstance.IsCreature)
                    creature = OpenTibiaUnity.CreatureStorage.GetCreature(objectInstance.Data);
            } else {
                creature = obj as Creatures.Creature;
            }

            if (action == AppearanceActions.SmartClick) {
                if (creature == OpenTibiaUnity.Player)
                    action = AppearanceActions.None;
                else if (!!objectInstance && objectInstance.Type.DefaultAction != 0)
                    action = (AppearanceActions)objectInstance.Type.DefaultAction;
                else if (!!creature)
                    action = AppearanceActions.AttackOrTalk;
                else if (!!objectInstance && objectInstance.Type.IsUsable)
                    action = AppearanceActions.UseOrOpen;
                else
                    action = AppearanceActions.Look;
            }

            if (action == AppearanceActions.UseOrOpen) {
                action = AppearanceActions.Use;
                if (!!objectInstance && objectInstance.Type.IsContainer)
                    action = AppearanceActions.Open;
            }

            if (action == AppearanceActions.AttackOrTalk) {
                action = AppearanceActions.Attack;
                if (!!creature && creature.IsNPC)
                    action = AppearanceActions.Talk;
            }

            if (action == AppearanceActions.AutoWalk && !!objectInstance && objectInstance.Type.DefaultAction == (int)AppearanceActions.AutoWalkHighlight)
                action = AppearanceActions.AutoWalkHighlight;

            return action;
        }
    }
}
