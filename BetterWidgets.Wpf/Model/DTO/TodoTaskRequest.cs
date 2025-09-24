namespace BetterWidgets.Model.DTO
{
    public class TodoTaskRequest
    {
        public string Id { get; set; }
        public string ListId { get; set; }
        public string Title { get; set; }
        public bool IsReminderOn { get; set; }
        public bool IsImportant { get; set; }
        public bool IsCompleted { get; set; }
        public string Body { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
