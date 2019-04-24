
namespace OpenTibiaUnity.Core.Network
{
    using AppearanceInstance = Appearances.AppearanceInstance;
    using ObjectInstance = Appearances.ObjectInstance;
    using OutfitInstance = Appearances.OutfitInstance;

    public sealed partial class ProtocolGame
    {
        private AppearanceInstance ReadCreatureOutfit(InputMessage message, AppearanceInstance instance = null) {
            int lookType;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameLooktypeU16))
                lookType = message.GetU16();
            else
                lookType = message.GetU8();

            if (lookType != 0) {
                int head = message.GetU8();
                int body = message.GetU8();
                int legs = message.GetU8();
                int feet = message.GetU8();

                int addons = 0;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerAddons))
                    addons = message.GetU8();

                OutfitInstance outfitInstance = instance as OutfitInstance;
                if (!!outfitInstance) {
                    outfitInstance.UpdateProperties(head, body, legs, feet, addons);
                    return instance;
                }

                return m_AppearanceStorage.CreateOutfitInstance((uint)lookType, head, body, legs, feet, addons);
            }

            int lookTypeEx = message.GetU16();

            ObjectInstance objectInstance = instance as ObjectInstance;
            if (!!objectInstance && objectInstance.ID == (uint)lookTypeEx) {
                return objectInstance;
            }

            if (lookTypeEx == 0)
                return m_AppearanceStorage.CreateOutfitInstance(OutfitInstance.InvisibleOutfitID, 0, 0, 0, 0, 0);

            return m_AppearanceStorage.CreateObjectInstance((uint)lookTypeEx, 0);
        }

        private AppearanceInstance ReadMountOutfit(InputMessage message, AppearanceInstance instance = null) {
            uint lookType = message.GetU16();

            OutfitInstance outfitInstance = instance as OutfitInstance;
            if (!!outfitInstance && outfitInstance.ID == lookType)
                return outfitInstance;

            if (lookType != 0)
                return m_AppearanceStorage.CreateOutfitInstance(lookType, 0, 0, 0, 0, 0);

            return null;
        }
        
        private Creatures.Creature ReadCreatureInstance(InputMessage message, int type = -1,
                    UnityEngine.Vector3Int? absolutePosition = null) {

            if (type == -1)
                type = message.GetU16();

            if (type != AppearanceInstance.Creature && type != AppearanceInstance.OutdatedCreature && type != AppearanceInstance.UnknownCreature)
                throw new System.Exception("ProtocolGame.ReadCreatureInstance: Invalid creature type");

            var gameManager = OpenTibiaUnity.GameManager;

            Creatures.Creature creature;
            switch (type) {
                case AppearanceInstance.UnknownCreature:
                case AppearanceInstance.OutdatedCreature:
                    if (type == AppearanceInstance.UnknownCreature) {
                        uint removeID = message.GetU32();
                        uint newID = message.GetU32();
                        int creatureType;

                        if (gameManager.ClientVersion >= 910) {
                            creatureType = message.GetU8();
                        } else {
                            if (newID >= Constants.PlayerStartID && newID < Constants.PlayerEndID)
                                creatureType = (int)CreatureTypes.Player;
                            else if (newID >= Constants.MonsterStartID && newID < Constants.MonsterEndID)
                                creatureType = (int)CreatureTypes.Monster;
                            else
                                creatureType = (int)CreatureTypes.NPC;
                        }

                        if (newID == m_Player.ID)
                            creature = m_Player;
                        else
                            creature = new Creatures.Creature(newID);

                        creature = m_CreatureStorage.ReplaceCreature(creature, removeID);
                        if (!creature)
                            throw new System.Exception("ProtocolGame.ReadCreatureInstance: Failed to append creature.");

                        creature.Type = (CreatureTypes)creatureType;

                        creature.Name = message.GetString();
                    } else {
                        creature = m_CreatureStorage.GetCreature(message.GetU32());
                        if (!creature)
                            throw new System.Exception("ProtocolGame.ReadCreatureInstance: Outdated creature not found.");
                    }

                    creature.SetSkill(SkillTypes.HealthPercent, message.GetU8());
                    creature.Direction = (Directions)message.GetU8();
                    creature.Outfit = ReadCreatureOutfit(message, creature.Outfit);
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerMounts))
                        creature.MountOutfit = ReadMountOutfit(message, creature.MountOutfit);
                    creature.Brightness = message.GetU8();
                    creature.LightColor = Colors.ColorFrom8Bit(message.GetU8());
                    creature.SetSkill(SkillTypes.Speed, message.GetU16());
                    creature.SetPKFlag((PKFlags)message.GetU8());
                    creature.SetPartyFlag((PartyFlags)message.GetU8());

                    if (gameManager.GetFeature(GameFeatures.GameCreatureEmblems) && type == AppearanceInstance.UnknownCreature)
                        creature.SetGuildFlag((GuildFlags)message.GetU8());

                    if (gameManager.GetFeature(GameFeatures.GameThingMarks))
                        creature.Type = (CreatureTypes)message.GetU8();

                    // TODO, idk what version this respects to //
                    //if (creature.IsSummon) {
                    //    creature.SetSummonerID(message.GetU32());
                    //} else {
                    //    creature.SetSummonerID(0);
                    //}

                    if (gameManager.GetFeature(GameFeatures.GameCreatureIcons))
                        creature.SetSpeechCategory((SpeechCategories)message.GetU8());

                    if (gameManager.GetFeature(GameFeatures.GameThingMarks)) {
                        creature.Marks.SetMark(MarkTypes.Permenant, message.GetU8());

                        // TODO insspection state
                        //message.GetU8(); // inspection state

                        creature.NumberOfPvPHelpers = message.GetU16();
                    }

                    if (gameManager.ClientVersion >= 854)
                        creature.Unpassable = message.GetU8() != 0;
                    else
                        creature.Unpassable = true;
                    break;
                case AppearanceInstance.Creature:
                    creature = m_CreatureStorage.GetCreature(message.GetU32());
                    if (!creature)
                        throw new System.Exception("ProtocolGame.ReadCreatureInstance: Known creature not found.");

                    creature.Direction = (Directions)message.GetU8();

                    if (gameManager.ClientVersion >= 953)
                        creature.Unpassable = message.GetU8() != 0;
                    else
                        creature.Unpassable = true;
                    break;

                default:
                    throw new System.Exception("ProtocolGame.ReadCreatureInstance: unknown creature identity type.");
            }

            if (absolutePosition.HasValue)
                creature.Position = absolutePosition.Value;

            m_CreatureStorage.MarkOpponentVisible(creature, true);
            m_CreatureStorage.InvalidateOpponents();
            return creature;
        }

        private ObjectInstance ReadObjectInstance(InputMessage message, int id = -1) {
            if (id == -1)
                id = message.GetU16();

            if (id == 0)
                return null;
            else if (id <= AppearanceInstance.Creature)
                throw new System.Exception("ProtocolGameUtility.ReadObjectInstance: Invalid type");

            ObjectInstance instance = m_AppearanceStorage.CreateObjectInstance((uint)id, 0);
            if (!instance)
                throw new System.Exception("ProtocolGameUtility.ReadObjectInstance: Invalid instance with id " + id);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameThingMarks))
                instance.Marks.SetMark(MarkTypes.Permenant, message.GetU8());

            if (instance.Type.IsStackable || instance.Type.IsFluidContainer || instance.Type.IsSplash)
                instance.Data = message.GetU8();

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameItemAnimationPhase)) {
                if (instance.Type.FrameGroups[(int)Proto.Appearances.FrameGroupType.Idle].IsAnimation) {
                    int phase = message.GetU8();
                    instance.Phase = phase == 0 ? Constants.PhaseAutomatic : phase;
                }
            }

            return instance;
        }

        private int ReadField(InputMessage message, int x, int y, int z) {
            UnityEngine.Vector3Int position = new UnityEngine.Vector3Int() {
                x = x,
                y = y,
                z = z
            };

            UnityEngine.Vector3Int absolutePosition = m_WorldMapStorage.ToAbsolute(position);

            int typeOrId;
            int stackPos = 0;
            bool effect = false;

            while (true) {
                typeOrId = message.GetU16();
                if (typeOrId >= 65280)
                    break;

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameEnvironmentEffect) && !effect) {
                    ObjectInstance tmpInstance = m_AppearanceStorage.CreateEnvironmentalEffect((uint)typeOrId);
                    m_WorldMapStorage.SetEnvironmentalEffect(position, tmpInstance);
                    effect = true;
                    continue;
                }
                
                if (typeOrId == AppearanceInstance.UnknownCreature || typeOrId == AppearanceInstance.OutdatedCreature
                            || typeOrId == AppearanceInstance.Creature) {
                    Creatures.Creature creature = ReadCreatureInstance(message, typeOrId, absolutePosition);

                    ObjectInstance tmpInstance = m_AppearanceStorage.CreateObjectInstance(AppearanceInstance.Creature, creature.ID);
                    if (stackPos < Constants.MapSizeW)
                        m_WorldMapStorage.AppendObject(position, tmpInstance);
                } else {
                    ObjectInstance tmpInstance = ReadObjectInstance(message, typeOrId);
                    if (stackPos < Constants.MapSizeW)
                        m_WorldMapStorage.AppendObject(position, tmpInstance);
                    else
                        throw new System.Exception("ProtocolGameUtility.ReadField: Expected creatures but received regular object.");
                }

                stackPos++;
            }

            return typeOrId - 65280;
        }

        private int ReadArea(InputMessage message, int startx, int starty, int endx, int endy) {
            UnityEngine.Vector3Int position = m_WorldMapStorage.Position;

            int z, endz, zstep;
            if (position.z <= Constants.GroundLayer) {
                z = 0;
                endz = Constants.GroundLayer + 1;
                zstep = 1;
            } else {
                z = 2 * Constants.UndergroundLayer;
                endz = System.Math.Max(-1, position.z - Constants.MapMaxZ + 1);
                zstep = -1;
            }
            
            int skip = 0;
            for (; z != endz; z += zstep) {
                for (int x = startx; x <= endx; x++) {
                    for (int y = starty;  y <= endy; y++) {
                        if (skip > 0) {
                            skip--;
                        } else {
                            skip = ReadField(message, x, y, z);
                        }
                        
                        UnityEngine.Vector3Int mapPosition = new UnityEngine.Vector3Int(x, y, z);
                        UnityEngine.Vector3Int absolutePosition = m_WorldMapStorage.ToAbsolute(mapPosition);
                        
                        if (absolutePosition.z == m_MiniMapStorage.PositionZ) {
                            m_WorldMapStorage.UpdateMiniMap(mapPosition);
                            uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                            int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                            m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                        }
                    }
                }
            }

            return skip;
        }

        private int ReadFloor(InputMessage message, int z, int skip) {
            for (int x = 0; x <= Constants.MapSizeX - 1; x++) {
                for (int y = 0; y <= Constants.MapSizeY - 1; y++) {
                    if (skip > 0) {
                        skip--;
                    } else {
                        skip = ReadField(message, x, y, z);
                    }

                    UnityEngine.Vector3Int mapPosition = new UnityEngine.Vector3Int(x, y, z);
                    UnityEngine.Vector3Int absolutePosition = m_WorldMapStorage.ToAbsolute(mapPosition);

                    if (absolutePosition.z == m_MiniMapStorage.PositionZ) {
                        m_WorldMapStorage.UpdateMiniMap(mapPosition);
                        uint color = m_WorldMapStorage.GetMiniMapColour(mapPosition);
                        int cost = m_WorldMapStorage.GetMiniMapCost(mapPosition);
                        m_MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                    }
                }
            }

            return skip;
        }
    }
}
