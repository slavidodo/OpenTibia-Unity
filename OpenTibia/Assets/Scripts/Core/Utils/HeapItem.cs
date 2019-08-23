namespace OpenTibiaUnity.Core.Utils
{
    public class HeapItem
    {
        public int HeapKey { get; set; } = 0;
        public int HeapPosition { get; set; } = 0;
        public Heap HeapParent { get; set; } = null;

        public void Reset() {
            HeapKey = 0;
            HeapPosition = 0;
            HeapParent = null;
        }
    }
}