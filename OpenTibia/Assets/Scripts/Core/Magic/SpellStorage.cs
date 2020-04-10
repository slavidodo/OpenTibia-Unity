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
            int l = 0, r = Runes.Length - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var rune = Runes[i];
                if (rune._id < id)
                    l = i + 1;
                else if (rune._id > id)
                    r = i - 1;
                else
                    return rune;
            }

            return null;
        }

        public static Spell GetSpell(int id) {
            int l = 0, r = Spells.Length - 1;
            while (l <= r) {
                int i = l + r >> 1;
                var spell = Spells[i];
                if (spell._id < id)
                    l = i + 1;
                else if (spell._id > id)
                    r = i - 1;
                else
                    return spell;
            }

            return null;
        }
    }
}
