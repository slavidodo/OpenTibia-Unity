using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    public static class GameActionFactory
    {
        public static UseActionImpl CreateUseAction(Vector3Int absolutePosition, ObjectInstance objectInstance, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, objectInstance, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        public static UseActionImpl CreateUseAction(Vector3Int absolutePosition, AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, appearanceType, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        public static UseActionImpl CreateUseAction(Vector3Int absolutePosition, uint object_id, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, object_id, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        public static MoveActionImpl CreateMoveAction(Vector3Int sourceAbsolute, ObjectInstance @object, int stackPos, Vector3Int destAbsolute, int moveAmount) {
            return new MoveActionImpl(sourceAbsolute, @object, stackPos, destAbsolute, moveAmount);
        }

        public static AutowalkActionImpl CreateAutowalkAction(Vector3Int destination, bool diagonal, bool exact) {
            return new AutowalkActionImpl(destination, diagonal, exact);
        }

        public static TalkActionImpl CreateTalkAction(string text, bool autoSend) {
            return new TalkActionImpl(text, autoSend);
        }

        public static GreetAction CreateGreetAction(Creature npcCreature) {
            return new GreetAction(npcCreature);
        }

        public static ToggleAttackTargetActionImpl CreateToggleAttackTargetAction(Creature creature, bool send) {
            return new ToggleAttackTargetActionImpl(creature, send);
        }
    }
}
