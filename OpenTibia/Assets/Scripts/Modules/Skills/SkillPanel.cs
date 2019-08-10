namespace OpenTibiaUnity.Modules.Skills
{
    public abstract class SkillPanel : Core.Components.Base.AbstractComponent
    {

        internal abstract TMPro.TextMeshProUGUI labelText { get; }
        internal abstract TMPro.TextMeshProUGUI labelValue { get; }

        public virtual void SetProgressColor(UnityEngine.Color color) { }
        public virtual void SetIcon(UnityEngine.Sprite icon) { }
        public abstract void SetText(string text);
        public abstract void SetValue(long value);
        public virtual void SetValue(long value, float percent) => SetValue(value);
    }
}
