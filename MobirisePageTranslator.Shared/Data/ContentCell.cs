using System.ComponentModel;

namespace MobirisePageTranslator.Shared.Data
{
    public sealed class ContentCell : ICell, INotifyPropertyChanged
    {
        private string _content;

        public ContentCell(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public CellType Type => CellType.Content;

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
