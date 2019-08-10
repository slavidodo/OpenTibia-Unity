namespace OpenTibiaUnity
{
    internal class TextResources
    {
        // Login Resources
        internal const string ACCOUNT_IDENTIFIER_EMAIL = "Email Address:";
        internal const string ACCOUNT_IDENTIFIER_ACCOUNTNAME = "Account Name:";
        internal const string ACCOUNT_IDENTIFIER_ACCOUNTNUMBER = "Account Number:";
        internal const string LOGIN_OKBUTTON_LEGACY = "Ok";
        internal const string LOGIN_OKBUTTON_NEW = "Login";
        internal const string LOGIN_AUTHBUTTON_ON = "Auth.";
        internal const string LOGIN_AUTHBUTTON_OFF = "No Auth.";
        internal const string IP_ADDRESS_LABEL = "IP Address";
        internal const string LOGIN_WEB_ADDRESS_LABEL = "Login Web Address";
        internal const string LOGIN_WEB_CLIENT_LOGIN_ERROR_TECHNICAL_V12 = "Login failed. OpenTibiaUnity might be currently down for maintenance. Please try again later.";
        internal const string LOGIN_WEB_CLIENT_LOGIN_ERROR_RESPONSE_PARSING = "Illegal value.";
        internal const string LOGIN_WEB_CLIENT_LOGIN_ERROR_NAME_RESOLUTION_FAILURE = "Host not found.";
        internal const string LOGIN_WEB_CLIENT_LOGIN_ERROR_NOT_FOUND = "Error downloading {0} - server replied: not found.";

        // Connection Resources
        internal const string ERRORMSG_AUTHENTICATION_ERROR = "You have entered incorrect authenticator token.";
        internal const string ERRORMSG_10061_LOGIN_HOSTUNREACHABLE = "Cannot connect to a login server.\n\nError(10061): Connection refused.\n\nAll login servers are offline. Check www.opentibiaunity.com for more information.\n\nFor more information take a look at the FAQS in the support section at www.opentibiaunity.com";
        internal const string ERRORMSG_10061_GAME_HOSTUNREACHABLE = "Cannot connect to the game server.\n\nError(10061): Connection refused.\n\nThe game server is offline. Check www.opentibiaunity.com for more information.\n\nFor more information take a look at the FAQS in the support section at www.opentibiaunity.com";

        // Pathfinder Resources
        internal const string MSG_PATH_GO_UPSTAIRS = "First go upstairs.";
        internal const string MSG_NPC_TOO_FAR = "You are too far away.";
        internal const string MSG_PATH_UNREACHABLE = "There is no way.";
        internal const string MSG_PATH_GO_DOWNSTAIRS = "First go downstairs.";
        internal const string MSG_PATH_TOO_FAR = "Destination is out of range.";
        internal const string MSG_SORRY_NOT_POSSIBLE = "Sorry, not possible.";
        internal const string MSG_CHANNEL_NO_ANONYMOUS = "This is not a chat channel.";

        internal const string GAME_MOVE_UNMOVEABLE = "You cannot move this object.";

        // Window titles
        internal const string WINDOWTITLE_NPCTRADE_WITH_NAME = "Trade with {0}";
        internal const string WINDOWTITLE_NPCTRADE_NO_NAME = "NPC Trade";
        
        // Context menu resources
        // ... | Object
        internal const string CTX_OBJECT_LOOK = "Look";
        internal const string CTX_OBJECT_USE = "Use";
        internal const string CTX_OBJECT_INSPECT_OBJECT = "Inspect";
        internal const string CTX_OBJECT_OPEN = "Open";
        internal const string CTX_OBJECT_OPEN_NEW_WINDOW = "Open in New Window ";
        internal const string CTX_OBJECT_UNWRAP = "Unwrap";
        internal const string CTX_OBJECT_REPORT_FIELD = "Report Coordinate";
        internal const string CTX_OBJECT_TRADE = "Trade with ...";
        internal const string CTX_OBJECT_MOVE_UP = "Move up";
        internal const string CTX_OBJECT_MULTIUSE = "Use with ...";
        internal const string CTX_OBJECT_BROWSE_FIELD = "Browse Field";
        internal const string CTX_OBJECT_TURN = "Rotate";
        internal const string CTX_OBJECT_WRAP = "Wrap";
        internal const string CTX_OBJECT_SHOW_IN_MARKET = "Show in Market";
        internal const string CTX_OBJECT_MANAGE_LOOT_CONTAINERS = "Manage Loot Containers";
        internal const string CTX_OBJECT_QUICK_LOOT = "Add to Loot List";
        internal const string CTX_OBJECT_STOW = "Stow";
        internal const string CTX_OBJECT_STOW_ALL = "Stow all items of this type";
        internal const string CTX_OBJECT_STOW_CONTENT = "Stow container's content";
        // ... | Creature
        internal const string CTX_CREATURE_TALK = "Talk";
        internal const string CTX_CREATURE_COPY_NAME = "Copy Name";
        internal const string CTX_CREATURE_FOLLOW_START = "Follow";
        internal const string CTX_CREATURE_FOLLOW_STOP = "Stop Follow";
        internal const string CTX_CREATURE_ATTACK_START = "Attack";
        internal const string CTX_CREATURE_ATTACK_STOP = "Stop Attack";
        // ... | Player
        internal const string CTX_PLAYER_MOUNT = "Mount";
        internal const string CTX_PLAYER_REPORT_NAME = "Report Name";
        internal const string CTX_PLAYER_UNIGNORE = "Unignore {0}";
        internal const string CTX_PLAYER_DISMOUNT = "Dismount";
        internal const string CTX_PLAYER_CHAT_INVITE = "Invite to Private Chat";
        internal const string CTX_PLAYER_IGNORE = "Ignore {0}";
        internal const string CTX_PLAYER_SET_OUTFIT = "Set Outfit";
        internal const string CTX_PLAYER_ADD_BUDDY = "Add {0} to VIP List";
        internal const string CTX_PLAYER_OPEN_PREY_DIALOG = "Open Prey Dialog";
        internal const string CTX_PLAYER_CHAT_MESSAGE = "Message to {0}";
        internal const string CTX_PLAYER_REPORT_BOT = "Report Bot/Macro";
        internal const string CTX_PLAYER_INSPECT_CHARACTER = "Inspect {0}";
        // ... | Party
        internal const string CTX_PARTY_EXCLUDE = "Revoke {0}'s Invitation";
        internal const string CTX_PARTY_INVITE = "Invite to Party";
        internal const string CTX_PARTY_JOIN = "Join {0}'s Party";
        internal const string CTX_PARTY_JOIN_AGGRESSION = "Join Aggression of {0}";
        internal const string CTX_PARTY_LEAVE = "Leave Party";
        internal const string CTX_PARTY_PASS_LEADERSHIP = "Pass Leadership to {0}";
        internal const string CTX_PARTY_ENABLE_SHARED_EXPERIENCE = "Enable Shared Experience";
        internal const string CTX_PARTY_DISABLE_SHARED_EXPERIENCE = "Disable Shared Experience";
        // ... | Chat
        internal const string CTX_VIEW_PRIVATE_MESSAGE = "Message to {0}";
        internal const string CTX_VIEW_ADD_BUDDY = "Add {0} to VIP list";
        internal const string CTX_VIEW_PRIVATE_INVITE = "Invite to Private Chat";
        internal const string CTX_VIEW_PRIVATE_EXCLUDE = "Exclude from Private Chat";
        internal const string CTX_VIEW_PLAYER_IGNORE = "Ignore {0}";
        internal const string CTX_VIEW_PLAYER_UNIGNORE = "Unignore {0}";
        internal const string CTX_VIEW_COPY_NAME = "Copy Name";
        internal const string CTX_VIEW_COPY_SELECTED = "Copy";
        internal const string CTX_VIEW_COPY_MESSAGE = "Copy Message";
        internal const string CTX_VIEW_SELECT_ALL = "Select All";

        // Skills resources
        internal const string SKILLS_LEVEL = "Level";
        internal const string SKILLS_EXPERIENCE = "Experience";
        internal const string SKILLS_EXPERIENCE_MINIMAL = "Exp.";
        internal const string SKILLS_XPGAIN = "Xp Gain";
        internal const string SKILLS_HITPOINTS = "Hit Points";
        internal const string SKILLS_MANA = "Mana";
        internal const string SKILLS_SOULPOINTS = "Soul";
        internal const string SKILLS_CAPACITY = "Capacity";
        internal const string SKILLS_SPEED = "Speed";
        internal const string SKILLS_REGENERATION = "Food";
        internal const string SKILLS_STAMINA = "Stamina";
        internal const string SKILLS_MAGIC = "Magic";
        internal const string SKILLS_FIST = "Fist";
        internal const string SKILLS_FIST_LEGACY = "Fist Fighting";
        internal const string SKILLS_CLUB = "Club";
        internal const string SKILLS_CLUB_LEGACY = "Club Fighting";
        internal const string SKILLS_SWORD = "Sword";
        internal const string SKILLS_SWORD_LEGACY = "Sword Fighting";
        internal const string SKILLS_AXE = "Axe";
        internal const string SKILLS_AXE_LEGACY = "Axe Fighting";
        internal const string SKILLS_DISTANCE = "Distance";
        internal const string SKILLS_DISTANCE_LEGACY = "Distance Fighting";
        internal const string SKILLS_SHIELDING = "Shielding";
        internal const string SKILLS_FISHING = "Fishing";
        internal const string SKILLS_CRITICALHIT = "Critical Hit";
        internal const string SKILLS_CRITICALHIT_CHANCE = "Chance";
        internal const string SKILLS_CRITICALHIT_EXTRADMG = "Extra Damage";
        internal const string SKILLS_LIFELEECH = "Life Leech";
        internal const string SKILLS_LIFELEECH_CHANCE = "Chance";
        internal const string SKILLS_LIFELEECH_AMOUNT = "Amount";
        internal const string SKILLS_MANALEECH = "Mana Leech";
        internal const string SKILLS_MANALEECH_CHANCE = "Chance";
        internal const string SKILLS_MANALEECH_AMOUNT = "Amount";

        // Console resources
        internal const string CHANNEL_NAME_DEFAULT = "Local Chat";
        internal const string CHANNEL_NAME_DEFAULT_LEGACY = "Default";
        internal const string CHANNEL_NAME_SERVERLOG = "Server Log";
        internal const string CHANNEL_MSG_CHANNEL_CLOSED = "The channel has been closed. You need to re-join the channel if you get invited.";
        internal const string CHANNEL_MSG_HELP_LEGACY = "Welcome to the help channel! Feel free to ask questions concerning client controls, general game play, use of accounts and the official homepage. In-depth questions about the content of the game will not be answered. Experienced players will be glad to help you to the best of their knowledge. Keep in mind that this is not a chat channel for general conversations. Therefore please limit your statements to relevant questions and answers.";
        internal const string CHANNEL_MSG_HELP = "Welcome to the help channel.\nIn this channel you can ask questions about Tibia.Experienced players will gladly help you to the best of their knowledge.\nFor detailed information about quests and other game content please take a look at our supported fansites at http://www.tibia.com/community/?subtopic=fansites\nPlease remember to write in English here so everybody can understand you.";
        internal const string CHANNEL_MSG_ADVERTISING = "Here you can advertise all kinds of things. Among others, you can trade Tibia items, advertise ingame events, seek characters for a quest or a hunting group, find members for your guild or look for somebody to help you with something.\nIt goes without saying that all advertisements must be conform to the Tibia Rules.Keep in mind that it is illegal to advertise trades including premium time, real money or Tibia characters.";

        // Outfit window resources
        internal const string OUTFIT_LABEL_INFO_LEGACY_PROTOCOL = "Choose an outfit and determine the colours of various parts of your character's body";
        internal const string OUTFIT_LABEL_INFO_NEW_PROTOCOL = "Select an outfit and determine the colours of the various parts of your character's body.\nTo activate an addon, tick the corresponding box.\nAddons are outfit accessories that can be earned in the game by premium players.";
        internal const string OUTFIT_LABEL_INFO_NEW_PROTOCOL_MOUNT = "Select an outfit by clicking the arrows below the character box. Individualise the single parts by using the colour palette. If you are premium and have earned an outfit addon, you can activate it by checking the corresponding box. Mounts earned in the game can be selected from the right-hand box.";
    }
}
