using UnityEngine;

using UnityUI = UnityEngine.UI;
using TR = OpenTibiaUnity.TextResources;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    public class LegacyOptionsWidget : UI.Legacy.PopUpBase
    {
        [SerializeField]
        private LegacyOptionsWidgetItem _legacyOptionsWidgetItemTemplate = null;
        [SerializeField]
        private LegacyOptionsWidgetItem _legacyOptionsWidgetItemGreenTemplate = null;

        protected override void Awake() {
            base.Awake();

            AddButton(UI.Legacy.PopUpButtonMask.Ok);
        }

        protected override void Start() {
            base.Start();

            SetupWithOptions();
        }

        protected override void OnClientVersionChange(int oldVersion, int newVersion) {
            foreach (Transform child in _content)
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

        private void OnGeneralWindowClosed(UI.Legacy.PopUpBase _) {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyGeneralOptionsWidget, OnGeneralWindowClosed);
        }

        private void OnGraphicsWindowClosed(UI.Legacy.PopUpBase _) {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyGraphicsOptionWidget, OnGraphicsWindowClosed);
        }

        private void OnConsoleWindowClosed(UI.Legacy.PopUpBase _) {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyConsoleOptionsWidget, OnConsoleWindowClosed);
        }

        private void OnHotkeysWindowClosed(UI.Legacy.PopUpBase _) {
            SelfOpenAndRevokeListener(ModulesManager.Instance.LegacyHotkeyOptionsWidget, OnHotkeysWindowClosed);
        }
        
        void SelfOpenAndRevokeListener(UI.Legacy.PopUpBase popUp, UnityEngine.Events.UnityAction<UI.Legacy.PopUpBase> closeCallback) {
            Show();
            OpenTibiaUnity.GameManager.InvokeOnMainThread(() =>
                popUp.onClose.RemoveListener(closeCallback)
            );
        }

        void SelfCloseAndBindListner(UI.Legacy.PopUpBase popUp, UnityEngine.Events.UnityAction<UI.Legacy.PopUpBase> closeCallback) {
            Hide();
            popUp.Show();
            popUp.onClose.AddListener(closeCallback);
        }

        void OpenGeneralOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyGeneralOptionsWidget, OnGeneralWindowClosed);
        }

        void OpenGraphicsOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyGraphicsOptionWidget, OnGraphicsWindowClosed);
        }

        void OpenConsoleOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyConsoleOptionsWidget, OnConsoleWindowClosed);
        }

        void OpenHotkeyOptions() {
            SelfCloseAndBindListner(ModulesManager.Instance.LegacyHotkeyOptionsWidget, OnHotkeysWindowClosed);
        }

        void ShowMOTD() {

        }

        void GetPremium() {

        }

        private LegacyOptionsWidgetItem CreateOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(_legacyOptionsWidgetItemTemplate, _content);
            item.button.text = title;
            item.button.onClick.AddListener(callback);
            item.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private LegacyOptionsWidgetItem CreateGreenOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(_legacyOptionsWidgetItemGreenTemplate, _content);
            item.button.text = title;
            item.button.onClick.AddListener(callback);
            item.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private void CreateSeparator() {
            var separator = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, _content);
            var layoutElement = separator.AddComponent<UnityUI.LayoutElement>();
            layoutElement.minHeight = 2;
        }

        private void SetupWithOptions() {

        }
    }
}
