using UnityEngine;

namespace OpenTibiaUnity.Modules.Login
{
    public class AccountCharacter : UI.Legacy.ToggleListItem
    {
        // serialized fields
        [SerializeField]
        private TMPro.TextMeshProUGUI _characterName = null;
        [SerializeField]
        private TMPro.TextMeshProUGUI _worldName = null;

        // properties
        public string CharacterName {
            get => _characterName.text;
            set => _characterName.text = value;
        }

        public string WorldName {
            get => _worldName.text;
            set => _worldName.text = value;
        }
    }
}
