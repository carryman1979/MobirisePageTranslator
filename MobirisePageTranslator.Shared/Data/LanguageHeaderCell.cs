using MobirisePageTranslator.Shared.Converter;
using System.Globalization;

namespace MobirisePageTranslator.Shared.Data
{
    public sealed class LanguageHeaderCell : ICell
    {
        public LanguageHeaderCell(CultureInfo cultureInfo, int columnId)
        {
            var extractedLanguageName = new CultureInfoToLanguageNameConverter()
                .Convert(cultureInfo, null, null, null)
                .ToString();
            LanguageCulture = cultureInfo;
            Content = $"{extractedLanguageName} {cultureInfo.ThreeLetterISOLanguageName}";
            Col = columnId;
        }

        public CellType Type => CellType.Header;

        public string Content { get; }

        public CultureInfo LanguageCulture { get; }

        public int Row => 0;

        public int Col { get; }
    }
}
