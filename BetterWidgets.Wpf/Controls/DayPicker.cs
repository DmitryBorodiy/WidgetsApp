using System.Windows;
using System.Windows.Controls;
using BetterWidgets.ViewModel.Components;
using System.Collections.ObjectModel;
using BetterWidgets.Events;

namespace BetterWidgets.Controls
{
    public sealed class DayPicker : Control
    {
        #region Consts
        private const string PART_PrevButton = nameof(PART_PrevButton);
        private const string PART_NextButton = nameof(PART_NextButton);
        private const string PART_DaysCollection = nameof(PART_DaysCollection);
        private const string PART_UpPrevButton = nameof(PART_UpPrevButton);
        private const string PART_DownNextButton = nameof(PART_DownNextButton);
        #endregion

        public DayPicker()
        {
            DefaultStyleKey = typeof(DayPicker);
        }

        #region PropsRegistractions

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(DayPicker),
            new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        public static readonly DependencyProperty SelectedDateViewProperty = DependencyProperty.Register(
            nameof(SelectedDateView),
            typeof(DateViewModel),
            typeof(DayPicker),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MaxiumDaysProperty = DependencyProperty.Register(
           nameof(MaxiumDays),
           typeof(int),
           typeof(DayPicker),
           new PropertyMetadata(int.MaxValue, OnMaximumDaysPropertyChanged));

        public static readonly DependencyProperty HorizontalFlipButtonsVisibilityProperty = DependencyProperty.Register(
           nameof(HorizontalFlipButtonsVisibility),
           typeof(Visibility),
           typeof(DayPicker),
           new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty VerticalFlipButtonsVisibilityProperty = DependencyProperty.Register(
           nameof(VerticalFlipButtonsVisibility),
           typeof(Visibility),
           typeof(DayPicker),
           new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyPropertyKey DaysItemsSourcePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(DaysItemsSource),
            typeof(ObservableCollection<DateViewModel>),
            typeof(DayPicker),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
            nameof(SelectedDate),
            typeof(DateTime),
            typeof(DayPicker),
            new PropertyMetadata(DateTime.Now, OnSelectedDateTimeChanged));

        public static readonly DependencyProperty DaysItemsSourceProperty = DaysItemsSourcePropertyKey.DependencyProperty;

        #endregion

        #region Props

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public int MaxiumDays
        {
            get => (int)GetValue(MaxiumDaysProperty);
            set => SetValue(MaxiumDaysProperty, value);
        }

        public ObservableCollection<DateViewModel> DaysItemsSource
        {
            get => (ObservableCollection<DateViewModel>)GetValue(DaysItemsSourceProperty);
            private set => SetValue(DaysItemsSourcePropertyKey, value);
        }

        public DateViewModel SelectedDateView
        {
            get => (DateViewModel)GetValue(SelectedDateViewProperty) ?? new DateViewModel();
            set => SetValue(SelectedDateViewProperty, value);
        }

        public Visibility HorizontalFlipButtonsVisibility
        {
            get => (Visibility)GetValue(HorizontalFlipButtonsVisibilityProperty);
            set => SetHorizontalFlipButtonsVisibility(value);
        }

        public Visibility VerticalFlipButtonsVisibility
        {
            get => (Visibility)GetValue(VerticalFlipButtonsVisibilityProperty);
            set => SetVerticalFlipButtonsVisibility(value);
        }

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        #endregion

        #region EventsRegistration

        public static readonly RoutedEvent SelectedDateChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedDateChanged),
            RoutingStrategy.Bubble,
            typeof(EventHandler<SelectedDateChangedEventArgs>),
            typeof(DayPicker));

        #endregion

        #region Events

        public event EventHandler<SelectedDateChangedEventArgs> SelectedDateChanged
        {
            add { AddHandler(SelectedDateChangedEvent, value); }
            remove { RemoveHandler(SelectedDateChangedEvent, value); }
        }

        #endregion

        #region Utils

        private bool IsLastDay()
            => SelectedDateView?.DateTime.Day == 
               DateTime.DaysInMonth(SelectedDateView.DateTime.Year, SelectedDateView.DateTime.Month);

        private bool IsFirstDay() => SelectedDateView?.DateTime.Day == 1;

        private void UpdateDaysCollection(DateTime baseDate)
        {
            if(DaysItemsSource == null)
               DaysItemsSource = new ObservableCollection<DateViewModel>();

            DaysItemsSource.Clear();

            var year = baseDate.Year;
            var month = baseDate.Month;
            var daysInMonth = DateTime.DaysInMonth(year, month);

            var startDay = baseDate.Day;
            var maxAvailableDays = daysInMonth - startDay + 1;
            var count = Math.Min(MaxiumDays, maxAvailableDays);

            for(int i = 0; i < count; i++)
            {
                var date = baseDate.Date.AddDays(i);
                DaysItemsSource.Add(new DateViewModel(date));
            }
        }

        private static void OnMaximumDaysPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is DayPicker picker)
            {
                picker.UpdateDaysCollection(picker.SelectedDateView?.DateTime ?? picker.SelectedDate);
                picker.SetValue(SelectedDateProperty, picker.SelectedDate);
            }
        }

        private void SetSelectedDate(DateTime value)
        {
            UpdateDaysCollection(value);

            var day = DaysItemsSource.FirstOrDefault(d => d.DateTime.Date == value.Date);
            
            SetValue(SelectedDateViewProperty, day);
            RaiseEvent(new SelectedDateChangedEventArgs(SelectedDateChangedEvent, value));
        }

        private void SetHorizontalFlipButtonsVisibility(Visibility value)
        {
            if(Orientation == Orientation.Vertical)
            {
                SetValue(HorizontalFlipButtonsVisibilityProperty, Visibility.Collapsed);

                return;
            }
            else SetValue(HorizontalFlipButtonsVisibilityProperty, value);
        }

        private void SetVerticalFlipButtonsVisibility(Visibility value)
        {
            if(Orientation == Orientation.Horizontal)
            {
                SetValue(VerticalFlipButtonsVisibilityProperty, Visibility.Collapsed);

                return;
            }
            else SetValue(HorizontalFlipButtonsVisibilityProperty, value);
        }

        #endregion

        #region EventHandlers

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            if(!IsLastDay())
               SelectedDate = SelectedDate.AddDays(1);
            else
               SelectedDate = new DateTime(
                   SelectedDate.Year,
                   SelectedDate.Month,
                   1).AddMonths(1);
        }

        private void OnPreviousButtonClick(object sender, RoutedEventArgs e)
        {
            if(!IsFirstDay())
               SelectedDate = SelectedDate.AddDays(-1);
            else
            {
                var prevMonth = SelectedDate.AddMonths(-1);
                int lastDay = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);

                SelectedDate = new DateTime(prevMonth.Year, prevMonth.Month, lastDay);
            }
        }

        private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is DayPicker picker)
            {
                picker.SetCurrentValue(SelectedDateProperty, e.NewValue);
                picker.SetSelectedDate((DateTime)e.NewValue);
            }
        }

        private void OnDayCollectionMouseDown(object sender, RoutedEventArgs e)
        {
            if(((FrameworkElement)e.OriginalSource).DataContext is DateViewModel dateView)
               SelectedDate = dateView.DateTime;
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is DayPicker picker)
            {
                bool isHorizontal = ((Orientation)e.NewValue) == Orientation.Horizontal;

                Visibility verticalVisibility = isHorizontal ? Visibility.Collapsed : Visibility.Visible;
                Visibility horizontalVisibility = isHorizontal ? Visibility.Visible : Visibility.Collapsed;

                picker.SetValue(VerticalFlipButtonsVisibilityProperty, verticalVisibility);
                picker.SetValue(HorizontalFlipButtonsVisibilityProperty, horizontalVisibility);
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild(PART_NextButton) is Wpf.Ui.Controls.Button nextBtn)
               nextBtn.Click += OnNextButtonClick;

            if(GetTemplateChild(PART_UpPrevButton) is Wpf.Ui.Controls.Button prevUpBtn)
               prevUpBtn.Click += OnPreviousButtonClick;

            if(GetTemplateChild(PART_PrevButton) is Wpf.Ui.Controls.Button prevBtn)
               prevBtn.Click += OnPreviousButtonClick;

            if(GetTemplateChild(PART_DownNextButton) is Wpf.Ui.Controls.Button nextDownBtn)
               nextDownBtn.Click += OnNextButtonClick;

            if (GetTemplateChild(PART_DaysCollection) is ListBox daysUI)
               daysUI.AddHandler(UIElement.MouseLeftButtonUpEvent, new RoutedEventHandler(OnDayCollectionMouseDown), true);
        }
    }
}
