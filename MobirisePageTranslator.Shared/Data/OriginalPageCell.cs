namespace MobirisePageTranslator.Shared.Data
{
    public sealed class OriginalPageCell : ICell
    {
        private string _content;
        private string _iso3Letter;

        public OriginalPageCell(string content, string iso3Letter, int row, int col)
        {
            _content = content;
            _iso3Letter = iso3Letter;
            Row = row;
            Col = col;
        }

        public CellType Type => CellType.SubHeader & CellType.Content;

        public string Content => $"{_content}_{_iso3Letter}";

        public int Row { get; }

        public int Col { get; }
    }
}
