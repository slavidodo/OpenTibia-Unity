using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    [DisallowMultipleComponent]
    internal class LegacyOptionsWindow : Core.Components.Base.Window
    {
        [SerializeField] private RectTransform m_PanelContent = null;
        [SerializeField] private Button m_OkButton = null;
        [SerializeField] private LegacyOptionsWindowItem LegacyOptionsWindowItemTemplate = null;
        [SerializeField] private LegacyOptionsWindowItem LegacyOptionsWindowItemGreenTemplate = null;

        protected override void Start() {
            base.Start();

            m_OkButton.onClick.AddListener(OnOkClick);

            ModulesManager.Instance.LegacyGeneralOptionsWindow.onClosed.AddListener(() => OpenWindow());

            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);

            SetupWithOptions();
        }

        private void OnClientVersionChange(int oldVersion, int newVersion) {
            foreach (Transform child in m_PanelContent) {
                Destroy(child.gameObject);
            }
            
            CreateOption("General", "Change general\ngame options", OpenGeneralOptions);
            CreateOption("Graphics", "Change graphics and performance settings", OpenGraphicsOptions);
            CreateOption("Console", "Customise the console", OpenConsoleOptions);
            CreateOption("Hotkeys", "Edit your hotkey texts", OpenHotkeyOptions);
            CreateSeparator();
            CreateOption("Motd", "Show the most recent message of the day.", ShowMOTD);

            if (newVersion >= 1010)
                CreateGreenOption("Get Premium", "Gain access to all premium features.", GetPremium);
        }
        
        private void OnOkClick() {
            CloseWindow();
        }

        void OpenGeneralOptions() {
            CloseWindow();
            ModulesManager.Instance.LegacyGeneralOptionsWindow.OpenWindow();
        }

        void OpenGraphicsOptions() {

        }

        void OpenConsoleOptions() {

        }

        void OpenHotkeyOptions() {

        }

        void ShowMOTD() {

        }

        void GetPremium() {

        }

        private LegacyOptionsWindowItem CreateOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(LegacyOptionsWindowItemTemplate, m_PanelContent);
            item.buttonWrapper.label.text = title;
            item.buttonWrapper.button.onClick.AddListener(callback);
            item.label.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private LegacyOptionsWindowItem CreateGreenOption(string title, string description, UnityEngine.Events.UnityAction callback) {
            var item = Instantiate(LegacyOptionsWindowItemGreenTemplate, m_PanelContent);
            item.buttonWrapper.label.text = title;
            item.buttonWrapper.button.onClick.AddListener(callback);
            item.label.text = description;
            item.gameObject.SetActive(true);
            return item;
        }

        private void CreateSeparator() {
            var separator = Instantiate(OpenTibiaUnity.GameManager.HorizontalSeparator, m_PanelContent);
            var layoutElement = separator.AddComponent<LayoutElement>();
            layoutElement.minHeight = 2;
        }

        private void SetupWithOptions() {

        }
    }
}
