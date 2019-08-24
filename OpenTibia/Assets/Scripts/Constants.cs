namespace OpenTibiaUnity
{
    public static class Constants
    {
        public const string RealTibiaClientServicesAddress = "https://www.tibia.com/services/clientservices.php";
        public const string OpenTibiaDefaultClientServicesAddress = "http://127.0.0.1/clientservices.php";

        public const string OpenTibiaDefaultFullIPAddress = "127.0.0.1:7171";
        public const string OpenTibiaDefaultIPAddress = "127.0.0.1";
        public const int OpenTibiaDefaultPort = 7171;

        // Don't modify those unless you know what you are doing.
        public const int PingDelay = 1000;
        public const int ConnectionTimeout = 30 * 1000;
        
        public const int MapWidth = 15;
        public const int MapHeight = 11;
        public const int MapSizeW = 10;
        public const int MapSizeX = MapWidth + 3;
        public const int MapSizeY = MapHeight + 3;
        public const int MapSizeZ = 8;
        public const int MapMinX = 0; // Tibia restricted it to 24576 -> 24576 + (1 << 14 -1)
        public const int MapMinY = 0;
        public const int MapMaxX = MapMinX + 2 * (1 << 15 - 1);
        public const int MapMaxY = MapMinY + 2 * (1 << 15 - 1);
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

        public const int PlayerOffsetY = 6;
        public const int PlayerOffsetX = 8;

        public const int GroundLayer = 7;
        public const int UndergroundLayer = 2;

        public const int ObjectsUpdateInterval = 40;
        public const int AmbientUpdateInterval = 1000;

        public const int OnscreenMessageHeight = 195;
        public const int OnscreenMessageWidth = 360;

        public const int MaxTalkLength = 255;
        public const int MaxChannelLength = 255;
        public const int MaxContainerNameLength = 255;
        public const int MaxContainerViews = 32;

        public const int MiniMapSideBarViewHeight = 106;
        public const int MiniMapSideBarViewWidth = 106;

        public const int MiniMapSectorSize = 256;
        public const int MiniMapSideBarZoomMin = -1;
        public const int MiniMapSideBarZoomMax = 4;

        public const int WorldMapScreenWidth = MapSizeX * FieldSize;
        public const int WorldMapScreenHeight = MapSizeY * FieldSize;

        public const int WorldMapRealWidth = MapWidth * FieldSize;
        public const int WorldMapRealHeight = MapHeight * FieldSize;

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
        PathErrorpublic,
    }
    public enum PathDirection : int
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
    public enum PKFlag : byte
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
    public enum SummonTypeFlags : byte
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

        First = None,
        Last = Travel,
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

    public enum SkillType : byte
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

        // public usage only
        ExperienceGain = 253,
        None = 254,
        Experience = 255,
    }
    public enum States : int
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
    public enum FluidsColor : int
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
    public enum FluidsType : int
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
    public enum HUDStyles : int
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
    public enum AntialiasingModes : int
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
    public enum ResourceTypes : int
    {
        BankGold = 0,
        InventoryGold = 1,

        PreyBonusRerolls = 10,
        CollectionTokens = 20,
    }
    public enum ReportTypes : int
    {
        Name = 0,
        Statement = 1,
        Bot = 2,
    }
    public enum PopupMenuType
    {
        NoButtons = 0,
        OK = 1 << 0,
        Cancel = 1 << 1,
        OKCancel = OK | Cancel,
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

    public enum OpponentSortTypes
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

    public enum OpponentStates
    {
        NoAction,
        Refresh,
        Rebuild,
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

    public enum BlessingTypes
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

    public enum MarkType
    {
        ClientMapWindow = 1,
        ClientBattleList = 2,
        OneSecondTemp = 3,
        Permenant = 4,

        None = 255,
    }

    public enum ObjectCategory
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

    public enum MarketDetails
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

    public enum MarketOfferTypes
    {
        Buy = 0,
        Sell = 1,
    }

    public enum MarketOfferStates
    {
        Active = 0,
        Cancelled = 1,
        Expired = 2,
        Accepted = 3,
    }

    public enum MessageDialog
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

    public enum StoreServiceType
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

    public enum DailyRewardStates : byte
    {
        PickedItems = 1,
        FixedItems = 2,
    }

    public enum DailyRewardTypes : byte
    {
        Object = 1,
        PreyBonusRerolls = 2,
        FiftyPercentXpBoost = 3,
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
        Cyclopedia = 3,
    }

    public enum GameFeature
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
        GameContainerPagination,
        GameCreatureMarks,
        GameObjectMarks,
        GameOutfitIdU16,
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