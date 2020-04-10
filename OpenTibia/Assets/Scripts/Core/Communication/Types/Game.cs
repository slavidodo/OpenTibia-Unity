namespace OpenTibiaUnity.Core.Communication.Types
{
    public enum GameclientMessageType
    {
        /** (Missing):
         * GameclientMessageClientCheck
         * SubscribeToUpdates
         * TournamentInformation
         * TournamentLeaderboard
         * SetHirelingName
         * FriendSystemAction
         * OpenTransactionDetails
         * Greet
         * TournamentTicketAction
         * OpenDepotSearch
         * CloseDepotSearch
         * DepotSearchType
         * OpenParentContainer
         * DepotSearchRetrieve
         * GetOfferDescription
         * Teleport
         */

        PendingGame = 10,
        EnterWorld = 15,
        QuitGame = 20,
        Unknown28 = 28, // 12.00 / suspect: ConnectionPingBack
        Ping = 29,
        PingBack = 30,
        PerformanceMetrics = 31,
        StashAction = 40,

        GoPath = 100,
        GoNorth = 101,
        GoEast = 102,
        GoSouth = 103,
        GoWest = 104,
        Stop = 105,
        GoNorthEast = 106,
        GoSouthEast = 107,
        GoSouthWest = 108,
        GoNorthWest = 109,
        TurnNorth = 111,
        TurnEast = 112,
        TurnSouth = 113,
        TurnWest = 114,
        // empty (115~118)
        EquipObject = 119,
        MoveObject = 120,
        LookInNpcTrade = 121,
        BuyObject = 122,
        SellObject = 123,
        CloseNpcTrade = 124,
        TradeObject = 125,
        LookTrade = 126,
        AcceptTrade = 127,
        RejectTrade = 128,
        // empty (129)
        UseObject = 130,
        UseTwoObjects = 131,
        UseOnCreature = 132,
        TurnObject = 133,
        // empty (134)
        CloseContainer = 135,
        UpContainer = 136,
        EditText = 137,
        EditList = 138,
        ToggleWrapState = 139,
        Look = 140,
        LookAtCreature = 141,
        JoinAggression = 142,
        QuickLoot = 143,
        LootContainer = 144,
        QuickLootBlackWhitelist = 145,
        OpenDepotSearch = 146,
        // empty (147~149)
        Talk = 150,
        GetChannels = 151,
        JoinChannel = 152,
        LeaveChannel = 153,
        PrivateChannel = 154,
        CloseNPCChannel = 158,
        // empty (159)
        SetTactics = 160,
        Attack = 161,
        Follow = 162,
        InviteToParty = 163,
        JoinParty = 164,
        RevokeInvitation = 165,
        PassLeadership = 166,
        LeaveParty = 167,
        ShareExperience = 168,
        // empty (169)
        OpenChannel = 170,
        InviteToChannel = 171,
        ExcludeFromChannel = 172,
        // empty (173~189)
        Cancel = 190,
        // empty (191~197)
        // unknown (198)
        // empty (199~202)
        BrowseField = 203,
        SeekInContainer = 204,
        InspectObject = 205,
        InspectPlayer = 206,
        BlessingsDialog = 207,
        TrackQuestflags = 208,
        MarketStatistics = 209,
        GetOutfit = 210,
        SetOutfit = 211,
        Mount = 212,
        ApplyImbuement = 213,
        ApplyClearingCharm = 214,
        CloseImbuingDialog = 215,
        OpenRewardWall = 216,
        DailyRewardHistory = 217,
        CollectDailyReward = 218,
        // empty (219)
        AddBuddy = 220,
        RemoveBuddy = 221,
        EditBuddy = 222,
        AddBuddyGroup = 223,
        MarkGameNewsAsRead = 224,
        OpenMonsterCyclopedia = 225, // 0 payload
        OpenMonsterCyclopediaMonsters = 226,
        OpenMonsterCyclopediaRace = 227,
        MonsterBonusEffectAction = 228,
        OpenCyclopediaCharacterInfo = 229,
        // empty (225~229)
        BugReport = 230,
        ThankYou = 231,
        ErrorFileEntry = 232, // removed in 11
        GetOfferDescription = 232, // 1180
        StoreEvent = 233,
        FeatureEvent = 234, // cyclopedia, analytics, ...
        PreyAction = 235,
        // empty (236)
        RequestResourceBalance = 237,
        // empty (238)
        TransferCurrency = 239,
        GetQuestLog = 240,
        GetQuestLine = 241,
        RuleViolationReport = 242,
        GetObjectInfo = 243,
        MarketLeave = 244,
        MarketBrowse = 245,
        MarketCreate = 246,
        MarketCancel = 247,
        MarketAccept = 248,
        AnswerModalDialog = 249,
        OpenStore = 250, // RequestStoreCategories in tibia 11.70
        RequestStoreOffers = 251,
        BuyStoreOffer = 252,
        OpenTransactionHistory = 253,
        GetTransactionHistory = 254,
    }

    public enum GameserverMessageType : byte
    {
        /** Missing Messages
         * * SpecialContainersAvailable
         * * HirelingNameChange
         * * TransactionDetails
         **/
        
        CreatureData = 3,
        Login_PendingState = 10,
        GMActions_ReadyForSecondaryConnection = 11,

        WorldEntered = 15,
        UpdateNeeded = 17, // removed in tibia 12
        LoginError = 20,
        LoginAdvice = 21,
        LoginWait = 22,
        LoginSuccess = 23,
        LoginToken = 24, // removed in tibia 12
        StoreButtonIndicators = 25,
        Ping = 29,
        PingBack = 30,
        Challenge = 31,
        Death = 40,
        Stash = 41,
        DepotTileState = 42,
        // empty (43~49)
        // custom (50~98)
        GameFirstMessageType = 50,
        OTClientExtendedOpcode = 50,
        // in tibia 12, 50~99 remains unused
        // end custom
        ClientCheck = 99, // 1121
        FullMap = 100,
        MapTopRow = 101,
        MapRightRow = 102,
        MapBottomRow = 103,
        MapLeftRow = 104,
        FieldData = 105, // UpdateTile
        CreateOnMap = 106,
        ChangeOnMap = 107,
        DeleteOnMap = 108,
        MoveCreature = 109,
        OpenContainer = 110,
        CloseContainer = 111,
        CreateInContainer = 112,
        ChangeInContainer = 113,
        DeleteInContainer = 114,
        // empty (115-116)
        ScreenshotEvent = 117,
        InspectionList = 118,
        InspectionState = 119,
        SetInventory = 120,
        DeleteInventory = 121,
        NpcOffer = 122,
        PlayerGoods = 123,
        CloseNpcTrade = 124,
        OwnOffer = 125,
        CounterOffer = 126,
        CloseTrade = 127,
        // empty(128~129)
        AmbientLight = 130,
        GraphicalEffect = 131, // in 12.03 this is called GraphicalEffects //
        TextEffect_RemoveGraphicalEffect = 132, // this removed in 9.00, but in 12.00 it was back as RemoveGraphicalEffect
        RemoveGraphicalEffect = 132,
        MissleEffect = 133, // in 12.03 this is unlikely to be missile effect
        CreatureMark = 134, // removed (idk when)
        Trappers = 135, // removed (idk when)
        CreatureHealth = 140,
        CreatureLight = 141,
        CreatureOutfit = 142,
        CreatureSpeed = 143,
        CreatureSkull = 144,
        CreatureShield = 145,
        CreatureUnpass = 146,
        CreatureMarks = 147,
        PlayerHelpers = 148, // removed in 11.85
        CreatureType = 149,
        EditText = 150,
        EditList = 151,
        GameNews = 152,
        // empty (153~154)
        BlessingsDialog = 155,
        Blessings = 156,
        Preset = 157,
        PremiumTrigger = 158,
        PlayerBasicData = 159,
        PlayerStats = 160,
        PlayerSkills = 161,
        PlayerStates = 162,
        ClearTarget = 163,
        SpellDelay = 164,
        SpellGroupDelay = 165,
        MultiUseDelay = 166,
        SetTactics = 167,
        SetStoreDeepLink = 168,
        RestingAreaState = 169,
        Talk = 170,
        Channels = 171,
        OpenChannel = 172,
        PrivateChannel = 173,
        EditGuildChannel = 174,
        // empty
        OpenOwnChannel = 178,
        CloseChannel = 179,
        TextMessage = 180,
        CancelWalk = 181,
        Wait = 182, // i think this is related to player movement
        UnjustifiedPoints = 183,
        PvpSituations = 184,
        // empty (185~189)
        TopFloor = 190,
        BottomFloor = 191,
        UpdateLootContainers = 192,
        PlayerDataTournament = 193, // 12.15
        // empty (194~195)
        TournamentInformation = 196, // 12.15
        TournamentLeaderboard = 197, // 12.15
        // empty (198~199)
        OutfitDialog = 200,
        MessageExivaSuppressed = 201, // 11.50
        UpdateExivaOptions = 202, // 11.50
        TransactionDetails = 203, // 12.03~
        ImpactTracking = 204,
        MarketStatistics = 205,
        ItemWasted = 206,
        ItemLooted = 207,
        TrackedQuestFlags = 208,
        KillTracking = 209,
        BuddyAdd = 210,
        BuddyState = 211,
        BuddyLogout_BuddyGroupData = 212,
        MonsterCyclopedia = 213,
        MonsterCyclopediaMonsters = 214,
        MonsterCyclopediaRace = 215,
        MonsterCyclopediaBonusEffects = 216,
        MonsterCyclopediaNewDetails = 217,
        CyclopediaCharacterInfo = 218,
        HirelingNameChange = 219, // 12.03~
        TutorialHint = 220,
        AutomapFlag_CyclopediaMapData = 221, // this packet was extended with cyclopedia map implementation
        DailyRewardCollectionState = 222,
        CreditBalance = 223, // store balance
        StoreError = 224,
        RequestPurchaseData = 225,
        OpenRewardWall = 226,
        CloseRewardWall = 227,
        DailyRewardBasic = 228,
        DailyRewardHistory = 229,
        PreyFreeListRerollAvailability = 230,
        PreyTimeLeft = 231,
        PreyData = 232,
        PreyPrices = 233,
        OfferDescription = 234,
        ImbuingDialogRefresh = 235,
        CloseImbuingDialog = 236,
        ShowMessageDialog = 237,
        ResourceBalance = 238,
        TibiaTime = 239,
        QuestLog = 240,
        QuestLine = 241,
        UpdatingStoreBalance = 242, // removed in 1100
        ChannelEvent = 243,
        ObjectInfo = 244,
        PlayerInventory = 245,
        MarketEnter = 246,
        MarketLeave = 247,
        MarketDetail = 248,
        MarketBrowse = 249,
        ShowModalDialog = 250,
        PremiumStore = 251, // StoreCategories
        PremiumStoreOffers = 252, // StoreOffers
        TransactionHistory = 253,
        StoreSuccess = 254,
    }
}
