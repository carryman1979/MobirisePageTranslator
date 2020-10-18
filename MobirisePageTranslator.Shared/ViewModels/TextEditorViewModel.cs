using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MobirisePageTranslator.Shared.ViewModels
{
    internal sealed class TextEditorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<ICell> _cells;
        private ContentCell _currentCell;
        private static volatile Lazy<TextEditorViewModel> _lazyObject = new Lazy<TextEditorViewModel>(new TextEditorViewModel());

        public static void Initialize(ObservableCollection<ICell> cells)
        {
            _lazyObject.Value.Setup(cells);
        }

        public static TextEditorViewModel Get => _lazyObject.Value;

        private string _original;
        public string Original
        {
            get => _original; 
            set
            {
                if (!Equals(_original, value))
                {
                    _original = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _translate;
        public string Translate
        {
            get => _translate;
            set
            {
                if (!Equals(_translate, value))
                {
                    _translate = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (!Equals(_isOpen, value))
                {
                    _isOpen = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Action<object> SaveAction { get; } 

        public Action<object> CancelAction { get; }

        public Action<object> OpenAction { get; }

        private TextEditorViewModel()
        {
            SaveAction = new Action<object>(paramObj =>
            {
                _currentCell.Content = Translate;
                CleanUp();
            });
            CancelAction = new Action<object>(paramObj => 
            {
                CleanUp();
            });
            OpenAction = new Action<object>(paramObj =>
            {
                SetActiveContentCell((ContentCell)paramObj);
                IsOpen = true;
            });
        }

        private void Setup(ObservableCollection<ICell> cells)
        {
            _cells = cells;

            RaisePropertyChanged(nameof(Original));
            RaisePropertyChanged(nameof(Translate));
        }

        private void CleanUp()
        {
            _currentCell = null;
            Original = string.Empty;
            Translate = string.Empty;
            IsOpen = false;
        }

        private void SetActiveContentCell(ContentCell contentCell)
        {
            _currentCell = contentCell;
            Original = _cells.OfType<OriginalCell>().Single(x => x.Row == _currentCell.Row).Content;
            Translate = _currentCell.Content.Substring(1, _currentCell.Content.Length - 2);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
