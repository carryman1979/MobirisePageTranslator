using MobirisePageTranslator.Shared.Data;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public class CellTypeToBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((CellType)value).HasFlag(CellType.Header) ? new Thickness(0, 0, 3, 3) : new Thickness(0, 0, 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
