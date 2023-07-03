namespace display_stats.Data
{
    public class DisplayTableData
    {
        public List<string> _headings = new List<string>();
        private List<string[]> _column_data = new List<string[]>();

        public string[] Headings => _headings.ToArray();
        public string[][] Columns => _column_data.ToArray();
        public int ColumnsCount => _headings.Count;
        public int RowsCount => (_column_data.Count > 0) ? _column_data[0].Length : 0;

        public DisplayTableData() { }

        public void AddColumn(string heading, string[] data)
        {
            if (_column_data.Count > 0 && data.Length != _column_data[0].Length)
            {
                throw new ArgumentException("Inconsistent column data lengths. All columns must have the same number of items.");
            }
            _headings.Add(heading);
            _column_data.Add(data);
        }
    }
}
