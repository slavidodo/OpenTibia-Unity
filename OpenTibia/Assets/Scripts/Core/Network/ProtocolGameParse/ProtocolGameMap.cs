namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {
        private UnityEngine.Vector3Int m_LastSnapback = UnityEngine.Vector3Int.zero;
        private int m_SnapbackCount = 0;

        private void ParseFullMap(InputMessage message) {
            UnityEngine.Vector3Int position = message.GetPosition();

            m_Player.StopAutowalk(true);
            m_CreatureStorage.MarkAllOpponentsVisible(false);
            m_MiniMapStorage.Position = position;
            m_WorldMapStorage.ResetMap();
            m_WorldMapStorage.InvalidateOnscreenMessages();
            m_WorldMapStorage.Position = position;

            ReadArea(message, 0, 0, Constants.MapSizeX - 1, Constants.MapSizeY - 1);
            m_WorldMapStorage.Valid = true;
        }

        private void ParseMapTopRow(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.y--;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;
            m_WorldMapStorage.ScrollMap(0, 1);
            m_WorldMapStorage.InvalidateOnscreenMessages();
            ReadArea(message, 0, 0, Constants.MapSizeX - 1, 0);
        }

        private void ParseMapRightRow(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.x++;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;
            m_WorldMapStorage.ScrollMap(-1, 0);
            m_WorldMapStorage.InvalidateOnscreenMessages();
            ReadArea(message, Constants.MapSizeX - 1, 0, Constants.MapSizeX - 1, Constants.MapSizeY - 1);
        }

        private void ParseMapBottomRow(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.y++;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;
            m_WorldMapStorage.ScrollMap(0, -1);
            m_WorldMapStorage.InvalidateOnscreenMessages();
            ReadArea(message, 0, Constants.MapSizeY - 1, Constants.MapSizeX - 1, Constants.MapSizeY - 1);
        }

        private void ParseMapLeftRow(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.x--;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;
            m_WorldMapStorage.ScrollMap(1, 0);
            m_WorldMapStorage.InvalidateOnscreenMessages();
            ReadArea(message, 0, 0, 0, Constants.MapSizeY - 1);
        }

        private void ParseFieldData(InputMessage message) {
            UnityEngine.Vector3Int absolutePosition = message.GetPosition();
            if (!m_WorldMapStorage.IsVisible(absolutePosition, true))
                throw new System.Exception("ProtocolGame.ParseFieldData: Co-ordinate " + absolutePosition + " is out of range.");

            var mapPosition = m_WorldMapStorage.ToMap(absolutePosition);
            m_WorldMapStorage.ResetField(mapPosition, true, false);
            ReadField(message, mapPosition.x, mapPosition.y, mapPosition.z);

            if (absolutePosition.z == m_MiniMapStorage.PositionZ) {
                m_WorldMapStorage.UpdateMiniMap(mapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
            }
        }

        private void ParseCreateOnMap(InputMessage message) {
            UnityEngine.Vector3Int absolutePosition = message.GetPosition();
            if (!m_WorldMapStorage.IsVisible(absolutePosition, true))
                throw new System.Exception("ProtocolGame.ParseCreateOnMap: Co-ordinate " + absolutePosition + " is out of range.");

            UnityEngine.Vector3Int mapPosition = m_WorldMapStorage.ToMap(absolutePosition);
            int stackPos = 255;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 841)
                stackPos = message.GetU8();

            int typeOrId = message.GetU16();

            Appearances.ObjectInstance obj;
            if (typeOrId == Appearances.AppearanceInstance.Creature || typeOrId == Appearances.AppearanceInstance.OutdatedCreature || typeOrId == Appearances.AppearanceInstance.UnknownCreature) {
                Creatures.Creature creature = ReadCreatureInstance(message, typeOrId, absolutePosition);
                if (creature.ID == m_Player.ID) {
                    m_Player.StopAutowalk(true);
                }

                obj = m_AppearanceStorage.CreateObjectInstance(Appearances.AppearanceInstance.Creature, creature.ID);
            } else {
                obj = ReadObjectInstance(message, typeOrId);
            }

            if (stackPos == 255) {
                m_WorldMapStorage.PutObject(mapPosition, obj);
            } else {
                if (stackPos > Constants.MapSizeW)
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Invalid stack position (" + stackPos + ").");

                m_WorldMapStorage.InsertObject(mapPosition, stackPos, obj);
            }

            if (absolutePosition.z == m_MiniMapStorage.PositionZ) {
                m_WorldMapStorage.UpdateMiniMap(mapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
            }
        }

        private void ParseChangeOnMap(InputMessage message) {
            int x = message.GetU16();
            Appearances.ObjectInstance objectInstance;
            Creatures.Creature creature = null;

            UnityEngine.Vector3Int absolutePosition;
            UnityEngine.Vector3Int mapPosition;

            if (x != 65535) {
                absolutePosition = message.GetPosition(x);
                if (!m_WorldMapStorage.IsVisible(absolutePosition, true))
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Co-ordinate " + absolutePosition + " is out of range.");

               mapPosition = m_WorldMapStorage.ToMap(absolutePosition);
                int stackPos = message.GetU8();
                if (!(objectInstance = m_WorldMapStorage.GetObject(mapPosition, stackPos)))
                    throw new System.Exception("ProtocolGame.ParseChangeOnMap: Object not found.");

                if (objectInstance.IsCreature && !(creature = m_CreatureStorage.GetCreature(objectInstance.Data)))
                    throw new System.Exception("ProtocolGame.ParseChangeOnMap: Creature not found: " + objectInstance.Data);

                if (!!creature)
                    m_CreatureStorage.MarkOpponentVisible(creature, false);

                int typeOrId = message.GetU16();
                if (typeOrId == Appearances.AppearanceInstance.UnknownCreature
                        || typeOrId == Appearances.AppearanceInstance.OutdatedCreature
                        || typeOrId == Appearances.AppearanceInstance.Creature) {
                    creature = ReadCreatureInstance(message, typeOrId, absolutePosition);
                    objectInstance = m_AppearanceStorage.CreateObjectInstance(Appearances.AppearanceInstance.Creature, creature.ID);
                } else {
                    objectInstance = ReadObjectInstance(message, typeOrId);
                }

                m_WorldMapStorage.ChangeObject(mapPosition, stackPos, objectInstance);
            } else {
                uint creatureID = message.GetU32();
                if (!(creature = m_CreatureStorage.GetCreature(creatureID)))
                    throw new System.Exception("ProtocolGame.ParseChangeOnMap: Creature " + creatureID + " not found");

                absolutePosition = creature.Position;
                if (!m_WorldMapStorage.IsVisible(absolutePosition, true))
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Co-ordinate " + absolutePosition + " is out of range.");

                mapPosition = m_WorldMapStorage.ToMap(absolutePosition);
                m_CreatureStorage.MarkOpponentVisible(creature, false);

                int otherType = message.GetU16();
                if (otherType == Appearances.AppearanceInstance.Creature || otherType == Appearances.AppearanceInstance.OutdatedCreature
                    || otherType == Appearances.AppearanceInstance.UnknownCreature) {
                    creature = ReadCreatureInstance(message, otherType);
                } else {
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Received object of type " + otherType + " when a creature was expected.");
                }
            }

            if (absolutePosition.z == m_MiniMapStorage.PositionZ) {
                m_WorldMapStorage.UpdateMiniMap(mapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
            }
        }

        private void ParseDeleteOnMap(InputMessage message) {
            int x = message.GetU16();

            Appearances.ObjectInstance objectInstance;
            Creatures.Creature creature = null;

            UnityEngine.Vector3Int absolutePosition;
            UnityEngine.Vector3Int mapPosition;

            if (x != 65535) {
                absolutePosition = message.GetPosition(x);

                if (!m_WorldMapStorage.IsVisible(absolutePosition, true)) {
                    throw new System.Exception($"ProtocolGame.ParseDeleteOnMap: Co-oridnate ({absolutePosition.x}, {absolutePosition.y}, {absolutePosition.z}) is out of range.");
                }

                mapPosition = m_WorldMapStorage.ToMap(absolutePosition);

                int stackPos = message.GetU8();
                if (!(objectInstance = m_WorldMapStorage.GetObject(mapPosition, stackPos))) {
                    throw new System.Exception($"ProtocolGame.ParseDeleteOnMap: Object not found.");
                }

                if (objectInstance.IsCreature && (creature = m_CreatureStorage.GetCreature(objectInstance.Data)) == null) {
                    throw new System.Exception($"ProtocolGame.ParseDeleteOnMap: Creature not found.");
                }

                m_WorldMapStorage.DeleteObject(mapPosition, stackPos);
            } else {
                uint creatureID = message.GetU32();
                if ((creature = m_CreatureStorage.GetCreature(creatureID)) == null) {
                    throw new System.Exception($"ProtocolGame.ParseDeleteOnMap: Object not found.");
                }

                absolutePosition = creature.Position;
                if (!m_WorldMapStorage.IsVisible(absolutePosition, true)) {
                    throw new System.Exception($"ProtocolGame.ParseDeleteOnMap: Co-oridnate ({absolutePosition.x}, {absolutePosition.y}, {absolutePosition.z}) is out of range.");
                }

                mapPosition = m_WorldMapStorage.ToMap(absolutePosition);
            }

            if (!!creature) {
                m_CreatureStorage.MarkOpponentVisible(creature, false);
            }

            if (absolutePosition.z == m_MiniMapStorage.Position.z) {
                m_WorldMapStorage.UpdateMiniMap(mapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
            }
        }

        private void ParseCreatureMove(InputMessage message) {
            int x = message.GetU16();

            UnityEngine.Vector3Int oldAbsolutePosition;
            UnityEngine.Vector3Int oldMapPosition;
            int stackPos = -1;
            Appearances.ObjectInstance obj;
            Creatures.Creature creature;

            if (x != 65535) {
                oldAbsolutePosition = message.GetPosition(x);
                if (!m_WorldMapStorage.IsVisible(oldAbsolutePosition, true))
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Start Co-ordinate " + oldAbsolutePosition + " is out of range.");

                oldMapPosition = m_WorldMapStorage.ToMap(oldAbsolutePosition);
                stackPos = message.GetU8();
                obj = m_WorldMapStorage.GetObject(oldMapPosition, stackPos);
                if (!obj || !obj.IsCreature || !(creature = m_CreatureStorage.GetCreature(obj.Data)))
                    throw new System.Exception("ProtocolGame.ParseCreatureMove: no creature at position " + oldAbsolutePosition);
            } else {
                uint creatureID = message.GetU32();
                obj = m_AppearanceStorage.CreateObjectInstance(Appearances.AppearanceInstance.Creature, creatureID);
                if (!(creature = m_CreatureStorage.GetCreature(creatureID)))
                    throw new System.Exception("ProtocolGame.ParseCreatureMove: Creature " + creatureID + " not found");

                oldAbsolutePosition = creature.Position;
                if (!m_WorldMapStorage.IsVisible(oldAbsolutePosition, true))
                    throw new System.Exception("ProtocolGame.ParseCreateOnMap: Start Co-ordinate " + oldAbsolutePosition + " is out of range.");

                oldMapPosition = m_WorldMapStorage.ToMap(oldAbsolutePosition);
            }

            UnityEngine.Vector3Int newAbsolutePosition = message.GetPosition();
            if (!m_WorldMapStorage.IsVisible(newAbsolutePosition, true))
                throw new System.Exception("ProtocolGame.ParseCreateOnMap: Target Co-ordinate " + oldAbsolutePosition + " is out of range.");

            UnityEngine.Vector3Int newMapPosition = m_WorldMapStorage.ToMap(newAbsolutePosition);
            UnityEngine.Vector3Int delta = newMapPosition - oldMapPosition;

            // if the movement is not actually a move (usually he is teleported)
            bool pushMovement = delta.z != 0 || System.Math.Abs(delta.x) > 1 || System.Math.Abs(delta.y) > 1;
            Appearances.ObjectInstance otherObj = null;
            if (!pushMovement && (!(otherObj = m_WorldMapStorage.GetObject(newMapPosition, 0)) || !otherObj.Type || !otherObj.Type.IsGround))
                throw new System.Exception("ProtocolGame.ParseCreateOnMap: Target field " + newAbsolutePosition + " has no BANK.");

            if (x != 65535)
                m_WorldMapStorage.DeleteObject(oldMapPosition, stackPos);

            m_WorldMapStorage.PutObject(newMapPosition, obj);
            creature.Position = newAbsolutePosition;
            
            if (pushMovement) {
                if (creature.ID == m_Player.ID)
                    m_Player.StopAutowalk(true);

                if (delta.x > 0)
                    creature.Direction = Directions.East;
                else if (delta.x < 0)
                    creature.Direction = Directions.West;
                else if (delta.y < 0)
                    creature.Direction = Directions.North;
                else if (delta.y > 0)
                    creature.Direction = Directions.South;
                
                if (creature.ID != m_Player.ID)
                    creature.StopMovementAnimation();
            } else {
                creature.StartMovementAnimation(delta.x, delta.y, (int)otherObj.Type.GroundSpeed);
            }

            m_CreatureStorage.MarkOpponentVisible(creature, true);
            m_CreatureStorage.InvalidateOpponents();

            if (oldAbsolutePosition.z == m_MiniMapStorage.PositionZ) {
                m_WorldMapStorage.UpdateMiniMap(oldMapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(oldMapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(oldMapPosition);
                m_MiniMapStorage.UpdateField(oldAbsolutePosition, color, cost, false);
            }

            if (newAbsolutePosition.z == m_MiniMapStorage.PositionZ) {
                m_WorldMapStorage.UpdateMiniMap(newMapPosition);
                uint color = m_WorldMapStorage.GetMiniMapColour(newMapPosition);
                int cost = m_WorldMapStorage.GetMiniMapCost(newMapPosition);
                m_MiniMapStorage.UpdateField(newAbsolutePosition, color, cost, false);
            }
        }

        private void ParseCancelWalk(InputMessage message) {
            int direction = message.GetU8();

            var absolutePosition = m_Player.Position;
            if (absolutePosition == m_LastSnapback)
                m_SnapbackCount++;
            else
                m_SnapbackCount = 0;

            m_LastSnapback.Set(absolutePosition.x, absolutePosition.y, absolutePosition.z);
            if (m_SnapbackCount >= 16) {
                m_Player.StopAutowalk(true);
                m_CreatureStorage.SetAttackTarget(null, false);
                SendCancel();
                m_SnapbackCount = 0;
            }

            m_Player.AbortAutowalk((Directions)direction);
        }

        private void ParseMapTopFloor(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.x++; position.y++; position.z--;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;

            if (position.z > Constants.GroundLayer) {
                m_WorldMapStorage.ScrollMap(0, 0, -1);
                ReadFloor(message, 2 * Constants.UndergroundLayer, 0);
            } else if (position.z == Constants.GroundLayer) {
                m_WorldMapStorage.ScrollMap(0, 0, -(Constants.UndergroundLayer + 1));
                int skip = 0;
                for (int zposition = Constants.UndergroundLayer; zposition <= Constants.GroundLayer; zposition++) {
                    skip = ReadFloor(message, zposition, skip);
                }
            }

            m_Player.StopAutowalk(true);
            m_WorldMapStorage.InvalidateOnscreenMessages();

            UnityEngine.Vector3Int tmpPosition = m_WorldMapStorage.ToMap(position);
            
            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; x < Constants.MapSizeY; y++) {
                    tmpPosition.x = x;
                    tmpPosition.y = y;

                    UnityEngine.Vector3Int absolutePosition = m_WorldMapStorage.ToAbsolute(tmpPosition);
                    m_WorldMapStorage.UpdateMiniMap(tmpPosition);
                    uint color = m_WorldMapStorage.GetMiniMapColour(tmpPosition);
                    int cost = m_WorldMapStorage.GetMiniMapCost(tmpPosition);
                    m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                }
            }
        }

        private void ParseMapBottomFloor(InputMessage message) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;
            position.x--; position.y--; position.z++;

            m_WorldMapStorage.Position = position;
            m_MiniMapStorage.Position = position;

            if (position.z > Constants.GroundLayer + 1) {
                m_WorldMapStorage.ScrollMap(0, 0, 1);
                if (position.z <= Constants.MapMaxZ - Constants.UndergroundLayer) {
                    ReadFloor(message, 2 * Constants.UndergroundLayer, 0);
                }
            } else if (position.z == Constants.GroundLayer + 1) {
                m_WorldMapStorage.ScrollMap(0, 0, Constants.UndergroundLayer + 1);
                int skip = 0;
                for (int zposition = Constants.UndergroundLayer; zposition >= 0; zposition--) {
                    skip = ReadFloor(message, zposition, skip);
                }
            }

            m_Player.StopAutowalk(true);
            m_WorldMapStorage.InvalidateOnscreenMessages();

            UnityEngine.Vector3Int tmpPosition = m_WorldMapStorage.ToMap(position);

            for (int x = 0; x < Constants.MapSizeX; x++) {
                for (int y = 0; x < Constants.MapSizeY; y++) {
                    tmpPosition.x = x;
                    tmpPosition.y = y;

                    UnityEngine.Vector3Int absolutePosition = m_WorldMapStorage.ToAbsolute(tmpPosition);
                    m_WorldMapStorage.UpdateMiniMap(tmpPosition);
                    uint color = m_WorldMapStorage.GetMiniMapColour(tmpPosition);
                    int cost = m_WorldMapStorage.GetMiniMapCost(tmpPosition);
                    m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                }
            }
        }

        private void ParseClearTarget(InputMessage message) {
            uint creatureID = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameAttackSeq))
                creatureID = message.GetU32();
            
            Creatures.Creature creature;
            if (!!(creature = m_CreatureStorage.AttackTarget) && (creature.ID == creatureID || creatureID == 0))
                m_CreatureStorage.SetAttackTarget(null, false);
            else if (!!(creature = m_CreatureStorage.FollowTarget) && (creature.ID == creatureID || creatureID == 0))
                m_CreatureStorage.SetFollowTarget(null, false);
        }
    }
}
