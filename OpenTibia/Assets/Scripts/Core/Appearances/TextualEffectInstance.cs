namespace OpenTibiaUnity.Core.Appearances
{
    public class TextualEffectInstance : AppearanceInstance
    {
        private int m_Phase = 0;
        private int m_LastPhaseChange;
        private int m_Value = 0;
        private UnityEngine.Color m_Color;

        public TextualEffectInstance(int color, int value) : base(0, null) {
            m_Color = Colors.ColorFrom8Bit(color);
            m_Value = value;
            m_LastPhaseChange = OpenTibiaUnity.TicksMillis;
        }

        public bool Merge(AppearanceInstance other) {
            var textualEffect = other as TextualEffectInstance;
            if (!!textualEffect && textualEffect.m_Phase <= 0 && this.m_Phase <= 0 && textualEffect.m_Color != m_Color) {
                m_Value = textualEffect.m_Value;
                return true;
            }

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
