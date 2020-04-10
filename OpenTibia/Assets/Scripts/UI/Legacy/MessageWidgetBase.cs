using UnityEngine;

using UnityUI = UnityEngine.UI;

namespace OpenTibiaUnity.UI.Legacy
{
    public class MessageWidgetBase : PopUpBase
    {
        // Serializable fields
        [SerializeField]
        protected TMPro.TextMeshProUGUI _message = null;

        // Fields
        private float _maxWidth = -1;
        private float _maxHeight = -1;

        // Properties
        public float maxWidth {
            get => _maxWidth;
            set {
                if (value != _maxWidth) {
                    _maxWidth = value;

                    RecalculateMessageBounds();
                }
            }
        }

        public float maxHeight {
            get => _maxHeight;
            set {
                if (value != _maxHeight) {
                    _maxHeight = value;
                    RecalculateMessageBounds();
                }
            }
        }

        public Vector2 maxSize {
            get => new Vector2(maxWidth, maxHeight);
            set {
                if (_maxWidth != value.x || _maxHeight != value.y) {
                    _maxWidth = value.x;
                    _maxHeight = value.y;
                    RecalculateMessageBounds();
                }
            }
        }

        public virtual string message {
            get => _message.text;
            set {
                _message.SetText(value);
                _message.ForceMeshUpdate(true);
                RecalculateMessageBounds();
            }
        }

        public TMPro.TextAlignmentOptions alignment {
            get => _message.alignment;
            set => _message.alignment = value;
        }

        protected virtual void RecalculateMessageBounds() {
            float width = maxWidth < 0 ? float.MaxValue : maxWidth;
            float height = maxHeight < 0 ? float.MaxValue : maxHeight;
            var preferedSize = _message.GetPreferredValues(width, height);
            var layoutElement = _message.GetComponent<UnityUI.LayoutElement>();
            layoutElement.preferredWidth = preferedSize.x;
            layoutElement.preferredHeight = preferedSize.y;
        }
    }
}
