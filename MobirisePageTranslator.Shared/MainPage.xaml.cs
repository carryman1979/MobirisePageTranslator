using MobirisePageTranslator.Shared;
using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MobirisePageTranslator
{
    public sealed partial class MainPage : Page
    {
        private StorageFile _mobiriseProjectFile;
        private StorageFolder _mobiriseProjectFolder;

        public IReadOnlyCollection<CultureInfo> AvailableLanguages { get; }

        public ObservableCollection<CultureInfo> AddedLanguages { get; }

        public CultureInfo CurrentSelectedCulture { get; set; }

        internal MobiriseProjectViewModel MobiriseProjectViewModel { get; set; }

        public MainPage()
        {
            AvailableLanguages = new SupportedLanguages();
            AddedLanguages = new ObservableCollection<CultureInfo>();
            CurrentSelectedCulture = AvailableLanguages.FirstOrDefault(x => 
                x.ThreeLetterISOLanguageName == CultureInfo.CurrentCulture.ThreeLetterISOLanguageName);
            
            InitializeComponent();
        }

        private void SearchProjectFile_Click(object sender, RoutedEventArgs e)
        {
#if NETFX_CORE

            UWP_SearchMobiriseProjectFile()
                .ContinueWith(tsk => 
                {
                    Sync(() => TextBox_MobiriseProjectPath.Text = tsk.Result.Path);
                    UWP_ReadMobiriseProjectFileToString(tsk.Result)
                        .ContinueWith(subTsk => { /* TODO */ });
                });
            
#elif __MACOS__

#endif
        }

#if NETFX_CORE
        private async Task<StorageFile> UWP_SearchMobiriseProjectFile()
        {
            var mobiriseProjectFilePicker = new Windows.Storage.Pickers.FileOpenPicker();
            mobiriseProjectFilePicker.FileTypeFilter.Add(".mobirise");
            mobiriseProjectFilePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;

            var mobiriseProjectFile = await mobiriseProjectFilePicker.PickSingleFileAsync();
            Sync(() => _mobiriseProjectFile = mobiriseProjectFile);
            var mobiriseProjectFolder = await mobiriseProjectFile.GetParentAsync();
            Sync(() => _mobiriseProjectFolder = mobiriseProjectFolder);

            return mobiriseProjectFile;
        }

        private async Task<string> UWP_ReadMobiriseProjectFileToString(StorageFile mobiriseProjectFile)
        {
            return await FileIO.ReadTextAsync(mobiriseProjectFile);
        }
#endif

        private void Sync(Action action)
        {
#pragma warning disable CS4014
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () => action());
#pragma warning restore CS4014
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (!AddedLanguages.Contains(CurrentSelectedCulture))
            {
                AddedLanguages.Add(CurrentSelectedCulture);
            }
        }

        private void RemoveLanguage_Click(object sender, RoutedEventArgs e)
        {
            var removingLanguage = (CultureInfo)((Button)sender).DataContext;

            AddedLanguages.Remove(removingLanguage);
        }

        private void TextBox_MobiriseProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            StartMobiriseProjectParser(_mobiriseProjectFile, _mobiriseProjectFolder);
        }

        private void StartMobiriseProjectParser(StorageFile filePath, StorageFolder folderPath)
        {
            var projectFileWalker = new MobiriseProjectViewModel(filePath, folderPath, AddedLanguages);

            projectFileWalker.Initialize();
        }
    }
}
