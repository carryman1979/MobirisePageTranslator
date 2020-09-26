using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using Windows.Data.Json;
using Windows.Storage;

namespace MobirisePageTranslator.Shared
{
    internal sealed class MobiriseProjectViewModel
    {
        private StorageFile _projectFile;
        private StorageFolder _projectFolder;
        private ObservableCollection<CultureInfo> _languages;

        public ObservableCollection<ICell> CellItems { get; }

        public MobiriseProjectViewModel(StorageFile projectFile, StorageFolder projectfolder, ObservableCollection<CultureInfo> languages)
        {
            CellItems = new ObservableCollection<ICell>();
            _projectFile = projectFile;
            _projectFolder = projectfolder;
            _languages = languages;
            _languages.CollectionChanged += OnLanguagesChanged;
            InitializeLanguages(_languages);
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
            var newCol = CellItems.Count == 0 ? 0 : CellItems.Max(x => x.Col) + 1;
            CellItems.Add(new LanguageHeaderCell(newCultureInfo, newCol));
            if (newCol != 1)
            {
                var rowCount = CellItems.Count(x => x.Col == 0);
                for (var i = 1; i < rowCount; i++)
                {
                    CellItems.Add(new ContentCell(i, newCol));
                }
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

        public void Initialize()
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

            var rowIdx = 1;

            foreach (var key in pages.Keys)
            {
                CellItems.Add(new PageCell(key, "eng", ++rowIdx, 0));

                var page = pages[key].GetObject();
                var pageSettings = page["settings"].GetObject();
                var title = pageSettings["title"].GetString();
                var meta_Descr = pageSettings["meta_descr"].GetString();
                var custom_Html = pageSettings["custom_html"].GetString();

                CellItems.Add(new OriginalCell(title, ++rowIdx));
                CellItems.Add(new OriginalCell(meta_Descr, ++rowIdx));
                CellItems.Add(new OriginalCell(custom_Html, ++rowIdx));
            }
        }


    }


}
