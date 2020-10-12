using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public sealed class TrimTextConverter : FrameworkElement, IValueConverter
    {
        public int MaxSignCount
        {
            get { return (int)GetValue(MaxSignCountProperty); }
            set { SetValue(MaxSignCountProperty, value); }
        }

        public static readonly DependencyProperty MaxSignCountProperty =
            DependencyProperty.Register(nameof(MaxSignCount), typeof(int), typeof(TrimTextConverter), new PropertyMetadata(16));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var trimmedText = value.ToString().Trim();
            if (trimmedText.Contains("<") && trimmedText.Contains(">"))
                trimmedText = "HTML content...";

            return trimmedText.Length > MaxSignCount
                ? $"{trimmedText.Substring(0, MaxSignCount)}..."
                : trimmedText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
