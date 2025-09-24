using BetterWidgets.Services;
using BetterWidgets.Tests.Consts;
using BetterWidgets.Tests.Extensions;
using BetterWidgets.Tests.Fixtures;
using BetterWidgets.Tests.Helper;
using BetterWidgets.Tests.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Models.ODataErrors;

namespace BetterWidgets.Tests
{
    public class MSCalendarServiceTests : IClassFixture<GraphServiceFixture>
    {
        #region Services
        private readonly ICalendarService<CalendarWidget> _calendar;
        private readonly IMSGraphService _graph;
        #endregion

        public MSCalendarServiceTests(GraphServiceFixture fixture)
        {
            _graph = fixture.Services.GetService<IMSGraphService>();
            _calendar = fixture.Services.GetService<ICalendarService<CalendarWidget>>();
        }

        [Fact]
        public async Task Should_Get_Appointments()
        {
            if (!_graph.IsSignedIn) await _graph.SignInAsync();

            var result = await _calendar.GetAllAppointmentsAsync();

            if (result.ex != null) throw result.ex;

            Assert.NotNull(result.appointments);
        }

        [Fact]
        public async Task Should_Get_Calendars()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var result = await _calendar.GetAllCalendarsAsync();

            if(result.ex != null) throw result.ex;

            Assert.NotNull(result.calendars);
        }

        [Fact]
        public async Task Should_Create_Appointment()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var appointment = TestAppointment.CreateMSTestAppointment();
            var result = await _calendar.CreateAppointmentAsync(appointment);

            if(result.ex != null) throw result.ex;

            await _calendar.DeleteAppointmentAsync(result.appointment.Id);

            Assert.NotNull(result.appointment);
            Assert.Equal(appointment.Title, result.appointment.Subject);
            Assert.Equal(appointment.Start.ToUtcDateTimeString(), result.appointment.Start.Value.ToUtcDateTimeString());
            Assert.Equal(appointment.End.ToUtcDateTimeString(), result.appointment.End.Value.ToUtcDateTimeString());
        }

        [Fact]
        public async Task Should_Delete_Appointment()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var appointment = TestAppointment.CreateMSTestAppointment();
            var result = await _calendar.CreateAppointmentAsync(appointment);

            if(result.ex != null) throw result.ex;

            Assert.NotNull(result.appointment);

            await _calendar.DeleteAppointmentAsync(result.appointment.Id);

            var recievedResult = await _calendar.GetAppointmentByIdAsync(result.appointment.Id);

            Assert.Null(recievedResult.appointment);
            Assert.True(recievedResult.ex is Exception);
        }

        [Fact]
        public async Task Should_Get_Appointment_By_Id()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var newAppointment = TestAppointment.CreateMSTestAppointment();
            var result = await _calendar.CreateAppointmentAsync(newAppointment);

            if(result.ex != null) throw result.ex;

            Assert.NotNull(result.appointment);
            Assert.Equal(newAppointment.Title, result.appointment.Subject);
            Assert.Equal(newAppointment.Start.ToUtcDateTimeString(), result.appointment.Start.Value.ToUtcDateTimeString());
            Assert.Equal(newAppointment.End.ToUtcDateTimeString(), result.appointment.End.Value.ToUtcDateTimeString());

            var receivedAppointment = await _calendar.GetAppointmentByIdAsync(result.appointment.Id);

            if(receivedAppointment.ex != null) throw receivedAppointment.ex;

            await _calendar.DeleteAppointmentAsync(receivedAppointment.appointment.Id);

            Assert.NotNull(receivedAppointment.appointment);
            Assert.Equal(receivedAppointment.appointment.Id, result.appointment.Id);
            Assert.Equal(newAppointment.Title, receivedAppointment.appointment.Subject);
            Assert.Equal(newAppointment.Start.ToUtcDateTimeString(), receivedAppointment.appointment.Start.Value.ToUtcDateTimeString());
            Assert.Equal(newAppointment.End.ToUtcDateTimeString(), receivedAppointment.appointment.End.Value.ToUtcDateTimeString());
        }

        [Fact]
        public async Task Should_Get_Appointments_By_Date()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var appointmentCreationRequest = TestAppointment.CreateMSTestAppointment(DateTime.Now, DateTime.Now.AddDays(2));
            var startDate = appointmentCreationRequest.Start.Date;
            var endDate = appointmentCreationRequest.End.Date;

            var createdAppointment = await _calendar.CreateAppointmentAsync(appointmentCreationRequest);

            if(createdAppointment.ex != null) throw createdAppointment.ex;

            Assert.NotNull(createdAppointment.appointment);
            Assert.Equal(createdAppointment.appointment.Subject, appointmentCreationRequest.Title);
            Assert.Equal(createdAppointment.appointment.Start.Value.Date, startDate);
            Assert.Equal(createdAppointment.appointment.End.Value.Date, endDate);

            var appointments = await _calendar.GetAppointmentsByDateAsync(startDate, endDate);

            if(appointments.ex != null) throw appointments.ex;

            Assert.NotNull(appointments.appointments);
            Assert.Contains(appointments.appointments, a => a.Id == createdAppointment.appointment.Id);

            await _calendar.DeleteAppointmentAsync(createdAppointment.appointment.Id);
        }

        [Fact]
        public async Task Should_Get_Cashed_Appointments()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var newAppointments = await _calendar.GetCachedAsync
            (
                FileNames.appointmentsCache,
                () => _calendar.GetAppointmentsByDateAsync(startDate, endDate),
                true
            );

            if(newAppointments.ex != null) throw newAppointments.ex;

            var cachedAppointments = await _calendar.GetCachedAsync
            (
                FileNames.appointmentsCache,
                () => _calendar.GetAppointmentsByDateAsync(startDate, endDate),
                false
            );

            if(cachedAppointments.ex != null) throw cachedAppointments.ex;

            await _calendar.ResetCacheAsync(FileNames.appointmentsCache);

            Assert.True(newAppointments.data.Count() == cachedAppointments.data.Count());
            Assert.True(newAppointments.data.Zip(cachedAppointments.data, (a, b) => a.Id == b.Id).All(x => x));
            Assert.True(newAppointments.data.Zip(cachedAppointments.data, (a, b) => a.Subject == b.Subject).All(x => x));
        }

        [Fact]
        public async Task Should_Get_Updated_Cached_Data()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var apointments = await _calendar.GetCachedAsync
            (
                FileNames.appointmentsCache,
                _calendar.GetAllAppointmentsAsync
            );

            if(apointments.ex != null) throw apointments.ex;

            var appointment = TestAppointment.CreateMSTestAppointment();
            var result = await _calendar.CreateAppointmentAsync(appointment);

            if(result.ex != null) throw result.ex;

            Assert.DoesNotContain(apointments.data, a => a.Id == result.appointment.Id);

            var newApointments = await _calendar.GetCachedAsync
            (
                FileNames.appointmentsCache,
                _calendar.GetAllAppointmentsAsync,
                true
            );

            Assert.Contains(newApointments.data, a => a.Id == result.appointment.Id);

            await _calendar.DeleteAppointmentAsync(result.appointment.Id);
        }

        [Fact]
        public async Task Should_Get_Appointments_By_Query()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var start = DateTime.Now;
            var end = DateTime.Now.AddDays(3);

            var testAppointment = TestAppointment.CreateMSTestAppointment(start, end);
            var createdAppointment = await _calendar.CreateAppointmentAsync(testAppointment);

            if(createdAppointment.ex != null) throw createdAppointment.ex;

            var appointments = await _calendar.GetAppointmentsByQuery(new()
            {
                Start = start,
                End = end
            });

            if(appointments.ex != null) throw appointments.ex;

            await _calendar.DeleteAppointmentAsync(createdAppointment.appointment.Id);

            Assert.NotNull(appointments.appointments);
            Assert.NotNull(createdAppointment.appointment);
            Assert.Contains(appointments.appointments, a => a.Id == createdAppointment.appointment.Id);
            Assert.Contains(appointments.appointments, a => a.Start?.Date == start.Date);
            Assert.Contains(appointments.appointments, a => a.End?.Date == end.Date);
        }

        [Fact]
        public async Task Should_Update_Appointment()
        {
            if(!_graph.IsSignedIn) await _graph.SignInAsync();

            var creationDateStart = DateTime.Now;
            var creationDateEnd = DateTime.Now.AddDays(3);

            var request = TestAppointment.CreateMSTestAppointment(creationDateStart, creationDateEnd);
            var creationResult = await _calendar.CreateAppointmentAsync(request);

            if(creationResult.ex != null) throw creationResult.ex;

            Assert.NotNull(creationResult.appointment);
            Assert.Equal(creationResult.appointment.Subject, request.Title);
            Assert.Equal(creationResult.appointment.Start?.Date, creationDateStart.Date);
            Assert.Equal(creationResult.appointment.End?.Date, creationDateEnd.Date);

            request.Id = creationResult.appointment.Id;
            request.Title = Guid.NewGuid().ToString();
            request.End = request.End.AddDays(2);

            var updateResult = await _calendar.UpdateAppointmentAsync(request);

            if(updateResult.ex != null) throw updateResult.ex;

            Assert.NotNull(updateResult.appointment);
            Assert.Equal(updateResult.appointment.Subject, request.Title);
            Assert.Equal(updateResult.appointment.End?.Date, request.End.Date);

            await _calendar.DeleteAppointmentAsync(updateResult.appointment.Id);
        }
    }
}
