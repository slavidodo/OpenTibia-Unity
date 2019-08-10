using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using UnityEngine;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    internal static class GameActionFactory
    {
        internal static UseActionImpl CreateUseAction(Vector3Int absolutePosition, ObjectInstance objectInstance, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, objectInstance, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        internal static UseActionImpl CreateUseAction(Vector3Int absolutePosition, AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, appearanceType, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        internal static UseActionImpl CreateUseAction(Vector3Int absolutePosition, uint objectID, int stackPosOrData, Vector3Int targetAbsolute, ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            return new UseActionImpl(absolutePosition, objectID, stackPosOrData, targetAbsolute, targetObject, targetStackPosOrData, useTarget);
        }

        internal static MoveActionImpl CreateMoveAction(Vector3Int sourceAbsolute, ObjectInstance @object, int stackPos, Vector3Int destAbsolute, int moveAmount) {
            return new MoveActionImpl(sourceAbsolute, @object, stackPos, destAbsolute, moveAmount);
        }

        internal static AutowalkActionImpl CreateAutowalkAction(Vector3Int destination, bool diagonal, bool exact) {
            return new AutowalkActionImpl(destination, diagonal, exact);
        }

        internal static TalkActionImpl CreateTalkAction(string text, bool autoSend) {
            return new TalkActionImpl(text, autoSend);
        }

        internal static GreetAction CreateGreetAction(Creature npcCreature) {
            return new GreetAction(npcCreature);
        }

        internal static ToggleAttackTargetActionImpl CreateToggleAttackTargetAction(Creature creature, bool send) {
            return new ToggleAttackTargetActionImpl(creature, send);
        }
    }
}
