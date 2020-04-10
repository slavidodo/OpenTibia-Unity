using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Creatures
{
    public struct Skill
    {
        public long Level;
        public long BaseLevel;
        public float Percentage;

        public Skill(int level, int baseLevel, float percentage) {
            Level = level;
            BaseLevel = baseLevel;
            Percentage = percentage;
        }

        public override string ToString() {
            return $"{{Level = {Level}, BaseLevel = {BaseLevel}, Percentage = {Percentage}}}";
        }
    }

    public class Creature {
        public class StringCreatureChangeEvent : UnityEvent<Creature, string, string> { }
        public class UIntCreatureChangeEvent : UnityEvent<Creature, uint, uint> { }
        public class IntCreatureChangeEvent : UnityEvent<Creature, int, int> { }
        public class IntCreatureChangeEvent2 : UnityEvent<Creature, int> { }
        public class DirectionCreatureChangeEvent : UnityEvent<Creature, Direction, Direction> { }
        public class BoolCreatureChangeEvent : UnityEvent<Creature, bool> { };
        public class SkillsCreatureChangeEvent : UnityEvent<Creature, SkillType, Skill> { };
        public class LightCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Color> { };
        public class PositionCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Vector3Int, UnityEngine.Vector3Int> { };
        public class MarksCreatureChangeEvent : UnityEvent<Creature, Appearances.Marks> { };
        public class OutfitCreatureChangeEvent : UnityEvent<Creature, Appearances.AppearanceInstance> { };

        public class CreatureTypesCreatureChangeEvent : UnityEvent<Creature, CreatureType, CreatureType> { };
        public class PartyFlagsCreatureChangeEvent : UnityEvent<Creature, PartyFlag, PartyFlag> { };
        public class PkFlagsCreatureChangeEvent : UnityEvent<Creature, PkFlag, PkFlag> { };
        public class GuildFlagsCreatureChangeEvent : UnityEvent<Creature, GuildFlag, GuildFlag> { };
        public class SpeechCategoryCreatureChangeEvent : UnityEvent<Creature, SpeechCategory, SpeechCategory> { };

        public static UIntCreatureChangeEvent onIdChange = new UIntCreatureChangeEvent();
        public static StringCreatureChangeEvent onNameChange = new StringCreatureChangeEvent();
        public static CreatureTypesCreatureChangeEvent onTypeChange = new CreatureTypesCreatureChangeEvent();
        public static UIntCreatureChangeEvent onSummonerChange = new UIntCreatureChangeEvent();
        public static BoolCreatureChangeEvent onUnpassableChange = new BoolCreatureChangeEvent();
        public static DirectionCreatureChangeEvent onDirectionChange = new DirectionCreatureChangeEvent();
        public static PartyFlagsCreatureChangeEvent onPartyFlagChange = new PartyFlagsCreatureChangeEvent();
        public static PkFlagsCreatureChangeEvent onPkFlagChange = new PkFlagsCreatureChangeEvent();
        public static GuildFlagsCreatureChangeEvent onGuildFlagChange = new GuildFlagsCreatureChangeEvent();
        public static OutfitCreatureChangeEvent onOutfitChange = new OutfitCreatureChangeEvent(); // outfit
        public static OutfitCreatureChangeEvent onMountOutfitChange = new OutfitCreatureChangeEvent(); // mountOutfit
        public static IntCreatureChangeEvent2 onBrightnessChange = new IntCreatureChangeEvent2(); // brightness
        public static LightCreatureChangeEvent onLightColorChange = new LightCreatureChangeEvent(); // color
        public static IntCreatureChangeEvent onSpeedChange = new IntCreatureChangeEvent(); // newSpeed, oldSpeed
        public static SpeechCategoryCreatureChangeEvent onSpeechCategoryChange = new SpeechCategoryCreatureChangeEvent(); // speechType
        public static IntCreatureChangeEvent2 onNumberOfPvPHelpersChange = new IntCreatureChangeEvent2(); // helpers
        public static PositionCreatureChangeEvent onPositionChange = new PositionCreatureChangeEvent(); // newPosition, oldPosition
        public static MarksCreatureChangeEvent onMarksChange = new MarksCreatureChangeEvent(); // marks
        public static BoolCreatureChangeEvent onVisbilityChange = new BoolCreatureChangeEvent(); // visibility
        public static SkillsCreatureChangeEvent onSkillChange = new SkillsCreatureChangeEvent(); // skill, level, baseLevel, percentage

        protected uint _id = 0;
        public uint Id {
            get { return _id; }
            set { if (_id != value) { var old = _id; _id = value; onIdChange.Invoke(this, _id, old); } }
        }

        protected string _name;
        public string Name {
            get { return _name; }
            set { var old = _name; _name = value; onNameChange.Invoke(this, _name, old); }
        }

        protected CreatureType _type = 0;
        public CreatureType Type {
            get { return _type; }
            set { if (_type != value) { var old = _type; _type = value; onTypeChange.Invoke(this, _type, old); } }
        }

        protected uint _summonerId = 0;
        public uint SummonerId {
            get { return _summonerId; }
            set { if (_summonerId != value) { var old = _summonerId; _summonerId = value; onSummonerChange.Invoke(this, _summonerId, old); } }
        }

        protected bool _trapper = false;
        public bool Trapper {
            get { return _trapper; }
            set { if (_trapper != value) { _trapper = value; } }
        }

        protected bool _unpassable = false;
        public bool Unpassable {
            get { return _unpassable; }
            set { if (_unpassable != value) { _unpassable = value; onUnpassableChange.Invoke(this, _unpassable); } }
        }

        protected Direction _direction = Direction.North;
        public Direction Direction {
            get { return _movementRunning ? _animationDirection : _direction; }
            set { if (_direction != value) { var old = _direction; _direction = value; onDirectionChange.Invoke(this, _direction, old); } }
        }

        protected PartyFlag _partyFlag = PartyFlag.None;
        public PartyFlag PartyFlag {
            get { return _partyFlag; }
            set { if (_partyFlag != value) { var old = _partyFlag; _partyFlag = value; onPartyFlagChange.Invoke(this, _partyFlag, old); } }
        }

        protected PkFlag _pkFlag = PkFlag.None;
        public PkFlag PkFlag {
            get { return _pkFlag; }
            set { if (_pkFlag != value) { var old = _pkFlag; _pkFlag = value; onPkFlagChange.Invoke(this, _pkFlag, old); } }
        }

        protected SummonType _summonType = SummonType.None;
        public SummonType SummonType {
            get { return _summonType; }
            set { if (_summonType != value) { _summonType = value; } }
        }

        protected SpeechCategory _speechCategory = SpeechCategory.None;
        public SpeechCategory SpeechCategory {
            get { return _speechCategory; }
            set { if (_speechCategory != value) { var old = _speechCategory; _speechCategory = value; onSpeechCategoryChange.Invoke(this, _speechCategory, old); } }
        }

        public RisknessFlag RisknessFlag {
            get {
                if (_numberOfPvPHelpers >= Constants.NumPvpHelpersForRisknessDangerous)
                    return RisknessFlag.Dangerous;
                return RisknessFlag.None;
            }
        }

        protected GuildFlag _guildFlag = GuildFlag.None;
        public GuildFlag GuildFlag {
            get { return _guildFlag; }
            set { if (_guildFlag != value) { var old = _guildFlag; _guildFlag = value; onGuildFlagChange.Invoke(this, _guildFlag, old); } }
        }

        protected Appearances.AppearanceInstance _outfit = null;
        public Appearances.AppearanceInstance Outfit {
            get { return _outfit; }
            set { if (_outfit != value) { _outfit = value; onOutfitChange.Invoke(this, _outfit); } }
        }

        protected Appearances.AppearanceInstance _mountOutfit = null;
        public Appearances.AppearanceInstance MountOutfit {
            get { return _mountOutfit; }
            set { if (_mountOutfit != value) { _mountOutfit = value; onMountOutfitChange.Invoke(this, _mountOutfit); } }
        }

        protected int _brightness = 0;
        public int Brightness {
            get { return _brightness; }
            set { if (_brightness != value) { _brightness = value; onBrightnessChange.Invoke(this, _brightness); } }
        }

        protected UnityEngine.Color _lightColor = UnityEngine.Color.black;
        public UnityEngine.Color LightColor {
            get { return _lightColor; }
            set { if (_lightColor != value) { _lightColor = value; onLightColorChange.Invoke(this, _lightColor); } }
        }
        
        protected int _numberOfPvPHelpers = 0;
        public int NumberOfPvPHelpers {
            get { return _numberOfPvPHelpers; }
            set { if (_numberOfPvPHelpers != value) { _numberOfPvPHelpers = value; onNumberOfPvPHelpersChange.Invoke(this, _numberOfPvPHelpers); } }
        }

        protected UnityEngine.Vector3Int _position = UnityEngine.Vector3Int.zero;
        public UnityEngine.Vector3Int Position {
            get { return _position; }
            set { if (_position != value) { var old = _position; _position = value; onPositionChange.Invoke(this, _position, old); } }
        }

        protected Appearances.Marks _marks = new Appearances.Marks();
        public Appearances.Marks Marks {
            get { return _marks; }
            set { if (_marks != value) { _marks = value;} }
        }

        protected bool m_Visible = false;
        public bool Visible {
            get { return m_Visible; }
            set { if (m_Visible != value) { m_Visible = value; onVisbilityChange.Invoke(this, m_Visible); } }
        }

        protected int _knownSince = -1;
        public int KnownSince {
            get { return _knownSince; }
            set { if (_knownSince != value) { _knownSince = value; } }
        }

        public virtual int HealthPercent { get => (int)GetSkillValue(SkillType.HealthPercent); }

        public virtual int ManaPercent { get => 100; }

        public bool IsHuman { get => _type == CreatureType.Player; }
        public bool IsMonster { get => _type == CreatureType.Monster; }
        public bool IsNPC { get => _type == CreatureType.NPC; }
        public bool IsSummon { get => _type == CreatureType.Summon; }
        public bool IsConfirmedPartyMember {
            get => _partyFlag == PartyFlag.Leader_SharedXP_Active
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || _partyFlag == PartyFlag.Leader_SharedXP_Off
                || _partyFlag == PartyFlag.Member_SharedXP_Active
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent
                || _partyFlag == PartyFlag.Member_SharedXP_Off;
        }
        public bool IsPartySharedExperienceActive {
            get => _partyFlag == PartyFlag.Leader_SharedXP_Active
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || _partyFlag == PartyFlag.Member_SharedXP_Active
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent;
        }
        public bool IsPartyLeader {
            get => _partyFlag == PartyFlag.Leader
                || _partyFlag == PartyFlag.Leader_SharedXP_Active
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || _partyFlag == PartyFlag.Leader_SharedXP_Off;
        }
        public bool IsPartyMember {
            get => _partyFlag == PartyFlag.Member
                || _partyFlag == PartyFlag.Member_SharedXP_Active
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || _partyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent
                || _partyFlag == PartyFlag.Member_SharedXP_Off;
        }
        public bool HasFlag {
            get => PkFlag != PkFlag.None
                || PartyFlag != PartyFlag.None
                || SummonType != SummonType.None
                || GuildFlag != GuildFlag.None
                || RisknessFlag != RisknessFlag.None
                || SpeechCategory != SpeechCategory.None;
        }

        public UnityEngine.Vector3Int AnimationDelta {
            get { return _animationDelta; }
        }

        protected int _movementEnd = 0;
        protected Direction _animationDirection = Direction.North;
        protected int _animationEnd = 0;
        protected bool _movementRunning = false;
        protected UnityEngine.Vector3Int _animationDelta = UnityEngine.Vector3Int.zero;
        protected UnityEngine.Vector3Int _animationSpeed = UnityEngine.Vector3Int.zero;
        protected Skill[] _skills;

        public static double SpeedA = 0.0f;
        public static double SpeedB = 1.0f;
        public static double SpeedC = 1.0f;

        public Creature(uint id, CreatureType type = CreatureType.Monster, string name = null) {
            _id = id;
            _type = type;
            _name = name;

            _marks.onMarksChange.AddListener((marks) => { onMarksChange.Invoke(this, marks); });
            ResetSkills();
        }
        
        public virtual void Reset() {
            ResetSkills();
            _animationDelta = UnityEngine.Vector3Int.zero;
            _animationDirection = Direction.North;
            _animationEnd = 0;
            _animationSpeed = UnityEngine.Vector3Int.zero;
            _brightness = 0;
            _direction = Direction.North;
            _id = 0;
            _trapper = false;
            _unpassable = false;
            _knownSince = -1;
            _lightColor = UnityEngine.Color.black;
            _movementEnd = 0;
            _movementRunning = false;
            Name = null;
            _partyFlag = PartyFlag.None;
            _pkFlag = PkFlag.Revenge;
            _type = CreatureType.Monster;
            m_Visible = false;
            _guildFlag = GuildFlag.None;
            _marks.Clear();
            _mountOutfit = null;
            _outfit = null;
            _position = UnityEngine.Vector3Int.zero;
        }

        public virtual void ResetSkills() {
            _skills = new Skill[(int)SkillType.Speed + 1];
        }

        public virtual void SetSkill(SkillType skillType, long level, long baseLevel = 0, float percentage = 0) {
            int skill = (int)skillType;
            if (skill >= _skills.Length)
                return;

            _skills[skill].Level = level;
            _skills[skill].BaseLevel = baseLevel;
            _skills[skill].Percentage = percentage;
            onSkillChange.Invoke(this, skillType, _skills[skill]);
        }

        public virtual void SetSkillValue(SkillType skillType, int level) {
            int skill = (int)skillType;
            if (skill >= _skills.Length)
                return;

            if (_skills[skill].Level != level) {
                _skills[skill].Level = level;
                onSkillChange.Invoke(this, skillType, _skills[skill]);
            }
        }

        public Skill GetSkill(SkillType skill) {
            if ((int)skill >= _skills.Length)
                return default;

            return _skills[(int)skill];
        }

        public long GetSkillValue(SkillType skill) {
            if ((int)skill >= _skills.Length)
                return 0;

            return _skills[(int)skill].Level;
        }

        public long GetSkillbase(SkillType skill) {
            if ((int)skill >= _skills.Length)
                return 0;

            return _skills[(int)skill].BaseLevel;
        }

        public double GetSKillProgress(SkillType skill) {
            if ((int)skill >= _skills.Length)
                return 0;

            return _skills[(int)skill].Percentage;
        }

        public virtual void StartMovementAnimation(int deltaX, int deltaY, int movementCost) {
            if (deltaX > 0)
                _direction = Direction.East;
            else if (deltaX < 0)
                _direction = Direction.West;
            else if (deltaY < 0)
                _direction = Direction.North;
            else if (deltaY > 0)
                _direction = Direction.South;

            _animationDirection = _direction;
            _animationDelta.x = -deltaX * Constants.FieldSize;
            _animationDelta.y = -deltaY * Constants.FieldSize;
            _animationDelta.z = 0;
            
            int beatDuration = OpenTibiaUnity.ProtocolGame.BeatDuration;

            int movementSpeed = System.Math.Max(1, GetMovementSpeed());
            int interval = (int)System.Math.Floor(1000 * movementCost / (float)movementSpeed);

            if (OpenTibiaUnity.GameManager.ClientVersion >= 910)
                interval = (int)System.Math.Ceiling(interval / (double)beatDuration) * beatDuration;

            _animationSpeed.x = _animationDelta.x;
            _animationSpeed.y = _animationDelta.y;
            _animationSpeed.z = interval;
            _animationEnd = OpenTibiaUnity.TicksMillis + interval;

            float diagonalFactor = 3f;
            if (OpenTibiaUnity.GameManager.ClientVersion <= 810)
                diagonalFactor = 2f;

            if (deltaX != 0 && deltaY != 0)
                interval = (int)System.Math.Floor(1000 * movementCost * diagonalFactor / movementSpeed);

            _movementEnd = OpenTibiaUnity.TicksMillis + interval;
            _movementRunning = true;
        }

        public void StopMovementAnimation() {
            _animationDirection = _direction;
            _animationDelta = UnityEngine.Vector3Int.zero;
            _animationSpeed = UnityEngine.Vector3Int.zero;
            _animationEnd = 0;
            _movementEnd = 0;
            _movementRunning = false;
        }

        public virtual void AnimateMovement(int ticks) {
            _animationDelta = UnityEngine.Vector3Int.zero;
            if (_movementRunning) {
                int diff = ticks - (_animationEnd - _animationSpeed.z);
                if (diff < 0) {
                    _animationDelta.x = _animationSpeed.x;
                    _animationDelta.y = _animationSpeed.y;
                } else if (diff >= _animationSpeed.z) {
                    _animationDelta.x = 0;
                    _animationDelta.y = 0;
                } else if (_animationSpeed.z != 0) {
                    _animationDelta.x = _animationSpeed.x - (int)System.Math.Round((double)_animationSpeed.x * diff / _animationSpeed.z);
                    _animationDelta.y = _animationSpeed.y - (int)System.Math.Round((double)_animationSpeed.y * diff / _animationSpeed.z);
                }
            }

            _movementRunning = ticks < _movementEnd || _animationDelta.x != 0 || _animationDelta.y != 0;
        }

        public void AnimateOutfit(int ticks) {
            bool isIdle = !_movementRunning || _movementEnd != _animationEnd && ticks >= _animationEnd;
            if (!!_outfit && !!_outfit.Type) {
                _outfit.SwitchFrameGroup(ticks, isIdle ? (int)Protobuf.Shared.FrameGroupType.Idle : (int)Protobuf.Shared.FrameGroupType.Walking);
                _outfit.Animate(ticks, isIdle ? 0 : _animationSpeed.z);
            }

            if (!!_mountOutfit && !!_mountOutfit.Type) {
                _mountOutfit.SwitchFrameGroup(ticks, isIdle ? (int)Protobuf.Shared.FrameGroupType.Idle : (int)Protobuf.Shared.FrameGroupType.Walking);
                bool result = _mountOutfit.Animate(ticks, isIdle ? 0 : _animationSpeed.z);
            }
        }

        public int GetMovementSpeed() {
            int speed = (int)GetSkillValue(SkillType.Speed);
            if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewSpeedLaw))
                return speed;

            if (speed <= -SpeedB)
                return 0;

            return (int)System.Math.Round(SpeedA * System.Math.Log(speed + SpeedB) + SpeedC);
        }

        public void SetPartyFlag(PartyFlag partyFlag) {
            if (partyFlag < PartyFlag.First || partyFlag > PartyFlag.Last)
                throw new System.ArgumentException("Creature.SetPartyFlag: Invalid party flag (" + (int)partyFlag + ").");

            PartyFlag = partyFlag;
        }

        public void SetPKFlag(PkFlag pkFlag) {
            if (pkFlag < PkFlag.First || pkFlag > PkFlag.Last)
                throw new System.ArgumentException("Creature.SetPKFlag: Invalid pkFlag (" + (int)pkFlag + ")");

            PkFlag = pkFlag;
        }

        public void SetSpeechCategory(SpeechCategory speechCategory) {
            if (speechCategory < SpeechCategory.First || speechCategory > SpeechCategory.Last)
                throw new System.Exception("Creature.SetSpeechCategory: Invalid speechCategory (" + (int)speechCategory + ")");

            SpeechCategory = speechCategory;
        }

        public void SetGuildFlag(GuildFlag guildFlag) {
            if (guildFlag < GuildFlag.First || guildFlag > GuildFlag.Last)
                throw new System.ArgumentException("Creature.SetGuildFlag: Invalid guildFlag (" + (int)guildFlag + ")");

            GuildFlag = guildFlag;
        }

        public void SetSummonerId(uint summonerId) {
            if (summonerId == OpenTibiaUnity.Player._id)
                SummonType = SummonType.Own;
            else if (summonerId != 0)
                SummonType = SummonType.Other;
            else
                SummonType = SummonType.None;

            SummonerId = summonerId;
        }
        
        public bool IsReportTypeAllowed(ReportTypes reportType) {
            return IsHuman && (reportType == ReportTypes.Name || reportType == ReportTypes.Bot);
        }

        public UnityEngine.Color GetHealthColor() {
            return GetHealthColor(HealthPercent);
        }

        public static UnityEngine.Color32 GetHealthColor(int percent) {
            UnityEngine.Color healthColor;
            if (percent < 4) {
                healthColor = Colors.ColorFromRGB(96, 0, 0);
            } else if (percent < 10) {
                healthColor = Colors.ColorFromRGB(192, 0, 0);
            } else if (percent < 30) {
                healthColor = Colors.ColorFromRGB(192, 48, 48);
            } else if (percent < 60) {
                healthColor = Colors.ColorFromRGB(192, 192, 0);
            } else if (percent < 95) {
                healthColor = Colors.ColorFromRGB(96, 192, 96);
            } else {
                healthColor = Colors.ColorFromRGB(0, 192, 0);
            }
            return healthColor;
        }

        public static bool operator !(Creature creature) {
            return creature == null;
        }

        public static bool operator true(Creature creature) {
            return !(!creature);
        }

        public static bool operator false(Creature creature) {
            return !(creature);
        }
    }
}
