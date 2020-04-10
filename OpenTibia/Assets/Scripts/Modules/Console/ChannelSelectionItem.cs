using UnityEngine;

namespace OpenTibiaUnity.Modules.Console
{
    public class ChannelSelectionItem : UI.Legacy.ToggleListItem
    {
        // serialized fields
        [SerializeField]
        private TMPro.TextMeshProUGUI _channelName = null;

        // properties
        public string ChannelName {
            get => _channelName.text;
            set => _channelName.text = value;
        }

        public Core.Utils.UnionStrInt ChannelId { get; set; } = null;
    }
}
