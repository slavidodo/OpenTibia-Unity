using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

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
        public Components.ExitWindow ExitWindowPrefab;
        public Components.PopupWindow PopupWindowPrefab;
        public Components.CharacterPanel CharacterPanelPrefab;
        public Components.CreatureStatusPanel CreatureStatusPanelPrefab;
        [Tooltip("Module.Console")] public Modules.Console.ChannelMessageLabel ChannelMessageLabelPrefab;
        [Tooltip("Module.Console")] public Modules.Console.ChannelButton ChannelButtonPrefab;
        public TMPro.TextMeshProUGUI LabelOnscreenMessageBoxPrefab;

        // UI Default Objects
        [Header("Canvases & Overlays")]
        public Canvas BackgroundCanvas;
        public RectTransform BackgroundPanelOverlay;
        public Canvas GameCanvas;
        public RectTransform GamePanelOverlay;

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
        public Material DefaultMaterial;
        public Material OutfitsMaterial;
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
        public Texture2D SampleTexture;
        public Sprite[] PartySprites;
        public Sprite[] PKSprites;
        public Sprite[] TypeSprites;
        public Sprite[] SpeechSprites;
        public Sprite[] GuildSprites;
#pragma warning restore CS0649 // never assigned to

        public RectTransform ActiveOverlay {
            get {
                if (BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundPanelOverlay;
                return GamePanelOverlay;
            }
        }

        public Canvas ActiveCanvas {
            get {
                if (BackgroundCanvas.gameObject.activeSelf)
                    return BackgroundCanvas;
                return GameCanvas;
            }
        }

        public bool IsGameRunning {
            get {
                return ProtocolGame != null && ProtocolGame.IsGameRunning;
            }
        }

        // private fields
        private Queue<UnityAction> m_ActionQueue;
        private Components.ExitWindow m_ExitWindow;
        private AssetBundleCreateRequest m_SpritesBundleLoadingRequest = null;
        private bool m_LastMiniMapSaveWasSuccessfull = false;

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

        System.Collections.IEnumerator Start() {
            LoadProtoAppearanaces();
            yield return LoadSpriteAssets();

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
            } else if (e.type == EventType.MouseDown) {
                InputHandler.OnMouseDown(e);
            } else if (e.type == EventType.MouseUp) {
                InputHandler.OnMouseUp(e);
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
            lock (m_ActionQueue) {
                while (m_ActionQueue.Count > 0)
                    m_ActionQueue.Dequeue().Invoke();
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

        public void LoadProtoAppearanaces() {
            TextAsset appearancesBinary = Resources.Load("appearances001") as TextAsset;
            if (!appearancesBinary)
                throw new System.Exception("GameManager.LoadSpriteAssets: Unable to appearances asset.");

            AppearanceStorage.SetProtoAppearances(Proto.Appearances001.Appearances.Parser.ParseFrom(appearancesBinary.bytes));
        }

        public System.Collections.IEnumerator LoadSpriteAssets() {
            m_SpritesBundleLoadingRequest = AssetBundle.LoadFromFileAsync(Path.Combine(Application.streamingAssetsPath, "sprites"));
            yield return m_SpritesBundleLoadingRequest;
            
            var assetBundle = m_SpritesBundleLoadingRequest.assetBundle;
            if (assetBundle == null) {
                Debug.LogWarning("Failed to load AssetBundle!");
                yield break;
            }

            TextAsset catalogContent = Resources.Load("catalog-content") as TextAsset;
            if (!catalogContent) {
                Debug.LogWarning("Failed to load AssetBundle!");
                assetBundle.Unload(true);
                yield break;
            }

            Appearances.SpritesProvider spriteProvider = new Appearances.SpritesProvider(assetBundle, catalogContent);
            AppearanceStorage.SetSpriteProvider(spriteProvider);

            BackgroundCenterPanel.gameObject.SetActive(true);
            LobbyPanel.gameObject.SetActive(true);
            LoadingAppearancesWindow.gameObject.SetActive(false);
        }

        public void ProcessGameStart() {
            PendingCharacterIndex = -1;

            BackgroundCanvas.gameObject.SetActive(false);
            GameCanvas.gameObject.SetActive(true);

            CharactersWindow.ProcessGameStart();
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(GameCanvas.gameObject);
        }

        public void ProcessGameEnd() {
            WorldMapRenderer.DestroyUIElements();

            BackgroundCanvas.gameObject.SetActive(true);
            GameCanvas.gameObject.SetActive(false);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(BackgroundCanvas.gameObject);

#if !UNITY_EDITOR
            OpenTibiaUnity.GameManager.SetApplicationTitle("OpenTibiaUnity");
#endif

            // show the characters window
            CharactersWindow.ResetToCenter();
            CharactersWindow.Show();

            // if there is a pending character, the characters window will take care
            // of the new logging process
            if (PendingCharacterIndex >= 0)
                CharactersWindow.TryToEnterGame(PendingCharacterIndex);
        }
        
        public void ProcessChangeCharacter() {
            CharactersWindow.ResetToCenter();
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
                EnableFeature(GameFeatures.GameIdleAnimations);
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