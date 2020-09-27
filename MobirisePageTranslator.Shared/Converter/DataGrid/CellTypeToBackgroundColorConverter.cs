using MobirisePageTranslator.Shared.Data;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public class CellTypeToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((CellType)value).HasFlag(CellType.Header)
                ? new SolidColorBrush(Colors.LightSeaGreen) 
                : ((CellType)value).HasFlag(CellType.SubHeader)
                    ? new SolidColorBrush(Colors.LightGray)
                    : new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
