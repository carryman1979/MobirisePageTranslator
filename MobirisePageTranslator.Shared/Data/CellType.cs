using System;

namespace MobirisePageTranslator.Shared.Data
{
    [Flags]
    public enum CellType
    {
        Content = 1,
        Header = 2,
        SubHeader = 4,
        Original = 8
    }
}
