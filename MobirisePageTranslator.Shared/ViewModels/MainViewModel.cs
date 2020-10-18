using MobirisePageTranslator.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;

namespace MobirisePageTranslator.Shared.ViewModels
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private StorageFile _mobiriseProjectFile;
        private bool _canAddLanguage = true;
        private bool _canParseNewLanguagesToProject;
        private CultureInfo _currentSelectedLanguageCulture;

        public IReadOnlyCollection<CultureInfo> AvailableLanguages { get; } = new SupportedLanguages();

        public ObservableCollection<CultureInfo> AddedLanguages { get; } = new ObservableCollection<CultureInfo>();

        public CultureInfo CurrentSelectedLanguageCulture 
        {
            get => _currentSelectedLanguageCulture;
            set
            {
                if (!Equals(_currentSelectedLanguageCulture, value))
                {
                    _currentSelectedLanguageCulture = value;
                    CanAddLanguage = !AddedLanguages.Contains(_currentSelectedLanguageCulture);
                    RaisePropertyChanged();
                }
            }
        }

        public string ProjectFilePath { get; set; }

        public MainViewModel()
        {
            _currentSelectedLanguageCulture = AvailableLanguages.FirstOrDefault(x =>
                x.ThreeLetterISOLanguageName == CultureInfo.CurrentCulture.ThreeLetterISOLanguageName);

            AddLanguage = new Action(AddLanguageLogic);
            RemoveLanguage = new Action<object>(RemoveLanguageLogic);
            ParseNewLanguagesToProject = new Action(MobiriseProjectViewModel.Get.ParseNewPagesToProject);
            OpenProjectFileSearchDialog = new Action(SearchProjectFile);
        }

        private void RemoveLanguageLogic(object parameter)
        {
            var languageCulture = parameter as CultureInfo;

            if (languageCulture != null && AddedLanguages.Contains(languageCulture))
            {
                AddedLanguages.Remove(languageCulture);
                CanAddLanguage = !AddedLanguages.Contains(_currentSelectedLanguageCulture);
            }
        }

        private void AddLanguageLogic()
        {
            if (_currentSelectedLanguageCulture != null && !AddedLanguages.Contains(_currentSelectedLanguageCulture))
            {
                CanAddLanguage = false;
                AddedLanguages.Add(_currentSelectedLanguageCulture);
                StartMobiriseProjectParser();
            }
        }

        private void SearchProjectFile()
        {
#if NETFX_CORE

            UWP_SearchMobiriseProjectFile()
                .ContinueWith(tsk =>
                {
                    Sync(() =>
                    {
                        ProjectFilePath = tsk.Result?.Path ?? string.Empty;
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

#elif __MACOS__


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
            //TranslateButton.IsEnabled = false;
            //LanguageSelectorComboBox.IsEnabled = false;
            //AddLanguageButton.IsEnabled = false;
            ProjectFilePath = string.Empty;
            MobiriseProjectViewModel.Get.CleanUp();
        }

        private void EnableLanguageSelection()
        {
            //LanguageSelectorComboBox.IsEnabled = true;
            //AddLanguageButton.IsEnabled = true;
        }

        public Action AddLanguage { get; }

        public bool CanAddLanguage 
        {
            get => _canAddLanguage;
            set
            {
                if (!Equals(_canAddLanguage, value))
                {
                    _canAddLanguage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public Action<object> RemoveLanguage { get; }

        public Action ParseNewLanguagesToProject { get; }

        public Action OpenProjectFileSearchDialog { get; }

        public bool CanParseNewLanguagesToProject 
        {
            get => _canParseNewLanguagesToProject; 
            set
            {
                if (!Equals(_canParseNewLanguagesToProject, value))
                {
                    _canParseNewLanguagesToProject = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void StartMobiriseProjectParser()
        {
            if (AddedLanguages.Count == 1)
                MobiriseProjectViewModel.Get.Initialize(_mobiriseProjectFile, AddedLanguages);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
