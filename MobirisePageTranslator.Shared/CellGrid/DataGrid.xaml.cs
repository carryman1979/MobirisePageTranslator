using MobirisePageTranslator.Shared.Data;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace MobirisePageTranslator.Shared.CellGrid
{
    public sealed partial class DataGrid : ItemsControl
    {
        public DataGrid()
        {
            InitializeComponent();
        }

        protected override void OnItemsChanged(object e)
        {
            var cellList = ((ICollection<ICell>)ItemsSource).ToList();

            if (cellList.Any())
            {
                var maxRow = cellList
                    .Max(x => x.Row) + 1;
                var maxCol = cellList
                    .Max(x => x.Col) + 1;

                var rootGrid = (Grid)ItemsPanelRoot;
                var rowCount = rootGrid.RowDefinitions.Count;
                var colCount = rootGrid.ColumnDefinitions.Count;

                if (rowCount < maxRow)
                {
                    rootGrid.RowDefinitions.Add(new RowDefinition());
                }
                else if (rowCount > maxRow)
                {
                    var lastRowDef = rootGrid.RowDefinitions.Last();
                    rootGrid.RowDefinitions.Remove(lastRowDef);
                }

                if (colCount < maxCol)
                {
                    rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }
                else if (colCount > maxCol)
                {
                    var lastColDef = rootGrid.ColumnDefinitions.Last();
                    rootGrid.ColumnDefinitions.Remove(lastColDef);
                }
            }
            
            base.OnItemsChanged(e);
        }
    }
}
