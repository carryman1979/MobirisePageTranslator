namespace MobirisePageTranslator.Shared.Data
{
    internal sealed class PageCell : ICell
    {
        private string _content;
        private string _iso3Letter;

        public PageCell(string content, string iso3Letter, int row, int col)
        {
            _content = content;
            _iso3Letter = iso3Letter;
            Row = row;
            Col = col;
        }

        public CellType Type => CellType.SubHeader;

        public string Content => $"{_content}_{_iso3Letter}";

        public int Row { get; }

        public int Col { get; }
    }
}
