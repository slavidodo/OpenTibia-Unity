using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.Modules.Login
{
    public class PremiumBenefitPanel : UI.Legacy.BasicElement
    {
        // serialized fields
        [SerializeField]
        private UnityUI.Image _image = null;

        [SerializeField]
        private TMPro.TextMeshProUGUI _label = null;

        public Sprite sprite {
            get => _image.sprite;
            set => _image.sprite = value;
        }

        public string text {
            get => _label.text;
            set => _label.text = value;
        }
    }
}
