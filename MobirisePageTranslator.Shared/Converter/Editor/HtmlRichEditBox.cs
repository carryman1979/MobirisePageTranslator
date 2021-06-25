using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MobirisePageTranslator.Shared.Commands;
using System.ComponentModel;

namespace MobirisePageTranslator.Shared.Converter.Editor
{
    public enum TextType
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

    public struct ReplacementDto
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

    public sealed class HtmlRichEditBox : RichEditBox, INotifyPropertyChanged
    {
        private bool _isHtml;
        private const RegexOptions _regexOptions = RegexOptions.Multiline;
        private readonly List<HtmlRtfConverterDto> _htmlRtfConverterData = new List<HtmlRtfConverterDto>
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

        public HtmlRichEditBox()
        {
            BoldCommand = new RelayCommand 
            { 
                DoThat = () => 
                { 
                    IsBoldActive = !IsBoldActive;
                    RaisePropertyChanged(nameof(IsBoldActive));
                    PrepareTextForBold();
                }
            };
            ItalicCommand = new RelayCommand 
            { 
                DoThat = () => 
                {
                    IsItalicActive = !IsItalicActive;
                    RaisePropertyChanged(nameof(IsItalicActive));
                    PrepareTextForItalic();
                }
            };
            UnderlineCommand = new RelayCommand 
            { 
                DoThat = () => 
                {
                    IsUnderlineActive = !IsUnderlineActive;
                    RaisePropertyChanged(nameof(IsUnderlineActive));
                    PrepareTextForUnderline();
                }
            };
        }

        public ObservableCollection<ReplacementDto> ReplacedText { get; } = new ObservableCollection<ReplacementDto>();

        public ICommand BoldCommand { get; }

        public ICommand ItalicCommand { get; }

        public ICommand UnderlineCommand { get; }

        public bool IsBoldActive { get; private set; }
        public bool IsItalicActive { get; private set; }
        public bool IsUnderlineActive { get; private set; }

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

        public event PropertyChangedEventHandler PropertyChanged;

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
            ReplacedText.Clear();

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
                                ReplacedText.Add(replaceDto);

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

        private void PrepareTextForBold()
        {
            TextDocument.GetText(TextGetOptions.FormatRtf, out string text);
            
            if (TextDocument.Selection.Length > 0)
            {
                text =
                    $"{text.Substring(0, TextDocument.Selection.StartPosition)}" +
                    $"\b{text.Substring(TextDocument.Selection.StartPosition, TextDocument.Selection.Length)}\b0" +
                    $"{text.Substring(TextDocument.Selection.EndPosition)}";
                TextDocument.SetText(TextSetOptions.FormatRtf, text);
            }
            else
            {
                var wordIndex = TextDocument.Selection.GetIndex(TextRangeUnit.Word);
                text =
                    $"{text.Substring(0, wordIndex)}" +
                    $"\b \b0" +
                    $"{text.Substring(wordIndex)}";
                TextDocument.SetText(TextSetOptions.FormatRtf, text);
                TextDocument.Selection.SetRange(TextDocument.Selection.StartPosition + 2, TextDocument.Selection.EndPosition + 2);
            }
        }

        private void PrepareTextForItalic()
        {

        }

        private void PrepareTextForUnderline()
        {

        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
