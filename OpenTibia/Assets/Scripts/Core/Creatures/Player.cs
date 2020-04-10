using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Creatures
{
    public sealed class Player : Creature
    {
        public class UIntPlayerChangeEvent : UnityEvent<Player, uint, uint> { }
        
        public UIntCreatureChangeEvent onStateFlagsChange = new UIntCreatureChangeEvent();
        public IntCreatureChangeEvent onFreeCapacityChange = new IntCreatureChangeEvent();
        public BoolCreatureChangeEvent onDepotStateChange = new BoolCreatureChangeEvent();

        private bool _autowalkPathAborting = false;
        private bool _autowalkTargetDiagonal = false;
        private bool _autowalkTargetExact = false;
#pragma warning disable CS0414 // Remove unread private members
        private bool _premium = false;
        private bool _hasReachedMain = false;

        private int _premiumUntil = 0;
        private int _profession = 0;
        private int _bankGoldBalance = 0;
        private int _inventoryGoldBalance = 0;
#pragma warning restore CS0414 // Remove unread private members

        private double _experienceBonus = 0.0f;

        private UnityEngine.Vector3Int _autowalkPathDelta = UnityEngine.Vector3Int.zero;
        private UnityEngine.Vector3Int _autowalkTarget = new UnityEngine.Vector3Int(-1, -1, -1);
        private List<int> _autowalkPathSteps = new List<int>();
        private List<int> _knownSpells = new List<int>();

        public override int HealthPercent {
            get => (int)((GetSkillValue(SkillType.Health) / (float)GetSkillbase(SkillType.Health)) * 100);
        }

        public override int ManaPercent {
            get {
                long maxMana = GetSkillbase(SkillType.Mana);
                if (maxMana == 0)
                    return 100;
                
                long mana = GetSkillValue(SkillType.Mana);
                return (int)((mana / (float)maxMana) * 100);
            }
        }

        public long Mana {
            get => GetSkillValue(SkillType.Mana);
        }

        public long Level {
            get => GetSkillValue(SkillType.Level);
        }

        public double LevelPercent {
            get => GetSKillProgress(SkillType.Level);
        }

        public int EarliestMoveTime {
            get => _movementEnd;
            set => _movementEnd = value;
        }

        private ExperienceGainInfo _experienceGainInfo = new ExperienceGainInfo();
        private SkillCounter _experienceCounter = new SkillCounter();

        public double ExperienceBonus { get => _experienceBonus; set => _experienceBonus = value; } // <= 1096
        public ExperienceGainInfo ExperienceGainInfo { get => _experienceGainInfo; } // >= 1097

        private uint _stateFlags = 0;
        public uint StateFlags {
            get => _stateFlags;
            set => UpdateStateFlags(value);
        }

        private uint _blessings = 0;
        public uint Blessings {
            get => _blessings;
            set => _blessings = value; // todo dispatch
        }

        private bool _hasFullBlessings;
        public bool HasFullBlessings {
            get {
                if (OpenTibiaUnity.GameManager.ProtocolVersion >= 1141)
                    return (_blessings & (uint)BlessingTypes.All_1141) == (uint)BlessingTypes.All_1141;
                else if (OpenTibiaUnity.GameManager.ProtocolVersion >= 1120)
                    return (_blessings & (uint)BlessingTypes.All_1120) == (uint)BlessingTypes.All_1120;
                else
                    return _hasFullBlessings;
            }

            set {
                if (OpenTibiaUnity.GameManager.ProtocolVersion >= 1141) {
                    if (value)
                        _blessings |= (uint)BlessingTypes.All_1141;
                    else
                        _blessings &= ~(uint)BlessingTypes.All_1141;
                } else if (OpenTibiaUnity.GameManager.ProtocolVersion >= 1120) {
                    if (value)
                        _blessings |= (uint)BlessingTypes.All_1120;
                    else
                        _blessings &= ~(uint)BlessingTypes.All_1120;
                } else {
                    _hasFullBlessings = value;
                }
            }
        }

        private bool _isInDepot = false;
        public bool IsInDepot {
            get => _isInDepot;
            set { var old = _isInDepot; _isInDepot = value; if (value != old) onDepotStateChange.Invoke(this, _isInDepot); }
        }

        private int _freeCapacity = 0;
        public int FreeCapacity {
            get => _freeCapacity;
            set { var old = _freeCapacity; _freeCapacity = value; if (value != old) onFreeCapacityChange.Invoke(this, old, _freeCapacity); }
        }

        public UnityEngine.Vector3Int AnticipatedPosition {
            get => _position + _autowalkPathDelta;
        }

        public bool IsFighting {
            get => (_stateFlags & 1 << (int)PlayerState.Fighting) > 0;
        }

        public Player() : this(0) {}
        public Player(uint id) : this(id, null) {}
        public Player(uint id, string name) : base(id, CreatureType.Player, null) {}

        public void StartAutowalk(UnityEngine.Vector3Int tarReadPosition, bool diagonal, bool exact) {
            StartAutowalk(tarReadPosition.x, tarReadPosition.y, tarReadPosition.z, diagonal, exact);
        }

        public void StartAutowalk(int targetX, int targetY, int targetZ, bool diagonal, bool exact) {
            if (targetX == _autowalkTarget.x && targetY == _autowalkTarget.y && targetZ == _autowalkTarget.z)
                return;

            if (targetX == _position.x + 2 * _autowalkPathDelta.x
                && targetY == _position.y + 2 * _autowalkPathDelta.y
                && targetZ == _position.z + 2 * _autowalkPathDelta.z)
                return;

            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (!protocolGame || !protocolGame.IsGameRunning || worldMapStorage == null)
                return;

            _autowalkTarget.Set(-1, -1, -1);
            _autowalkTargetDiagonal = false;
            _autowalkTargetExact = false;

            // check if the tile can actually be entered (before proceeding with anything)
            // the tile must be visible, otherwise it would then be checked in the pathfinder
            if (worldMapStorage.IsVisible(targetX, targetY, targetZ, true)) {
                UnityEngine.Vector3Int s_v1 = new UnityEngine.Vector3Int(targetX, targetY, targetZ);
                worldMapStorage.ToMap(s_v1, out s_v1);
                if ((s_v1.x != Constants.PlayerOffsetX || s_v1.y != Constants.PlayerOffsetY) && worldMapStorage.GetEnterPossibleFlag(s_v1.x, s_v1.y, s_v1.z, true) == EnterPossibleFlag.NotPossible) {
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_SORRY_NOT_POSSIBLE);
                    return;
                }
            }

            _autowalkTarget.Set(targetX, targetY, targetZ);
            _autowalkTargetDiagonal = diagonal;
            _autowalkTargetExact = exact;

            // we can't walk while already walking //
            if (_autowalkPathAborting || _autowalkPathSteps.Count == 1)
                return;

            // if there is no steps left, permit a new walk
            if (_autowalkPathSteps.Count == 0) {
                StartAutowalkInternal();
            } else {
                protocolGame.SendStop();
                _autowalkPathAborting = true;
            }
        }

        private void StartAutowalkInternal() {
            if (_movementRunning || _autowalkPathAborting || _autowalkPathDelta != UnityEngine.Vector3Int.zero || _autowalkTarget.x == -1 && _autowalkTarget.y == -1 && _autowalkTarget.z == -1)
                return;
            
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var minimapStorage = OpenTibiaUnity.MiniMapStorage;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (!protocolGame || !protocolGame.IsGameRunning || minimapStorage == null || worldMapStorage == null)
                return;

            _autowalkPathAborting = false;
            _autowalkPathDelta.Set(0, 0, 0);
            _autowalkPathSteps.Clear();
            
            PathState pathState = minimapStorage.CalculatePath(_position, _autowalkTarget, _autowalkTargetDiagonal, _autowalkTargetExact, _autowalkPathSteps);
            if (worldMapStorage.IsVisible(_autowalkTarget, false)) {
                _autowalkTarget.Set(-1, -1, -1);
                _autowalkTargetDiagonal = false;
                _autowalkTargetExact = false;
            }
            
            switch (pathState) {
                case PathState.PathEmpty:
                    break;
                case PathState.PathExists:
                    protocolGame.SendGo(_autowalkPathSteps);
                    NextAutowalkStep();
                    break;
                case PathState.PathErrorGoDownstairs:
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_PATH_GO_DOWNSTAIRS);
                    break;
                case PathState.PathErrorGoUpstairs:
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_PATH_GO_UPSTAIRS);
                    break;
                case PathState.PathErrorTooFar:
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_PATH_TOO_FAR);
                    break;
                case PathState.PathErrorUnreachable:
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_PATH_UNREACHABLE);
                    StopAutowalk(false);
                    break;
                case PathState.PathErrorInternal:
                    worldMapStorage.AddOnscreenMessage(MessageModeType.Failure, TextResources.MSG_SORRY_NOT_POSSIBLE);
                    StopAutowalk(false);
                    break;
                default:
                    throw new System.Exception("Player.StartAutowalkpublic: Unknown path state: " + pathState);
            }
        }

        private void NextAutowalkStep() {
            if (_movementRunning || _autowalkPathAborting || _autowalkPathDelta != UnityEngine.Vector3Int.zero || _autowalkPathSteps.Count == 0)
                return;
            
            var protocolGame = OpenTibiaUnity.ProtocolGame;
            var worldMapStorage = OpenTibiaUnity.WorldMapStorage;
            if (!protocolGame || !protocolGame.IsGameRunning || worldMapStorage == null)
                return;

            switch ((PathDirection)(_autowalkPathSteps[0] & 65535)) {
                case PathDirection.East:
                    _autowalkPathDelta.x = 1;
                    break;
                case PathDirection.NorthEast:
                    _autowalkPathDelta.x = 1;
                    _autowalkPathDelta.y = -1;
                    break;
                case PathDirection.North:
                    _autowalkPathDelta.y = -1;
                    break;
                case PathDirection.NorthWest:
                    _autowalkPathDelta.x = -1;
                    _autowalkPathDelta.y = -1;
                    break;
                case PathDirection.West:
                    _autowalkPathDelta.x = -1;
                    break;
                case PathDirection.SouthWest:
                    _autowalkPathDelta.x = -1;
                    _autowalkPathDelta.y = 1;
                    break;
                case PathDirection.South:
                    _autowalkPathDelta.y = 1;
                    break;
                case PathDirection.SouthEast:
                    _autowalkPathDelta.x = 1;
                    _autowalkPathDelta.y = 1;
                    break;
                default:
                    throw new System.Exception("Player.NextAutowalkStep: Invalid step(1): " + (_autowalkPathSteps[0] & 65535));
            }

            var mapPosition = worldMapStorage.ToMap(_position);
            var tmpMapPosition = mapPosition;

            tmpMapPosition.x += _autowalkPathDelta.x;
            tmpMapPosition.y += _autowalkPathDelta.y;

            // Tile should be walkable (full ground)
            Appearances.ObjectInstance @object = null;
            if (!(@object = worldMapStorage.GetObject(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z, 0)) || !@object.Type || !@object.Type.IsGround) {
                _autowalkPathDelta.Set(0, 0, 0);
                return;
            }

            EnterPossibleFlag enterFlag = worldMapStorage.GetEnterPossibleFlag(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z, false);
            if (enterFlag == EnterPossibleFlag.NotPossible || worldMapStorage.GetFieldHeight(mapPosition.x, mapPosition.y, mapPosition.z) + 1 < worldMapStorage.GetFieldHeight(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z)) {
                _autowalkPathDelta.Set(0, 0, 0);
                return;
            }

            if (enterFlag == EnterPossibleFlag.Possible) {
                base.StartMovementAnimation(_autowalkPathDelta.x, _autowalkPathDelta.y, (int)@object.Type.GroundSpeed);
                _animationDelta.x = _animationDelta.x + _autowalkPathDelta.x * Constants.FieldSize;
                _animationDelta.y = _animationDelta.y + _autowalkPathDelta.y * Constants.FieldSize;
            } else if (enterFlag == EnterPossibleFlag.PossibleNoAnimation) {
                _animationDelta.Set(0, 0, 0);
            }

            for (int i = 1; i < _autowalkPathSteps.Count; i++) {
                var pathDirection = (PathDirection)(_autowalkPathSteps[i] & 65535);
                var mapCost = ((uint)_autowalkPathSteps[i] & 4294901760U) >> 16;
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
                        throw new System.Exception("Player.NextAutowalkStep: Invalid step(2): " + (_autowalkPathSteps[i] & 65535));
                }

                if (tmpMapPosition.x < 0 || tmpMapPosition.x >= Constants.MapSizeX || tmpMapPosition.y < 0 || tmpMapPosition.y >= Constants.MapSizeY)
                    break;

                if (worldMapStorage.GetMiniMapCost(tmpMapPosition.x, tmpMapPosition.y, tmpMapPosition.z) > mapCost) {
                    protocolGame.SendStop();
                    _autowalkPathAborting = true;
                    break;
                }
            }
        }

        public void StopAutowalk(bool stopAnimation) {
            if (stopAnimation)
                StopMovementAnimation();
            _autowalkPathAborting = false;
            _autowalkPathDelta.Set(0, 0, 0);
            _autowalkPathSteps.Clear();
            _autowalkTarget.Set(-1, -1, -1);
            _autowalkTargetDiagonal = false;
            _autowalkTargetExact = false;
        }

        public void AbortAutowalk(Direction direction) {
            _direction = direction;
            _autowalkPathAborting = false;
            _autowalkPathSteps.Clear();
            if (!_movementRunning || _autowalkPathDelta != UnityEngine.Vector3Int.zero) {
                _autowalkPathDelta.Set(0, 0, 0);
                StopMovementAnimation();
                StartAutowalkInternal();
            }
        }

        public void ResetAutowalk() {
            StopAutowalk(false);
            AbortAutowalk(Direction.South);
        }

        public override void StartMovementAnimation(int deltaX, int deltaY, int movementCost) {
            if (deltaX != _autowalkPathDelta.x || deltaY != _autowalkPathDelta.y)
                base.StartMovementAnimation(deltaX, deltaY, movementCost);

            _autowalkPathDelta.Set(0, 0, 0);
            if (_autowalkPathSteps.Count > 0) {
                _autowalkPathSteps.RemoveAt(0);
            }
        }

        public override void AnimateMovement(int ticks) {
            base.AnimateMovement(ticks);
            _animationDelta.x += _autowalkPathDelta.x * Constants.FieldSize;
            _animationDelta.y += _autowalkPathDelta.y * Constants.FieldSize;
            if (!_movementRunning && _autowalkPathDelta == UnityEngine.Vector3Int.zero) {
                if (_autowalkPathSteps.Count > 0)
                    NextAutowalkStep();
                else
                    StartAutowalkInternal();
            }
        }

        public void ResetFlags() {

        }

        public override void ResetSkills() {
            _skills = new Skill[(int)SkillType.ManaLeechAmount + 1];
            _experienceCounter.Reset();
            _experienceGainInfo.Reset();
        }

        public override void SetSkill(SkillType skillType, long level, long baseLevel = 0, float percentage = 0) {
            switch (skillType) {
                case SkillType.Experience:
                    var currentLevel = GetSkillValue(skillType);
                    if (level >= currentLevel) {
                        if (currentLevel > 0)
                            _experienceCounter.AddSkillGain(level - currentLevel);
                    } else {
                        _experienceCounter.Reset();
                    }

                    base.SetSkill(skillType, level, baseLevel, percentage);
                    break;
                case SkillType.Food:
                    base.SetSkill(skillType, level, baseLevel, percentage);
                    UpdateStateFlags();
                    break;
                default:
                    base.SetSkill(skillType, level, baseLevel, percentage);
                    break;
            }
        }

        public override void SetSkillValue(SkillType skillType, int level) {
            switch (skillType) {
                case SkillType.Experience:
                    var currentLevel = GetSkillValue(skillType);
                    if (level >= currentLevel) {
                        if (currentLevel > 0)
                            _experienceCounter.AddSkillGain(level - currentLevel);
                    } else {
                        _experienceCounter.Reset();
                    }

                    base.SetSkillValue(skillType, level);
                    break;
                case SkillType.Food:
                    base.SetSkillValue(skillType, level);
                    UpdateStateFlags();
                    break;
                default:
                    base.SetSkillValue(skillType, level);
                    break;
            }
        }

        public override void Reset() {
            uint myId = _id;
            base.Reset();
            ResetAutowalk();
            ResetFlags();
            ResetSkills();
            _id = myId;
            _knownSpells.Clear();
            _premium = false;
            _premiumUntil = 0;
            _hasReachedMain = false;
            _blessings = 0;
            _profession = 0;
            _type = CreatureType.Player;
            _bankGoldBalance = 0;
            _inventoryGoldBalance = 0;
        }

        public void UpdateStateFlags(uint? value = null) {
            var flags = value ?? _stateFlags;
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GamePlayerRegenerationTime)
                && GetSkillValue(SkillType.Food) <= GetSkillbase(SkillType.Food))
                flags |= 1U << (int)PlayerState.Hungry;

            if (_stateFlags != flags) {
                var old = _stateFlags;
                _stateFlags = flags;
                onStateFlagsChange.Invoke(this, _stateFlags, old);
            }
        }

        public int GetRuneUses(Magic.Rune rune) {
            if (rune == null || rune.RestrictLevel > GetSkillValue(SkillType.Level)
                             || rune.RestrictMagicLevel > GetSkillValue(SkillType.MagLevel)
                             || (rune.RestrictProfession & (1 << _profession)) == 0)
                return -1;


            if (rune.CastMana <= Mana)
                return rune.CastMana > 0 ? (int)Mana / rune.CastMana : int.MaxValue;
            return 0;
        }
    }
}
