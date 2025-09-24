using BetterWidgets.Model.DTO;
using Microsoft.Graph.Models;
using BetterWidgets.Extensions.Appointments;
using TaskStatus = Microsoft.Graph.Models.TaskStatus;

namespace BetterWidgets.Extensions.Tasks
{
    public static class TodoTaskRequestExtensions
    {
        private static readonly string _utcFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

        private static DateTimeTimeZone GetMSDateTime(DateTime? dateTime, bool convertTime = true)
        {
            if(!dateTime.HasValue) return null;

            return new DateTimeTimeZone() 
            {
                TimeZone = "UTC",
                DateTime = (convertTime ? 
                            dateTime.Value.ToUniversalTime() : dateTime.Value)
                            .ToString(_utcFormat)
            };
        }

        public static TodoTask ToMSGraphTodoTask(this TodoTaskRequest request) 
        {
            if(request == null) throw new ArgumentNullException(nameof(request));
            if(string.IsNullOrEmpty(request.Title)) throw new FormatException(nameof(request.Title));
            if(string.IsNullOrEmpty(request.ListId)) throw new FormatException(nameof(request.ListId));

            return new TodoTask()
            {
                Id = request.Id ?? string.Empty,
                Title = request.Title,
                Status = request.IsCompleted ? TaskStatus.Completed : TaskStatus.NotStarted,
                IsReminderOn = request.IsReminderOn,
                Importance = request.IsImportant ? Importance.High : Importance.Normal,
                StartDateTime = GetMSDateTime(request.StartDate, false),
                DueDateTime = GetMSDateTime(request.DueDate, false),
                ReminderDateTime = GetMSDateTime(request.ReminderDate),
                Body = new ItemBody()
                {
                    Content = request.Body ?? string.Empty,
                    ContentType = BodyType.Text
                }
            };
        }
    }
}
