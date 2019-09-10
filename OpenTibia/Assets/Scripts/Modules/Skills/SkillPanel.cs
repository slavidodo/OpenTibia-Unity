namespace OpenTibiaUnity.Modules.Skills
{
    public abstract class SkillPanel : Core.Components.Base.AbstractComponent
    {

        public abstract TMPro.TextMeshProUGUI labelText { get; }
        public abstract TMPro.TextMeshProUGUI labelValue { get; }

        public virtual void SetProgressColor(UnityEngine.Color color) { }
        public virtual void SetIcon(UnityEngine.Sprite icon) { }
        public abstract void SetText(string text);
        public abstract void SetValue(long value);
        public abstract void SetValue(string value);
        public virtual void SetValue(long value, float percent) => SetValue(value);
        public virtual void SetValue(string value, float percent) => SetValue(value);
    }
}
