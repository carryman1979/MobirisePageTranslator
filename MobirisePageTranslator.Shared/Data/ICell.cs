using MobirisePageTranslator.Shared.ViewModels;

namespace MobirisePageTranslator.Shared.Data
{
    public interface ICell
    {
        CellType Type { get; }

        string Content { get; }

        int Row { get; }

        int Col { get; }
    }
}
