namespace OpenTibiaUnity.Core.Appearances
{
    public class TextualEffectInstance : AppearanceInstance
    {
        private int m_Phase = 0;
        private int m_LastPhaseChange;
        private string m_Value = null;
        private UnityEngine.Color m_Color;

        public TextualEffectInstance(int color, int value) : base(0, null) {
            m_Color = Colors.ColorFrom8Bit(color);
            m_Value = value.ToString();
            m_LastPhaseChange = OpenTibiaUnity.TicksMillis;
        }

        public TextualEffectInstance(int color, string value) : base(0, null) {
            m_Color = Colors.ColorFrom8Bit(color);
            m_Value = value;
            m_LastPhaseChange = OpenTibiaUnity.TicksMillis;
        }

        public bool Merge(AppearanceInstance other) {
            var textualEffect = other as TextualEffectInstance;
            if (!!textualEffect && textualEffect.m_Phase <= 0 && m_Phase <= 0 && textualEffect.m_Color != m_Color) {
                m_Value = textualEffect.m_Value;
                return true;
            }

            return false;
        }

        public override bool Animate(int ticks, int delay = 0) {
            return false;
        }

        public int Width {
            get { return 32; }
        }

        public int Height {
            get { return 32; }
        }
    }
}
