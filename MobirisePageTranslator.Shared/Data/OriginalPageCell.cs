using Windows.Data.Json;

namespace MobirisePageTranslator.Shared.Data
{
    public sealed class OriginalPageCell : ICell
    {
        public OriginalPageCell(JsonObject originalPageObject, string content, int row, int col)
        {
            OriginalPageObject = originalPageObject;
            Content = content;
            Row = row;
            Col = col;
        }

        public JsonObject OriginalPageObject { get; }

        public CellType Type => CellType.SubHeader;

        public string Content { get; }

        public int Row { get; }

        public int Col { get; }
    }
}
