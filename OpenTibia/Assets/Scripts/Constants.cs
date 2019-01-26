namespace OpenTibiaUnity
{
    public static class Constants
    {
        public const string LocalHostIP = "127.0.0.1";
        public const int LocalHostLoginPort = 7171;
        
        public const ushort ProtocolVersion = 1100;
        public const uint ClientVersion = 100000019;

        public const ushort ContentRevision = 0x1165;
        public const uint DatSignature = 0x154554;
        public const uint SprSignature = 0x154554;
        public const uint PicSignature = 0x12564;
        
        // Don't modify those unless you know what you are doing.
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

        public const uint FieldEnterPossible = 0;
        public const uint FieldEnterPossibleNoAnimation = 1;
        public const uint FieldEnterNotPossible = 2;

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

        // Legacy Things
        public const uint PlayerStartID = 0x10000000;
        public const uint PlayerEndID = 0x40000000;
        public const uint MonsterStartID = 0x40000000;
        public const uint MonsterEndID = 0x80000000;
        public const uint NpcStartID = 0x80000000;
        public const uint NpcEndID = 0xffffffff;
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
    public enum Directions : byte
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
    public enum CreatureTypes : byte
    {
        Player,
        Monster,
        NPC,
        Summon,

        First = Player,
        Last = Summon,
    }
    public enum PartyFlags : byte
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
    public enum PKFlags : byte
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
    }
    public enum SpeechCategories : byte
    {
        None,
        Normal,
        Trader,
        Quest,
        QuestTrader,
        Travel,
    }
    public enum GuildFlags : byte
    {
        None,
        WarAlly,
        WarEnemy,
        WarNeutral,
        Member,
        Other
    }
    public enum SkillTypes : int
    {
        ExperienceGain = -2,
        None,
        Experience,
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
    }
    public enum HUDArcOrientation
    {
        Left,
        Right,
    }
    public enum MessageModes : byte
    {
        None = 0,
        Say = 1,
        Whisper = 2,
        Yell = 3,
        PrivateFrom = 4,
        PrivateTo = 5,
        ChannelManagment = 6,
        Channel = 7,
        ChannelHighlight = 8,
        Spell = 9,
        NpcFromStartBlock = 10,
        NpcFrom = 11,
        NpcTo = 12,
        GamemasterBroadcast = 13,
        GamemasterChannel = 14,
        GamemasterPrivateFrom = 15,
        GamemasterPrivateTo = 16,
        Login = 17,
        Admin = 18,
        Game = 19,
        GameHighlight = 20,
        Failure = 21,
        Look = 22,
        DamageDealed = 23,
        DamageReceived = 24,
        Heal = 25,
        Exp = 26,
        DamageOthers = 27,
        HealOthers = 28,
        ExpOthers = 29,
        Status = 30,
        Loot = 31,
        TradeNpc = 32,
        Guild = 33,
        PartyManagement = 34,
        Party = 35,
        BarkLow = 36,
        BarkLoud = 37,
        Report = 38,
        HotkeyUse = 39,
        TutorialHint = 40,
        Thankyou = 41,
        Market = 42,
        Mana = 43,

        BeyondLast = 44,
        Last = Mana,
        Invalid = 255,
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
    public enum MouseButtons : byte
    {
        None = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Middle = 1 << 3,
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
        ShiftPlusLeft,
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
        SmartClick = 100,
        UseOrOpen = 101,
        AttackOrTalk = 102,
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
    public enum ClothSlots : int
    {
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
        StoreInbox = 11,
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
        Attack = 3,
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

    public enum GameFeatures
    {
        GameProtocolChecksum = 1,
        GameAccountNames = 2,
        GameChallengeOnLogin = 3,
        GamePenalityOnDeath = 4,
        GameNameOnNpcTrade = 5,
        GameDoubleFreeCapacity = 6,
        GameDoubleExperience = 7,
        GameTotalCapacity = 8,
        GameSkillsBase = 9,
        GamePlayerRegenerationTime = 10,
        GameChannelPlayerList = 11,
        GamePlayerMounts = 12,
        GameEnvironmentEffect = 13,
        GameCreatureEmblems = 14,
        GameItemAnimationPhase = 15,
        GameMagicEffectU16 = 16,
        GamePlayerMarket = 17,
        GameSpritesU32 = 18,
        // 19 unused
        GameOfflineTrainingTime = 20,
        GamePurseSlot = 21,
        GameFormatCreatureName = 22,
        GameSpellList = 23,
        GameClientPing = 24,
        GameExtendedClientPing = 25,
        GameDoubleHealth = 28,
        GameDoubleSkills = 29,
        GameChangeMapAwareRange = 30,
        GameMapMovePosition = 31,
        GameAttackSeq = 32,
        GameBlueNpcNameColor = 33,
        GameDiagonalAnimatedText = 34,
        GameLoginPending = 35,
        GameNewSpeedLaw = 36,
        GameForceFirstAutoWalkStep = 37,
        GameMinimapRemove = 38,
        GameDoubleShopSellAmount = 39,
        GameContainerPagination = 40,
        GameThingMarks = 41,
        GameLooktypeU16 = 42,
        GamePlayerStamina = 43,
        GamePlayerAddons = 44,
        GameMessageStatements = 45,
        GameMessageLevel = 46,
        GameNewFluids = 47,
        GamePlayerStateU16 = 48,
        GameNewOutfitProtocol = 49,
        GamePVPMode = 50,
        GameWritableDate = 51,
        GameAdditionalVipInfo = 52,
        GameBaseSkillU16 = 53,
        GameCreatureIcons = 54,
        GameHideNpcNames = 55,
        GameSpritesAlphaChannel = 56,
        GamePremiumExpiration = 57,
        GameBrowseField = 58,
        GameEnhancedAnimations = 59,
        GameOGLInformation = 60,
        GameMessageSizeCheck = 61,
        GamePreviewState = 62,
        GameLoginPacketEncryption = 63,
        GameClientVersion = 64,
        GameContentRevision = 65,
        GameExperienceBonus = 66,
        GameAuthenticator = 67,
        GameUnjustifiedPoints = 68,
        GameSessionKey = 69,
        GameDeathType = 70,
        GameIdleAnimations = 71,
        GameKeepUnawareTiles = 72,
        GameIngameStore = 73,
        GameIngameStoreHighlights = 74,
        GameIngameStoreServiceType = 75,
        GameAdditionalSkills = 76,
        GameWorldName = 77,
        GameProtocolSequenceNumber = 78,

        LastGameFeature = 101,
    };
}