using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using BetterWidgets.Helpers;
using System.Windows.Media;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class StickyNotesSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<StickyNotesWidget> _settings;
        #endregion

        public StickyNotesSettingsViewModel()
        {
            _settings = App.Services?.GetRequiredService<Settings<StickyNotesWidget>>();
            _logger = App.Services?.GetRequiredService<ILogger<StickyNotesSettingsViewModel>>();

            SystemFonts = Fonts.SystemFontFamilies.ToList();
            SelectedFontFamilyIndex = GetDefaultFontFamilyIndex();
        }

        #region Props

        public bool IsSoundsEnabled
        {
            get => _settings.GetSetting(nameof(IsSoundsEnabled), true);
            set => _settings.SetSetting(nameof(IsSoundsEnabled), value);
        }

        public string DefaultFontFamily
        {
            get => _settings.GetSetting(nameof(DefaultFontFamily), "Georgia");
            set => _settings.SetSetting(nameof(DefaultFontFamily), value);
        }

        public int DefaultFontSize
        {
            get => _settings.GetSetting(nameof(DefaultFontSize), 15);
            set => _settings.SetSetting(nameof(DefaultFontSize), value);
        }

        [ObservableProperty]
        public int selectedFontFamilyIndex;

        [ObservableProperty]
        public List<FontFamily> systemFonts;

        public bool IsUpdateEnabled
        {
            get => _settings.GetSetting(nameof(IsUpdateEnabled), false);
            set
            {
                _settings.SetSetting(nameof(IsUpdateEnabled), value);
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public int UpdateInterval
        {
            get => _settings.GetSetting(nameof(UpdateInterval), 35);
            set => _settings.SetSetting(nameof(UpdateInterval), value);
        }

        public bool IsNoteAutosaveEnabled
        {
            get => _settings.GetSetting(nameof(IsNoteAutosaveEnabled), false);
            set => _settings.SetSetting(nameof(IsNoteAutosaveEnabled), value);
        }

        #endregion

        #region Commands

        public ICommand BackCommand => new RelayCommand(ShellHelper.GoBack);

        #endregion

        #region Utils

        private int GetDefaultFontFamilyIndex()
        {
            return SystemFonts.IndexOf
                (SystemFonts.FirstOrDefault(f => f.Source == DefaultFontFamily));
        }

        #endregion

        #region Handlers

        partial void OnSelectedFontFamilyIndexChanged(int value)
        {
            if(value >= 0)
               DefaultFontFamily = SystemFonts[value].Source;
        }

        #endregion
    }
}
