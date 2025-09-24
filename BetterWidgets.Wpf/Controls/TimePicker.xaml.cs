using BetterWidgets.Enums;
using BetterWidgets.Events;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace BetterWidgets.Controls
{
    public partial class TimePicker : UserControl
    {
        public TimePicker()
        {
            Loaded += OnLoaded;

            InitializeComponent();
        }

        #region PropsRegistrations

        #region Internal

        internal static readonly DependencyPropertyKey HoursItemsSourcePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HoursItemsSource),
            typeof(string[]),
            typeof(TimePicker),
            new PropertyMetadata());

        internal static readonly DependencyProperty HoursItemsSourceProperty = HoursItemsSourcePropertyKey.DependencyProperty;

        internal static readonly DependencyProperty TimeTypeProperty = DependencyProperty.Register(
            nameof(TimeType),
            typeof(int),
            typeof(TimePicker),
            new PropertyMetadata(0, OnTimeTypeChanged));

        internal static readonly DependencyPropertyKey HoursPropertyKey = DependencyProperty.RegisterReadOnly(
           nameof(Hours),
           typeof(List<int>),
           typeof(TimePicker),
           new PropertyMetadata(null));

        internal static readonly DependencyProperty HoursProperty = HoursPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey MinutesPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Minutes),
            typeof(List<int>),
            typeof(TimePicker),
            new PropertyMetadata(null));

        internal static readonly DependencyProperty MinutesProperty = MinutesPropertyKey.DependencyProperty;

        #endregion

        public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
            nameof(SelectedTime),
            typeof(DateTime),
            typeof(TimePicker),
            new PropertyMetadata(DateTime.Now, OnSelectedTimeChanged));

        public static readonly DependencyProperty TimeFormatProperty = DependencyProperty.Register(
           nameof(TimeFormat),
           typeof(TimeFormat),
           typeof(TimePicker),
           new PropertyMetadata(TimeFormat.Hour24, OnTimeFormatChanged));

        public static readonly DependencyProperty MinutesIncrementProperty = DependencyProperty.Register(
            nameof(MinutesIncrement),
            typeof(int),
            typeof(TimePicker),
            new PropertyMetadata(0));

        #endregion

        #region Props

        #region Internals

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal string[] HoursItemsSource
        {
            get => (string[])GetValue(HoursItemsSourceProperty);
            private set => SetValue(HoursItemsSourcePropertyKey, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal int TimeType
        {
            get => (int)GetValue(TimeTypeProperty);
            private set => SetValue(TimeTypeProperty, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal List<int> Hours
        {
            get => (List<int>)GetValue(HoursProperty);
            private set => SetValue(HoursPropertyKey, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal List<int> Minutes
        {
            get => (List<int>)GetValue(MinutesProperty);
            private set => SetValue(MinutesPropertyKey, value);
        }

        #endregion

        public DateTime SelectedTime
        {
            get => (DateTime)GetValue(SelectedTimeProperty);
            set => SetValue(SelectedTimeProperty, value);
        }

        public TimeFormat TimeFormat
        {
            get => (TimeFormat)GetValue(TimeFormatProperty);
            set => SetValue(TimeFormatProperty, value);
        }

        public int MinutesIncrement
        {
            get => (int)GetValue(MinutesIncrementProperty);
            set => SetValue(MinutesIncrementProperty, value);
        }

        #endregion

        #region EventsRegistration

        public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedTimeChanged),
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(TimePicker));

        #endregion

        #region Events

        public event RoutedEventHandler SelectedTimeChanged
        {
            add { AddHandler(SelectedTimeChangedEvent, value); }
            remove { RemoveHandler(SelectedTimeChangedEvent, value); }
        }

        #endregion

        #region Utils

        private List<int> GetHours(TimeFormat format)
           => format == TimeFormat.Hour12 ?
              Enumerable.Range(1, 12).ToList() : Enumerable.Range(0, 24).ToList();

        private List<int> GetMinutes(int minuteIncrement = 0)
        {
            var minutes = Enumerable.Range(0, 60);

            if(minuteIncrement > 0)
               return minutes.Where(m => m % minuteIncrement == 0).ToList();

            return minutes.ToList();
        }

        private TimeFormat GetCurrentTimeFormat()
        {
            var dtfi = CultureInfo.CurrentCulture.DateTimeFormat;

            return string.IsNullOrEmpty(dtfi.AMDesignator) && string.IsNullOrEmpty(dtfi.PMDesignator) ?
                   TimeFormat.Hour24 : TimeFormat.Hour12;
        }

        #endregion

        #region EventHandlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Hours = GetHours(TimeFormat);
            Minutes = GetMinutes(MinutesIncrement);
            TimeFormat = GetCurrentTimeFormat();

            UIMeridiemPicker.Visibility
                = TimeFormat == TimeFormat.Hour12 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnHourSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                if(e.AddedItems[0] is int hour)
                {
                    SelectedTime = new DateTime(
                        SelectedTime.Year, SelectedTime.Month, SelectedTime.Day,
                        hour, SelectedTime.Minute, SelectedTime.Second);
                }
            }
        }

        private void OnMinuteSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                if(e.AddedItems[0] is int minute)
                {
                    SelectedTime = new DateTime(
                        SelectedTime.Year, SelectedTime.Month, SelectedTime.Day,
                        SelectedTime.Hour, minute, SelectedTime.Second);
                }
            }
        }

        private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TimePicker control)
            {
                var oldTime = e.OldValue as DateTime?;
                var newTime = e.NewValue as DateTime?;

                if(newTime.HasValue)
                {
                    int displayHour;

                    if(control.TimeFormat == TimeFormat.Hour12)
                    {
                        displayHour = newTime.Value.Hour % 12;

                        if(displayHour == 0) displayHour = 12;

                        control.TimeType = newTime.Value.Hour >= 12 ? 1 : 0;
                    }
                    else displayHour = newTime.Value.Hour;

                    control.UIHourPicker.SelectedItem = displayHour;
                    control.UIMinutePicker.SelectedItem = newTime.Value.Minute;
                }

                var args = new SelectedTimeChangedEventArgs(SelectedTimeChangedEvent, control, oldTime, newTime);
                control.RaiseEvent(args);
            }
        }

        private static void OnTimeFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TimePicker control) control.OnLoaded(control, new RoutedEventArgs());
        }

        private static void OnTimeTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TimePicker control && e.NewValue is int newType)
            {
                int hour = control.SelectedTime.Hour;

                if(control.TimeFormat == TimeFormat.Hour12)
                {
                    if(newType == 0)
                    {
                        if(hour >= 12) hour -= 12;
                    }
                    else
                    {
                        if(hour < 12) hour += 12;
                    }

                    control.SelectedTime = new DateTime(
                        control.SelectedTime.Year,
                        control.SelectedTime.Month,
                        control.SelectedTime.Day,
                        hour,
                        control.SelectedTime.Minute,
                        control.SelectedTime.Second);
                }
            }
        }

        #endregion
    }
}
