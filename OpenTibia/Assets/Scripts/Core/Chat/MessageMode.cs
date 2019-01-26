namespace OpenTibiaUnity.Core.Chat
{
    public class MessageModeProperties
    {
        public MessageModes Mode;
        public bool ShowOnscreen;
        public bool ShowChannel;
        public uint TextARGB;
        public uint HighlightARGB;
        public bool Editable;
        public bool IgnoreNameFilter;
        public MessageScreenTargets ScreenTarget;
        public MessageModeHeaders Header;
        public MessageModePrefixes Prefix;

        public MessageModeProperties(MessageModes mode, bool showOnscreen, bool showChannel, uint textARGB, uint highlightARGB, bool editable, bool ignoreNameFilter, MessageScreenTargets screenTarget, MessageModeHeaders header = MessageModeHeaders.None, MessageModePrefixes prefix = MessageModePrefixes.None) {
            Mode = mode;
            ShowOnscreen = showOnscreen;
            ShowChannel = showChannel;
            TextARGB = textARGB;
            HighlightARGB = highlightARGB;
            Editable = editable;
            IgnoreNameFilter = ignoreNameFilter;
            ScreenTarget = screenTarget;
            Header = header;
            Prefix = prefix;
        }
    }

    public class MessageMode {
        public static MessageModeProperties[] MessageModeDefaults;
        public static uint[] MessageModeColors;

        static MessageMode() {
            MessageModeDefaults = new MessageModeProperties[(int)MessageModes.BeyondLast];
            // None (no color)
            MessageModeDefaults[(int)MessageModes.None] = new MessageModeProperties(MessageModes.None, false, false, 0, 0, false, true, MessageScreenTargets.None);
            // Say (yellow at local chat & coordinates)
            MessageModeDefaults[(int)MessageModes.Say] = new MessageModeProperties(MessageModes.Say, true, true, 0xFFFF00, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Say);
            // Whisper (yellow at local chat & coordinates)
            MessageModeDefaults[(int)MessageModes.Whisper] = new MessageModeProperties(MessageModes.Whisper, true, true, 0xFFFF00, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Whisper);
            // Yell (yellow at local chat && coordinates)
            MessageModeDefaults[(int)MessageModes.Yell] = new MessageModeProperties(MessageModes.Yell, true, true, 0xFFFF00, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Yell);
            // PrivateFrom (sent from a player, cyan at chat & center of screen)
            MessageModeDefaults[(int)MessageModes.PrivateFrom] = new MessageModeProperties(MessageModes.PrivateFrom, true, true, 0x5FF7F7, 0, true, false, MessageScreenTargets.BoxTop, MessageModeHeaders.None, MessageModePrefixes.PrivateFrom);
            // PrivateTo (sent from me, violet at chat)
            MessageModeDefaults[(int)MessageModes.PrivateTo] = new MessageModeProperties(MessageModes.PrivateTo, false, true, 0x9F9DFD, 0, true, false, MessageScreenTargets.None);
            // ChannelManagment (white at channel & center of screen)
            MessageModeDefaults[(int)MessageModes.ChannelManagment] = new MessageModeProperties(MessageModes.ChannelManagment, true, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.BoxHigh);
            // Channel (yellow at a channel)
            MessageModeDefaults[(int)MessageModes.Channel] = new MessageModeProperties(MessageModes.Channel, false, true, 0xFFFF00, 0, false, false, MessageScreenTargets.None);
            // ChannelHighlight (orange at a channel)
            MessageModeDefaults[(int)MessageModes.ChannelHighlight] = new MessageModeProperties(MessageModes.ChannelHighlight, false, true, 0xFE6500, 0, false, false, MessageScreenTargets.None);
            // Spell (yellow at chat and coordinates)
            MessageModeDefaults[(int)MessageModes.Spell] = new MessageModeProperties(MessageModes.Spell, true, true, 0xFFFF00, 2, true, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Spell);
            // NpcFromStartBlock (same as NpcFrom)
            MessageModeDefaults[(int)MessageModes.NpcFromStartBlock] = new MessageModeProperties(MessageModes.NpcFromStartBlock, true, true, 0x5FF7F7, 0x5FF7F7, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFromStartBlock);
            // NpcFrom (message from npc, cyan at NPC channel & coordinates)
            MessageModeDefaults[(int)MessageModes.NpcFrom] = new MessageModeProperties(MessageModes.NpcFrom, true, true, 0x5FF7F7, 0x5FF7F7, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFrom);
            // NpcTo (message from the player to the npc, violet at NPC channel)
            MessageModeDefaults[(int)MessageModes.NpcTo] = new MessageModeProperties(MessageModes.NpcTo, false, true, 0x9F9DFD, 0, false, true, MessageScreenTargets.None);
            // GamemasterBroadcast (red at chat and screen)
            MessageModeDefaults[(int)MessageModes.GamemasterBroadcast] = new MessageModeProperties(MessageModes.GamemasterBroadcast, true, true, 0xF55E5E, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterBroadcast);
            // GamemasterChannel (same as broadcast but no screen)
            MessageModeDefaults[(int)MessageModes.GamemasterChannel] = new MessageModeProperties(MessageModes.GamemasterChannel, false, true, 0xF55E5E, 0, false, true, MessageScreenTargets.None);
            // GamemasterPrivateFrom (same as broadcast)
            MessageModeDefaults[(int)MessageModes.GamemasterPrivateFrom] = new MessageModeProperties(MessageModes.GamemasterPrivateFrom, true, true, 0xF55E5E, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterPrivateFrom);
            // GamemasterPrivateTo (player to gm, violet at chat)
            MessageModeDefaults[(int)MessageModes.GamemasterPrivateTo] = new MessageModeProperties(MessageModes.GamemasterPrivateTo, false, true, 0x9F9DFD, 0, false, true, MessageScreenTargets.BoxTop);
            // Login (white at server log & center screen)
            MessageModeDefaults[(int)MessageModes.Login] = new MessageModeProperties(MessageModes.Login, true, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.BoxBottom);
            // Admin (Warning at server log & center screen)
            MessageModeDefaults[(int)MessageModes.Admin] = new MessageModeProperties(MessageModes.Admin, true, true, 0xF55E5E, 0, false, true, MessageScreenTargets.BoxLow);
            // Game (white at server log & center screen)
            MessageModeDefaults[(int)MessageModes.Game] = new MessageModeProperties(MessageModes.Game, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.BoxLow);
            // GameHighlight (TODO: color)
            MessageModeDefaults[(int)MessageModes.GameHighlight] = new MessageModeProperties(MessageModes.GameHighlight, true, true, 0, 0, false, true, MessageScreenTargets.BoxLow);
            // Failure (white at bottom of screen only, i.e sorry not possible)
            MessageModeDefaults[(int)MessageModes.Failure] = new MessageModeProperties(MessageModes.Failure, true, false, 0xFFFFFF, 0, false, true, MessageScreenTargets.BoxBottom);
            // Look (green at server log & center)
            MessageModeDefaults[(int)MessageModes.Look] = new MessageModeProperties(MessageModes.Look, true, true, 0x00EB00, 0, true, true, MessageScreenTargets.BoxHigh);
            // DamageDealed (white at chat & coordinate)
            MessageModeDefaults[(int)MessageModes.DamageDealed] = new MessageModeProperties(MessageModes.DamageDealed, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.DamageReceived] = new MessageModeProperties(MessageModes.DamageReceived, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.Heal] = new MessageModeProperties(MessageModes.Heal, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.Exp] = new MessageModeProperties(MessageModes.Exp, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.DamageOthers] = new MessageModeProperties(MessageModes.DamageOthers, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.HealOthers] = new MessageModeProperties(MessageModes.DamageOthers, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.ExpOthers] = new MessageModeProperties(MessageModes.ExpOthers, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.EffectCoordinate);
            MessageModeDefaults[(int)MessageModes.Status] = new MessageModeProperties(MessageModes.Status, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.BoxBottom);
            // Loot (same as look)
            MessageModeDefaults[(int)MessageModes.Loot] = new MessageModeProperties(MessageModes.Loot, true, true, 0x00EB00, 0, true, true, MessageScreenTargets.BoxHigh);
            // TradeNpx (center white & server log)
            MessageModeDefaults[(int)MessageModes.TradeNpc] = new MessageModeProperties(MessageModes.TradeNpc, true, true, 0xFFFFFF, 0, true, true, MessageScreenTargets.BoxHigh);
            MessageModeDefaults[(int)MessageModes.Guild] = new MessageModeProperties(MessageModes.Guild, false, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.BoxLow);
            MessageModeDefaults[(int)MessageModes.PartyManagement] = new MessageModeProperties(MessageModes.PartyManagement, false, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.None);
            MessageModeDefaults[(int)MessageModes.Party] = new MessageModeProperties(MessageModes.Party, false, true, 0x00EB00, 0, false, true, MessageScreenTargets.BoxLow);
            MessageModeDefaults[(int)MessageModes.BarkLow] = new MessageModeProperties(MessageModes.BarkLow, true, false, 0xFE6500, 0, false, true, MessageScreenTargets.BoxCoordinate);
            MessageModeDefaults[(int)MessageModes.BarkLoud] = new MessageModeProperties(MessageModes.BarkLoud, true, false, 0xFE6500, 0, false, true, MessageScreenTargets.BoxCoordinate);
            MessageModeDefaults[(int)MessageModes.Report] = new MessageModeProperties(MessageModes.Report, true, true, 0xF55E5E, 0, false, true, MessageScreenTargets.BoxLow);
            MessageModeDefaults[(int)MessageModes.HotkeyUse] = new MessageModeProperties(MessageModes.HotkeyUse, true, true, 0x00EB00, 0, true, true, MessageScreenTargets.BoxBottom);
            // TODO unknown (but i think green)
            MessageModeDefaults[(int)MessageModes.TutorialHint] = new MessageModeProperties(MessageModes.TutorialHint, true, true, 0, 0, false, true, MessageScreenTargets.BoxBottom);
            MessageModeDefaults[(int)MessageModes.Thankyou] = new MessageModeProperties(MessageModes.Thankyou, true, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.BoxLow);
            MessageModeDefaults[(int)MessageModes.Market] = new MessageModeProperties(MessageModes.Market, false, false, 0xFFFFFF, 0, false, true, MessageScreenTargets.None);
            MessageModeDefaults[(int)MessageModes.Mana] = new MessageModeProperties(MessageModes.Mana, true, true, 0xFFFFFF, 0, false, true, MessageScreenTargets.EffectCoordinate);
        }

        private MessageModes m_ID;
        private bool m_ShowOnscreenMessage;
        private bool m_ShowChannelMessage;
        private uint m_TextARGB;
        private uint m_HighlightARGB;
        private MessageScreenTargets m_ScreenTarget;
        private MessageModeHeaders m_Header;
        private MessageModePrefixes m_Prefix;

        public MessageModes ID {
            get { return m_ID; }
        }
        public bool ShowOnScreen {
            get { return m_ShowOnscreenMessage; }
        }
        public bool ShowChannelMessage {
            get { return m_ShowChannelMessage; }
        }
        public uint TextARGB {
            get { return m_TextARGB; }
        }
        public uint HighlightARGB {
            get { return m_HighlightARGB; }
        }
        public MessageScreenTargets ScreenTarget {
            get { return m_ScreenTarget; }
        }

        public MessageMode(MessageModes mode) {
            m_ID = mode;
            m_ShowOnscreenMessage = MessageModeDefaults[(int)m_ID].ShowOnscreen;
            m_ShowChannelMessage = MessageModeDefaults[(int)m_ID].ShowChannel;
            m_TextARGB = MessageModeDefaults[(int)m_ID].TextARGB;
            m_HighlightARGB = MessageModeDefaults[(int)m_ID].HighlightARGB;
            m_ScreenTarget = MessageModeDefaults[(int)m_ID].ScreenTarget;
            m_Header = MessageModeDefaults[(int)m_ID].Header;
            m_Prefix = MessageModeDefaults[(int)m_ID].Prefix;
        }

        public string GetOnscreenMessageHeader(params object[] rest) {
            switch (m_Header) {
                case MessageModeHeaders.Say:
                    return string.Format("{0} says:", rest);
                case MessageModeHeaders.Whisper:
                    return string.Format("{0} whispers:", rest);
                case MessageModeHeaders.Yell:
                    return string.Format("{0} yells:", rest);
                case MessageModeHeaders.Spell:
                    return string.Format("{0} casts:", rest);
                case MessageModeHeaders.NpcFrom:
                    return string.Format("{0}:\n:", rest);

                default:
                    return null;
            }
        }

        public string GetOnscreenMessagePrefix(params object[] rest) {
            switch (m_Prefix) {
                case MessageModePrefixes.PrivateFrom:
                case MessageModePrefixes.GamemasterBroadcast:
                case MessageModePrefixes.GamemasterPrivateFrom:
                    return string.Format("{0}:\n", rest);

                default:
                    return null;
            }
        }

        public static bool s_CheckMode(int mode) {
            return mode >= 0 && mode < (int)MessageModes.BeyondLast;
        }
    }
}
