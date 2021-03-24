using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace MobirisePageTranslator.Shared.Converter.Editor
{
    public sealed class HtmlToRtfTypeConverter : IValueConverter
    {
        private Dictionary<string, string> _htmlToRtfMatching = new Dictionary<string, string>
        {
            { @"<h1[\W\D\S]*>([\W\D\S]+)<\/h1[\S]*>", "\\cf1\\sf36\\b\\ul {0} \\ul0\\b0\\sf0\\line" },
            { @"<h2[\W\D\S]*>([\W\D\S]+)<\/h2[\S]*>", "\\cf1\\sf32\\b\\ul\\i {0} \\ul0\\b0\\sf0\\line" },
            { @"<h3[\W\D\S]*>([\W\D\S]+)<\/h3[\S]*>", "\\cf1\\sf28\\b {0} \\b0\\sf0\\line" },
            { @"<h4[\W\D\S]*>([\W\D\S]+)<\/h4[\S]*>", "\\cf1\\sf26\\b\\ul {0} \\ul0\\b0\\sf0\\line" },
            { @"<h5[\W\D\S]*>([\W\D\S]+)<\/h5[\S]*>", "\\cf1\\sf24\\b\\ul\\i {0} \\i0\\ul0\\b0\\sf0\\line" },
            { @"<h6[\W\D\S]*>([\W\D\S]+)<\/h6[\S]*>", "\\cf1\\sf24\\ul {0} \\ul0\\sf0\\line" },
            { @"<p[\W\D\S]*>([\W\D\S]+)<\/p[\S]*>"  , "\\cf1\\sf20 {0} \\sf0\\line" },
            { @"<a[\W\D\S]*>([\W\D\S]+)<\/a[\S]*>"  , "\\cf2 {0} \\line" }
        };

        public HtmlToRtfTypeConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var editor = new RichEditBox();
            var regexOptions = RegexOptions.Multiline;
            var rtfText =
                "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}}" +
                "{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue210;}";

            foreach (var regexRtfElement in _htmlToRtfMatching)
            {
                var regex = new Regex(regexRtfElement.Key, regexOptions);
                var results = regex.Matches(value.ToString());

                foreach (Match match in results)
                {
                    if (match.Success)
                    {
                        // Prepare string
                        var formatedValue = match.Groups[1].Value.Replace("\n", " ").Replace("\r", string.Empty).Replace("\t", " ");
                        var options = RegexOptions.None;
                        var whitespacesRegex = new Regex("[ ]{2,}", options);
                        
                        formatedValue = whitespacesRegex.Replace(formatedValue, " ");

                        // Add to rtf formatted string...
                        rtfText += string.Format(regexRtfElement.Value, formatedValue);
                    }
                }
            }
            rtfText += "}";

            editor.TextDocument.SetText(Windows.UI.Text.TextSetOptions.FormatRtf, rtfText);

            return editor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var result = string.Empty;
            (value as RichEditBox)?.TextDocument.GetText(Windows.UI.Text.TextGetOptions.None, out result);

            result.Replace("", "");

            return result;
        }
    }
}
