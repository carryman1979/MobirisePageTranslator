using System;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter
{
    public sealed class IsNotNullOrWhitespaceToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !string.IsNullOrWhiteSpace(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
