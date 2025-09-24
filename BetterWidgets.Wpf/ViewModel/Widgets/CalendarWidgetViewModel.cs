using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Extensions;
using BetterWidgets.Extensions.Appointments;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using BetterWidgets.Resources;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.ViewModel.Dialogs;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Windows.Storage;
using Windows.System;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class CalendarWidgetViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger<CalendarWidget> _logger;
        private readonly IMSGraphService _graph;
        private readonly Settings<CalendarWidget> _settings;
        private readonly ICalendarService<CalendarWidget> _msCalendar;
        private readonly IPermissionManager<CalendarWidget> _permissions;
        private readonly IShareService _share;
        #endregion

        public CalendarWidgetViewModel()
        {
            _logger = App.Services?.GetRequiredService<ILogger<CalendarWidget>>();
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();
            _msCalendar = App.Services?.GetService<ICalendarService<CalendarWidget>>();
            _graph = App.Services?.GetService<IMSGraphService>();
            _permissions = App.Services?.GetService<IPermissionManager<CalendarWidget>>();
            _share = App.Services?.GetService<IShareService>();

            if(_settings != null)
               _settings.ValueChanged += OnSettingsValueChanged;

            if(_graph != null)
            {
                IsSignedIn = _graph.IsSignedIn;

                _graph.SignedIn += GraphSignedIn;
                _graph.SignedOut += GraphSignedOut;
            }
        }

        #region Fields
        private int _month = DateTime.Now.Month;
        private DateTime _selectedDateTime = DateTime.Now;
        private readonly string eventsFileStore = nameof(eventsFileStore);
        #endregion

        #region Props

        private Widget _widget;
        private Widget Widget
        {
            get => _widget;
            set
            {
                _widget = value;

                OnPropertyChanged(nameof(CanManipulateEvents));
            }
        }

        private WidgetSizes Size { get; set; } = WidgetSizes.Medium;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanManipulateEvents))]
        public bool isSignedIn;

        [ObservableProperty]
        public bool isClickable = true;

        [ObservableProperty]
        public DateViewModel selectedDate;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAppointmentSelected))]
        public AppointmentViewModel selectedAppointment;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectedDateTime), nameof(SelectedDate), nameof(HasAppointments))]
        public ObservableCollection<AppointmentViewModel> appointmentViews;

        private Model.Calendar selectedCalendar;
        public Model.Calendar SelectedCalendar
        {
            get => selectedCalendar;
            set => SetCalendar(value);
        }

        public bool HasAppointments => AppointmentViews?.Any() ?? false;

        [ObservableProperty]
        public bool isTitleBarEnabled = true;

        private Visibility rootVisibility = Visibility.Visible;
        public Visibility RootVisibility
        {
            get => rootVisibility;
            set => SetProperty(ref rootVisibility, value);
        }

        public DateTime SelectedDateTime
        {
            get => _selectedDateTime;
            set => SetSelectedDateTime(value);
        }

        public bool CanManipulateEvents => IsSignedIn && (!Widget?.IsPreview ?? false);
        public string TimeFormat => _settings?.GetSetting(nameof(TimeFormat), TimeFormatValidator.Default);
        public string DateFormat => _settings?.GetSetting(nameof(DateFormat), "dd MMM");

        public bool IsAppointmentSelected => SelectedAppointment != null;

        #endregion

        #region Utils

        private void ShowPermissionMessage()
        {
            Widget?.ShowNotify
            (
                Resources.Resources.AppointmentsPermissionRequestSubtitle,
                Resources.Resources.Appointments,
                true,
                InfoBarSeverity.Warning,
                true,
                TimeSpan.FromSeconds(6)
            );
        }

        private PermissionState CheckPermissionState()
        {
            var permissionState = _permissions?.TryCheckPermissionState(new Permission(Scopes.Appointments, PermissionLevel.HighLevel));

            if(permissionState != PermissionState.Allowed) ShowPermissionMessage();
            else Widget?.HideNotify();

            return permissionState ?? PermissionState.Undefined;
        }

        private async void SetSelectedDateTime(DateTime value)
        {
            if(value == DateTime.MinValue) return;

            _selectedDateTime = value;
            SelectedDate = new DateViewModel(value);
            OnPropertyChanged(nameof(SelectedDateTime));

            AppointmentViews = await GetCalendarAppointmentsAsync(value.Month != _month, value);

            _month = value.Month;
        }

        private async void SetCalendar(Model.Calendar value)
        {
            if(value == selectedCalendar) return;

            SetProperty(ref selectedCalendar, value, nameof(SelectedCalendar));

            AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task OnAppeared(Widget widget)
        {
            Widget = widget;

            if(widget != null)
            {
                OnSizeChanged(widget.Size);

                DependencyPropertyDescriptor.FromProperty(Widget.IsNetworkAvailableProperty, typeof(Widget))
                                            .AddValueChanged(widget, OnNetworkChanged);

                if(CheckPermissionState() == PermissionState.Allowed)
                   AppointmentViews = await GetCalendarAppointmentsAsync(false, SelectedDateTime);
            }
        }

        [RelayCommand]
        private void OnSizeChanged(Size size)
        {
            if(Widget == null) return;

            var newSize = WidgetSize.GetSize(size);

            if(Size == newSize) return;

            Size = newSize;
            IsClickable = newSize != WidgetSizes.Small;

            foreach(var element in ((Panel)Widget.Content).Children)
            {
                if(element is FrameworkElement control)
                {
                    if(control.Resources.Contains(newSize.ToString()) &&
                       control.Resources[newSize.ToString()] is Style style)
                       control.Style = style;
                }
            }
        }

        [RelayCommand]
        private async Task OnPinnedAsync(Widget widget)
        {
            if(widget != null)
            {
                var permission = new Permission(Scopes.Appointments);
                var permissionState = await _permissions.RequestAccessAsync(permission);

                if(permissionState == PermissionState.Allowed)
                   AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
                else
                   ShowPermissionMessage();
            }
        }

        [RelayCommand]
        private void OnUnpin(Widget widget)
        {
            if(widget != null)
               AppointmentViews = null;
        }

        [RelayCommand]
        private async Task RefreshAsync() => await GetCalendarAppointmentsAsync(true, SelectedDateTime);

        [RelayCommand]
        private void LaunchSettings()
        {
            if(Widget == null) return;

            ShellHelper.LaunchSettingsById(Widget.Id, default);
        }

        [RelayCommand]
        private void RequestDeleteAppointment()
        {
            if(SelectedAppointment == null) return;
            if(_msCalendar == null) return;

            var dialogParams = new WidgetContentDialogParams
            {
                Title = Resources.Resources.DeleteAppointmentTitle,
                Content = Resources.Resources.DeleteAppointmentSubtitle,
                PrimaryButtonContent = Resources.Resources.Yes,
                SecondaryButtonContent = Resources.Resources.No,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                SecondaryButtonAppearance = ControlAppearance.Secondary,
                PrimaryButtonCommand = DeleteAppointmentCommand,
                PrimaryButtonParameter = SelectedAppointment,
                SecondaryButtonVisibility = Visibility.Visible
            };

            Widget?.ShowContentDialog(dialogParams);
        }

        [RelayCommand]
        private async Task DeleteAppointment(AppointmentViewModel appointment)
        {
            await _msCalendar.DeleteAppointmentAsync(appointment.Id);

            AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
        }

        [RelayCommand]
        private void CreateAppointment()
        {
            if(Widget == null) return;

            var model = new AppointmentViewModel()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1.5)
            };

            var editViewModel = new EditAppointmentViewModel(model, Widget);
            var view = new EditAppointmentView(editViewModel);

            Widget.ShowContentDialog(new ()
            {
                Content = view,
                Title = Resources.Resources.CreateEvent,
                PrimaryButtonContent = Resources.Resources.Create,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                SecondaryButtonVisibility = Visibility.Visible,
                PrimaryButtonParameter = model,
                PrimaryButtonCommand = PostAppointmentCommand
            });
        }

        [RelayCommand]
        private async Task PostAppointmentAsync(AppointmentViewModel appointment)
        {
            if(appointment == null) return;
            if(!NetworkHelper.IsConnected) return;

            var request = appointment.ToMSAppointmentRequest();
            var creationResult = await _msCalendar.CreateAppointmentAsync(request);

            if(creationResult.ex != null)
               _logger?.LogError(creationResult.ex, creationResult.ex.Message, creationResult.ex.StackTrace);
            if(creationResult.appointment == null) return;

            if(creationResult.appointment.Start?.Date == SelectedDateTime.Date)
               AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
        }

        [RelayCommand]
        private void ShowCalendarPicker()
        {
            if(Widget == null) return;

            var picker = new PickCalendarView();

            var dialog = new WidgetContentDialogParams()
            {
                Content = picker,
                PrimaryButtonContent = Resources.Resources.SelectLabel,
                PrimaryButtonCommand = picker.VM?.SelectCalendarCommand,
                PrimaryButtonParameter = this,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                SecondaryButtonVisibility = Visibility.Visible,
                Title = Resources.Resources.SelectCalendar
            };

            Widget.ShowContentDialog(dialog);
        }

        [RelayCommand]
        private void RequestAppointmentUpdate()
        {
            if(Widget == null) return;

            var editViewModel = new EditAppointmentViewModel(SelectedAppointment, Widget);
            var view = new EditAppointmentView(editViewModel);

            Widget.ShowContentDialog(new()
            {
                Content = view,
                Title = Resources.Resources.Edit,
                PrimaryButtonContent = Resources.Resources.Save,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                SecondaryButtonVisibility = Visibility.Visible,
                PrimaryButtonParameter = SelectedAppointment,
                PrimaryButtonCommand = UpdateAppointmentCommand
            });
        }

        [RelayCommand]
        private async Task UpdateAppointmentAsync(AppointmentViewModel appointment)
        {
            if(appointment == null) return;

            var request = appointment.ToMSAppointmentRequest();
            var updateResult = await _msCalendar?.UpdateAppointmentAsync(request);

            if(updateResult.ex != null)
               _logger?.LogError(updateResult.ex, updateResult.ex.Message, updateResult.ex.StackTrace);

            if(updateResult.appointment.Start?.Date == SelectedDateTime.Date)
               AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
        }

        [RelayCommand]
        private async Task ViewAppointmentAsync()
        {
            if(SelectedAppointment != null)
               await Launcher.LaunchUriAsync(SelectedAppointment.WebLink.ToUri());
        }

        [RelayCommand]
        private void ShowCalendarDialog()
        {
            if(Widget == null) return;

            var viewModel = new CalendarPickerViewModel();

            viewModel.Widget = Widget;
            viewModel.SelectedDateTime = SelectedDateTime;

            var view = new CalendarPicker(viewModel);

            Widget.ShowContentDialog(new()
            {
                Content = view,
                Title = Resources.Resources.CalendarWidgetTitle,
                PrimaryButtonVisibility = Visibility.Collapsed,
                SecondaryButtonVisibility = Visibility.Collapsed,
                SecondaryButtonParameter = viewModel,
                SecondaryButtonCommand = SelectDateCommand
            });
        }

        [RelayCommand]
        private void SelectDate(CalendarPickerViewModel picker)
        {
            if(picker == null) return;

            SelectedDateTime = picker.SelectedDateTime;
        }

        [RelayCommand]
        private async Task ShareAsync()
        {
            try
            {
                if(!IsAppointmentSelected) return;
                if(SelectedAppointment.Appointment == null) return;

                var serializationResult = _msCalendar.SerializeIcs(SelectedAppointment.Appointment);

                if(serializationResult.ex != null ||
                   serializationResult.serialized == null)
                {
                    Widget?.ShowNotify(
                        title: Resources.Resources.CantShare,
                        message: Resources.Resources.CantShareSubtitle + serializationResult.ex?.Message,
                        isClosable: true,
                        severity: InfoBarSeverity.Warning);
                }

                var local = ApplicationData.Current.LocalFolder;
                var calendarFile = await local.CreateFileAsync
                (
                    $"{SelectedAppointment.TitleLabel}.ics",
                    CreationCollisionOption.ReplaceExisting
                );

                await FileIO.WriteTextAsync(calendarFile, serializationResult.serialized);

                _share.RequestShare(calendarFile, Widget, SelectedAppointment.TitleLabel, SelectedAppointment.Body);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Tasks

        private async Task<ObservableCollection<AppointmentViewModel>> GetCalendarAppointmentsAsync(bool forceRefresh = true, DateTime selectedDate = default)
        {
            try
            {
                if(!_graph.IsSignedIn) return null;
                if(_graph.Client == null) return null;
                if(selectedDate == default) selectedDate = DateTime.Now;
                if(CheckPermissionState() != PermissionState.Allowed) return null;

                var appointments = await FetchAppointmentsAsync(forceRefresh, NetworkHelper.IsConnected &&
                                                                              _graph.Client != null);

                if(appointments.ex != null) throw appointments.ex;
                if(appointments.data == null) return null;

                var views = appointments.data
                            .Where(a => selectedDate.Date >= a.Start.Value.Date &&
                                        selectedDate.Date <= a.End.Value.Date)
                            .Select(a => new AppointmentViewModel(a, TimeFormat, DateFormat))
                            .OrderByDescending(a => a.IsAllDay);

                return new ObservableCollection<AppointmentViewModel>(views);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private async Task<(IEnumerable<CalendarAppointment> data, Exception ex)> FetchAppointmentsAsync(bool forceRefresh  = false, bool fetchData = true) => await _msCalendar.GetCachedAsync
        (
            eventsFileStore, () => _msCalendar.GetAppointmentsByQuery(new()
            {
                Start = new DateTime(SelectedDateTime.Year, SelectedDateTime.Month, 1),
                End = new DateTime(
                      SelectedDateTime.Year,
                      SelectedDateTime.Month,
                      DateTime.DaysInMonth(SelectedDateTime.Year, SelectedDateTime.Month)),
                CalendarId = SelectedCalendar?.Id
            }),
            forceRefresh, fetchData
        );

        #endregion

        #region EventHandlers

        private void GraphSignedOut(object sender, EventArgs e)
        {
            IsSignedIn = false;
            AppointmentViews = null;
        }

        private async void GraphSignedIn(object sender, EventArgs e)
        {
            IsSignedIn = true;

            AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
        }

        private async void OnNetworkChanged(object sender, EventArgs e)
        {
            if(sender is Widget widget)
            {
                if(widget.IsNetworkAvailable)
                   AppointmentViews = await GetCalendarAppointmentsAsync(true, SelectedDateTime);
            }
        }

        private void OnSettingsValueChanged(object sender, string e)
        {
            if(nameof(TimeFormat) == e) OnPropertyChanged(nameof(TimeFormat));
            if(nameof(DateFormat) == e) OnPropertyChanged(nameof(DateFormat));
        }

        #endregion
    }
}
