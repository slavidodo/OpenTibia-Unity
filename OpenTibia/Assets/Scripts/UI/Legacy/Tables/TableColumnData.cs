namespace OpenTibiaUnity.UI.Legacy.Tables
{
    public struct TableColumnData
    {
        public string text;
        public TableColumn colStyle;
        public object sortValue;

        // optional
        public int width;

        public TableColumnData(string text, TableColumn colStyle = null, int width = -1, object sortValue = null) {
            this.text = text;
            this.colStyle = colStyle;
            this.width = width;
            this.sortValue = sortValue;
        }

        public TableColumnData(string text, int width = -1) {
            this.text = text;
            this.colStyle = null;
            this.width = width;
            this.sortValue = text;
        }
    }
}
