using System.Collections.Generic;

namespace OpenTibiaUnity.UI.Legacy.Tables
{
    [UnityEngine.RequireComponent(typeof(ScrollRect))]
    public class Table : BasicElement
    {
        // styling fields
        public TableRow tableRowStyle = null;
        public TableColumn tableColumnStyle = null;
        public TableHeaderRow tableHeaderRowStyle = null;
        public TableHeaderColumn tableHeaderColumnStyle = null;

        public int defaultColumnWidth = 80;

        // easy-access
        private ScrollRect _scrollRect = null;
        public ScrollRect scrollRect {
            get {
                if (!_scrollRect)
                    _scrollRect = GetComponent<ScrollRect>();
                return _scrollRect;
            }
        }

        public void AddRow(IEnumerable<TableColumnData> data, int height = 0) {
            var contentPanel = scrollRect.content;
            var row = Instantiate(tableRowStyle, contentPanel);
            row.name = $"row{contentPanel.childCount}";

            int index = 0;
            foreach (var columnData in data)
                AddRowData(row, index, columnData);
        }

        private TableColumn AddRowData(TableRow row, int index, TableColumnData data) {
            var columnStyle = data.colStyle;
            if (!columnStyle)
                columnStyle = tableColumnStyle;

            int columnWidth = data.width;
            if (columnWidth == -1) {
                int headerWidth = GetHeaderWidth(index);
                if (headerWidth != -1)
                    columnWidth = headerWidth;
                else
                    columnWidth = defaultColumnWidth;
            }

            System.Type type = data.sortValue.GetType();

            if (type != GetHeaderType(index))
                throw new System.ArgumentException("Invalid sort value type.");

            var column = Instantiate(columnStyle, row.rectTransform);
            column.layoutElement.preferredWidth = columnWidth;
            column.sortValue = data.sortValue;
            return column;
        }

        private int GetHeaderWidth(int index) {
            return -1;
        }

        private System.Type GetHeaderType(int index) {
            return null;
        }
    }
}
