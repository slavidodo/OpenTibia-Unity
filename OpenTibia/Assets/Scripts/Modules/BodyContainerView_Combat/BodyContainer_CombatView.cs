using OpenTibiaUnity.Core.Appearances;
using OpenTibiaUnity.Core.Creatures;
using OpenTibiaUnity.Core.Game;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.BodyContainerView_Combat
{
    public class BodyContainer_CombatView : Core.Components.Base.AbstractComponent, IBodyContainerViewWidget, ICombatViewWidget, IUseWidget, IMoveWidget
    {
#pragma warning disable CS0649 // never assigned to
        [SerializeField] private RawImage m_HeadImageComponent;
        [SerializeField] private RawImage m_TorsoImageComponent;
        [SerializeField] private RawImage m_LegsImageComponent;
        [SerializeField] private RawImage m_FeetImageComponent;
        [SerializeField] private RawImage m_NeckImageComponent;
        [SerializeField] private RawImage m_LeftHandImageComponent;
        [SerializeField] private RawImage m_FingerImageComponent;
        [SerializeField] private RawImage m_BackpackImageComponent;
        [SerializeField] private RawImage m_RightHandImageComponent;
        [SerializeField] private RawImage m_HipImageComponent;

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
        [SerializeField] private ToggleGroup m_PvPModeToggleGroup;

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
#pragma warning restore CS0649 // never assigned to
        
        private RenderTexture m_SlotsRenderTexture;

        private Core.Container.BodyContainerView m_BodyContainerView { get { return OpenTibiaUnity.ContainerStorage.BodyContainerView; } }
        private Core.Options.OptionStorage m_OptionStorage { get { return OpenTibiaUnity.OptionStorage; } }

        protected override void Awake() {
            base.Awake();
            m_BodyContainerView.onSlotChange.AddListener(OnInventorySlotChange);
            Creature.onSkillChange.AddListener(OnSkillChange);

            m_SlotsRenderTexture = new RenderTexture(32 * (int)ClothSlots.Hip, 32, 16, RenderTextureFormat.ARGB32);
        }

        protected override void Start() {
            base.Start();

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

            m_ExpertModeToggle.onValueChanged.AddListener((value) => {
                SetPvPMode(CombatPvPModes.Dove, true, true);
            });
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
                obj.DrawTo(new Vector2(32 * i, 0), zoom, 0, 0, 0);
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
                    m_CapacityLabel.text = string.Format("Cap:\n{0}", skill.level);
            } else if (skillType == SkillTypes.SoulPoints) {
                m_SoulPointsLabel.text = string.Format("Soul:\n{0}", skill.level);
            }
        }

        public void OnInventorySlotChange(ClothSlots slot, ObjectInstance obj) {
            switch (slot) {
                case ClothSlots.Head: InitialiseObjectImage(slot, m_HeadImageComponent, m_HeadDefaultTexture, obj); break;
                case ClothSlots.Neck: InitialiseObjectImage(slot, m_NeckImageComponent, m_NeckDefaultTexture, obj); break;
                case ClothSlots.Backpack: InitialiseObjectImage(slot, m_BackpackImageComponent, m_BackpackDefaultTexture, obj); break;
                case ClothSlots.Torso: InitialiseObjectImage(slot, m_TorsoImageComponent, m_TorsoDefaultTexture, obj); break;
                case ClothSlots.RightHand: InitialiseObjectImage(slot, m_RightHandImageComponent, m_RightHandDefaultTexture, obj); break;
                case ClothSlots.LeftHand: InitialiseObjectImage(slot, m_LeftHandImageComponent, m_LeftHandDefaultTexture, obj); break;
                case ClothSlots.Legs: InitialiseObjectImage(slot, m_LegsImageComponent, m_LegsDefaultTexture, obj); break;
                case ClothSlots.Feet: InitialiseObjectImage(slot, m_FeetImageComponent, m_FeetDefaultTexture, obj); break;
                case ClothSlots.Finger: InitialiseObjectImage(slot, m_FingerImageComponent, m_FingerDefaultTexture, obj); break;
                case ClothSlots.Hip: InitialiseObjectImage(slot, m_HipImageComponent, m_HipDefaultTexture, obj); break;
            }
        }

        private void InitialiseObjectImage(ClothSlots slot, RawImage imageComponent, Texture2D defaultTexture, ObjectInstance obj) {
            if (slot > ClothSlots.Hip)
                return;

            if (!!obj) {
                imageComponent.texture = m_SlotsRenderTexture;
                float w = 32f / m_SlotsRenderTexture.width;
                imageComponent.uvRect = new Rect(w * ((int)slot - 1), 0, w, 1);
            } else {
                imageComponent.texture = defaultTexture;
                imageComponent.uvRect = new Rect(0, 0, 1, 1);
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

        public void MinimizeStyle() {
            // TODO
        }

        public void MaximizeStyle() {
            // TODO
        }

        public ObjectInstance GetUseObjectUnderPoint(Vector2 point) {
            return GetUseObjectUnderPoint(point);
        }

        public ObjectInstance GetMultiUseObjectUnderPoint(Vector2 point) {
            return null;
        }

        public ObjectInstance GetMoveObjectUnderPoint(Vector2 point) {
            return GetUseObjectUnderPoint(point);
        }
    }
}
