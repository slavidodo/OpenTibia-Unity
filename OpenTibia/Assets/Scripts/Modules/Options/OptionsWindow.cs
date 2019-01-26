using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    public sealed class OptionsWindow : Core.Components.Base.Window
    {
        public const string OptionsFileName = "Settings.json";

        [SerializeField] private RectTransform m_BasicTabbarRectTransform;
        [SerializeField] private RectTransform m_AdvancedTabbarRectTransform;
        [SerializeField] private RectTransform m_ContentPanelRectTransform;
        [SerializeField] private Button m_OkButton;
        [SerializeField] private Button m_ApplyButton;
        [SerializeField] private Button m_CancelButton;

        private Core.Options.OptionStorage m_OptionStorage;
        private Core.Options.OptionStorage m_TmpOptionStorage;
        
        private void OnOkPressed() {
            ApplyTemporaryOptions();
            CloseOptionsWindow();
        }

        private void OnApplyPressed() {
            ApplyTemporaryOptions();
        }

        private void OnCancelPressed() {
            ResetPanelToActual();
            CloseOptionsWindow();
        }

        public void ApplyTemporaryOptions() {
        }

        private void ResetPanelToActual() {
            var optionStorage = OpenTibiaUnity.OptionStorage;
        }

        private void CloseOptionsWindow() {
            UnlockFromOverlay();
            gameObject.SetActive(false);
        }
    }
}
