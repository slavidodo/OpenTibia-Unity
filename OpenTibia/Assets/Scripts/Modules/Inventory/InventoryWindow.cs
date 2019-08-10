using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using OpenTibiaUnity.Core.Input.GameAction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Inventory
{
    internal class InventoryWindow : Core.Components.Base.MiniWindow, IUseWidget, IMoveWidget, IWidgetContainerWidget
    {
        private const int ModernMaximizedWindowHeight = 158;
        private const int LegacyMaximizedWindowHeight = ModernMaximizedWindowHeight + 26;
        private const int ModernMinimizedWindowHeight = 58;
        private const int LegacyMinimizedWindowHeight = ModernMinimizedWindowHeight + 26;

        private static Core.Container.BodyContainerView BodyContainerView { get => OpenTibiaUnity.ContainerStorage.BodyContainerView; }
        private static Core.Options.OptionStorage OptionStorage { get => OpenTibiaUnity.OptionStorage; }

        #region Components
        [SerializeField] private Container.ItemView m_HeadItemView = null;
        [SerializeField] private Container.ItemView m_TorsoItemView = null;
        [SerializeField] private Container.ItemView m_LegsItemView = null;
        [SerializeField] private Container.ItemView m_FeetItemView = null;
        [SerializeField] private Container.ItemView m_NeckItemView = null;
        [SerializeField] private Container.ItemView m_LeftHandItemView = null;
        [SerializeField] private Container.ItemView m_FingerItemView = null;
        [SerializeField] private Container.ItemView m_BackItemView = null;
        [SerializeField] private Container.ItemView m_RightHandItemView = null;
        [SerializeField] private Container.ItemView m_HipItemView = null;

        [SerializeField] private Texture2D m_HeadDefaultTexture = null;
        [SerializeField] private Texture2D m_TorsoDefaultTexture = null;
        [SerializeField] private Texture2D m_LegsDefaultTexture = null;
        [SerializeField] private Texture2D m_FeetDefaultTexture = null;
        [SerializeField] private Texture2D m_NeckDefaultTexture = null;
        [SerializeField] private Texture2D m_LeftHandDefaultTexture = null;
        [SerializeField] private Texture2D m_FingerDefaultTexture = null;
        [SerializeField] private Texture2D m_BackpackDefaultTexture = null;
        [SerializeField] private Texture2D m_RightHandDefaultTexture = null;
        [SerializeField] private Texture2D m_HipDefaultTexture = null;
        
        [SerializeField] private TMPro.TextMeshProUGUI m_CapacityLabel = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_SoulPointsLabel = null;

        [SerializeField] private Toggle m_ChaseOffToggle = null;
        [SerializeField] private Toggle m_ChaseOnToggle = null;

        [SerializeField] private Toggle m_ExpertModeToggle = null;
        [SerializeField] private Toggle m_ExpertModeSmallToggle = null;

        [SerializeField] private Toggle m_PvpDoveToggle = null;
        [SerializeField] private Toggle m_PvpWhiteHandToggle = null;
        [SerializeField] private Toggle m_PvpYellowHandToggle = null;
        [SerializeField] private Toggle m_PvpRedFistToggle = null;

        [SerializeField] private Toggle m_AttackOffensiveToggle = null;
        [SerializeField] private Toggle m_AttackBalancedToggle = null;
        [SerializeField] private Toggle m_AttackDefensiveToggle = null;

        [SerializeField] private Toggle m_SecureModeToggle = null;
        [SerializeField] private Toggle m_SecureModeLegacyToggle = null;

        [SerializeField] private Button m_StopButton = null;
        [SerializeField] private Button m_ToggleStyleButton = null;
        [SerializeField] private Button m_StoreInboxButton = null;
        [SerializeField] private Button m_StoreInboxLegacyButton = null;
        [SerializeField] private Button m_StoreButton = null;
        [SerializeField] private Button m_StopLegacyButton = null;
        [SerializeField] private Button m_QuestsLegacyButton = null;
        [SerializeField] private Button m_OptionsLegacyButton = null;
        [SerializeField] private Button m_HelpLegacyButton = null;
        [SerializeField] private Button m_LogoutLegacyButton = null;

        [SerializeField] private Toggle m_SkillsLegacyToggle = null;
        [SerializeField] private Toggle m_BattleLegacyToggle = null;
        [SerializeField] private Toggle m_BuddyLegacyToggle = null;

        [SerializeField] private RectTransform m_ConditionsPanel = null;

        [SerializeField] private Sprite m_MinimizeSpriteNormal = null;
        [SerializeField] private Sprite m_MinimizeSpritePressed = null;
        [SerializeField] private Sprite m_MaximizeSpriteNormal = null;
        [SerializeField] private Sprite m_MaximizeSpritePressed = null;
        #endregion

        public Toggle skillsToggle { get => m_SkillsLegacyToggle; }
        public Toggle battleToggle { get => m_BattleLegacyToggle; }
        public Toggle buddyToggle { get => m_BuddyLegacyToggle; }

        protected MiniWindows.MiniWindowButtons miniWindowButtons {
            get => OpenTibiaUnity.GameManager.GetModule<MiniWindows.MiniWindowButtons>();
        }

        private int m_SlotUnderMouse = -1;
        private bool m_IsMinimized = false;
        private RenderTexture m_SlotsRenderTexture;
        private ObjectDragImpl<InventoryWindow> m_DragHandler;
        private bool m_HandlingExpertPvPToggle = false;

        protected override void Awake() {
            base.Awake();
            BodyContainerView.onSlotChange.AddListener(OnInventorySlotChange);
            Creature.onSkillChange.AddListener(OnSkillChange);

            m_SlotsRenderTexture = new RenderTexture(Constants.FieldSize * (int)ClothSlots.Hip, Constants.FieldSize, 0, RenderTextureFormat.ARGB32);
            m_DragHandler = new ObjectDragImpl<InventoryWindow>(this);
            ObjectMultiUseHandler.RegisterContainer(this);

            OpenTibiaUnity.InputHandler.AddMouseUpListener(Core.Utility.EventImplPriority.Default, OnMouseUp);
        }

        protected override void Start() {
            base.Start();

            m_HeadItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_HeadItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_NeckItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_NeckItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_BackItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_BackItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_TorsoItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_TorsoItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_RightHandItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_RightHandItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_LeftHandItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_LeftHandItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_LegsItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_LegsItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_FeetItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_FeetItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_FingerItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_FingerItemView.onPointerExit.AddListener(OnItemPointerExit);
            m_HipItemView.onPointerEnter.AddListener(OnItemPointerEnter);
            m_HipItemView.onPointerExit.AddListener(OnItemPointerExit);

            OpenTibiaUnity.GameManager.onTacticsChangeEvent.AddListener((attackMode, chaseMode, secureMode, pvpMode) => {
                SetAttackMode(attackMode, false, true);
                SetChaseMode(chaseMode, false, true);
                SetSecureMode(secureMode, false, true);
                SetPvPMode(pvpMode, false, true);
            });

            m_ChaseOffToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.Off, true, false); });
            m_ChaseOnToggle.onValueChanged.AddListener((value) => { if (value) SetChaseMode(CombatChaseModes.On, true, false); });

            m_AttackOffensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Offensive, true, false); });
            m_AttackBalancedToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Balanced, true, false); });
            m_AttackDefensiveToggle.onValueChanged.AddListener((value) => { if (value) SetAttackMode(CombatAttackModes.Defensive, true, false); });

            m_PvpDoveToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.Dove, true, false); });
            m_PvpWhiteHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.WhiteHand, true, false); });
            m_PvpYellowHandToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.YellowHand, true, false); });
            m_PvpRedFistToggle.onValueChanged.AddListener((value) => { if (value) SetPvPMode(CombatPvPModes.RedFist, true, false); });

            m_SecureModeToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });
            m_SecureModeLegacyToggle.onValueChanged.AddListener((value) => { SetSecureMode(!value, true, false); });

            m_ExpertModeToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);
            m_ExpertModeSmallToggle.onValueChanged.AddListener(OnExpertPvPToggleValueChanged);

            m_StopButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            m_StopLegacyButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            m_ToggleStyleButton.onClick.AddListener(ToggleStyle);

            m_SkillsLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnSkillsToggleValueChanged);
            m_BattleLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnBattleToggleValueChanged);
            m_BuddyLegacyToggle.onValueChanged.AddListener(miniWindowButtons.OnBuddyToggleValueChanged);
            m_QuestsLegacyButton.onClick.AddListener(miniWindowButtons.OnQuestsButtonClicked);
            m_OptionsLegacyButton.onClick.AddListener(miniWindowButtons.OnOptionsButtonClicked);
            m_HelpLegacyButton.onClick.AddListener(miniWindowButtons.OnHelpButtonClicked);
            m_LogoutLegacyButton.onClick.AddListener(miniWindowButtons.OnLogoutButtonClicked);
        }

        private void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            var gameManager = OpenTibiaUnity.GameManager;
            InternalStartMouseAction(Input.mousePosition, MouseButton.None, false, true);

            Vector2 zoom = new Vector2(Screen.width / (float)m_SlotsRenderTexture.width, Screen.height / (float)m_SlotsRenderTexture.height);

            m_SlotsRenderTexture.Release();
            RenderTexture.active = m_SlotsRenderTexture;
            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var @object = BodyContainerView.Objects[i];
                if (@object) {
                    @object.Animate(OpenTibiaUnity.TicksMillis);
                    @object.DrawTo(new Vector2(Constants.FieldSize * i, 0), zoom, 0, 0, 0);
                }
            }

            RenderTexture.active = null;
        }

        protected override void OnDestroy() {
            base.OnDestroy();

            m_SlotsRenderTexture.Release();
        }

        public void OnMouseUp(Event e, MouseButton mouseButton, bool repeat) {
            if (InternalStartMouseAction(e.mousePosition, mouseButton, true, false))
                e.Use();
        }

        private bool InternalStartMouseAction(Vector3 mousePosition, MouseButton mouseButton, bool applyAction = false, bool updateCursor = false) {
            var gameManager = OpenTibiaUnity.GameManager;
            if (!m_MouseCursorOverRenderer || !gameManager.GameCanvas.gameObject.activeSelf || gameManager.GamePanelBlocker.gameObject.activeSelf)
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
                        action = @object.Type.IsContainer ? AppearanceActions.Open : AppearanceActions.Use;
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
                var absolutePosition = new Vector3Int(65535, m_SlotUnderMouse, 0);
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
                            GameActionFactory.CreateUseAction(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
                        break;
                    case AppearanceActions.Open:
                        GameActionFactory.CreateUseAction(absolutePosition, @object.Type, absolutePosition.z, Vector3Int.zero, null, 0, UseActionTarget.Auto).Perform();
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
                m_StopButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, 14.0f);
            } else {
                m_StopButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, 0.0f);
            }

            GetComponent<LayoutElement>().minHeight = newHeight;

            m_StopButton.gameObject.SetActive(hasExpertPvp);
            m_StoreInboxButton.gameObject.SetActive(hasStoreInbox && !hasLegacyStoreInbox);
            m_StoreInboxLegacyButton.gameObject.SetActive(hasLegacyStoreInbox);
            m_StoreButton.gameObject.SetActive(hasStoreButton);
            m_StopLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_QuestsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_OptionsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_HelpLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_LogoutLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_SkillsLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            m_BattleLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            m_BuddyLegacyToggle.gameObject.SetActive(!hasExpertPvp);

            m_SecureModeToggle.gameObject.SetActive(hasExpertPvp);
            m_SecureModeLegacyToggle.gameObject.SetActive(!hasExpertPvp);

            m_ExpertModeToggle.gameObject.SetActive(hasExpertPvp);
            if (!hasExpertPvp)
                m_PvpDoveToggle.transform.parent.gameObject.SetActive(false);
        }

        protected void OnSkillChange(Creature creature, SkillType skillType, Skill skill) {
            var player = creature as Player;
            if (!player)
                return;

            if (skillType == SkillType.Capacity) {
                if (player.FreeCapacity <= -1)
                    m_CapacityLabel.text = "<color=#00EB00>infinity</color>";
                else
                    m_CapacityLabel.text = string.Format("{0}", Utility.Commafy(player.FreeCapacity / 1000));
            } else if (skillType == SkillType.SoulPoints) {
                m_SoulPointsLabel.text = string.Format("{0}", skill.Level);
            }
        }

        public void OnInventorySlotChange(ClothSlots slot, ObjectInstance @object) {
            switch (slot) {
                case ClothSlots.Head: InitialiseObjectImage(slot, m_HeadItemView, m_HeadDefaultTexture, @object); break;
                case ClothSlots.Neck: InitialiseObjectImage(slot, m_NeckItemView, m_NeckDefaultTexture, @object); break;
                case ClothSlots.Backpack: InitialiseObjectImage(slot, m_BackItemView, m_BackpackDefaultTexture, @object); break;
                case ClothSlots.Torso: InitialiseObjectImage(slot, m_TorsoItemView, m_TorsoDefaultTexture, @object); break;
                case ClothSlots.RightHand: InitialiseObjectImage(slot, m_RightHandItemView, m_RightHandDefaultTexture, @object); break;
                case ClothSlots.LeftHand: InitialiseObjectImage(slot, m_LeftHandItemView, m_LeftHandDefaultTexture, @object); break;
                case ClothSlots.Legs: InitialiseObjectImage(slot, m_LegsItemView, m_LegsDefaultTexture, @object); break;
                case ClothSlots.Feet: InitialiseObjectImage(slot, m_FeetItemView, m_FeetDefaultTexture, @object); break;
                case ClothSlots.Finger: InitialiseObjectImage(slot, m_FingerItemView, m_FingerDefaultTexture, @object); break;
                case ClothSlots.Hip: InitialiseObjectImage(slot, m_HipItemView, m_HipDefaultTexture, @object); break;
            }
        }

        private void InitialiseObjectImage(ClothSlots slot, Container.ItemView itemView, Texture2D defaultTexture, ObjectInstance @object) {
            if (slot > ClothSlots.Hip)
                return;

            itemView.objectInstance = @object;
            if (!!@object) {
                itemView.objectAmount = (int)@object.Data;
                itemView.showAmount = itemView.objectAmount > 1;

                itemView.itemImage.texture = m_SlotsRenderTexture;
                float w = (float)Constants.FieldSize / m_SlotsRenderTexture.width;
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
                        m_ChaseOnToggle.isOn = true;
                        break;
                    case CombatChaseModes.Off:
                        m_ChaseOffToggle.isOn = true;
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
                        m_AttackOffensiveToggle.isOn = true;
                        break;
                    case CombatAttackModes.Balanced:
                        m_AttackBalancedToggle.isOn = true;
                        break;
                    case CombatAttackModes.Defensive:
                        m_AttackBalancedToggle.isOn = true;
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
                m_SecureModeToggle.isOn = !secureMode;
                m_SecureModeLegacyToggle.isOn = !secureMode;
            }
        }
        
        public void SetPvPMode(CombatPvPModes pvpMode, bool send, bool toggle) {
            if (pvpMode == OptionStorage.CombatPvPMode)
                return;

            // force toggle of e-pvp button
            if (pvpMode != CombatPvPModes.Dove) {
                m_ExpertModeToggle.isOn = true;
                m_ExpertModeSmallToggle.isOn = true;
            }

            OptionStorage.CombatPvPMode = pvpMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                m_PvpDoveToggle.isOn = pvpMode == CombatPvPModes.Dove;
                m_PvpWhiteHandToggle.isOn = pvpMode == CombatPvPModes.WhiteHand;
                m_PvpYellowHandToggle.isOn = pvpMode == CombatPvPModes.YellowHand;
                m_PvpRedFistToggle.isOn = pvpMode == CombatPvPModes.RedFist;
            }
        }

        public void ToggleStyle() {
            var spriteState = m_ToggleStyleButton.spriteState;
            var imageComponent = m_ToggleStyleButton.GetComponent<Image>();

            if (m_IsMinimized) {
                MaximizeStyle();
                imageComponent.sprite = m_MinimizeSpriteNormal;
                spriteState.pressedSprite = m_MinimizeSpritePressed;
            } else {
                MinimizeStyle();
                imageComponent.sprite = m_MaximizeSpriteNormal;
                spriteState.pressedSprite = m_MaximizeSpritePressed;
            }

            m_ToggleStyleButton.spriteState = spriteState;
            m_IsMinimized = !m_IsMinimized;
        }

        private void MinimizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePVPMode);
            bool hasStoreButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameIngameStore)&& OpenTibiaUnity.GameManager.ClientVersion < 1100;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);
            bool hasPurseButton = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePurseSlot) && !hasStoreInbox;

            m_HeadItemView.gameObject.SetActive(false);
            m_TorsoItemView.gameObject.SetActive(false);
            m_LegsItemView.gameObject.SetActive(false);
            m_FeetItemView.gameObject.SetActive(false);
            m_BackItemView.gameObject.SetActive(false);
            m_RightHandItemView.gameObject.SetActive(false);
            m_HipItemView.gameObject.SetActive(false);
            m_NeckItemView.gameObject.SetActive(false);
            m_LeftHandItemView.gameObject.SetActive(false);
            m_FingerItemView.gameObject.SetActive(false);

            m_SecureModeToggle.gameObject.SetActive(false);
            m_SecureModeLegacyToggle.gameObject.SetActive(true);
            m_ExpertModeToggle.gameObject.SetActive(false);
            m_ExpertModeSmallToggle.gameObject.SetActive(hasExpertPvp);
            m_StoreInboxButton.gameObject.SetActive(false);

            m_QuestsLegacyButton.gameObject.SetActive(false);
            m_OptionsLegacyButton.gameObject.SetActive(false);
            m_HelpLegacyButton.gameObject.SetActive(false);

            m_StopButton.gameObject.SetActive(hasExpertPvp);
            m_StopLegacyButton.gameObject.SetActive(!hasExpertPvp);
            
            m_CapacityLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(19.0f, 0.0f);
            m_SoulPointsLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(19.0f, -20.0f);
            
            m_AttackOffensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-92.0f, 0.0f);
            m_AttackBalancedToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-72.0f, -0.0f);
            m_AttackDefensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-52.0f, -0.0f);
            m_ChaseOffToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-92.0f, -23.0f);
            m_ChaseOnToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-72.0f, -23.0f);
            m_SecureModeLegacyToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-52.0f, -23.0f);
            m_ConditionsPanel.anchoredPosition = new Vector2(5.0f, -46.0f);
            m_StopLegacyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, -0.0f);

            int newHeight = hasExpertPvp ? ModernMinimizedWindowHeight : LegacyMinimizedWindowHeight;
            if (hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100)
                newHeight += 14;

            layoutElement.preferredHeight = newHeight;
        }

        private void MaximizeStyle() {
            bool hasExpertPvp = OpenTibiaUnity.GameManager.ClientVersion > 1000;
            bool hasStoreInbox = OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameStoreInboxSlot);

            m_HeadItemView.gameObject.SetActive(true);
            m_TorsoItemView.gameObject.SetActive(true);
            m_LegsItemView.gameObject.SetActive(true);
            m_FeetItemView.gameObject.SetActive(true);
            m_BackItemView.gameObject.SetActive(true);
            m_RightHandItemView.gameObject.SetActive(true);
            m_HipItemView.gameObject.SetActive(true);
            m_NeckItemView.gameObject.SetActive(true);
            m_LeftHandItemView.gameObject.SetActive(true);
            m_FingerItemView.gameObject.SetActive(true);

            m_SecureModeToggle.gameObject.SetActive(hasExpertPvp);
            m_SecureModeLegacyToggle.gameObject.SetActive(!hasExpertPvp);
            m_ExpertModeToggle.gameObject.SetActive(hasExpertPvp);
            m_ExpertModeSmallToggle.gameObject.SetActive(false);

            m_QuestsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_OptionsLegacyButton.gameObject.SetActive(!hasExpertPvp);
            m_HelpLegacyButton.gameObject.SetActive(!hasExpertPvp);
            
            m_CapacityLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(79.0f, -122.0f);
            m_SoulPointsLabel.rectTransform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(5.0f, -122.0f);
            
            m_AttackOffensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, 0.0f);
            m_AttackBalancedToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, -23.0f);
            m_AttackDefensiveToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-28.0f, -46.0f);
            m_ChaseOffToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, 0.0f);
            m_ChaseOnToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5, -23.0f);
            m_SecureModeLegacyToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5f, -46.0f);
            m_ConditionsPanel.anchoredPosition = new Vector2(5.0f, -147.0f);
            m_StopLegacyButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-5.0f, -69.0f);

            int newHeight = hasExpertPvp ? ModernMaximizedWindowHeight : LegacyMaximizedWindowHeight;
            if (hasStoreInbox && OpenTibiaUnity.GameManager.ClientVersion < 1100)
                newHeight += 14;

            GetComponent<LayoutElement>().minHeight = newHeight;
        }

        public ObjectInstance GetObjectUnderMouse() {
            if (m_SlotUnderMouse == -1)
                return null;

            return BodyContainerView.GetObject((ClothSlots)m_SlotUnderMouse);
        }

        public int GetTopObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return m_SlotUnderMouse;
        }

        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            return m_SlotUnderMouse;
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            @object = GetObjectUnderMouse();
            if (!@object.Type.IsMultiUse) {
                @object = null;
                return -1;
            }

            return m_SlotUnderMouse;
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance @object) {
            return GetUseObjectUnderPoint(mousePosition, out @object);
        }

        public Vector3Int? MousePositionToAbsolutePosition(Vector3 mousePosition) {
            if (m_SlotUnderMouse == -1)
                return null;

            return new Vector3Int(65535, m_SlotUnderMouse, 0);
        }

        public Vector3Int? MousePositionToMapPosition(Vector3 mousePosition) {
            return null;
        }

        private void OnItemPointerEnter(ClothSlots slot) {
            m_SlotUnderMouse = (int)slot;
        }

        private void OnItemPointerExit(ClothSlots _) {
            m_SlotUnderMouse = -1;
        }

        private void OnExpertPvPToggleValueChanged(bool value) {
            if (m_HandlingExpertPvPToggle)
                return;

            try {
                m_HandlingExpertPvPToggle = true;
                m_ExpertModeToggle.isOn = value;
                m_ExpertModeSmallToggle.isOn = value;
                m_PvpDoveToggle.transform.parent.gameObject.SetActive(value);

                SetPvPMode(CombatPvPModes.Dove, true, true);
            } catch (System.Exception) {
            } finally {
                m_HandlingExpertPvPToggle = false;
            }
        }
    }
}
