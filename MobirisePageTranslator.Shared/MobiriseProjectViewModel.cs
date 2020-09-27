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

namespace MobirisePageTranslator.Shared
{
    public sealed class MobiriseProjectViewModel
    {
        private StorageFile _projectFile;
        private ObservableCollection<CultureInfo> _languages;

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

            var jsonPrjObj = JsonValue.Parse(result).GetObject();
            var settings = jsonPrjObj["settings"].GetObject();
            var pages = jsonPrjObj["pages"].GetObject();

            var rowIdx = 0;
            JsonObject lastPage = null;
            string lastKey = null;

            foreach (var key in pages.Keys)
            {
                var page = pages[key].GetObject();
                lastPage = page;
                lastKey = key;
                var pageSettings = page["settings"].GetObject();

                CellItems.Add(new OriginalPageCell(page, key, ++rowIdx, 0));

                if (pageSettings.ContainsKey("title"))
                {
                    var title = pageSettings["title"].GetString();
                    CellItems.Add(new OriginalCell(title, ++rowIdx));
                }
                if (pageSettings.ContainsKey("meta_descr"))
                {
                    var meta_Descr = pageSettings["meta_descr"].GetString();
                    CellItems.Add(new OriginalCell(meta_Descr, ++rowIdx));
                }
                if (pageSettings.ContainsKey("custom_html"))
                {
                    var custom_Html = pageSettings["custom_html"].GetString();
                    CellItems.Add(new OriginalCell(custom_Html, ++rowIdx));
                }
            }

            var newPage = JsonObject.Parse(lastPage.ToString());
            newPage["settings"].GetObject()["title"]. "Neuer deutscher Titel");
            newPage["settings"].GetObject()["meta_descr"] = JsonValue.Parse("Neue Metadescription.");

            pages.Add("deu_" + lastKey, newPage);

            var res = jsonPrjObj.ToString();
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