using BetterWidgets.Extensions;
using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media;
using Windows.System;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.SettingsViews
{
    public partial class AppearanceSettingsViewModel : ObservableObject
    {
        private readonly Settings _settings;

        public AppearanceSettingsViewModel()
        {
            _settings = App.Services?.GetService<Settings>();
        }

        #region Props

        public bool IsDarkMode
        {
            get => _settings?.IsDarkMode ?? false;
            set
            {
                if(_settings != null)
                   _settings.IsDarkMode = value;

                OnPropertyChanged(nameof(IsDarkMode));
            }
        }

        public double WidgetTransparency
        {
            get => _settings?.WidgetTransparency ?? 1;
            set => SetWidgetTransparency(value);
        }

        public Color AccentColor => AccentColorHelper.AccentColor;

        public bool IsCustomAccentColorEnabled
        {
            get => _settings?.GetValue(nameof(IsCustomAccentColorEnabled), false) ?? false;
            set
            {
                _settings.SetValue(nameof(IsCustomAccentColorEnabled), value);
                OnPropertyChanged(nameof(IsCustomAccentColorEnabled));
            }
        }

        #endregion

        #region Utils

        private void SetWidgetTransparency(double value)
        {
            if(_settings != null)
               _settings.WidgetTransparency = value;

            OnPropertyChanged(nameof(WidgetTransparency));

            var widgets = WidgetManager.Current.GetActivatedWidgets();

            foreach(var widget in widgets) widget.BackdropOpacity = value;
        }

        #endregion

        #region Methods

        [RelayCommand]
        private async Task LaunchUri(string parameter)
        {
            await Launcher.LaunchUriAsync(new Uri(parameter));
        }

        [RelayCommand]
        private void PickColor(string key)
        {
            ColorPickerDialog dialog = new ColorPickerDialog()
            {
                SelectedColor = AccentColorHelper.AccentColor
            };

            dialog.ShowDialog();

            if(dialog.Result == ContentDialogResult.Primary)
            {
                _settings.SetValue(key, dialog.SelectedColor.ToHex());
                AccentColorHelper.SetAccentColor(IsDarkMode, dialog.SelectedColor);

                OnPropertyChanged(nameof(AccentColor));
            }
        }

        #endregion
    }
}
