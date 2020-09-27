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

        public IReadOnlyCollection<CultureInfo> AvailableLanguages { get; }

        public ObservableCollection<CultureInfo> AddedLanguages { get; }

        public CultureInfo CurrentSelectedCulture { get; set; }

        public MobiriseProjectViewModel MobiriseProjectViewModel { get; } = new MobiriseProjectViewModel();

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
                    Sync(() => 
                    { 
                        TextBox_MobiriseProjectPath.Text = tsk.Result.Path;
                        StartMobiriseProjectParser();
                    });
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
            if (mobiriseProjectFile?.IsAvailable ?? false)
            {
                Sync(() => 
                { 
                    _mobiriseProjectFile = mobiriseProjectFile;
                    EnableLanguageSelection();
                });
            }
            else
            {
                Sync(() =>
                {
                    _mobiriseProjectFile = null;
                    DisableLanguageSelection();
                });
            }

            return mobiriseProjectFile;
        }
#endif

        private void Sync(Action action)
        {
#pragma warning disable CS4014
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () => action());
#pragma warning restore CS4014
        }

        private void DisableLanguageSelection()
        {
            TranslateButton.IsEnabled = false;
            LanguageSelectorComboBox.IsEnabled = false;
            AddLanguageButton.IsEnabled = false;
            TextBox_MobiriseProjectPath.Text = null;
            MobiriseProjectViewModel.CleanUp();
        }

        private void EnableLanguageSelection()
        {
            LanguageSelectorComboBox.IsEnabled = true;
            AddLanguageButton.IsEnabled = true;
        }

        private void AddLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (!AddedLanguages.Contains(CurrentSelectedCulture))
            {
                AddedLanguages.Add(CurrentSelectedCulture);

                StartMobiriseProjectParser();
            }
        }

        private void RemoveLanguage_Click(object sender, RoutedEventArgs e)
        {
            var removingLanguage = (CultureInfo)((Button)sender).DataContext;

            AddedLanguages.Remove(removingLanguage);
        }

        private void StartMobiriseProjectParser()
        {
            if (_mobiriseProjectFile != null && AddedLanguages.Count == 1)
            {
                MobiriseProjectViewModel.Initialize(_mobiriseProjectFile, AddedLanguages);

                TranslateButton.IsEnabled = true;
            }
        }
    }
}
