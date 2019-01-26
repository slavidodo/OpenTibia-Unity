
namespace OpenTibiaUnity.Core.Network
{
    public sealed class ClientLoginOpCodes {
        public const byte EnterAccount = 1;
        public const byte GamePending = 2;
        public const byte EnterGame = 15;
        public const byte LeaveGame = 20;
        public const byte Ping = 29;
        public const byte PingBack = 30;
    }

    public sealed class ClientServerOpCodes
    {
        public const byte EnterAccount = 1;
        public const byte PendingGame = 10;
        public const byte EnterGame = 15;
        public const byte LeaveGame = 20;
        public const byte Ping = 29;
        public const byte PingBack = 30;

        public const byte GoPath = 100;
        public const byte GoNorth = 101;
        public const byte GoEast = 102;
        public const byte GoSouth = 103;
        public const byte GoWest = 104;
        public const byte Stop = 105;
        public const byte GoNorthEast = 106;
        public const byte GoSouthEast = 107;
        public const byte GoSouthWest = 108;
        public const byte GoNorthWest = 109;
        public const byte TurnNorth = 111;
        public const byte TurnEast = 112;
        public const byte TurnSouth = 113;
        public const byte TurnWest = 114;
        
        public const byte UseObject = 130;
        public const byte UseTwoObject = 131;
        public const byte UseOnCreature = 132;

        public const byte Look = 140;
        public const byte LookAtCreature = 141;

        public const byte Talk = 150;
        public const byte GetChannels = 151;
        public const byte JoinChannel = 152;
        public const byte LeaveChannel = 153;
        public const byte CloseNPCChannel = 158;

        public const byte SetTactics = 160;
        public const byte Attack = 161;
        public const byte Follow = 162;

        public const byte InviteToChannel = 171;
        public const byte ExcludeFromChannel = 171;

        public const byte Cancel = 190;

        public const byte Mount = 212;
    }

    public sealed class LoginServerOpCodes
    {
        public const byte LoginRetry = 10;
        public const byte LoginError = 11;
        public const byte LoginTokenSuccess = 12;
        public const byte LoginTokenError = 13;
        public const byte LoginMotd = 20;
        public const byte UpdateRequired = 30;
        public const byte LoginSessionKey = 40;
        public const byte LoginCharacterList = 100;
    }

    public sealed class GameServerOpCodes
    {
        public const byte LoginOrPendingState					= 10;
        public const byte GMActions								= 11;
        public const byte WorldEntered					    	= 15;
        public const byte UpdateNeeded							= 17;
        public const byte LoginError							= 20;
        public const byte LoginAdvice							= 21;
        public const byte LoginWait								= 22;
        public const byte LoginSuccess							= 23;
        public const byte LoginToken							= 24;
        public const byte StoreButtonIndicators					= 25;
        public const byte PingBack								= 29;
        public const byte Ping									= 30;
        public const byte Challenge								= 31;
        public const byte Death									= 40;
        public const byte OTClientOpcode                        = 50;

        public const byte FullMap                               = 100;
        public const byte MapTopRow								= 101;
        public const byte MapRightRow							= 102;
        public const byte MapBottomRow							= 103;
        public const byte MapLeftRow                            = 104;
        public const byte FieldData                             = 105; // UpdateTile
        public const byte CreateOnMap                           = 106;
        public const byte ChangeOnMap                           = 107;
        public const byte DeleteOnMap                           = 108;
        public const byte MoveCreature                          = 109;
        public const byte OpenContainer                         = 110;
        public const byte CloseContainer                        = 111;
        public const byte CreateContainer                       = 112;
        public const byte ChangeInContainer                     = 113;
        public const byte DeleteInContainer                     = 114;

        public const byte SetInventory                          = 120;
        public const byte DeleteInventory                       = 121;

        public const byte AmbientLight                          = 130;
        public const byte GraphicalEffect                       = 131;
        public const byte TextEffect                            = 132;
        public const byte MissleEffect                          = 133;

        public const byte CreatureHealth                        = 140;
        public const byte CreatureLight                         = 141;
        public const byte CreatureOutfit                        = 142;
        public const byte CreatureSpeed                         = 143;
        public const byte CreatureSkull                         = 144;
        public const byte CreatureShield                        = 145;
        public const byte CreatureUnpass                        = 146;
        public const byte CreatureMarks                         = 147;
        public const byte PlayerHelpers                         = 148;
        public const byte CreatureType                          = 149;

        public const byte PlayerBlessings                       = 156;
        // preset: 157
        // trigger: 158
        public const byte PlayerBasicData                       = 159;
        public const byte PlayerStats                           = 160;
        public const byte PlayerSkills                          = 161;
        public const byte PlayerStates                          = 162;
        public const byte ClearTarget                           = 163;

        public const byte SetTactics                            = 167;
        // SetStoreDeepLink

        public const byte Talk                                  = 170;
        public const byte Channels                              = 171;
        public const byte OpenChannel                           = 172;
        public const byte PrivateChannel                        = 173;
        public const byte EditGuildChannel                      = 174;
        // empty
        public const byte OpenOwnChannel                        = 178;
        public const byte CloseChannel                          = 179;
        public const byte TextMessage                           = 180;
        public const byte CancelWalk                            = 181;

        public const byte TopFloor                              = 190;
        public const byte BottomFloor                           = 191;

        public const byte TrackedQuestFlags                     = 208;
        public const byte VipAdd                                = 210;
        public const byte VipState                              = 211;
        public const byte VipLogout                             = 212;

        public const byte PreyFreeListRerollAvailability        = 230;
        public const byte PreyTimeLeft                          = 231;
        public const byte PreyData                              = 232;
        public const byte PreyRerollPrice                       = 233;

        public const byte ResourceBalance                       = 238;

        public const byte ChannelEvent                          = 243;
        public const byte PlayerInventory                       = 245;
    }
}
