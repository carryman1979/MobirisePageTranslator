using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace MobirisePageTranslator.Shared
{
    public sealed class MobiriseProjectViewModel
    {
        private StorageFile _projectFile;
        private ObservableCollection<CultureInfo> _languages;
        private JsonObject _jsonPrjObj;
        private JsonObject _pages;
        private readonly List<JsonObject> _currentCopiedPage = new List<JsonObject>();
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

            _jsonPrjObj["pages"] = _pages;

            var newMobiriseJsonProject = _jsonPrjObj.ToString();

            _ = WriteToFile(newMobiriseJsonProject);
        }

        private async Task WriteToFile(string newContent)
        {
            using (StreamWriter writer = new StreamWriter(await _projectFile.OpenStreamForWriteAsync()))
            {
                await writer.FlushAsync();
                await writer.WriteAsync(newContent);
                await writer.FlushAsync();
            }
        }

        private void ParseItems(ICell cellItem, int countOfLanguages)
        {
            var pageCell = cellItem as OriginalPageCell;
            
            if (pageCell != null)
            {
                ParseNewPages(countOfLanguages, pageCell);
            }
            else
            {
                ParseNewContentOfPages(cellItem, countOfLanguages);
            }
        }

        private void ParseNewContentOfPages(ICell cellItem, int countOfLanguages)
        {
            var originalCell = (OriginalCell)cellItem;

            for (var lngId = 0; lngId < countOfLanguages; lngId++)
            {
                _currentCopiedPage[lngId]["settings"].GetObject()[originalCell.JsonKey] =
                    JsonValue.CreateStringValue(
                        string.Format(
                            originalCell.Format,
                            CellItems
                            .Single(x => x.Row == originalCell.Row && x.Col == lngId + 1)
                            .Content));
            }
        }

        private void ParseNewPages(int countOfLanguages, OriginalPageCell pageCell)
        {
            var origPageJson = pageCell.OriginalPageObject.ToString();
            if (_currentCopiedPage.Count > 0)
            {
                for (var lngId = 0; lngId < countOfLanguages; lngId++)
                {
                    _pages.Add(
                        CellItems
                            .Single(x => x.Row == pageCell.Row && x.Col == lngId + 1)
                            .Content,
                        JsonObject.Parse(origPageJson));
                }
                _currentCopiedPage.Clear();
            }
            Enumerable
                .Repeat(0, countOfLanguages)
                .ToList()
                .ForEach(x =>
                    _currentCopiedPage.Add(JsonObject.Parse(origPageJson)));
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

            GetInitialOriginalItemsFromJsonObjects();
        }

        private void GetInitialOriginalItemsFromJsonObjects()
        {
            var rowIdx = 0;

            foreach (var key in _pages.Keys)
            {
                var page = _pages[key].GetObject();
                var pageSettings = page["settings"].GetObject();

                CellItems.Add(new OriginalPageCell(page, key, ++rowIdx, 0));

                _mobirisePureTextKeys
                    .ForEach(k => TryGetPageItem(k, ref rowIdx, pageSettings));
            }
        }

        private int TryGetPageItem(string key, ref int rowIdx, JsonObject pageSettings)
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