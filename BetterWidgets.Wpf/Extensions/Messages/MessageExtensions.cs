using BetterWidgets.Model.Notes;

namespace BetterWidgets.Extensions.Messages
{
    public static class MessageExtensions
    {
        public static StickyNote ToStickyNote(this Microsoft.Graph.Models.Message message) => new StickyNote()
        {
            Id = message.Id,
            Title = message.Subject,
            CreatedDate = message.CreatedDateTime?.DateTime.ToLocalTime(),
            LastEditedDateTime = message.LastModifiedDateTime?.DateTime.ToLocalTime(),
            Content = message.Body?.Content,
            PreviewContent = message.BodyPreview
        };
    }
}
