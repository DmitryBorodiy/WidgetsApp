using BetterWidgets.Abstractions;
using BetterWidgets.Enums;
using BetterWidgets.Model.DTO;
using BetterWidgets.ViewModel.Components;
using Microsoft.Graph.Models;

namespace BetterWidgets.Extensions.Appointments
{
    public static class MSEventExtensions
    {
        public static CalendarImportance GetMSImportance(this Event msEvent)
        {
            switch(msEvent.Importance)
            {
                case Importance.Normal: return CalendarImportance.Normal;
                case Importance.Low: return CalendarImportance.Low;
                case Importance.High: return CalendarImportance.High;
                default: return CalendarImportance.Unknown;
            }
        }

        public static AppointmentStatus GetMSStatus(this Event msEvent)
        {
            switch(msEvent.ShowAs)
            {
                default: return AppointmentStatus.Unknown;
                case FreeBusyStatus.Busy: return AppointmentStatus.Busy;
                case FreeBusyStatus.Oof: return AppointmentStatus.Oof;
                case FreeBusyStatus.WorkingElsewhere: return AppointmentStatus.WorkingElsewhere;
                case FreeBusyStatus.Tentative: return AppointmentStatus.Tentative;
                case FreeBusyStatus.Free: return AppointmentStatus.Free;
            }
        }

        public static DateTime? GetMSDateTime(this DateTimeTimeZone msEventDateTimeZone, bool? isAllDay)
        {
            var utcDateTime = DateTime.Parse(msEventDateTimeZone.DateTime);

            if(isAllDay ?? default) return utcDateTime;

            var timezone = TimeZoneInfo.Local;
            var dateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone);

            return dateTime;
        }

        public static IAppointmentRequest ToMSAppointmentRequest(this AppointmentViewModel appointment) => new MSAppointmentRequest()
        {
            Id = appointment.Id,
            Title = appointment.TitleLabel,
            Start = appointment.StartDate,
            End = appointment.EndDate,
            Body = appointment.Body,
            Location = appointment.Location,
            WebLink = appointment.WebLink,
            IsAllDay = appointment.IsAllDay
        };
    }
}
