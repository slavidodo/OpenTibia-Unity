using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Communication.Internal;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.MiniMap;
using OpenTibiaUnity.Core.WorldMap;
using UnityEngine;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public static class ProtocolGameExtentions
    {
        private static AppearanceStorage AppearanceStorage {
            get => OpenTibiaUnity.AppearanceStorage;
        }
        private static CreatureStorage CreatureStorage {
            get => OpenTibiaUnity.CreatureStorage;
        }
        private static MiniMapStorage MiniMapStorage {
            get => OpenTibiaUnity.MiniMapStorage;
        }
        private static WorldMapStorage WorldMapStorage {
            get => OpenTibiaUnity.WorldMapStorage;
        }
        private static Player Player {
            get => OpenTibiaUnity.Player;
        }

        public static AppearanceInstance ReadCreatureOutfit(CommunicationStream message, AppearanceInstance instance = null) {
            int outfitId;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameOutfitIdU16))
                outfitId = message.ReadUnsignedShort();
            else
                outfitId = message.ReadUnsignedByte();

            if (outfitId != 0) {
                int headColor = message.ReadUnsignedByte();
                int torsoColor = message.ReadUnsignedByte();
                int legsColor = message.ReadUnsignedByte();
                int detailColor = message.ReadUnsignedByte();

                int addonsFlags = 0;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerAddons))
                    addonsFlags = message.ReadUnsignedByte();

                var outfitInstance = instance as OutfitInstance;
                if (!!outfitInstance) {
                    outfitInstance.UpdateProperties(headColor, torsoColor, legsColor, detailColor, addonsFlags);
                    return instance;
                }

                return AppearanceStorage.CreateOutfitInstance((uint)outfitId, headColor, torsoColor, legsColor, detailColor, addonsFlags);
            }

            uint objectId = message.ReadUnsignedShort();
            var objectInstance = instance as ObjectInstance;
            if (!!objectInstance && objectInstance.Id == objectId)
                return objectInstance;

            if (objectId == OutfitInstance.InvisibleOutfitId) {
                var effect = AppearanceStorage.CreateInvisibleEffect();
                effect.SetEndless();
                return effect;
            }

            return AppearanceStorage.CreateObjectInstance(objectId, 0);
        }

        public static AppearanceInstance ReadMountOutfit(CommunicationStream message, AppearanceInstance instance = null) {
            uint outfitId = message.ReadUnsignedShort();

            var outfitInstance = instance as OutfitInstance;
            if (!!outfitInstance && outfitInstance.Id == outfitId)
                return outfitInstance;

            if (outfitId != 0)
                return AppearanceStorage.CreateOutfitInstance(outfitId, 0, 0, 0, 0, 0);

            return null;
        }

        public static Creature ReadCreatureInstance(CommunicationStream message, int type = -1, Vector3Int? absolutePosition = null) {
            if (type == -1)
                type = message.ReadUnsignedShort();

            if (type != AppearanceInstance.Creature && type != AppearanceInstance.OutdatedCreature && type != AppearanceInstance.UnknownCreature)
                throw new System.Exception("ProtocolGame.ReadCreatureInstance: Invalid creature type");

            var gameManager = OpenTibiaUnity.GameManager;

            Creature creature;
            switch (type) {
                case AppearanceInstance.UnknownCreature:
                case AppearanceInstance.OutdatedCreature: {
                    if (type == AppearanceInstance.UnknownCreature) {
                        uint removeId = message.ReadUnsignedInt();
                        uint newId = message.ReadUnsignedInt();
                        CreatureType creatureType;

                        if (gameManager.ClientVersion >= 910) {
                            creatureType = message.ReadEnum<CreatureType>();
                        } else {
                            if (newId >= Constants.PlayerStartId && newId < Constants.PlayerEndId)
                                creatureType = CreatureType.Player;
                            else if (newId >= Constants.MonsterStartId && newId < Constants.MonsterEndId)
                                creatureType = CreatureType.Monster;
                            else
                                creatureType = CreatureType.NPC;
                        }

                        if (newId == Player.Id)
                            creature = Player;
                        else
                            creature = new Creature(newId);

                        creature = CreatureStorage.ReplaceCreature(creature, removeId);
                        if (!creature)
                            throw new System.Exception("ProtocolGame.ReadCreatureInstance: Failed to append creature.");

                        creature.Type = creatureType;
                        if (gameManager.ClientVersion >= 1120)
                            creature.SetSummonerId(creature.IsSummon ? message.ReadUnsignedInt() : 0);

                        creature.Name = message.ReadString();
                    } else {
                        uint creatureId = message.ReadUnsignedInt();
                        creature = CreatureStorage.GetCreatureById(creatureId);
                        if (!creature)
                            throw new System.Exception("ProtocolGame.ReadCreatureInstance: Outdated creature not found.");
                    }

                    creature.SetSkill(SkillType.HealthPercent, message.ReadUnsignedByte());
                    creature.Direction = message.ReadEnum<Direction>();
                    creature.Outfit = ReadCreatureOutfit(message, creature.Outfit);
                    if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                        creature.MountOutfit = ReadMountOutfit(message, creature.MountOutfit);
                    creature.Brightness = message.ReadUnsignedByte();
                    creature.LightColor = Colors.ColorFrom8Bit(message.ReadUnsignedByte());
                    creature.SetSkill(SkillType.Speed, message.ReadUnsignedShort());
                    creature.SetPKFlag(message.ReadEnum<PkFlag>());
                    creature.SetPartyFlag(message.ReadEnum<PartyFlag>());

                    if (gameManager.GetFeature(GameFeature.GameCreatureEmblems) && type == AppearanceInstance.UnknownCreature)
                        creature.SetGuildFlag(message.ReadEnum<GuildFlag>());

                    if (gameManager.GetFeature(GameFeature.GameCreatureMarks)) {
                        // todo; at 11.20 SummonOther was removed
                        // back at 12.03 Hireling was added with the same enum number
                        // optimally we shouldn't allow any invalid enum value between
                        // these versions, so we could use separate enums then cast to
                        // to the preferred one
                        creature.Type = message.ReadEnum<CreatureType>();
                        if (gameManager.ClientVersion >= 1120)
                            creature.SetSummonerId(creature.IsSummon ? message.ReadUnsignedInt() : 0);
                    }

                    if (gameManager.ClientVersion >= 1220 && creature.Type == CreatureType.Player) {
                        byte unknown = message.ReadUnsignedByte(); // suggestion: boolean isFriend (friend system)
                    }

                    if (gameManager.GetFeature(GameFeature.GameCreatureIcons))
                        creature.SetSpeechCategory(message.ReadEnum<SpeechCategory>());

                    if (gameManager.GetFeature(GameFeature.GameCreatureMarks)) {
                        creature.Marks.SetMark(MarkType.Permenant, message.ReadUnsignedByte());

                        if (gameManager.GetFeature(GameFeature.GameInspectionWindow))
                            message.ReadUnsignedByte(); // inspection state

                        if (gameManager.ClientVersion < 1185)
                            creature.NumberOfPvPHelpers = message.ReadUnsignedShort();
                    }

                    if (gameManager.ClientVersion >= 854)
                        creature.Unpassable = message.ReadUnsignedByte() != 0;
                    else
                        creature.Unpassable = true;
                    break;
                }

                case AppearanceInstance.Creature: {
                    uint creatureId = message.ReadUnsignedInt();
                    creature = CreatureStorage.GetCreatureById(creatureId);
                    if (!creature)
                        throw new System.Exception(string.Format("ProtocolGame.ReadCreatureInstance: Known creature not found ({0}).", creatureId));

                    creature.Direction = message.ReadEnum<Direction>();

                    if (gameManager.ClientVersion >= 953)
                        creature.Unpassable = message.ReadUnsignedByte() != 0;
                    break;
                }

                default:
                    throw new System.Exception("ProtocolGame.ReadCreatureInstance: unknown creature identity type.");
            }

            if (absolutePosition.HasValue)
                creature.Position = absolutePosition.Value;

            CreatureStorage.MarkOpponentVisible(creature, true);
            return creature;
        }

        public static ObjectInstance ReadObjectInstance(CommunicationStream message, int id = -1) {
            if (id == -1)
                id = message.ReadUnsignedShort();

            if (id == 0)
                return null;
            else if (id <= AppearanceInstance.Creature)
                throw new System.Exception("ProtocolGameExtentions.ReadObjectInstance: Invalid type (id = " + id + ")");

            var @object = AppearanceStorage.CreateObjectInstance((uint)id, 0);
            if (!@object)
                throw new System.Exception("ProtocolGameExtentions.ReadObjectInstance: Invalid instance with id " + id);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameObjectMarks))
                @object.Marks.SetMark(MarkType.Permenant, message.ReadUnsignedByte());

            if (@object.Type.IsStackable || @object.Type.IsFluidContainer || @object.Type.IsSplash)
                @object.Data = message.ReadUnsignedByte();

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameItemAnimationPhase)) {
                if (@object.Type.FrameGroups[(int)Protobuf.Shared.FrameGroupType.Idle].SpriteInfo.IsAnimation) {
                    int phase = message.ReadUnsignedByte();
                    @object.Phase = phase == 0 ? Constants.PhaseAutomatic : phase;
                }
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameQuickLoot) && @object.Type.IsContainer) {
                bool assignedToQuickLoot = message.ReadBoolean();
                if (assignedToQuickLoot) {
                    uint lootContainers = message.ReadUnsignedInt(); // 1 << ObjectCategory | ....
                }
            }

            return @object;
        }

        public static int ReadField(CommunicationStream message, int x, int y, int z) {
            var mapPosition = new Vector3Int(x, y, z);
            var absolutePosition = WorldMapStorage.ToAbsolute(mapPosition);

            int typeOrId;
            int stackPos = 0;
            bool gotEffect = false;

            while (true) {
                typeOrId = message.ReadUnsignedShort();
                if (typeOrId >= 65280)
                    break;

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameEnvironmentEffect) && !gotEffect) {
                    var effectObject = AppearanceStorage.CreateEnvironmentalEffect((uint)typeOrId);
                    WorldMapStorage.SetEnvironmentalEffect(mapPosition, effectObject);
                    gotEffect = true;
                    continue;
                }

                if (typeOrId == AppearanceInstance.UnknownCreature || typeOrId == AppearanceInstance.OutdatedCreature
                            || typeOrId == AppearanceInstance.Creature) {
                    var creature = ReadCreatureInstance(message, typeOrId, absolutePosition);

                    var @object = AppearanceStorage.CreateObjectInstance(AppearanceInstance.Creature, creature.Id);
                    if (stackPos < Constants.MapSizeW)
                        WorldMapStorage.AppendObject(mapPosition, @object);
                } else {
                    var @object = ReadObjectInstance(message, typeOrId);
                    if (stackPos < Constants.MapSizeW)
                        WorldMapStorage.AppendObject(mapPosition, @object);
                    else
                        throw new System.Exception("ProtocolGameExtentions.ReadField: Expected creatures but received regular object.");
                }

                stackPos++;
            }

            return typeOrId - 65280;
        }

        public static int ReadArea(CommunicationStream message, int startx, int starty, int endx, int endy) {
            var position = WorldMapStorage.Position;

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
                    for (int y = starty; y <= endy; y++) {
                        if (skip > 0)
                            skip--;
                        else
                            skip = ReadField(message, x, y, z);

                        var mapPosition = new Vector3Int(x, y, z);
                        var absolutePosition = WorldMapStorage.ToAbsolute(mapPosition);

                        if (absolutePosition.z == MiniMapStorage.PositionZ) {
                            WorldMapStorage.UpdateMiniMap(mapPosition);
                            uint color = WorldMapStorage.GetMiniMapColour(mapPosition);
                            int cost = WorldMapStorage.GetMiniMapCost(mapPosition);
                            MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                        }
                    }
                }
            }

            return skip;
        }

        public static int ReadFloor(CommunicationStream message, int z, int skip) {
            for (int x = 0; x <= Constants.MapSizeX - 1; x++) {
                for (int y = 0; y <= Constants.MapSizeY - 1; y++) {
                    if (skip > 0)
                        skip--;
                    else
                        skip = ReadField(message, x, y, z);

                    var mapPosition = new Vector3Int(x, y, z);
                    var absolutePosition = WorldMapStorage.ToAbsolute(mapPosition);

                    if (absolutePosition.z == MiniMapStorage.PositionZ) {
                        WorldMapStorage.UpdateMiniMap(mapPosition);
                        uint color = WorldMapStorage.GetMiniMapColour(mapPosition);
                        int cost = WorldMapStorage.GetMiniMapCost(mapPosition);
                        MiniMapStorage.UpdateField(absolutePosition, color, cost, false);
                    }
                }
            }

            return skip;
        }
    }
}
