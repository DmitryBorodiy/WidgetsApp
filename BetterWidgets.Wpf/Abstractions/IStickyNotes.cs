using BetterWidgets.Model.Notes;

namespace BetterWidgets.Abstractions
{
    public interface IStickyNotes<TWidget> : IPermissionable, ICachable where TWidget : IWidget
    {
        List<StickyNote> PinnedNotes { get; }

        event EventHandler<string> NoteDeleted;
        event EventHandler<StickyNote> NoteCreated;
        event EventHandler<StickyNote> NoteUpdated;

        Task<(IEnumerable<StickyNote> notes, Exception ex)> GetAllNotesAsync();

        Task<(StickyNote note, Exception ex)> GetStickyNoteById(string id);

        Task<(StickyNote created, Exception ex)> CreateStickyNoteAsync(StickyNote note);
        Task<(StickyNote updated, Exception ex)> UpdateStickyNoteAsync(StickyNote note);

        Task<(bool success, Exception ex)> DeleteStickyNoteAsync(string id);

        Task<(bool contains, Exception ex)> ContainsPinnedNoteIdAsync(Guid clientId);
        Task<(IEnumerable<StickyNote> notes, Exception ex)> GetPinnedNotesAsync();
        Task<(bool success, Exception ex)> PinNoteAsync(StickyNote note);
        Task<(bool success, Exception ex)> UnpinNoteAsync(Guid clientId);

        Task<(bool success, Exception ex)> ClearAllPinnedNotesAsync();
    }
}
