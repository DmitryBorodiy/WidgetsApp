using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Services;
using BetterWidgets.Abstractions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using BetterWidgets.Views;
using BetterWidgets.Controls;
using Wpf.Ui.Designer;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Model;
using Windows.System;
using BetterWidgets.Views.Windows;

namespace BetterWidgets.ViewModel.SettingsViews
{
    public partial class SettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger Logger;
        private readonly ISettingsManager SettingsManager;
        private readonly IMSAccountInformation AccountInfo;
        private readonly IMSGraphService Graph;
        private readonly MainWindow Shell;
        #endregion

        public SettingsViewModel()
        {
            if(DesignerHelper.IsInDesignMode) return;

            SettingsManager = App.Services.GetService<ISettingsManager>();
            Logger = App.Services?.GetRequiredService<ILogger<SettingsViewModel>>();
            AccountInfo = App.Services?.GetService<IMSAccountInformation>();
            Graph = App.Services?.GetService<IMSGraphService>();
            Shell = App.Services?.GetService<MainWindow>();

            Categories = GetCategories();
            Widgets = WidgetManager.Current.Widgets.Values.Select(w => new WidgetViewItem(w, WidgetManager.Current));

            if(Graph != null)
            {
                Graph.SignedIn += OnMSGraphSignedIn;
                Graph.SignedOut += OnMSGrapgSignedOut;
            }
        }

        #region Props

        [ObservableProperty]
        public IEnumerable<ISettingCategory> categories;

        [ObservableProperty]
        public IEnumerable<WidgetViewItem> widgets;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSignedIn))]
        public Account account;

        public bool IsSignedIn => Graph?.IsSignedIn ?? false;

        #endregion

        #region Commands

        public ICommand NavigateCommand => new RelayCommand<object>(Navigate);
        public ICommand NavigateWidgetCommand => new RelayCommand<WidgetViewItem>(NavigateWidget);

        #endregion

        #region Merhods

        private IEnumerable<ISettingCategory> GetCategories()
            => SettingsManager?.Categories?.Reverse() ?? Enumerable.Empty<ISettingCategory>();

        private void Navigate(object parameter)
        {
            if(parameter == null) return;

            Shell?.VM.Frame.NavigationService?.Navigate(parameter);
        }

        private void NavigateWidget(WidgetViewItem parameter)
        {
            if(parameter == null) return;

            var page = SettingsManager?.WidgetSettings?.FirstOrDefault(p => p.Id == parameter.Id.ToString());

            if(page == null) return;

            Shell?.VM.Frame.NavigationService?.Navigate(page);
        }

        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            if(Graph?.IsSignedIn ?? false)
               Account = await AccountInfo?.GetAccountInformationAsync();
        }

        [RelayCommand]
        private async Task LaunchUriAsync(string uri)
        {
            if(!Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute)) return;

            await Launcher.LaunchUriAsync(new Uri(uri));
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            var result = await Graph.SignInAsync();

            if(result != null)
               Account = await AccountInfo?.GetAccountInformationAsync();
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await Graph.SignOutAsync();
            Account = null;
        }

        #endregion

        #region EventHandlers

        private async void OnMSGraphSignedIn(object sender, EventArgs e)
        {
            if(Graph?.IsSignedIn ?? false)
               Account = await AccountInfo?.GetAccountInformationAsync();
        }

        private void OnMSGrapgSignedOut(object sender, EventArgs e)
        {
            Account = null;
        }

        #endregion
    }
}
