using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Core
{

    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public class TacticsChangeEvent : UnityEvent<CombatAttackModes, CombatChaseModes, bool, CombatPvPModes> { }
        public class VersionChangeEvent : UnityEvent<int, int> { }
        public class ClientSpecificationChangeEvent : UnityEvent<ClientSpecification, ClientSpecification> { }
        public class RequestChatSendEvent : UnityEvent<string, bool, int> { }
        public class RequestOutfitDialogEvent : UnityEvent<Appearances.AppearanceInstance, Appearances.AppearanceInstance, List<Communication.Game.ProtocolOutfit>, List<Communication.Game.ProtocolMount>> { }
        public class RequestNPCTradeEvent : UnityEvent<string, List<Trade.TradeObjectRef>, List<Trade.TradeObjectRef>> { }
        public class RequestTradeOfferEvent : UnityEvent<string, List<Appearances.ObjectInstance>> { }
        public class RequestModalDialogEvent : UnityEvent<Communication.Game.ProtocolModalDialog> { }
        public class ReceiveChannelsEvent : UnityEvent<List<Chat.Channel>> { }

        /// <summary>
        /// Game Manager should only exist once..
        /// </summary>
        public static GameManager Instance { get; private set; }

        [System.NonSerialized] public Material InternalColoredMaterial;

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, string lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(string className, string windowName);
#endif

        // Serialiazable fields
        // Prefabs
        [Header("Prefabs")]
        public TMPro.TextMeshProUGUI DefaultLabel = null;
        public UI.Legacy.Button ButtonPrefab = null;
        public UI.Legacy.Button BlueButtonPrefab = null;
        public UI.Legacy.Button GreenButtonPrefab = null;
        public UI.Legacy.Button YellowButtonPrefab = null;
        public GameObject HorizontalSeparator = null;
        public GameObject VerticalSeparator = null;
        public GameObject SidebarWidgetShadowVariant = null;
        public UI.Legacy.MessageWidget MessageWidgetPrefab = null;
        public UI.Legacy.ConnectionLostWidget ConnectionLostWidgetPrefab = null;
        public TMPro.TextMeshProUGUI LabelOnscreenMessageBoxPrefab = null;
        public UI.Legacy.SplitStackWidget SplitStackWindowPrefab = null;
        public GameObject ContextMenuBasePrefab = null;
        public GameObject ContextMenuItemPrefab = null;
        public UI.Legacy.CheckboxPanel PanelCheckBox = null;
        public UI.Legacy.ItemPanel ItemPanelPrefab = null;

        // Utility objects for rendering
        [Header("Utility Invariants")]
        public TMPro.TextMeshProUGUI LabelOnscreenText = null;
        public TMPro.TextMeshProUGUI LabelOnscreenMessage = null; // optimized for onscreen messages (290x195)

        // UI Default Objects
        [Header("Canvases & Overlays")]
        public Canvas BackgroundCanvas = null;
        public Canvas BackgroundPanelBlocker = null;
        public Canvas GameCanvas = null;
        public Canvas GamePanelBlocker = null;
        public EventSystem EventSystem = null;

        [Header("Static Panels")]
        public RectTransform LobbyPanel = null;
        public RectTransform WorldMapRenderingPanel = null;

        [Header("Windows")]
        public UI.Legacy.PopUpBase LoadingAppearancesWindow = null;

        [Header("Materials")]
        public Material ColoredMaterial = null;
        public Material AppearanceTypeMaterial = null;
        public Material OutfitTypeMaterial = null;
        public Material MarksViewMaterial = null;
        public Material LightmapMaterial = null;
        public Material VerdanaFontMaterial = null;
        public Material OutlinedVerdanaFontMaterial = null;
        public Material BitmapVerdanaFontMaterial = null;

        [Header("Render Textures")]
        public CustomRenderTexture WorldMapRenderTexture = null;
        public RenderTexture MiniMapRenderingTexture = null;

        [Header("Main Elements")]
        public Camera MainCamera = null;
        
        [Header("Utilities")]
        public Utils.CursorController CursorController = null;

        [Header("Textures & Sprites")]
        public Texture2D MarksViewTexture = null;
        public Texture2D TileCursorTexture = null;
        public Texture2D StateFlagsTexture = null;
        public Texture2D SpeechFlagsTexture = null;

        [Header("Colors")]
        public Color NormalColor = Colors.ColorFromRGB(0x414141);
        public Color AlternateColor = Colors.ColorFromRGB(0x484848);
        public Color HighlightColor = Colors.ColorFromRGB(0x585858);

        // Properties
        public Thread MainThread { get; private set; }
        public float TicksSecondsF { get; private set; }
        public int TicksSeconds { get; private set; }
        public float TicksMillisF { get; private set; }
        public int TicksMillis { get; private set; }
        public float DeltaTicksSecondsF { get; private set; }
        public int DeltaTicksSeconds { get; private set; }
        public float DeltaTicksMillisF { get; private set; }
        public int DeltaTicksMillis { get; private set; }
        public Options.OptionStorage OptionStorage { get; private set; }
        public Input.InputHandler InputHandler { get; private set; }
        public Appearances.AppearanceStorage AppearanceStorage { get; private set; }
        public Creatures.CreatureStorage CreatureStorage { get; private set; }
        public MiniMap.MiniMapStorage MiniMapStorage { get; private set; }
        public MiniMap.Rendering.MiniMapRenderer MiniMapRenderer { get; private set; }
        public WorldMap.WorldMapStorage WorldMapStorage { get; private set; }
        public WorldMap.Rendering.WorldMapRenderer WorldMapRenderer { get; private set; }
        public Chat.ChatStorage ChatStorage { get; private set; }
        public Chat.MessageStorage MessageStorage { get; private set; }
        public Container.ContainerStorage ContainerStorage { get; private set; }
        public Magic.SpellStorage SpellStorage { get; private set; }
        public Store.StoreStorage StoreStorage { get; private set; }
        public Cyclopedia.CyclopediaStorage CyclopediaStorage { get; private set; }
        public Communication.Game.ProtocolGame ProtocolGame { get; set; }
        public int PendingCharacterIndex { get; set; } = -1;
        public UnityEvent onSecondaryTimeCheck { get; private set; }
        public TacticsChangeEvent onTacticsChange { get; private set; }
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        public System.IntPtr WindowPtr { get; private set; }
#endif
        public System.Collections.BitArray Features { get; private set; }
        public ClientSpecification ClientSpecification { get; private set; } = ClientSpecification.Cipsoft;
        public int ProtocolVersion { get; private set; } = 0;
        public int ClientVersion { get; private set; } = 0;
        public int BuildVersion { get; private set; } = 0; // tibia 11.xx.(xxxx)
        public VersionChangeEvent onClientVersionChange { get; private set; }
        public VersionChangeEvent onProtocolVersionChange { get; private set; }
        public VersionChangeEvent onBuildVersionChange { get; private set; }
        public ClientSpecificationChangeEvent onClientSpecificationChange { get; private set; }
        public UnityEvent onLoadedGameAssets { get; private set; }
        public UnityEvent onGameStart { get; private set; }
        public UnityEvent onGameEnd { get; private set; }
        public UnityEvent onProcessLogoutCharacter { get; private set; }
        public UnityEvent onProcessChangeCharacter { get; private set; }
        public UnityEvent onRequestHotkeysDialog { get; private set; }
        public UnityEvent onRequestChatHistoryPrev { get; private set; }
        public UnityEvent onRequestChatHistoryNext { get; private set; }
        public RequestChatSendEvent onRequestChatSend { get; private set; }
        public RequestOutfitDialogEvent onRequestOutfitDialog { get; private set; }
        public RequestNPCTradeEvent onRequestNPCTrade { get; private set; }
        public UnityEvent onRequestCloseNPCTrade { get; private set; }
        public RequestTradeOfferEvent onRequestOwnOffer { get; private set; }
        public RequestTradeOfferEvent onRequestCounterOffer { get; private set; }
        public UnityEvent onRequestCloseTrade { get; private set; }
        public RequestModalDialogEvent onRequestModalDialog { get; private set; }
        public ReceiveChannelsEvent onReceiveChannels { get; private set; }

        public Canvas ActiveBlocker {
            get {
                if (BackgroundCanvas && BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundPanelBlocker;
                return GameCanvas ? GamePanelBlocker : null;
            }
        }

        public Canvas ActiveCanvas {
            get => BackgroundCanvas && BackgroundCanvas.gameObject.activeSelf ? BackgroundCanvas : (GameCanvas ? GameCanvas : null);
        }

        public UnityUI.GraphicRaycaster ActiveRaycaster {
            get => ActiveCanvas ? ActiveCanvas.GetComponent<UnityUI.GraphicRaycaster>() : null;
        }

        public bool IsGameRunning { get => !!ProtocolGame && ProtocolGame.IsGameRunning; }
        
        public bool IsRealTibia { get => ClientSpecification == ClientSpecification.Cipsoft; }
        public bool IsOpenTibia { get => ClientSpecification == ClientSpecification.OpenTibia; }

        private object _loadingClientAssetsLock;
        public bool IsLoadingClientAssets {
            get {
                lock (_loadingClientAssetsLock)
                    return _loadingClientAssets;
            }
            private set {
                lock (_loadingClientAssetsLock)
                    _loadingClientAssets = value;
            }
        }
        private object _loadedClientAssetsLock;
        public bool HasLoadedClientAssets {
            get {
                lock (_loadedClientAssetsLock)
                    return _loadedClientAssets;
            }
            private set {
                lock (_loadingClientAssetsLock)
                    _loadedClientAssets = value;
            }
        }

        // private fields
        private Queue<UnityAction> _actionQueue;
        private bool _lastMiniMapSaveWasSuccessfull = false;

        private bool _loadingClientAssets = false;
        private bool _loadedClientAssets = false;
        private int _loadedClientVersion = 0;
        private int _loadedBuildVersion = 0;

        private List<Components.Base.Module> _modules;

        private bool _showingQuitNotification = false;

        private void Awake() {
            // setup basic utilities
            Random.InitState(new System.DateTime().Millisecond);

            // setup static fields
            Instance = this;
            MainThread = Thread.CurrentThread;

            OpenTibiaUnity.GraphicsVendor = SystemInfo.graphicsDeviceVendor;
            OpenTibiaUnity.GraphicsDevice = SystemInfo.graphicsDeviceName;
            OpenTibiaUnity.GraphicsVersion = SystemInfo.graphicsDeviceVersion;
            
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
            WindowPtr = FindWindow(null, "OpenTibiaUnity");
#endif
            Features = new System.Collections.BitArray(new bool[(int)GameFeature.LastGameFeature]);

            // setup threading mutex & lockers
            _actionQueue = new Queue<UnityAction>();
            _loadingClientAssetsLock = new object();
            _loadedClientAssetsLock = new object();

            // quit-notification related
            OpenTibiaUnity.Quiting = false;

            InternalColoredMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

            // events
            onSecondaryTimeCheck = new UnityEvent();
            onTacticsChange = new TacticsChangeEvent();
            onClientVersionChange = new VersionChangeEvent();
            onProtocolVersionChange = new VersionChangeEvent();
            onBuildVersionChange = new VersionChangeEvent();
            onClientSpecificationChange = new ClientSpecificationChangeEvent();
            onLoadedGameAssets = new UnityEvent();
            onGameStart = new UnityEvent();
            onGameEnd = new UnityEvent();
            onProcessLogoutCharacter = new UnityEvent();
            onProcessChangeCharacter = new UnityEvent();
            onRequestHotkeysDialog = new UnityEvent();
            onRequestChatHistoryPrev = new UnityEvent();
            onRequestChatHistoryNext = new UnityEvent();
            onRequestChatSend = new RequestChatSendEvent();
            onRequestOutfitDialog = new RequestOutfitDialogEvent();
            onRequestNPCTrade = new RequestNPCTradeEvent();
            onRequestCloseNPCTrade = new UnityEvent();
            onRequestOwnOffer = new RequestTradeOfferEvent();
            onRequestCounterOffer = new RequestTradeOfferEvent();
            onRequestCloseTrade = new UnityEvent();
            onRequestModalDialog = new RequestModalDialogEvent();
            onReceiveChannels = new ReceiveChannelsEvent();

            // setup ingame facilities
            OptionStorage = new Options.OptionStorage();
            InputHandler = new Input.InputHandler();
            AppearanceStorage = new Appearances.AppearanceStorage();
            CreatureStorage = new Creatures.CreatureStorage();
            MiniMapStorage = new MiniMap.MiniMapStorage();
            MiniMapRenderer = new MiniMap.Rendering.MiniMapRenderer();
            WorldMapStorage = new WorldMap.WorldMapStorage(LabelOnscreenMessageBoxPrefab);
            WorldMapRenderer = new WorldMap.Rendering.WorldMapRenderer();
            ChatStorage = new Chat.ChatStorage(OptionStorage);
            MessageStorage = new Chat.MessageStorage();
            ContainerStorage = new Container.ContainerStorage();
            SpellStorage = new Magic.SpellStorage();
            StoreStorage = new Store.StoreStorage();
            CyclopediaStorage = new Cyclopedia.CyclopediaStorage();

            // Load options
            OptionStorage.LoadOptions();
            
            // update input settings
            InputHandler.UpdateMapping();

            // initialize rendering textures
            WorldMapRenderTexture.Initialize();
            WorldMapRenderTexture.IncrementUpdateCount();
            
            // initialize core game actions
            Game.ObjectMultiUseHandler.Initialize();
            Game.ObjectDragHandler.Initialize();

            _modules = new List<Components.Base.Module>();
        }

        private void Start() {
            InvokeRepeating("SecondaryTimerCheck", 0, 0.05f);
            InvokeRepeating("MiniMapIOTimer", 0, 0.25f);

#if !UNITY_EDITOR
            Application.wantsToQuit += ApplicationWantsToQuit;
#endif
        }
        
        private void FixedUpdate() {
            // this is called before any other action
            float time = Time.time;
            float deltaTime = Time.fixedDeltaTime;

            TicksSecondsF = time;
            TicksSeconds = (int)TicksSecondsF;
            TicksMillisF = time * 1000;
            TicksMillis = (int)TicksMillisF;

            DeltaTicksSecondsF = deltaTime;
            DeltaTicksSeconds = (int)DeltaTicksSecondsF;
            DeltaTicksMillisF = deltaTime * 1000;
            DeltaTicksMillis = (int)DeltaTicksMillisF;

            DequeueMainThreadActions();
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

            if (IsGameRunning)
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

            WorldMapRenderTexture.Release();
            MiniMapRenderingTexture.Release();
        }

        public void AddSecondaryTimerListener(UnityAction action) {
            onSecondaryTimeCheck.AddListener(action);
        }

        public void RemoveSecondaryTimerListener(UnityAction action) {
            onSecondaryTimeCheck.RemoveListener(action);
        }

        private void SecondaryTimerCheck() {
            onSecondaryTimeCheck.Invoke();
        }

        public void MiniMapIOTimer() {
            if (!IsGameRunning) {
                if (!_lastMiniMapSaveWasSuccessfull)
                    return;
                else
                    _lastMiniMapSaveWasSuccessfull = false;
            } else {
                _lastMiniMapSaveWasSuccessfull = true;
            }

            MiniMapStorage.OnIOTimer();
        }

        public void InvokeOnMainThread(UnityAction action) {
            lock (_actionQueue)
                _actionQueue.Enqueue(action);
        }

        private void DequeueMainThreadActions() {
            if (OpenTibiaUnity.Quiting)
                return;

            while (true) {
                UnityAction action;
                lock (_actionQueue) {
                    if (_actionQueue.Count == 0)
                        break;

                    action = _actionQueue.Dequeue();
                }

                action.Invoke();
            }
        }

        private bool ApplicationWantsToQuit() {
            if (!IsGameRunning || OpenTibiaUnity.Quiting)
                return true;

            return !ShowQuitNotification();
        }

        public bool ShowQuitNotification() {
            if (_showingQuitNotification)
                return false;

            _showingQuitNotification = true;

            void OnExitClick() {
                Application.Quit();
            }
            void OnLogoutClick() {
                _showingQuitNotification = false;
                if (IsGameRunning)
                    ProtocolGame.Disconnect(false);
            }
            void OnCancelClick() {
                _showingQuitNotification = false;
            }

            var exitWidget = UI.Legacy.MessageWidget.CreateMessageWidget(ActiveCanvas.transform, TextResources.EXIT_WINDOW_TITLE, TextResources.EXIT_WINDOW_MESSAGE);
            exitWidget.AddButton("Exit", KeyCode.E, OnExitClick);
            exitWidget.AddButton(UI.Legacy.PopUpButtonMask.Ok, "Logout", OnLogoutClick);
            exitWidget.AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelClick);
            return true;
        }

        public string GetAssetsPath(int clientVersion, int buildVersion, ClientSpecification specification) {
            string assetsPath;
            if (clientVersion >= 1100)
                assetsPath = string.Format("{0}.{1}", clientVersion, buildVersion);
            else
                assetsPath = clientVersion.ToString();

            assetsPath = Path.Combine(Application.streamingAssetsPath, assetsPath);
            return assetsPath;
        }

        public bool CanLoadThings(int clientVersion, int buildVersion, ClientSpecification specification) {
            string assetsPath = GetAssetsPath(clientVersion, buildVersion, specification);
            
            string appearancesPath = Path.Combine(assetsPath, "appearances.otud");
            if (!File.Exists(appearancesPath))
                return false;

            string spritesPath = Path.Combine(assetsPath, "assets.otus");
            if (!File.Exists(spritesPath))
                return false;

            return true;
        }

        public async void LoadThingsAsync(int clientVersion, int buildVersion, ClientSpecification specification) {
            await LoadThingsAsyncAwaitable(clientVersion, buildVersion, specification);
        }

        public async Task<bool> LoadThingsAsyncAwaitable(int clientVersion, int buildVersion, ClientSpecification specification) {
            if (IsLoadingClientAssets)
                return false;

            if (clientVersion < 1100) {
                if (_loadedClientVersion == clientVersion)
                    return true;
            } else if (_loadedClientVersion == clientVersion && _loadedBuildVersion == buildVersion)
                return true;

            IsLoadingClientAssets = true;
            
            try {
                string assetsPath = GetAssetsPath(clientVersion, buildVersion, specification);

                AppearanceStorage.Unload();
                LoadProtoAppearanaces(assetsPath);
                await LoadSpriteAssets(assetsPath);

                HasLoadedClientAssets = true;
                _loadedClientVersion = clientVersion;
                _loadedBuildVersion = buildVersion;

                if (Thread.CurrentThread == OpenTibiaUnity.MainThread)
                    onLoadedGameAssets.Invoke();
                else
                    InvokeOnMainThread(() => onLoadedGameAssets.Invoke());
            } catch (System.Exception e) {
#if DEBUG || NDEBUG
                Debug.Log($"GameManager.LoadThingsAsyncAwaitable failed ({e.Message})");
#endif
                HasLoadedClientAssets = false;
            }

            IsLoadingClientAssets = false;
            return HasLoadedClientAssets;
        }

        protected void LoadProtoAppearanaces(string assetsPath) {
            string appearancesPath = Path.Combine(assetsPath, "appearances.otud");
            if (!File.Exists(appearancesPath))
                throw new FileNotFoundException(string.Format("Couldn't find the appearances ({0}).", appearancesPath));
            
            try {
                byte[] bytes = File.ReadAllBytes(appearancesPath);
                AppearanceStorage.SetProtoAppearances(Protobuf.Appearances.Appearances.Parser.ParseFrom(bytes));
            } catch (System.Exception e) {
                throw new System.Exception(string.Format("Unable to appearances.otud ({0}).", e.Message));
            }
        }

        protected async Task LoadSpriteAssets(string assetsPath) {
            string spritesPath = Path.Combine(assetsPath, "assets.otus");
            if (!File.Exists(spritesPath))
                throw new FileNotFoundException(string.Format("Couldn't find assets.otus ({0}).", spritesPath));

            using (var stream = File.OpenRead(spritesPath)) {
                var spriteProvider = await LoadSpriteProvider(stream);
                AppearanceStorage.SetSpriteProvider(spriteProvider);
            }
        }

        private async Task<Appearances.SpritesProvider> LoadSpriteProvider(Stream stream) {
            var spriteProvider = new Appearances.SpritesProvider();

            var c = spriteProvider.Parse(stream);
            while (c.MoveNext())
                await Task.Yield();

            return spriteProvider;
        }

        public void ProcessGamePending() {
            // onGamePending.Invoke();

            ProtocolGame.SendEnterGame();
        }

        public void ProcessGameStart() {
            onGameStart.Invoke();
            PendingCharacterIndex = -1;

            ProtocolGame.onConnectionError.AddListener(OnConnectionError);
            ProtocolGame.onLoginAdvice.AddListener(OnLoginAdvice);
            ProtocolGame.onConnectionLost.AddListener(OnConnectionLost);

            InvokeRepeating("OnCheckAlive", 1, 1);
        }

        public void ProcessGameEnd() {
            onGameEnd.Invoke();
            
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

        public void ProcessChangeCharacter() {
            onProcessChangeCharacter.Invoke();
        }

        public void OnCheckAlive() {
            if (!!OpenTibiaUnity.ProtocolGame)
                OpenTibiaUnity.ProtocolGame.OnCheckAlive();
        }

        private void OnConnectionError(string message, bool disconnected) {
#if DEBUG || NDEBUG
            Debug.Log($"ProcessGame.OnConnectionError ({message}) Disconnected: {disconnected}");
#endif
            if (disconnected) {
                if (Thread.CurrentThread == OpenTibiaUnity.MainThread)
                    ProcessGameEnd();
                else
                    InvokeOnMainThread(() => ProcessGameEnd());
            }
        }

        private void OnLoginAdvice(string message) {

        }

        private void OnConnectionLost() {
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            protocolGame.onConnectionRecovered.AddListener(OnConnectionRecovered);

            var connectionLostWidget = UI.Legacy.ConnectionLostWidget.CreateWidget();
        }

        private void OnConnectionRecovered() {

        }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        public void SetApplicationTitle(string title) {
            SetWindowText(WindowPtr, title);
        }
#endif

        public void EnableFeature(GameFeature feature) => Features.Set((int)feature, true);
        public void DisableFeature(GameFeature feature) => Features.Set((int)feature, false);
        public bool GetFeature(GameFeature feature) => Features.Get((int)feature);

        public void SetClientSpecification(ClientSpecification specification) {
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

        public void SetClientVersion(int version) {
            if (ClientVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change client version while online");

            int minimumVersion = OpenTibiaUnity.GetMinimumClientVersion();
            int maximumVersion = OpenTibiaUnity.GetMaximumClientVersion();
            if (version != 0 && (version < minimumVersion || version > maximumVersion))
                throw new System.Exception(string.Format("Client version {0} not supported", version));

            Features.SetAll(false); // reset all

            // basic game features that are disabled later
            EnableFeature(GameFeature.GameDebugAssertion);

            if (version >= 770) {
                EnableFeature(GameFeature.GameOutfitIdU16);
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

            if (version >= 800) {
                EnableFeature(GameFeature.GameNPCInterface);
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
                EnableFeature(GameFeature.GameWrappableFurniture);
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
                EnableFeature(GameFeature.GameQuestTracker);
            }

            if (version >= 1132) {
                EnableFeature(GameFeature.GameCompendium);
            }
            
            if (version >= 1140) {
                EnableFeature(GameFeature.GamePlayerStateU32);
                EnableFeature(GameFeature.GameRewardWall);
                EnableFeature(GameFeature.GameAnalytics);
            }

            if (version >= 1150) {
                EnableFeature(GameFeature.GameQuickLoot);
                EnableFeature(GameFeature.GameExtendedCapacity);
                DisableFeature(GameFeature.GameTotalCapacity);
                EnableFeature(GameFeature.GameCyclopediaMonsters);
            }
            
            if (version >= 1180) {
                EnableFeature(GameFeature.GameStash);
                EnableFeature(GameFeature.GameCyclopediaMapAdditionalDetails);
            }
            
            if (version >= 1185) {
                DisableFeature(GameFeature.GameEnvironmentEffect);
                DisableFeature(GameFeature.GameObjectMarks);
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

        public void SetProtocolVersion(int version) {
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

        public void SetBuildVersion(int version) {
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

        public bool RegisterModule(Components.Base.Module module) {
            if (_modules.Contains(module))
                return false;
            
            _modules.Add(module);
            return true;
        }

        public bool UnregisterModule(Components.Base.Module module) {
            return _modules.Remove(module);
        }

        public T GetModule<T>() where T : Components.Base.Module {
            foreach (var module in _modules) {
                if (typeof(T) == module.GetType())
                    return module as T;
            }

            return null;
        }

        public T GetLastModule<T>() where T : Components.Base.Module {
            for (int i = _modules.Count - 1; i >= 0; i--) {
                var module = _modules[i];
                if (typeof(T) == module.GetType())
                    return module as T;
            }

            return null;
        }

        public List<T> GetModules<T>() where T : Components.Base.Module {
            List<T> modules = new List<T>();
            foreach (var module in _modules) {
                if (module is T result)
                    modules.Add(result);
            }

            return modules;
        }
    }
}