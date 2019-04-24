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

        protected double m_ExperienceBonus = 0.0f;

        protected UnityEngine.Vector3Int m_AutowalkPathDelta = UnityEngine.Vector3Int.zero;
        protected UnityEngine.Vector3Int m_AutowalkTarget = new UnityEngine.Vector3Int(-1, -1, -1);
        protected List<int> m_AutowalkPathSteps = new List<int>();
        protected List<int> m_KnownSpells = new List<int>();

        public override int HealthPercent {
            get => (int)((GetSkillValue(SkillTypes.Health) / (float)GetSkillbase(SkillTypes.Health)) * 100);
        }

        public override int ManaPercent {
            get => (int)((GetSkillValue(SkillTypes.Mana) / (float)GetSkillbase(SkillTypes.Mana)) * 100);
        }

        public int Mana {
            get => GetSkillValue(SkillTypes.Mana);
        }

        public int Level {
            get => GetSkillValue(SkillTypes.Level);
        }

        public int LevelPercent {
            get => GetSKillProgress(SkillTypes.Level);
        }

        protected ExperienceGainInfo m_ExperienceGainInfo = new ExperienceGainInfo();
        protected SkillCounter m_ExperienceCounter = new SkillCounter();

        public double ExperienceBonus { get => m_ExperienceBonus; set => m_ExperienceBonus = value; } // <= 1096
        public ExperienceGainInfo ExperienceGainInfo { get => m_ExperienceGainInfo; } // >= 1097

        protected uint m_StateFlags = 0;
        public uint StateFlags {
            get => m_StateFlags;
            set => UpdateStateFlags(value);
        }

        public UnityEngine.Vector3Int AnticipatedPosition {
            get => m_Position + m_AutowalkPathDelta;
        }

        public bool IsFighting {
            get => (m_StateFlags & 1 << (int)States.Fighting) > 0;
        }

        public Player(uint id, string name = null) : base(id, CreatureTypes.Player, name) {}

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
                if ((s_v1.x != Constants.PlayerOffsetX || s_v1.y != Constants.PlayerOffsetY) && worldMapStorage.GetEnterPossibleFlag(s_v1.x, s_v1.y, s_v1.z, true) == EnterPossibleFlag.NotPossible) {
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

            var mapPosition = worldMapStorage.ToMap(m_Position);
            var tmpMapPosition = mapPosition;

            tmpMapPosition.x += m_AutowalkPathDelta.x;
            tmpMapPosition.y += m_AutowalkPathDelta.y;

            // Tile should be walkable (full ground)
            Appearances.ObjectInstance obj = null;
            if (!(obj = worldMapStorage.GetObject(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z, 0)) || !obj.Type || !obj.Type.IsGround) {
                m_AutowalkPathDelta.Set(0, 0, 0);
                return;
            }

            EnterPossibleFlag enterFlag = worldMapStorage.GetEnterPossibleFlag(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z, false);
            if (enterFlag == EnterPossibleFlag.NotPossible || worldMapStorage.GetFieldHeight(mapPosition.x, mapPosition.y, mapPosition.z) + 1 < worldMapStorage.GetFieldHeight(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z)) {
                m_AutowalkPathDelta.Set(0, 0, 0);
                return;
            }

            if (enterFlag == EnterPossibleFlag.Possible) {
                base.StartMovementAnimation(m_AutowalkPathDelta.x, m_AutowalkPathDelta.y, (int)obj.Type.GroundSpeed);
                m_AnimationDelta.x = m_AnimationDelta.x + m_AutowalkPathDelta.x * Constants.FieldSize;
                m_AnimationDelta.y = m_AnimationDelta.y + m_AutowalkPathDelta.y * Constants.FieldSize;
            } else if (enterFlag == EnterPossibleFlag.PossibleNoAnimation) {
                m_AnimationDelta.Set(0, 0, 0);
            }

            for (int i = 1; i < m_AutowalkPathSteps.Count; i++) {
                var pathDirection = (PathDirection)(m_AutowalkPathSteps[i] & 65535);
                var mapCost = ((uint)m_AutowalkPathSteps[i] & 4294901760U) >> 16;
                switch (pathDirection) {
                    case PathDirection.East:
                        tmpMapPosition.x += 1;
                        break;
                    case PathDirection.NorthEast:
                        tmpMapPosition.x += 1;
                        tmpMapPosition.y -= 1;
                        break;
                    case PathDirection.North:
                        tmpMapPosition.y -= 1;
                        break;
                    case PathDirection.NorthWest:
                        tmpMapPosition.x -= 1;
                        tmpMapPosition.y -= 1;
                        break;
                    case PathDirection.West:
                        tmpMapPosition.x -= 1;
                        break;
                    case PathDirection.SouthWest:
                        tmpMapPosition.x -= 1;
                        tmpMapPosition.y += 1;
                        break;
                    case PathDirection.South:
                        tmpMapPosition.y += 1;
                        break;
                    case PathDirection.SouthEast:
                        tmpMapPosition.x += 1;
                        tmpMapPosition.y += 1;
                        break;
                    default:
                        throw new System.Exception("Player.NextAutowalkStep: Invalid step(2): " + (m_AutowalkPathSteps[i] & 65535));
                }

                if (tmpMapPosition.x < 0 || tmpMapPosition.x >= Constants.MapSizeX || tmpMapPosition.y < 0 || tmpMapPosition.y >= Constants.MapSizeY)
                    break;

                if (worldMapStorage.GetMiniMapCost(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z) > mapCost) {
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
            if (deltaX != m_AutowalkPathDelta.x || deltaY != m_AutowalkPathDelta.y)
                base.StartMovementAnimation(deltaX, deltaY, movementCost);

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
                if (m_AutowalkPathSteps.Count > 0)
                    NextAutowalkStep();
                else
                    StartAutowalkInternal();
            }
        }

        public void ResetFlags() {

        }

        public override void ResetSkills() {
            m_Skills = new SkillStruct[(int)SkillTypes.ManaLeechAmount + 1];
            //m_ExperienceCounter.Reset();
            //m_ExperienceGainInfo.Reset();
        }

        public override void SetSkill(SkillTypes skillType, int level, int baseLevel = 0, int percentage = 0) {
            switch (skillType) {
                case SkillTypes.Experience:
                    var currentLevel = GetSkillValue(skillType);
                    if (level >= currentLevel) {
                        if (currentLevel > 0)
                            m_ExperienceCounter.AddSkillGain(level - currentLevel);
                    } else {
                        m_ExperienceCounter.Reset();
                    }

                    base.SetSkill(skillType, level, baseLevel, percentage);
                    break;
                case SkillTypes.Food:
                    base.SetSkill(skillType, level, baseLevel, percentage);
                    UpdateStateFlags();
                    break;
                default:
                    base.SetSkill(skillType, level, baseLevel, percentage);
                    break;
            }
        }

        public override void SetSkillValue(SkillTypes skillType, int level) {
            switch (skillType) {
                case SkillTypes.Experience:
                    var currentLevel = GetSkillValue(skillType);
                    if (level >= currentLevel) {
                        if (currentLevel > 0)
                            m_ExperienceCounter.AddSkillGain(level - currentLevel);
                    } else {
                        m_ExperienceCounter.Reset();
                    }

                    base.SetSkillValue(skillType, level);
                    break;
                case SkillTypes.Food:
                    base.SetSkillValue(skillType, level);
                    UpdateStateFlags();
                    break;
                default:
                    base.SetSkillValue(skillType, level);
                    break;
            }
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

        public void UpdateStateFlags(uint? value = null) {
            var flags = value ?? m_StateFlags;
            if (GetSkillValue(SkillTypes.Food) <= GetSkillbase(SkillTypes.Food))
                flags |= 1U << (int)States.Hungry;

            if (m_StateFlags != flags) {
                var old = m_StateFlags;
                m_StateFlags = flags;
                onStateFlagsChange.Invoke(this, m_StateFlags, old);
            }
        }

        public int GetRuneUses(Magic.Rune rune) {
            if (rune == null || rune.RestrictLevel > GetSkillValue(SkillTypes.Level)
                             || rune.RestrictMagicLevel > GetSkillValue(SkillTypes.MagLevel)
                             || (rune.RestrictProfession & (1 << m_Profession)) == 0)
                return -1;


            if (rune.CastMana <= Mana)
                return rune.CastMana > 0 ? Mana / rune.CastMana : int.MaxValue;
            return 0;
        }
    }
}
