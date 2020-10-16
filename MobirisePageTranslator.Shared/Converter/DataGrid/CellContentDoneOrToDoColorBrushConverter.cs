using MobirisePageTranslator.Shared.Data;
using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MobirisePageTranslator.Shared.Converter.DataGrid
{
    public sealed class CellContentDoneOrToDoColorBrushConverter : DependencyObject, IValueConverter 
    {
        public Brush DoneBrush
        {
            get { return (Brush)GetValue(DoneBrushProperty); }
            set { SetValue(DoneBrushProperty, value); }
        }

        public static readonly DependencyProperty DoneBrushProperty =
            DependencyProperty.Register(
                nameof(DoneBrush), 
                typeof(Brush), 
                typeof(CellContentDoneOrToDoColorBrushConverter), 
                new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        public Brush ToDoBrush
        {
            get { return (Brush)GetValue(ToDoBrushProperty); }
            set { SetValue(ToDoBrushProperty, value); }
        }

        public static readonly DependencyProperty ToDoBrushProperty =
            DependencyProperty.Register(
                nameof(ToDoBrush), 
                typeof(Brush), 
                typeof(CellContentDoneOrToDoColorBrushConverter), 
                new PropertyMetadata(new SolidColorBrush(Colors.DarkGray)));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString().First() == '[' && value.ToString().Last() == ']'
                ? ToDoBrush
                : DoneBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
