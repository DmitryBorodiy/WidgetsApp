using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Helpers;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using ListView = System.Windows.Controls.ListView;

namespace BetterWidgets.Controls
{
    public class SettingsPage : Page, ISettingsPage
    {
        private readonly string UISettingsCategory = nameof(UISettingsCategory);

        public SettingsPage()
        {
            Settings = new ObservableCollection<ISetting>();
            Messages = new ObservableCollection<ErrorMessage>();
            Style = (Style)Application.Current.Resources[KnownResources.DefaultSettingPage];
            DefaultStyleKey = typeof(SettingsPage);

            Loaded += SettingsPage_Loaded;
        }

        #region UI
        private ListView SettingsCollection;
        #endregion

        #region PropsRegistractions

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            nameof(Id),
            typeof(string),
            typeof(SettingsPage),
            new PropertyMetadata(string.Empty));

        public static new readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingsPage),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(SettingsPage),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(SettingsPage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings),
            typeof(ObservableCollection<ISetting>),
            typeof(SettingsPage),
            new PropertyMetadata(null, OnItemsChanged));

        public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(
            nameof(Messages),
            typeof(ObservableCollection<ErrorMessage>),
            typeof(SettingsPage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand),
            typeof(ICommand),
            typeof(SettingsPage),
            new PropertyMetadata(new RelayCommand(ShellHelper.GoBack)));

        public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.Register(
            nameof(LoadedCommand),
            typeof(ICommand),
            typeof(SettingsPage),
            new PropertyMetadata(null));

        public static readonly DependencyProperty LoadedCommandParameterProperty = DependencyProperty.Register(
            nameof(LoadedCommandParameter),
            typeof(object),
            typeof(SettingsPage),
            new PropertyMetadata(null));

        #endregion

        #region Props

        private string FindId { get; set; }
        public SearchType SearchType => SearchType.Settings;

        public string Id
        {
            get => (string)GetValue(IdProperty);
            set => SetValue(IdProperty, value);
        }

        public new string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public ObservableCollection<ISetting> Settings 
        { 
            get => (ObservableCollection<ISetting>)GetValue(SettingsProperty);
            set => SetValue(SettingsProperty, value);
        }
        
        public ObservableCollection<ErrorMessage> Messages
        {
            get => (ObservableCollection<ErrorMessage>)GetValue(MessagesProperty);
            set => SetValue(MessagesProperty, value);
        }

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        public ICommand LoadedCommand
        {
            get => (ICommand)GetValue(LoadedCommandProperty);
            set => SetValue(LoadedCommandProperty, value);
        }

        public object LoadedCommandParameter
        {
            get => GetValue(LoadedCommandProperty);
            set => SetValue(LoadedCommandProperty, value);
        }

        #endregion

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingsPage)d;

            if(e.OldValue is ObservableCollection<ISetting> oldItems)
               oldItems.CollectionChanged -= control.OnSettingsCollectionChanged;

            if(e.NewValue is ObservableCollection<ISetting> newItems)
               newItems.CollectionChanged += control.OnSettingsCollectionChanged;
        }

        private void OnSettingsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems == null) return;

            foreach(var item in e.NewItems)
               ((FrameworkElement)item).DataContext = DataContext;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsCollection = GetTemplateChild(UISettingsCategory) as ListView;

            LoadedCommand?.Execute(LoadedCommandParameter);

            if(!string.IsNullOrEmpty(FindId)) FindSetting(FindId);
        }

        public void FindSetting(string id)
        {
            if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
            if(SettingsCollection != null && Settings != null)
            {
                var setting = Settings.FirstOrDefault(x => x.Id == id);

                if(setting is Control control)
                {
                    SettingsCollection.ScrollIntoView(control);
                    control.Focus();

                    FindId = string.Empty;
                }
            }
            else FindId = id;
        }
    }
}
