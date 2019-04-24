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
    public class TacticsChangeEvent : UnityEvent<CombatAttackModes, CombatChaseModes, bool, CombatPvPModes> { }
    public class VersionChangeEvent : UnityEvent<int, int> { }

    public class ElapsedEvent : UnityEvent { }

    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour {
        /// <summary>
        /// Gamemanager should only exist once..
        /// </summary>
        private static GameManager s_Instance;
        public static GameManager Instance {
            get {
                return s_Instance;
            }
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        public static extern bool SetWindowText(System.IntPtr hwnd, string lpString);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern System.IntPtr FindWindow(string className, string windowName);

        public Options.OptionStorage OptionStorage { get; private set; }
        public InputManagment.InputHandler InputHandler { get; private set; }
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
        public Network.ProtocolGame ProtocolGame { get; set; }
        public int PendingCharacterIndex { get; set; } = -1;
        public ElapsedEvent OnSecondaryTimeCheck { get; private set; }
        public TacticsChangeEvent OnTacticsChangeEvent { get; private set; }
        public System.IntPtr WindowPtr { get; private set; }
        public System.Collections.BitArray Features { get; private set; }
        public int ProtocolVersion { get; set; } = 0;
        public int ClientVersion { get; set; } = 0;
        public VersionChangeEvent onClientVersionChange { get; private set; }
        public VersionChangeEvent onProtocolVersionChange { get; private set; }

        // Serialiazable fields
#pragma warning disable CS0649 // never assigned to
        // Prefabs
        [Header("Prefabs")]
        public TMPro.TextMeshProUGUI DefaultLabel;
        public Button DefaultButton;
        public Button DefaulBlueButton;
        public Button DefaulGreenButton;
        public Button DefaulYellowButton;
        public GameObject HorizontalSeparator;
        public GameObject VerticalSeparator;
        public Components.ExitWindow ExitWindowPrefab;
        public Components.PopupWindow PopupWindowPrefab;
        public Components.CharacterPanel CharacterPanelPrefab;
        public Components.CreatureStatusPanel CreatureStatusPanelPrefab;
        [Tooltip("Module.Console")] public Modules.Console.ChannelMessageLabel ChannelMessageLabelPrefab;
        [Tooltip("Module.Console")] public Modules.Console.ChannelButton ChannelButtonPrefab;
        public TMPro.TextMeshProUGUI LabelOnscreenMessageBoxPrefab;
        public Components.SplitStackWindow SplitStackWindowPrefab;
        public GameObject ContextMenuBasePrefab;
        public LayoutElement ContextMenuItemPrefab;

        // UI Default Objects
        [Header("Canvases & Overlays")]
        public Canvas BackgroundCanvas;
        public Canvas BackgroundPanelBlocker;
        public Canvas GameCanvas;
        public Canvas GamePanelBlocker;
        public EventSystem EventSystem;

        [Header("Static Panels")]
        public RectTransform BackgroundCenterPanel;
        public RectTransform LobbyPanel;
        public RectTransform WorldMapRenderingPanel;
        public RectTransform OnscreenMessagesContainer;
        public RectTransform CreatureStatusContainer;

        [Header("Modules")]
        public Modules.Login.CharactersWindow CharactersWindow;
        public Modules.Login.LoginWindow LoginWindow;
        public Components.Base.Window LoadingAppearancesWindow;

        [Header("Materials")]
        public Material AppearanceTypeMaterial;
        public Material OutfitTypeMaterial;
        public Material MarksViewMaterial;
        public Material LightmapMaterial;
        public Material LightSurfaceMaterial;

        [Header("Render Textures")]
        public CustomRenderTexture WorldMapRenderingTexture;
        public RenderTexture MiniMapRenderingTexture;

        [Header("Main Elements")]
        public Camera MainCamera;

        [Header("Onscreen Static Labels")]
        public TMPro.TextMeshProUGUI LabelMessageBoxTop;
        public TMPro.TextMeshProUGUI LabelMessageBoxHigh;
        public TMPro.TextMeshProUGUI LabelMessageBoxLow;
        public TMPro.TextMeshProUGUI LabelMessageBoxBottom;

        [Header("Utilities")]
        public Utility.CursorController CursorController;

        [Header("Textures & Sprites")]
        public Texture2D MarksViewTexture;
        public Texture2D TileCursorTexture;
        public Sprite[] PartySprites;
        public Sprite[] PKSprites;
        public Sprite[] TypeSprites;
        public Sprite[] SpeechSprites;
        public Sprite[] GuildSprites;
#pragma warning restore CS0649 // never assigned to

        public Canvas ActiveBlocker {
            get {
                if (BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundPanelBlocker;
                return GamePanelBlocker;
            }
        }

        public Canvas ActiveCanvas {
            get {
                if (BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundCanvas;
                return GameCanvas;
            }
        }

        public GraphicRaycaster ActiveRaycaster {
            get => ActiveCanvas.GetComponent<GraphicRaycaster>();
        }

        public bool IsGameRunning {
            get => ProtocolGame != null && ProtocolGame.IsGameRunning;
        }

        public bool IsLoadingClientAssets {
            get => m_LoadingClientAssets;
        }

        // private fields
        private Queue<UnityAction> m_ActionQueue;
        private Components.ExitWindow m_ExitWindow;
        private bool m_LastMiniMapSaveWasSuccessfull = false;
        private bool m_LoadingClientAssets = false;

        protected void Awake() {
            // setup static fields
            s_Instance = this;
            OpenTibiaUnity.GraphicsVendor = SystemInfo.graphicsDeviceVendor;
            OpenTibiaUnity.GraphicsDevice = SystemInfo.graphicsDeviceName;
            OpenTibiaUnity.GraphicsVersion = SystemInfo.graphicsDeviceVersion;
            OpenTibiaUnity.MainThread = Thread.CurrentThread;

            WindowPtr = FindWindow(null, "OpenTibiaUnity");

            Features = new System.Collections.BitArray(new bool[(int)GameFeatures.LastGameFeature]);

            // setup threading mutex & lockers
            m_ActionQueue = new Queue<UnityAction>();

            // quit-notification related
            m_ExitWindow = null;
            OpenTibiaUnity.Quiting = false;

            // events
            OnSecondaryTimeCheck = new ElapsedEvent();
            OnTacticsChangeEvent = new TacticsChangeEvent();

            // setup ingame facilities
            OptionStorage = new Options.OptionStorage();
            InputHandler = new InputManagment.InputHandler();
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

            // initializing events
            onClientVersionChange = new VersionChangeEvent();
            onProtocolVersionChange = new VersionChangeEvent();
        }

        void Start() {
            InvokeRepeating("SecondaryTimerCheck", 0, 0.05f);
            InvokeRepeating("SaveMiniMap", 0, 0.5f);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.wantsToQuit += EditorApplication_wantsToQuit;
#else
            Application.wantsToQuit += Application_wantsToQuit;
#endif

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 1000;
        }

        public void FixedUpdate() {
            DequeueMainThreadActions();
        }

        private void OnGUI() {
            // use() can't be performed on a "layout" and "repaint"
            var e = Event.current;
            if (e.type == EventType.Layout || e.type == EventType.Repaint)
                return;

            if ((e.type == EventType.KeyDown || e.type == EventType.KeyUp) && e.keyCode != KeyCode.None) {
                InputHandler.OnKeyEvent(e);
                e.Use();
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

        public void OnDestroy() {
            // since we are using threads to do our work (networking, ..)
            // away of unity's default classes, we need to shutdown things
            // to avoid errors in the console while debugging

            if (ProtocolGame != null && ProtocolGame.IsGameRunning)
                ProtocolGame.Disconnect(); // force disconnection (by trying to logout)

            AppearanceStorage.UnloadSpriteProvider();

            OnSecondaryTimeCheck.RemoveAllListeners();
            CancelInvoke("SecondaryTimerCheck");
            CancelInvoke("SaveMiniMap");
            
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

        public void AddSecondaryTimerListener(UnityAction action) {
            OnSecondaryTimeCheck.AddListener(action);
        }

        public void RemoveSecondaryTimerListener(UnityAction action) {
            OnSecondaryTimeCheck.RemoveListener(action);
        }

        private void SecondaryTimerCheck() {
            OnSecondaryTimeCheck.Invoke();
        }

        public void SaveMiniMap() {
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

        public void InvokeOnMainThread(UnityAction action) {
            lock (m_ActionQueue)
                m_ActionQueue.Enqueue(action);
        }

        public void DequeueMainThreadActions() {
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

        private bool EditorApplication_wantsToQuit() {
            return Application_wantsToQuit();
        }

        public void ShowQuitNotification() {
            // are you sure you want to exit?
            if (m_ExitWindow == null) {
                m_ExitWindow = Instantiate(ExitWindowPrefab, GameCanvas.transform);
                (m_ExitWindow.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
            } else if (m_ExitWindow.gameObject.activeSelf == false) {
                m_ExitWindow.gameObject.SetActive(true);
                (m_ExitWindow.transform as RectTransform).anchoredPosition = new Vector2(0, 0);
            }
        }

        public async void LoadThingsAsync(int version) {
            if (m_LoadingClientAssets)
                return;

            m_LoadingClientAssets = true;
            try {
                AppearanceStorage.Unload();
                LoadProtoAppearanaces(version);
                await LoadSpriteAssets(version);
            } catch (System.Exception) {}
            m_LoadingClientAssets = false;
        }

        protected void LoadProtoAppearanaces(int version) {
            try {
                byte[] bytes = File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, version.ToString(), "appearances.dat"));
                AppearanceStorage.SetProtoAppearances(Proto.Appearances.Appearances.Parser.ParseFrom(bytes));
            } catch (System.Exception e) {
                throw new System.Exception(string.Format("Unable to appearances.dat ({0}).", e.Message));
            }
        }

        protected async Task LoadSpriteAssets(int version) {
            string path = Path.Combine(Application.streamingAssetsPath, version.ToString(), "sprites");
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("Couldn't find the sprites assetbundle ({0}).", path));
            
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
                await Task.Yield();

            var assetBundle = request.assetBundle;
            if (assetBundle == null)
                throw new System.Exception("Unable to load asset bundle.");

            try {
                string catalogJson = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, version.ToString(), "catalog-content.json"));
                Appearances.SpritesProvider spriteProvider = new Appearances.SpritesProvider(assetBundle, catalogJson);
                AppearanceStorage.SetSpriteProvider(spriteProvider);
            } catch (System.Exception e) {
                assetBundle.Unload(true);
                throw new System.Exception(string.Format("Unable to catalog-content.json ({0}).", e.Message));
            }
        }

        public void ProcessGamePending() {
            // onGamePending.Invoke();

            ProtocolGame.SendEnterGame();
        }

        public void ProcessGameStart() {
            // onGameStart.Invoke();

            PendingCharacterIndex = -1;

            BackgroundCanvas.gameObject.SetActive(false);
            GameCanvas.gameObject.SetActive(true);

            CharactersWindow.ProcessGameStart();
            EventSystem.SetSelectedGameObject(GameCanvas.gameObject);
        }

        public void ProcessGameEnd() {
            // onGameEnd.Invoke();

            WorldMapRenderer.DestroyUIElements();

            CursorController.SetCursorState(CursorState.Default, CursorPriority.Low);

            BackgroundCanvas.gameObject.SetActive(true);
            GameCanvas.gameObject.SetActive(false);
            EventSystem.SetSelectedGameObject(BackgroundCanvas.gameObject);

#if !UNITY_EDITOR
            OpenTibiaUnity.GameManager.SetApplicationTitle(Application.productName);
#endif

            // show the characters window
            CharactersWindow.ResetLocalPosition();
            CharactersWindow.Show();

            // if there is a pending character, the characters window will take care
            // of the new logging process
            if (PendingCharacterIndex >= 0)
                CharactersWindow.TryToEnterGame(PendingCharacterIndex);
        }
        
        public void ProcessChangeCharacter() {
            CharactersWindow.ResetLocalPosition();
            CharactersWindow.Show();
        }

        public void SetApplicationTitle(string title) {
            SetWindowText(WindowPtr, title);
        }

        public void EnableFeature(GameFeatures feature) => Features.Set((int)feature, true);
        public void DisableFeature(GameFeatures feature) => Features.Set((int)feature, false);
        public bool GetFeature(GameFeatures feature) => Features.Get((int)feature);

        public void SetClientVersion(int version) {
            if (ClientVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change client version while online");

            if (version != 0 && (version < 740 || version > 1099))
                throw new System.Exception(string.Format("Client version {0} not supported", version));

            Features.SetAll(false); // reset all

            if (version >= 770) {
                EnableFeature(GameFeatures.GameLooktypeU16);
                EnableFeature(GameFeatures.GameMessageStatements);
                EnableFeature(GameFeatures.GameLoginPacketEncryption);
            }

            if (version >= 780) {
                EnableFeature(GameFeatures.GamePlayerAddons);
                EnableFeature(GameFeatures.GamePlayerStamina);
                EnableFeature(GameFeatures.GameNewFluids);
                EnableFeature(GameFeatures.GameMessageLevel);
                EnableFeature(GameFeatures.GamePlayerStateU16);
                EnableFeature(GameFeatures.GameNewOutfitProtocol);
            }

            if (version >= 790) {
                EnableFeature(GameFeatures.GameWritableDate);
            }

            if (version >= 840) {
                EnableFeature(GameFeatures.GameProtocolChecksum);
                EnableFeature(GameFeatures.GameAccountNames);
                EnableFeature(GameFeatures.GameDoubleFreeCapacity);
            }

            if (version >= 841) {
                EnableFeature(GameFeatures.GameChallengeOnLogin);
                EnableFeature(GameFeatures.GameMessageSizeCheck);
            }

            if (version >= 854) {
                EnableFeature(GameFeatures.GameCreatureEmblems);
            }

            if (version >= 860) {
                EnableFeature(GameFeatures.GameAttackSeq);
            }

            if (version >= 862) {
                EnableFeature(GameFeatures.GamePenalityOnDeath);
            }

            if (version >= 870) {
                EnableFeature(GameFeatures.GameDoubleExperience);
                EnableFeature(GameFeatures.GamePlayerMounts);
                EnableFeature(GameFeatures.GameSpellList);
            }

            if (version >= 910) {
                EnableFeature(GameFeatures.GameNameOnNpcTrade);
                EnableFeature(GameFeatures.GameTotalCapacity);
                EnableFeature(GameFeatures.GameSkillsBase);
                EnableFeature(GameFeatures.GamePlayerRegenerationTime);
                EnableFeature(GameFeatures.GameChannelPlayerList);
                EnableFeature(GameFeatures.GameEnvironmentEffect);
                EnableFeature(GameFeatures.GameItemAnimationPhase);
            }

            if (version >= 940) {
                EnableFeature(GameFeatures.GamePlayerMarket);
            }

            if (version >= 953) {
                EnableFeature(GameFeatures.GamePurseSlot);
                EnableFeature(GameFeatures.GameClientPing);
            }

            if (version >= 960) {
                EnableFeature(GameFeatures.GameSpritesU32);
                EnableFeature(GameFeatures.GameOfflineTrainingTime);
            }

            if (version >= 963) {
                EnableFeature(GameFeatures.GameAdditionalVipInfo);
            }

            if (version >= 980) {
                EnableFeature(GameFeatures.GamePreviewState);
                EnableFeature(GameFeatures.GameClientVersion);
            }

            if (version >= 981) {
                EnableFeature(GameFeatures.GameLoginPending);
                EnableFeature(GameFeatures.GameNewSpeedLaw);
            }

            if (version >= 984) {
                EnableFeature(GameFeatures.GameContainerPagination);
                EnableFeature(GameFeatures.GameBrowseField);
            }

            if (version >= 1000) {
                EnableFeature(GameFeatures.GameThingMarks);
                EnableFeature(GameFeatures.GamePVPMode);
            }

            if (version >= 1035) {
                EnableFeature(GameFeatures.GameDoubleSkills);
                EnableFeature(GameFeatures.GameBaseSkillU16);
            }

            if (version >= 1036) {
                EnableFeature(GameFeatures.GameCreatureIcons);
                EnableFeature(GameFeatures.GameHideNpcNames);
            }

            if (version >= 1038) {
                EnableFeature(GameFeatures.GamePremiumExpiration);
            }

            if (version >= 1050) {
                EnableFeature(GameFeatures.GameEnhancedAnimations);
            }

            if (version >= 1053) {
                EnableFeature(GameFeatures.GameUnjustifiedPoints);
            }

            if (version >= 1054) {
                EnableFeature(GameFeatures.GameExperienceBonus);
            }

            if (version >= 1055) {
                EnableFeature(GameFeatures.GameDeathType);
            }

            if (version >= 1057) {
                EnableFeature(GameFeatures.GameSeparateAnimationGroups);
            }

            if (version >= 1061) {
                EnableFeature(GameFeatures.GameOGLInformation);
            }

            if (version >= 1071) {
                EnableFeature(GameFeatures.GameContentRevision);
            }

            if (version >= 1072) {
                EnableFeature(GameFeatures.GameAuthenticator);
            }

            if (version >= 1074) {
                EnableFeature(GameFeatures.GameSessionKey);
            }

            if (version >= 1080) {
                EnableFeature(GameFeatures.GameIngameStore);
            }

            if (version >= 1092) {
                EnableFeature(GameFeatures.GameIngameStoreServiceType);
            }

            if (version >= 1093) {
                EnableFeature(GameFeatures.GameIngameStoreHighlights);
            }

            if (version >= 1094) {
                EnableFeature(GameFeatures.GameAdditionalSkills);
            }

            if (version >= 1102) {
                EnableFeature(GameFeatures.GameWorldName);
            }

            if (version >= 1132) {
                DisableFeature(GameFeatures.GameProtocolChecksum);
                EnableFeature(GameFeatures.GameProtocolSequenceNumber);
            }

            int oldVersion = ClientVersion;
            ClientVersion = version;

            onClientVersionChange.Invoke(oldVersion, ClientVersion);
        }

        public void SetProtocolVersion(int version) {
            if (ProtocolVersion == version)
                return;

            if (IsGameRunning)
                throw new System.Exception("Unable to change client version while online");

            if (version != 0 && (version < 740 || version > 1099))
                throw new System.Exception(string.Format("Client version {0} not supported", version));

            int oldVersion = ProtocolVersion;
            ProtocolVersion = version;

            onProtocolVersionChange.Invoke(oldVersion, ProtocolVersion);
        }
    }
}