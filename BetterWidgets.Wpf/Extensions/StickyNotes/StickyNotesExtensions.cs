using Newtonsoft.Json;
using BetterWidgets.Model.Notes;
using Microsoft.Graph.Models;
using System.Net;

namespace BetterWidgets.Extensions.StickyNotes
{
    public static class StickyNotesExtensions
    {
        public static string Serialize(this StickyNote note)
            => JsonConvert.SerializeObject(note);

        private static ItemBody CreateBody(string content) => new ItemBody()
        {
            ContentType = BodyType.Html,
            Content = content
        };

        public static Message AsRequest(this StickyNote note) => new Message()
        {
            Subject = note.Title,
            Body = CreateBody(note.Content)
        };
    }
}
