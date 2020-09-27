using MobirisePageTranslator.Shared.Data;
using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public class CellTypeToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((CellType)value).HasFlag(CellType.Header) || ((CellType)value).HasFlag(CellType.SubHeader) 
                ? FontWeights.Bold 
                : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
