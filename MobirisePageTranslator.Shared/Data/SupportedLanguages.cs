using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MobirisePageTranslator.Shared.Data
{
    internal sealed class SupportedLanguages : ReadOnlyCollection<CultureInfo>
    {
        public SupportedLanguages() 
            : base(new List<CultureInfo> 
            { 
                new CultureInfo("en-GB"),
                new CultureInfo("de-DE"),
                new CultureInfo("fr-FR"),
                new CultureInfo("es-ES"),
                new CultureInfo("pt-PT"),
                new CultureInfo("el-GR"),
                new CultureInfo("it-IT"),
                new CultureInfo("hu-HU"),
                new CultureInfo("hr-HR"),
                new CultureInfo("ro-RO"),
                new CultureInfo("ru-RU"),
                new CultureInfo("da-DK"),
                new CultureInfo("nb-NO"),
                new CultureInfo("sv-SE"),
                new CultureInfo("lv-LV"),
                new CultureInfo("pl-PL"),
                new CultureInfo("uk-UA"),
                new CultureInfo("fi-FI"),
                new CultureInfo("sq-AL"),
                new CultureInfo("nl-NL"),
                new CultureInfo("ca-AD"),
                new CultureInfo("mk-MK"),
                new CultureInfo("az-Latn-AZ"),
                new CultureInfo("ka-GA"),
                new CultureInfo("sr-Latn-RS"),
                new CultureInfo("bg-BG"),
                new CultureInfo("sk-SK"),
                new CultureInfo("sl-SL"),
                new CultureInfo("lt-LT"),
                new CultureInfo("is-IS"),
                new CultureInfo("cs-CZ"),
                new CultureInfo("tr-TR"),
                new CultureInfo("et-EE"),
                new CultureInfo("zh-CN"),
                new CultureInfo("vi-VN"),
                new CultureInfo("th-TH"),
                new CultureInfo("ko-KR"),
                new CultureInfo("ko-KR"),
                new CultureInfo("ja-JP"),
                new CultureInfo("ar-SA")
            })
        {
        }
    }
}
