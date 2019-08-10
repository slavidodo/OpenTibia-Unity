namespace OpenTibiaUnity
{
    internal static class Constants
    {
        internal const string RealTibiaClientServicesAddress = "https://www.tibia.com/services/clientservices.php";
        internal const string OpenTibiaDefaultClientServicesAddress = "http://127.0.0.1/clientservices.php";

        internal const string OpenTibiaDefaultFullIPAddress = "127.0.0.1:7171";
        internal const string OpenTibiaDefaultIPAddress = "127.0.0.1";
        internal const int OpenTibiaDefaultPort = 7171;

        // Don't modify those unless you know what you are doing.
        internal const int PingDelay = 1000;
        internal const int ConnectionTimeout = 30 * 1000;
        
        internal const int MapWidth = 15;
        internal const int MapHeight = 11;
        internal const int MapSizeW = 10;
        internal const int MapSizeX = MapWidth + 3;
        internal const int MapSizeY = MapHeight + 3;
        internal const int MapSizeZ = 8;
        internal const int MapMinX = 0; // Tibia restricted it to 24576 -> 24576 + (1 << 14 -1)
        internal const int MapMinY = 0;
        internal const int MapMaxX = MapMinX + 2 * (1 << 15 - 1);
        internal const int MapMaxY = MapMinY + 2 * (1 << 15 - 1);
        internal const int MapMinZ = 0;
        internal const int MapMaxZ = 15;

        internal const int PathMaxSteps = 128;
        internal const int PathMaxDistance = 110;
        internal const int PathMatrixCenter = PathMaxDistance;
        internal const int PathMatrixSize = 2 * PathMaxDistance + 1;

        internal const int PathCostMax = 250;
        internal const int PathCostUndefined = 254;
        internal const int PathCostObstacle = 255;

        internal const int NumEffects = 200;
        internal const int NumOnscreenMessages = 16;
        internal const int NumFields = MapSizeX * MapSizeY * MapSizeZ;
        internal const int FieldSize = 32;
        internal const int FieldCacheSize = 32;
        internal const int FieldHeight = 24;

        internal const int PlayerOffsetY = 6;
        internal const int PlayerOffsetX = 8;

        internal const int GroundLayer = 7;
        internal const int UndergroundLayer = 2;

        internal const int ObjectsUpdateInterval = 40;
        internal const int AmbientUpdateInterval = 1000;

        internal const int OnscreenMessageHeight = 195;
        internal const int OnscreenMessageWidth = 360;

        internal const int MaxTalkLength = 255;
        internal const int MaxChannelLength = 255;
        internal const int MaxContainerNameLength = 255;
        internal const int MaxContainerViews = 32;

        internal const int MiniMapSideBarViewHeight = 106;
        internal const int MiniMapSideBarViewWidth = 106;

        internal const int MiniMapSectorSize = 256;
        internal const int MiniMapSideBarZoomMin = -1;
        internal const int MiniMapSideBarZoomMax = 4;

        internal const int WorldMapScreenWidth = MapSizeX * FieldSize;
        internal const int WorldMapScreenHeight = MapSizeY * FieldSize;

        internal const int WorldMapRealWidth = MapWidth * FieldSize;
        internal const int WorldMapRealHeight = MapHeight * FieldSize;

        internal const int WorldMapMinimumWidth = (int)(WorldMapRealWidth * 0.6667f);
        internal const int WorldMapMinimumHeight = (int)(WorldMapRealHeight * 0.6667f);
        
        internal const int MaxNpcDistance = 3;

        internal const int LightmapShrinkFactor = 8;

        internal const uint MarkThicknessThin = 1;
        internal const uint MarkThicknessBold = 2;

        internal static UnityEngine.Color ColorAboveGround = new UnityEngine.Color32(200, 200, 255, 255);
        internal static UnityEngine.Color ColorBelowGround = new UnityEngine.Color32(255, 255, 255, 255);
        internal static UnityEngine.Color ObjectCursorColor = new UnityEngine.Color32(255, 225, 55, 255);

        internal const float HighlightMinOpacity = 0.3f;
        internal const float HighlightMaxOpacity = 0.6f;

        internal const uint PlayerStartID = 0x10000000;
        internal const uint PlayerEndID = 0x40000000;
        internal const uint MonsterStartID = 0x40000000;
        internal const uint MonsterEndID = 0x80000000;
        internal const uint NpcStartID = 0x80000000;
        internal const uint NpcEndID = 0xffffffff;

        internal const int AnimationDelayBeforeReset = 1000;
        internal const int PhaseAutomatic = -1;
        internal const int PhaseAsynchronous = 255;
        internal const int PhaseRandom = 254;

        internal const int MaxTalkHistory = 200;

        internal const int SecondsPerDay = 24 * 60 * 60;
        internal const int MarketRequestOwnOffers = 65534;
        internal const int MarketRequestOwnHistory = 65535;
    }

    internal enum ClientSpecification
    {
        Cipsoft,
        OpenTibia,
    }

    internal enum AppearanceCategory : byte
    {
        Object,
        Outfit,
        Effect,
        Missile,
    }

    internal enum EnterPossibleFlag : byte
    {
        Possible,
        PossibleNoAnimation,
        NotPossible,
    }

    internal enum PathState : byte
    {
        PathEmpty,
        PathExists,
        PathErrorGoDownstairs,
        PathErrorGoUpstairs,
        PathErrorTooFar,
        PathErrorUnreachable,
        PathErrorInternal,
    }
    internal enum PathDirection : int
    {
        East = 1,
        NorthEast = 2,
        North = 3,
        NorthWest = 4,
        West = 5,
        SouthWest = 6,
        South = 7,
        SouthEast = 8,
    }
    internal enum Direction : byte
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
    internal enum CreatureType : byte
    {
        Player,
        Monster,
        NPC,
        Summon,

        First = Player,
        Last = Summon,
    }
    internal enum PartyFlag : byte
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
    internal enum PKFlag : byte
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
    internal enum SummonTypeFlags : byte
    {
        None,
        Own,
        Other,

        First = None,
        Last = Other,
    }
    internal enum SpeechCategory : byte
    {
        None,
        Normal,
        Trader,
        Quest,
        QuestTrader,
        Travel,

        First = None,
        Last = Travel,
    }
    internal enum GuildFlag : byte
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

    internal enum SkillType : byte
    {
        Level = 0,
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

        // internal usage only
        ExperienceGain = 253,
        None = 254,
        Experience = 255,
    }
    internal enum States : int
    {
        None = -1,
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
    }
    internal enum FluidsColor : int
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
    internal enum FluidsType : int
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
    internal enum HUDArcOrientation
    {
        Left,
        Right,
    }
    internal enum MessageModeType : byte
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
        BeyondLast = 42,

        // deprecated
        MonsterYell = 43,
        MonsterSay = 44,
        Red = 45,
        Blue = 46,
        RVRChannel = 47,
        RVRAnswer = 48,
        RVRContinue = 49,
        GameHighlight = 50,
        NpcFromStartBlock = 51,
        LastMessage = 52,
        Invalid = 255
    }
    internal enum MessageScreenTargets : int
    {
        None = -1,
        BoxBottom = 0,
        BoxLow = 1,
        BoxHigh = 2,
        BoxTop = 3,
        BoxCoordinate = 4,
        EffectCoordinate = 5,
    }
    internal enum MessageModeHeaders : int
    {
        None = -1,
        Say,
        Whisper,
        Yell,
        Spell,
        NpcFromStartBlock,
        NpcFrom,
    }
    internal enum MessageModePrefixes : int
    {
        None = -1,
        PrivateFrom,
        GamemasterBroadcast,
        GamemasterPrivateFrom,
    }
    internal enum ChannelEvent : byte
    {
        PlayerJoined = 0,
        PlayerLeft = 1,
        PlayerInvited = 2,
        PlayerExcluded = 3,
        PlayerPending = 4,
    }
    internal enum MouseButton : byte
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
        Both = Left | Right,
    }
    internal enum MousePresets : byte
    {
        Classic,
        Regular,
        LeftSmartClick,
    }
    internal enum MouseLootPresets : int
    {
        Right,
        ShiftPlusRight,
        Left,
    }
    internal enum HUDStyles : int
    {
        Bars = 0,
        Arcs = 1,
    }
    internal enum CoulouriseLootValuesTypes : int
    {
        None = 0,
        Corners = 1,
        Frames = 2,
    }
    internal enum AntialiasingModes : int
    {
        None = 0,
        Antialiasing = 1,
        SmoothRetro = 2,
    }

    internal enum AppearanceActions : int
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
    internal enum CombatAttackModes : int
    {
        Offensive = 1,
        Balanced = 2,
        Defensive = 3,
    }
    internal enum CombatChaseModes : int
    {
        Off = 0,
        On = 1,
    }
    internal enum CombatPvPModes : int
    {
        Dove = 0,
        WhiteHand = 1,
        YellowHand = 2,
        RedFist = 3,
    }
    internal enum ClothSlots : byte
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
    internal enum ResourceTypes : int
    {
        BankGold = 0,
        InventoryGold = 1,

        PreyBonusRerolls = 10,
        CollectionTokens = 20,
    }
    internal enum ReportTypes : int
    {
        Name = 0,
        Statement = 1,
        Bot = 2,
    }
    internal enum PopupMenuType
    {
        NoButtons = 0,
        OK = 1 << 0,
        Cancel = 1 << 1,
        OKCancel = OK | Cancel,
    }
    internal enum RenderError
    {
        None,
        WorldMapNotValid,
        SizeNotEffecient,

        MiniMapNotValid,
        PositionNotValid,
    }
    internal enum DeathType
    {
        DeathTypeRegular = 0x00,
        DeathTypeUnfair = 0x01,
        DeathTypeBlessed = 0x02,
        DeathTypeNoPenalty = 0x03,
    }
    internal enum CursorState
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

    internal enum UseActionTarget
    {
        Auto = 0,
        NewWindow = 1,
        Self = 2,
        Target = 3,
        CrossHair = 4,
    }

    internal enum CursorPriority
    {
        Low,
        Medium,
        High,
    }

    internal enum OpponentFilters
    {
        None = 0,
        Players = 1 << 0,
        NPCs = 1 << 1,
        Monsters = 1 << 2,
        NonSkulled = 1 << 3,
        Party = 1 << 4,
        Summons = 1 << 5,
    }

    internal enum OpponentSortTypes
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

    internal enum OpponentStates
    {
        NoAction,
        Refresh,
        Rebuild,
    }

    internal enum DialogType
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

    internal enum BlessingTypes
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

    internal enum MarkType
    {
        ClientMapWindow = 1,
        ClientBattleList = 2,
        OneSecondTemp = 3,
        Permenant = 4,

        None = 255,
    }

    internal enum ObjectCategory
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
        Axes  = 17,
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

    internal enum MarketDetails
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

    internal enum MarketOfferTypes
    {
        Buy = 0,
        Sell = 1,
    }

    internal enum MarketOfferStates
    {
        Active = 0,
        Cancelled = 1,
        Expired = 2,
        Accepted = 3,
    }

    internal enum MessageDialog
    {
        ImbuementSuccess = 0,
        ImbuementError = 1,
        ImbuingStationNotFound = 2,
        ImbuementRollFailed = 3,

        ClearingCharmSuccess = 10,
        ClearingCharmError = 11,

        PreyMessage = 20,
        PreyError = 21,
    }

    internal enum StoreServiceType
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

    internal enum PreySlotStates : byte
    {
        Locked = 0,
        Inactive = 1,
        Active = 2,
        Selection = 3,
        SelectionChangeMonster = 4,
    }

    internal enum PreyBonusTypes : byte
    {
        DamageBoost = 0,
        DamageReduction = 1,
        XpBonus = 2,
        ImprovedLoot = 3,
        None = 4,
    }

    internal enum PreySlotUnlockType : byte
    {
        PremiumOrStore = 0,
        Store = 1,
        None = 2,
    }

    internal enum DailyRewardStates : byte
    {
        PickedItems = 1,
        FixedItems = 2,
    }

    internal enum DailyRewardTypes : byte
    {
        Object = 1,
        PreyBonusRerolls = 2,
        FiftyPercentXpBoost = 3,
    }

    internal enum CyclopediaRaceDifficulty : byte
    {
        Harmless = 0,
        Trivial = 1,
        Easy = 2,
        Medium = 3,
        Hard = 4,
    }

    internal enum CyclopediaRaceOccurence : byte
    {
        Common = 0,
        Uncommon = 1,
        SemiRare = 2,
        Rare = 3,
        VeryRare = 4,
    }

    internal enum CyclopediaRaceLootType : byte
    {
        One = 0,
        Several = 1,
    }

    internal enum CyclopediaRaceAttackType : byte
    {
        Melee = 0,
        Distance = 1,
        NoAttack = 2,
    }

    internal enum CyclopediaCombatType : byte
    {
        Physical = 0,
        Fire = 1,
        Poison = 2,
    }

    internal enum CyclopediaCharacterInfoType : byte
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
    
    internal enum CyclopediaPvpKillStatus : byte
    {
        Justified = 0,
        Unjustified = 1,
        GuildWar = 2,
        Assisted = 3,
        Arena = 4,
    }

    internal enum InspectObjectTypes
    {
        NormalObject = 0,
        Cyclopedia = 3,
    }

    internal enum GameFeature
    {
        GameProtocolChecksum,
        GameAccountNames,
        GameChallengeOnLogin,
        GamePenalityOnDeath,
        GameNameOnNpcTrade,
        GameDoubleFreeCapacity,
        GameDoubleExperience,
        GameTotalCapacity,
        GameSkillsBase,
        GamePlayerRegenerationTime,
        GameChannelPlayerList,
        GamePlayerMounts,
        GameEnvironmentEffect,
        GameCreatureEmblems,
        GameServerLog,
        GameItemAnimationPhase,
        GameMagicEffectU16,
        GamePlayerMarket,
        GameSpritesU32,
        GameOfflineTrainingTime,
        GamePurseSlot,
        GameFormatCreatureName,
        GameSpellList,
        GameClientPing,
        GameExtendedClientPing,
        GameDoubleHealth,
        GameDoubleSkills,
        GameChangeMapAwareRange,
        GameMapMovePosition,
        GameAttackSeq,
        GameBlueNpcNameColor,
        GameDiagonalAnimatedText,
        GameLoginPending,
        GameNewSpeedLaw,
        GameForceFirstAutoWalkStep,
        GameMinimapRemove,
        GameDoubleShopSellAmount,
        GameContainerPagination,
        GameCreatureMarks,
        GameObjectMarks,
        GameOutfitIDU16,
        GamePlayerStamina,
        GamePlayerAddons,
        GameMessageStatements,
        GameMessageLevel,
        GameNewFluids,
        GamePlayerStateU16,
        GameNewOutfitProtocol,
        GamePVPMode,
        GameWritableDate,
        GameAdditionalVipInfo,
        GameBaseSkillU16,
        GameCreatureIcons,
        GameHideNpcNames,
        GameSpritesAlphaChannel,
        GamePremiumExpiration,
        GameBrowseField,
        GameEnhancedAnimations,
        GameOGLInformation,
        GameMessageSizeCheck,
        GamePreviewState,
        GameLoginPacketEncryption,
        GameClientVersion,
        GameContentRevision,
        GameExperienceBonus,
        GameAuthenticator,
        GameUnjustifiedPoints,
        GameSessionKey,
        GameEquipHotkey,
        GameDeathType,
        GameSeparateAnimationGroups,
        GameKeepUnawareTiles,
        GameIngameStore,
        GameIngameStoreHighlights,
        GameIngameStoreServiceType,
        GameStoreInboxSlot,
        GameAdditionalSkills,
        GameExperienceGain,
        GameWorldProxyIdentification,
        GamePrey,
        GameImbuing,
        GameBuddyGroups,
        GameInspectionWindow,
        GameProtocolSequenceNumber,
        GameBlessingDialog,
        QuestTracker,
        GamePlayerStateU32,
        GameRewardWall,
        GameAnalytics,
        GameCyclopedia,
        GameQuickLoot,
        GameExtendedCapacity,
        GameCyclopediaMap,
        GameStash,
        GamePercentSkillU16,
        GameTournament,
        GameAccountEmailAddress,
        LastGameFeature,
    };
}