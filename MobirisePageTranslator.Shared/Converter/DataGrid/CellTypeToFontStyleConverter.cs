using MobirisePageTranslator.Shared.Data;
using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public class CellTypeToFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((CellType)value).HasFlag(CellType.Original) 
                ? FontStyle.Italic 
                : FontStyle.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
