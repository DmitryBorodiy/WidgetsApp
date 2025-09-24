using BetterWidgets.Model;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace BetterWidgets.Views.Dialogs
{
    [ObservableObject]
    public partial class TimeZonePicker : Page
    {
        public TimeZonePicker()
        {
            DataContext = this;
            Loaded += TimeZonePicker_Loaded;

            InitializeComponent();
        }

        #region Props

        [ObservableProperty]
        public TimeZoneModel timeZone;

        public IEnumerable<TimeZoneModel> TimeZones =>
            TimeZoneInfo.GetSystemTimeZones().Select(tz => new TimeZoneModel(tz));

        #endregion

        public async Task<MessageBoxResult> ShowDialogAsync() => await new MessageBox()
        {
            Content = this,
            MinWidth = 300,
            PrimaryButtonText = BetterWidgets.Resources.Resources.SelectLabel,
            Title = BetterWidgets.Resources.Resources.TimeZonesTitle,
            IsSecondaryButtonEnabled = false
        }.ShowDialogAsync();

        private void TimeZonePicker_Loaded(object sender, RoutedEventArgs e)
        {
            TimeZone = TimezonesBoxUI.Items.Cast<TimeZoneModel>().FirstOrDefault();
        }
    }
}
