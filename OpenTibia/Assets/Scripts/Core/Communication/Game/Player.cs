using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Communication.Game
{
    public struct ProtocolOutfit
    {
        public ushort _id;
        public string Name;
        public int AddOns;
        public bool Locked;
        public uint StoreOfferId;
    }

    public struct ProtocolMount
    {
        public ushort _id;
        public string Name;
        public bool Locked;
        public uint StoreOfferId;
    }
    
    public partial class ProtocolGame : Internal.Protocol
    {
        private void ParseDeath(Internal.ByteArray message) {
            var deathType = DeathType.DeathTypeRegular;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDeathType))
                deathType = (DeathType)message.ReadUnsignedByte();

            int penalty = 100;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePenalityOnDeath) && deathType == DeathType.DeathTypeRegular)
                penalty = message.ReadUnsignedByte();

            bool useDeathRedemption = false;
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1120)
                useDeathRedemption = message.ReadBoolean();

            // TODO death actions...
            //LocalPlayer.OnDeath(deathType, penalty);
        }

        private void ParseSupplyStash(Internal.ByteArray message) {
            
        }

        private void ParseDepotTileState(Internal.ByteArray message) {
            Player.IsInDepot = message.ReadBoolean();
        }

        private void ParseSetInventory(Internal.ByteArray message) {
            var slot = message.ReadEnum<ClothSlots>();
            var @object = ReadObjectInstance(message);
            
            OpenTibiaUnity.ContainerStorage.BodyContainerView.SetObject(slot, @object);
        }

        private void ParseDeleteInventory(Internal.ByteArray message) {
            var slot = message.ReadEnum<ClothSlots>();
            OpenTibiaUnity.ContainerStorage.BodyContainerView.SetObject(slot, null);
        }

        private void ParseBlessingsDialog(Internal.ByteArray message) {

        }

        private void ParseBlessings(Internal.ByteArray message) {
            var protocolVersion = OpenTibiaUnity.GameManager.ProtocolVersion;
            ushort blessings = message.ReadUnsignedShort();

            if (protocolVersion < 1120)
                Player.HasFullBlessings = blessings == 1;
            else
                Player.Blessings = blessings;

            if (protocolVersion >= 1120)
                message.ReadUnsignedByte(); // buttonStatus
        }

        private void ParsePremiumTrigger(Internal.ByteArray message) {
            int triggers = message.ReadUnsignedByte();
            for (int i = 0; i < triggers; i++) {
                message.ReadUnsignedByte(); // trigger
                // TODO
            }
        }

        private void ParseBasicData(Internal.ByteArray message) {
            bool premium = message.ReadBoolean();
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePremiumExpiration)) {
                uint premiumExpiration = message.ReadUnsignedInt();
            }
            
            byte vocation = message.ReadUnsignedByte();
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1100) {
                bool hasReachedMain = message.ReadBoolean();
            }

            List<byte> spells = new List<byte>();
            ushort spellsCount = message.ReadUnsignedShort();
            for (int i = 0; i < spellsCount; i++) {
                spells.Add(message.ReadUnsignedByte());
            }

            if (Player) {
                //_player.PremiumStatus = premium;
                //_player.PremiumExpiration = premiumExpiration;
                //_player.Vocation = vocation;
                //_player.ReachedMain = hasReachedMain;
            }
        }

        private void ParsePlayerStats(Internal.ByteArray message) {
            int ticks = OpenTibiaUnity.TicksMillis;

            int health, maxHealth;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleHealth)) {
                health = message.ReadInt();
                maxHealth = message.ReadInt();
            } else {
                health = message.ReadUnsignedShort();
                maxHealth = message.ReadUnsignedShort();
            }
            
            Player.SetSkill(SkillType.Health, health, maxHealth, 0);

            int freeCapacity;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleFreeCapacity))
                freeCapacity = message.ReadInt();
            else
                freeCapacity = message.ReadShort();

            Player.FreeCapacity = freeCapacity;

            int totalCapacity = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameTotalCapacity))
                totalCapacity = message.ReadInt();


            Player.SetSkill(SkillType.Capacity, freeCapacity, totalCapacity, 0);

            long experience;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleExperience))
                experience = message.ReadLong();
            else
                experience = message.ReadInt();
            
            Player.SetSkill(SkillType.Experience, experience, 1, 0);

            ushort level = message.ReadUnsignedShort();
            byte levelPercent = message.ReadUnsignedByte();
            Player.SetSkill(SkillType.Level, level, 1, levelPercent);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceGain)) {
                float baseXpGain = message.ReadUnsignedShort() / 100f;
                float voucherAddend = 0;
                if (OpenTibiaUnity.GameManager.ClientVersion < 1150)
                    voucherAddend = message.ReadUnsignedShort() / 100f;
                float grindingAddend = message.ReadUnsignedShort() / 100f;
                float storeBoostAddend = message.ReadUnsignedShort() / 100f;
                float huntingBoostFactor = message.ReadUnsignedShort() / 100f;
                
                Player.ExperienceGainInfo.UpdateGainInfo(baseXpGain, voucherAddend, grindingAddend, storeBoostAddend, huntingBoostFactor);
            } else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceBonus)) {
                double experienceBonus = message.ReadDouble();
                Player.ExperienceBonus = experienceBonus;
            }

            int mana, maxMana;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleHealth)) {
                mana = message.ReadInt();
                maxMana = message.ReadInt();
            } else {
                mana = message.ReadUnsignedShort();
                maxMana = message.ReadUnsignedShort();
            }

            Player.SetSkill(SkillType.Mana, mana, maxMana, 0);
            
            if (OpenTibiaUnity.GameManager.ClientVersion < 1200) {
                byte magicLevel = message.ReadUnsignedByte();
                byte baseMagicLevel = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSkillsBase) ? message.ReadUnsignedByte() : magicLevel;
                byte magicLevelPercent = message.ReadUnsignedByte();
                
                Player.SetSkill(SkillType.MagLevel, magicLevel, baseMagicLevel, magicLevelPercent);
            }

            int soul = message.ReadUnsignedByte();
            Player.SetSkill(SkillType.SoulPoints, soul, 1, 0);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerStamina)) {
                int stamina = ticks + 60000 * message.ReadUnsignedShort();
                Player.SetSkill(SkillType.Stamina, stamina, ticks, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSkillsBase)) {
                ushort baseSpeed = message.ReadUnsignedShort();
                Player.SetSkill(SkillType.Speed, Player.GetSkillValue(SkillType.Speed), baseSpeed, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerRegenerationTime)) {
                int regeneration = ticks + 1000 * message.ReadUnsignedShort();
                Player.SetSkill(SkillType.Food, regeneration, ticks, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameOfflineTrainingTime)) {
                int training = ticks + 60000 * message.ReadUnsignedShort();
                Player.SetSkill(SkillType.OfflineTraining, training, ticks, 0);

                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameExperienceGain)) {
                    uint remainingSeconds = message.ReadUnsignedShort();
                    bool canBuyMoreXpBoosts = message.ReadBoolean();
                    Player.ExperienceGainInfo.UpdateStoreXpBoost(remainingSeconds, canBuyMoreXpBoosts);
                }
            }
        }

        private Creatures.Skill ReadSkill(Internal.ByteArray message, bool special = false) {
            int level;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameDoubleSkills))
                level = message.ReadUnsignedShort();
            else
                level = message.ReadUnsignedByte();
            
            int baseLevel;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameSkillsBase))
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameBaseSkillU16))
                    baseLevel = message.ReadUnsignedShort();
                else
                    baseLevel = message.ReadUnsignedByte();
            else
                baseLevel = level;

            if (!special && OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePercentSkillU16)) {
                ushort loyaltyBonus = message.ReadUnsignedShort();
            }
            
            float percentage = 0.0f;
            if (!special) {
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePercentSkillU16))
                    percentage = message.ReadUnsignedShort() / 100f;
                else
                    percentage = message.ReadUnsignedByte();
            }
            
            return new Creatures.Skill(level, baseLevel, percentage);
        }

        private void ParsePlayerSkills(Internal.ByteArray message) {
            // magic level is being parsed
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1200) {
                var skillStruct = ReadSkill(message);
                Player.SetSkill(SkillType.MagLevel, skillStruct.Level, skillStruct.BaseLevel, skillStruct.Percentage);
            }

            var skills = new SkillType[] {
                SkillType.Fist,
                SkillType.Club,
                SkillType.Sword,
                SkillType.Axe,
                SkillType.Distance,
                SkillType.Shield,
                SkillType.Fishing };
            
            foreach (var skill in skills) {
                var skillStruct = ReadSkill(message);
                Player.SetSkill(skill, skillStruct.Level, skillStruct.BaseLevel, skillStruct.Percentage);
            }
            
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAdditionalSkills)) {
                SkillType[] specialSkills = new SkillType[] {
                    SkillType.CriticalHitChance,
                    SkillType.CriticalHitDamage,
                    SkillType.LifeLeechChance,
                    SkillType.LifeLeechAmount,
                    SkillType.ManaLeechChance,
                    SkillType.ManaLeechAmount };

                foreach (var skill in specialSkills) {
                    var skillStruct = ReadSkill(message, true);
                    Player.SetSkill(skill, skillStruct.Level, skillStruct.BaseLevel);
                }
            }

            // todo: find if this is capacity
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1150) {
                int totalCapacity = message.ReadInt();
                int baseCapacity = message.ReadInt();

                Player.SetSkill(SkillType.Capacity, totalCapacity, baseCapacity);
            }
        }

        private void ParsePlayerStates(Internal.ByteArray message) {
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerStateU32))
                Player.StateFlags = message.ReadUnsignedInt();
            else if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerStateU16))
                Player.StateFlags = message.ReadUnsignedShort();
            else
                Player.StateFlags = message.ReadUnsignedByte();
        }

        private void ParseClearTarget(Internal.ByteArray message) {
            uint creatureId = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameAttackSeq))
                creatureId = message.ReadUnsignedInt();

            Creatures.Creature creature;
            if (!!(creature = CreatureStorage.AttackTarget) && (creature.Id == creatureId || creatureId == 0))
                CreatureStorage.SetAttackTarget(null, false);
            else if (!!(creature = CreatureStorage.FollowTarget) && (creature.Id == creatureId || creatureId == 0))
                CreatureStorage.SetFollowTarget(null, false);
        }

        private void ParseSetTactics(Internal.ByteArray message) {
            int attackMode = message.ReadUnsignedByte();
            int chaseMode = message.ReadUnsignedByte();
            int secureMode = message.ReadUnsignedByte();
            int pvpMode = message.ReadUnsignedByte();

            OpenTibiaUnity.GameManager.onTacticsChangeEvent.Invoke(
                (CombatAttackModes)attackMode,
                (CombatChaseModes)chaseMode,
                secureMode == 1,
                (CombatPvPModes)pvpMode);
        }

        private void ParseResourceBalance(Internal.ByteArray message) {
            byte type = message.ReadUnsignedByte();
            ulong balance = message.ReadUnsignedLong();

            switch (type) {
                case (int)ResourceTypes.BankGold:
                    //_player.BankGold = balance;
                    break;

                case (int)ResourceTypes.InventoryGold:
                    //_player.InventoryGold = balance;
                    break;

                case (int)ResourceTypes.PreyBonusRerolls:
                    if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePrey))
                        throw new System.Exception("ProtocolGame.ParseResourceBalance: Invalid resource type: " + type + ".");
                    //PreyManager.Insance.BonusRerollAmount = balance;
                    break;

                case (int)ResourceTypes.CollectionTokens:
                    if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameRewardWall))
                        throw new System.Exception("ProtocolGame.ParseResourceBalance: Invalid resource type: " + type + ".");
                    break;

                default:
                    throw new System.Exception("ProtocolGame.ParseResourceBalance: Invalid resource type: " + type + ".");
            }
        }
        
        private void ParseUnjustifiedPoints(Internal.ByteArray message) {
            message.ReadUnsignedByte(); // dailyProgress
            message.ReadUnsignedByte(); // dailyRemaining
            message.ReadUnsignedByte(); // weeklyProgress
            message.ReadUnsignedByte(); // weeklyRemaining
            message.ReadUnsignedByte(); // monthlyProgress
            message.ReadUnsignedByte(); // monthlyRemaining
            message.ReadUnsignedByte(); // skullDuration
        }

        private void ParsePvpSituations(Internal.ByteArray message) {
            message.ReadUnsignedByte(); // situations
        }

        private void ParseOutfitDialog(Internal.ByteArray message) {
            var outfit = ReadCreatureOutfit(message);
            Appearances.AppearanceInstance mountOutfit = null;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts))
                mountOutfit = ReadMountOutfit(message);

            var outfitList = new List<ProtocolOutfit>();
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewOutfitProtocol)) {
                int count;
                if (OpenTibiaUnity.GameManager.ClientVersion >= 1185)
                    count = message.ReadUnsignedShort();
                else
                    count = message.ReadUnsignedByte();
                
                for (int i = 0; i < count; i++) {
                    ushort outfitId = message.ReadUnsignedShort();
                    var outfitName = message.ReadString();
                    int addOns = message.ReadUnsignedByte();
                    bool locked = true;
                    uint offerId = 0;

                    if (OpenTibiaUnity.GameManager.ClientVersion >= 1185) {
                        locked = message.ReadBoolean();
                        if (locked)
                            offerId = message.ReadUnsignedInt();
                    }

                    outfitList.Add(new ProtocolOutfit() {
                        _id = outfitId,
                        Name = outfitName,
                        AddOns = addOns,
                        Locked = locked,
                        StoreOfferId = offerId,
                    });
                }
            } else {
                ushort outfitStart, outfitEnd;
                if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameOutfitIdU16)) {
                    outfitStart = message.ReadUnsignedShort();
                    outfitEnd = message.ReadUnsignedShort();
                } else {
                    outfitStart = message.ReadUnsignedByte();
                    outfitEnd = message.ReadUnsignedByte();
                }

                for (ushort i = outfitStart; i <= outfitEnd; i++)
                    outfitList.Add(new ProtocolOutfit() { _id = i });
            }

            List<ProtocolMount> mountList = null;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerMounts)) {
                mountList = new List<ProtocolMount>();
                int count;
                if (OpenTibiaUnity.GameManager.ClientVersion >= 1185)
                    count = message.ReadUnsignedShort();
                else
                    count = message.ReadUnsignedByte();

                for (int i = 0; i < count; i++) {
                    ushort mountId = message.ReadUnsignedShort();
                    var mountName = message.ReadString();
                    bool locked = true;
                    uint offerId = 0;

                    if (OpenTibiaUnity.GameManager.ClientVersion >= 1185) {
                        locked = message.ReadBoolean();
                        if (locked)
                            offerId = message.ReadUnsignedInt();
                    }

                    mountList.Add(new ProtocolMount() {
                        _id = mountId,
                        Name = mountName,
                        Locked = locked,
                        StoreOfferId = offerId,
                    });
                }
            }
            
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1185) {
                bool tryOutfit = message.ReadBoolean();
                bool mounted = message.ReadBoolean();
            }

            OpenTibiaUnity.GameManager.onRequestOutfitDialog.Invoke(outfit, mountOutfit, outfitList, mountList);
        }

        private void ParsePlayerInventory(Internal.ByteArray message) {
            //List<KeyValuePair<ushort, ushort>> items = new List<KeyValuePair<ushort, ushort>>();
            ushort size = message.ReadUnsignedShort();
            for (int i = 0; i < size; i++) {
                ushort id = message.ReadUnsignedShort();
                byte subType = message.ReadUnsignedByte();
                ushort count = message.ReadUnsignedShort();
            }
        }
    }
}
