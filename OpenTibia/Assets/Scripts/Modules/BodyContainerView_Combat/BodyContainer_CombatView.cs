using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.BodyContainerView_Combat
{
    public class BodyContainer_CombatView : Core.Components.Base.AbstractComponent,
        IBodyContainerViewWidget, ICombatViewWidget, IUseWidget, IMoveWidget,
        IWidgetContainerWidget
    {
        private const int ModernWindowSize = 158;
        private const int LegacyWindowSize = ModernWindowSize + 26;
        

#pragma warning disable CS0649 // never assigned to
        [SerializeField] private Core.Components.ItemView m_HeadItemView;
        [SerializeField] private Core.Components.ItemView m_TorsoItemView;
        [SerializeField] private Core.Components.ItemView m_LegsItemView;
        [SerializeField] private Core.Components.ItemView m_FeetItemView;
        [SerializeField] private Core.Components.ItemView m_NeckItemView;
        [SerializeField] private Core.Components.ItemView m_LeftHandItemView;
        [SerializeField] private Core.Components.ItemView m_FingerItemView;
        [SerializeField] private Core.Components.ItemView m_BackItemView;
        [SerializeField] private Core.Components.ItemView m_RightHandItemView;
        [SerializeField] private Core.Components.ItemView m_HipItemView;

        [SerializeField] private Texture2D m_HeadDefaultTexture;
        [SerializeField] private Texture2D m_TorsoDefaultTexture;
        [SerializeField] private Texture2D m_LegsDefaultTexture;
        [SerializeField] private Texture2D m_FeetDefaultTexture;
        [SerializeField] private Texture2D m_NeckDefaultTexture;
        [SerializeField] private Texture2D m_LeftHandDefaultTexture;
        [SerializeField] private Texture2D m_FingerDefaultTexture;
        [SerializeField] private Texture2D m_BackpackDefaultTexture;
        [SerializeField] private Texture2D m_RightHandDefaultTexture;
        [SerializeField] private Texture2D m_HipDefaultTexture;
        
        [SerializeField] private ToggleGroup m_ChaseModeToggleGroup;
        [SerializeField] private ToggleGroup m_AttackModeToggleGroup;
        [SerializeField] private ToggleGroup m_PvpModeToggleGroup;

        [SerializeField] private TMPro.TextMeshProUGUI m_CapacityLabel;
        [SerializeField] private TMPro.TextMeshProUGUI m_SoulPointsLabel;

        [SerializeField] private Toggle m_ChaseOffToggle;
        [SerializeField] private Toggle m_ChaseOnToggle;

        [SerializeField] private Toggle m_ExpertModeToggle;

        [SerializeField] private Toggle m_PvpDoveToggle;
        [SerializeField] private Toggle m_PvpWhiteHandToggle;
        [SerializeField] private Toggle m_PvpYellowHandToggle;
        [SerializeField] private Toggle m_PvpRedFistToggle;

        [SerializeField] private Toggle m_AttackOffensiveToggle;
        [SerializeField] private Toggle m_AttackBalancedToggle;
        [SerializeField] private Toggle m_AttackDefensiveToggle;

        [SerializeField] private Toggle m_SecureModeToggle;
        [SerializeField] private Toggle m_SecureModeLegacyToggle;

        [SerializeField] private Button m_StopButton;
        [SerializeField] private Button m_ToggleStyleButton;

        [SerializeField] private Button m_StopLegacyButton;
        [SerializeField] private Button m_QuestsLegacyButton;
        [SerializeField] private Button m_OptionsLegacyButton;
        [SerializeField] private Button m_HelpLegacyButton;
        [SerializeField] private Button m_LogoutLegacyButton;

        [SerializeField] private Toggle m_SkillsLegacyToggle;
        [SerializeField] private Toggle m_BattleLegacyToggle;
        [SerializeField] private Toggle m_VIPLegacyToggle;

        [SerializeField] private Sprite m_MinimizeSpriteNormal;
        [SerializeField] private Sprite m_MinimizeSpritePressed;
        [SerializeField] private Sprite m_MaximizeSpriteNormal;
        [SerializeField] private Sprite m_MaximizeSpritePressed;
#pragma warning restore CS0649 // never assigned to

        private int m_MouseOverSlot = -1;

        private RenderTexture m_SlotsRenderTexture;

        private ObjectDragImpl<BodyContainer_CombatView> m_DragHandler;

        private Core.Container.BodyContainerView m_BodyContainerView { get { return OpenTibiaUnity.ContainerStorage.BodyContainerView; } }
        private Core.Options.OptionStorage m_OptionStorage { get { return OpenTibiaUnity.OptionStorage; } }

        protected override void Awake() {
            base.Awake();
            m_BodyContainerView.onSlotChange.AddListener(OnInventorySlotChange);
            Creature.onSkillChange.AddListener(OnSkillChange);

            m_SlotsRenderTexture = new RenderTexture(Constants.FieldSize * (int)ClothSlots.Hip, Constants.FieldSize, 16, RenderTextureFormat.ARGB32);
            m_DragHandler = new ObjectDragImpl<BodyContainer_CombatView>(this);
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

            OpenTibiaUnity.GameManager.OnTacticsChangeEvent.AddListener((attackMode, chaseMode, secureMode, pvpMode) => {
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

            m_ExpertModeToggle.onValueChanged.AddListener((value) => {
                SetPvPMode(CombatPvPModes.Dove, true, true);
            });

            m_StopButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));
            m_StopLegacyButton.onClick.AddListener(() => OpenTibiaUnity.Player?.StopAutowalk(false));

            m_ToggleStyleButton.onClick.AddListener(ToggleStyle);
        }

        private void OnGUI() {
            if (Event.current.type != EventType.Repaint)
                return;

            Vector2 zoom = new Vector2(Screen.width / (float)m_SlotsRenderTexture.width, Screen.height / (float)m_SlotsRenderTexture.height);

            m_SlotsRenderTexture.Release();
            RenderTexture.active = m_SlotsRenderTexture;
            for (int i = 0; i < (int)ClothSlots.Hip; i++) {
                var obj = m_BodyContainerView.Objects[i];
                if (!obj)
                    continue;
                
                obj.Animate(OpenTibiaUnity.TicksMillis);
                obj.DrawTo(new Vector2(Constants.FieldSize * i, 0), zoom, 0, 0, 0);
            }

            RenderTexture.active = null;
        }

        protected void OnSkillChange(Creature creature, SkillTypes skillType, SkillStruct skill) {
            var player = creature as Player;
            if (!player)
                return;

            if (skillType == SkillTypes.Capacity) {
                if (skill.level <= -1)
                    m_CapacityLabel.text = "Cap:\n<color=#00EB00>infinity</color>";
                else
                    m_CapacityLabel.text = string.Format("Cap:\n{0}", skill.level / 1000);
            } else if (skillType == SkillTypes.SoulPoints) {
                m_SoulPointsLabel.text = string.Format("Soul:\n{0}", skill.level);
            }
        }

        public void OnInventorySlotChange(ClothSlots slot, ObjectInstance obj) {
            switch (slot) {
                case ClothSlots.Head: InitialiseObjectImage(slot, m_HeadItemView, m_HeadDefaultTexture, obj); break;
                case ClothSlots.Neck: InitialiseObjectImage(slot, m_NeckItemView, m_NeckDefaultTexture, obj); break;
                case ClothSlots.Backpack: InitialiseObjectImage(slot, m_BackItemView, m_BackpackDefaultTexture, obj); break;
                case ClothSlots.Torso: InitialiseObjectImage(slot, m_TorsoItemView, m_TorsoDefaultTexture, obj); break;
                case ClothSlots.RightHand: InitialiseObjectImage(slot, m_RightHandItemView, m_RightHandDefaultTexture, obj); break;
                case ClothSlots.LeftHand: InitialiseObjectImage(slot, m_LeftHandItemView, m_LeftHandDefaultTexture, obj); break;
                case ClothSlots.Legs: InitialiseObjectImage(slot, m_LegsItemView, m_LegsDefaultTexture, obj); break;
                case ClothSlots.Feet: InitialiseObjectImage(slot, m_FeetItemView, m_FeetDefaultTexture, obj); break;
                case ClothSlots.Finger: InitialiseObjectImage(slot, m_FingerItemView, m_FingerDefaultTexture, obj); break;
                case ClothSlots.Hip: InitialiseObjectImage(slot, m_HipItemView, m_HipDefaultTexture, obj); break;
            }
        }

        private void InitialiseObjectImage(ClothSlots slot, Core.Components.ItemView itemView, Texture2D defaultTexture, ObjectInstance obj) {
            if (slot > ClothSlots.Hip)
                return;

            itemView.objectInstance = obj;
            if (!!obj) {
                itemView.itemImage.texture = m_SlotsRenderTexture;
                float w = (float)Constants.FieldSize / m_SlotsRenderTexture.width;
                itemView.itemImage.uvRect = new Rect(w * ((int)slot - 1), 0, w, 1);
            } else {
                itemView.itemImage.texture = defaultTexture;
                itemView.itemImage.uvRect = new Rect(0, 0, 1, 1);
            }
        }

        public void SetChaseMode(CombatChaseModes chaseMode, bool send, bool toggle) {
            if (chaseMode != m_OptionStorage.CombatChaseMode)
                return;

            m_OptionStorage.CombatChaseMode = chaseMode;
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
            if (attackMode == m_OptionStorage.CombatAttackMode)
                return;

            m_OptionStorage.CombatAttackMode = attackMode;
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
            if (secureMode == m_OptionStorage.CombatSecureMode)
                return;

            m_OptionStorage.CombatSecureMode = secureMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle)
                m_SecureModeToggle.isOn = !secureMode;
        }
        
        public void SetPvPMode(CombatPvPModes pvpMode, bool send, bool toggle) {
            if (pvpMode == m_OptionStorage.CombatPvPMode)
                return;

            // force toggle of e-pvp button
            if (pvpMode != CombatPvPModes.Dove)
                m_ExpertModeToggle.isOn = true;

            m_OptionStorage.CombatPvPMode = pvpMode;
            if (send) {
                var protocolGame = OpenTibiaUnity.ProtocolGame;
                if (!!protocolGame && protocolGame.IsGameRunning)
                    protocolGame.SendSetTactics();
            }

            if (toggle) {
                switch (pvpMode) {
                    case CombatPvPModes.Dove:
                        break;
                    case CombatPvPModes.WhiteHand:
                        break;
                    case CombatPvPModes.YellowHand:
                        break;
                    case CombatPvPModes.RedFist:
                        break;

                    default:
                        return;
                }
            }
        }

        public void ToggleStyle() {

        }

        private void MinimizeStyle() {
            // TODO
        }

        private void MaximizeStyle() {
            // TODO
        }

        public ObjectInstance GetObjectUnderMouse() {
            if (m_MouseOverSlot == -1)
                return null;

            return m_BodyContainerView.GetObject((ClothSlots)m_MouseOverSlot);
        }
        
        public int GetUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            return GetMultiUseObjectUnderPoint(mousePosition, out obj);
        }

        public int GetMultiUseObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            obj = GetObjectUnderMouse();
            return m_MouseOverSlot;
        }

        public int GetMoveObjectUnderPoint(Vector3 mousePosition, out ObjectInstance obj) {
            return GetUseObjectUnderPoint(mousePosition, out obj);
        }

        public Vector3Int? PointToAbsolute(Vector3 mousePosition) {
            if (m_MouseOverSlot == -1)
                return null;

            return new Vector3Int(65535, m_MouseOverSlot, 0);
        }

        public Vector3Int? PointToMap(Vector3 mousePosition) {
            return null;
        }

        public void OnItemPointerEnter(ClothSlots slot) => m_MouseOverSlot = (int)slot;
        public void OnItemPointerExit(ClothSlots _) => m_MouseOverSlot = -1;
    }
}
