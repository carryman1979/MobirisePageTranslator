using MobirisePageTranslator.Shared.ViewModels;
using System.ComponentModel;

namespace MobirisePageTranslator.Shared.Data
{
    internal sealed class PageCell : ICell, INotifyPropertyChanged
    {
        private string _content;

        public PageCell(string content, string iso3Letter, int row, int col)
        {
            _content = $"{iso3Letter}_{content}";
            Row = row;
            Col = col;
        }

        public CellType Type => CellType.SubHeader | CellType.Content;

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }

        public int Row { get; }

        public int Col { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
