namespace OpenTibiaUnity.Core.Magic
{
    public class SpellStorage
    {
        public static Rune[] Runes;
        public static Spell[] Spells;

        static SpellStorage() {
            Runes = new Rune[] { };
            Spells = new Spell[] { };
        }

        public void Reset() {
            return;
        }

        public static bool CheckRune(int id) {
            return GetRune(id) != null;
        }

        public static Rune GetRune(int id) {
            int lastIndex = Runes.Length - 1;

            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var rune = Runes[tmpIndex];
                if (rune._id < id)
                    index = tmpIndex + 1;
                else if (rune._id > id)
                    lastIndex = tmpIndex - 1;
                else
                    return rune;
            }

            return null;
        }

        public static Spell GetSpell(int id) {
            int lastIndex = Spells.Length - 1;

            int index = 0;
            while (index <= lastIndex) {
                int tmpIndex = index + lastIndex >> 1;
                var spell = Spells[tmpIndex];
                if (spell._id < id)
                    index = tmpIndex + 1;
                else if (spell._id > id)
                    lastIndex = tmpIndex - 1;
                else
                    return spell;
            }

            return null;
        }
    }
}
