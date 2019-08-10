using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Core
{
    [DisallowMultipleComponent]
    internal class GameManager : MonoBehaviour {
        internal class TacticsChangeEvent : UnityEvent<CombatAttackModes, CombatChaseModes, bool, CombatPvPModes> { }
        internal class VersionChangeEvent : UnityEvent<int, int> { }
        internal class ClientSpecificationChangeEvent : UnityEvent<ClientSpecification, ClientSpecification> { }
        internal class RequestChatSendEvent : UnityEvent<string, bool, int> { }
        internal class RequestOutfitDialogEvent : UnityEvent<Appearances.AppearanceInstance, Appearances.AppearanceInstance, List<Communication.Game.ProtocolOutfit>, List<Communication.Game.ProtocolMount>> { }
        internal class RequestNPCTradeEvent : UnityEvent<string, List<Trade.TradeObjectRef>, List<Trade.TradeObjectRef>> { }
        internal class RequestTradeOfferEvent : UnityEvent<string, List<Appearances.ObjectInstance>> { }

        /// <summary>
        /// Game Manager should only exist once..
        /// </summary>
        internal static GameManager Instance { get; private set; }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        internal static extern bool SetWindowText(System.IntPtr hwnd, string lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        internal static extern System.IntPtr FindWindow(string className, string windowName);
#endif

        // Serialiazable fields
        // Prefabs
        [Header("Prefabs")]
        [SerializeField] internal TMPro.TextMeshProUGUI DefaultLabel = null;
        [SerializeField] internal Button DefaultButton = null;
        [SerializeField] internal Button DefaulBlueButton = null;
        [SerializeField] internal Button DefaulGreenButton = null;
        [SerializeField] internal Button DefaulYellowButton = null;
        [SerializeField] internal GameObject HorizontalSeparator = null;
        [SerializeField] internal GameObject VerticalSeparator = null;
        [SerializeField] internal GameObject MiniWindowShadowVariant = null;
        [SerializeField] internal Components.ExitWindow ExitWindowPrefab = null;
        [SerializeField] internal Components.PopupWindow PopupWindowPrefab = null;
        [SerializeField] internal Components.CreatureStatusPanel CreatureStatusPanelPrefab = null;
        [SerializeField] internal TMPro.TextMeshProUGUI LabelOnscreenMessageBoxPrefab = null;
        [SerializeField] internal Components.SplitStackWindow SplitStackWindowPrefab = null;
        [SerializeField] internal GameObject ContextMenuBasePrefab = null;
        [SerializeField] internal LayoutElement ContextMenuItemPrefab = null;
        [SerializeField] internal Components.CheckboxWrapper PanelCheckBox = null;

        // UI Default Objects
        [Header("Canvases & Overlays")]
        [SerializeField] internal Canvas BackgroundCanvas = null;
        [SerializeField] internal Canvas BackgroundPanelBlocker = null;
        [SerializeField] internal Canvas GameCanvas = null;
        [SerializeField] internal Canvas GamePanelBlocker = null;
        [SerializeField] internal EventSystem EventSystem = null;

        [Header("Static Panels")]
        [SerializeField] internal RectTransform LobbyPanel = null;
        [SerializeField] internal RectTransform WorldMapRenderingPanel = null;
        [SerializeField] internal RectTransform OnscreenMessagesContainer = null;
        [SerializeField] internal RectTransform CreatureStatusContainer = null;

        [Header("Windows")]
        [SerializeField] internal Components.Base.Window LoadingAppearancesWindow = null;

        [Header("Materials")]
        [SerializeField] internal Material AppearanceTypeMaterial = null;
        [SerializeField] internal Material OutfitTypeMaterial = null;
        [SerializeField] internal Material MarksViewMaterial = null;
        [SerializeField] internal Material LightmapMaterial = null;
        [SerializeField] internal Material LightSurfaceMaterial = null;

        [Header("Render Textures")]
        [SerializeField] internal CustomRenderTexture WorldMapRenderingTexture = null;
        [SerializeField] internal RenderTexture MiniMapRenderingTexture = null;

        [Header("Main Elements")]
        [SerializeField] internal Camera MainCamera = null;

        [Header("Onscreen Static Labels")]
        [SerializeField] internal TMPro.TextMeshProUGUI LabelMessageBoxTop = null;
        [SerializeField] internal TMPro.TextMeshProUGUI LabelMessageBoxHigh = null;
        [SerializeField] internal TMPro.TextMeshProUGUI LabelMessageBoxLow = null;
        [SerializeField] internal TMPro.TextMeshProUGUI LabelMessageBoxBottom = null;

        [Header("Utilities")]
        [SerializeField] internal Utility.CursorController CursorController = null;

        [Header("Textures & Sprites")]
        [SerializeField] internal Texture2D MarksViewTexture = null;
        [SerializeField] internal Texture2D TileCursorTexture = null;
        [SerializeField] internal Sprite[] PartySprites = null;
        [SerializeField] internal Sprite[] PKSprites = null;
        [SerializeField] internal Sprite[] TypeSprites = null;
        [SerializeField] internal Sprite[] SpeechSprites = null;
        [SerializeField] internal Sprite[] GuildSprites = null;

        // Properties
        internal Options.OptionStorage OptionStorage { get; private set; }
        internal Input.InputHandler InputHandler { get; private set; }
        internal Appearances.AppearanceStorage AppearanceStorage { get; private set; }
        internal Creatures.CreatureStorage CreatureStorage { get; private set; }
        internal MiniMap.MiniMapStorage MiniMapStorage { get; private set; }
        internal MiniMap.Rendering.MiniMapRenderer MiniMapRenderer { get; private set; }
        internal WorldMap.WorldMapStorage WorldMapStorage { get; private set; }
        internal WorldMap.Rendering.WorldMapRenderer WorldMapRenderer { get; private set; }
        internal Chat.ChatStorage ChatStorage { get; private set; }
        internal Chat.MessageStorage MessageStorage { get; private set; }
        internal Container.ContainerStorage ContainerStorage { get; private set; }
        internal Magic.SpellStorage SpellStorage { get; private set; }
        internal Communication.Game.ProtocolGame ProtocolGame { get; set; }
        internal int PendingCharacterIndex { get; set; } = -1;
        internal UnityEvent onSecondaryTimeCheck { get; private set; }
        internal TacticsChangeEvent onTacticsChangeEvent { get; private set; }
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        internal System.IntPtr WindowPtr { get; private set; }
#endif
        internal System.Collections.BitArray Features { get; private set; }
        internal ClientSpecification ClientSpecification { get; private set; } = ClientSpecification.Cipsoft;
        internal int ProtocolVersion { get; private set; } = 0;
        internal int ClientVersion { get; private set; } = 0;
        internal int BuildVersion { get; private set; } = 0; // tibia 11.xx.(xxxx)
        internal VersionChangeEvent onClientVersionChange { get; private set; }
        internal VersionChangeEvent onProtocolVersionChange { get; private set; }
        internal VersionChangeEvent onBuildVersionChange { get; private set; }
        internal ClientSpecificationChangeEvent onClientSpecificationChange { get; private set; }
        internal UnityEvent onGameStart { get; private set; }
        internal UnityEvent onGameEnd { get; private set; }
        internal UnityEvent onProcessChangeCharacter { get; private set; }
        internal UnityEvent onRequestShowOptionsHotkey { get; private set; }
        internal RequestChatSendEvent onRequestChatSend { get; private set; }
        internal RequestOutfitDialogEvent onRequestOutfitDialog { get; private set; }
        internal RequestNPCTradeEvent onRequestNPCTrade { get; private set; }
        internal UnityEvent onRequestCloseNPCTrade { get; private set; }
        internal RequestTradeOfferEvent onRequestOwnOffer { get; private set; }
        internal RequestTradeOfferEvent onRequestCounterOffer { get; private set; }
        internal UnityEvent onRequestCloseTrade { get; private set; }

        internal Canvas ActiveBlocker {
            get {
                if (BackgroundCanvas && BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundPanelBlocker;
                return GameCanvas ? GamePanelBlocker : null;
            }
        }

        internal Canvas ActiveCanvas {
            get => BackgroundCanvas && BackgroundCanvas.gameObject.activeSelf ? BackgroundCanvas : (GameCanvas ? GameCanvas : null);
        }

        internal GraphicRaycaster ActiveRaycaster {
            get => ActiveCanvas ? ActiveCanvas.GetComponent<GraphicRaycaster>() : null;
        }

        internal bool IsGameRunning {
            get => ProtocolGame != null && ProtocolGame.IsGameRunning;
        }
        
        internal bool IsRealTibia { get => ClientSpecification == ClientSpecification.Cipsoft; }
        internal bool IsOpenTibia { get => ClientSpecification == ClientSpecification.OpenTibia; }

        internal bool IsLoadingClientAssets { get => m_LoadingClientAssets; }
        internal bool HasLoadedClientAssets { get => m_LoadedClientAssets; }

        internal int ShouldSendPingAt { get; set; } = -1;

        // private fields
        private Queue<UnityAction> m_ActionQueue;
        private Components.ExitWindow m_ExitWindow;
        private bool m_LastMiniMapSaveWasSuccessfull = false;

        private bool m_LoadingClientAssets = false;
        private bool m_LoadedClientAssets = false;
        private int m_LoadedClientVersion = 0;
        private int m_LoadedBuildVersion = 0;

        private List<Components.Base.Module> m_Modules;

        private void Awake() {
            // setup static fields
            Instance = this;
            OpenTibiaUnity.GraphicsVendor = SystemInfo.graphicsDeviceVendor;
            OpenTibiaUnity.GraphicsDevice = SystemInfo.graphicsDeviceName;
            OpenTibiaUnity.GraphicsVersion = SystemInfo.graphicsDeviceVersion;
            OpenTibiaUnity.MainThread = Thread.CurrentThread;

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            WindowPtr = FindWindow(null, "OpenTibiaUnity");
#endif
            Features = new System.Collections.BitArray(new bool[(int)GameFeature.LastGameFeature]);

            // setup threading mutex & lockers
            m_ActionQueue = new Queue<UnityAction>();

            // quit-notification related
            m_ExitWindow = null;
            OpenTibiaUnity.Quiting = false;

            // events
            onSecondaryTimeCheck = new UnityEvent();
            onTacticsChangeEvent = new TacticsChangeEvent();
            onClientVersionChange = new VersionChangeEvent();
            onProtocolVersionChange = new VersionChangeEvent();
            onBuildVersionChange = new VersionChangeEvent();
            onClientSpecificationChange = new ClientSpecificationChangeEvent();
            onGameStart = new UnityEvent();
            onGameEnd = new UnityEvent();
            onProcessChangeCharacter = new UnityEvent();
            onRequestShowOptionsHotkey = new UnityEvent();
            onRequestChatSend = new RequestChatSendEvent();
            onRequestOutfitDialog = new RequestOutfitDialogEvent();
            onRequestNPCTrade = new RequestNPCTradeEvent();
            onRequestCloseNPCTrade = new UnityEvent();
            onRequestOwnOffer = new RequestTradeOfferEvent();
            onRequestCounterOffer = new RequestTradeOfferEvent();
            onRequestCloseTrade = new UnityEvent();

            // setup ingame facilities
            OptionStorage = new Options.OptionStorage();
            InputHandler = new Input.InputHandler();
            AppearanceStorage = new Appearances.AppearanceStorage();
            CreatureStorage = new Creatures.CreatureStorage();
            MiniMapStorage = new MiniMap.MiniMapStorage();
            MiniMapRenderer = new MiniMap.Rendering.MiniMapRenderer();
            WorldMapStorage = new WorldMap.WorldMapStorage(
                LabelMessageBoxBottom,
                LabelMessageBoxLow,
                LabelMessageBoxHigh,
                LabelMessageBoxTop,
                LabelOnscreenMessageBoxPrefab);
            WorldMapRenderer = new WorldMap.Rendering.WorldMapRenderer();
            ChatStorage = new Chat.ChatStorage(OptionStorage);
            MessageStorage = new Chat.MessageStorage();
            ContainerStorage = new Container.ContainerStorage();
            SpellStorage = new Magic.SpellStorage();

            // Load options
            OptionStorage.LoadOptions();

            // update input settings
            InputHandler.UpdateMapping();

            // initialize rendering textures
            WorldMapRenderingTexture.Initialize();
            WorldMapRenderingTexture.IncrementUpdateCount();
            
            // initialize core game actions
            Game.ObjectMultiUseHandler.Initialize();

            m_Modules = new List<Components.Base.Module>();
        }

        private void Start() {
            InvokeRepeating("SecondaryTimerCheck", 0, 0.05f);
            InvokeRepeating("SaveMiniMap", 0, 0.5f);
            
#if !UNITY_EDITOR
            Application.wantsToQuit += Application_wantsToQuit;
#endif

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 1000;
        }

        private void FixedUpdate() {
            DequeueMainThreadActions();

            if (ShouldSendPingAt != -1 && OpenTibiaUnity.TicksMillis >= ShouldSendPingAt) {
                ShouldSendPingAt = -1;
                if (IsGameRunning)
                    ProtocolGame.SendPing();
            }
        }

        private void OnGUI() {
            // use() can't be performed on a "layout" and "repaint"
            var e = Event.current;
            if (e.type == EventType.Layout || e.type == EventType.Repaint)
                return;

            if ((e.type == EventType.KeyDown || e.type == EventType.KeyUp) && e.keyCode != KeyCode.None) {
                InputHandler.OnKeyEvent(e);
                return;
            } else if (e.type == EventType.MouseDown || e.type == EventType.MouseUp) {
                InputHandler.OnMouseEvent(e);
            } else if (e.type == EventType.MouseMove) {
                // TODO: mouseMove
            } else if (e.type == EventType.MouseDrag) {
                InputHandler.OnDragEvent(e);
            }
            
            // avoid concurrent actions on the same input module //
            var ctrl = (e.modifiers & EventModifiers.Control) != 0;
            var alt = (e.modifiers & EventModifiers.Alt) != 0;
            if (ctrl || alt)
                e.Use();
        }

        private void OnDestroy() {
            // since we are using threads to do our work (networking, ..)
            // away of unity's default classes, we need to shutdown things
            // to avoid errors in the console while debugging

            if (ProtocolGame != null && ProtocolGame.IsGameRunning)
                ProtocolGame.Disconnect(); // force disconnection (by trying to logout)

            AppearanceStorage.UnloadSpriteProvider();
            AssetBundle.UnloadAllAssetBundles(true);

            onSecondaryTimeCheck.RemoveAllListeners();
            CancelInvoke("SecondaryTimerCheck");
            CancelInvoke("SaveMiniMap");

            OptionStorage.SaveOptions();

            OptionStorage = null;
            InputHandler = null;
            AppearanceStorage = null;
            CreatureStorage = null;
            MiniMapStorage = null;
            MiniMapRenderer = null;
            WorldMapStorage = null;
            WorldMapRenderer = null;
            ChatStorage = null;
            MessageStorage = null;

            WorldMapRenderingTexture.Release();
            MiniMapRenderingTexture.Release();
        }

        internal void AddSecondaryTimerListener(UnityAction action) {
            onSecondaryTimeCheck.AddListener(action);
        }

        internal void RemoveSecondaryTimerListener(UnityAction action) {
            onSecondaryTimeCheck.RemoveListener(action);
        }

        private void SecondaryTimerCheck() {
            onSecondaryTimeCheck.Invoke();
        }

        internal void SaveMiniMap() {
            if (!IsGameRunning) {
                if (!m_LastMiniMapSaveWasSuccessfull)
                    return;
                else
                    m_LastMiniMapSaveWasSuccessfull = false;
            } else {
                m_LastMiniMapSaveWasSuccessfull = true;
            }

            MiniMapStorage.OnIOTimer();
        }

        internal void InvokeOnMainThread(UnityAction action) {
            lock (m_ActionQueue)
                m_ActionQueue.Enqueue(action);
        }

        internal void DequeueMainThreadActions() {
            while (true) {
                UnityAction action;
                lock (m_ActionQueue) {
                    if (m_ActionQueue.Count == 0)
                        break;

                    action = m_ActionQueue.Dequeue();
                }

                action.Invoke();
            }
        }

        private bool Application_wantsToQuit() {
            if (!ProtocolGame || !ProtocolGame.IsGameRunning || OpenTibiaUnity.Quiting)
                return true;
            
            ShowQuitNotification();
            return false;
        }

        internal void ShowQuitNotification() {
            // are you sure you want to exit?
            if (m_ExitWindow == null) {
                m_ExitWindow = Instantiate(ExitWindowPrefab, GameCanvas.transform);
                m_ExitWindow.rectTransform.anchoredPosition = new Vector2(0, 0);
            } else if (m_ExitWindow.gameObject.activeSelf == false) {
                m_ExitWindow.gameObject.SetActive(true);
                m_ExitWindow.rectTransform.anchoredPosition = new Vector2(0, 0);
            }
        }

        internal string GetAssetsPath(int clientVersion, int buildVersion, ClientSpecification specification) {
            string assetsPath;
            if (clientVersion >= 1100)
                assetsPath = string.Format("{0}.{1}", clientVersion, buildVersion);
            else
                assetsPath = clientVersion.ToString();

            assetsPath = Path.Combine(Application.streamingAssetsPath, assetsPath);
            return assetsPath;
        }

        internal bool CanLoadThings(int clientVersion, int buildVersion, ClientSpecification specification) {
            string assetsPath = GetAssetsPath(clientVersion, buildVersion, specification);
            
            string appearancesPath = Path.Combine(assetsPath, "appearances.dat");
            if (!File.Exists(appearancesPath))
                return false;

            string spritesPath = Path.Combine(assetsPath, "sprites");
            if (!File.Exists(spritesPath))
                return false;

            return true;
        }

        internal async void LoadThingsAsync(int clientVersion, int buildVersion, ClientSpecification specification) {
            await LoadThingsAsyncAwaitable(clientVersion, buildVersion, specification);
        }

        internal async Task<bool> LoadThingsAsyncAwaitable(int clientVersion, int buildVersion, ClientSpecification specification) {
            if (m_LoadingClientAssets)
                return false;

            if (clientVersion < 1100) {
                if (m_LoadedClientVersion == clientVersion)
                    return true;
            } else if (m_LoadedClientVersion == clientVersion && m_LoadedBuildVersion == buildVersion)
                return true;

            m_LoadingClientAssets = true;
            
            try {
                string assetsPath = GetAssetsPath(clientVersion, buildVersion, specification);

                AppearanceStorage.Unload();
                LoadProtoAppearanaces(assetsPath);
                await LoadSpriteAssets(assetsPath);

                m_LoadedClientAssets = true;
                m_LoadedClientVersion = clientVersion;
                m_LoadedBuildVersion = buildVersion;
            } catch (System.Exception) {
                m_LoadedClientAssets = false;
            }

            m_LoadingClientAssets = false;
            return m_LoadedClientAssets;
        }

        protected void LoadProtoAppearanaces(string assetsPath) {
            string appearancesPath = Path.Combine(assetsPath, "appearances.dat");
            if (!File.Exists(appearancesPath))
                throw new FileNotFoundException(string.Format("Couldn't find the appearances ({0}).", appearancesPath));
            
            try {
                byte[] bytes = File.ReadAllBytes(appearancesPath);
                AppearanceStorage.SetProtoAppearances(Protobuf.Appearances.Appearances.Parser.ParseFrom(bytes));
            } catch (System.Exception e) {
                throw new System.Exception(string.Format("Unable to appearances.dat ({0}).", e.Message));
            }
        }

        protected async Task LoadSpriteAssets(string assetsPath) {
            string spritesPath = Path.Combine(assetsPath, "sprites");
            if (!File.Exists(spritesPath))
                throw new FileNotFoundException(string.Format("Couldn't find sprites ({0}).", spritesPath));

            string catalogPath = Path.Combine(assetsPath, "catalog-content.json");
            if (!File.Exists(catalogPath))
                throw new FileNotFoundException(string.Format("Couldn't find catalog-content ({0}).", catalogPath));
            
            var request = AssetBundle.LoadFromFileAsync(spritesPath);
            while (!request.isDone)
                await Task.Yield();
            
            var assetBundle = request.assetBundle;
            if (assetBundle == null)
                throw new System.Exception(string.Format("Unable to load asset bundle ({0}).", spritesPath));
            
            try {
                string catalogJson = File.ReadAllText(catalogPath);
                AppearanceStorage.SetSpriteProvider(new Appearances.SpritesProvider(assetBundle, catalogJson));
            } catch (System.Exception e) {
                assetBundle.Unload(true);
                throw new System.Exception(string.Format("Unable to catalog-content.json ({0}).", e.Message));
            }
        }

        internal void ProcessGamePending() {
            // onGamePending.Invoke();

            ProtocolGame.SendEnterGame();
        }

        internal void ProcessGameStart() {
            onGameStart.Invoke();
            PendingCharacterIndex = -1;

            ProtocolGame.onConnectionError.AddListener(OnConnectionError);
            ProtocolGame.onLoginAdvice.AddListener(OnLoginAdvice);
            ProtocolGame.onConnectionLost.AddListener(OnConnectionLost);
            ProtocolGame.onConnectionRecovered.AddListener(OnConnectionRecovered);

            ShouldSendPingAt = OpenTibiaUnity.TicksMillis + Constants.PingDelay;
            InvokeRepeating("OnCheckAlive", 1, 1);
        }

        internal void ProcessGameEnd() {
            onGameEnd.Invoke();

            WorldMapRenderer.DestroyUIElements();
            CursorController.SetCursorState(CursorState.Default, CursorPriority.High);

            CancelInvoke("OnCheckAlive");

            if (BackgroundCanvas && BackgroundCanvas.gameObject) {
                BackgroundCanvas.gameObject.SetActive(true);
                EventSystem.SetSelectedGameObject(BackgroundCanvas.gameObject);
            }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            OpenTibiaUnity.GameManager.SetApplicationTitle(Application.productName);
#endif
        }

        internal void ProcessChangeCharacter() {
            onProcessChangeCharacter.Invoke();
        }

        internal void OnCheckAlive() {
            if (!!OpenTibiaUnity.ProtocolGame)
                OpenTibiaUnity.ProtocolGame.OnCheckAlive();
        }

        private void OnConnectionError(string message, bool disconnected) {
            Debug.Log("ProcessGameError: " + message + ", " + disconnected);
            if (disconnected) {
                if (System.Threading.Thread.CurrentThread == OpenTibiaUnity.MainThread)
                    ProcessGameEnd();
                else
                    InvokeOnMainThread(() => ProcessGameEnd());
            }
        }

        private void OnLoginAdvice(string message) {

        }

        private void OnConnectionLost() {

        }

        private void OnConnectionRecovered() {

        }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        internal void SetApplicationTitle(string title) {
            SetWindowText(WindowPtr, title);
        }
#endif

        internal void EnableFeature(GameFeature feature) => Features.Set((int)feature, true);
        internal void DisableFeature(GameFeature feature) => Features.Set((int)feature, false);
        internal bool GetFeature(GameFeature feature) => Features.Get((int)feature);

        internal void SetClientSpecification(ClientSpecification specification) {
            if (ClientSpecification == specification)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change client version while online");

            if (ClientVersion == 0)
                throw new System.InvalidOperationException("Cannot set specification before assigning a client version.");

            ClientSpecification = specification;

            var oldSpecification = ClientSpecification;
            ClientSpecification = specification;

            onClientSpecificationChange.Invoke(oldSpecification, specification);
            return;
        }

        internal void SetClientVersion(int version) {
            if (ClientVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change client version while online");

            int minimumVersion = OpenTibiaUnity.GetMinimumClientVersion();
            int maximumVersion = OpenTibiaUnity.GetMaximumClientVersion();
            if (version != 0 && (version < minimumVersion || version > maximumVersion))
                throw new System.Exception(string.Format("Client version {0} not supported", version));

            Features.SetAll(false); // reset all

            if (version >= 770) {
                EnableFeature(GameFeature.GameOutfitIDU16);
                EnableFeature(GameFeature.GameMessageStatements);
                EnableFeature(GameFeature.GameLoginPacketEncryption);
            }

            if (version >= 780) {
                EnableFeature(GameFeature.GamePlayerAddons);
                EnableFeature(GameFeature.GamePlayerStamina);
                EnableFeature(GameFeature.GameNewFluids);
                EnableFeature(GameFeature.GameMessageLevel);
                EnableFeature(GameFeature.GamePlayerStateU16);
                EnableFeature(GameFeature.GameNewOutfitProtocol);
            }

            if (version >= 790) {
                EnableFeature(GameFeature.GameWritableDate);
            }

            if (version >= 840) {
                EnableFeature(GameFeature.GameProtocolChecksum);
                EnableFeature(GameFeature.GameAccountNames);
                EnableFeature(GameFeature.GameDoubleFreeCapacity);
            }

            if (version >= 841) {
                EnableFeature(GameFeature.GameChallengeOnLogin);
                EnableFeature(GameFeature.GameMessageSizeCheck);
            }

            if (version >= 854) {
                EnableFeature(GameFeature.GameCreatureEmblems);
                EnableFeature(GameFeature.GameServerLog);
            }

            if (version >= 860) {
                EnableFeature(GameFeature.GameAttackSeq);
            }

            if (version >= 862) {
                EnableFeature(GameFeature.GamePenalityOnDeath);
            }

            if (version >= 870) {
                EnableFeature(GameFeature.GameDoubleExperience);
                EnableFeature(GameFeature.GamePlayerMounts);
                EnableFeature(GameFeature.GameSpellList);
            }

            if (version >= 910) {
                EnableFeature(GameFeature.GameNameOnNpcTrade);
                EnableFeature(GameFeature.GameTotalCapacity);
                EnableFeature(GameFeature.GameSkillsBase);
                EnableFeature(GameFeature.GamePlayerRegenerationTime);
                EnableFeature(GameFeature.GameChannelPlayerList);
                EnableFeature(GameFeature.GameEnvironmentEffect);
                EnableFeature(GameFeature.GameItemAnimationPhase);
            }

            if (version >= 940) {
                EnableFeature(GameFeature.GamePlayerMarket);
            }

            if (version >= 953) {
                EnableFeature(GameFeature.GamePurseSlot);
                EnableFeature(GameFeature.GameClientPing);
            }

            if (version >= 960) {
                EnableFeature(GameFeature.GameSpritesU32);
                EnableFeature(GameFeature.GameOfflineTrainingTime);
            }

            if (version >= 963) {
                EnableFeature(GameFeature.GameAdditionalVipInfo);
            }

            if (version >= 980) {
                EnableFeature(GameFeature.GamePreviewState);
                EnableFeature(GameFeature.GameClientVersion);
            }

            if (version >= 981) {
                EnableFeature(GameFeature.GameLoginPending);
                EnableFeature(GameFeature.GameNewSpeedLaw);
            }

            if (version >= 984) {
                EnableFeature(GameFeature.GameContainerPagination);
                EnableFeature(GameFeature.GameBrowseField);
            }

            if (version >= 1000) {
                EnableFeature(GameFeature.GameCreatureMarks);
                EnableFeature(GameFeature.GameObjectMarks);
                EnableFeature(GameFeature.GamePVPMode);
            }

            if (version >= 1035) {
                EnableFeature(GameFeature.GameDoubleSkills);
                EnableFeature(GameFeature.GameBaseSkillU16);
            }

            if (version >= 1036) {
                EnableFeature(GameFeature.GameCreatureIcons);
                EnableFeature(GameFeature.GameHideNpcNames);
            }

            if (version >= 1038) {
                EnableFeature(GameFeature.GamePremiumExpiration);
            }

            if (version >= 1050) {
                EnableFeature(GameFeature.GameEnhancedAnimations);
            }

            if (version >= 1053) {
                EnableFeature(GameFeature.GameUnjustifiedPoints);
            }

            if (version >= 1054) {
                EnableFeature(GameFeature.GameExperienceBonus);
            }

            if (version >= 1055) {
                EnableFeature(GameFeature.GameDeathType);
            }

            if (version >= 1057) {
                EnableFeature(GameFeature.GameSeparateAnimationGroups);
            }

            if (version >= 1061) {
                EnableFeature(GameFeature.GameOGLInformation);
            }

            if (version >= 1071) {
                EnableFeature(GameFeature.GameContentRevision);
            }

            if (version >= 1072) {
                EnableFeature(GameFeature.GameAuthenticator);
            }

            if (version >= 1074) {
                EnableFeature(GameFeature.GameSessionKey);
            }

            if (version >= 1076) {
                EnableFeature(GameFeature.GameEquipHotkey);
            }

            if (version >= 1080) {
                EnableFeature(GameFeature.GameIngameStore);
            }

            if (version >= 1092) {
                EnableFeature(GameFeature.GameIngameStoreServiceType);
                EnableFeature(GameFeature.GameStoreInboxSlot);
            }

            if (version >= 1093) {
                EnableFeature(GameFeature.GameIngameStoreHighlights);
            }

            if (version >= 1094) {
                EnableFeature(GameFeature.GameAdditionalSkills);
            }

            if (version >= 1097) {
                EnableFeature(GameFeature.GameExperienceGain);
            }

            if (version >= 1102) {
                EnableFeature(GameFeature.GameWorldProxyIdentification);
                EnableFeature(GameFeature.GamePrey);
                EnableFeature(GameFeature.GameImbuing);
            }

            if (version >= 1110) {
                EnableFeature(GameFeature.GameBuddyGroups);
                EnableFeature(GameFeature.GameInspectionWindow);
            }

            if (version >= 1111) {
                DisableFeature(GameFeature.GameProtocolChecksum);
                EnableFeature(GameFeature.GameProtocolSequenceNumber);
            }

            if (version >= 1120) {
                EnableFeature(GameFeature.GameBlessingDialog);
                EnableFeature(GameFeature.QuestTracker);
            }
            
            if (version >= 1140) {
                EnableFeature(GameFeature.GamePlayerStateU32);
                EnableFeature(GameFeature.GameRewardWall);
                EnableFeature(GameFeature.GameAnalytics);
                EnableFeature(GameFeature.GameCyclopedia);
            }

            if (version >= 1150) {
                EnableFeature(GameFeature.GameQuickLoot);
                EnableFeature(GameFeature.GameExtendedCapacity);
                DisableFeature(GameFeature.GameTotalCapacity);
            }
            
            if (version >= 1180) {
                EnableFeature(GameFeature.GameStash);
            }
            
            if (version >= 1185) {
                DisableFeature(GameFeature.GameEnvironmentEffect);
                DisableFeature(GameFeature.GameObjectMarks);
                EnableFeature(GameFeature.GameCyclopediaMap);
            }

            // all these features are not verified (1200 is just the version we could obtain)
            if (version >= 1200) {
                EnableFeature(GameFeature.GamePercentSkillU16);
            }

            if (version >= 1215) {
                EnableFeature(GameFeature.GameTournament);
            }

            int oldVersion = ClientVersion;
            ClientVersion = version;

            onClientVersionChange.Invoke(oldVersion, version);
        }

        internal void SetProtocolVersion(int version) {
            if (ProtocolVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change protocol version while online");

            int minimumVersion = OpenTibiaUnity.GetMinimumProtocolVersion();
            int maximumVersion = OpenTibiaUnity.GetMaximumProtocolVersion();
            if (version != 0 && (version < minimumVersion || version > maximumVersion))
                throw new System.Exception(string.Format("Protocol version {0} not supported", version));
            
            int oldVersion = ProtocolVersion;
            ProtocolVersion = version;

            onProtocolVersionChange.Invoke(oldVersion, version);
        }

        internal void SetBuildVersion(int version) {
            if (BuildVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change build version while online");

            int minimumVersion = OpenTibiaUnity.GetMinimumBuildVersion();
            int maximumVersion = OpenTibiaUnity.GetMaximumBuildVersion();
            if (version != 0 && (version < minimumVersion || version > maximumVersion))
                throw new System.Exception(string.Format("Build version {0} not supported", version));

            if (version >= 8762) {
                EnableFeature(GameFeature.GameAccountEmailAddress);
            }

            int oldVersion = BuildVersion;
            BuildVersion = version;

            onBuildVersionChange.Invoke(oldVersion, version);
        }

        internal bool RegisterModule(Components.Base.Module module) {
            if (m_Modules.Contains(module))
                return false;
            
            m_Modules.Add(module);
            return true;
        }

        internal bool UnregisterModule(Components.Base.Module module) {
            return m_Modules.Remove(module);
        }

        internal T GetModule<T>() where T : Components.Base.Module {
            foreach (var module in m_Modules) {
                if (typeof(T) == module.GetType())
                    return module as T;
            }

            return null;
        }

        internal T GetLastModule<T>() where T : Components.Base.Module {
            for (int i = m_Modules.Count - 1; i >= 0; i--) {
                var module = m_Modules[i];
                if (typeof(T) == module.GetType())
                    return module as T;
            }

            return null;
        }

        internal List<T> GetModules<T>() where T : Components.Base.Module {
            List<T> modules = new List<T>();
            foreach (var module in m_Modules) {
                if (module is T result)
                    modules.Add(result);
            }

            return modules;
        }
    }
}