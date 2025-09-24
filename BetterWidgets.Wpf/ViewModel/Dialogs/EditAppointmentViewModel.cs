using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Controls;
using BetterWidgets.Properties;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class EditAppointmentViewModel : ObservableObject
    {
        #region Services
        private readonly Widget _widget;
        private readonly Settings<CalendarWidget> _settings;
        #endregion

        public EditAppointmentViewModel() : this(null, null) { }
        
        public EditAppointmentViewModel(AppointmentViewModel appointment, Widget widget)
        {
            _widget = widget;
            _settings = App.Services?.GetService<Settings<CalendarWidget>>();

            Appointment = appointment;

            Title = appointment.TitleLabel;
            SelectedStartDate = appointment.StartDate.Date;
            SelectedStartTime = appointment.StartDate;
            SelectedEndDate = appointment.EndDate.Date;
            SelectedEndTime = appointment.EndDate;
        }

        #region Props

        [ObservableProperty]
        public bool hasErrors;

        [ObservableProperty]
        public AppointmentViewModel appointment;

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetTitle(value);
        }

        private DateTime selectedStartDate = DateTime.Now.Date;
        public DateTime SelectedStartDate
        {
            get => selectedStartDate;
            set => SetStartDate(value);
        }

        private DateTime selectedEndDate = DateTime.Now.Date;
        public DateTime SelectedEndDate
        {
            get => selectedEndDate;
            set => SetEndDate(value);
        }

        private DateTime selectedStartTime = DateTime.Now;
        public DateTime SelectedStartTime
        {
            get => selectedStartTime;
            set => SetStartTime(value);
        }

        private DateTime selectedEndTime = DateTime.Now;
        public DateTime SelectedEndTime
        {
            get => selectedEndTime;
            set => SetEndTime(value);
        }

        public bool IsAllDay
        {
            get => Appointment?.IsAllDay ?? false;
            set => SetIsAllDay(value);
        }

        public bool IsNotAllDay => !IsAllDay;

        #endregion

        #region Utils

        private void SetIsValid(bool isValid)
        {
            if(_widget != null)
               _widget.IsDialogPrimaryButtonEnabled = isValid;
        }

        private void SetTitle(string value)
        {
            SetIsValid(!string.IsNullOrEmpty(value));

            if(SetProperty(ref title, value, nameof(Title)))
               Appointment.TitleLabel = value;
        }

        private void SetStartDate(DateTime value)
        {
            if(value > SelectedEndDate) value = SelectedEndDate;

            if(SetProperty(ref selectedStartDate, value, nameof(SelectedStartDate)))
               Appointment.StartDate = selectedStartDate.Date + selectedStartTime.TimeOfDay;
        }

        private void SetStartTime(DateTime value)
        {
            var newStart = SelectedStartDate.Date + value.TimeOfDay;
            var currentEnd = SelectedEndDate.Date + SelectedEndTime.TimeOfDay;

            if(newStart > currentEnd)
               value = SelectedEndTime.AddMinutes(-30);

            if(SetProperty(ref selectedStartTime, value, nameof(SelectedStartTime)))
               Appointment.StartDate = SelectedStartDate.Date + selectedStartTime.TimeOfDay;
        }

        private void SetEndDate(DateTime value)
        {
            if(value < SelectedStartDate) value = SelectedStartDate;

            if(SetProperty(ref selectedEndDate, value, nameof(SelectedEndDate)))
               Appointment.EndDate = selectedEndDate.Date + selectedEndTime.TimeOfDay;
        }

        private void SetEndTime(DateTime value)
        {
            var newEnd = SelectedEndDate.Date + value.TimeOfDay;
            var currentStart = SelectedStartDate.Date + SelectedStartTime.TimeOfDay;

            if(newEnd < currentStart)
               value = SelectedStartTime.AddMinutes(30);

            if(SetProperty(ref selectedEndTime, value, nameof(SelectedEndTime)))
               Appointment.EndDate = SelectedEndDate.Date + selectedEndTime.TimeOfDay;
        }

        private void SetIsAllDay(bool value)
        {
            if(Appointment == null) return;
            if(Appointment.IsAllDay == value) return;

            Appointment.IsAllDay = value;

            if(value)
            {
                SelectedStartTime = SelectedStartDate.Date;
                SelectedEndTime = SelectedEndDate.Date;
            }

            OnPropertyChanged(nameof(IsNotAllDay));
        }

        #endregion
    }
}
