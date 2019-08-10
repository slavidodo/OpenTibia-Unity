using UnityEngine;
using UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Options
{
    internal class LegacyGeneralOptionsWindow : Core.Components.Base.Window
    {
        [SerializeField] private RectTransform m_PanelContent;
        [SerializeField] private Button m_OkButton;
        [SerializeField] private Button m_CancelButton;

        protected override void Start() {
            base.Start();

            m_OkButton.onClick.AddListener(OnOkClick);
            m_CancelButton.onClick.AddListener(OnCancelClick);

            OpenTibiaUnity.GameManager.onClientVersionChange.AddListener(OnClientVersionChange);
            if (OpenTibiaUnity.GameManager.ClientVersion != 0)
                OnClientVersionChange(0, OpenTibiaUnity.GameManager.ClientVersion);
        }

        private void OnOkClick() {

        }

        private void OnCancelClick() {

        }

        private void OnClientVersionChange(int oldVersion, int newVersion) {
            foreach (Transform child in m_PanelContent) {
                Destroy(child.gameObject);
            }

            CreateOption("Tibia Classic Control");
            CreateOption("Auto Chase Off");

            if (newVersion < 810)
                CreateOption("Show Hints");

            CreateOption("Show Names of Creatures");
            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameCreatureMarks)) {
                CreateOption("Show Marks on Creatures");
                CreateOption("Show PvP Frames on Creatures");
            }

            if (OpenTibiaUnity.GameManager.GetFeature(GameFeature.GameCreatureIcons)) {
                CreateOption("Show Icons on NPCs");
            }

            CreateOption("Show Textual Effects");
            if (newVersion > 870) {
                CreateOption("Show Cooldown Bar");
            }

            if (newVersion >= 1055) {
                CreateOption("Auto-Switch Hotkey Preset");
            }
        }

        private Core.Components.CheckboxWrapper CreateOption(string text) {
            var checkboxWrapper = Instantiate(OpenTibiaUnity.GameManager.PanelCheckBox, m_PanelContent);
            checkboxWrapper.label.text = text;
            
            return checkboxWrapper;
        }
    }
}
