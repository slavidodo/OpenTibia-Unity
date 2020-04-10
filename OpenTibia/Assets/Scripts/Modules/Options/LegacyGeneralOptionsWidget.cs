using System.Collections.Generic;

namespace OpenTibiaUnity.Modules.Options
{
    public class LegacyGeneralOptionsWidget : UI.Legacy.PopUpBase
    {
        private enum Option
        {
            TibiaClassicControls,
            AutoChaseOff,
            ShowHints,
            ShowNamesOfCreatures,
            ShowMarksOnCreatures,
            ShowPvpFramesOnCreatures,
            ShowIconsOnNpcs,
            ShowTextualEffects,
            ShowCooldownBar,
            AutoSwitchHotkeyPreset,
        }

        private Dictionary<Option, UI.Legacy.CheckboxPanel> _options = new Dictionary<Option, UI.Legacy.CheckboxPanel>();

        protected override void Awake() {
            base.Awake();

            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelClick);
        }

        private void OnOkClick() {
            UpdateOptionStorage();
        }

        private void OnCancelClick() {
            RevertOptionsBack();
        }

        protected override void OnClientVersionChange(int oldVersion, int newVersion) {
            foreach (var child in _options)
                Destroy(child.Value.gameObject);

            AddOption(Option.TibiaClassicControls);
            AddOption(Option.AutoChaseOff);

            if (newVersion < 810) {
                AddOption(Option.ShowHints);
            }

            AddOption(Option.ShowNamesOfCreatures);
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameCreatureMarks)) {
                AddOption(Option.ShowMarksOnCreatures);
                AddOption(Option.ShowPvpFramesOnCreatures);
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameCreatureIcons)) {
                AddOption(Option.ShowIconsOnNpcs);
            }

            AddOption(Option.ShowTextualEffects);
            if (newVersion > 870) {
                AddOption(Option.ShowCooldownBar);
            }

            if (newVersion >= 1055) {
                AddOption(Option.AutoSwitchHotkeyPreset);
            }

            RevertOptionsBack();
        }

        private UI.Legacy.CheckboxPanel AddOption(Option id) {
            var checkboxPanel = Instantiate(OpenTibiaUnity.GameManager.PanelCheckBox, _content);
            checkboxPanel.label.text = GetText(id);

            _options.Add(id, checkboxPanel);
            return checkboxPanel;
        }

        private string GetText(Option id) {
            switch (id) {
                case Option.TibiaClassicControls:
                    return TextResources.LEGACYOPTIONS_GENERAL_TIBIA_CLASSIC_CONTROLS;
                case Option.AutoChaseOff:
                    return TextResources.LEGACYOPTIONS_GENERAL_AUTO_CHASE_OFF;
                case Option.ShowHints:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOWHINTS;
                case Option.ShowNamesOfCreatures:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_NAMES_OF_CREATURES;
                case Option.ShowMarksOnCreatures:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_MARKS_ON_CREATURES;
                case Option.ShowPvpFramesOnCreatures:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_PVP_FRAMES_ON_CREATURES;
                case Option.ShowIconsOnNpcs:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_ICONS_ON_NPCS;
                case Option.ShowTextualEffects:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_TEXTUAL_EFFECTS;
                case Option.ShowCooldownBar:
                    return TextResources.LEGACYOPTIONS_GENERAL_SHOW_COOLDOWN_BAR;
                case Option.AutoSwitchHotkeyPreset:
                    return TextResources.LEGACYOPTIONS_GENERAL_AUTO_SWITCH_HOTKEY_PRESET;
            }

            return string.Empty;
        }

        private void UpdateOptionStorage() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            foreach (var option in _options) {
                var active = option.Value.checkbox.Checked;
                switch (option.Key) {
                    case Option.TibiaClassicControls:
                        optionStorage.MousePreset = active ? MousePresets.Classic : MousePresets.Regular;
                        break;
                    case Option.AutoChaseOff:
                        optionStorage.AutoChaseOff = active;
                        break;
                    case Option.ShowHints:
                        // todo; hints
                        break;
                    case Option.ShowNamesOfCreatures:
                        optionStorage.ShowNameForOtherCreatures = active;
                        break;
                    case Option.ShowMarksOnCreatures:
                        optionStorage.ShowMarksForOtherCreatures = active;
                        break;
                    case Option.ShowPvpFramesOnCreatures:
                        optionStorage.ShowPvpFrames = active;
                        break;
                    case Option.ShowIconsOnNpcs:
                        optionStorage.ShowNPCIcons = active;
                        break;
                    case Option.ShowTextualEffects:
                        optionStorage.ShowTextualEffects = active;
                        break;
                    case Option.ShowCooldownBar:
                        optionStorage.ShowCooldownBar = active;
                        break;
                    case Option.AutoSwitchHotkeyPreset:
                        optionStorage.AutoSwitchHotkeyPreset = active;
                        break;
                }
            }
        }

        private void RevertOptionsBack() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            foreach (var option in _options) {
                bool active = false;
                switch (option.Key) {
                    case Option.TibiaClassicControls:
                        active = optionStorage.MousePreset == MousePresets.Classic;
                        break;
                    case Option.AutoChaseOff:
                        active = optionStorage.AutoChaseOff;
                        break;
                    case Option.ShowHints:
                        active = false; // todo; hints
                        break;
                    case Option.ShowNamesOfCreatures:
                        active = optionStorage.ShowNameForOtherCreatures;
                        break;
                    case Option.ShowMarksOnCreatures:
                        active = optionStorage.ShowMarksForOtherCreatures;
                        break;
                    case Option.ShowPvpFramesOnCreatures:
                        active = optionStorage.ShowPvpFrames;
                        break;
                    case Option.ShowIconsOnNpcs:
                        active = optionStorage.ShowNPCIcons;
                        break;
                    case Option.ShowTextualEffects:
                        active = optionStorage.ShowTextualEffects;
                        break;
                    case Option.ShowCooldownBar:
                        active = optionStorage.ShowCooldownBar;
                        break;
                    case Option.AutoSwitchHotkeyPreset:
                        active = optionStorage.AutoSwitchHotkeyPreset;
                        break;
                }

                option.Value.checkbox.Checked = active;
            }
        }
    }
}
