using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Network
{
    public partial class ProtocolGame : Protocol
    {

        private void ParseSetInventory(InputMessage message) {
            int slot = message.GetU8();
            var obj = ReadObjectInstance(message);

            OpenTibiaUnity.ContainerStorage.BodyContainerView.SetObject((ClothSlots)slot, obj);
        }

        private void ParseDeleteInventory(InputMessage message) {
            int slot = message.GetU8();
            OpenTibiaUnity.ContainerStorage.BodyContainerView.SetObject((ClothSlots)slot, null);
        }


        private void ParsePlayerBlessings(InputMessage message) {
            ushort blessings = message.GetU16();
            message.GetU8(); // unknown

            //m_Player.Blessings = blessings;
        }

        private void ParseBasicData(InputMessage message) {
            bool premium = message.GetBool();
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePremiumExpiration)) {
                uint premiumExpiration = message.GetU32();
            }
            
            byte vocation = message.GetU8();
            if (OpenTibiaUnity.GameManager.ClientVersion >= 1100) {
                bool hasReachedMain = message.GetBool();
            }

            List<byte> spells = new List<byte>();
            ushort spellsCount = message.GetU16();
            for (int i = 0; i < spellsCount; i++) {
                spells.Add(message.GetU8());
            }

            if (m_Player) {
                //m_Player.PremiumStatus = premium;
                //m_Player.PremiumExpiration = premiumExpiration;
                //m_Player.Vocation = vocation;
                //m_Player.ReachedMain = hasReachedMain;
            }
        }

        private void ParsePlayerStats(InputMessage message) {
            int ticks = OpenTibiaUnity.TicksMillis;

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameDoubleHealth)) {
                uint health = message.GetU32();
                uint maxHealth = message.GetU32();
                m_Player.SetSkill(SkillTypes.Health, (int)health, (int)maxHealth, 0);
            } else {
                int health = message.GetU16();
                int maxHealth = message.GetU16();
                m_Player.SetSkill(SkillTypes.Health, health, maxHealth, 0);
            }

            int freeCapacity;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameDoubleFreeCapacity)) {
                freeCapacity = message.GetS32();
            } else {
                freeCapacity = message.GetS16();
            }

            int totalCapacity = 0;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameTotalCapacity))
                totalCapacity = message.GetS32();

            m_Player.SetSkill(SkillTypes.Capacity, freeCapacity, totalCapacity, 0);

            long experience;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameDoubleExperience)) {
                experience = message.GetS64();
            } else {
                experience = message.GetS32();
            }

            m_Player.SetSkill(SkillTypes.Experience, (int)experience, 1, 0);

            ushort level = message.GetU16();
            byte levelPercent = message.GetU8();
            m_Player.SetSkill(SkillTypes.Level, level, 1, levelPercent);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameExperienceBonus)) {
                float baseXpGain = message.GetU16() / 100f;
                float voucherAddend = message.GetU16() / 100f;
                float grindingAddend = message.GetU16() / 100f;
                float storeBoostAddend = message.GetU16() / 100f;
                float huntingBoostFactor = message.GetU16() / 100f;
                m_Player.ExperienceGainInfo.UpdateGainInfo(baseXpGain, voucherAddend, grindingAddend, storeBoostAddend, huntingBoostFactor);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameDoubleHealth)) {
                uint mana = message.GetU32();
                uint maxMana = message.GetU32();
                m_Player.SetSkill(SkillTypes.Mana, (int)mana, (int)maxMana, 0);
            } else {
                int mana = message.GetU16();
                int maxMana = message.GetU16();
                m_Player.SetSkill(SkillTypes.Mana, mana, maxMana, 0);
            }

            byte magicLevel = message.GetU8();
            byte baseMagicLevel = OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameSkillsBase) ? message.GetU8() : magicLevel;
            byte magicLevelPercent = message.GetU8();
            m_Player.SetSkill(SkillTypes.MagLevel, magicLevel, baseMagicLevel, magicLevelPercent);

            int soul = message.GetU8();
            m_Player.SetSkill(SkillTypes.SoulPoints, soul, 1, 0);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerStamina)) {
                int stamina = ticks + 60000 * message.GetU16();
                m_Player.SetSkill(SkillTypes.Stamina, stamina, ticks, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameSkillsBase)) {
                ushort baseSpeed = message.GetU16();
                m_Player.SetSkill(SkillTypes.Speed, m_Player.GetSkillValue(SkillTypes.Speed), baseSpeed, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerRegenerationTime)) {
                int regeneration = ticks + 60000 * message.GetU16();
                m_Player.SetSkill(SkillTypes.Food, regeneration, ticks, 0);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameOfflineTrainingTime)) {
                int training = ticks + 60000 * message.GetU16();
                m_Player.SetSkill(SkillTypes.OfflineTraining, training, ticks, 0);

                if (OpenTibiaUnity.GameManager.ClientVersion >= 1097) {
                    uint remainingSeconds = message.GetU16();
                    bool canBuyMoreXpBoosts = message.GetBool();
                    m_Player.ExperienceGainInfo.UpdateStoreXpBoost(remainingSeconds, canBuyMoreXpBoosts);
                }
            }
        }

        private void ParsePlayerSkills(InputMessage message) {
            SkillTypes[] skills = new SkillTypes[] {
                SkillTypes.Fist,
                SkillTypes.Club,
                SkillTypes.Sword,
                SkillTypes.Axe,
                SkillTypes.Distance,
                SkillTypes.Shield,
                SkillTypes.Fishing };
            SkillTypes[] specialSkills = new SkillTypes[] {
                SkillTypes.CriticalHitChance,
                SkillTypes.CriticalHitDamage,
                SkillTypes.LifeLeechChance,
                SkillTypes.LifeLeechAmount,
                SkillTypes.ManaLeechChance,
                SkillTypes.ManaLeechAmount };

            foreach (var skill in skills) {
                int level = message.GetU16();
                int baseLevel = message.GetU16();
                int percentage = message.GetU8();

                m_Player.SetSkill(skill, level, baseLevel, percentage);
            }

            foreach (var skill in specialSkills) {
                int level = message.GetU16();
                int baseLevel = message.GetU16();

                m_Player.SetSkill(skill, level, baseLevel);
            }
        }

        private void ParsePlayerStates(InputMessage message) {
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GamePlayerStateU16))
                m_Player.StateFlags = message.GetU16();
            else
                m_Player.StateFlags = message.GetU8();
        }

        private void ParsePlayerResource(InputMessage message) {
            byte type = message.GetU8();
            ulong balance = message.GetU64();

            switch (type) {
                case (int)ResourceTypes.BankGold:
                    //m_Player.BankGold = balance;
                    break;

                case (int)ResourceTypes.InventoryGold:
                    //m_Player.InventoryGold = balance;
                    break;

                case (int)ResourceTypes.PreyBonusRerolls:
                    //PreyManager.getInstance().bonusRerollAmount = _loc3_;
                    break;
            }
        }

        private void ParsePlayerInventory(InputMessage message) {
            //List<KeyValuePair<ushort, ushort>> items = new List<KeyValuePair<ushort, ushort>>();
            ushort size = message.GetU16();
            for (int i = 0; i < size; i++) {
                ushort id = message.GetU16();
                byte subType = message.GetU8();
                ushort count = message.GetU16();
            }
        }
    }
}
