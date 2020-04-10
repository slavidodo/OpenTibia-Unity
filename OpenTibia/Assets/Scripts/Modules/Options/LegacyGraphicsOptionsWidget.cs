using System.Collections.Generic;
using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    public class LegacyGraphicsOptionsWidget : UI.Legacy.PopUpBase
    {
        [SerializeField]
        private TMPro.TMP_Dropdown _resolutionDropdown = null;
        [SerializeField]
        private TMPro.TMP_Dropdown _qualityDropdown = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _fullscreenCheckbox = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _vsyncCheckbox = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _antialiasingCheckbox = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _noFramerateLimitCheckbox = null;
        [SerializeField]
        private UI.Legacy.Slider _framerateLimitSlider = null;
        [SerializeField]
        private UI.Legacy.CheckboxPanel _showLightEffectsCheckbox = null;
        [SerializeField]
        private UI.Legacy.Slider _ambientLightSlider = null;

        protected override void Awake() {
            base.Awake();

            AddButton(UI.Legacy.PopUpButtonMask.Ok, OnOkClick);
            AddButton(UI.Legacy.PopUpButtonMask.Cancel, OnCancelClick);

            var resolutions = new List<string>();
            int currentResolutionIndex = 0;
            var currentResolution = Screen.currentResolution;
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                var resolution = Screen.resolutions[i];
                resolutions.Add($"{resolution.width}x{resolution.height}");

                if (currentResolution.width == resolution.width && currentResolution.height == resolution.height)
                    currentResolutionIndex = i;
            }

            _resolutionDropdown.AddOptions(resolutions);
            _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            _resolutionDropdown.value = currentResolutionIndex;
            _qualityDropdown.value = QualitySettings.GetQualityLevel();

            _noFramerateLimitCheckbox.onValueChanged.AddListener(OnNoFramerateLimitValueChanged);
            _framerateLimitSlider.SetMinMax(Constants.MinimumManageableFramerate, Constants.MaximumManageableFramerate);

            _showLightEffectsCheckbox.onValueChanged.AddListener(OnShowLightEffectsValueChanged);
            _ambientLightSlider.SetMinMax(0, 100);

            RevertOptionsBack();
        }

        private void OnOkClick() {
            UpdateOptionStorage();
            ModulesManager.Instance.LegacyOptionsWidget.Show();
        }

        private void OnCancelClick() {
            RevertOptionsBack();
            ModulesManager.Instance.LegacyOptionsWidget.Show();
        }

        private void OnNoFramerateLimitValueChanged(bool value) {
            _framerateLimitSlider.SetEnabled(!value);
        }

        private void OnShowLightEffectsValueChanged(bool value) {
            _ambientLightSlider.SetEnabled(value);
        }

        private void UpdateOptionStorage() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            optionStorage.GameResolutionIndex = _resolutionDropdown.value;
            optionStorage.GameQualityLevel = _qualityDropdown.value;

            bool antialiasing = _antialiasingCheckbox.checkbox.Checked;
            optionStorage.GameAntialiasingMode = antialiasing ? AntialiasingMode.Antialiasing : AntialiasingMode.None;
            optionStorage.FullscreenMode = _fullscreenCheckbox.checkbox.Checked;
            optionStorage.VsyncEnabled = _vsyncCheckbox.checkbox.Checked;
            optionStorage.NoFramerateLimit = _noFramerateLimitCheckbox.checkbox.Checked;
            optionStorage.FramerateLimit = (int)_framerateLimitSlider.value;
            optionStorage.ShowLightEffects = _showLightEffectsCheckbox.checkbox.Checked;
            optionStorage.AmbientBrightness = (int)_ambientLightSlider.value;

            optionStorage.UpdateQualitySettings();
            optionStorage.UpdateFullscreenMode();
        }

        private void RevertOptionsBack() {
            // resolution & quality
            int currentResolutionIndex = 0;
            var currentResolution = Screen.currentResolution;
            for (int i = 0; i < Screen.resolutions.Length; i++) {
                var resolution = Screen.resolutions[i];
                if (currentResolution.width == resolution.width && currentResolution.height == resolution.height) {
                    currentResolutionIndex = i;
                    break;
                }
            }

            _resolutionDropdown.value = currentResolutionIndex;
            _qualityDropdown.value = QualitySettings.GetQualityLevel();

            var optionStorage = OpenTibiaUnity.OptionStorage;
            _fullscreenCheckbox.checkbox.Checked = optionStorage.FullscreenMode;
            _vsyncCheckbox.checkbox.Checked = optionStorage.VsyncEnabled;
            _antialiasingCheckbox.checkbox.Checked = optionStorage.GameAntialiasingMode != AntialiasingMode.None;

            _noFramerateLimitCheckbox.checkbox.Checked = optionStorage.NoFramerateLimit;
            _framerateLimitSlider.SetEnabled(!optionStorage.NoFramerateLimit);
            _framerateLimitSlider.value = optionStorage.FramerateLimit;
            _framerateLimitSlider.ForceUpdateLabel();

            _showLightEffectsCheckbox.checkbox.Checked = optionStorage.ShowLightEffects;
            _ambientLightSlider.SetEnabled(optionStorage.ShowLightEffects);
            _ambientLightSlider.value = optionStorage.AmbientBrightness;
            _ambientLightSlider.ForceUpdateLabel();
        }
    }
}