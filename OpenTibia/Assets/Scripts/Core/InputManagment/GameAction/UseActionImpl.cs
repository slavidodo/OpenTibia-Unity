using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Core.InputManagment.GameAction
{
    public class UseActionImpl : IActionImpl
    {
        public static UseActionImpl ConcurrentMultiUse = null;

        Vector3Int m_AbsolutePosition;
        Appearances.AppearanceType m_AppearanceType;
        int m_PositionOrData;
        UseActionTarget m_UseActionTarget;

        public UseActionImpl(Vector3Int absolutePosition, Appearances.ObjectInstance objectInstance, int positionOrData, UseActionTarget useTarget) {
            Init(absolutePosition, objectInstance?.Type, positionOrData, useTarget);
        }

        public UseActionImpl(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int positionOrData, UseActionTarget useTarget) {
            Init(absolutePosition, appearanceType, positionOrData, useTarget);
        }

        public UseActionImpl(Vector3Int absolutePosition, uint objectID, int positionOrData, UseActionTarget useTarget) {
            var appearnceType = OpenTibiaUnity.AppearanceStorage.GetObjectType(objectID);
            Init(absolutePosition, appearnceType, positionOrData, useTarget);
        }
        
        protected void Init(Vector3Int absolutePosition, Appearances.AppearanceType appearanceType, int positionOrData, UseActionTarget useTarget) {
            m_AppearanceType = appearanceType;
            if (!m_AppearanceType)
                throw new System.ArgumentException("UseActionImpl.UseActionImpl: Invalid type: " + appearanceType);

            m_AbsolutePosition = absolutePosition;
            if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y == 0)
                m_PositionOrData = positionOrData;
            else if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y != 0)
                m_PositionOrData = m_AbsolutePosition.z;
            else
                m_PositionOrData = positionOrData;

            m_UseActionTarget = useTarget;
        }

        public void Perform(bool repeat = false) {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            if (!protocolGame || !protocolGame.IsGameRunning)
                return;

            // obtain storages
            var worldStorage = OpenTibiaUnity.WorldMapStorage;
            var creatureStorage = OpenTibiaUnity.CreatureStorage;
            var containerStorage = OpenTibiaUnity.ContainerStorage;
            var player = OpenTibiaUnity.Player;

            if (m_AbsolutePosition.x == 65535 && m_AbsolutePosition.y == 0) {
                if (containerStorage.GetAvailableInventory(m_AppearanceType.ID, m_PositionOrData) < 1)
                    return;
            
                if (m_AppearanceType.IsMultiUse) {
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
                
                protocolGame.SendUseObject(m_AbsolutePosition, m_AppearanceType.ID, m_PositionOrData, index);
            } else if (!m_AppearanceType.IsMultiUse) {
                protocolGame.SendUseObject(m_AbsolutePosition, m_AppearanceType.ID, m_PositionOrData, 0);
            } else if (m_UseActionTarget == UseActionTarget.Self) {
                protocolGame.SendUseOnCreature(m_AbsolutePosition, m_AppearanceType.ID, m_PositionOrData, player.ID);
            } else if (m_UseActionTarget == UseActionTarget.Attack && creatureStorage.AttackTarget != null) {
                protocolGame.SendUseOnCreature(m_AbsolutePosition, m_AppearanceType.ID, m_PositionOrData, creatureStorage.AttackTarget.ID);
            } else {
                if (m_AbsolutePosition.x < 65535)
                    player.StartAutowalk(m_AbsolutePosition, false, false);

                // TODO: Concurrent Use (MultiUse)

                if (ConcurrentMultiUse != null) {
                    ConcurrentMultiUse.UpdateGlobalListeners(false);
                    ConcurrentMultiUse.UpdateCursor(false);
                    ConcurrentMultiUse = null;
                }

                UpdateGlobalListeners(true);
                UpdateCursor(true);
                ConcurrentMultiUse = this;
            }
        }

        private void OnUsePerform(Event e, MouseButtons mouseButton, bool repeat) {
            if ((mouseButton & MouseButtons.Left) == 0)
                return;
            
            UpdateGlobalListeners(false);
            UpdateCursor(false);
            ConcurrentMultiUse = null;
            
            var pointerEventData = new PointerEventData(OpenTibiaUnity.EventSystem);
            List<RaycastResult> results = new List<RaycastResult>();

            // we must use the active raycaster to make sure that if the game
            // is not running, we catch nothing in terms of (IWidgetContainerWidget)
            OpenTibiaUnity.ActiveRaycaster.Raycast(pointerEventData, results);
            foreach (var result in results) {
                Debug.Log(result.gameObject.name + ": " + result.distance);
            }
        }

        private void OnUseAbort(Event e, MouseButtons mouseButton, bool repeat) {
            if ((mouseButton & MouseButtons.Right) == 0)
                return;

            UpdateGlobalListeners(false);
            UpdateCursor(false);
            ConcurrentMultiUse = null;
        }
        
        private void UpdateGlobalListeners(bool add) {
            var inputManager = OpenTibiaUnity.InputHandler;
            if (add) {
                inputManager.AddMouseUpListener(Utility.EventImplPriority.High, OnUsePerform);
                inputManager.AddMouseDownListener(Utility.EventImplPriority.High, OnUseAbort);
            } else {
                inputManager.RemoveMouseUpListener(OnUsePerform);
                inputManager.RemoveMouseDownListener(OnUseAbort);
            }
        }

        private void UpdateCursor(bool activate) {
            if (activate)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Crosshair, CursorPriority.High);
            else
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(CursorState.Default, CursorPriority.Low);
        }
    }
}
