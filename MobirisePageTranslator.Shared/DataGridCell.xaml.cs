using MobirisePageTranslator.Shared.Commands;
using MobirisePageTranslator.Shared.Data;
using System.ComponentModel;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MobirisePageTranslator.Shared
{
    public sealed partial class DataGridCell : UserControl, INotifyPropertyChanged
    {
        public ICommand OpenEditorCommand
        {
            get { return (ICommand)GetValue(OpenEditorCommandProperty); }
            set { SetValue(OpenEditorCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenEditorCommandProperty =
            DependencyProperty.Register(
                nameof(OpenEditorCommand), 
                typeof(ICommand), 
                typeof(DataGridCell), 
                new PropertyMetadata(
                    new RelayCommand(), 
                    new PropertyChangedCallback((obj, args) => 
                    {
                        var gridCell = (DataGridCell)obj;

                        if (args.NewValue != args.OldValue)
                            gridCell.PropertyChanged?.Invoke(gridCell, new PropertyChangedEventArgs(nameof(OpenEditorCommand)));

                    })));

        public event PropertyChangedEventHandler PropertyChanged;

        public DataGridCell()
        {
            SizeChanged += (obj, args) =>
            {
                var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
                var newCellData = DataContext as ICell;

                if (parent != null && newCellData != null)
                {
                    Grid.SetRow(parent, newCellData.Row);
                    Grid.SetColumn(parent, newCellData.Col);
                }
            };

            InitializeComponent();
        }
    }
}
