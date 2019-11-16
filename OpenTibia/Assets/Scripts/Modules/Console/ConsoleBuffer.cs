using OpenTibiaUnity.Core.Chat;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OpenTibiaUnity.Modules.Console
{
    // TODO
    // we should find a way to cancel onScroll being called on the textInput
    // for some reason it's bugged and no matter what you do
    // to the scrollbar / scrollrect it's reversed (top-to-bottom instead of bottom-to-top)
    // hence affecting the placeholder
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class ConsoleBuffer : Core.Components.Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ConsoleModule _consoleModule = null;

        private TMPro.TMP_InputField _textLabel;
        public TMPro.TMP_InputField textInput {
            get {
                if (_textLabel)
                    return _textLabel;

                _textLabel = GetComponent<TMPro.TMP_InputField>();
                return _textLabel;
            }
        }

        private List<ChannelMessage> _history = null;

        protected override void Awake() {
            base.Awake();

            _history = new List<ChannelMessage>();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
            // for some reason, to handle pointer up in labels
            // you have to handle pointerDown
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
            int linkIndex = TMPro.TMP_TextUtilities.FindIntersectingLink(textInput.textComponent, eventData.position, eventData.pressEventCamera);
            if (linkIndex == -1)
                return;

            var linkInfo = textInput.textComponent.textInfo.linkInfo[linkIndex];
            int linkId;
            if (!int.TryParse(linkInfo.GetLinkID(), out linkId))
                return;
            
            var linkText = linkInfo.GetLinkText();
            var channelMessage = _history.Find((x) => x.GetHashCode() == linkId);

            if (eventData.button == PointerEventData.InputButton.Left) {
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                var mousePosition = new Vector3(eventData.position.x, Screen.height - eventData.position.y);
                _consoleModule.CreateChannelMessageContextMenu(channelMessage, textInput).Display(mousePosition);
            }
        }

        public void AddChannelMessage(ChannelMessage channelMessage) {
            // check for history
            // we can support up to 200 message, afterwhich we keep removing
            string text = textInput.text;
            if (_history.Count >= Constants.MaxTalkHistory) {
                var oldMessage = _history[0];
                _history.RemoveAt(0);

                var hashCode = oldMessage.GetHashCode();
                var linkInfo = System.Array.Find(textInput.textComponent.textInfo.linkInfo, (link) => link.GetLinkID() == hashCode.ToString());
                text = text.Substring(linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength);
            }

            _history.Add(channelMessage);
            if (text.Length > 0)
                text += '\n';
            text += FormatChannelMessage(channelMessage);
            textInput.text = text;
        }

        public void ResetChannelHistory(IEnumerable<ChannelMessage> talkHistory) {
            _history = talkHistory.ToList();

            string text = "";
            foreach (var channelMessage in _history) {
                if (text.Length > 0)
                    text += '\n';
                text += FormatChannelMessage(channelMessage);
            }
            textInput.text = text;
        }

        public string FormatChannelMessage(ChannelMessage channelMessage) {
            return string.Format("<link=\"{0}\">{1}</link>", channelMessage.GetHashCode().ToString(), channelMessage.RichText);
        }
    }
}
