using BetterWidgets.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Abstractions;
using BetterWidgets.Tests.Widgets;
using BetterWidgets.Services;
using BetterWidgets.Extensions.StickyNotes;
using Xunit.Abstractions;
using BetterWidgets.Model.Notes;

namespace BetterWidgets.Tests
{
    public class StickyNotesTests : IClassFixture<StickyNotesFixture>
    {
        private readonly ITestOutputHelper _output;
        private readonly IStickyNotes<StickyNotesWidget> _notes;

        public StickyNotesTests(StickyNotesFixture fixture, ITestOutputHelper testOutput)
        {
            _output = testOutput;
            _notes = fixture.Services.GetRequiredService<IStickyNotes<StickyNotesWidget>>();
        }

        #region Utils

        private async Task DeleteNoteAsync(string id)
        {
            var deletionResult = await _notes.DeleteStickyNoteAsync(id);

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);
        }

        #endregion

        [Fact]
        public async Task Should_Get_All_Sticky_Notes()
        {
            var result = await _notes.GetAllNotesAsync();

            if(result.ex != null) throw result.ex;

            Assert.NotNull(result.notes);

            _output.WriteLine(string.Join(',', result.notes.Select(n => $"{n.Serialize()} \n")));
            Assert.True(result.notes.Any());
        }

        [Fact]
        public async Task Should_Create_Sticky_Note()
        {
            var noteCreationRequest = new StickyNote()
            {
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString()
            };

            var noteCreationResult = await _notes.CreateStickyNoteAsync(noteCreationRequest);

            if(noteCreationResult.ex != null) throw noteCreationResult.ex;

            Assert.NotNull(noteCreationResult.created);

            _output.WriteLine(noteCreationResult.created.Serialize());

            Assert.Equal(noteCreationRequest.Title, noteCreationResult.created.Title);
            Assert.Equal(noteCreationRequest.Content, noteCreationResult.created.PreviewContent);

            await DeleteNoteAsync(noteCreationResult.created.Id);
        }

        [Fact]
        public async Task Should_Get_Sticky_Note_By_Id()
        {
            var noteCreationRequest = new StickyNote()
            {
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString()
            };

            var noteCreationResult = await _notes.CreateStickyNoteAsync(noteCreationRequest);

            if(noteCreationResult.ex != null) throw noteCreationResult.ex;

            Assert.NotNull(noteCreationResult.created);

            _output.WriteLine(noteCreationResult.created.Serialize());

            var getNoteResult = await _notes.GetStickyNoteById(noteCreationResult.created.Id);

            if(getNoteResult.ex != null) throw getNoteResult.ex;

            Assert.NotNull(getNoteResult.note);

            Assert.Equal(noteCreationResult.created.Id, getNoteResult.note.Id);
            Assert.Equal(noteCreationResult.created.Title, getNoteResult.note.Title);
            Assert.Equal(noteCreationResult.created.Content, getNoteResult.note.Content);
            Assert.Equal(noteCreationResult.created.PreviewContent, getNoteResult.note.PreviewContent);
            Assert.Equal(noteCreationResult.created.CreatedDate, getNoteResult.note.CreatedDate);
            Assert.Equal(noteCreationResult.created.LastEditedDateTime, getNoteResult.note.LastEditedDateTime);

            await DeleteNoteAsync(getNoteResult.note.Id);
        }

        [Fact]
        public async Task Should_Update_Sticky_Note()
        {
            var noteRequest = new StickyNote()
            {
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString()
            };

            var noteCreationResult = await _notes.CreateStickyNoteAsync(noteRequest);

            if(noteCreationResult.ex != null) throw noteCreationResult.ex;

            Assert.NotNull(noteCreationResult.created);

            _output.WriteLine(noteCreationResult.created.Serialize());

            Assert.Equal(noteRequest.Title, noteCreationResult.created.Title);
            Assert.Equal(noteRequest.Content, noteCreationResult.created.PreviewContent);

            noteRequest = noteCreationResult.created;

            noteRequest.Title = Guid.NewGuid().ToString();
            noteRequest.Content = Guid.NewGuid().ToString();

            var noteUpdateResult = await _notes.UpdateStickyNoteAsync(noteRequest);

            if(noteUpdateResult.ex != null) throw noteUpdateResult.ex;

            Assert.Equal(noteUpdateResult.updated.Title, noteRequest.Title);
            Assert.Equal(noteUpdateResult.updated.PreviewContent, noteRequest.Content);

            await DeleteNoteAsync(noteUpdateResult.updated.Id);
        }

        [Fact]
        public async Task Should_Pin_Sticky_Note_Async()
        {
            var noteRequest = new StickyNote()
            {
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString()
            };

            var noteCreationResult = await _notes.CreateStickyNoteAsync(noteRequest);

            if(noteCreationResult.ex != null) throw noteCreationResult.ex;

            Assert.NotNull(noteCreationResult.created);

            _output.WriteLine(noteCreationResult.created.Serialize());

            Assert.Equal(noteRequest.Title, noteCreationResult.created.Title);
            Assert.Equal(noteRequest.Content, noteCreationResult.created.PreviewContent);

            var notePinResult = await _notes.PinNoteAsync(noteCreationResult.created);

            if(notePinResult.ex != null) throw notePinResult.ex;

            Assert.True(notePinResult.success);
            Assert.True(notePinResult.success);
            Assert.True(noteCreationResult.created.ClientId != default, noteCreationResult.created.ClientId.ToString());

            await _notes.ClearAllPinnedNotesAsync();

            var noteDeletionResult = await _notes.DeleteStickyNoteAsync(noteCreationResult.created.Id);

            if(noteDeletionResult.ex != null) throw noteDeletionResult.ex;

            Assert.True(noteDeletionResult.success);
        }

        [Fact]
        public async Task Should_Unpin_Sticky_Note()
        {
            var noteRequest = new StickyNote()
            {
                Title = Guid.NewGuid().ToString(),
                Content = Guid.NewGuid().ToString()
            };

            var noteCreationResult = await _notes.CreateStickyNoteAsync(noteRequest);

            if(noteCreationResult.ex != null) throw noteCreationResult.ex;

            Assert.NotNull(noteCreationResult.created);

            _output.WriteLine(noteCreationResult.created.Serialize());

            Assert.Equal(noteRequest.Title, noteCreationResult.created.Title);
            Assert.Equal(noteRequest.Content, noteCreationResult.created.PreviewContent);

            var notePinResult = await _notes.PinNoteAsync(noteCreationResult.created);

            if(notePinResult.ex != null) throw notePinResult.ex;

            Assert.True(notePinResult.success);
            Assert.True(notePinResult.success);
            Assert.True(noteCreationResult.created.ClientId != default, noteCreationResult.created.ClientId.ToString());

            var unpinNoteResult = await _notes.UnpinNoteAsync(noteCreationResult.created.ClientId);

            if(unpinNoteResult.ex != null) throw unpinNoteResult.ex;

            Assert.True(unpinNoteResult.success);
            Assert.True(noteCreationResult.created.ClientId == default);

            await _notes.ClearAllPinnedNotesAsync();

            var noteDeletionResult = await _notes.DeleteStickyNoteAsync(noteCreationResult.created.Id);

            if(noteDeletionResult.ex != null) throw noteDeletionResult.ex;

            Assert.True(noteDeletionResult.success);
        }
    }
}
