using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Creatures
{
    public class Player : Creature
    {
        public class UIntPlayerChangeEvent : UnityEvent<Player, uint, uint> { }

        public const int Blessing_None = 0;
        public const int Profession_None = 0;

        public static UIntCreatureChangeEvent onStateFlagsChange = new UIntCreatureChangeEvent();

        protected bool m_AutowalkPathAborting = false;
        protected bool m_AutowalkTargetDiagonal = false;
        protected bool m_AutowalkTargetExact = false;
        protected bool m_Premium = false;
        protected bool m_HasReachedMain = false;

        protected int m_PremiumUntil = 0;
        protected int m_Blessings = 0;
        protected int m_Profession = 0;
        protected int m_BankGoldBalance = 0;
        protected int m_InventoryGoldBalance = 0;

        protected UnityEngine.Vector3Int m_AutowalkPathDelta = UnityEngine.Vector3Int.zero;
        protected UnityEngine.Vector3Int m_AutowalkTarget = new UnityEngine.Vector3Int(-1, -1, -1);
        protected List<int> m_AutowalkPathSteps = new List<int>();
        protected List<int> m_KnownSpells = new List<int>();

        public override int HealthPercent {
            get {
                return (int)((GetSkillValue(SkillTypes.Health) / (float)GetSkillbase(SkillTypes.Health)) * 100);
            }
        }

        public override int ManaPercent {
            get {
                return (int)((GetSkillValue(SkillTypes.Mana) / (float)GetSkillbase(SkillTypes.Mana)) * 100);
            }
        }

        public int Mana {
            get {
                return GetSkillValue(SkillTypes.Mana);
            }
        }

        public int Level {
            get {
                return GetSkillValue(SkillTypes.Level);
            }
        }

        public int LevelPercent {
            get {
                return GetSKillProgress(SkillTypes.Level);
            }
        }

        protected ExperienceGainInfo m_ExperienceGainInfo = new ExperienceGainInfo();
        public ExperienceGainInfo ExperienceGainInfo {
            get { return m_ExperienceGainInfo; }
        }

        protected uint m_StateFlags = 0;
        public uint StateFlags {
            get { return m_StateFlags; }
            set { if (m_StateFlags != value) { var old = m_StateFlags; m_StateFlags = value; onStateFlagsChange.Invoke(this, m_StateFlags, old); } }
        }

        public UnityEngine.Vector3Int AnticipatedPosition {
            get {
                return m_Position + m_AutowalkPathDelta;
            }
        }
        
        public Player(uint id, string name = null) : base(id, CreatureTypes.Player, name) {
            
        }

        public void StartAutowalk(UnityEngine.Vector3Int targetPosition, bool diagonal, bool exact) {
            StartAutowalk(targetPosition.x, targetPosition.y, targetPosition.z, diagonal, exact);
        }

        public void StartAutowalk(int targetX, int targetY, int targetZ, bool diagonal, bool exact) {
            if (targetX == m_AutowalkTarget.x && targetY == m_AutowalkTarget.y && targetZ == m_AutowalkTarget.z)
                return;

            if (targetX == m_Position.x + 2 * m_AutowalkPathDelta.x
                && targetY == m_Position.y + 2 * m_AutowalkPathDelta.y
                && targetZ == m_Position.z + 2 * m_AutowalkPathDelta.z)
                return;

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (protocolGame == null || !protocolGame.IsGameRunning || worldMapStorage == null)
                return;

            m_AutowalkTarget.Set(-1, -1, -1);
            m_AutowalkTargetDiagonal = false;
            m_AutowalkTargetExact = false;

            // check if the tile can actually be entered (before proceeding with anything)
            // the tile must be visible, otherwise it would then be checked in the pathfinder
            if (worldMapStorage.IsVisible(targetX, targetY, targetZ, true)) {
                UnityEngine.Vector3Int s_v1 = new UnityEngine.Vector3Int(targetX, targetY, targetZ);
                worldMapStorage.ToMap(s_v1, out s_v1);
                if ((s_v1.x != Constants.PlayerOffsetX || s_v1.y != Constants.PlayerOffsetY) && worldMapStorage.GetEnterPossibleFlag(s_v1.x, s_v1.y, s_v1.z, true) == Constants.FieldEnterNotPossible) {
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_SORRY_NOT_POSSIBLE);
                    return;
                }
            }

            m_AutowalkTarget.Set(targetX, targetY, targetZ);
            m_AutowalkTargetDiagonal = diagonal;
            m_AutowalkTargetExact = exact;
            if (m_AutowalkPathAborting || m_AutowalkPathSteps.Count == 1)
                return;

            if (m_AutowalkPathSteps.Count == 0) {
                StartAutowalkInternal();
            } else {
                protocolGame.SendStop();
                m_AutowalkPathAborting = true;
            }
        }

        private void StartAutowalkInternal() {
            if (m_MovementRunning || m_AutowalkPathAborting || m_AutowalkPathDelta != UnityEngine.Vector3Int.zero || m_AutowalkTarget.x == -1 && m_AutowalkTarget.y == -1 && m_AutowalkTarget.z == -1)
                return;
            
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var minimapStorage = OpenTibiaUnity.MiniMapStorage;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (protocolGame == null || !protocolGame.IsGameRunning || minimapStorage == null || worldMapStorage == null)
                return;

            m_AutowalkPathAborting = false;
            m_AutowalkPathDelta.Set(0, 0, 0);
            m_AutowalkPathSteps.Clear();
            
            PathState pathState = minimapStorage.CalculatePath(m_Position, m_AutowalkTarget, m_AutowalkTargetDiagonal, m_AutowalkTargetExact, m_AutowalkPathSteps);
            if (worldMapStorage.IsVisible(m_AutowalkTarget, false)) {
                m_AutowalkTarget.Set(-1, -1, -1);
                m_AutowalkTargetDiagonal = false;
                m_AutowalkTargetExact = false;
            }
            
            switch (pathState) {
                case PathState.PathEmpty:
                    break;
                case PathState.PathExists:
                    protocolGame.SendGo(m_AutowalkPathSteps);
                    NextAutowalkStep();
                    break;
                case PathState.PathErrorGoDownstairs:
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_PATH_GO_DOWNSTAIRS);
                    break;
                case PathState.PathErrorGoUpstairs:
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_PATH_GO_UPSTAIRS);
                    break;
                case PathState.PathErrorTooFar:
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_PATH_TOO_FAR);
                    break;
                case PathState.PathErrorUnreachable:
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_PATH_UNREACHABLE);
                    StopAutowalk(false);
                    break;
                case PathState.PathErrorInternal:
                    worldMapStorage.AddOnscreenMessage(MessageModes.Failure, TextResources.MSG_SORRY_NOT_POSSIBLE);
                    StopAutowalk(false);
                    break;
                default:
                    throw new System.Exception("Player.StartAutowalkInternal: Unknown path state: " + pathState);
            }
        }

        private void NextAutowalkStep() {
            if (m_MovementRunning || m_AutowalkPathAborting || m_AutowalkPathDelta != UnityEngine.Vector3Int.zero || m_AutowalkPathSteps.Count == 0)
                return;
            
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (protocolGame == null || !protocolGame.IsGameRunning || worldMapStorage == null)
                return;

            switch ((PathDirection)(m_AutowalkPathSteps[0] & 65535)) {
                case PathDirection.East:
                    m_AutowalkPathDelta.x = 1;
                    break;
                case PathDirection.NorthEast:
                    m_AutowalkPathDelta.x = 1;
                    m_AutowalkPathDelta.y = -1;
                    break;
                case PathDirection.North:
                    m_AutowalkPathDelta.y = -1;
                    break;
                case PathDirection.NorthWest:
                    m_AutowalkPathDelta.x = -1;
                    m_AutowalkPathDelta.y = -1;
                    break;
                case PathDirection.West:
                    m_AutowalkPathDelta.x = -1;
                    break;
                case PathDirection.SouthWest:
                    m_AutowalkPathDelta.x = -1;
                    m_AutowalkPathDelta.y = 1;
                    break;
                case PathDirection.South:
                    m_AutowalkPathDelta.y = 1;
                    break;
                case PathDirection.SouthEast:
                    m_AutowalkPathDelta.x = 1;
                    m_AutowalkPathDelta.y = 1;
                    break;
                default:
                    throw new System.Exception("Player.NextAutowalkStep: Invalid step(1): " + (m_AutowalkPathSteps[0] & 65535));
            }

            var s_v1 = worldMapStorage.ToMap(m_Position);
            var s_v2 = worldMapStorage.ToMap(m_Position);

            s_v2.x += m_AutowalkPathDelta.x;
            s_v2.y += m_AutowalkPathDelta.y;

            // Tile should be walkable (full ground)
            Appearances.ObjectInstance obj = null;
            if (!(obj = worldMapStorage.GetObject(s_v2.x, s_v2.y, s_v2.z, 0)) || !obj.Type || !obj.Type.IsBank) {
                m_AutowalkPathDelta.Set(0, 0, 0);
                return;
            }

            uint possibleFlag = worldMapStorage.GetEnterPossibleFlag(s_v2.x, s_v2.y, s_v2.z, false);
            if (possibleFlag == Constants.FieldEnterNotPossible || worldMapStorage.GetFieldHeight(s_v1.x, s_v1.y, s_v1.z) + 1 < worldMapStorage.GetFieldHeight(s_v2.x, s_v2.y, s_v2.z)) {
                m_AutowalkPathDelta.Set(0, 0, 0);
                return;
            }

            if (possibleFlag == Constants.FieldEnterPossible) {
                base.StartMovementAnimation(m_AutowalkPathDelta.x, m_AutowalkPathDelta.y, (int)obj.Type.Waypoints);
                m_AnimationDelta.x = m_AnimationDelta.x + m_AutowalkPathDelta.x * Constants.FieldSize;
                m_AnimationDelta.y = m_AnimationDelta.y + m_AutowalkPathDelta.y * Constants.FieldSize;
            } else if (possibleFlag == Constants.FieldEnterPossibleNoAnimation) {
                m_AnimationDelta.Set(0, 0, 0);
            }

            for (int i = 1; i < m_AutowalkPathSteps.Count; i++) {
                var rawDirection = m_AutowalkPathSteps[i] & 65535;
                var mapCost = ((uint)m_AutowalkPathSteps[i] & 4294901760U) >> 16;
                switch ((PathDirection)rawDirection) {
                    case PathDirection.East:
                        s_v2.x = s_v2.x + 1;
                        break;
                    case PathDirection.NorthEast:
                        s_v2.x = s_v2.x + 1;
                        s_v2.y = s_v2.y - 1;
                        break;
                    case PathDirection.North:
                        s_v2.y = s_v2.y - 1;
                        break;
                    case PathDirection.NorthWest:
                        s_v2.x = s_v2.x - 1;
                        s_v2.y = s_v2.y - 1;
                        break;
                    case PathDirection.West:
                        s_v2.x = s_v2.x - 1;
                        break;
                    case PathDirection.SouthWest:
                        s_v2.x = s_v2.x - 1;
                        s_v2.y = s_v2.y + 1;
                        break;
                    case PathDirection.South:
                        s_v2.y = s_v2.y + 1;
                        break;
                    case PathDirection.SouthEast:
                        s_v2.x = s_v2.x + 1;
                        s_v2.y = s_v2.y + 1;
                        break;
                    default:
                        throw new System.Exception("Player.NextAutowalkStep: Invalid step(2): " + (m_AutowalkPathSteps[i] & 65535));
                }

                if (s_v2.x < 0 || s_v2.x >= Constants.MapSizeX || s_v2.y < 0 || s_v2.y >= Constants.MapSizeY)
                    break;

                if (worldMapStorage.GetMiniMapCost(s_v2.x, s_v2.y, s_v2.z) > mapCost) {
                    protocolGame.SendStop();
                    m_AutowalkPathAborting = true;
                    break;
                }
            }
        }

        public void StopAutowalk(bool stopAnimation) {
            if (stopAnimation)
                StopMovementAnimation();
            m_AutowalkPathAborting = false;
            m_AutowalkPathDelta.Set(0, 0, 0);
            m_AutowalkPathSteps.Clear();
            m_AutowalkTarget.Set(-1, -1, -1);
            m_AutowalkTargetDiagonal = false;
            m_AutowalkTargetExact = false;
        }

        public void AbortAutowalk(Directions direction) {
            m_Direction = direction;
            m_AutowalkPathAborting = false;
            m_AutowalkPathSteps.Clear();
            if (!m_MovementRunning || m_AutowalkPathDelta != UnityEngine.Vector3Int.zero) {
                m_AutowalkPathDelta.Set(0, 0, 0);
                StopMovementAnimation();
                StartAutowalkInternal();
            }
        }

        public void ResetAutowalk() {
            StopAutowalk(false);
            AbortAutowalk(Directions.South);
        }

        public override void StartMovementAnimation(int deltaX, int deltaY, int movementCost) {
            if (deltaX != m_AutowalkPathDelta.x || deltaY != m_AutowalkPathDelta.y) {
                base.StartMovementAnimation(deltaX, deltaY, movementCost);
            }

            m_AutowalkPathDelta.Set(0, 0, 0);
            if (m_AutowalkPathSteps.Count > 0) {
                m_AutowalkPathSteps.RemoveAt(0);
            }
        }

        public override void AnimateMovement(int ticks) {
            base.AnimateMovement(ticks);
            m_AnimationDelta.x += m_AutowalkPathDelta.x * Constants.FieldSize;
            m_AnimationDelta.y += m_AutowalkPathDelta.y * Constants.FieldSize;
            if (!m_MovementRunning && m_AutowalkPathDelta == UnityEngine.Vector3Int.zero) {
                if (m_AutowalkPathSteps.Count > 0) {
                    NextAutowalkStep();
                } else {
                    StartAutowalkInternal();
                }
            }
        }

        public void ResetFlags() {

        }

        public override void ResetSkills() {
            m_Skills = new SkillStruct[(int)SkillTypes.ManaLeechAmount + 1];
            //m_ExperienceCounter.Reset();
            //m_ExperienceGainInfo.Reset();
        }

        public override void Reset() {
            uint myID = m_ID;
            base.Reset();
            ResetAutowalk();
            ResetFlags();
            ResetSkills();
            m_ID = myID;
            m_KnownSpells.Clear();
            m_Premium = false;
            m_PremiumUntil = 0;
            m_HasReachedMain = false;
            m_Blessings = Blessing_None;
            m_Profession = Profession_None;
            m_Type = CreatureTypes.Player;
            m_BankGoldBalance = 0;
            m_InventoryGoldBalance = 0;
        }

        public int GetRuneUses(Magic.Rune rune) {
            if (rune == null || rune.RestrictLevel > GetSkillValue(SkillTypes.Level)
                             || rune.RestrictMagicLevel > GetSkillValue(SkillTypes.MagLevel)
                             || (rune.RestrictProfession & (1 << m_Profession)) == 0) {
                return -1;
            }

            if (rune.CastMana <= Mana) {
                return rune.CastMana > 0 ? Mana / rune.CastMana : int.MaxValue;
            }
            return 0;
        }
    }
}
