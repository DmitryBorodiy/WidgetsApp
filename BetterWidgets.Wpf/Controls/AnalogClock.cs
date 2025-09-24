using BetterWidgets.ViewModel.Components;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BetterWidgets.Controls
{
    public sealed class AnalogClock : Control
    {
        public AnalogClock()
        {
            DefaultStyleKey = typeof(AnalogClock);

            SetValue(HoursPropertyKey, new ObservableCollection<ClockHourMark>(
                Enumerable.Range(1, 12).Select(i => new ClockHourMark(i))
            ));
        }

        #region PropsRegistration

        public static readonly DependencyProperty DateTimeProperty = DependencyProperty.Register(
            nameof(DateTime),
            typeof(DateTime),
            typeof(AnalogClock),
            new PropertyMetadata(DateTime.Now, OnDateTimeChanged));

        public static readonly DependencyProperty TimezoneProperty = DependencyProperty.Register(
            nameof(Timezone),
            typeof(TimeZoneInfo),
            typeof(AnalogClock),
            new PropertyMetadata(TimeZoneInfo.Local, OnDateTimeChanged));

        public static readonly DependencyProperty IsClockFaceEnabledProperty = DependencyProperty.Register(
            nameof(IsClockFaceEnabled),
            typeof(bool),
            typeof(AnalogClock),
            new PropertyMetadata(true));

        public static readonly DependencyProperty IsSecondsArrowEnabledProperty = DependencyProperty.Register(
            nameof(IsSecondsArrowEnabled),
            typeof(bool),
            typeof(AnalogClock),
            new PropertyMetadata(true));

        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register(
            nameof(BackgroundOpacity),
            typeof(double),
            typeof(AnalogClock),
            new PropertyMetadata(1.0));

        internal static readonly DependencyProperty HourAngleProperty = DependencyProperty.Register(
            nameof(HourAngle),
            typeof(double),
            typeof(AnalogClock),
            new PropertyMetadata(0.0));

        internal static readonly DependencyProperty MinuteAngleProperty = DependencyProperty.Register(
            nameof(MinuteAngle),
            typeof(double),
            typeof(AnalogClock),
            new PropertyMetadata(0.0));

        internal static readonly DependencyProperty SecondAngleProperty = DependencyProperty.Register(
            nameof(SecondAngle),
            typeof(double),
            typeof(AnalogClock),
            new PropertyMetadata(0.0));

        private static readonly DependencyPropertyKey HoursPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Hours),
            typeof(ObservableCollection<ClockHourMark>),
            typeof(AnalogClock),
            new PropertyMetadata(null));

        internal static readonly DependencyProperty HoursProperty = HoursPropertyKey.DependencyProperty;

        #endregion

        #region Props

        public DateTime DateTime
        {
            get => (DateTime)GetValue(DateTimeProperty);
            set => SetValue(DateTimeProperty, value);
        }

        public TimeZoneInfo Timezone
        {
            get => (TimeZoneInfo)GetValue(TimezoneProperty);
            set => SetValue(TimezoneProperty, value);
        }

        public bool IsClockFaceEnabled
        {
            get => (bool)GetValue(IsClockFaceEnabledProperty);
            set => SetValue(IsClockFaceEnabledProperty, value);
        }

        public bool IsSecondsArrowEnabled
        {
            get => (bool)GetValue(IsSecondsArrowEnabledProperty);
            set => SetValue(IsSecondsArrowEnabledProperty, value);
        }

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        internal double HourAngle
        {
            get => (double)GetValue(HourAngleProperty);
            private set => SetValue(HourAngleProperty, value);
        }

        internal double MinuteAngle
        {
            get => (double)GetValue(MinuteAngleProperty);
            private set => SetValue(MinuteAngleProperty, value);
        }

        internal double SecondAngle
        {
            get => (double)GetValue(SecondAngleProperty);
            private set => SetValue(SecondAngleProperty, value);
        }

        internal ObservableCollection<ClockHourMark> Hours 
            => (ObservableCollection<ClockHourMark>)GetValue(HoursProperty);

        #endregion

        #region Utils

        private static void AnimateAngle(AnalogClock control, DependencyProperty property, double fromValue, double toValue)
        {
            if(toValue < fromValue) toValue += 360;

            var animation = new DoubleAnimation
            {
                From = fromValue,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            control.BeginAnimation(property, animation);
        }

        #endregion

        #region EventHandlers

        private static void OnDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (AnalogClock)d;

            var time = TimeZoneInfo.ConvertTime(control.DateTime, control.Timezone);

            var currentSecondAngle = control.SecondAngle % 360;
            var currentMinuteAngle = control.MinuteAngle % 360;
            var currentHourAngle = control.HourAngle % 360;

            var newSecondAngle = time.Second * 6;
            var newMinuteAngle = time.Minute * 6 + time.Second * 0.1;
            var newHourAngle = (time.Hour % 12) * 30 + time.Minute * 0.5;

            AnimateAngle(control, SecondAngleProperty, currentSecondAngle, newSecondAngle);
            AnimateAngle(control, MinuteAngleProperty, currentMinuteAngle, newMinuteAngle);
            AnimateAngle(control, HourAngleProperty, currentHourAngle, newHourAngle);
        }

        #endregion
    }
}
