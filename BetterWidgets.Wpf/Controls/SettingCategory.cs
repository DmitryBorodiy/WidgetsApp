using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Extensions;
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
    public class SettingCategory : Page, ISettingCategory
    {
        public SettingCategory()
        {
            Settings = new ObservableCollection<ISetting>();
            Style = (Style)Application.Current.Resources[KnownResources.DefaultSettingCategory];
            DefaultStyleKey = typeof(SettingCategory);

            Loaded += OnLoaded;
        }

        public SearchType SearchType => SearchType.Settings;

        #region PropsRegistractions

        public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
            nameof(Id),
            typeof(string),
            typeof(SettingCategory),
            new PropertyMetadata(string.Empty));

        public static new readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingCategory),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(SettingCategory),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(IconElement),
            typeof(SettingCategory),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings),
            typeof(ObservableCollection<ISetting>),
            typeof(SettingCategory),
            new PropertyMetadata(null, OnSettingsChanged));

        public static readonly DependencyProperty LoadedCommandParameterProperty = DependencyProperty.Register(
            nameof(LoadedCommandParameter),
            typeof(object),
            typeof(SettingCategory),
            new PropertyMetadata(null));

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand),
            typeof(ICommand),
            typeof(SettingCategory),
            new PropertyMetadata(new RelayCommand(ShellHelper.GoBack)));

        public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.Register(
            nameof(LoadedCommand),
            typeof(ICommand),
            typeof(SettingCategory),
            new PropertyMetadata(null));

        #endregion

        #region Props

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

        public object LoadedCommandParameter
        {
            get => GetValue(LoadedCommandParameterProperty);
            set => SetValue(LoadedCommandParameterProperty, value);
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

        #endregion

        #region Methods

        public void FindSetting(string id)
        {
            if(Settings == null) return;
            if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);

            var collectionUI = this.FindChild<ListView>("UISettingsCategory");

            if(collectionUI != null)
            {
                var setting = Settings.FirstOrDefault(s => s.Id == id);
                int index = Settings.IndexOf(setting);

                collectionUI.ScrollIntoView(collectionUI.Items[index]);

                if(setting is Control settingControl)
                   settingControl?.Focus();
            }
        }

        private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingCategory)d;

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

        private void OnLoaded(object sender, RoutedEventArgs e)
            => LoadedCommand?.Execute(LoadedCommandParameter);

        #endregion
    }
}
