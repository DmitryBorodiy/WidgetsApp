using BetterWidgets.Model.Tasks;
using BetterWidgets.Extensions.Appointments;
using BetterWidgets.Model.DTO;

namespace BetterWidgets.Extensions.Tasks
{
    public static class TodoTaskExtensions
    {
        public static TodoTask ToTodoTask(this Microsoft.Graph.Models.ChecklistItem todoTaskList, string mainTaskId = null) => new TodoTask
        {
            Id = mainTaskId,
            SubTaskId = todoTaskList.Id,
            IsSubTask = true,
            Title = todoTaskList.DisplayName ?? string.Empty,
            IsCompleted = todoTaskList.IsChecked ?? false,
            CreatedDateTime = todoTaskList.CreatedDateTime?.ToLocalTime().DateTime ?? DateTime.MinValue,
            LastModifiedDateTime = todoTaskList.CheckedDateTime?.ToLocalTime().DateTime ?? DateTime.MinValue
        };

        public static TodoTask ToTodoTask(this Microsoft.Graph.Models.TodoTask todoTask) => new TodoTask
        {
            Id = todoTask.Id ?? string.Empty,
            IsImportant = todoTask.Importance == Microsoft.Graph.Models.Importance.High,
            IsCompleted = todoTask.Status == Microsoft.Graph.Models.TaskStatus.Completed,
            IsReminderOn = todoTask.IsReminderOn ?? false,
            HasAttachments = todoTask.HasAttachments ?? false,
            Title = todoTask.Title ?? string.Empty,
            Body = todoTask.Body?.Content ?? string.Empty,
            ReminderDateTime = todoTask.ReminderDateTime?.GetMSDateTime(false) ?? default,
            CreatedDateTime = todoTask.CreatedDateTime?.ToLocalTime().DateTime ?? default,
            LastModifiedDateTime = todoTask.LastModifiedDateTime?.DateTime ?? default,
            StartDateTime = todoTask.StartDateTime?.GetMSDateTime(false) ?? default,
            DueDateTime = todoTask.DueDateTime?.GetMSDateTime(false) ?? default,
            CheckListItems = todoTask.ChecklistItems?.Select(i => i.ToTodoTask(todoTask.Id)).ToList() ?? new List<TodoTask>()
        };

        public static TodoTaskRequest TodoTaskRequest(this TodoTask todoTask, string listId) => new TodoTaskRequest
        {
            Id = todoTask.Id,
            Title = todoTask.Title,
            Body = todoTask.Body,
            IsImportant = todoTask.IsImportant,
            IsCompleted = todoTask.IsCompleted,
            DueDate = todoTask.DueDateTime == DateTime.MinValue ?
                      default : todoTask.DueDateTime,
            StartDate = todoTask.StartDateTime == DateTime.MinValue ?
                        default : todoTask.StartDateTime,
            ReminderDate = todoTask.ReminderDateTime == DateTime.MinValue ? 
                           default : todoTask.ReminderDateTime,
            ListId = listId
        };

        public static Microsoft.Graph.Models.ChecklistItem ToMSCheckListItem(this ChecklistRequest request)
        {
            var checkList = new Microsoft.Graph.Models.ChecklistItem()
            {
                DisplayName = request.Title,
                IsChecked = request.IsChecked ?? false
            };

            return checkList;
        }
    }
}
