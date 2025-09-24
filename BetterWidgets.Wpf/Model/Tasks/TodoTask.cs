namespace BetterWidgets.Model.Tasks
{
    public class TodoTask
    {
        public string Id { get; set; } = string.Empty;
        public string SubTaskId { get; set; }
        public bool IsSubTask { get; set; }
        public bool IsImportant { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsReminderOn { get; set; }
        public bool HasAttachments { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime? ReminderDateTime { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? LastModifiedDateTime { get; set; }
        public DateTime? StartDateTime { get; set; } 
        public DateTime? DueDateTime { get; set; }
        public List<TodoTask> CheckListItems { get; set; }
    }
}
