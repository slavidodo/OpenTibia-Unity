using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    public class LegacyGraphicsOptionsWindow : Core.Components.Base.Window
    {
        [SerializeField] private TMPro.TMP_Dropdown _resolutionDropdown = null;
        [SerializeField] private TMPro.TMP_Dropdown _qualityDropdown = null;
        [SerializeField] private Core.Components.CheckboxWrapper _fullscreenCheckboxWrapper = null;
        [SerializeField] private Core.Components.CheckboxWrapper _vsyncCheckboxWrapper = null;
        [SerializeField] private Core.Components.CheckboxWrapper _antialiasingCheckboxWrapper = null;
        [SerializeField] private Core.Components.CheckboxWrapper _noFramerateLimitCheckboxWrapper = null;
        [SerializeField] private Core.Components.SliderWrapper _framerateLimitSliderWrapper = null;
        [SerializeField] private Core.Components.CheckboxWrapper _showLightEffectsCheckboxWrapper = null;
        [SerializeField] private Core.Components.SliderWrapper _ambientLightSliderWrapper = null;

        [SerializeField] private Button _okButton = null;
        [SerializeField] private Button _cancelButton = null;

        protected override void Awake() {
            base.Awake();

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

            _noFramerateLimitCheckboxWrapper.onValueChanged.AddListener(OnNoFramerateLimitValueChanged);
            _framerateLimitSliderWrapper.SetMinMax(Constants.MinimumManageableFramerate, Constants.MaximumManageableFramerate);

            _showLightEffectsCheckboxWrapper.onValueChanged.AddListener(OnShowLightEffectsValueChanged);
            _ambientLightSliderWrapper.SetMinMax(0, 100);

            RevertOptionsBack();
        }

        protected override void Start() {
            base.Start();

            _okButton.onClick.AddListener(OnOkClick);
            _cancelButton.onClick.AddListener(OnCancelClick);
        }

        private void OnOkClick() {
            UpdateOptionStorage();
            Close();
            ModulesManager.Instance.LegacyOptionsWindow.Open();
        }

        private void OnCancelClick() {
            Close();
            RevertOptionsBack();
            ModulesManager.Instance.LegacyOptionsWindow.Open();
        }

        private void OnNoFramerateLimitValueChanged(bool value) {
            _framerateLimitSliderWrapper.SetEnabled(!value);
        }

        private void OnShowLightEffectsValueChanged(bool value) {
            _ambientLightSliderWrapper.SetEnabled(value);
        }

        private void UpdateOptionStorage() {
            var optionStorage = OpenTibiaUnity.OptionStorage;

            optionStorage.GameResolutionIndex = _resolutionDropdown.value;
            optionStorage.GameQualityLevel = _qualityDropdown.value;

            bool antialiasing = _antialiasingCheckboxWrapper.checkbox.Checked;
            optionStorage.GameAntialiasingMode = antialiasing ? AntialiasingMode.Antialiasing : AntialiasingMode.None;
            optionStorage.FullscreenMode = _fullscreenCheckboxWrapper.checkbox.Checked;
            optionStorage.VsyncEnabled = _vsyncCheckboxWrapper.checkbox.Checked;
            optionStorage.NoFramerateLimit = _noFramerateLimitCheckboxWrapper.checkbox.Checked;
            optionStorage.FramerateLimit = (int)_framerateLimitSliderWrapper.slider.value;
            optionStorage.ShowLightEffects = _showLightEffectsCheckboxWrapper.checkbox.Checked;
            optionStorage.AmbientBrightness = (int)_ambientLightSliderWrapper.slider.value;

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
            _fullscreenCheckboxWrapper.checkbox.Checked = optionStorage.FullscreenMode;
            _vsyncCheckboxWrapper.checkbox.Checked = optionStorage.VsyncEnabled;
            _antialiasingCheckboxWrapper.checkbox.Checked = optionStorage.GameAntialiasingMode != AntialiasingMode.None;

            _noFramerateLimitCheckboxWrapper.checkbox.Checked = optionStorage.NoFramerateLimit;
            _framerateLimitSliderWrapper.SetEnabled(!optionStorage.NoFramerateLimit);
            _framerateLimitSliderWrapper.slider.value = optionStorage.FramerateLimit;
            _framerateLimitSliderWrapper.ForceUpdateLabel();

            _showLightEffectsCheckboxWrapper.checkbox.Checked = optionStorage.ShowLightEffects;
            _ambientLightSliderWrapper.SetEnabled(optionStorage.ShowLightEffects);
            _ambientLightSliderWrapper.slider.value = optionStorage.AmbientBrightness;
            _ambientLightSliderWrapper.ForceUpdateLabel();
        }
    }
}