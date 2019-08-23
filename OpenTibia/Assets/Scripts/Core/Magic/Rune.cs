namespace OpenTibiaUnity.Core.Magic
{
    public class Rune
    {
        public int _id { get; private set; }
        public int RestrictLevel { get; private set; }
        public int RestrictMagicLevel { get; private set; }
        public int RestrictProfession { get; private set; }
        public int CastMana { get; private set; }

        public Rune(int id, int level, int magLevel, int profession, int castMana) {
            _id = id;
            RestrictLevel = level;
            RestrictMagicLevel = magLevel;
            RestrictProfession = profession;
            CastMana = castMana;
        }
    }
}
