using MobirisePageTranslator.Shared.Data;
using System;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public class CellTypeToFontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((CellType)value).HasFlag(CellType.Header) 
                ? 20 
                : ((CellType)value).HasFlag(CellType.SubHeader)
                    ? 14
                    : 12;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
