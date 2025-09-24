using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Extensions;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Views;
using BetterWidgets.Views.Settings;
using BetterWidgets.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace BetterWidgets.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger Logger;
        private readonly Settings Settings;
        public readonly IWidgetManager WidgetManager;
        private readonly IMSAccountInformation AccountInformation;
        private readonly IMSGraphService GraphService;
        private readonly IStoreService Store;
        private readonly ISearchService Search;
        private readonly ISettingsManager SettingsManager;
        private readonly IThemeService Theme;
        #endregion

        public MainWindowViewModel()
        {
            Logger = App.Services?.GetService<ILogger<MainWindowViewModel>>();
            Settings = App.Services?.GetService<Settings>();
            Frame = App.Services?.GetService<Frame>();
            AccountInformation = App.Services?.GetService<IMSAccountInformation>();
            GraphService = App.Services?.GetService<IMSGraphService>();
            Store = App.Services?.GetService<IStoreService>();
            Search = App.Services?.GetService<ISearchService>();
            SettingsManager = App.Services?.GetService<ISettingsManager>();
            Theme = App.Services?.GetService<IThemeService>();

            WidgetManager = Services.WidgetManager.Current;
            SelectedWidgetItem = Widgets.FirstOrDefault(w => w.Id == SelectedWidgetId) ??
                                 Widgets.FirstOrDefault();

            Settings.ValueChanged += OnSettingChanged;

            if(GraphService != null)
            {
                GraphService.SignedIn += GraphService_SignedIn;
                GraphService.SignedOut += GraphService_SignedOut;
            }
        }

        #region Props

        [ObservableProperty] public Frame frame;

        #region WidgetsProps

        public ObservableCollection<WidgetViewItem> Widgets
            => WidgetManager?.Widgets?.Values.Where(w => !w.DevMode)
                                             .Select(w => new WidgetViewItem(w, WidgetManager))?
                                             .ToObservableCollection();

        public ObservableCollection<WidgetViewItem> TrayWidgets
            => WidgetManager?.Widgets?.Values.Where(w => !w.DevMode)
                                             .Select(w => new WidgetViewItem(w, WidgetManager))?
                                             .Where(w => w.IsShowInTray)
                                             .ToObservableCollection();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsSearch), nameof(IsWidgetsCollectionVisible))]
        public string searchText;

        [ObservableProperty]
        public ObservableCollection<SearchView> searchItemsSource;

        [ObservableProperty]
        public SearchView selectedSearchView;

        [ObservableProperty]
        public Widget previewWidget;

        private WidgetViewItem selectedWidgetItem;
        public WidgetViewItem SelectedWidgetItem
        {
            get => selectedWidgetItem;
            set => SetSelectedWidget(value);
        }

        private Guid SelectedWidgetId
        {
            get
            {
                if(Settings.ContainsKey(nameof(SelectedWidgetId)))
                   return new Guid(Settings.GetValue<string>(nameof(SelectedWidgetId)));
                else
                   return default;
            }
            set => Settings.SetValue(nameof(SelectedWidgetId), value.ToString());
        }

        public bool IsSearch => !string.IsNullOrEmpty(SearchText);

        #endregion

        #region SidebarProps

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PaneColumnMinWidth))]
        [NotifyPropertyChangedFor(nameof(PaneColumnWidth))]
        [NotifyPropertyChangedFor(nameof(TitleBarMargin))]
        public bool isPaneOpened = true;

        public double PaneColumnMaxWidth => Values.MenuPaneMaxWidth;
        public double PaneColumnMinWidth => IsPaneOpened ? Values.MenuPaneMinWidth : 0;

        public GridLength PaneColumnWidth
        {
            get => IsPaneOpened ? new GridLength(Settings.SidebarPaneWidth) : new GridLength(0);
            set => Settings.SidebarPaneWidth = value.Value;
        }

        public Thickness TitleBarMargin => IsPaneOpened ?
                                           new Thickness(0) :
                                           new Thickness(90, 0, 0, 0);

        public bool IsWidgetsCollectionVisible => !IsSearch;

        #endregion

        #region AccountProps

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AccountAvatar))]
        public Account account;

        [ObservableProperty]
        public bool isAccountFlyoutOpen = false;

        public ImageSource AccountAvatar => Account?.AvatarImageSource;

        [ObservableProperty]
        public bool isAccountInformationLoading;

        #endregion

        #region MessageBarProps

        private bool isMessageBarOpen = false;
        public bool IsMessageBarOpen
        {
            get => isMessageBarOpen;
            set => SetMessageBarOpen(value);
        }

        [ObservableProperty]
        public string messageBarTitle = string.Empty;

        [ObservableProperty]
        public string messageBarSubtitle = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMessageBarCommandEnabled))]
        public string messageBarActionText = string.Empty;

        [ObservableProperty]
        public InfoBarSeverity messageBarSeverity = InfoBarSeverity.Warning;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsMessageBarCommandEnabled))]
        public ICommand messageBarCommand;

        public bool IsMessageBarCommandEnabled => MessageBarCommand != null &&
                                                  !string.IsNullOrEmpty(MessageBarActionText);

        #endregion

        #region Theme

        public SolidColorBrush SolidAltThemeBrush => Theme.GetTheme() == ApplicationTheme.Dark ?
            new SolidColorBrush(Color.FromArgb(130, 0, 0, 0)) : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));

        #endregion

        #endregion

        #region Commands

        public ICommand NavigateCommand => new RelayCommand<Type>(Navigate);
        public ICommand TogglePaneCommand => new RelayCommand<ResourceDictionary>(TogglePane);

        public ICommand AccountFlyoutCommand => new RelayCommand<Button>(SwitchAccountFlyout);

        public ICommand LoadedCommand => new AsyncRelayCommand(OnLoadedAsync);
        public ICommand SignInCommand => new AsyncRelayCommand(SignInAsync);
        public ICommand SignOutCommand => new AsyncRelayCommand(async () => await GraphService.SignOutAsync());

        [RelayCommand]
        private async Task RateReviewAsync() => await Store?.RateReviewAsync();

        [RelayCommand]
        private void ActivateMainWindow()
        {
            var shell = ShellHelper.GetAppShell();

            if(shell.WindowState != WindowState.Normal)
               shell.WindowState = WindowState.Normal;

            shell.Show();
            shell.Activate();
            shell.Focus();
        }

        [RelayCommand]
        private void RefreshTrayWidgetsList() => OnPropertyChanged(nameof(TrayWidgets));

        #endregion

        #region Methods

        private void TogglePane(ResourceDictionary parameter)
        {
            try
            {
                if(parameter == null) return;

                IsPaneOpened = !IsPaneOpened;

                if(!IsPaneOpened)
                   ((Storyboard)parameter["ShowPaneAnimation"])?.Begin();
                else
                   ((Storyboard)parameter["HidePaneAnimation"])?.Begin();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private void SetSelectedWidget(WidgetViewItem widget)
        {
            selectedWidgetItem = widget;

            if(WidgetManager.CurrentPreview != null)
            {
                if(!WidgetManager.IsActivated(WidgetManager.CurrentPreview.Id)) 
                   WidgetManager.CurrentPreview.UnpinCommand?.Execute(default);
            }

            if(widget != null)
            {
                SelectedWidgetId = widget.Id;
                PreviewWidget = WidgetManager?.CreatePreview(widget.WidgetType);

                if(PreviewWidget == null) return;

                Frame.NavigationService.Navigate(new WidgetDetailView(PreviewWidget, WidgetManager, Settings));
            }

            OnPropertyChanged(nameof(SelectedWidgetItem));
        }

        private void Navigate(Type parameter)
        {
            if(parameter == null) throw new ArgumentNullException(Errors.TypeWasNull);

            var page = App.Services?.GetRequiredService(parameter);

            ActivateMainWindow();

            SelectedWidgetItem = null;
            Frame.NavigationService.Navigate(page);
        }

        private void SwitchAccountFlyout(Button accountCommand)
        {
            IsAccountFlyoutOpen = Account != null && AccountInformation.IsSignedIn ?
                                  !IsAccountFlyoutOpen : false;

            accountCommand.ContextMenu.IsOpen = !IsAccountFlyoutOpen;
        }

        private void SetMessageBarOpen(bool value)
        {
            isMessageBarOpen = value;
            OnPropertyChanged(nameof(IsMessageBarOpen));

            if(!value)
            {
                MessageBarTitle = string.Empty;
                MessageBarSubtitle = string.Empty;
                MessageBarSeverity = default;
                MessageBarActionText = string.Empty;
                MessageBarCommand = null;
            }
        }

        #endregion

        #region Tasks

        private async Task OnLoadedAsync()
        {
            await RefreshAccountInformationAsync();
        }

        private async Task RefreshAccountInformationAsync()
        {
            if(GraphService.IsSignedIn && GraphService.Client != null)
               Account = await AccountInformation?.GetAccountInformationAsync();
        }

        private async Task SignInAsync()
        {
            IsAccountInformationLoading = true;

            await GraphService.SignInAsync();

            IsAccountInformationLoading = false;
        }

        private async Task SearchAsync(string query)
        {
            try
            {
                var result = await Search?.SearchAsync(query);

                if(result.ex != null) throw result.ex;
                if(result.results != null)
                   SearchItemsSource = new ObservableCollection<SearchView>
                      (result.results.Select(s => new SearchView(s)).Reverse());
            }
            catch(Exception ex)
            {
                Logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region EventHandler

        private void GraphService_SignedIn(object sender, EventArgs e)
        {
            LoadedCommand?.Execute(default);
        }

        private void GraphService_SignedOut(object sender, EventArgs e)
        {
            Account = null;
            IsAccountFlyoutOpen = false;
        }

        async partial void OnSearchTextChanged(string value)
        {
            if(!string.IsNullOrEmpty(value)) await SearchAsync(value);
            else SearchItemsSource = null;
        }

        partial void OnSelectedSearchViewChanged(SearchView value)
        {
            if(value == null) return;
            if(value.Tag == null) throw new InvalidOperationException(Errors.TheTagWasNull);

            switch(value.Tag)
            {
                case WidgetMetadata widget:

                    SelectedWidgetItem = Widgets.FirstOrDefault(w => w.Id == widget.Id);

                    break;
                case ISetting setting:

                    var category = SettingsManager.Categories.FirstOrDefault
                        (c => c.Settings.Any(s => s.Id == setting.Id));

                    if(category != null)
                    {
                        Frame.NavigationService.Navigate(category);
                        category.FindSetting(setting.Id);

                        return;
                    }

                    var widgetCategory = SettingsManager.WidgetSettings.FirstOrDefault
                        (c => c.Settings.Any(s => s.Id == setting.Id));

                    if(widgetCategory != null)
                    {
                        bool success = Frame.NavigationService.Navigate(widgetCategory);
                        
                        if(success)
                           widgetCategory.FindSetting(setting.Id);
                    }

                    break;
            }
        }

        private void OnSettingChanged(object sender, string e)
        {
            if(e == nameof(Settings.IsDarkMode))
            {
                OnPropertyChanged(nameof(SolidAltThemeBrush));
            }
        }

        #endregion
    }
}
