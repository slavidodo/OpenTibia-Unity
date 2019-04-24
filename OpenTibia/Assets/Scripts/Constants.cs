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
        public const int MapSizeX = MapWidth + 3; // 18
        public const int MapSizeY = MapHeight + 3; // 14
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

        public const uint PlayerStartID = 0x10000000;
        public const uint PlayerEndID = 0x40000000;
        public const uint MonsterStartID = 0x40000000;
        public const uint MonsterEndID = 0x80000000;
        public const uint NpcStartID = 0x80000000;
        public const uint NpcEndID = 0xffffffff;

        public const int AnimationDelayBeforeReset = 1000;
        public const int PhaseAutomatic = -1;
        public const int PhaseAsynchronous = 255;
        public const int PhaseRandom = 254;
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

        First = None,
        Last = Other,
    }
    public enum SpeechCategories : byte
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
    public enum GuildFlags : byte
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
    public enum States
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
    public enum MouseButtons : byte
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
    public enum ClothSlots : int
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

    public enum MarkTypes
    {
        ClientMapWindow = 1,
        ClientBattleList = 2,
        OneSecondTemp = 3,
        Permenant = 4,

        None = 255,
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
        GameSeparateAnimationGroups = 71,
        GameKeepUnawareTiles = 72,
        GameIngameStore = 73,
        GameIngameStoreHighlights = 74,
        GameIngameStoreServiceType = 75,
        GameAdditionalSkills = 76,
        GameExperienceGain = 77,
        GameWorldName = 78,
        GameProtocolSequenceNumber = 79,

        LastGameFeature = 101,
    };
}