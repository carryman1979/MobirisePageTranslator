using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Printing;

namespace MobirisePageTranslator.Shared
{
    public sealed class MobiriseProjectViewModel
    {
        private StorageFile _projectFile;
        private ObservableCollection<CultureInfo> _languages;
        private JsonObject _jsonPrjObj;
        private JsonObject _pages;
        private JsonObject[] _currentCopiedPage;
        private readonly List<string> _mobirisePureTextKeys = new List<string>
        {
            "title",
            "meta_descr",
            "custom_html"
        };

        public ObservableCollection<ICell> CellItems { get; }

        public MobiriseProjectViewModel()
        {
            CellItems = new ObservableCollection<ICell>();
        }

        public void Initialize(StorageFile projectFile, in ObservableCollection<CultureInfo> languages)
        {
            _projectFile = projectFile;
            _languages = languages;
            _languages.CollectionChanged += OnLanguagesChanged;
            InitializeLanguages(_languages);
        }

        public void ParseNewPagesToProject()
        {
            if (CellItems.Count > 0)
            {
                var countOfLanguages = CellItems.Max(x => x.Col);
                if (countOfLanguages > 0)
                {
                    CellItems
                        .Where(x => x.Col == 0 && x.Type != CellType.Header)
                        .OrderBy(y => y.Row)
                        .ToList()
                        .ForEach(z => ParseItems(z, countOfLanguages));
                }
            }
        }

        private void ParseItems(ICell cellItem, int countOfLanguages)
        {
            var pageCell = cellItem as OriginalPageCell;
            
            if (pageCell != null)
            {
                var origPageJson = pageCell.OriginalPageObject.ToString();

                //for (var lngId = 0; lngId < )
            }
            else
            {
                var originalCell = (OriginalCell)cellItem;
                //_currentCopiedPage[originalCell.JsonKey] = JsonValue.CreateStringValue(string.Format(originalCell.Format, originalCell.Content));
            }
        }

        public void CleanUp()
        {
            _languages.CollectionChanged -= OnLanguagesChanged;
            CellItems.Clear();
        }

        private void OnLanguagesChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    args.NewItems.OfType<CultureInfo>().ToList().ForEach(AddLanguage);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    args.OldItems.OfType<CultureInfo>().ToList().ForEach(RemoveLanguage);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void InitializeLanguages(ICollection<CultureInfo> languageCultures)
        {
            languageCultures
                .ToList()
                .ForEach(AddLanguage);
        }

        private void AddLanguage(CultureInfo newCultureInfo)
        {
            var newCol = CellItems.Count == 0 ? 0 : (CellItems.Max(x => x.Col) + 1);
            CellItems.Add(new LanguageHeaderCell(newCultureInfo, newCol));
            if (newCol != 0)
            {
                AddNewLanguageItems(newCol, newCultureInfo.ThreeLetterISOLanguageName);
            }
            else
            {
                InitializeOriginalItems();
            }
        }

        private void AddNewLanguageItems(int newCol, string iso3LetterLanguageName)
        {
            var rowCount = CellItems.Count(x => x.Col == 0);
            for (var i = 1; i < rowCount; i++)
            {
                var originalCell = CellItems.Single(x => x.Col == 0 && x.Row == i);

                CellItems.Add(originalCell.Type == CellType.SubHeader
                    ? (ICell)new PageCell(originalCell.Content, iso3LetterLanguageName, i, newCol)
                    : new ContentCell(i, newCol, $"[{originalCell.Content}]"));
            }
        }

        private void InitializeOriginalItems()
        {
            var result = string.Empty;
            var tsk = _projectFile.OpenStreamForReadAsync();
            tsk.Wait();

            using (var reader = new StreamReader(tsk.Result))
            {
                result = reader.ReadToEnd();
            }

            _jsonPrjObj = JsonValue.Parse(result).GetObject();
            var settings = _jsonPrjObj["settings"].GetObject();
            _pages = _jsonPrjObj["pages"].GetObject();

            var rowIdx = 0;

            foreach (var key in _pages.Keys)
            {
                var page = _pages[key].GetObject();
                var pageSettings = page["settings"].GetObject();

                CellItems.Add(new OriginalPageCell(page, key, ++rowIdx, 0));

                _mobirisePureTextKeys
                    .ForEach(k => TryGetPageItem(k, rowIdx, pageSettings));
            }
        }

        private int TryGetPageItem(string key, int rowIdx, JsonObject pageSettings)
        {
            if (pageSettings.ContainsKey(key))
            {
                var text = pageSettings[key].GetString();
                CellItems.Add(new OriginalCell(text, ++rowIdx, key));
            }

            return rowIdx;
        }

        private void RemoveLanguage(CultureInfo removedLanguage)
        {
            var languageItem = CellItems
                .OfType<LanguageHeaderCell>()
                .First(x => Equals(x.LanguageCulture, removedLanguage));
            var itemsForRemove = CellItems
                .Where(x => Equals(x.Col, languageItem.Col))
                .ToList();

            itemsForRemove.Add(languageItem);
            itemsForRemove.ForEach(x => CellItems.Remove(x));
        }
    }
}