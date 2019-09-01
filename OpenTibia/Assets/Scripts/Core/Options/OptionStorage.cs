using OpenTibiaUnity.Core.Chat;
using OpenTibiaUnity.Core.Input;
using OpenTibiaUnity.Core.Input.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenTibiaUnity.Core.Options
{
    [Serializable]
    public class OptionStorage
    {
        // Options are preferred to be listed as how they are placed
        // in the actual options window (except presets, these have their own handler)
        // some values may exist in both the basic and advanced, just write them in the
        // basic area..

        /* Basic Options */
        public MousePresets MousePreset { get; set; } = MousePresets.Classic;
        public MouseLootPresets MouseLootPreset { get; set; } = MouseLootPresets.Left;
        public bool AllowAllToInspectMe = false;
        public bool AutoChaseOff = true;
        public HUDStyles HUDStyle = HUDStyles.Bars;
        public bool ShowActionBar1 = true;
        public bool ShowActionBar2 = false;
        //private CoulouriseLootValuesTypes ColouriseLootValues = CoulouriseLootValuesTypes.Frames;
        public AntialiasingModes AntialiasingMode = AntialiasingModes.Antialiasing;
        public bool FullscreenMode = false;
        
        // Advanced Options
        public bool UseDefaultKeyboardDelay { get; set; } = true;
        private int _keyboardDelay = 250;
        public int KeyboardDelay {
            get { return UseDefaultKeyboardDelay ? 250 : _keyboardDelay; }
            set { _keyboardDelay = value; }
        }
        
        public bool RotateYourCharacterUsingCtrl = true;
        public bool RotateYourCharacterUsingShift = false;
        public bool RotateYourCharacterUsingAlt = false;
        public bool PressCtrlToDragCompleteStacks = false;
        public bool HighlightMouseTarget = true;
        public bool UseNativeMouseCursor = false;
        public bool ShowAnimatedMouseCursor = true;
        public bool ShowCooldownBar = true;
        public bool ShowLinkCopyWarning = true;
        public bool ShowHUDForOwnCharacter = true;
        /* HUDStyles (basic) */
        public bool ShowNameForOwnCharacter = true;
        public bool ShowHealthForOwnCharacter= true;
        public bool ShowManaForOwnCharacter = true;
        public bool ShowMarksForOwnCharacter = true;
        public bool ShowHUDForOtherCreatures = true;
        public bool ShowNameForOtherCreatures = true;
        public bool ShowHealthForOtherCreatures = true;
        public bool ShowMarksForOtherCreatures = true;
        public bool ShowNPCIcons = true;
        public bool ShowCustomizableStatusBars = false;
        public bool ShowStatusBars = false;
        public bool ShowInfoMessages = true;
        public bool ShowEventMessages = true;
        public bool ShowStatusMessages = true;
        public bool ShowStatusMessagesOfOthers = true;
        public bool ShowTimestamps = true;
        public bool ShowLevels = true;
        public bool ShowTextualEffects = true;
        public bool ShowMessages = true;
        public bool ShowPrivateMessages = true;
        public bool ShowSpells = true;
        public bool ShowSpellsOfOthers = true;
        public bool ShowCombatFrames = true;
        public bool ShowPvpFrames = true;
        public bool ScaleUsingOnlyIntegralMultiples = false;
        /* ShowActionBar1, ShowActionBar2 (basic) */
        public bool ShowAssignedHotkeyForActionButton = true;
        public bool ShowActionAmountOfAssignedObjectsForActionButton = true;
        public bool ShowActionSpellParametersForActionButton = true;
        public bool ShowGraphicalCooldownForActionButton = true;
        public bool ShowCooldownInSecondsForActionButton = true;
        public bool ShowActionButtonTooltip = true;
        /* GraphicsEngine (implementation undetermined, as unity does this step in building actually..) */
        /* AntialiasingMode (basic) */
        /* FullScreenMode (basic) */
        public bool VsyncEnabled = false;
        public bool NoFramerateLimit = true;
        public int FramerateLimit = 50;
        public bool ShowLightEffects = true;
        public int AmbientBrightness = 25; // Default: 25
        public int LightLevelSeparator = 100; // Default: 33. If you want to play like tibia 10 (set this to 100)
        public int CloudAndIndoorEffect = 100; // Default: 100
        public bool AskBeforeBuyingProducts = true;
        public bool AskBeforeShowingContainerContent = true;
        public bool StayLoggedInForASession = true;
        public bool OptimiseConnectionStability = true;
        public bool QuickLogin = true;
        /* AllowAllToInspectMe (basic) */
        /* AutoChaseOff (basic) */
        public bool OnlyCaptureGameWindow = true;
        public bool KeepBacklogOfScreenshotsOfTheLast5Seconds = false;
        public bool EnableAutoScreenshots = true;
        public bool ScreenshotsOnLevelUp = true;
        public bool ScreenshotsOnSkillUp = true;
        public bool ScreenshotOnAchievement = true;

        public bool ShowAdvancedSettings = false;

        public bool GeneralActionBarsLock = false;
        public int GeneralInputSetId = MappingSet.DefaultSet;
        public int GeneralInputSetMode = MappingSet.ChatModeON;

        // public Client Options
        public bool AuthenticatorTokenOn = false;
        public string LoginAddress = string.Empty;
        public int SelectedClientVersion = -1;
        public int SelectedBuildVersion = -1;

        // public Game Options
        public CombatAttackModes CombatAttackMode = CombatAttackModes.Balanced;
        public CombatChaseModes CombatChaseMode = CombatChaseModes.Off;
        public bool CombatSecureMode = true;
        public CombatPvPModes CombatPvPMode = CombatPvPModes.Dove;
        public OpponentFilters OpponentFilter = OpponentFilters.None;
        public OpponentSortTypes OpponentSort = OpponentSortTypes.SortKnownSinceAsc;

        // public Storages (Mappings) (TODO: Rename Mapping to Preset)
        // These storages are not serialized with default options, as they are independant theirselves..

        //[NonSerialized] private List<int> _knownTutorialHint;
        //[NonSerialized] private List<SideBarSet> _sideBarSets;
        //[NonSerialized] private List<ActionBarSet> _actionBarSets;
        [NonSerialized] private List<MappingSet> _mappingSets;
        [NonSerialized] private List<MessageFilterSet> _messageFilterSets;
        //[NonSerialized] private List<ChannelSet> _channelSets;
        [NonSerialized] private List<NameFilterSet> _nameFilterSets;
        //[NonSerialized] private List<BuddySet> _buddySets;

        // Proper options for client versions
        public int FixedLightLevelSeparator {
            get {
                if (OpenTibiaUnity.GameManager.ClientVersion < 1100)
                    return 100;
                return LightLevelSeparator;
            }
        }

        public OptionStorage() {
            _mappingSets = new List<MappingSet>();
            _messageFilterSets = new List<MessageFilterSet>();
            _nameFilterSets = new List<NameFilterSet>();

            InitialiseMappingSet();
            InitialiseMessageFilterSets();
            InitialiseNameFilterSets();
        }

        private void InitialiseMappingSet() {
            var mappingSet = new MappingSet(MappingSet.DefaultSet);
            mappingSet.InitialiseDefaultBindings();

            _mappingSets.Clear();
            _mappingSets.Add(mappingSet);
        }

        public void InitialiseMessageFilterSets() {
            _messageFilterSets.Clear();
            _messageFilterSets.Add(new MessageFilterSet(MessageFilterSet.DefaultSet));
        }

        public void InitialiseNameFilterSets() {
            _nameFilterSets.Clear();
            _nameFilterSets.Add(new NameFilterSet(NameFilterSet.DefaultSet));
        }

        public void RemoveStarterMappings() {
            _mappingSets = _mappingSets.Where((x) => {
                return x.Name != "Knight" && x.Name != "Paladin" && x.Name != "Sorcerer" && x.Name != "Druid";
            }).ToList();
        }

        public void LoadOptions() {
            RemoveStarterMappings();
            
            LoadGeneral();
        }

        public bool SaveOptions() {
            return SaveGeneral();
        }

        public MappingSet GetMappingSet(int id) {
            return GetListItem(_mappingSets, id);
        }

        public MessageFilterSet GetMessageFilterSet(int id) {
            return GetListItem(_messageFilterSets, id);
        }

        public NameFilterSet GetNameFilterSet(int id) {
            return GetListItem(_nameFilterSets, id);
        }

        public T GetListItem<T>(List<T> list, int index) {
            if (index < 0 || index >= list.Count)
                return default;
            return list[index];
        }

        private const string GeneralFileName = "general.json";

        private bool LoadGeneral() {
            string jsonData = null;

            var path = Path.Combine(Application.persistentDataPath, GeneralFileName);
            if (!File.Exists(path))
                return false;

            FileStream stream = new FileStream(path, FileMode.Open);
            if (stream == null)
                return false;

            using (StreamReader reader = new StreamReader(stream))
                jsonData = reader.ReadToEnd();

            stream.Close();
            if (jsonData == null || jsonData.Length == 0)
                return false;

            JsonUtility.FromJsonOverwrite(jsonData, this);
            return true;
        }

        private bool SaveGeneral() {
            FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, GeneralFileName), FileMode.Create);
            if (stream == null)
                return false;

            var jsonData = JsonUtility.ToJson(this, true);
            var bytes = new UTF8Encoding(true).GetBytes(jsonData);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            return true;
        }
    }
}
