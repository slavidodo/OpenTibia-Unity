using OpenTibiaUnity.Core.Chat;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Modules.Console
{
    public class ChannelMessageLabel : UIBehaviour
    {
        public Channel Channel;
        public ChannelMessage ChannelMessage;

        public void SetText(string text) {
            GetComponent<TMPro.TextMeshProUGUI>().text = text;
        }
    }
}
