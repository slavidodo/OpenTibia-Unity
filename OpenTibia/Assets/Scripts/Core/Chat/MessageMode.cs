using System.Collections.Generic;

namespace OpenTibiaUnity.Core.Chat
{
    internal class MessageModeProperties
    {
        internal MessageModeType Mode;
        internal bool ShowOnscreen;
        internal bool ShowChannel;
        internal uint TextARGB;
        internal uint HighlightARGB;
        internal bool Editable;
        internal bool IgnoreNameFilter;
        internal MessageScreenTargets ScreenTarget;
        internal MessageModeHeaders Header;
        internal MessageModePrefixes Prefix;

        internal MessageModeProperties(MessageModeType mode, bool showOnscreen, bool showChannel, uint textARGB, uint highlightARGB, bool editable, bool ignoreNameFilter, MessageScreenTargets screenTarget = MessageScreenTargets.None, MessageModeHeaders header = MessageModeHeaders.None, MessageModePrefixes prefix = MessageModePrefixes.None) {
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

    internal static class MessageColors
    {
        internal const uint White = 0xFFFFFF;
        internal const uint Yellow = 0xFFFF00;

        internal const uint Red = 0xF55E5E;
        internal const uint Green = 0x00EB00;

        internal const uint Blue = 0x9F9DFD;
        internal const uint Cyan = 0x5FF7F7;

        internal const uint Orange = 0xFE6500;

        internal const uint Pink = 0xF080CE;

        internal const uint Grey = 0x7F7F7F;
    }

    internal class MessageMode {
        internal static Dictionary<MessageModeType, MessageModeProperties> MessageModeDefaults;

        static MessageMode() {
            MessageModeDefaults = new Dictionary<MessageModeType, MessageModeProperties>();
            MessageModeDefaults.Add(MessageModeType.None, new MessageModeProperties(MessageModeType.None, false, false, 0, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.Say, new MessageModeProperties(MessageModeType.Say, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Say));
            MessageModeDefaults.Add(MessageModeType.Whisper, new MessageModeProperties(MessageModeType.Whisper, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Whisper));
            MessageModeDefaults.Add(MessageModeType.Yell, new MessageModeProperties(MessageModeType.Yell, true, true, MessageColors.Yellow, 0, false, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Yell));
            MessageModeDefaults.Add(MessageModeType.PrivateFrom, new MessageModeProperties(MessageModeType.PrivateFrom, true, true, MessageColors.Cyan, 0, true, false, MessageScreenTargets.BoxTop, MessageModeHeaders.None, MessageModePrefixes.PrivateFrom));
            MessageModeDefaults.Add(MessageModeType.PrivateTo, new MessageModeProperties(MessageModeType.PrivateTo, false, true, MessageColors.Blue, 0, true, false));
            MessageModeDefaults.Add(MessageModeType.ChannelManagement, new MessageModeProperties(MessageModeType.ChannelManagement, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModeType.Channel, new MessageModeProperties(MessageModeType.Channel, false, true, MessageColors.Yellow, 0, false, false));
            MessageModeDefaults.Add(MessageModeType.ChannelHighlight, new MessageModeProperties(MessageModeType.ChannelHighlight, false, true, MessageColors.Orange, 0, false, false));
            MessageModeDefaults.Add(MessageModeType.Spell, new MessageModeProperties(MessageModeType.Spell, true, true, MessageColors.Yellow, 2, true, false, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.Spell));
            MessageModeDefaults.Add(MessageModeType.NpcFrom, new MessageModeProperties(MessageModeType.NpcFrom, true, true, MessageColors.Cyan, MessageColors.Cyan, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFrom));
            MessageModeDefaults.Add(MessageModeType.NpcTo, new MessageModeProperties(MessageModeType.NpcTo, false, true, MessageColors.Blue, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.GamemasterBroadcast, new MessageModeProperties(MessageModeType.GamemasterBroadcast, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterBroadcast));
            MessageModeDefaults.Add(MessageModeType.GamemasterChannel, new MessageModeProperties(MessageModeType.GamemasterChannel, false, true, MessageColors.Red, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.GamemasterPrivateFrom, new MessageModeProperties(MessageModeType.GamemasterPrivateFrom, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow, MessageModeHeaders.None, MessageModePrefixes.GamemasterPrivateFrom));
            MessageModeDefaults.Add(MessageModeType.GamemasterPrivateTo, new MessageModeProperties(MessageModeType.GamemasterPrivateTo, false, true, MessageColors.Blue, 0, false, true, MessageScreenTargets.BoxTop));
            MessageModeDefaults.Add(MessageModeType.Login, new MessageModeProperties(MessageModeType.Login, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModeType.Admin, new MessageModeProperties(MessageModeType.Admin, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.Game, new MessageModeProperties(MessageModeType.Game, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.Failure, new MessageModeProperties(MessageModeType.Failure, true, false, MessageColors.White, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModeType.Look, new MessageModeProperties(MessageModeType.Look, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModeType.DamageDealed, new MessageModeProperties(MessageModeType.DamageDealed, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.DamageReceived, new MessageModeProperties(MessageModeType.DamageReceived, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.Heal, new MessageModeProperties(MessageModeType.Heal, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.Exp, new MessageModeProperties(MessageModeType.Exp, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.DamageOthers, new MessageModeProperties(MessageModeType.DamageOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.HealOthers, new MessageModeProperties(MessageModeType.DamageOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.ExpOthers, new MessageModeProperties(MessageModeType.ExpOthers, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.EffectCoordinate));
            MessageModeDefaults.Add(MessageModeType.Status, new MessageModeProperties(MessageModeType.Status, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModeType.Loot, new MessageModeProperties(MessageModeType.Loot, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModeType.TradeNpc, new MessageModeProperties(MessageModeType.TradeNpc, true, true, MessageColors.White, 0, true, true, MessageScreenTargets.BoxHigh));
            MessageModeDefaults.Add(MessageModeType.Guild, new MessageModeProperties(MessageModeType.Guild, false, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.PartyManagement, new MessageModeProperties(MessageModeType.PartyManagement, false, true, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.Party, new MessageModeProperties(MessageModeType.Party, false, true, MessageColors.Green, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.BarkLow, new MessageModeProperties(MessageModeType.BarkLow, true, false, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModeType.BarkLoud, new MessageModeProperties(MessageModeType.BarkLoud, true, true, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModeType.Report, new MessageModeProperties(MessageModeType.Report, true, true, MessageColors.Red, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.HotkeyUse, new MessageModeProperties(MessageModeType.HotkeyUse, true, true, MessageColors.Green, 0, true, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModeType.TutorialHint, new MessageModeProperties(MessageModeType.TutorialHint, true, true, MessageColors.Green, 0, false, true, MessageScreenTargets.BoxBottom));
            MessageModeDefaults.Add(MessageModeType.Thankyou, new MessageModeProperties(MessageModeType.Thankyou, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.Market, new MessageModeProperties(MessageModeType.Market, false, false, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.Mana, new MessageModeProperties(MessageModeType.Mana, true, true, MessageColors.White, 0, false, true, MessageScreenTargets.EffectCoordinate));
            
            MessageModeDefaults.Add(MessageModeType.MonsterYell, new MessageModeProperties(MessageModeType.MonsterYell, true, true, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModeType.MonsterSay, new MessageModeProperties(MessageModeType.MonsterSay, true, false, MessageColors.Orange, 0, false, true, MessageScreenTargets.BoxCoordinate));
            MessageModeDefaults.Add(MessageModeType.Red, new MessageModeProperties(MessageModeType.Red, false, true, MessageColors.Red, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.Blue, new MessageModeProperties(MessageModeType.Blue, false, true, MessageColors.Blue, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.RVRChannel, new MessageModeProperties(MessageModeType.RVRChannel, true, true, MessageColors.White, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.RVRAnswer, new MessageModeProperties(MessageModeType.RVRAnswer, true, true, MessageColors.Orange, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.RVRContinue, new MessageModeProperties(MessageModeType.RVRContinue, true, true, MessageColors.Yellow, 0, false, true));
            MessageModeDefaults.Add(MessageModeType.GameHighlight, new MessageModeProperties(MessageModeType.GameHighlight, true, true, 0, 0, false, true, MessageScreenTargets.BoxLow));
            MessageModeDefaults.Add(MessageModeType.NpcFromStartBlock, new MessageModeProperties(MessageModeType.NpcFromStartBlock, true, true, MessageColors.Cyan, MessageColors.Cyan, false, true, MessageScreenTargets.BoxCoordinate, MessageModeHeaders.NpcFromStartBlock));
        }

        private MessageModeType m_ID;
        private bool m_ShowOnscreenMessage;
        private bool m_ShowChannelMessage;
        private uint m_TextARGB;
        private uint m_HighlightARGB;
        private MessageScreenTargets m_ScreenTarget;
        private bool m_IgnoreNameFilter;
        private MessageModeHeaders m_Header;
        private MessageModePrefixes m_Prefix;

        internal MessageModeType ID { get => m_ID; }
        internal bool ShowOnScreen { get => m_ShowOnscreenMessage; }
        internal bool ShowChannelMessage { get => m_ShowChannelMessage; }
        internal uint TextARGB { get => FormattedTextARGB(); }
        internal uint HighlightARGB { get => m_HighlightARGB; }
        internal MessageScreenTargets ScreenTarget { get => m_ScreenTarget; }
        internal bool IgnoreNameFilter { get => m_IgnoreNameFilter; }

        internal MessageMode(MessageModeType mode) {
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
            m_IgnoreNameFilter = MessageModeDefaults[m_ID].IgnoreNameFilter;
        }

        internal string GetOnscreenMessageHeader(params object[] rest) {
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

        internal string GetOnscreenMessagePrefix(params object[] rest) {
            switch (m_Prefix) {
                case MessageModePrefixes.PrivateFrom:
                case MessageModePrefixes.GamemasterBroadcast:
                case MessageModePrefixes.GamemasterPrivateFrom:
                    return string.Format("{0}:\n", rest);

                default:
                    return null;
            }
        }

        internal uint FormattedTextARGB() {
            var clientVersion = OpenTibiaUnity.GameManager.ClientVersion;

            switch (m_ID) {
                case MessageModeType.Spell:
                    if (clientVersion > 1100 && clientVersion < 1110)
                        return MessageColors.Pink;
                    break;
                case MessageModeType.Loot:
                    if (clientVersion >= 1200)
                        return MessageColors.White;
                    break;
            }

            return m_TextARGB;
        }

        internal static bool s_CheckMode(int mode) {
            return mode >= 0 && mode != (int)MessageModeType.Invalid;
        }
    }
}
