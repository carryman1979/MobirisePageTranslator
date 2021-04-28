using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;

namespace MobirisePageTranslator.Shared.Converter.Editor
{
    internal enum TextType
    {
        H1,
        H2,
        H3,
        H4, 
        H5,
        H6,
        P,
        A
    }

    internal struct ReplacementDto
    {
        public TextType Type { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public string Attributes { get; set; }
    }

    internal struct HtmlRtfConverterDto
    {
        public TextType Type { get; set; }
        public Regex HtmlSearchRegex { get; set; }
        public string RtfReplacementString { get; set; }
        public Regex RtfSearchRegex { get; set; }
    }

    public sealed class HtmlRichEditBox : RichEditBox
    {
        private bool _isHtml = false;
        private const RegexOptions _regexOptions = RegexOptions.Multiline;
        private List<HtmlRtfConverterDto> _htmlRtfConverterData = new List<HtmlRtfConverterDto>
        {
            new HtmlRtfConverterDto
            {
                Type = TextType.H1,
                HtmlSearchRegex = new Regex(@"<h1[\W\D\S]*>([\W\D\S]+)<\/h1[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf36\\b\\ul {0} \\ul0\\b0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf32\\b\\ul\\i([\W\D\S]+)\\ul0\\b0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.H2,
                HtmlSearchRegex = new Regex(@"<h2[\W\D\S]*>([\W\D\S]+)<\/h2[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf32\\b\\ul\\i {0} \\ul0\\b0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf32\\b\\ul\\i([\W\D\S]+)\\ul0\\b0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.H3,
                HtmlSearchRegex = new Regex(@"<h3[\W\D\S]*>([\W\D\S]+)<\/h3[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf28\\b {0} \\b0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf28\\b([\W\D\S]+)\\b0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.H4,
                HtmlSearchRegex = new Regex(@"<h4[\W\D\S]*>([\W\D\S]+)<\/h4[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf26\\b\\ul {0} \\ul0\\b0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf26\\b\\ul([\W\D\S]+)\\ul0\\b0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.H5,
                HtmlSearchRegex = new Regex(@"<h5[\W\D\S]*>([\W\D\S]+)<\/h5[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf24\\b\\ul\\i {0} \\i0\\ul0\\b0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf24\\b\\ul\\i([\W\D\S]+)\\i0\\ul0\\b0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.H6,
                HtmlSearchRegex = new Regex(@"<h6[\W\D\S]*>([\W\D\S]+)<\/h6[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf24\\ul {0} \\ul0\\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf24\\ul([\W\D\S]+)\\ul0\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.P,
                HtmlSearchRegex = new Regex(@"<p[\W\D\S]*>([\W\D\S]+)<\/p[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf1\\sf20 {0} \\sf0\\line",
                RtfSearchRegex = new Regex(@"\\cf1\\sf20([\W\D\S]+)\\sf0\\line", _regexOptions)
            },
            new HtmlRtfConverterDto
            {
                Type = TextType.A,
                HtmlSearchRegex = new Regex(@"<a[\W\D\S]*>([\W\D\S]+)<\/a[\S]*>", _regexOptions),
                RtfReplacementString = "\\cf2 {0} \\line",
                RtfSearchRegex = new Regex(@"\\cf2([\W\D\S]+)\\line", _regexOptions)
            },
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
            string result;
            if (_isHtml)
                TextDocument.GetText(TextGetOptions.FormatRtf, out result);
            else
                TextDocument.GetText(TextGetOptions.None, out result);
            HtmlContent = result;

            base.OnLostFocus(e);
        }

        private void Convert(string htmlText)
        {
            bool originalReadOnlyValue = StartUpdate();
            var result = htmlText;
            _isHtml = false;

            if (htmlText.Contains("<") && htmlText.Contains(">"))
            {
                _isHtml = true;
                var rtfText =
                    "{\\rtf1\\ansi\\deff0{\\fonttbl{\\f0\\fnil\\fcharset0 Arial;}}" +
                    "{\\colortbl;\\red0\\green0\\blue0;\\red0\\green0\\blue210;}";

                _htmlRtfConverterData
                    .ForEach(regRepl =>
                    {
                        regRepl
                            .HtmlSearchRegex
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
                                var replaceDto = new ReplacementDto
                                {
                                    Position = match.Index,
                                    Length = match.Length,
                                    Type = regRepl.Type,
                                    Attributes = match.Groups[0].Value
                                };

                                formatedValue = whitespacesRegex.Replace(formatedValue, " ");

                                // Add to rtf formatted string...
                                rtfText += string.Format(regRepl.RtfReplacementString, formatedValue);
                            });
                    });

                rtfText += "}";
                result = rtfText;
            }

            TextDocument.SetText(TextSetOptions.FormatRtf, result);

            EndUpdate(originalReadOnlyValue);
        }

        private void EndUpdate(bool originalReadOnlyValue)
        {
            if (originalReadOnlyValue)
                IsReadOnly = originalReadOnlyValue;
        }

        private bool StartUpdate()
        {
            var originalReadOnlyValue = IsReadOnly;
            if (IsReadOnly)
                IsReadOnly = false;
            return originalReadOnlyValue;
        }
    }
}
