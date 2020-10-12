using MobirisePageTranslator.Shared.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PopUp = Windows.UI.Xaml.Controls.Primitives.Popup;

namespace MobirisePageTranslator.Shared.Editor
{
    public sealed partial class TextEditor : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ICell> Originals
        {
            get { return (ObservableCollection<ICell>)GetValue(OriginalProperty); }
            set { SetValue(OriginalProperty, value); }
        }

        public static readonly DependencyProperty OriginalProperty =
            DependencyProperty.Register(
                nameof(Originals), 
                typeof(ObservableCollection<ICell>), 
                typeof(TextEditor), 
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) => 
                {
                    if (args.NewValue != args.OldValue)
                    {
                        var textEditor = (TextEditor)obj;

                        if (args.OldValue != null)
                        {
                            ((ObservableCollection<ICell>)args.OldValue).CollectionChanged -= textEditor.OnCellItemsChanged;
                        }
                        if (args.NewValue != null)
                        {
                            ((ObservableCollection<ICell>)args.NewValue).CollectionChanged += textEditor.OnCellItemsChanged;
                        }

                        textEditor.UpdateOriginal();
                    }
                })));
        private string _originalText;

        public ICell Original { get; private set; }

        public ContentCell Translate
        {
            get { return (ContentCell)GetValue(TranslateProperty); }
            set { SetValue(TranslateProperty, value); }
        }

        public static readonly DependencyProperty TranslateProperty =
            DependencyProperty.Register(
                nameof(Translate), 
                typeof(ContentCell), 
                typeof(TextEditor), 
                new PropertyMetadata(null, new PropertyChangedCallback((obj, args) => {
                    if (args.NewValue != args.OldValue)
                    {
                        ((TextEditor)obj).UpdateOriginal();
                    }
                })));

        public PopUp OwnPopUp
        {
            get { return (PopUp)GetValue(OwnPopUpProperty); }
            set { SetValue(OwnPopUpProperty, value); }
        }

        public static readonly DependencyProperty OwnPopUpProperty =
            DependencyProperty.Register(nameof(OwnPopUp), typeof(PopUp), typeof(TextEditor), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public TextEditor()
        {
            InitializeComponent();
        }

        private void UpdateOriginal()
        {
            if (Originals != null && Translate != null)
            {
                _originalText = Translate.Content;
                Original = Originals.Single(x => x.Col == 0 && x.Row == Translate.Row);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Original)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Translate)));
        }

        private void OnCellItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateOriginal();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            _originalText = Translate.Content;
            OwnPopUp.IsOpen = false;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Translate.Content = _originalText;
            OwnPopUp.IsOpen = false;
        }
    }
}
