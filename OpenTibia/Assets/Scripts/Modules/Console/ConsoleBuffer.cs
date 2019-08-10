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
    internal class ConsoleBuffer : Core.Components.Base.AbstractComponent, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private ConsoleModule m_ConsoleModule = null;

        private TMPro.TMP_InputField m_TextLabel;
        internal TMPro.TMP_InputField textInput {
            get {
                if (m_TextLabel)
                    return m_TextLabel;

                m_TextLabel = GetComponent<TMPro.TMP_InputField>();
                return m_TextLabel;
            }
        }

        private List<ChannelMessage> m_History = null;

        protected override void Awake() {
            base.Awake();

            m_History = new List<ChannelMessage>();
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
            int linkID;
            if (!int.TryParse(linkInfo.GetLinkID(), out linkID))
                return;
            
            var linkText = linkInfo.GetLinkText();
            var channelMessage = m_History.Find((x) => x.GetHashCode() == linkID);

            if (eventData.button == PointerEventData.InputButton.Left) {
                
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                var mousePosition = new Vector3(eventData.position.x, Screen.height - eventData.position.y);
                m_ConsoleModule.CreateChannelMessageContextMenu(channelMessage, textInput).Display(mousePosition);
            }
        }

        internal void AddChannelMessage(ChannelMessage channelMessage) {
            // check for history
            // we can support up to 200 message, afterwhich we keep removing
            string text = textInput.text;
            if (m_History.Count >= Constants.MaxTalkHistory) {
                var oldMessage = m_History[0];
                m_History.RemoveAt(0);

                var hashCode = oldMessage.GetHashCode();
                var linkInfo = System.Array.Find(textInput.textComponent.textInfo.linkInfo, (link) => link.GetLinkID() == hashCode.ToString());
                text = text.Substring(linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength);
            }

            m_History.Add(channelMessage);
            if (text.Length > 0)
                text += '\n';
            text += FormatChannelMessage(channelMessage);
            textInput.text = text;
        }

        internal void ResetTalkHistory(IEnumerable<ChannelMessage> talkHistory) {
            m_History = talkHistory.ToList();

            string text = "";
            foreach (var channelMessage in m_History) {
                if (text.Length > 0)
                    text += '\n';
                text += FormatChannelMessage(channelMessage);
            }
            textInput.text = text;
        }

        internal string FormatChannelMessage(ChannelMessage channelMessage) {
            return string.Format("<link=\"{0}\">{1}</link>", channelMessage.GetHashCode().ToString(), channelMessage.RichText);
        }
    }
}
