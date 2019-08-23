namespace OpenTibiaUnity.Core.Utils
{
    public class Delay
    {
        public int Start;
        public int End;

        public Delay(int start, int end) {
            Start = start;
            End = end;
        }

        public static Delay Merge(Delay a, Delay b) {
            if (a == null && b == null)
                return new Delay(0, 0);

            if (a == null)
                return new Delay(b.Start, b.End);

            if (b == null)
                return new Delay(a.Start, a.End);

            if (a.End < b.Start || a.Start >= b.Start && a.End <= b.End)
                return new Delay(b.Start, b.End);

            if (a.Start > b.End || a.Start <= b.Start && a.End >= b.End)
                return new Delay(a.Start, a.End);

            if (a.Start < b.Start && a.End >= b.Start)
                return new Delay(a.Start, b.End);

            if (a.Start <= b.End && a.End > b.End)
                return new Delay(b.Start, a.End);

            throw new System.Exception("Delay.Merge: Can't merge delays.");
        }

        public int Duration {
            get {
                return End - Start;
            }
        }

        public bool Containes(int t) {
            return t >= Start && t <= End;
        }

        public Delay Clone() {
            return new Delay(Start, End);
        }
    }
}
