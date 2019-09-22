namespace OpenTibiaUnity.Core.Communication.Game
{
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseMonsterCyclopedia(Internal.ByteArray message) {
            int count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                string classification = message.ReadString();

                uint total = message.ReadUnsignedShort();
                uint known = message.ReadUnsignedShort();
            }
        }

        private void ParseMonsterCyclopediaMonsters(Internal.ByteArray message) {
            string classification = message.ReadString();

            int monsters = message.ReadUnsignedShort();
            for (int i = 0; i < monsters; i++) {
                // the client should detect monster name
                // and some information about it through staticdata
                ushort raceID = message.ReadUnsignedShort();
                var stage = message.ReadEnum<CyclopediaRaceStage>();
                if (stage != CyclopediaRaceStage.Unlocked) {
                    var occurrence = message.ReadEnum<CyclopediaRaceOccurence>();
                }
            }
        }

        private void ParseMonsterCyclopediaRace(Internal.ByteArray message) {
            ushort raceID = message.ReadUnsignedShort();
            var classification = message.ReadString();

            int unlockState = message.ReadUnsignedByte();
            uint totalKills = message.ReadUnsignedInt();

            int killsToFirstDetailStage = message.ReadUnsignedShort();
            int killsToSecondDetailStage = message.ReadUnsignedShort();
            int killsToThirdDetailStage = message.ReadUnsignedShort();

            if (unlockState >= 1) {
                var difficulty = message.ReadEnum<CyclopediaRaceDifficulty>();
                var occurrence = message.ReadEnum<CyclopediaRaceOccurence>();

                int count = message.ReadUnsignedByte();
                for (int i = 0; i < count; i++) {
                    ushort objectID = message.ReadUnsignedShort();
                    var rarity = message.ReadEnum<CyclopediaRaceLootRarity>();
                    bool specialEventLoot = message.ReadBoolean();
                    if (objectID != 0) {
                        var objectName = message.ReadString();
                        var lootType = message.ReadEnum<CyclopediaRaceLootType>();
                    }
                }
            }

            if (unlockState >= 2) {
                ushort charmPoints = message.ReadUnsignedShort();
                var attackType = message.ReadEnum<CyclopediaRaceAttackType>();
                bool castSpellsOrUseSkills = message.ReadBoolean(); // shows the icon of spells
                uint health = message.ReadUnsignedInt();
                uint experience = message.ReadUnsignedInt();
                ushort speed = message.ReadUnsignedShort();
                ushort armor = message.ReadUnsignedShort();
            }

            if (unlockState >= 3) {
                int count = message.ReadUnsignedByte();
                for (int i = 0; i < count; i++) {
                    var combatType = message.ReadEnum<CyclopediaCombatType>();
                    int value = message.ReadShort();
                }

                count = message.ReadUnsignedShort();
                for (int i = 0; i < count; i++) {
                    string location = message.ReadString();
                }
            }

            if (unlockState >= 4) {
                bool hasCharmSelected = message.ReadBoolean();
                if (hasCharmSelected) {
                    int selectedCharmID = message.ReadUnsignedByte();
                    int unknown0 = message.ReadUnsignedByte();
                    int unknown1 = message.ReadUnsignedByte();
                    int unknown2 = message.ReadUnsignedByte();
                }

                bool canSelectCharm = message.ReadBoolean();
            }
        }

        private void ParseMonsterCyclopediaBonusEffects(Internal.ByteArray message) {
            int charmPoints = message.ReadInt();

            int count = message.ReadUnsignedByte();
            for (int i = 0; i < count; i++) {
                int charmType = message.ReadUnsignedByte();
                string charmName = message.ReadString();
                string charmDescription = message.ReadString();

                byte unknown2 = message.ReadUnsignedByte(); // valid values: 0, 1, 2 (likely to be agressive/defensive/passive)
                ushort price = message.ReadUnsignedShort();

                bool unlocked = message.ReadBoolean();
                bool activated = message.ReadBoolean();
                if (activated) {
                    ushort selectedCreature = message.ReadUnsignedShort(); // raceID
                    uint clearPrice = message.ReadUnsignedInt();
                }
            }

            byte remainingNumberOfAssignableBonusEffects = message.ReadUnsignedByte();
            bool hasCharmExpansion = remainingNumberOfAssignableBonusEffects == 255;

            // selectable race ids
            count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                ushort raceID = message.ReadUnsignedShort();
            }
        }

        private void ParseMonsterCyclopediaNewDetails(Internal.ByteArray message) {
            ushort raceID = message.ReadUnsignedShort();
        }

        private void ParseCyclopediaCharacterInfo(Internal.ByteArray message) {
            // TODO, in 12.15 there are extra 4 bytes in both client/server
            // i suggest that these bytes might have to do with character id
            // as tibia introduced the so called "Friends widget"

            var type = message.ReadEnum<CyclopediaCharacterInfoType>();
            switch (type) {
                case CyclopediaCharacterInfoType.BaseInformation: {
                    ReadCyclopediaCharacterInfoBaseInformation(message);
                    break;
                }

                case CyclopediaCharacterInfoType.GeneralStats: {
                    ReadCyclopediaCharacterInfoGeneralStats(message);
                    break;
                }

                case CyclopediaCharacterInfoType.CombatStats: {
                    ReadCyclopediaCharacterInfoCombatStats(message);
                    break;
                }

                case CyclopediaCharacterInfoType.RecentDeaths: {
                    ReadCyclopediaCharacterInfoRecentDeaths(message);
                    break;
                }

                case CyclopediaCharacterInfoType.RecentPvpKills: {
                    ReadCyclopediaCharacterInfoRecentPvpKills(message);
                    break;
                }

                case CyclopediaCharacterInfoType.Achievements: {
                    ReadCyclopediaCharacterInfoAchievements(message);
                    break;
                }
            }
        }

        private void ParseCyclopediaMapData(Internal.ByteArray message) {
            int dataType = message.ReadUnsignedByte();

            // unfinished packet, more information needed....
            switch (dataType) {
                case 0: { // StaticMarker (sure)
                    var position = message.ReadPosition();
                    int markType = message.ReadUnsignedByte();
                    string markDescription = message.ReadString();
                    break;
                }

                case 1: { // AreaUnlock (sure)
                    int count = message.ReadUnsignedShort();
                    for (int i = 0; i < count; i++) {
                        // UnlockedArea (struct)
                        message.ReadUnsignedByte(); // unknown
                        message.ReadUnsignedByte(); // unknown
                        message.ReadUnsignedByte(); // (enum) maximum value: 3
                        message.ReadUnsignedByte(); // unknown
                    }

                    count = message.ReadUnsignedShort();
                    for (int i = 0; i < count; i++) {
                        message.ReadUnsignedShort(); // unknown
                    }

                    count = message.ReadUnsignedShort();
                    for (int i = 0; i < count; i++) {
                        message.ReadUnsignedShort(); // unknown
                    }

                    break;
                }

                case 2: { // Raid (sure)
                    var position = message.ReadPosition();
                    message.ReadUnsignedByte(); /* (enum) unknown (0 = Active Raid) */
                    break;
                }

                case 3: { // ImminentRaid (not sure)
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown
                    break;
                }

                case 4: { // ViewPoints (not sure)
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown
                    break;
                }


                case 5: { // MonsterKnowledge (not sure)
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown
                    message.ReadUnsignedByte(); // unknown

                    // the following bytes are unknown //
                    int count = message.ReadUnsignedByte();
                    for (int i = 0; i < count; i++) {
                        message.ReadUnsignedInt(); // 4 bytes (idk if this is unsigned int)
                        message.ReadUnsignedByte();

                        int something = message.ReadUnsignedByte();
                    }

                    break;
                }

                case 6: {
                    break;
                }

                case 7: {

                    break;
                }

                case 8: {

                    break;
                }

                case 9: {

                    break;
                }

                case 10: {
                    ushort unknown = message.ReadUnsignedShort();
                    break;
                }

                case 11: {

                    break;
                }
            }
        }

        private void ReadCyclopediaCharacterInfoBaseInformation(Internal.ByteArray message) {
            string characterName = message.ReadString();
            string vocation = message.ReadString();
            ushort level = message.ReadUnsignedShort();
            var outfit = ReadCreatureOutfit(message);
        }

        private void ReadCyclopediaCharacterInfoGeneralStats(Internal.ByteArray message) {
            ulong experience = message.ReadUnsignedLong();
            ushort level = message.ReadUnsignedShort();
            int levelPercent = message.ReadUnsignedByte();

            float baseXpGain = message.ReadUnsignedShort() / 100f;
            float grindingAddend = message.ReadUnsignedShort() / 100f;
            float storeBoostAddend = message.ReadUnsignedShort() / 100f;
            float huntingBoostFactor = message.ReadUnsignedShort() / 100f;

            message.ReadUnsignedShort(); // yet unknown
            bool canBuyXpBoost = message.ReadBoolean();

            int health = message.ReadUnsignedShort();
            int maxHealth = message.ReadUnsignedShort();
            int mana = message.ReadUnsignedShort();
            int maxMana = message.ReadUnsignedShort();
            int soul = message.ReadUnsignedByte();

            int ticks = OpenTibiaUnity.TicksMillis;
            int stamina = ticks + 60000 * message.ReadUnsignedShort();
            int regeneration = ticks + 1000 * message.ReadUnsignedShort();
            int training = ticks + 60000 * message.ReadUnsignedShort();

            int speed = message.ReadUnsignedShort();
            int baseSpeed = message.ReadUnsignedShort();

            uint totalCapacity = message.ReadUnsignedInt();
            uint baseCapacity = message.ReadUnsignedInt();
            uint freeCapacity = message.ReadUnsignedInt();

            int skillCount = message.ReadUnsignedByte();
            for (int i = 0; i < skillCount; i++) {
                var skillType = message.ReadEnum<SkillType>();
                var skill = ReadSkill(message);
            }
        }

        private void ReadCyclopediaCharacterInfoCombatStats(Internal.ByteArray message) {
            SkillType[] specialSkills = new SkillType[] {
                    SkillType.CriticalHitChance,
                    SkillType.CriticalHitDamage,
                    SkillType.LifeLeechChance,
                    SkillType.LifeLeechAmount,
                    SkillType.ManaLeechChance,
                    SkillType.ManaLeechAmount };

            foreach (var skill in specialSkills) {
                var skillStruct = ReadSkill(message, true);
            }

            int playerBlessings = message.ReadUnsignedByte();
            int totalBlessings = message.ReadUnsignedByte();

            ushort attackValue = message.ReadUnsignedShort();
            var combatType = message.ReadEnum<CyclopediaCombatType>();

            int convertedDamage = message.ReadUnsignedByte();
            combatType = message.ReadEnum<CyclopediaCombatType>();

            int armorValue = message.ReadUnsignedShort();
            int defenseValue = message.ReadUnsignedShort();

            int combats = message.ReadUnsignedByte();
            for (int i = 0; i < combats; i++) {
                combatType = message.ReadEnum<CyclopediaCombatType>();
                int combatValue = message.ReadUnsignedByte();
            }
        }

        private void ReadCyclopediaCharacterInfoRecentPvpKills(Internal.ByteArray message) {
            ushort count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                uint time = message.ReadUnsignedInt();
                string description = message.ReadString();
                var status = message.ReadEnum<CyclopediaPvpKillStatus>();
            }
        }

        private void ReadCyclopediaCharacterInfoRecentDeaths(Internal.ByteArray message) {
            ushort count = message.ReadUnsignedShort();
            for (int i = 0; i < count; i++) {
                uint time = message.ReadUnsignedInt();
                string cause = message.ReadString();
            }
        }

        private void ReadCyclopediaCharacterInfoAchievements(Internal.ByteArray message) {
            ushort totalPoints = message.ReadUnsignedShort();
            ushort totalSecretAchievements = message.ReadUnsignedShort();

            ushort obtainedAchievements = message.ReadUnsignedShort();
            for (int i = 0; i < obtainedAchievements; i++) {
                ushort achievementID = message.ReadUnsignedShort();
                uint time = message.ReadUnsignedInt();
                bool secret = message.ReadBoolean();
                if (secret) {
                    string name = message.ReadString();
                    string description = message.ReadString();
                    int grade = message.ReadUnsignedByte();
                }
            }
        }
    }
}