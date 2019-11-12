using UnityEngine;
using UnityEngine.UI;

using TR = OpenTibiaUnity.TextResources;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    public class LegacyOptionsWindow : Core.Components.Base.Window
    {
        [SerializeField] private RectTransform _panelContent = null;
        [SerializeField] private Button _okButton = null;
        [SerializeField] private LegacyOptionsWindowItem LegacyOptionsWindowItemTemplate = null;
        [SerializeField] private LegacyOptionsWindowItem LegacyOptionsWindowItemGreenTemplate = null;

        protected override void Start() {
            base.Start();

            _okButton.onClick.AddListener(OnOkClick);

            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);

            SetupWithOptions();
        }

        private void OnClientVersionChange(int oldVersion, int newVersion) {
            foreach (Transform child in _panelContent)
                Destroy(child.gameObject);

            CreateOption(TR.LEGACYOPTIONS_WINDOW_GENERAL_TEXT, TR.LEGACYOPTIONS_WINDOW_GENERAL_DESCRIPTION, OpenGeneralOptions);
            CreateOption(TR.LEGACYOPTIONS_WINDOW_GRAPHICS_TEXT, TR.LEGACYOPTIONS_WINDOW_GRAPHICS_DESCRIPTION, OpenGraphicsOptions);
            CreateOption(TR.LEGACYOPTIONS_WINDOW_CONSOLE_TEXT, TR.LEGACYOPTIONS_WINDOW_CONSOLE_DESCRIPTION, OpenConsoleOptions);
            CreateOption(TR.LEGACYOPTIONS_WINDOW_HOTKEYS_TEXT, TR.LEGACYOPTIONS_WINDOW_HOTKEYS_DESCRIPTION, OpenHotkeyOptions);
            CreateSeparator();
            CreateOption(TR.LEGACYOPTIONS_WINDOW_MOTD_TEXT, TR.LEGACYOPTIONS_WINDOW_MOTD_DESCRIPTION, ShowMOTD);

            if (newVersion >= 1010) {
                CreateGreenOption(TextResources.LEGACYOPTIONS_WINDOW_GETPREMIUM_TEXT, TextResources.LEGACYOPTIONS_WINDOW_GETPREMIUM_DESCRIPTION, GetPremium);
            }
        }
        
        private void OnOkClick() {
            Close();
        }

        private void OnGeneralWindowClosed() {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyGeneralOptionsWindow, OnGeneralWindowClosed);
        }

        private void OnGraphicsWindowClosed() {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyGraphicsOptionWindow, OnGraphicsWindowClosed);
        }

        private void OnConsoleWindowClosed() {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyConsoleOptionsWindow, OnConsoleWindowClosed);
        }

        private void OnHotkeysWindowClosed() {
            SelfOpenAndRevokeListener(ModulesManager.Instance.HotkeysWindow, OnHotkeysWindowClosed);
        }
        
        void SelfOpenAndRevokeListener(Core.Components.Base.Window window, UnityEngine.Events.UnityAction closeCallback) {
            Open();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() =>
                window.onClosed.RemoveListener(closeCallback)
            );
        }

        void SelfCloseAndBindListner(Core.Components.Base.Window window, UnityEngine.Events.UnityAction closeCallback) {
            Close();
            window.Open();
            window.onClosed.AddListener(closeCallback);
        }

        void OpenGeneralOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyGeneralOptionsWindow, OnGeneralWindowClosed);
        }

        void OpenGraphicsOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyGraphicsOptionWindow, OnGraphicsWindowClosed);
        }

        void OpenConsoleOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyConsoleOptionsWindow, OnConsoleWindowClosed);
        }

        void OpenHotkeyOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.HotkeysWindow, OnHotkeysWindowClosed);
        }

        void ShowMOTD() {

        }

        void GetPremium() {

        }

        private LegacyOptionsWindowItem CreateOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(LegacyOptionsWindowItemTemplate, _panelContent);
            item.buttonWrapper.label.text = title;
            item.buttonWrapper.button.onClick.AddListener(callback);
            item.label.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private LegacyOptionsWindowItem CreateGreenOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(LegacyOptionsWindowItemGreenTemplate, _panelContent);
            item.buttonWrapper.label.text = title;
            item.buttonWrapper.button.onClick.AddListener(callback);
            item.label.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private void CreateSeparator() {
            var separator = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, _panelContent);
            var layoutElement = separator.AddComponent<LayoutElement>();
            layoutElement.minHeight = 2;
        }

        private void SetupWithOptions() {

        }
    }
}
