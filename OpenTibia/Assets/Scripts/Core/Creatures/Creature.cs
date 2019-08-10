using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Creatures
{
    internal struct Skill
    {
        internal long Level;
        internal long BaseLevel;
        internal float Percentage;

        internal Skill(int level, int baseLevel, float percentage) {
            Level = level;
            BaseLevel = baseLevel;
            Percentage = percentage;
        }
    }

    internal class Creature
    {
        internal class StringCreatureChangeEvent : UnityEvent<Creature, string, string> {}
        internal class UIntCreatureChangeEvent : UnityEvent<Creature, uint, uint> {}
        internal class IntCreatureChangeEvent : UnityEvent<Creature, int, int> {}
        internal class IntCreatureChangeEvent2 : UnityEvent<Creature, int> {}
        internal class DirectionCreatureChangeEvent : UnityEvent<Creature, Direction, Direction> {}
        internal class BoolCreatureChangeEvent : UnityEvent<Creature, bool> {};
        internal class SkillsCreatureChangeEvent : UnityEvent<Creature, SkillType, Skill> {};
        internal class LightCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Color> {};
        internal class PositionCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Vector3Int, UnityEngine.Vector3Int> {};
        internal class MarksCreatureChangeEvent : UnityEvent<Creature, Appearances.Marks> {};
        internal class OutfitCreatureChangeEvent : UnityEvent<Creature, Appearances.AppearanceInstance> {};

        internal class CreatureTypesCreatureChangeEvent : UnityEvent<Creature, CreatureType, CreatureType> {};
        internal class PartyFlagsCreatureChangeEvent : UnityEvent<Creature, PartyFlag, PartyFlag> {};
        internal class PKFlagsCreatureChangeEvent : UnityEvent<Creature, PKFlag, PKFlag> {};
        internal class GuildFlagsCreatureChangeEvent : UnityEvent<Creature, GuildFlag, GuildFlag> {};
        internal class SpeechCategoryCreatureChangeEvent : UnityEvent<Creature, SpeechCategory, SpeechCategory> {};

        internal static UIntCreatureChangeEvent onIDChange = new UIntCreatureChangeEvent();
        internal static StringCreatureChangeEvent onNameChange = new StringCreatureChangeEvent();
        internal static CreatureTypesCreatureChangeEvent onTypeChange = new CreatureTypesCreatureChangeEvent();
        internal static UIntCreatureChangeEvent onSummonerChange = new UIntCreatureChangeEvent();
        internal static BoolCreatureChangeEvent onUnpassableChange = new BoolCreatureChangeEvent();
        internal static DirectionCreatureChangeEvent onDirectionChange = new DirectionCreatureChangeEvent();
        internal static PartyFlagsCreatureChangeEvent onPartyFlagChange = new PartyFlagsCreatureChangeEvent();
        internal static PKFlagsCreatureChangeEvent onPKFlagChange = new PKFlagsCreatureChangeEvent();
        internal static GuildFlagsCreatureChangeEvent onGuildFlagChange = new GuildFlagsCreatureChangeEvent();
        internal static OutfitCreatureChangeEvent onOutfitChange = new OutfitCreatureChangeEvent(); // outfit
        internal static OutfitCreatureChangeEvent onMountOutfitChange = new OutfitCreatureChangeEvent(); // mountOutfit
        internal static IntCreatureChangeEvent2 onBrightnessChange = new IntCreatureChangeEvent2(); // brightness
        internal static LightCreatureChangeEvent onLightColorChange = new LightCreatureChangeEvent(); // color
        internal static IntCreatureChangeEvent onSpeedChange = new IntCreatureChangeEvent(); // newSpeed, oldSpeed
        internal static SpeechCategoryCreatureChangeEvent onSpeechCategoryChange = new SpeechCategoryCreatureChangeEvent(); // speechType
        internal static IntCreatureChangeEvent2 onNumberOfPvPHelpersChange = new IntCreatureChangeEvent2(); // helpers
        internal static PositionCreatureChangeEvent onPositionChange = new PositionCreatureChangeEvent(); // newPosition, oldPosition
        internal static MarksCreatureChangeEvent onMarksChange = new MarksCreatureChangeEvent(); // marks
        internal static BoolCreatureChangeEvent onVisbilityChange = new BoolCreatureChangeEvent(); // visibility
        internal static SkillsCreatureChangeEvent onSkillChange = new SkillsCreatureChangeEvent(); // skill, level, baseLevel, percentage

        protected uint m_ID = 0;
        internal uint ID {
            get { return m_ID; }
            set { if (m_ID != value) { var old = m_ID;  m_ID = value; onIDChange.Invoke(this, m_ID, old); } }
        }

        protected string m_Name;
        internal string Name {
            get { return m_Name; }
            set { var old = m_Name; m_Name = value; onNameChange.Invoke(this, m_Name, old); }
        }

        protected CreatureType m_Type = 0;
        internal CreatureType Type {
            get { return m_Type; }
            set { if (m_Type != value) { var old = m_Type;  m_Type = value; onTypeChange.Invoke(this, m_Type, old); } }
        }

        protected uint m_SummonerID = 0;
        internal uint SummonerID {
            get { return m_SummonerID; }
            set { if (m_SummonerID != value) { var old = m_SummonerID;  m_SummonerID = value; onSummonerChange.Invoke(this, m_SummonerID, old); } }
        }

        protected bool m_Trapper = false;
        internal bool Trapper {
            get { return m_Trapper; }
            set { if (m_Trapper != value) { m_Trapper = value; } }
        }

        protected bool m_Unpassable = false;
        internal bool Unpassable {
            get { return m_Unpassable; }
            set { if (m_Unpassable != value) { m_Unpassable = value; onUnpassableChange.Invoke(this, m_Unpassable); } }
        }

        protected Direction m_Direction = Direction.North;
        internal Direction Direction {
            get { return m_MovementRunning ? m_AnimationDirection : m_Direction; }
            set { if (m_Direction != value) { var old = m_Direction; m_Direction = value; onDirectionChange.Invoke(this, m_Direction, old); } }
        }

        protected PartyFlag m_PartyFlag = PartyFlag.None;
        internal PartyFlag PartyFlag {
            get { return m_PartyFlag; }
            set { if (m_PartyFlag != value) { var old = m_PartyFlag; m_PartyFlag = value; onPartyFlagChange.Invoke(this, m_PartyFlag, old); } }
        }

        protected PKFlag m_PKFlag = PKFlag.None;
        internal PKFlag PKFlag {
            get { return m_PKFlag; }
            set { if (m_PKFlag != value) { var old = m_PKFlag; m_PKFlag = value; onPKFlagChange.Invoke(this, m_PKFlag, old); } }
        }

        protected SummonTypeFlags m_SummonTypeFlag = SummonTypeFlags.None;
        internal SummonTypeFlags SummonTypeFlag {
            get { return m_SummonTypeFlag; }
            set { if (m_SummonTypeFlag != value) { m_SummonTypeFlag = value; } }
        }

        protected SpeechCategory m_SpeechCategory = SpeechCategory.None;
        internal SpeechCategory SpeechCategory {
            get { return m_SpeechCategory; }
            set { if (m_SpeechCategory != value) { var old = m_SpeechCategory;  m_SpeechCategory = value; onSpeechCategoryChange.Invoke(this, m_SpeechCategory, old); } }
        }

        protected GuildFlag m_GuildFlag = GuildFlag.None;
        internal GuildFlag GuildFlag {
            get { return m_GuildFlag; }
            set { if (m_GuildFlag != value) { var old = m_GuildFlag; m_GuildFlag = value; onGuildFlagChange.Invoke(this, m_GuildFlag, old); } }
        }

        protected Appearances.AppearanceInstance m_Outfit = null;
        internal Appearances.AppearanceInstance Outfit {
            get { return m_Outfit; }
            set { if (m_Outfit != value) { m_Outfit = value; onOutfitChange.Invoke(this, m_Outfit); } }
        }

        protected Appearances.AppearanceInstance m_MountOutfit = null;
        internal Appearances.AppearanceInstance MountOutfit {
            get { return m_MountOutfit; }
            set { if (m_MountOutfit != value) { m_MountOutfit = value; onMountOutfitChange.Invoke(this, m_MountOutfit); } }
        }

        protected int m_Brightness = 0;
        internal int Brightness {
            get { return m_Brightness; }
            set { if (m_Brightness != value) { m_Brightness = value; onBrightnessChange.Invoke(this, m_Brightness); } }
        }

        protected UnityEngine.Color m_LightColor = UnityEngine.Color.black;
        internal UnityEngine.Color LightColor {
            get { return m_LightColor; }
            set { if (m_LightColor != value) { m_LightColor = value; onLightColorChange.Invoke(this, m_LightColor); } }
        }
        
        protected int m_NumberOfPvPHelpers = 0;
        internal int NumberOfPvPHelpers {
            get { return m_NumberOfPvPHelpers; }
            set { if (m_NumberOfPvPHelpers != value) { m_NumberOfPvPHelpers = value; onNumberOfPvPHelpersChange.Invoke(this, m_NumberOfPvPHelpers); } }
        }

        protected UnityEngine.Vector3Int m_Position = UnityEngine.Vector3Int.zero;
        internal UnityEngine.Vector3Int Position {
            get { return m_Position; }
            set { if (m_Position != value) { var old = m_Position; m_Position = value; onPositionChange.Invoke(this, m_Position, old); } }
        }

        protected Appearances.Marks m_Marks = new Appearances.Marks();
        internal Appearances.Marks Marks {
            get { return m_Marks; }
            set { if (m_Marks != value) { m_Marks = value;} }
        }

        protected bool m_Visible = false;
        internal bool Visible {
            get { return m_Visible; }
            set { if (m_Visible != value) { m_Visible = value; onVisbilityChange.Invoke(this, m_Visible); } }
        }

        protected int m_KnownSince = -1;
        internal int KnownSince {
            get { return m_KnownSince; }
            set { if (m_KnownSince != value) { m_KnownSince = value; } }
        }

        internal virtual int HealthPercent {
            get {
                return (int)GetSkillValue(SkillType.HealthPercent);
            }
        }

        internal virtual int ManaPercent {
            get { return 100; }
        }

        internal bool IsHuman {
            get { return m_Type == CreatureType.Player; }
        }
        internal bool IsMonster {
            get { return m_Type == CreatureType.Monster; }
        }
        internal bool IsNPC {
            get { return m_Type == CreatureType.NPC; }
        }
        internal bool IsSummon {
            get { return m_Type == CreatureType.Summon; }
        }
        internal bool IsConfirmedPartyMember {
            get => m_PartyFlag == PartyFlag.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Off
                || m_PartyFlag == PartyFlag.Member_SharedXP_Active
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlag.Member_SharedXP_Off;
        }
        internal bool IsPartySharedExperienceActive {
            get => m_PartyFlag == PartyFlag.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlag.Member_SharedXP_Active
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent;
        }
        internal bool IsPartyLeader {
            get => m_PartyFlag == PartyFlag.Leader
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlag.Leader_SharedXP_Off;
        }
        internal bool IsPartyMember {
            get => m_PartyFlag == PartyFlag.Member
                || m_PartyFlag == PartyFlag.Member_SharedXP_Active
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlag.Member_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlag.Member_SharedXP_Off;
        }

        internal UnityEngine.Vector3Int AnimationDelta {
            get { return m_AnimationDelta; }
        }

        protected int m_MovementEnd = 0;
        protected Direction m_AnimationDirection = Direction.North;
        protected int m_AnimationEnd = 0;
        protected bool m_MovementRunning = false;
        protected UnityEngine.Vector3Int m_AnimationDelta = UnityEngine.Vector3Int.zero;
        protected UnityEngine.Vector3Int m_AnimationSpeed = UnityEngine.Vector3Int.zero;
        protected Skill[] m_Skills;

        internal static double SpeedA = 0.0f;
        internal static double SpeedB = 1.0f;
        internal static double SpeedC = 1.0f;

        internal Creature(uint id, CreatureType type = CreatureType.Monster, string name = null) {
            m_ID = id;
            m_Type = type;
            m_Name = name;

            m_Marks.onMarksChange.AddListener((marks) => { onMarksChange.Invoke(this, marks); });
            ResetSkills();
        }
        
        internal virtual void Reset() {
            ResetSkills();
            m_AnimationDelta = UnityEngine.Vector3Int.zero;
            m_AnimationDirection = Direction.North;
            m_AnimationEnd = 0;
            m_AnimationSpeed = UnityEngine.Vector3Int.zero;
            m_Brightness = 0;
            m_Direction = Direction.North;
            m_ID = 0;
            m_Trapper = false;
            m_Unpassable = false;
            m_KnownSince = -1;
            m_LightColor = UnityEngine.Color.black;
            m_MovementEnd = 0;
            m_MovementRunning = false;
            Name = null;
            m_PartyFlag = PartyFlag.None;
            m_PKFlag = PKFlag.Revenge;
            m_Type = CreatureType.Monster;
            m_Visible = false;
            m_GuildFlag = GuildFlag.None;
            m_Marks.Clear();
            m_MountOutfit = null;
            m_Outfit = null;
            m_Position = UnityEngine.Vector3Int.zero;
        }

        internal virtual void ResetSkills() {
            m_Skills = new Skill[(int)SkillType.Speed + 1];
        }

        internal virtual void SetSkill(SkillType skillType, long level, long baseLevel = 0, float percentage = 0) {
            int skill = (int)skillType;
            if (skill >= m_Skills.Length)
                return;

            m_Skills[skill].Level = level;
            m_Skills[skill].BaseLevel = baseLevel;
            m_Skills[skill].Percentage = percentage;
            onSkillChange.Invoke(this, skillType, m_Skills[skill]);
        }

        internal virtual void SetSkillValue(SkillType skillType, int level) {
            int skill = (int)skillType;
            if (skill >= m_Skills.Length)
                return;

            if (m_Skills[skill].Level != level) {
                m_Skills[skill].Level = level;
                onSkillChange.Invoke(this, skillType, m_Skills[skill]);
            }
        }

        internal long GetSkillValue(SkillType skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].Level;
        }

        internal long GetSkillbase(SkillType skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].BaseLevel;
        }

        internal double GetSKillProgress(SkillType skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].Percentage;
        }

        internal virtual void StartMovementAnimation(int deltaX, int deltaY, int movementCost) {
            if (deltaX > 0)
                m_Direction = Direction.East;
            else if (deltaX < 0)
                m_Direction = Direction.West;
            else if (deltaY < 0)
                m_Direction = Direction.North;
            else if (deltaY > 0)
                m_Direction = Direction.South;

            m_AnimationDirection = m_Direction;
            m_AnimationDelta.x = -deltaX * Constants.FieldSize;
            m_AnimationDelta.y = -deltaY * Constants.FieldSize;
            m_AnimationDelta.z = 0;
            
            int beatDuration = OpenTibiaUnity.ProtocolGame.BeatDuration;

            int movementSpeed = System.Math.Max(1, GetMovementSpeed());
            int interval = (int)System.Math.Floor(1000 * movementCost / (float)movementSpeed);

            if (OpenTibiaUnity.GameManager.ClientVersion >= 910)
                interval = (int)System.Math.Ceiling(interval / (double)beatDuration) * beatDuration;

            m_AnimationSpeed.x = m_AnimationDelta.x;
            m_AnimationSpeed.y = m_AnimationDelta.y;
            m_AnimationSpeed.z = interval;
            m_AnimationEnd = OpenTibiaUnity.TicksMillis + interval;

            float diagonalFactor = 3f;
            if (OpenTibiaUnity.GameManager.ClientVersion <= 810)
                diagonalFactor = 2f;

            if (deltaX != 0 && deltaY != 0)
                interval = (int)System.Math.Floor(1000 * movementCost * diagonalFactor / movementSpeed);

            m_MovementEnd = OpenTibiaUnity.TicksMillis + interval;
            m_MovementRunning = true;
        }

        internal void StopMovementAnimation() {
            m_AnimationDirection = m_Direction;
            m_AnimationDelta = UnityEngine.Vector3Int.zero;
            m_AnimationSpeed = UnityEngine.Vector3Int.zero;
            m_AnimationEnd = 0;
            m_MovementEnd = 0;
            m_MovementRunning = false;
        }

        internal virtual void AnimateMovement(int ticks) {
            m_AnimationDelta = UnityEngine.Vector3Int.zero;
            if (m_MovementRunning) {
                int diff = ticks - (m_AnimationEnd - m_AnimationSpeed.z);
                if (diff < 0) {
                    m_AnimationDelta.x = m_AnimationSpeed.x;
                    m_AnimationDelta.y = m_AnimationSpeed.y;
                } else if (diff >= m_AnimationSpeed.z) {
                    m_AnimationDelta.x = 0;
                    m_AnimationDelta.y = 0;
                } else if (m_AnimationSpeed.z != 0) {
                    m_AnimationDelta.x = m_AnimationSpeed.x - (int)System.Math.Round((double)m_AnimationSpeed.x * diff / m_AnimationSpeed.z);
                    m_AnimationDelta.y = m_AnimationSpeed.y - (int)System.Math.Round((double)m_AnimationSpeed.y * diff / m_AnimationSpeed.z);
                }
            }

            m_MovementRunning = ticks < m_MovementEnd || m_AnimationDelta.x != 0 || m_AnimationDelta.y != 0;
        }

        internal void AnimateOutfit(int ticks) {
            bool isIdle = !m_MovementRunning || m_MovementEnd != m_AnimationEnd && ticks >= m_AnimationEnd;
            if (!!m_Outfit && !!m_Outfit.Type) {
                m_Outfit.SwitchFrameGroup(ticks, isIdle ? (int)Protobuf.Shared.FrameGroupType.Idle : (int)Protobuf.Shared.FrameGroupType.Walking);
                m_Outfit.Animate(ticks, isIdle ? 0 : m_AnimationSpeed.z);
            }

            if (!!m_MountOutfit && !!m_MountOutfit.Type) {
                m_MountOutfit.SwitchFrameGroup(ticks, isIdle ? (int)Protobuf.Shared.FrameGroupType.Idle : (int)Protobuf.Shared.FrameGroupType.Walking);
                bool result = m_MountOutfit.Animate(ticks, isIdle ? 0 : m_AnimationSpeed.z);
            }
        }

        internal int GetMovementSpeed() {
            int speed = (int)GetSkillValue(SkillType.Speed);
            if (!OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameNewSpeedLaw))
                return speed;

            if (speed <= -SpeedB)
                return 0;

            return (int)System.Math.Round(SpeedA * System.Math.Log(speed + SpeedB) + SpeedC);
        }

        internal void SetPartyFlag(PartyFlag partyFlag) {
            if (partyFlag < PartyFlag.First || partyFlag > PartyFlag.Last)
                throw new System.ArgumentException("Creature.SetPartyFlag: Invalid party flag (" + (int)partyFlag + ").");

            PartyFlag = partyFlag;
        }

        internal void SetPKFlag(PKFlag pkFlag) {
            if (pkFlag < PKFlag.First || pkFlag > PKFlag.Last)
                throw new System.ArgumentException("Creature.SetPKFlag: Invalid pkFlag (" + (int)pkFlag + ")");

            PKFlag = pkFlag;
        }

        internal void SetSpeechCategory(SpeechCategory speechCategory) {
            if (speechCategory < SpeechCategory.First || speechCategory > SpeechCategory.Last)
                throw new System.Exception("Creature.SetSpeechCategory: Invalid speechCategory (" + (int)speechCategory + ")");

            SpeechCategory = speechCategory;
        }

        internal void SetGuildFlag(GuildFlag guildFlag) {
            if (guildFlag < GuildFlag.First || guildFlag > GuildFlag.Last)
                throw new System.ArgumentException("Creature.SetGuildFlag: Invalid guildFlag (" + (int)guildFlag + ")");

            GuildFlag = guildFlag;
        }

        internal void SetSummonerID(uint summonerID) {
            if (summonerID == OpenTibiaUnity.Player.ID)
                SummonTypeFlag = SummonTypeFlags.Own;
            else if (summonerID != 0)
                SummonTypeFlag = SummonTypeFlags.Other;
            else
                SummonTypeFlag = SummonTypeFlags.None;

            SummonerID = summonerID;
        }
        
        internal bool IsReportTypeAllowed(ReportTypes reportType) {
            return IsHuman && (reportType == ReportTypes.Name || reportType == ReportTypes.Bot);
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
