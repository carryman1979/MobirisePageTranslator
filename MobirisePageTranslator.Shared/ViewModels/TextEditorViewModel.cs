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
        private static readonly Lazy<TextEditorViewModel> _lazyObject = new Lazy<TextEditorViewModel>(new TextEditorViewModel());
        private ObservableCollection<ICell> _cells;
        private ICell _currentCell;
        private string _original;
        private string _translate;
        private bool _isOpen;
        
        public static void Initialize(ObservableCollection<ICell> cells)
        {
            _lazyObject.Value.Setup(cells);
        }

        private TextEditorViewModel()
        {
            SaveAction = new Action(SaveActionLogic);
            CancelAction = new Action(CleanUp);
            OpenAction = new Action<object>(OpenActionLogic);
        }

        public static TextEditorViewModel Get => _lazyObject.Value;

        public Action SaveAction { get; }

        public Action CancelAction { get; }

        public Action<object> OpenAction { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OpenActionLogic(object paramObj)
        {
            SetActiveContentCell((ICell)paramObj);
            IsOpen = true;
        }

        private void SaveActionLogic()
        {
            var editableContentCell = _currentCell as ContentCell;
            if (editableContentCell == null)
            {
                var editablePageCell = _currentCell as PageCell;

                editablePageCell.Content = Translate;
            }
            else
                editableContentCell.Content = Translate;
            CleanUp();
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

        private void SetActiveContentCell(ICell contentCell)
        {
            _currentCell = contentCell;
            Original = _cells.OfType<OriginalCell>().SingleOrDefault(x => x.Row == _currentCell.Row)?.Content ??
                       _cells.OfType<OriginalPageCell>().Single(x => x.Row == _currentCell.Row).Content;
            Translate = _currentCell.Content.First() == '[' && _currentCell.Content.Last() == ']'
                ? _currentCell.Content.Substring(1, _currentCell.Content.Length - 2)
                : _currentCell.Content;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
