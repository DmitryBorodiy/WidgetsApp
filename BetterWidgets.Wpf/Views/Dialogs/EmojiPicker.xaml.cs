using Emoji.Wpf;
using Wpf.Ui.Controls;
using System.Windows;
using System.Windows.Controls;
using BetterWidgets.ViewModel.Components;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Extensions;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace BetterWidgets.Views.Dialogs
{
    public partial class EmojiPicker : Page
    {
        private readonly ILogger _logger;

        public EmojiPicker()
        {
            _logger = App.Services?.GetRequiredService<ILogger<EmojiPicker>>();

            InitializeComponent();
        }

        #region PropsRegistration

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
           nameof(SelectedGroup),
           typeof(EmojiGroupViewModel),
           typeof(EmojiPicker),
           new PropertyMetadata(null));

        internal static readonly DependencyPropertyKey EmojiGroupsPropertyKey = DependencyProperty.RegisterReadOnly(
           nameof(EmojiGroups),
           typeof(ObservableCollection<EmojiGroupViewModel>),
           typeof(EmojiPicker),
           new PropertyMetadata(null));
        internal static readonly DependencyProperty EmojiGroupsProperty = EmojiGroupsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register(
            nameof(Selected),
            typeof(string),
            typeof(EmojiPicker),
            new PropertyMetadata(string.Empty));

        #endregion

        #region Props

        public string Selected
        {
            get => (string)GetValue(SelectedProperty);
            set => SetValue(SelectedProperty, value);
        }

        internal ObservableCollection<EmojiGroupViewModel> EmojiGroups
        {
            get => (ObservableCollection<EmojiGroupViewModel>)GetValue(EmojiGroupsProperty);
            set => SetValue(EmojiGroupsPropertyKey, value);
        }

        public EmojiGroupViewModel SelectedGroup
        {
            get => (EmojiGroupViewModel)GetValue(SelectedGroupProperty);
            set => SetValue(SelectedGroupProperty, value);
        }

        #endregion

        #region EventsRegistration

        public static readonly RoutedEvent EmojiPickedEvent = EventManager.RegisterRoutedEvent(
            nameof(EmojiPicked),
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(EmojiPicker));

        #endregion

        #region Events

        public event RoutedEventHandler EmojiPicked
        {
            add { AddHandler(EmojiPickedEvent, value); }
            remove { RemoveHandler(EmojiPickedEvent, value); }
        }

        #endregion

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EmojiGroups = new ObservableCollection<EmojiGroupViewModel>
                (EmojiData.AllGroups.Select(g => new EmojiGroupViewModel(g)));
            SelectedGroup = EmojiGroups.FirstOrDefault();
        }

        private void OnEmojiPicked(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count <= 0) return;

            if(e.AddedItems[0] is EmojiViewModel emoji)
            {
                Selected = emoji.Symbol;

                RaiseEvent(new RoutedEventArgs(EmojiPickedEvent));
            }
        }

        private void OnSearch(object sender, TextChangedEventArgs args)
        {
            try
            {
                if(SelectedGroup == null) return;
                if(sender is TextBox senderView)
                {
                    bool isNotEmpty = !string.IsNullOrEmpty(senderView.Text);
                    var originalGroup = EmojiData.AllGroups.FirstOrDefault(g => g.Name == SelectedGroup.Title);

                    SelectedGroup.Emojis = isNotEmpty ?
                        originalGroup.EmojiList.Where(e => e.Name?.Contains(senderView.Text, StringComparison.OrdinalIgnoreCase) ?? false)
                                               .Select(e => new EmojiViewModel(e))
                                               .ToObservableCollection() :
                        originalGroup.EmojiList.Select(e => new EmojiViewModel(e))
                                               .ToObservableCollection();
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }
    }
}
