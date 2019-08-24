
namespace OpenTibiaUnity.Core.Communication.Game
{
    using AppearanceInstance = Appearances.AppearanceInstance;
    using ObjectInstance = Appearances.ObjectInstance;
    using OutfitInstance = Appearances.OutfitInstance;

    public partial class ProtocolGame : Internal.Protocol
    {
        private AppearanceInstance ReadCreatureOutfit(Internal.ByteArray message, AppearanceInstance instance = null) {
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

                OutfitInstance outfitInstance = instance as OutfitInstance;
                if (!!outfitInstance) {
                    outfitInstance.UpdateProperties(headColor, torsoColor, legsColor, detailColor, addonsFlags);
                    return instance;
                }

                return AppearanceStorage.CreateOutfitInstance((uint)outfitId, headColor, torsoColor, legsColor, detailColor, addonsFlags);
            }

            uint objectId = message.ReadUnsignedShort();
            ObjectInstance objectInstance = instance as ObjectInstance;
            if (!!objectInstance && objectInstance.Id == objectId)
                return objectInstance;

            if (objectId == 0)
                return AppearanceStorage.CreateOutfitInstance(OutfitInstance.InvisibleOutfitId, 0, 0, 0, 0, 0);

            return AppearanceStorage.CreateObjectInstance(objectId, 0);
        }

        private AppearanceInstance ReadMountOutfit(Internal.ByteArray message, AppearanceInstance instance = null) {
            uint outfitId = message.ReadUnsignedShort();

            OutfitInstance outfitInstance = instance as OutfitInstance;
            if (!!outfitInstance && outfitInstance.Id == outfitId)
                return outfitInstance;

            if (outfitId != 0)
                return AppearanceStorage.CreateOutfitInstance(outfitId, 0, 0, 0, 0, 0);

            return null;
        }
        
        private Creatures.Creature ReadCreatureInstance(Internal.ByteArray message, int type = -1,
                    UnityEngine.Vector3Int? absolutePosition = null) {

            if (type == -1)
                type = message.ReadUnsignedShort();

            if (type != AppearanceInstance.Creature && type != AppearanceInstance.OutdatedCreature && type != AppearanceInstance.UnknownCreature)
                throw new System.Exception("ProtocolGame.ReadCreatureInstance: Invalid creature type");

            var gameManager = OpenTibiaUnity.GameManager;

            Creatures.Creature creature;
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
                            creature = new Creatures.Creature(newId);

                        creature = CreatureStorage.ReplaceCreature(creature, removeId);
                        if (!creature)
                            throw new System.Exception("ProtocolGame.ReadCreatureInstance: Failed to append creature.");

                        creature.Type = creatureType;
                        if (gameManager.ClientVersion >= 1120)
                            creature.SetSummonerId(creature.IsSummon ? message.ReadUnsignedInt() : 0);
                        
                        creature.Name = message.ReadString();
                    } else {
                        uint creatureId = message.ReadUnsignedInt();
                        creature = CreatureStorage.GetCreature(creatureId);
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
                    creature.SetPKFlag(message.ReadEnum<PKFlag>());
                    creature.SetPartyFlag(message.ReadEnum<PartyFlag>());

                    if (gameManager.GetFeature(GameFeature.GameCreatureEmblems) && type == AppearanceInstance.UnknownCreature)
                        creature.SetGuildFlag(message.ReadEnum<GuildFlag>());

                    if (gameManager.GetFeature(GameFeature.GameCreatureMarks)) {
                        creature.Type = message.ReadEnum<CreatureType>();
                        if (gameManager.ClientVersion >= 1120)
                            creature.SetSummonerId(creature.IsSummon ? message.ReadUnsignedInt() : 0);
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
                    creature = CreatureStorage.GetCreature(creatureId);
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
            CreatureStorage.InvalidateOpponents();
            return creature;
        }

        private ObjectInstance ReadObjectInstance(Internal.ByteArray message, int id = -1) {
            if (id == -1)
                id = message.ReadUnsignedShort();

            if (id == 0)
                return null;
            else if (id <= AppearanceInstance.Creature)
                throw new System.Exception("ProtocolGameUtility.ReadObjectInstance: Invalid type (id = " + id + ")");

            var @object = AppearanceStorage.CreateObjectInstance((uint)id, 0);
            if (!@object)
                throw new System.Exception("ProtocolGameUtility.ReadObjectInstance: Invalid instance with id " + id);

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

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameQuickLoot) && @object.Type.IsContainer)
                message.ReadUnsignedByte(); // autolootIndex

            return @object;
        }

        private int ReadField(Internal.ByteArray message, int x, int y, int z) {
            var mapPosition = new UnityEngine.Vector3Int(x, y, z);
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
                        throw new System.Exception("ProtocolGameUtility.ReadField: Expected creatures but received regular object.");
                }

                stackPos++;
            }

            return typeOrId - 65280;
        }

        private int ReadArea(Internal.ByteArray message, int startx, int starty, int endx, int endy) {
            UnityEngine.Vector3Int position = WorldMapStorage.Position;

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
                        if (skip > 0)
                            skip--;
                        else
                            skip = ReadField(message, x, y, z);
                        
                        UnityEngine.Vector3Int mapPosition = new UnityEngine.Vector3Int(x, y, z);
                        UnityEngine.Vector3Int absolutePosition = WorldMapStorage.ToAbsolute(mapPosition);
                        
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

        private int ReadFloor(Internal.ByteArray message, int z, int skip) {
            for (int x = 0; x <= Constants.MapSizeX - 1; x++) {
                for (int y = 0; y <= Constants.MapSizeY - 1; y++) {
                    if (skip > 0)
                        skip--;
                    else
                        skip = ReadField(message, x, y, z);

                    UnityEngine.Vector3Int mapPosition = new UnityEngine.Vector3Int(x, y, z);
                    UnityEngine.Vector3Int absolutePosition = WorldMapStorage.ToAbsolute(mapPosition);

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
