using MobirisePageTranslator.Shared.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public sealed class CellTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ReadOnlyTemplate { get; set; }

        public DataTemplate WriteableTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var cellItem = item as ICell;

            if (cellItem == null) return base.SelectTemplateCore(item);
            if (cellItem.Type.HasFlag(CellType.Content)) return WriteableTemplate;

            return ReadOnlyTemplate;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }
    }
}
