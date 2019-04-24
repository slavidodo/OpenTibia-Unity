using UnityEngine.Events;

namespace OpenTibiaUnity.Core.Creatures
{
    public struct SkillStruct
    {
        public int level;
        public int baseLevel;
        public int percentage;
    }

    public class Creature
    {
        public class StringCreatureChangeEvent : UnityEvent<Creature, string, string> {}
        public class UIntCreatureChangeEvent : UnityEvent<Creature, uint, uint> {}
        public class IntCreatureChangeEvent : UnityEvent<Creature, int, int> {}
        public class IntCreatureChangeEvent2 : UnityEvent<Creature, int> {}
        public class DirectionCreatureChangeEvent : UnityEvent<Creature, Directions, Directions> {}
        public class BoolCreatureChangeEvent : UnityEvent<Creature, bool> {};
        public class SkillsCreatureChangeEvent : UnityEvent<Creature, SkillTypes, SkillStruct> {};
        public class LightCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Color> {};
        public class PositionCreatureChangeEvent : UnityEvent<Creature, UnityEngine.Vector3Int, UnityEngine.Vector3Int> {};
        public class MarksCreatureChangeEvent : UnityEvent<Creature, Appearances.Marks> {};
        public class OutfitCreatureChangeEvent : UnityEvent<Creature, Appearances.AppearanceInstance> {};

        public class CreatureTypesCreatureChangeEvent : UnityEvent<Creature, CreatureTypes, CreatureTypes> {};
        public class PartyFlagsCreatureChangeEvent : UnityEvent<Creature, PartyFlags, PartyFlags> {};
        public class PKFlagsCreatureChangeEvent : UnityEvent<Creature, PKFlags, PKFlags> {};
        public class GuildFlagsCreatureChangeEvent : UnityEvent<Creature, GuildFlags, GuildFlags> {};
        public class SpeechCategoryCreatureChangeEvent : UnityEvent<Creature, SpeechCategories, SpeechCategories> {};

        public static UIntCreatureChangeEvent onIDChange = new UIntCreatureChangeEvent();
        public static StringCreatureChangeEvent onNameChange = new StringCreatureChangeEvent();
        public static CreatureTypesCreatureChangeEvent onTypeChange = new CreatureTypesCreatureChangeEvent();
        public static UIntCreatureChangeEvent onSummonerChange = new UIntCreatureChangeEvent();
        public static BoolCreatureChangeEvent onUnpassableChange = new BoolCreatureChangeEvent();
        public static DirectionCreatureChangeEvent onDirectionChange = new DirectionCreatureChangeEvent();
        public static PartyFlagsCreatureChangeEvent onPartyFlagChange = new PartyFlagsCreatureChangeEvent();
        public static PKFlagsCreatureChangeEvent onPKFlagChange = new PKFlagsCreatureChangeEvent();
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

        protected uint m_ID = 0;
        public uint ID {
            get { return m_ID; }
            set { if (m_ID != value) { var old = m_ID;  m_ID = value; onIDChange.Invoke(this, m_ID, old); } }
        }

        protected string m_Name;
        public string Name {
            get { return m_Name; }
            set { var old = m_Name; m_Name = value; onNameChange.Invoke(this, m_Name, old); }
        }

        protected CreatureTypes m_Type = 0;
        public CreatureTypes Type {
            get { return m_Type; }
            set { if (m_Type != value) { var old = m_Type;  m_Type = value; onTypeChange.Invoke(this, m_Type, old); } }
        }

        protected uint m_SummonerID = 0;
        public uint SummonerID {
            get { return m_SummonerID; }
            set { if (m_SummonerID != value) { var old = m_SummonerID;  m_SummonerID = value; onSummonerChange.Invoke(this, m_SummonerID, old); } }
        }

        protected bool m_Trapper = false;
        public bool Trapper {
            get { return m_Trapper; }
            set { if (m_Trapper != value) { m_Trapper = value; } }
        }

        protected bool m_Unpassable = false;
        public bool Unpassable {
            get { return m_Unpassable; }
            set { if (m_Unpassable != value) { m_Unpassable = value; onUnpassableChange.Invoke(this, m_Unpassable); } }
        }

        protected Directions m_Direction = Directions.North;
        public Directions Direction {
            get { return m_MovementRunning ? m_AnimationDirection : m_Direction; }
            set { if (m_Direction != value) { var old = m_Direction; m_Direction = value; onDirectionChange.Invoke(this, m_Direction, old); } }
        }

        protected PartyFlags m_PartyFlag = PartyFlags.None;
        public PartyFlags PartyFlag {
            get { return m_PartyFlag; }
            set { if (m_PartyFlag != value) { var old = m_PartyFlag; m_PartyFlag = value; onPartyFlagChange.Invoke(this, m_PartyFlag, old); } }
        }

        protected PKFlags m_PKFlag = PKFlags.None;
        public PKFlags PKFlag {
            get { return m_PKFlag; }
            set { if (m_PKFlag != value) { var old = m_PKFlag; m_PKFlag = value; onPKFlagChange.Invoke(this, m_PKFlag, old); } }
        }

        protected SummonTypeFlags m_SummonTypeFlag = SummonTypeFlags.None;
        public SummonTypeFlags SummonTypeFlag {
            get { return m_SummonTypeFlag; }
            set { if (m_SummonTypeFlag != value) { m_SummonTypeFlag = value; } }
        }

        protected SpeechCategories m_SpeechCategory = SpeechCategories.None;
        public SpeechCategories SpeechCategory {
            get { return m_SpeechCategory; }
            set { if (m_SpeechCategory != value) { var old = m_SpeechCategory;  m_SpeechCategory = value; onSpeechCategoryChange.Invoke(this, m_SpeechCategory, old); } }
        }

        protected GuildFlags m_GuildFlag = GuildFlags.None;
        public GuildFlags GuildFlag {
            get { return m_GuildFlag; }
            set { if (m_GuildFlag != value) { var old = m_GuildFlag; m_GuildFlag = value; onGuildFlagChange.Invoke(this, m_GuildFlag, old); } }
        }

        protected Appearances.AppearanceInstance m_Outfit = null;
        public Appearances.AppearanceInstance Outfit {
            get { return m_Outfit; }
            set { if (m_Outfit != value) { m_Outfit = value; onOutfitChange.Invoke(this, m_Outfit); } }
        }

        protected Appearances.AppearanceInstance m_MountOutfit = null;
        public Appearances.AppearanceInstance MountOutfit {
            get { return m_MountOutfit; }
            set { if (m_MountOutfit != value) { m_MountOutfit = value; onMountOutfitChange.Invoke(this, m_MountOutfit); } }
        }

        protected int m_Brightness = 0;
        public int Brightness {
            get { return m_Brightness; }
            set { if (m_Brightness != value) { m_Brightness = value; onBrightnessChange.Invoke(this, m_Brightness); } }
        }

        protected UnityEngine.Color m_LightColor = UnityEngine.Color.black;
        public UnityEngine.Color LightColor {
            get { return m_LightColor; }
            set { if (m_LightColor != value) { m_LightColor = value; onLightColorChange.Invoke(this, m_LightColor); } }
        }
        
        protected int m_NumberOfPvPHelpers = 0;
        public int NumberOfPvPHelpers {
            get { return m_NumberOfPvPHelpers; }
            set { if (m_NumberOfPvPHelpers != value) { m_NumberOfPvPHelpers = value; onNumberOfPvPHelpersChange.Invoke(this, m_NumberOfPvPHelpers); } }
        }

        protected UnityEngine.Vector3Int m_Position = UnityEngine.Vector3Int.zero;
        public UnityEngine.Vector3Int Position {
            get { return m_Position; }
            set { if (m_Position != value) { var old = m_Position; m_Position = value; onPositionChange.Invoke(this, m_Position, old); } }
        }

        protected Appearances.Marks m_Marks = new Appearances.Marks();
        public Appearances.Marks Marks {
            get { return m_Marks; }
            set { if (m_Marks != value) { m_Marks = value;} }
        }

        protected bool m_Visible = false;
        public bool Visible {
            get { return m_Visible; }
            set { if (m_Visible != value) { m_Visible = value; onVisbilityChange.Invoke(this, m_Visible); } }
        }

        protected int m_KnownSince = -1;
        public int KnownSince {
            get { return m_KnownSince; }
            set { if (m_KnownSince != value) { m_KnownSince = value; } }
        }

        public virtual int HealthPercent {
            get {
                return GetSkillValue(SkillTypes.HealthPercent);
            }
        }

        public virtual int ManaPercent {
            get { return 100; }
        }

        public bool IsHuman {
            get { return m_Type == CreatureTypes.Player; }
        }
        public bool IsMonster {
            get { return m_Type == CreatureTypes.Monster; }
        }
        public bool IsNPC {
            get { return m_Type == CreatureTypes.NPC; }
        }
        public bool IsSummon {
            get { return m_Type == CreatureTypes.Summon; }
        }
        public bool IsConfirmedPartyMember {
            get => m_PartyFlag == PartyFlags.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Off
                || m_PartyFlag == PartyFlags.Member_SharedXP_Active
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlags.Member_SharedXP_Off;
        }
        public bool IsPartySharedExperienceActive {
            get => m_PartyFlag == PartyFlags.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlags.Member_SharedXP_Active
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Innocent;
        }
        public bool IsPartyLeader {
            get => m_PartyFlag == PartyFlags.Leader
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Active
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlags.Leader_SharedXP_Off;
        }
        public bool IsPartyMember {
            get => m_PartyFlag == PartyFlags.Member
                || m_PartyFlag == PartyFlags.Member_SharedXP_Active
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Guilty
                || m_PartyFlag == PartyFlags.Member_SharedXP_Inactive_Innocent
                || m_PartyFlag == PartyFlags.Member_SharedXP_Off;
        }

        public UnityEngine.Vector3Int AnimationDelta {
            get { return m_AnimationDelta; }
        }

        protected int m_MovementEnd = 0;
        protected Directions m_AnimationDirection = Directions.North;
        protected int m_AnimationEnd = 0;
        protected bool m_MovementRunning = false;
        protected UnityEngine.Vector3Int m_AnimationDelta = UnityEngine.Vector3Int.zero;
        protected UnityEngine.Vector3Int m_AnimationSpeed = UnityEngine.Vector3Int.zero;
        protected SkillStruct[] m_Skills;

        public static double SpeedA = 0.0f;
        public static double SpeedB = 1.0f;
        public static double SpeedC = 1.0f;

        public Creature(uint id, CreatureTypes type = CreatureTypes.Monster, string name = null) {
            m_ID = id;
            m_Type = type;
            m_Name = name;

            m_Marks.onMarksChange.AddListener((marks) => { onMarksChange.Invoke(this, marks); });
            ResetSkills();
        }
        
        public virtual void Reset() {
            ResetSkills();
            m_AnimationDelta = UnityEngine.Vector3Int.zero;
            m_AnimationDirection = Directions.North;
            m_AnimationEnd = 0;
            m_AnimationSpeed = UnityEngine.Vector3Int.zero;
            m_Brightness = 0;
            m_Direction = Directions.North;
            m_ID = 0;
            m_Trapper = false;
            m_Unpassable = false;
            m_KnownSince = -1;
            m_LightColor = UnityEngine.Color.black;
            m_MovementEnd = 0;
            m_MovementRunning = false;
            Name = null;
            m_PartyFlag = PartyFlags.None;
            m_PKFlag = PKFlags.Revenge;
            m_Type = CreatureTypes.Monster;
            m_Visible = false;
            m_GuildFlag = GuildFlags.None;
            m_Marks.Clear();
            m_MountOutfit = null;
            m_Outfit = null;
            m_Position = UnityEngine.Vector3Int.zero;
        }

        public virtual void ResetSkills() {
            m_Skills = new SkillStruct[(int)SkillTypes.Speed + 1];
        }

        public virtual void SetSkill(SkillTypes skillType, int level, int baseLevel = 0, int percentage = 0) {
            int skill = (int)skillType;
            if (skill >= m_Skills.Length)
                return;

            m_Skills[skill].level = level;
            m_Skills[skill].baseLevel = baseLevel;
            m_Skills[skill].percentage = percentage;
            onSkillChange.Invoke(this, skillType, m_Skills[skill]);
        }

        public virtual void SetSkillValue(SkillTypes skillType, int level) {
            int skill = (int)skillType;
            if (skill >= m_Skills.Length)
                return;

            if (m_Skills[skill].level != level) {
                m_Skills[skill].level = level;
                onSkillChange.Invoke(this, skillType, m_Skills[skill]);
            }
        }

        public int GetSkillValue(SkillTypes skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].level;
        }

        public int GetSkillbase(SkillTypes skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].baseLevel;
        }

        public int GetSKillProgress(SkillTypes skill) {
            if ((int)skill >= m_Skills.Length)
                return 0;

            return m_Skills[(int)skill].percentage;
        }

        public virtual void StartMovementAnimation(int deltaX, int deltaY, int movementCost) {
            if (deltaX > 0) {
                m_Direction = Directions.East;
            } else if (deltaX < 0) {
                m_Direction = Directions.West;
            } else if (deltaY < 0) {
                m_Direction = Directions.North;
            } else if (deltaY > 0) {
                m_Direction = Directions.South;
            }

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
                interval = (int)System.Math.Floor(1000 * movementCost * diagonalFactor / (float)movementSpeed);

            m_MovementEnd = OpenTibiaUnity.TicksMillis + interval;
            m_MovementRunning = true;
        }

        public void StopMovementAnimation() {
            m_AnimationDirection = m_Direction;
            m_AnimationDelta = UnityEngine.Vector3Int.zero;
            m_AnimationSpeed = UnityEngine.Vector3Int.zero;
            m_AnimationEnd = 0;
            m_MovementEnd = 0;
            m_MovementRunning = false;
        }

        public virtual void AnimateMovement(int ticks) {
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

        public void AnimateOutfit(int ticks) {
            bool isIdle = !m_MovementRunning || m_MovementEnd != m_AnimationEnd && ticks >= m_AnimationEnd;
            if (!!m_Outfit && !!m_Outfit.Type) {
                m_Outfit.SwitchFrameGroup(ticks, isIdle ? (int)Proto.Appearances.FrameGroupType.Idle : (int)Proto.Appearances.FrameGroupType.Walking);
                m_Outfit.Animate(ticks, isIdle ? 0 : m_AnimationSpeed.z);
            }

            if (!!m_MountOutfit && !!m_MountOutfit.Type) {
                m_MountOutfit.SwitchFrameGroup(ticks, isIdle ? (int)Proto.Appearances.FrameGroupType.Idle : (int)Proto.Appearances.FrameGroupType.Walking);
                m_MountOutfit.Animate(ticks, isIdle ? 0 : m_AnimationSpeed.z);
            }
        }

        public int GetMovementSpeed() {
            int speed = GetSkillValue(SkillTypes.Speed);
            if (!OpenTibiaUnity.GameManager.GetFeature(GameFeatures.GameNewSpeedLaw))
                return speed;

            if (speed <= -SpeedB)
                return 0;

            return (int)System.Math.Round(SpeedA * System.Math.Log(speed + SpeedB) + SpeedC);
        }

        public void SetPartyFlag(PartyFlags partyFlag) {
            if (partyFlag < PartyFlags.First || partyFlag > PartyFlags.Last)
                throw new System.ArgumentException("Creature.SetPartyFlag: Invalid party flag (" + (int)partyFlag + ").");

            PartyFlag = partyFlag;
        }

        public void SetPKFlag(PKFlags pkFlag) {
            if (pkFlag < PKFlags.First || pkFlag > PKFlags.Last)
                throw new System.ArgumentException("Creature.SetPKFlag: Invalid pkFlag (" + (int)pkFlag + ")");

            PKFlag = pkFlag;
        }

        public void SetSpeechCategory(SpeechCategories speechCategory) {
            if (speechCategory < SpeechCategories.First || speechCategory > SpeechCategories.Last)
                throw new System.Exception("Creature.SetSpeechCategory: Invalid speechCategory (" + (int)speechCategory + ")");

            SpeechCategory = speechCategory;
        }

        public void SetGuildFlag(GuildFlags guildFlag) {
            if (guildFlag < GuildFlags.First || guildFlag > GuildFlags.Last)
                throw new System.ArgumentException("Creature.SetGuildFlag: Invalid guildFlag (" + (int)guildFlag + ")");

            GuildFlag = guildFlag;
        }

        public void SetSummonerID(uint summonerID) {
            if (summonerID == OpenTibiaUnity.Player.ID)
                SummonTypeFlag = SummonTypeFlags.Own;
            else if (summonerID != 0)
                SummonTypeFlag = SummonTypeFlags.Other;
            else
                SummonTypeFlag = SummonTypeFlags.None;

            SummonerID = summonerID;
        }
        
        public bool IsReportTypeAllowed(ReportTypes reportType) {
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
