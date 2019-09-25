using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    public class UseActionImpl : IActionImpl
    {
        private Vector3Int _absolutePosition;
        private Appearances.AppearanceType _appearanceType;
        private int _stackPosOrData;
        private Vector3Int _targetAbsolutePosition;
        private Appearances.ObjectInstance _targetObject;
        private int _targetStackPosOrData;
        private UseActionTarget _useActionTarget;

        public UseActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            Init(absolutePosition, objectInstance?.Type, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }

        public UseActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            Init(absolutePosition, appearanceType, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }

        public UseActionImpl(Vector3Int absolutePosition, uint objectId, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectId);
            Init(absolutePosition, appearnceType, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }
        
        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            _appearanceType = appearanceType;
            if (!_appearanceType)
                throw new System.ArgumentException("UseActionImpl.UseActionImpl: Invalid type: " + appearanceType);

            _absolutePosition = absolutePosition;
            if (_absolutePosition.x == 65535 && _absolutePosition.y == 0)
                _stackPosOrData = stackPosOrData;
            else if (_absolutePosition.x == 65535 && _absolutePosition.y != 0)
                _stackPosOrData = _absolutePosition.z;
            else
                _stackPosOrData = stackPosOrData;

            _targetObject = targetObject;
            _targetAbsolutePosition = targetAbsolute;
            if (_targetAbsolutePosition.x == 65535 && _targetAbsolutePosition.y == 0)
                _targetStackPosOrData = targetStackPosOrData;
            else if (_targetAbsolutePosition.x == 65535 && _targetAbsolutePosition.y != 0)
                _targetStackPosOrData = _targetAbsolutePosition.z;
            else
                _targetStackPosOrData = targetStackPosOrData;

            _useActionTarget = useTarget;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;
            
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            var player = OpenTibiaUnity.Player;

            // aimbot check!
            if (_absolutePosition.x == 65535 && _absolutePosition.y == 0) {
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameEquipHotkey)) {
                    if (containerStorage.GetAvailableInventory(_appearanceType.Id, _stackPosOrData) < 1)
                        return;
                }

                if (_appearanceType.IsMultiUse) {
                    // todo verify what version the client receives profession details (basic data)
                    var rune = Magic.SpellStorage.GetRune((int)_appearanceType.Id);
                    if (rune != null && player.GetRuneUses(rune) < 1)
                        return;
                }
            }

            if (_appearanceType.IsContainer) {
                int index = 0;
                if (_useActionTarget == UseActionTarget.NewWindow || _absolutePosition.x < 65535 || _absolutePosition.y >= (int)ClothSlots.First && _absolutePosition.y <= (int)ClothSlots.Last)
                    index = containerStorage.GetFreeContainerViewId();
                else if (64 <= _absolutePosition.y && _absolutePosition.y < 64 + Constants.MaxContainerViews)
                    index = _absolutePosition.y - 64;

                protocolGame.SendUseObject(_absolutePosition, _appearanceType.Id, _stackPosOrData, index);
            } else if (!_appearanceType.IsMultiUse) {
                protocolGame.SendUseObject(_absolutePosition, _appearanceType.Id, _stackPosOrData, 0);
            } else if (_useActionTarget == UseActionTarget.Self) {
                protocolGame.SendUseOnCreature(_absolutePosition, _appearanceType.Id, _stackPosOrData, player.Id);
            } else if (_useActionTarget == UseActionTarget.Target && creatureStorage.AttackTarget != null) {
                protocolGame.SendUseOnCreature(_absolutePosition, _appearanceType.Id, _stackPosOrData, creatureStorage.AttackTarget.Id);
            } else {
                //if (_absolutePosition.x < 65535)
                //    GameActionFactory.CreateAutowalkAction(_absolutePosition, false, false).Perform();

                if (_targetObject.Id == Appearances.AppearanceInstance.Creature)
                    protocolGame.SendUseOnCreature(_absolutePosition, _appearanceType.Id, _stackPosOrData, _targetObject.Data);
                else
                    protocolGame.SendUseTwoObjects(_absolutePosition, _appearanceType.Id, _stackPosOrData, _targetAbsolutePosition, _targetObject.Id, _targetStackPosOrData);
            }
        }
    }
}
