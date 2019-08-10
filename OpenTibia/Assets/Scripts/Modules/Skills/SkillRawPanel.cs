using UnityEngine;

namespace OpenTibiaUnity.Modules.Skills
{
    public class SkillRawPanel : SkillPanel
    {
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelText = null;
        [SerializeField] private TMPro.TextMeshProUGUI m_LabelValue = null;

        internal override TMPro.TextMeshProUGUI labelText { get => m_LabelText; }
        internal override TMPro.TextMeshProUGUI labelValue { get => m_LabelValue; }

        public override void SetText(string text) => m_LabelText.SetText(text);
        public override void SetValue(long value) => SetValueInternal(Utility.Commafy(value));

        internal void SetValueInternal(string value) => m_LabelValue.SetText(value);
    }
}
