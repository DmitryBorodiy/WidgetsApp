namespace BetterWidgets.Model.DTO
{
    public class ChecklistRequest
    {
        public string ListId { get; set; }
        public string TaskId { get; set; }

        public string Id { get; set; }
        public string Title { get; set; }
        public bool? IsChecked { get; set; }
    }
}
