namespace MobirisePageTranslator.Shared.Data
{
    public sealed class OriginalCell : ICell
    {
        public OriginalCell(string content, int row)
        {
            Content = content;
            Row = row;
        }

        public CellType Type => CellType.Original;

        public string Content { get; }

        public int Row { get; }

        public int Col => 0;
    }
}
