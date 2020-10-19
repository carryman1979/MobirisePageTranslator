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

namespace MobirisePageTranslator.Shared.ViewModels
{
    internal sealed class MobiriseProjectViewModel
    {
        private static readonly Lazy<MobiriseProjectViewModel> _lazyObject = new Lazy<MobiriseProjectViewModel>(new MobiriseProjectViewModel());
        private readonly Dictionary<string, JsonObject> _currentCopiedPage = new Dictionary<string, JsonObject>();
        private readonly List<string> _mobirisePureTextKeys = new List<string>
        {
            "title",
            "meta_descr",
            "custom_html",
            "_customHTML"
        };
        private StorageFile _projectFile;
        private ObservableCollection<CultureInfo> _languages;
        private JsonObject _jsonPrjObj;
        private JsonObject _pages;

        private MobiriseProjectViewModel()
        {
            var cellItems = new ObservableCollection<ICell>();
            CellItems = cellItems;

            TextEditorViewModel.Initialize(cellItems);
        }

        public static MobiriseProjectViewModel Get => _lazyObject.Value;

        public ObservableCollection<ICell> CellItems { get; }

        public void Initialize(StorageFile projectFile, in ObservableCollection<CultureInfo> languages)
        {
            _projectFile = projectFile;
            _languages = languages;
            _languages.CollectionChanged += OnLanguagesChanged;
            InitializeLanguages(_languages);
        }

        public void CleanUp()
        {
            if (_languages != null)
                _languages.CollectionChanged -= OnLanguagesChanged;
            CellItems.Clear();
        }

        public void ParseNewPagesToProject()
        {
            if (CellItems.Count > 0)
            {
                var countOfLanguages = CellItems.Max(x => x.Col);

                if (countOfLanguages > 0)
                {
                    var testData = CellItems
                        .Where(x => x.Col == 0)
                        .OrderBy(y => y.Row)
                        .ToList();
                    testData.ForEach(z => ParseItems(z, countOfLanguages));
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
                ParseNewContentOfPages(cellItem);
            }
        }

        private void ParseNewContentOfPages(ICell cellItem)
        {
            var originalCell = cellItem as OriginalCell;

            if (originalCell != null)
            {
                foreach (var lngId in _currentCopiedPage.Keys)
                {
                    var colId = CellItems
                            .Single(y => y.Type.HasFlag(CellType.SubHeader) && Equals(y.Content, lngId))
                            .Col;
                    _currentCopiedPage[lngId]["settings"].GetObject()[originalCell.JsonKey] =
                        JsonValue.CreateStringValue(
                            string.Format(
                                originalCell.Format,
                                CellItems
                                .Single(x => x.Row == originalCell.Row && x.Col == colId)
                                .Content));
                }
            }
        }

        private void ParseNewPages(int countOfLanguages, OriginalPageCell pageCell)
        {
            var origPageJson = pageCell.OriginalPageObject.ToString();

            if (_currentCopiedPage.Count > 0)
            {
                foreach (var currentLanguageCopy in _currentCopiedPage)
                {
                    _pages.Add(currentLanguageCopy.Key, JsonObject.Parse(currentLanguageCopy.Value.ToString()));
                }
                _currentCopiedPage.Clear();
            }
            Enumerable
                .Repeat(0, countOfLanguages)
                .ToList()
                .ForEach(x =>
                    _currentCopiedPage.Add(
                        CellItems
                            .Single(y => y.Row == pageCell.Row && y.Col == x + 1)
                            .Content,
                        JsonObject.Parse(origPageJson)));
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
                var pageComponents = page["components"].GetArray();

                CellItems.Add(new OriginalPageCell(page, key, ++rowIdx, 0));

                _mobirisePureTextKeys
                    .ForEach(k => TryGetPageItem(k, ref rowIdx, pageSettings));
                _mobirisePureTextKeys
                    .ForEach(k => 
                        pageComponents
                            .ToList()
                            .ForEach(c => TryGetPageItem(k, ref rowIdx, c.GetObject())));
            }
        }

        private void TryGetPageItem(string key, ref int rowIdx, JsonObject jsonObject)
        {
            if (jsonObject.ContainsKey(key))
            {
                var text = jsonObject[key].GetString();
                CellItems.Add(new OriginalCell(text, ++rowIdx, key));
            }
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