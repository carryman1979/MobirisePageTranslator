namespace MobirisePageTranslator.Shared.Data
{
    public sealed class OriginalCell : ICell
    {
        public OriginalCell(string content, int row, string jsonKey, string format = "{0}")
        {
            Content = content;
            Row = row;
            JsonKey = jsonKey;
            Format = format;
        }

        public CellType Type => CellType.Original;

        public string JsonKey { get; }

        public string Format { get; }

        public string Content { get; }

        public int Row { get; }

        public int Col => 0;
    }
}
