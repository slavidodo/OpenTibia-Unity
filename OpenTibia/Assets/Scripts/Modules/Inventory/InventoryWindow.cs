using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CommandBuffer = UnityEngine.Rendering.CommandBuffer;

namespace OpenTibiaUnity.Modules.Inventory
{
    public class InventoryWindow : Core.Components.Base.MiniWindow, IUseWidget, IMoveWidget, IWidgetContainerWidget
    {
        private const int ModernMaximizedWindowHeight = 158;
        private const int LegacyMaximizedWindowHeight = ModernMaximizedWindowHeight + 26;
        private const int ModernMinimizedWindowHeight = 58;
        private const int LegacyMinimizedWindowHeight = ModernMinimizedWindowHeight + 26;

        private static Core.Container.BodyContainerView BodyContainerView { get => OpenTibiaUnity.ContainerStorage.BodyContainerView; }
        private static Core.Options.OptionStorage OptionStorage { get => OpenTibiaUnity.OptionStorage; }

        #region Components
        [SerializeField] private Container.ItemView _headItemView = null;
        [SerializeField] private Container.ItemView _torsoItemView = null;
        [SerializeField] private Container.ItemView _legsItemView = null;
        [SerializeField] private Container.ItemView _feetItemView = null;
        [SerializeField] private Container.ItemView _neckItemView = null;
        [SerializeField] private Container.ItemView _leftHandItemView = null;
        [SerializeField] private Container.ItemView _fingerItemView = null;
        [SerializeField] private Container.ItemView _backItemView = null;
        [SerializeField] private Container.ItemView _rightHandItemView = null;
        [SerializeField] private Container.ItemView _hipItemView = null;

        [SerializeField] private Texture2D _headDefaultTexture = null;
        [SerializeField] private Texture2D _torsoDefaultTexture = null;
        [SerializeField] private Texture2D _legsDefaultTexture = null;
        [SerializeField] private Texture2D _feetDefaultTexture = null;
        [SerializeField] private Texture2D _neckDefaultTexture = null;
        [SerializeField] private Texture2D _leftHandDefaultTexture = null;
        [SerializeField] private Texture2D _fingerDefaultTexture = null;
        [SerializeField] private Texture2D _backpackDefaultTexture = null;
        [SerializeField] private Texture2D _rightHandDefaultTexture = null;
        [SerializeField] private Texture2D _hipDefaultTexture = null;
        
        [SerializeField] private TMPro.TextMeshProUGUI _capacityLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI _soulPointsLabel = null;

        [SerializeField] private Toggle _chaseOffToggle = null;
        [SerializeField] private Toggle _chaseOnToggle = null;

        [SerializeField] private Toggle _expertModeToggle = null;
        [SerializeField] private Toggle _expertModeSmallToggle = null;

        [SerializeField] private Toggle _pvpDoveToggle = null;
        [SerializeField] private Toggle _pvpWhiteHandToggle = null;
        [SerializeField] private Toggle _pvpYellowHandToggle = null;
        [SerializeField] private Toggle _pvpRedFistToggle = null;

        [SerializeField] private Toggle _attackOffensiveToggle = null;
        [SerializeField] private Toggle _attackBalancedToggle = null;
        [SerializeField] private Toggle _attackDefensiveToggle = null;

        [SerializeField] private Toggle _secureModeToggle = null;
        [SerializeField] private Toggle _secureModeLegacyToggle = null;

        [SerializeField] private Button _stopButton = null;
        [SerializeField] private Button _toggleStyleButton = null;
        [SerializeField] private Button _storeInboxButton = null;
        [SerializeField] private Button _storeInboxLegacyButton = null;
        [SerializeField] private Button _storeButton = null;
        [SerializeField] private Button _stopLegacyButton = null;
        [SerializeField] private Button _questsLegacyButton = null;
        [SerializeField] private Button _optionsLegacyButton = null;
        [SerializeField] private Button _helpLegacyButton = null;
        [SerializeField] private Button _logoutLegacyButton = null;

        [SerializeField] private Toggle _skillsLegacyToggle = null;
        [SerializeField] private Toggle _battleLegacyToggle = null;
        [SerializeField] private Toggle _buddyLegacyToggle = null;

        [SerializeField] private RectTransform _conditionsPanel = null;

        [SerializeField] private Sprite _minimizeSpriteNormal = null;
        [SerializeField] private Sprite _minimizeSpritePressed = null;
        [SerializeField] private Sprite _maximizeSpriteNormal = null;
        [SerializeField] private Sprite _maximizeSpritePressed = null;
        #endregion

        public Toggle skillsToggle { get => _skillsLegacyToggle; }
        public Toggle battleToggle { get => _battleLegacyToggle; }
        public Toggle buddyToggle { get => _buddyLegacyToggle; }

        protected MiniWindows.MiniWindowButtons miniWindowButtons {
            get => OpenTibiaUnity.GameManager.GetModule<MiniWindows.MiniWindowButtons>();
        }

        private int _slotUnderMouse = -1;
        private bool _isMinimized = false;
        private RenderTexture _slotsRenderTexture;
        private ObjectDragImpl<InventoryWindow> _dragHandler;
        private bool _handlingExpertPvPToggle = false;

        protected override void Awake() {
            base.Awake();
            BodyContainerView.onSlotChange.AddListener(OnInventorySlotChange);
            Creature.onSkillChange.AddListener(OnSkillChange);

            _slotsRenderTexture = new RenderTexture(Constants.FieldSize * (int)ClothSlots.Hip, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
            _slotsRenderTexture.filterMode = FilterMode.Bilinear;
            _slotsRenderTexture.Create();

            _dragHandler = new ObjectDragImpl<InventoryWindow>(this);
            ObjectMultiUseHandler.RegisterContainer(this);

            OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utils.EventImplPriority.Default, OnMouseUp);
            OpenTibiaUnity.GameManager.onTacticsChangeEvent.AddListener((attackMode, chaseMode, secureMode, pvpMode) => {
                SetAttackMode(attackMode, false, true);
                SetChaseMode(chaseMode, false, true);
                SetSecureMode(secureMode, false, true);
                SetPvPMode(pvpMode, false, true);
            });
        }

        protected override void Start() {
            base.Start();

            _headItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _headItemView.onPointerExit.AddListener(OnItemPointerExit);
            _neckItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _neckItemView.onPointerExit.AddListener(OnItemPointerExit);
            _backItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _backItemView.onPointerExit.AddListener(OnItemPointerExit);
            _torsoItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _torsoItemView.onPointerExit.AddListener(OnItemPointerExit);
            _rightHandItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _rightHandItemView.onPointerExit.AddListener(OnItemPointerExit);
            _leftHandItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _leftHandItemView.onPointerExit.AddListener(OnItemPointerExit);
            _legsItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _legsItemView.onPointerExit.AddListener(OnItemPointerExit);
            _feetItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _feetItemView.onPointerExit.AddListener(OnItemPointerExit);
            _fingerItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _fingerItemView.onPointerExit.AddListener(OnItemPointerExit);
            _hipItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            _hipItemView.onPointerExit.AddListener(OnItemPointerExit);

            _chaseOffToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.Off, true, false); });
            _chaseOnToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.On, true, false); });

            _attackOffensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Offensive, true, false); });
            _attackBalancedToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Balanced, true, false); });
            _attackDefensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Defensive, true, false); });

            _pvpDoveToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.Dove, true, false); });
            _pvpWhiteHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.WhiteHand, true, false); });
            _pvpYellowHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.YellowHand, true, false); });
            _pvpRedFistToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.RedFist, true, false); });

            _secureModeToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });
            _secureModeLegacyToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });

            _expertModeToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);
            _expertModeSmallToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);

            _stopButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            _stopLegacyButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            _toggleStyleButton.onClick.AddListener(ToggleStyle);

            _skillsLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnSkillsToggleValueChanged);
            _battleLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnBattleToggleValueChanged);
            _buddyLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnBuddyToggleValueChanged);
            _questsLegacyButton.onClick.AddListener(miniWindowButtons.OnQuestsButtonClicked);
            _optionsLegacyButton.onClick.AddListener(miniWindowButtons.OnOptionsButtonClicked);
            _helpLegacyButton.onClick.AddListener(miniWindowButtons.OnHelpButtonClicked);
            _logoutLegacyButton.onClick.AddListener(miniWindowButtons.OnLogoutButtonClicked);
            _storeButton.onClick.AddListener(miniWindowButtons.OnStoreButtonClicked);

            _storeInboxButton.onClick.AddListener(OnStoreInboxButtonClick);
            _storeInboxLegacyButton.onClick.AddListener(OnStoreInboxButtonClick);

            OpenTibiaUnity.GameManager.GetModule<GameWindow.GameMapContainer>().onInvalidateTRS.AddListener(OnInvalidateTRS);
        }

        private void OnGUI() {
            if (Event.current.type != EventType.Repaint || _minimized)
                return;

            var gameManager = OpenTibiaUnity.GameManager;
            InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true);

            var commandBuffer = new CommandBuffer();
            commandBuffer.SetRenderTarget(_slotsRenderTexture);
            commandBuffer.ClearRenderTarget(false, true, Core.Utils.GraphicsUtility.TransparentColor);

            var zoom = new Vector2(Screen.width / (float)_slotsRenderTexture.width, Screen.height / (float)_slotsRenderTexture.height);
            commandBuffer.SetViewMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, zoom) *
                OpenTibiaUnity.GameManager.MainCamera.worldToCameraMatrix);

            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var @object = BodyContainerView.Objects[i];
                if (@object) {
                    if (!@object.ClampeToFieldSize)
                        @object.ClampeToFieldSize = true;
                    @object.Animate(OpenTibiaUnity.TicksMillis);
                    @object.Draw(commandBuffer, new Vector2Int(Constants.FieldSize * i, 0), 0, 0, 0);
                }
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Dispose();
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            _slotsRenderTexture.Release();
            _slotsRenderTexture = null;

            var gameMapContainer = OpenTibiaUnity.GameManager?.GetModule<GameWindow.GameMapContainer>();
            if (gameMapContainer)
                gameMapContainer.onInvalidateTRS.RemoveListener(OnInvalidateTRS);
        }

        private void OnInvalidateTRS() {
            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var @object = BodyContainerView.Objects[i];
                if (@object)
                    @object.InvalidateTRS();
            }
        }

        public void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!_mouseCursorOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
                return false;

            var @object = GetObjectUnderMouse();
            var eventModifiers = OpenTibiaUnity.InputHandler.GetRawEventModifiers();
            var action = DetermineAction(mousePosition, mouseButton, eventModifiers, @object, applyAction, updateCursor);
            return action != AppearanceActions.None;
        }

        public AppearanceActions DetermineAction(Vector3 mousePosition, MouseButton mouseButton, EventModifiers eventModifiers, ObjectInstance @object, bool applyAction = false, bool updateCursor = false) {
            if (!@object)
                return AppearanceActions.None;

            if (updateCursor)
                updateCursor = OpenTibiaUnity.GameManager.ClientVersion >= 1100;

            var inputHandler = OpenTibiaUnity.InputHandler;
            if (inputHandler.IsMouseButtonDragged(MouseButton.Left) || inputHandler.IsMouseButtonDragged(MouseButton.Right))
                return AppearanceActions.None;

            var action = AppearanceActions.None;
            var optionStorage = OpenTibiaUnity.OptionStorage;

            if (optionStorage.MousePreset == MousePresets.LeftSmartClick) {
                if (eventModifiers == EventModifiers.None || eventModifiers == EventModifiers.Control) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                        var defaultAction = @object.Type.DefaultAction;
                        if (defaultAction == Protobuf.Shared.PlayerAction.Open)
                            action = AppearanceActions.Open;
                        else if (defaultAction == Protobuf.Shared.PlayerAction.Look)
                            action = AppearanceActions.Look;
                        else if (defaultAction == Protobuf.Shared.PlayerAction.Use)
                            action = AppearanceActions.Use;
                        else if (@object.Type.IsContainer)
                            action = AppearanceActions.Open;
                        else if (@object.Type.IsForceUse || @object.Type.IsMultiUse || @object.Type.IsUsable)
                            action = AppearanceActions.Use;
                        else
                            action = AppearanceActions.Look;
                    } else if (mouseButton == MouseButton.Right && eventModifiers == EventModifiers.None) {
                        action = AppearanceActions.ContextMenu;
                    }
                } else if (eventModifiers == EventModifiers.Shift) {
                    if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None)
                        action = AppearanceActions.Look;
                }

            } else if (optionStorage.MousePreset == MousePresets.Classic) {
                if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                    if (eventModifiers == EventModifiers.Shift)
                        action = AppearanceActions.Look;
                    else if (eventModifiers == EventModifiers.Control)
                        action = AppearanceActions.ContextMenu;
                    else if (eventModifiers == EventModifiers.Alt)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                    else if (eventModifiers == EventModifiers.Control)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Both) {
                    action = AppearanceActions.Look;
                }
            } else if (optionStorage.MousePreset == MousePresets.Regular) {
                if (mouseButton == MouseButton.Left || mouseButton == MouseButton.None) {
                    if (eventModifiers == EventModifiers.Shift)
                        action = AppearanceActions.Look;
                    else if (eventModifiers == EventModifiers.Control)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                    else if ((eventModifiers & EventModifiers.Alt) != 0 && (eventModifiers & EventModifiers.Shift) == 0)
                        action = AppearanceActions.ContextMenu;
                } else if (mouseButton == MouseButton.Right) {
                    if (eventModifiers == EventModifiers.None)
                        action = AppearanceActions.ContextMenu;
                    else if (eventModifiers == EventModifiers.Control)
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
                }
            }

            if (updateCursor)
                OpenTibiaUnity.GameManager.CursorController.SetCursorState(action, CursorPriority.Medium);

            if (applyAction) {
                var absolutePosition = new Vector3Int(65535, _slotUnderMouse, 0);
                switch (action) {
                    case AppearanceActions.None: break;
                    case AppearanceActions.ContextMenu:
                        OpenTibiaUnity.CreateObjectContextMenu(absolutePosition, @object, absolutePosition.z, @object, absolutePosition.z, null)
                            .Display(mousePosition);
                        break;
                    case AppearanceActions.Look:
                        new LookActionImpl(absolutePosition, @object.Type, absolutePosition.z).Perform();
                        break;
                    case AppearanceActions.Use:
                        if (@object.Type.IsMultiUse)
                            ObjectMultiUseHandler.Activate(absolutePosition, @object, absolutePosition.z);
                        else
                            new UseActionImpl(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        new UseActionImpl(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Unset:
                        break;
                }
            }

            return action;
        }

        protected override void OnClientVersionChange(int _, int __) {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            bool hasStoreButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStore) && OpenTibiaUnity.GameManager.ClientVersion < 1100;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);
            bool hasLegacyStoreInbox = hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100;

            int newHeight = hasExpertPvp ? ModernMaximizedWindowHeight : LegacyMaximizedWindowHeight;
            if (hasLegacyStoreInbox) {
                newHeight += 14;
                _stopButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, 14.0f);
            } else {
                _stopButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, 0.0f);
            }

            GetComponent<LayoutElement>().minHeight = newHeight;

            _stopButton.gameObject.SetActive(hasExpertPvp);
            _storeInboxButton.gameObject.SetActive(hasStoreInbox && !hasLegacyStoreInbox);
            _storeInboxLegacyButton.gameObject.SetActive(hasLegacyStoreInbox);
            _storeButton.gameObject.SetActive(hasStoreButton);
            _stopLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _questsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _optionsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _helpLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _logoutLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _skillsLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            _battleLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            _buddyLegacyToggle.gameObject.SetActive(!hasExpertPvp);

            _secureModeToggle.gameObject.SetActive(hasExpertPvp);
            _secureModeLegacyToggle.gameObject.SetActive(!hasExpertPvp);

            _expertModeToggle.gameObject.SetActive(hasExpertPvp);
            if (!hasExpertPvp)
                _pvpDoveToggle.transform.parent.gameObject.SetActive(false);
        }

        protected void OnSkillChange(Creature creature, SkillType skillType, Skill skill) {
            var player = creature as Player;
            if (!player)
                return;

            if (skillType == SkillType.Capacity) {
                if (player.FreeCapacity <= -1)
                    _capacityLabel.text = "<color=#00EB00>infinity</color>";
                else
                    _capacityLabel.text = string.Format("{0}", Core.Utils.Utility.Commafy(player.FreeCapacity / 1000));
            } else if (skillType == SkillType.SoulPoints) {
                _soulPointsLabel.text = string.Format("{0}", skill.Level);
            }
        }

        public void OnInventorySlotChange(ClothSlots slot, ObjectInstance @object) {
            switch (slot) {
                case ClothSlots.Head: InitialiseObjectImage(slot, _headItemView, _headDefaultTexture, @object); break;
                case ClothSlots.Neck: InitialiseObjectImage(slot, _neckItemView, _neckDefaultTexture, @object); break;
                case ClothSlots.Backpack: InitialiseObjectImage(slot, _backItemView, _backpackDefaultTexture, @object); break;
                case ClothSlots.Torso: InitialiseObjectImage(slot, _torsoItemView, _torsoDefaultTexture, @object); break;
                case ClothSlots.RightHand: InitialiseObjectImage(slot, _rightHandItemView, _rightHandDefaultTexture, @object); break;
                case ClothSlots.LeftHand: InitialiseObjectImage(slot, _leftHandItemView, _leftHandDefaultTexture, @object); break;
                case ClothSlots.Legs: InitialiseObjectImage(slot, _legsItemView, _legsDefaultTexture, @object); break;
                case ClothSlots.Feet: InitialiseObjectImage(slot, _feetItemView, _feetDefaultTexture, @object); break;
                case ClothSlots.Finger: InitialiseObjectImage(slot, _fingerItemView, _fingerDefaultTexture, @object); break;
                case ClothSlots.Hip: InitialiseObjectImage(slot, _hipItemView, _hipDefaultTexture, @object); break;
            }
        }

        private void InitialiseObjectImage(ClothSlots slot, Container.ItemView itemView, Texture2D defaultTexture, ObjectInstance @object) {
            if (slot > ClothSlots.Hip)
                return;

            itemView.objectInstance = @object;
            if (!!@object) {
                itemView.objectAmount = (int)@object.Data;
                itemView.showAmount = itemView.objectAmount > 1;

                itemView.itemImage.texture = _slotsRenderTexture;
                float w = (float)Constants.FieldSize / _slotsRenderTexture.width;
                itemView.itemImage.uvRect = new Rect(w * ((int)slot - 1), 0, w, 1);
            } else {
                itemView.itemImage.texture = defaultTexture;
                itemView.itemImage.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        public void SetChaseMode(CombatChaseModes chaseMode, bool send, bool toggle) {
            if (chaseMode != OptionStorage.CombatChaseMode)
                return;

            OptionStorage.CombatChaseMode = chaseMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                switch (chaseMode) {
                    case CombatChaseModes.On:
                        _chaseOnToggle.isOn = true;
                        break;
                    case CombatChaseModes.Off:
                        _chaseOffToggle.isOn = true;
                        break;

                    default:
                        return;
                }
            }
        }

        public void SetAttackMode(CombatAttackModes attackMode, bool send, bool toggle) {
            if (attackMode == OptionStorage.CombatAttackMode)
                return;

            OptionStorage.CombatAttackMode = attackMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                switch (attackMode) {
                    case CombatAttackModes.Offensive:
                        _attackOffensiveToggle.isOn = true;
                        break;
                    case CombatAttackModes.Balanced:
                        _attackBalancedToggle.isOn = true;
                        break;
                    case CombatAttackModes.Defensive:
                        _attackBalancedToggle.isOn = true;
                        break;

                    default:
                        return;
                }
            }
        }

        public void SetSecureMode(bool secureMode, bool send, bool toggle) {
            if (secureMode == OptionStorage.CombatSecureMode)
                return;

            OptionStorage.CombatSecureMode = secureMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                _secureModeToggle.isOn = !secureMode;
                _secureModeLegacyToggle.isOn = !secureMode;
            }
        }
        
        public void SetPvPMode(CombatPvPModes pvpMode, bool send, bool toggle) {
            if (pvpMode == OptionStorage.CombatPvPMode)
                return;

            // force toggle of e-pvp button
            if (pvpMode != CombatPvPModes.Dove) {
                _expertModeToggle.isOn = true;
                _expertModeSmallToggle.isOn = true;
            }

            OptionStorage.CombatPvPMode = pvpMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                _pvpDoveToggle.isOn = pvpMode == CombatPvPModes.Dove;
                _pvpWhiteHandToggle.isOn = pvpMode == CombatPvPModes.WhiteHand;
                _pvpYellowHandToggle.isOn = pvpMode == CombatPvPModes.YellowHand;
                _pvpRedFistToggle.isOn = pvpMode == CombatPvPModes.RedFist;
            }
        }

        public void ToggleStyle() {
            var spriteState = _toggleStyleButton.spriteState;
            var imageComponent = _toggleStyleButton.GetComponent<Image>();

            if (_isMinimized) {
                MaximizeStyle();
                imageComponent.sprite = _minimizeSpriteNormal;
                spriteState.pressedSprite = _minimizeSpritePressed;
            } else {
                MinimizeStyle();
                imageComponent.sprite = _maximizeSpriteNormal;
                spriteState.pressedSprite = _maximizeSpritePressed;
            }

            _toggleStyleButton.spriteState = spriteState;
            _isMinimized = !_isMinimized;
        }

        private void MinimizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            bool hasStoreButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStore)&& OpenTibiaUnity.GameManager.ClientVersion < 1100;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);
            bool hasPurseButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePurseSlot) && !hasStoreInbox;

            _headItemView.gameObject.SetActive(false);
            _torsoItemView.gameObject.SetActive(false);
            _legsItemView.gameObject.SetActive(false);
            _feetItemView.gameObject.SetActive(false);
            _backItemView.gameObject.SetActive(false);
            _rightHandItemView.gameObject.SetActive(false);
            _hipItemView.gameObject.SetActive(false);
            _neckItemView.gameObject.SetActive(false);
            _leftHandItemView.gameObject.SetActive(false);
            _fingerItemView.gameObject.SetActive(false);

            _secureModeToggle.gameObject.SetActive(false);
            _secureModeLegacyToggle.gameObject.SetActive(true);
            _expertModeToggle.gameObject.SetActive(false);
            _expertModeSmallToggle.gameObject.SetActive(hasExpertPvp);
            _storeInboxButton.gameObject.SetActive(false);

            _questsLegacyButton.gameObject.SetActive(false);
            _optionsLegacyButton.gameObject.SetActive(false);
            _helpLegacyButton.gameObject.SetActive(false);

            _stopButton.gameObject.SetActive(hasExpertPvp);
            _stopLegacyButton.gameObject.SetActive(!hasExpertPvp);
            
            _capacityLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(19.0f, 0.0f);
            _soulPointsLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(19.0f, -20.0f);
            
            _attackOffensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-92.0f, 0.0f);
            _attackBalancedToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-72.0f, -0.0f);
            _attackDefensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-52.0f, -0.0f);
            _chaseOffToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-92.0f, -23.0f);
            _chaseOnToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-72.0f, -23.0f);
            _secureModeLegacyToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-52.0f, -23.0f);
            _conditionsPanel.anchoredPosition = new Vector2(5.0f, -46.0f);
            _stopLegacyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, -0.0f);

            int newHeight = hasExpertPvp ? ModernMinimizedWindowHeight : LegacyMinimizedWindowHeight;
            if (hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100)
                newHeight += 14;

            layoutElement.preferredHeight = newHeight;
        }

        private void MaximizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.ClientVersion > 1000;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);

            _headItemView.gameObject.SetActive(true);
            _torsoItemView.gameObject.SetActive(true);
            _legsItemView.gameObject.SetActive(true);
            _feetItemView.gameObject.SetActive(true);
            _backItemView.gameObject.SetActive(true);
            _rightHandItemView.gameObject.SetActive(true);
            _hipItemView.gameObject.SetActive(true);
            _neckItemView.gameObject.SetActive(true);
            _leftHandItemView.gameObject.SetActive(true);
            _fingerItemView.gameObject.SetActive(true);

            _secureModeToggle.gameObject.SetActive(hasExpertPvp);
            _secureModeLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            _expertModeToggle.gameObject.SetActive(hasExpertPvp);
            _expertModeSmallToggle.gameObject.SetActive(false);

            _questsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _optionsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            _helpLegacyButton.gameObject.SetActive(!hasExpertPvp);
            
            _capacityLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(79.0f, -122.0f);
            _soulPointsLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(5.0f, -122.0f);
            
            _attackOffensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, 0.0f);
            _attackBalancedToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, -23.0f);
            _attackDefensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, -46.0f);
            _chaseOffToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, 0.0f);
            _chaseOnToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, -23.0f);
            _secureModeLegacyToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5f, -46.0f);
            _conditionsPanel.anchoredPosition = new Vector2(5.0f, -147.0f);
            _stopLegacyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, -69.0f);

            int newHeight = hasExpertPvp ? ModernMaximizedWindowHeight : LegacyMaximizedWindowHeight;
            if (hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100)
                newHeight += 14;

            GetComponent<LayoutElement>().minHeight = newHeight;
        }

        public ObjectInstance GetObjectUnderMouse() {
            if (_slotUnderMouse == -1)
                return null;

            return BodyContainerView.GetObject((ClothSlots)_slotUnderMouse);
        }

        public int GetTopObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return _slotUnderMouse;
        }

        public int GetUseObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return _slotUnderMouse;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            if (!@object.Type.IsMultiUse) {
                @object = null;
                return -1;
            }

            return _slotUnderMouse;
        }

        public int GetMoveObjectUnderPoint(Vector3 _, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(_, out @object);
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 _) {
            if (_slotUnderMouse == -1)
                return null;

            return new Vector3Int(65535, _slotUnderMouse, 0);
        }

        public Vector3Int? MousePositionToMapPosition(Vector3 _) {
            return null;
        }

        private void OnItemPointerEnter(ClothSlots slot) {
            _slotUnderMouse = (int)slot;
        }

        private void OnItemPointerExit(ClothSlots _) {
            _slotUnderMouse = -1;
        }

        private void OnExpertPvPToggleValueChanged(bool value) {
            if (_handlingExpertPvPToggle)
                return;

            try {
                _handlingExpertPvPToggle = true;
                _expertModeToggle.isOn = value;
                _expertModeSmallToggle.isOn = value;
                _pvpDoveToggle.transform.parent.gameObject.SetActive(value);

                SetPvPMode(CombatPvPModes.Dove, true, true);
            } catch (System.Exception) {
            } finally {
                _handlingExpertPvPToggle = false;
            }
        }

        private void OnStoreInboxButtonClick() {
            var storeInbox = BodyContainerView.GetObject(ClothSlots.StoreInbox);
            if (!!storeInbox) {
                var absolutePosition = new Vector3Int(65535, (int)ClothSlots.StoreInbox, 0);
                new UseActionImpl(absolutePosition, storeInbox.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
            }
        }
    }
}
