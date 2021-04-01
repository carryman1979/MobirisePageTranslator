using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;

namespace MobirisePageTranslator.Shared.Converter.Editor
{
    public sealed class HtmlRichEditBox : RichEditBox
    {
        private const RegexOptions _regexOptions = RegexOptions.Multiline;
        private Dictionary<Regex, string> _htmlToRtfMatching = new Dictionary<Regex, string>
        {
            { new Regex(@"<h1[\W\D\S]*>([\W\D\S]+)<\/h1[\S]*>", _regexOptions), "\\cf1\\sf36\\b\\ul {0} \\ul0\\b0\\sf0\\line" },
            { new Regex(@"<h2[\W\D\S]*>([\W\D\S]+)<\/h2[\S]*>", _regexOptions), "\\cf1\\sf32\\b\\ul\\i {0} \\ul0\\b0\\sf0\\line" },
            { new Regex(@"<h3[\W\D\S]*>([\W\D\S]+)<\/h3[\S]*>", _regexOptions), "\\cf1\\sf28\\b {0} \\b0\\sf0\\line" },
            { new Regex(@"<h4[\W\D\S]*>([\W\D\S]+)<\/h4[\S]*>", _regexOptions), "\\cf1\\sf26\\b\\ul {0} \\ul0\\b0\\sf0\\line" },
            { new Regex(@"<h5[\W\D\S]*>([\W\D\S]+)<\/h5[\S]*>", _regexOptions), "\\cf1\\sf24\\b\\ul\\i {0} \\i0\\ul0\\b0\\sf0\\line" },
            { new Regex(@"<h6[\W\D\S]*>([\W\D\S]+)<\/h6[\S]*>", _regexOptions), "\\cf1\\sf24\\ul {0} \\ul0\\sf0\\line" },
            { new Regex(@"<p[\W\D\S]*>([\W\D\S]+)<\/p[\S]*>", _regexOptions)  , "\\cf1\\sf20 {0} \\sf0\\line" },
            { new Regex(@"<a[\W\D\S]*>([\W\D\S]+)<\/a[\S]*>", _regexOptions)  , "\\cf2 {0} \\line" }
        };

        public string HtmlContent
        {
            get => (string)GetValue(HtmlContentProperty);
            set => SetValue(HtmlContentProperty, value);
        }

        public static readonly DependencyProperty HtmlContentProperty =
            DependencyProperty.Register(
                nameof(HtmlContent), 
                typeof(string), 
                typeof(HtmlRichEditBox), 
                new PropertyMetadata(
                    null, 
                    new PropertyChangedCallback((obj, args) => 
                    {
                        if (obj is HtmlRichEditBox editBox &&
                            !string.IsNullOrWhiteSpace(args.NewValue?.ToString()))
                        {
                            editBox.Convert(args.NewValue.ToString());
                        }
                    })));

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            TextDocument.GetText(TextGetOptions.FormatRtf, out string result);
            HtmlContent = result;

            base.OnLostFocus(e);
        }

        private void Convert(string htmlText)
        {
            var rtfText =
                "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}}" +
                "{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue210;}";

            _htmlToRtfMatching
                .ToList()
                .ForEach(regRepl =>
                {
                    regRepl
                        .Key
                        .Matches(htmlText)
                        .Where(x => x.Success)
                        .OrderByDescending(y => y.Index + y.Length)
                        .ToList()
                        .ForEach(match =>
                        {
                            // Prepare string
                            var formatedValue = match.Groups[1].Value.Replace("\n", " ").Replace("\r", string.Empty).Replace("\t", " ");
                            var options = RegexOptions.None;
                            var whitespacesRegex = new Regex("[ ]{2,}", options);

                            formatedValue = whitespacesRegex.Replace(formatedValue, " ");

                            // Add to rtf formatted string...
                            rtfText += string.Format(regRepl.Value, formatedValue);
                        });
                });
            
            rtfText += "}";

            TextDocument.SetText(TextSetOptions.FormatRtf, rtfText);
        }
    }
}
