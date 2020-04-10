using System.Collections.Generic;

namespace OpenTibiaUnity.Modules.Options
{
    public class LegacyConsoleOptionsWidget : UI.Legacy.PopUpBase
    {
        private enum Option
        {
            ShowInfoMessagesInConsole,
            ShowEventMessagesInConsole,
            ShowStatusMessagesInConsole,
            ShowStatusMessagesOfOthersInConsole,
            ShowTimestampInConsole,
            ShowLevelsInConsle,
            ShowPrivateMessagesInGameWindow,
        }

        // fields
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

            AddOption(Option.ShowInfoMessagesInConsole);
            AddOption(Option.ShowEventMessagesInConsole);
            AddOption(Option.ShowStatusMessagesInConsole);

            if (newVersion >= 900) {
                AddOption(Option.ShowStatusMessagesOfOthersInConsole);
            }

            AddOption(Option.ShowTimestampInConsole);

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameMessageLevel)) {
                AddOption(Option.ShowLevelsInConsle);
            }

            AddOption(Option.ShowPrivateMessagesInGameWindow);

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
                case Option.ShowInfoMessagesInConsole:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_INFO_MESSAGES;
                case Option.ShowEventMessagesInConsole:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_EVENT_MESSAGES;
                case Option.ShowStatusMessagesInConsole:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_STATUS_MESSAGES;
                case Option.ShowStatusMessagesOfOthersInConsole:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_STATUS_MESSAGES_OF_OTHERS;
                case Option.ShowTimestampInConsole:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_TIMESTAMP;
                case Option.ShowLevelsInConsle:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_LEVELS;
                case Option.ShowPrivateMessagesInGameWindow:
                    return TextResources.LEGACYOPTIONS_CONSOLE_SHOW_PRIVATE_MESSAGES;
            }

            return string.Empty;
        }

        private void UpdateOptionStorage() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
            foreach (var option in _options) {
                var active = option.Value.checkbox.Checked;
                switch (option.Key) {
                    case Option.ShowInfoMessagesInConsole:
                        optionStorage.ShowInfoMessages = active;
                        break;
                    case Option.ShowEventMessagesInConsole:
                        optionStorage.ShowEventMessages = active;
                        break;
                    case Option.ShowStatusMessagesInConsole:
                        optionStorage.ShowStatusMessages = active;
                        break;
                    case Option.ShowStatusMessagesOfOthersInConsole:
                        optionStorage.ShowStatusMessagesOfOthers = active;
                        break;
                    case Option.ShowTimestampInConsole:
                        optionStorage.ShowTimestamps = active;
                        break;
                    case Option.ShowLevelsInConsle:
                        optionStorage.ShowLevels = active;
                        break;
                    case Option.ShowPrivateMessagesInGameWindow:
                        optionStorage.ShowPrivateMessages = active;
                        break;
                }
            }
        }

        private void RevertOptionsBack() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            foreach (var option in _options) {
                bool active = false;
                switch (option.Key) {
                    case Option.ShowInfoMessagesInConsole:
                        active = optionStorage.ShowInfoMessages;
                        break;
                    case Option.ShowEventMessagesInConsole:
                        active = optionStorage.ShowEventMessages;
                        break;
                    case Option.ShowStatusMessagesInConsole:
                        active = optionStorage.ShowStatusMessages;
                        break;
                    case Option.ShowStatusMessagesOfOthersInConsole:
                        active = optionStorage.ShowStatusMessagesOfOthers;
                        break;
                    case Option.ShowTimestampInConsole:
                        active = optionStorage.ShowTimestamps;
                        break;
                    case Option.ShowLevelsInConsle:
                        active = optionStorage.ShowLevels;
                        break;
                    case Option.ShowPrivateMessagesInGameWindow:
                        active = optionStorage.ShowPrivateMessages;
                        break;
                }

                option.Value.checkbox.Checked = active;
            }
        }
    }
}
