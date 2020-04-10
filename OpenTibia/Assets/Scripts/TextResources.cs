namespace OpenTibiaUnity
{
    public class TextResources
    {
        // General resources
        public const string LABEL_OK = "Ok";
        public const string LABEL_CLOSE = "Close";
        public const string LABEL_CANCEL = "Cancel";
        public const string LABEL_YES = "Yes";
        public const string LABEL_NO = "No";
        public const string LABEL_ABORT = "Abort";

        public const string LABEL_MINUTE = "{0} minute";
        public const string LABEL_MINUTES = "{0} minutes";
        public const string LABEL_SECOND = "{0} second";
        public const string LABEL_SECONDS = "{0} seconds";

        // Login resources
        public const string ACCOUNT_IDENTIFIER_EMAIL = "Email Address:";
        public const string ACCOUNT_IDENTIFIER_ACCOUNTNAME = "Account Name:";
        public const string ACCOUNT_IDENTIFIER_ACCOUNTNUMBER = "Account Number:";
        public const string LOGIN_DIALOG_TITLE_LEGACY = "Enter Game";
        public const string LOGIN_DIALOG_TITLE_V11 = "Journey Onwards";
        public const string LOGIN_OKBUTTON_LEGACY = "Ok";
        public const string LOGIN_OKBUTTON_NEW = "Login";
        public const string LOGIN_AUTHBUTTON_ON = "Auth.";
        public const string LOGIN_AUTHBUTTON_OFF = "No Auth.";
        public const string IP_ADDRESS_LABEL = "IP Address";
        public const string LOGIN_WEB_ADDRESS_LABEL = "Login Web Address";
        public const string LOGIN_WEB_CLIENT_LOGIN_ERROR_TECHNICAL_V12 = "Login failed. OpenTibiaUnity might be currently down for maintenance. Please try again later.";
        public const string LOGIN_WEB_CLIENT_LOGIN_ERROR_RESPONSE_PARSING = "Illegal value.";
        public const string LOGIN_WEB_CLIENT_LOGIN_ERROR_NAME_RESOLUTION_FAILURE = "Host not found.";
        public const string LOGIN_WEB_CLIENT_LOGIN_ERROR_NOT_FOUND = "Error downloading {0} - server replied: not found.";
        public const string LOGIN_WAIT_TEXT = "{0}\n\nTrying to reconnect in {1}.";

        // Connection resources
        public const string ERRORMSG_AUTHENTICATION_ERROR = "You have entered incorrect authenticator token.";
        public const string ERRORMSG_HEADER_LOGIN = "Cannot connect to a login server.\n\n";
        public const string ERRORMSG_HEADER_GAME = "Cannot connect to the game server.\n\n";
        public const string ERRORMSG_FOOTER = "\n\nFor more information take a look at the FAQS in the support section at www.opentibiaunity.com";
        public const string CONNECTION_LOST_MESSAGE = "Connection lost.";
        public const string CONNECTION_LOST_TEXT = "{0}\n\nWaiting another {1} before closing.";

        // Pathfinder resources
        public const string MSG_PATH_GO_UPSTAIRS = "First go upstairs.";
        public const string MSG_NPC_TOO_FAR = "You are too far away.";
        public const string MSG_PATH_UNREACHABLE = "There is no way.";
        public const string MSG_PATH_GO_DOWNSTAIRS = "First go downstairs.";
        public const string MSG_PATH_TOO_FAR = "Destination is out of range.";
        public const string MSG_SORRY_NOT_POSSIBLE = "Sorry, not possible.";
        public const string MSG_CHANNEL_NO_ANONYMOUS = "This is not a chat channel.";

        public const string GAME_MOVE_UNMOVEABLE = "You cannot move this object.";

        // Window titles
        public const string WINDOWTITLE_NPCTRADE_WITH_NAME = "Trade with {0}";
        public const string WINDOWTITLE_NPCTRADE_NO_NAME = "NPC Trade";
        
        // Context menu resources
        // ... | Object
        public const string CTX_OBJECT_LOOK = "Look";
        public const string CTX_OBJECT_USE = "Use";
        public const string CTX_OBJECT_INSPECT_OBJECT = "Inspect";
        public const string CTX_OBJECT_OPEN = "Open";
        public const string CTX_OBJECT_OPEN_NEW_WINDOW = "Open in New Window ";
        public const string CTX_OBJECT_UNWRAP = "Unwrap";
        public const string CTX_OBJECT_REPORT_FIELD = "Report Coordinate";
        public const string CTX_OBJECT_TRADE = "Trade with ...";
        public const string CTX_OBJECT_MOVE_UP = "Move up";
        public const string CTX_OBJECT_MULTIUSE = "Use with ...";
        public const string CTX_OBJECT_BROWSE_FIELD = "Browse Field";
        public const string CTX_OBJECT_TURN = "Rotate";
        public const string CTX_OBJECT_WRAP = "Wrap";
        public const string CTX_OBJECT_SHOW_IN_MARKET = "Show in Market";
        public const string CTX_OBJECT_MANAGE_LOOT_CONTAINERS = "Manage Loot Containers";
        public const string CTX_OBJECT_QUICK_LOOT = "Add to Loot List";
        public const string CTX_OBJECT_STOW = "Stow";
        public const string CTX_OBJECT_STOW_ALL = "Stow all items of this type";
        public const string CTX_OBJECT_STOW_CONTENT = "Stow container's content";
        // ... | Creature
        public const string CTX_CREATURE_TALK = "Talk";
        public const string CTX_CREATURE_COPY_NAME = "Copy Name";
        public const string CTX_CREATURE_FOLLOW_START = "Follow";
        public const string CTX_CREATURE_FOLLOW_STOP = "Stop Follow";
        public const string CTX_CREATURE_ATTACK_START = "Attack";
        public const string CTX_CREATURE_ATTACK_STOP = "Stop Attack";
        // ... | Player
        public const string CTX_PLAYER_MOUNT = "Mount";
        public const string CTX_PLAYER_REPORT_NAME = "Report Name";
        public const string CTX_PLAYER_UNIGNORE = "Unignore {0}";
        public const string CTX_PLAYER_DISMOUNT = "Dismount";
        public const string CTX_PLAYER_CHAT_INVITE = "Invite to Private Chat";
        public const string CTX_PLAYER_IGNORE = "Ignore {0}";
        public const string CTX_PLAYER_SET_OUTFIT = "Set Outfit";
        public const string CTX_PLAYER_ADD_BUDDY = "Add {0} to VIP List";
        public const string CTX_PLAYER_OPEN_PREY_DIALOG = "Open Prey Dialog";
        public const string CTX_PLAYER_CHAT_MESSAGE = "Message to {0}";
        public const string CTX_PLAYER_REPORT_BOT = "Report Bot/Macro";
        public const string CTX_PLAYER_INSPECT_CHARACTER = "Inspect {0}";
        // ... | Party
        public const string CTX_PARTY_EXCLUDE = "Revoke {0}'s Invitation";
        public const string CTX_PARTY_INVITE = "Invite to Party";
        public const string CTX_PARTY_JOIN = "Join {0}'s Party";
        public const string CTX_PARTY_JOIN_AGGRESSION = "Join Aggression of {0}";
        public const string CTX_PARTY_LEAVE = "Leave Party";
        public const string CTX_PARTY_PASS_LEADERSHIP = "Pass Leadership to {0}";
        public const string CTX_PARTY_ENABLE_SHARED_EXPERIENCE = "Enable Shared Experience";
        public const string CTX_PARTY_DISABLE_SHARED_EXPERIENCE = "Disable Shared Experience";
        // ... | Chat
        public const string CTX_VIEW_PRIVATE_MESSAGE = "Message to {0}";
        public const string CTX_VIEW_ADD_BUDDY = "Add {0} to VIP list";
        public const string CTX_VIEW_PRIVATE_INVITE = "Invite to Private Chat";
        public const string CTX_VIEW_PRIVATE_EXCLUDE = "Exclude from Private Chat";
        public const string CTX_VIEW_PLAYER_IGNORE = "Ignore {0}";
        public const string CTX_VIEW_PLAYER_UNIGNORE = "Unignore {0}";
        public const string CTX_VIEW_COPY_NAME = "Copy Name";
        public const string CTX_VIEW_COPY_SELECTED = "Copy";
        public const string CTX_VIEW_COPY_MESSAGE = "Copy Message";
        public const string CTX_VIEW_SELECT_ALL = "Select All";

        // Skills resources
        public const string SKILLS_LEVEL = "Level";
        public const string SKILLS_EXPERIENCE = "Experience";
        public const string SKILLS_EXPERIENCE_MINIMAL = "Exp.";
        public const string SKILLS_XPGAIN = "Xp Gain";
        public const string SKILLS_HITPOINTS = "Hit Points";
        public const string SKILLS_MANA = "Mana";
        public const string SKILLS_SOULPOINTS = "Soul";
        public const string SKILLS_CAPACITY = "Capacity";
        public const string SKILLS_SPEED = "Speed";
        public const string SKILLS_REGENERATION = "Food";
        public const string SKILLS_STAMINA = "Stamina";
        public const string SKILLS_OFFLINETRAINING = "Offline Training";
        public const string SKILLS_MAGIC = "Magic";
        public const string SKILLS_FIST = "Fist";
        public const string SKILLS_FIST_LEGACY = "Fist Fighting";
        public const string SKILLS_CLUB = "Club";
        public const string SKILLS_CLUB_LEGACY = "Club Fighting";
        public const string SKILLS_SWORD = "Sword";
        public const string SKILLS_SWORD_LEGACY = "Sword Fighting";
        public const string SKILLS_AXE = "Axe";
        public const string SKILLS_AXE_LEGACY = "Axe Fighting";
        public const string SKILLS_DISTANCE = "Distance";
        public const string SKILLS_DISTANCE_LEGACY = "Distance Fighting";
        public const string SKILLS_SHIELDING = "Shielding";
        public const string SKILLS_FISHING = "Fishing";
        public const string SKILLS_CRITICALHIT = "Critical Hit";
        public const string SKILLS_CRITICALHIT_CHANCE = "Chance";
        public const string SKILLS_CRITICALHIT_EXTRADMG = "Extra Damage";
        public const string SKILLS_LIFELEECH = "Life Leech";
        public const string SKILLS_LIFELEECH_CHANCE = "Chance";
        public const string SKILLS_LIFELEECH_AMOUNT = "Amount";
        public const string SKILLS_MANALEECH = "Mana Leech";
        public const string SKILLS_MANALEECH_CHANCE = "Chance";
        public const string SKILLS_MANALEECH_AMOUNT = "Amount";

        // Console resources
        public const string CHANNEL_NAME_DEFAULT = "Local Chat";
        public const string CHANNEL_NAME_DEFAULT_LEGACY = "Default";
        public const string CHANNEL_NAME_SERVERLOG = "Server Log";
        public const string CHANNEL_MSG_CHANNEL_CLOSED = "The channel has been closed. You need to re-join the channel if you get invited.";
        public const string CHANNEL_MSG_HELP_LEGACY = "Welcome to the help channel! Feel free to ask questions concerning client controls, general game play, use of accounts and the official homepage. In-depth questions about the content of the game will not be answered. Experienced players will be glad to help you to the best of their knowledge. Keep in mind that this is not a chat channel for general conversations. Therefore please limit your statements to relevant questions and answers.";
        public const string CHANNEL_MSG_HELP = "Welcome to the help channel.\nIn this channel you can ask questions about Tibia.Experienced players will gladly help you to the best of their knowledge.\nFor detailed information about quests and other game content please take a look at our supported fansites at http://www.tibia.com/community/?subtopic=fansites\nPlease remember to write in English here so everybody can understand you.";
        public const string CHANNEL_MSG_ADVERTISING = "Here you can advertise all kinds of things. Among others, you can trade Tibia items, advertise ingame events, seek characters for a quest or a hunting group, find members for your guild or look for somebody to help you with something.\nIt goes without saying that all advertisements must be conform to the Tibia Rules.Keep in mind that it is illegal to advertise trades including premium time, real money or Tibia characters.";

        // Exit window resources
        public const string EXIT_WINDOW_TITLE = "Warning";
        public const string EXIT_WINDOW_MESSAGE = "If you shut down the program, your character might stay in the game.\nClick on \"logout\" to ensure that your character leaves the game properly.\nClick on \"Exit\" if you want to exit the program without logging out your character.";

        // Outfit window resources
        public const string OUTFIT_LABEL_INFO_LEGACY_PROTOCOL = "Choose an outfit and determine the colours of various parts of your character's body";
        public const string OUTFIT_LABEL_INFO_NEW_PROTOCOL = "Select an outfit and determine the colours of the various parts of your character's body.\nTo activate an addon, tick the corresponding box.\nAddons are outfit accessories that can be earned in the game by premium players.";
        public const string OUTFIT_LABEL_INFO_NEW_PROTOCOL_MOUNT = "Select an outfit by clicking the arrows below the character box. Individualise the single parts by using the colour palette. If you are premium and have earned an outfit addon, you can activate it by checking the corresponding box. Mounts earned in the game can be selected from the right-hand box.";

        // LegacyOptions window resources
        public const string LEGACYOPTIONS_WINDOW_GENERAL_TEXT = "General";
        public const string LEGACYOPTIONS_WINDOW_GENERAL_DESCRIPTION = "Change general\ngame options";
        public const string LEGACYOPTIONS_WINDOW_GRAPHICS_TEXT = "Graphics";
        public const string LEGACYOPTIONS_WINDOW_GRAPHICS_DESCRIPTION = "Change graphics and performance settings";
        public const string LEGACYOPTIONS_WINDOW_CONSOLE_TEXT = "Console";
        public const string LEGACYOPTIONS_WINDOW_CONSOLE_DESCRIPTION = "Customise the console";
        public const string LEGACYOPTIONS_WINDOW_HOTKEYS_TEXT = "Hotkeys";
        public const string LEGACYOPTIONS_WINDOW_HOTKEYS_DESCRIPTION = "Edit your hotkey texts";
        public const string LEGACYOPTIONS_WINDOW_MOTD_TEXT = "MOTD";
        public const string LEGACYOPTIONS_WINDOW_MOTD_DESCRIPTION = "Show the most recent message of the day.";
        public const string LEGACYOPTIONS_WINDOW_GETPREMIUM_TEXT = "Get Premium";
        public const string LEGACYOPTIONS_WINDOW_GETPREMIUM_DESCRIPTION = "Gain access to all premium features.";

        public const string LEGACYOPTIONS_GENERAL_TIBIA_CLASSIC_CONTROLS = "Tibia Classic Control";
        public const string LEGACYOPTIONS_GENERAL_AUTO_CHASE_OFF = "Auto Chase Off";
        public const string LEGACYOPTIONS_GENERAL_SHOWHINTS = "Show Hints";
        public const string LEGACYOPTIONS_GENERAL_SHOW_NAMES_OF_CREATURES = "Show Names of Creatures";
        public const string LEGACYOPTIONS_GENERAL_SHOW_MARKS_ON_CREATURES = "Show Marks on Creatures";
        public const string LEGACYOPTIONS_GENERAL_SHOW_PVP_FRAMES_ON_CREATURES = "Show PvP Frames on Creatures";
        public const string LEGACYOPTIONS_GENERAL_SHOW_ICONS_ON_NPCS = "Show Icons on NPCs";
        public const string LEGACYOPTIONS_GENERAL_SHOW_TEXTUAL_EFFECTS = "Show Textual Effects";
        public const string LEGACYOPTIONS_GENERAL_SHOW_COOLDOWN_BAR = "Show Cooldown Bar";
        public const string LEGACYOPTIONS_GENERAL_AUTO_SWITCH_HOTKEY_PRESET = "Auto-Switch Hotkey Preset";

        public const string LEGACYOPTIONS_CONSOLE_SHOW_INFO_MESSAGES = "Show Info Messages in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_EVENT_MESSAGES = "Show Event Messages in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_STATUS_MESSAGES = "Show Status Messages in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_STATUS_MESSAGES_OF_OTHERS = "Show Status Messages of Others in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_TIMESTAMP = "Show Timestamp in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_LEVELS = "Show Levels in Console";
        public const string LEGACYOPTIONS_CONSOLE_SHOW_PRIVATE_MESSAGES = "Show Levels in Console";

        // Hotkeys dialog
        public const string HOTKEYS_DLG_USE_OBJECT_YOURSELF = "<color=#AFFEAF>{0}: (use object on yourself)</color>";
        public const string HOTKEYS_DLG_USE_OBJECT_TARGET = "<color=#FEAFAF>{0}: (use object on target)</color>";
        public const string HOTKEYS_DLG_USE_OBJECT_CROSSHAIRS = "<color=#C37A7A>{0}: (use object with crosshairs)</color>";
        public const string HOTKEYS_DLG_USE_OBJECT_AUTO = "<color=#AFAFFE>{0}: (use object)</color>";
    }
}
