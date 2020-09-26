using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter
{
    public sealed class CultureInfoToLanguageNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var cultureInfo = value as CultureInfo;

            return cultureInfo == null ? string.Empty :
                cultureInfo.EnglishName.Substring(0, cultureInfo.EnglishName.IndexOf('('));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
