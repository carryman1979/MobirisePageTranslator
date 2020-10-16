using MobirisePageTranslator.Shared.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MobirisePageTranslator.Shared
{
    public sealed partial class DataGridCell : UserControl
    {
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
