using System.Collections.Generic;

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

        public MessageModeProperties(MessageModes mode, bool showOnscreen, bool showChannel, uint textARGB, uint highlightARGB, bool editable, bool ignoreNameFilter, MessageScreenTargets screenTarget = MessageScreenTargets.None, MessageModeHeaders header = MessageModeHeaders.None, MessageModePrefixes prefix = MessageModePrefixes.None) {
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

    public static class MessageColors
    {
        public const uint White = 0xFFFFFF;
        public const uint Yellow = 0xFFFF00;

        public const uint Red = 0xF55E5E;
        public const uint Green = 0x00EB00;

        public const uint Blue = 0x9F9DFD;
        public const uint Cyan = 0x5FF7F7;

        public const uint Orange = 0xFE6500;

        public const uint Pink = 0xF080CE;
    }

    public class MessageMode {
        public static Dictionary<MessageModes, MessageModeProperties> MessageModeDefaults;
        public static uint[] MessageModeColors;

        static MessageMode() {
            MessageModeDefaults = new Dictionary<MessageModes, MessageModeProperties>();
            MessageModeDefaults.Add(MessageModes.None, new MessageModeProperties(MessageModes.None, false, false, 0, 0, false, true));
            MessageModeDefaults.Add(MessageModes.Say, new MessageModeProperties(MessageModes.Say, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Say));
            MessageModeDefaults.Add(MessageModes.Whisper, new MessageModeProperties(MessageModes.Whisper, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Whisper));
            MessageModeDefaults.Add(MessageModes.Yell, new MessageModeProperties(MessageModes.Yell, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Yell));
            MessageModeDefaults.Add(MessageModes.PrivateFrom, new MessageModeProperties(MessageModes.PrivateFrom, true, true, MessageColors.Cyan, 0, true, false, MessageScreenTargets.BoxTop, MessageModeHeaders.None, MessageModePrefixes.PrivateFrom));
            MessageModeDefaults.Add(MessageModes.PrivateTo, new MessageModeProperties(MessageModes.PrivateTo, false, true, MessageColors.Blue, 0, true, false));
            MessageModeDefaults.Add(MessageModes.ChannelManagement, new MessageModeProperties(MessageModes.ChannelManagement, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModes.Channel, new MessageModeProperties(MessageModes.Channel, false, true, MessageColors.Yellow, 0, false, false));
            MessageModeDefaults.Add(MessageModes.ChannelHighlight, new MessageModeProperties(MessageModes.ChannelHighlight, false, true, MessageColors.Orange, 0, false, false));
            MessageModeDefaults.Add(MessageModes.Spell, new MessageModeProperties(MessageModes.Spell, true, true, MessageColors.Yellow, 2, true, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Spell));
            MessageModeDefaults.Add(MessageModes.NpcFrom, new MessageModeProperties(MessageModes.NpcFrom, true, true, MessageColors.Cyan, MessageColors.Cyan, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFrom));
            MessageModeDefaults.Add(MessageModes.NpcTo, new MessageModeProperties(MessageModes.NpcTo, false, true, MessageColors.Blue, 0, false, true));
            MessageModeDefaults.Add(MessageModes.GamemasterBroadcast, new MessageModeProperties(MessageModes.GamemasterBroadcast, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterBroadcast));
            MessageModeDefaults.Add(MessageModes.GamemasterChannel, new MessageModeProperties(MessageModes.GamemasterChannel, false, true, MessageColors.Red, 0, false, true));
            MessageModeDefaults.Add(MessageModes.GamemasterPrivateFrom, new MessageModeProperties(MessageModes.GamemasterPrivateFrom, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterPrivateFrom));
            MessageModeDefaults.Add(MessageModes.GamemasterPrivateTo, new MessageModeProperties(MessageModes.GamemasterPrivateTo, false, true, MessageColors.Blue, 0, false, true, MessageScreenTargets.BoxTop));
            MessageModeDefaults.Add(MessageModes.Login, new MessageModeProperties(MessageModes.Login, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModes.Admin, new MessageModeProperties(MessageModes.Admin, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.Game, new MessageModeProperties(MessageModes.Game, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.Failure, new MessageModeProperties(MessageModes.Failure, true, false, MessageColors.White, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModes.Look, new MessageModeProperties(MessageModes.Look, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModes.DamageDealed, new MessageModeProperties(MessageModes.DamageDealed, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.DamageReceived, new MessageModeProperties(MessageModes.DamageReceived, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.Heal, new MessageModeProperties(MessageModes.Heal, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.Exp, new MessageModeProperties(MessageModes.Exp, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.DamageOthers, new MessageModeProperties(MessageModes.DamageOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.HealOthers, new MessageModeProperties(MessageModes.DamageOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.ExpOthers, new MessageModeProperties(MessageModes.ExpOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModes.Status, new MessageModeProperties(MessageModes.Status, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModes.Loot, new MessageModeProperties(MessageModes.Loot, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModes.TradeNpc, new MessageModeProperties(MessageModes.TradeNpc, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModes.Guild, new MessageModeProperties(MessageModes.Guild, false, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.PartyManagement, new MessageModeProperties(MessageModes.PartyManagement, false, true, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModes.Party, new MessageModeProperties(MessageModes.Party, false, true, MessageColors.Green, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.BarkLow, new MessageModeProperties(MessageModes.BarkLow, true, false, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModes.BarkLoud, new MessageModeProperties(MessageModes.BarkLoud, true, true, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModes.Report, new MessageModeProperties(MessageModes.Report, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.HotkeyUse, new MessageModeProperties(MessageModes.HotkeyUse, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModes.TutorialHint, new MessageModeProperties(MessageModes.TutorialHint, true, true, MessageColors.Green, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModes.Thankyou, new MessageModeProperties(MessageModes.Thankyou, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.Market, new MessageModeProperties(MessageModes.Market, false, false, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModes.Mana, new MessageModeProperties(MessageModes.Mana, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.EffectCoordinate));
            
            MessageModeDefaults.Add(MessageModes.MonsterYell, new MessageModeProperties(MessageModes.MonsterYell, true, true, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModes.MonsterSay, new MessageModeProperties(MessageModes.MonsterSay, true, false, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModes.Red, new MessageModeProperties(MessageModes.Red, false, true, MessageColors.Red, 0, false, true));
            MessageModeDefaults.Add(MessageModes.Blue, new MessageModeProperties(MessageModes.Blue, false, true, MessageColors.Cyan, 0, false, true));
            MessageModeDefaults.Add(MessageModes.RVRChannel, new MessageModeProperties(MessageModes.RVRChannel, true, true, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModes.RVRAnswer, new MessageModeProperties(MessageModes.RVRAnswer, true, true, MessageColors.Orange, 0, false, true));
            MessageModeDefaults.Add(MessageModes.RVRContinue, new MessageModeProperties(MessageModes.RVRContinue, true, true, MessageColors.Yellow, 0, false, true));
            MessageModeDefaults.Add(MessageModes.GameHighlight, new MessageModeProperties(MessageModes.GameHighlight, true, true, 0, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModes.NpcFromStartBlock, new MessageModeProperties(MessageModes.NpcFromStartBlock, true, true, MessageColors.Cyan, MessageColors.Cyan, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFromStartBlock));
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
            get { return FormattedTextARGB(); }
        }
        public uint HighlightARGB {
            get { return m_HighlightARGB; }
        }
        public MessageScreenTargets ScreenTarget {
            get { return m_ScreenTarget; }
        }

        public MessageMode(MessageModes mode) {
            m_ID = mode;
            if (!MessageModeDefaults.ContainsKey(mode)) {
                UnityEngine.Debug.LogWarningFormat("Unable to find a settings for mode: {0}.", mode);
                return;
            }

            m_ShowOnscreenMessage = MessageModeDefaults[m_ID].ShowOnscreen;
            m_ShowChannelMessage = MessageModeDefaults[m_ID].ShowChannel;
            m_TextARGB = MessageModeDefaults[m_ID].TextARGB;
            m_HighlightARGB = MessageModeDefaults[m_ID].HighlightARGB;
            m_ScreenTarget = MessageModeDefaults[m_ID].ScreenTarget;
            m_Header = MessageModeDefaults[m_ID].Header;
            m_Prefix = MessageModeDefaults[m_ID].Prefix;
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

        public uint FormattedTextARGB() {
            var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;

            switch (m_ID) {
                case MessageModes.Spell:
                    if (clientVersion > 1100 && clientVersion < 1110)
                        return MessageColors.Pink;
                    break;
            }

            return m_TextARGB;
        }

        public static bool s_CheckMode(int mode) {
            return mode >= 0 && mode != (int)MessageModes.Invalid;
        }
    }
}
