namespace OpenTibiaUnity
{
    public static class Constants
    {
        public const string RealTibiaClientServicesAddress = "https://www.tibia.com/services/clientservices.php";
        public const string OpenTibiaDefaultClientServicesAddress = "http://127.0.0.1/clientservices.php";

        public const string OpenTibiaDefaultFullIPAddress = "127.0.0.1:7171";
        public const string OpenTibiaDefaultIPAddress = "127.0.0.1";
        public const int OpenTibiaDefaultPort = 7171;

        public const string StoreHomeCategoryName = "Home";

        public const int MinimumManageableFramerate = 10;
        public const int MaximumManageableFramerate = 200;

        // Don't modify those unless you know what you are doing.
        public const int PingDelay = 1000;
        public const int ConnectionTimeout = 30 * 1000;
        public const int CharacterSwitchTimeout = 5 * 1000;

        public const int MapWidth = 15;
        public const int MapHeight = 11;
        public const int MapSizeW = 10;
        public const int MapSizeX = MapWidth + 3;
        public const int MapSizeY = MapHeight + 3;
        public const int MapSizeZ = 8;
        public const int MapMinX = 0; // was restricted to 24576, that's why flash-client didn't show minimap properly on custom servers
        public const int MapMinY = 0;
        public const int MapMaxX = MapMinX + 2 * ((1 << 15) - 1);
        public const int MapMaxY = MapMinY + 2 * ((1 << 15) - 1);
        public const int MapMinZ = 0;
        public const int MapMaxZ = 15;

        public const int PathMaxSteps = 128;
        public const int PathMaxDistance = 110;
        public const int PathMatrixCenter = PathMaxDistance;
        public const int PathMatrixSize = 2 * PathMaxDistance + 1;

        public const int PathCostMax = 250;
        public const int PathCostUndefined = 254;
        public const int PathCostObstacle = 255;

        public const int NumEffects = 200;
        public const int NumOnscreenMessages = 16;
        public const int NumFields = MapSizeX * MapSizeY * MapSizeZ;
        public const int FieldSize = 32;
        public const int FieldCacheSize = 32;
        public const int FieldHeight = 24;
        public const int MaxCreatureCount = 1300;

        public const int PlayerOffsetY = 6;
        public const int PlayerOffsetX = 8;

        public const int GroundLayer = 7;
        public const int UndergroundLayer = 2;

        public const int ObjectsUpdateInterval = 40;
        public const int AmbientUpdateInterval = 1000;

        public const int NumPvpHelpersForRisknessDangerous = 5;
        public const int StateFlagSize = 11;
        public const int StateFlagGap = 2;
        public const int SpeechFlagSize = 18;

        public const int OnscreenMessageHeight = 195;
        public const int OnscreenMessageWidth = 290;
        public const int OnscreenMessageGap = 10;

        public const int MaxTalkLength = 255;
        public const int MaxChannelLength = 255;
        public const int MaxContainerNameLength = 255;
        public const int MaxContainerViews = 32;

        public const int MiniMapSideBarViewHeight = 106;
        public const int MiniMapSideBarViewWidth = 106;

        public const int MiniMapCacheSize = 48;
        public const int MiniMapSectorSize = 256;
        public const int MiniMapSideBarZoomMin = -1;
        public const int MiniMapSideBarZoomMax = 2;

        public const int WorldMapScreenWidth = MapSizeX * FieldSize;
        public const int WorldMapScreenHeight = MapSizeY * FieldSize;

        public const int WorldMapRealWidth = MapWidth * FieldSize;
        public const int WorldMapRealHeight = MapHeight * FieldSize;

        public const int WorldMapUnscaledWidth = WorldMapScreenWidth - FieldSize;
        public const int WorldMapUnscaledHeight = WorldMapScreenHeight - FieldSize;

        public const int WorldMapMinimumWidth = (int)(WorldMapRealWidth * 0.6667f);
        public const int WorldMapMinimumHeight = (int)(WorldMapRealHeight * 0.6667f);

        public const int MaxNpcDistance = 3;

        public const int LightmapShrinkFactor = 8;

        public const uint MarkThicknessThin = 1;
        public const uint MarkThicknessBold = 2;

        public static UnityEngine.Color ColorAboveGround = new UnityEngine.Color32(200, 200, 255, 255);
        public static UnityEngine.Color ColorBelowGround = new UnityEngine.Color32(255, 255, 255, 255);
        public static UnityEngine.Color ObjectCursorColor = new UnityEngine.Color32(255, 225, 55, 255);

        public const float HighlightMinOpacity = 0.3f;
        public const float HighlightMaxOpacity = 0.6f;

        public const uint PlayerStartId = 0x10000000;
        public const uint PlayerEndId = 0x40000000;
        public const uint MonsterStartId = 0x40000000;
        public const uint MonsterEndId = 0x80000000;
        public const uint NpcStartId = 0x80000000;
        public const uint NpcEndId = 0xffffffff;

        public const int AnimationDelayBeforeReset = 1000;
        public const int PhaseAutomatic = -1;
        public const int PhaseAsynchronous = 255;
        public const int PhaseRandom = 254;

        public const int MaxTalkHistory = 200;

        public const int SecondsPerDay = 24 * 60 * 60;
        public const int MarketRequestOwnOffers = 65534;
        public const int MarketRequestOwnHistory = 65535;

        public const int HighlightObjectOpacityInterval = 50;
    }

    public enum ClientSpecification
    {
        Cipsoft,
        OpenTibia,
    }

    public enum AppearanceCategory : byte
    {
        Object,
        Outfit,
        Effect,
        Missile,
    }

    public enum EnterPossibleFlag : byte
    {
        Possible,
        PossibleNoAnimation,
        NotPossible,
    }

    public enum PathState : byte
    {
        PathEmpty,
        PathExists,
        PathErrorGoDownstairs,
        PathErrorGoUpstairs,
        PathErrorTooFar,
        PathErrorUnreachable,
        PathErrorInternal,
    }
    public enum PathDirection : byte
    {
        Invalid = 0,
        East = 1,
        NorthEast = 2,
        North = 3,
        NorthWest = 4,
        West = 5,
        SouthWest = 6,
        South = 7,
        SouthEast = 8,
    }
    public enum Direction : byte
    {
        North = 0,
        East,
        South,
        West,
        NorthEast,
        SouthEast,
        SouthWest,
        NorthWest,
        Stop,
    }
    public enum CreatureType : byte
    {
        Player,
        Monster,
        NPC,
        Summon,
        SummonOther, // legacy

        First = Player,
        Last = Summon,
    }
    public enum PartyFlag : byte
    {
        None,
        Leader,
        Member,
        Member_SharedXP_Off,
        Leader_SharedXP_Off,
        Member_SharedXP_Active,
        Leader_SharedXP_Active,
        Member_SharedXP_Inactive_Guilty,
        Leader_SharedXP_Inactive_Guilty,
        Member_SharedXP_Inactive_Innocent,
        Leader_SharedXP_Inactive_Innocent,
        Other,

        First = None,
        Last = Other,
    }
    public enum PkFlag : byte
    {
        None,
        Attacker,
        PartyMode,
        Aggressor,
        PlayerKiller,
        ExcplayerKiller,
        Revenge,

        First = None,
        Last = Revenge,
    }
    public enum SummonType : byte
    {
        None,
        Own,
        Other,

        First = None,
        Last = Other,
    }
    public enum SpeechCategory : byte
    {
        None,
        Normal,
        Trader,
        Quest,
        QuestTrader,
        Travel,
        Hireling = 7,

        First = None,
        Last = Hireling,
    }
    public enum GuildFlag : byte
    {
        None,
        WarAlly,
        WarEnemy,
        WarNeutral,
        Member,
        Other,

        First = None,
        Last = Other,
    }
    public enum RisknessFlag
    {
        None = 0,
        Dangerous = 1,
    }

    public enum SkillType : byte
    {
        Experience = 0,
        Level,
        MagLevel,
        HealthPercent,
        Health,
        Mana,
        Speed,
        Capacity,
        Shield,
        Distance,
        Club,
        Sword,
        Axe,
        Fist,
        Fishing,
        Food,
        SoulPoints,
        Stamina,
        OfflineTraining,
        CriticalHitChance,
        CriticalHitDamage,
        LifeLeechChance,
        LifeLeechAmount,
        ManaLeechChance,
        ManaLeechAmount,

        First = Experience,
        Last = ManaLeechAmount,

        // internal usage only
        ExperienceGain = 254,
        None = 255,
    }
    public enum PlayerState : byte
    {
        Poisoned = 0,
        Burning = 1,
        Electrified = 2,
        Drunk = 3,
        ManaShield = 4,
        Slow = 5,
        Fast = 6,
        Fighting = 7,
        Drowning = 8,
        Freezing = 9,
        Dazzled = 10,
        Cursed = 11,
        Strengthened = 12,
        PzBlock = 13,
        PzEntered = 14,
        Bleeding = 15,

        Hungry = 31,
        None = 255,
    }
    public enum FluidColor : int
    {
        Transparent = 0,
        Blue,
        Red,
        Brown,
        Green,
        Yellow,
        White,
        Purple
    }
    public enum FluidType : int
    {
        None = 0,
        Water,
        Mana,
        Beer,
        Oil,
        Blood,
        Slime,
        Mud,
        Lemonade,
        Milk,
        Wine,
        Health,
        Urine,
        Rum,
        FruidJuice,
        CoconutMilk,
        Tea,
        Mead
    }
    public enum HUDArcOrientation
    {
        Left,
        Right,
    }
    public enum MessageModeType : byte
    {
        None = 0,
        Say = 1,
        Whisper = 2,
        Yell = 3,
        PrivateFrom = 4,
        PrivateTo = 5,
        ChannelManagement = 6,
        Channel = 7,
        ChannelHighlight = 8,
        Spell = 9,
        NpcFrom = 10,
        NpcTo = 11,
        GamemasterBroadcast = 12,
        GamemasterChannel = 13,
        GamemasterPrivateFrom = 14,
        GamemasterPrivateTo = 15,
        Login = 16,
        Admin = 17,
        Game = 18,
        Failure = 19,
        Look = 20,
        DamageDealed = 21,
        DamageReceived = 22,
        Heal = 23,
        Exp = 24,
        DamageOthers = 25,
        HealOthers = 26,
        ExpOthers = 27,
        Status = 28,
        Loot = 29,
        TradeNpc = 30,
        Guild = 31,
        PartyManagement = 32,
        Party = 33,
        BarkLow = 34,
        BarkLoud = 35,
        Report = 36,
        HotkeyUse = 37,
        TutorialHint = 38,
        Thankyou = 39,
        Market = 40,
        Mana = 41,
        BoostedCreature = 42,
        BeyondLast = 43,

        // deprecated
        MonsterYell,
        MonsterSay,
        Red,
        Blue,
        RVRChannel,
        RVRAnswer,
        RVRContinue,
        GameHighlight,
        NpcFromStartBlock,

        LastMessage,
        Invalid = 255
    }
    public enum MessageScreenTargets : int
    {
        None = -1,
        BoxBottom = 0,
        BoxLow = 1,
        BoxHigh = 2,
        BoxTop = 3,
        BoxCoordinate = 4,
        EffectCoordinate = 5,
    }
    public enum MessageModeHeaders : int
    {
        None = -1,
        Say,
        Whisper,
        Yell,
        Spell,
        NpcFromStartBlock,
        NpcFrom,
    }
    public enum MessageModePrefixes : int
    {
        None = -1,
        PrivateFrom,
        GamemasterBroadcast,
        GamemasterPrivateFrom,
    }
    public enum ChannelEvent : byte
    {
        PlayerJoined = 0,
        PlayerLeft = 1,
        PlayerInvited = 2,
        PlayerExcluded = 3,
        PlayerPending = 4,
    }
    public enum MouseButton : byte
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
        Both = Left | Right,
    }
    public enum MousePresets : byte
    {
        Classic,
        Regular,
        LeftSmartClick,
    }
    public enum MouseLootPresets : int
    {
        Right,
        ShiftPlusRight,
        Left,
    }
    public enum HUDStyle : int
    {
        Bars = 0,
        Arcs = 1,
    }
    public enum CoulouriseLootValuesTypes : int
    {
        None = 0,
        Corners = 1,
        Frames = 2,
    }
    public enum AntialiasingMode : int
    {
        None = 0,
        Antialiasing = 1,
        SmoothRetro = 2,
    }

    public enum AppearanceActions : int
    {
        Unset = -1,
        None = 0,
        Attack = 1,
        AutoWalk = 3,
        AutoWalkHighlight = 4,
        ContextMenu = 5,
        Look = 6,
        Use = 7,
        Open = 8,
        Talk = 9,
        Loot = 10,
        SmartClick = 100,
    }
    public enum CombatAttackModes : int
    {
        Offensive = 1,
        Balanced = 2,
        Defensive = 3,
    }
    public enum CombatChaseModes : int
    {
        Off = 0,
        On = 1,
    }
    public enum CombatPvPModes : int
    {
        Dove = 0,
        WhiteHand = 1,
        YellowHand = 2,
        RedFist = 3,
    }
    public enum ClothSlots : byte
    {
        BothHands = 0,

        Head = 1,
        Neck = 2,
        Backpack = 3,
        Torso = 4,
        RightHand = 5,
        LeftHand = 6,
        Legs = 7,
        Feet = 8,
        Finger = 9,
        Hip = 10,

        // StoreInbox & Purse both represent the same slot in different client versions //
        StoreInbox = 11,
        Purse = 11,

        Store = 12,
        Blessings = 13,

        First = Head,
        Last = Blessings,
    }
    public enum ResourceType : byte
    {
        BankGold = 0,
        InventoryGold = 1,

        PreyBonusRerolls = 10,
        CollectionTokens = 20,
    }
    public enum ReportTypes : byte
    {
        Name = 0,
        Statement = 1,
        Bot = 2,
    }
    public enum RenderError
    {
        None,
        WorldMapNotValid,
        SizeNotEffecient,

        MiniMapNotValid,
        PositionNotValid,
    }
    public enum DeathType
    {
        DeathTypeRegular = 0x00,
        DeathTypeUnfair = 0x01,
        DeathTypeBlessed = 0x02,
        DeathTypeNoPenalty = 0x03,
    }
    public enum CursorState
    {
        Default = 0,
        DefaultDisabled,
        NResize,
        EResize,
        Hand,
        Crosshair,
        CrosshairDisabled,
        Scan,
        Attack,
        Walk,
        Use,
        Talk,
        Look,
        Open,
        Loot
    }

    public enum UseActionTarget
    {
        Auto = 0,
        NewWindow = 1,
        Self = 2,
        Target = 3,
        CrossHair = 4,
    }

    public enum CursorPriority
    {
        Low,
        Medium,
        High,
    }

    public enum OpponentFilters
    {
        None = 0,
        Players = 1 << 0,
        NPCs = 1 << 1,
        Monsters = 1 << 2,
        NonSkulled = 1 << 3,
        Party = 1 << 4,
        Summons = 1 << 5,
    }

    public enum OpponentSortType
    {
        SortDistanceDesc,
        SortDistanceAsc,
        SortHitpointsDesc,
        SortHitpointsAsc,
        SortNameDesc,
        SortNameAsc,
        SortKnownSinceDesc,
        SortKnownSinceAsc,
    }

    public enum DialogType
    {
        // TODO, these are no longer existing, remove them
        OptionsGeneral = 0,
        OptionsRenderer = 1,
        OptionsStatus = 2,
        OptionsMessage = 3,
        OptionsHotkey = 4,
        OptionsNameFilter = 5,
        OptionsMouseControl = 6,
        CharacterSpells = 7,
        CharacterProfile = 8,
        CharacterOutfit = 9,
        ChatChannelSelection = 10,
        HelpQuestLog = 11,
        PreyDialog = 12,
    }

    public enum BlessingTypes : uint
    {
        None = 0,
        Adventurer = 1 << 0,
        TwistOfFate = 1 << 1,
        WisdomOfSolitude = 1 << 2,
        SparkOfPhoenix = 1 << 3,
        FireOfSuns = 1 << 4,
        SpirtualShielding = 1 << 5,
        EmbraceOfTibia = 1 << 6,
        HeartOfTheMountain = 1 << 7,
        BloodOfTheMountain = 1 << 8,

        AllLegacy = WisdomOfSolitude | SparkOfPhoenix | FireOfSuns | SpirtualShielding | EmbraceOfTibia,
        All_1120 = Adventurer | TwistOfFate | AllLegacy,
        All_1141 = All_1120 | HeartOfTheMountain | BloodOfTheMountain,
    }

    public enum MarkType : byte
    {
        ClientMapWindow = 1,
        ClientBattleList = 2,
        OneSecondTemp = 3,
        Permenant = 4,

        None = 255,
    }

    public enum ObjectCategory : byte
    {
        Armors = 1,
        Amulets = 2,
        Boots = 3,
        Containers = 4,
        Decoration = 5,
        Food = 6,
        HelmetsHats = 7,
        Legs = 8,
        Others = 9,
        Potions = 10,
        Rings = 11,
        Runes = 12,
        Shields = 13,
        Tools = 14,
        Valuables = 15,
        Ammunition = 16,
        Axes = 17,
        Clubs = 18,
        DistanceWeapons = 19,
        Swords = 20,
        WandsRods = 21,
        PremiumScrolls = 22,
        TibiaCoins = 23,
        CreatureProducts = 24,
        StashRetrieve = 27,
        Unsorted = 28,
        AllWeapons = 29,
        Gold = 30,
        Default = 31, // links to Unassigned Loot (QuickLoot)

        MetaWeapons = 255,
        All = 255,
    }

    public enum MarketDetail : byte
    {
        Attack = 1,
        Capacity = 2,
        Defense = 3,
        Description = 4,
        Expire = 5,
        Protection = 6,
        RestrictLevel = 7,
        RestrictMagicLevel = 8,
        RestrictProfression = 9,
        RuneSpell = 10,
        SkillBoost = 11,
        Uses = 12,
        WeaponType = 13,
        Weight = 14,
        ImbuementSlots = 15,

        First = Attack,
        Last = ImbuementSlots,
    }

    public enum MarketOfferType : byte
    {
        Buy = 0,
        Sell = 1,
    }

    public enum MarketOfferState : byte
    {
        Active = 0,
        Cancelled = 1,
        Expired = 2,
        Accepted = 3,
    }

    public enum MessageDialogType : byte
    {
        ImbuementSuccess = 0,
        ImbuementError = 1,
        ImbuingStationNotFound = 2,
        ImbuementRollFailed = 3,

        ClearingCharmSuccess = 10,
        ClearingCharmError = 11,

        PreyMessage = 20,
        PreyError = 21,

        SupplyStashMessage = 30,

        DailyRewardError = 40,
    }

    public enum StoreServiceType : byte
    {
        Unknown = 0,
        CharacterNameChange = 1,
        Premium = 2,
        Outfits = 3,
        Mounts = 4,
        Blessings = 5,
        XPBoost = 6,
        Prey = 7,
    }

    public enum StoreOfferSortType : byte
    {
        Automatic = 0,
        MostPopular = 1,
        Alphabetically = 2,
        Newest = 3,
    }

    public enum StoreOfferType : byte
    {
        PreySlot = 0,
        PreyWildCard = 1,
        DailyReward = 2,
        CharmExpansion = 3,
        BlessingWisdomOfSolitude = 4,
        BlessingSparkOfThePhoniex = 5,
        BlessingFireOfTheSuns = 6,
        BlessingSpirtualShielding = 7,
        BlessingEmbraceOfTibia = 8,
        BlessingHeartOfTheMountain = 9,
        BlessingBloodOfTheMountain = 10,
        PremiumTime = 11,
        BlessingTwistOfFate = 12,
    }

    public enum StoreCategoryType : byte
    {
        PremiumTime = 0,
        XPBoost = 1,
        // 2 unknown
        // 3 unknown
        // 4 unknown
        // 5 unknown
        // 6 unknown
    }

    public enum StoreHighlightState : byte
    {
        None = 0,
        New = 1,
        Sale = 2,
        Timed = 3,
    }

    public enum StoreEvent : byte
    {
        SelectOffer = 0,
    }

    public enum StoreOpenParameterAction : byte
    {
        Invalid = 0,
        CategoryType = 1,
        CategoryAndFilter = 2,
        OfferType = 3,
        OfferId = 4,
        CategoryName = 5,
    }

    public enum StoreOfferDisableState : byte
    {
        None = 0,
        Disabled = 1,
        Hidden = 2,
    }

    public enum StoreOfferAppearanceType : byte
    {
        Icon = 0,
        Mount = 1, // no addons, black
        Outfit = 2, // colors included
        Object = 3,
    }

    public enum PreySlotStates : byte
    {
        Locked = 0,
        Inactive = 1,
        Active = 2,
        Selection = 3,
        SelectionChangeMonster = 4,
    }

    public enum PreyBonusTypes : byte
    {
        DamageBoost = 0,
        DamageReduction = 1,
        XpBonus = 2,
        ImprovedLoot = 3,
        None = 4,
    }

    public enum PreySlotUnlockType : byte
    {
        PremiumOrStore = 0,
        Store = 1,
        None = 2,
    }

    public enum PreyAction : byte
    {
        ListReroll = 0,
        BonusReroll = 1,
        SelectPrey = 2,
    }

    public enum DailyRewardType : byte
    {
        PickedItems = 1,
        FixedItems = 2,
    }

    public enum DailyRewardSubType : byte
    {
        Object = 1,
        PreyBonusRerolls = 2,
        FiftyPercentXpBoost = 3,
    }

    public enum FeatureEventType : byte
    {
        CyclopediaItems = 1,
        CyclopediaCharacterInfo = 2,
        CyclopediaCharacterGeneralStats = 3,
        CyclopediaCharacterCombatStats = 4,
        CyclopediaCharacterRecentDeaths = 5,
        CyclopediaCharacterRecentPvpKills = 6,
        CyclopediaCharacterAchievements = 7,
        CyclopediaCharacterItemSummary = 8,
        CyclopediaCharacterOutfitsAndMounts = 9,
        CyclopediaCharacterStoreSummary = 10,
    }

    public enum LootContainerAction : byte
    {
        AssignContainer = 0,
        ClearContainer = 1,
        // 2, unknown yet
        SetUseFallback = 3,
    }

    public enum QuickLootFilter : byte
    {
        SkippedLoot = 0,
        AcceptedLoot = 1,
    }

    public enum CyclopediaLootValueSource
    {
        NpcSaleData = 1,
        MarketAverageValue = 2,
    }

    public enum CyclopediaBonusEffectAction : byte
    {
        Unlock = 0,
        Select = 1,
        Clear = 2,
    }

    public enum CyclopediaRaceStage : byte
    {
        Locked = 0,
        Unlocked = 1,
        One = 2,
        Two = 3,
        FullyUnlocked = 4,
    }

    public enum CyclopediaRaceOccurence : byte
    {
        Common = 0,
        Uncommon = 1,
        SemiRare = 2,
        Rare = 3,
        VeryRare = 4,
    }

    public enum CyclopediaRaceDifficulty : byte
    {
        Harmless = 0,
        Trivial = 1,
        Easy = 2,
        Medium = 3,
        Hard = 4,
    }

    public enum CyclopediaRaceLootRarity : byte
    {
        Common = 0,
        Uncommon = 1,
        SemiRare = 2,
        Rare = 3,
        VeryRare = 4,
    }

    public enum CyclopediaRaceLootType : byte
    {
        One = 0,
        Several = 1,
    }

    public enum CyclopediaRaceAttackType : byte
    {
        Melee = 0,
        Distance = 1,
        NoAttack = 2,
    }

    public enum CyclopediaCombatType : byte
    {
        Physical = 0,
        Fire = 1,
        Poison = 2,
    }

    public enum CyclopediaCharacterInfoType : byte
    {
        BaseInformation = 0,
        GeneralStats = 1,
        CombatStats = 2,
        RecentDeaths = 3,
        RecentPvpKills = 4,
        Achievements = 5,
        ItemSummary = 6,
        OutfitsAndMounts = 7,
        StoreSummary = 8,
    }

    public enum CyclopediaPvpKillStatus : byte
    {
        Justified = 0,
        Unjustified = 1,
        GuildWar = 2,
        Assisted = 3,
        Arena = 4,
    }

    public enum InspectObjectTypes
    {
        NormalObject = 0,
        NpcTrade = 1,
        // 2 unknown yet
        Cyclopedia = 3,
    }

    public enum ClientInspectPlayerState
    {
        GrantPermission = 1,
        AskToInspect = 2,
        AllowToInspect = 3,
        InspectPlayer = 4,
        RevokePermission = 5,
        AllowAllToInspect = 6,
        RevokeAllToInspect = 7,
    }

    public enum ServerInspectPlayerState
    {
        None = 0,
        AllowedToInspectMe = 1 << 0,
        RequestedToInspectOther = 1 << 1,
        GrantedPermissionToInspectOther = 1 << 2,
        RequestedToInspectMe = 1 << 3,
    }

    public enum OutfitDialogType : byte
    {
        Normal = 0,
        TryOutfit = 1,
        TryMount = 2,
    }

    public enum OutfitLockType : byte
    {
        Unlocked = 0,
        Store = 1,
        GoldenOutfit = 2,
    }

    public enum GameFeature
    {
        GameDebugAssertion,
        GameOutfitIdU16,
        GameMessageStatements,
        GameLoginPacketEncryption,
        GamePlayerAddons,
        GamePlayerStamina,
        GameNewFluids,
        GameMessageLevel,
        GamePlayerStateU16,
        GameNewOutfitProtocol,
        GameWritableDate,
        GameNPCInterface,
        GameProtocolChecksum,
        GameAccountNames,
        GameDoubleFreeCapacity,
        GameChallengeOnLogin,
        GameMessageSizeCheck,
        GameCreatureEmblems,
        GameServerLog,
        GameAttackSeq,
        GamePenalityOnDeath,
        GameDoubleExperience,
        GamePlayerMounts,
        GameSpellList,
        GameNameOnNpcTrade,
        GameTotalCapacity,
        GameSkillsBase,
        GamePlayerRegenerationTime,
        GameChannelPlayerList,
        GameEnvironmentEffect,
        GameItemAnimationPhase,
        GamePlayerMarket,
        GamePurseSlot,
        GameClientPing,
        GameSpritesU32,
        GameOfflineTrainingTime,
        GameAdditionalVipInfo,
        GamePreviewState,
        GameClientVersion,
        GameLoginPending,
        GameNewSpeedLaw,
        GameContainerPagination,
        GameBrowseField,
        GameCreatureMarks,
        GameObjectMarks,
        GamePVPMode,
        GameDoubleSkills,
        GameBaseSkillU16,
        GameCreatureIcons,
        GameHideNpcNames,
        GamePremiumExpiration,
        GameEnhancedAnimations,
        GameUnjustifiedPoints,
        GameExperienceBonus,
        GameDeathType,
        GameSeparateAnimationGroups,
        GameOGLInformation,
        GameContentRevision,
        GameAuthenticator,
        GameSessionKey,
        GameEquipHotkey,
        GameIngameStore,
        GameWrappableFurniture,
        GameIngameStoreServiceType,
        GameStoreInboxSlot,
        GameIngameStoreHighlights,
        GameAdditionalSkills,
        GameExperienceGain,
        GameWorldProxyIdentification,
        GamePrey,
        GameImbuing,
        GameBuddyGroups,
        GameInspectionWindow,
        GameProtocolSequenceNumber,
        GameBlessingDialog,
        GameQuestTracker,
        GameCompendium,
        GamePlayerStateU32,
        GameRewardWall,
        GameAnalytics,
        GameQuickLoot,
        GameExtendedCapacity,
        GameCyclopediaMonsters,
        GameStash,
        GameCyclopediaMapAdditionalDetails,
        GamePercentSkillU16,
        GameTournament,
        GameAccountEmailAddress,

        LastGameFeature,
    };
}