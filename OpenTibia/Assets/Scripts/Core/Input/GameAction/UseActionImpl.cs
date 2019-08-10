using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.Input.GameAction
{
    internal class UseActionImpl : IActionImpl
    {
        private Vector3Int m_AbsolutePosition;
        private Appearances.AppearanceType m_AppearanceType;
        private int m_StackPosOrData;
        private Vector3Int m_TargetAbsolutePosition;
        private Appearances.ObjectInstance m_TargetObject;
        private int m_TargetStackPosOrData;
        private UseActionTarget m_UseActionTarget;

        internal UseActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            Init(absolutePosition, objectInstance?.Type, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }

        internal UseActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            Init(absolutePosition, appearanceType, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }

        internal UseActionImpl(Vector3Int absolutePosition, uint objectID, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetPosition, UseActionTarget useTarget) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectID);
            Init(absolutePosition, appearnceType, stackPosOrData, targetAbsolute, targetObject, targetPosition, useTarget);
        }
        
        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int stackPosOrData, Vector3Int targetAbsolute, Appearances.ObjectInstance targetObject, int targetStackPosOrData, UseActionTarget useTarget) {
            m_AppearanceType = appearanceType;
            if (!m_AppearanceType)
                throw new System.ArgumentException("UseActionImpl.UseActionImpl: Invalid type: " + appearanceType);

            m_AbsolutePosition = absolutePosition;
            if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y == 0)
                m_StackPosOrData = stackPosOrData;
            else if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y != 0)
                m_StackPosOrData = m_AbsolutePosition.z;
            else
                m_StackPosOrData = stackPosOrData;

            m_TargetObject = targetObject;
            m_TargetAbsolutePosition = absolutePosition;
            if (m_TargetAbsolutePosition.x == 65535 && m_TargetAbsolutePosition.y == 0)
                m_TargetStackPosOrData = targetStackPosOrData;
            else if (m_TargetAbsolutePosition.x == 65535 && m_TargetAbsolutePosition.y != 0)
                m_TargetStackPosOrData = m_TargetAbsolutePosition.z;
            else
                m_TargetStackPosOrData = targetStackPosOrData;

            m_UseActionTarget = useTarget;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;
            
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            var player = OpenTibiaUnity.Player;

            // aimbot check!
            if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y == 0) {
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameEquipHotkey)) {
                    if (containerStorage.GetAvailableInventory(m_AppearanceType.ID, m_StackPosOrData) < 1)
                        return;
                }

                if (m_AppearanceType.IsMultiUse) {
                    // todo verify what version the client receives profession details (basic data)
                    var rune = Magic.SpellStorage.GetRune((int)m_AppearanceType.ID);
                    if (rune != null && player.GetRuneUses(rune) < 1)
                        return;
                }
            }

            if (m_AppearanceType.IsContainer) {
                int index = 0;
                if (m_UseActionTarget == UseActionTarget.NewWindow || m_AbsolutePosition.x < 65535 || m_AbsolutePosition.y >= (int)ClothSlots.First && m_AbsolutePosition.y <= (int)ClothSlots.Last)
                    index = containerStorage.GetFreeContainerViewID();
                else if (64 <= m_AbsolutePosition.y && m_AbsolutePosition.y < 64 + Constants.MaxContainerViews)
                    index = m_AbsolutePosition.y - 64;

                protocolGame.SendUseObject(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, index);
            } else if (!m_AppearanceType.IsMultiUse) {
                protocolGame.SendUseObject(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, 0);
            } else if (m_UseActionTarget == UseActionTarget.Self) {
                protocolGame.SendUseOnCreature(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, player.ID);
            } else if (m_UseActionTarget == UseActionTarget.Target && creatureStorage.AttackTarget != null) {
                protocolGame.SendUseOnCreature(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, creatureStorage.AttackTarget.ID);
            } else {
                if (m_AbsolutePosition.x < 65535)
                    GameActionFactory.CreateAutowalkAction(m_AbsolutePosition, false, false).Perform();

                if (m_TargetObject.ID == Appearances.AppearanceInstance.Creature)
                    protocolGame.SendUseOnCreature(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, m_TargetObject.Data);
                else
                    protocolGame.SendUseTwoObjects(m_AbsolutePosition, m_AppearanceType.ID, m_StackPosOrData, m_TargetAbsolutePosition, m_TargetObject.ID, m_TargetStackPosOrData);
            }
        }
    }
}
