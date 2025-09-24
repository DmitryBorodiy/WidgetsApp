using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Model.Notes;
using BetterWidgets.Properties;
using Microsoft.Extensions.Logging;
using BetterWidgets.Extensions.Messages;
using BetterWidgets.Extensions.StickyNotes;
using BetterWidgets.Enums;
using BetterWidgets.Model;

namespace BetterWidgets.Services
{
    public sealed class StickyNotes<TWidget> : IStickyNotes<TWidget> where TWidget : IWidget 
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<StickyNotes<TWidget>> _settings;
        private readonly DataService<TWidget> _data;
        private readonly IMSGraphService _graph;
        private readonly IPermissionManager<TWidget> _permissions;
        #endregion

        public StickyNotes(
            ILogger<StickyNotes<TWidget>> logger,
            Settings<StickyNotes<TWidget>> settings,
            IMSGraphService graph,
            DataService<TWidget> data,
            IPermissionManager<TWidget> permissions)
        {
            _logger = logger;
            _settings = settings;
            _graph = graph;
            _data = data;
            _permissions = permissions;
        }

        #region Props

        public List<StickyNote> PinnedNotes { get; private set; }

        #endregion

        #region Events

        public event EventHandler<string> NoteDeleted;
        public event EventHandler<StickyNote> NoteCreated;
        public event EventHandler<StickyNote> NoteUpdated;

        #endregion

        #region Utils

        private (IEnumerable<StickyNote> notes, Exception ex) DefaultCollection(Exception ex = null) => (Enumerable.Empty<StickyNote>(), ex);
        private (StickyNote note, Exception ex) Default(Exception ex = null) => (null, ex);

        private async Task SavePinnedNotesAsync()
        {
            if(PinnedNotes == null) throw new NullReferenceException(Errors.CollectionWasNull);

           await _data.SetToFileAsync(FileNames.pinnedNotes, PinnedNotes);
        }

        private async Task LoadPinnedNotesAsync()
        {
            var notes = await GetPinnedNotesAsync();
            
            if(notes.ex != null) throw notes.ex;

            PinnedNotes = notes.notes.ToList();
        }

        #endregion

        public async Task<(StickyNote created, Exception ex)> CreateStickyNoteAsync(StickyNote note)
        {
            try
            {
                if(!_graph.IsSignedIn) return Default(new InvalidOperationException(Errors.UserIsNotSignedIn));
                if(_graph.Client == null) return Default(new InvalidOperationException(Errors.MSGraphClientIsNotInitialized));
                if(string.IsNullOrEmpty(note.Content)) throw new FormatException(nameof(note.Content));

                if(await RequestAccessAsync() != PermissionState.Allowed) return Default(new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + typeof(TWidget).Name));

                var request = note.AsRequest();
                var createdResult = await _graph.Client.Me.MailFolders["Notes"].Messages.PostAsync(request);
                var createdNote = createdResult.ToStickyNote();

                createdNote.Color = note.Color;

                NoteCreated?.Invoke(this, createdNote);

                return (createdNote, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(bool success, Exception ex)> DeleteStickyNoteAsync(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(!_graph.IsSignedIn) return (false, new InvalidOperationException(Errors.UserIsNotSignedIn));
                if(_graph.Client == null) return (false, new InvalidOperationException(Errors.MSGraphClientIsNotInitialized));

                if(await RequestAccessAsync() != PermissionState.Allowed) return (false, new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission));

                await _graph.Client.Me.MailFolders["Notes"].Messages[id].DeleteAsync();

                NoteDeleted?.Invoke(this, id);

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(IEnumerable<StickyNote> notes, Exception ex)> GetAllNotesAsync()
        {
            try
            {
                if(!_graph.IsSignedIn) return DefaultCollection(new InvalidOperationException(Errors.UserIsNotSignedIn));
                if(_graph.Client == null) return DefaultCollection(new InvalidOperationException(Errors.MSGraphClientIsNotInitialized));

                if(await RequestAccessAsync() != PermissionState.Allowed) return DefaultCollection(new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + typeof(TWidget).Name));

                var notes = await _graph.Client.Me.MailFolders["Notes"].Messages.GetAsync();
                var stickyNotes = notes.Value.Select(n => n.ToStickyNote()).ToList();

                if(PinnedNotes == null) await LoadPinnedNotesAsync();

                foreach(var note in stickyNotes)
                {
                    var pinnedNote = PinnedNotes.FirstOrDefault(n => n.Id == note.Id);

                    if(pinnedNote != null)
                    {
                        note.IsPinned = true;
                        note.ClientId = pinnedNote.ClientId;
                    }
                }

                return (stickyNotes, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return DefaultCollection(ex);
            }
        }

        public async Task<(StickyNote note, Exception ex)> GetStickyNoteById(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(!_graph.IsSignedIn) return Default(new InvalidOperationException(Errors.UserIsNotSignedIn));
                if(_graph.Client == null) return Default(new InvalidOperationException(Errors.MSGraphClientIsNotInitialized));
            
                if(await RequestAccessAsync() != PermissionState.Allowed) return Default(new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + typeof(TWidget).Name));

                var note = await _graph.Client.Me.MailFolders["Notes"].Messages[id].GetAsync();
                var stickyNote = note.ToStickyNote();

                return (stickyNote, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(StickyNote updated, Exception ex)> UpdateStickyNoteAsync(StickyNote note)
        {
            try
            {
                if(note == null) throw new ArgumentNullException(nameof(note));
                if(string.IsNullOrEmpty(note.Id)) throw new ArgumentException(Errors.IdNullOrEmpty);
                if(!_graph.IsSignedIn) return Default(new InvalidOperationException(Errors.UserIsNotSignedIn));
                if(_graph.Client == null) return Default(new InvalidOperationException(Errors.MSGraphClientIsNotInitialized));

                if(await RequestAccessAsync() != PermissionState.Allowed) return Default(new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + typeof(TWidget).Name));

                await DeleteStickyNoteAsync(note.Id);
                note.ClearColorCache();

                var request = note.AsRequest();
                var updatedResult = await _graph.Client.Me.MailFolders["Notes"].Messages.PostAsync(request);
                var updatedStickyNote = updatedResult.ToStickyNote();

                updatedStickyNote.IsPinned = note.IsPinned;
                updatedStickyNote.Color = note.Color;

                if(note.ClientId != default)
                {
                    updatedStickyNote.ClientId = note.ClientId;
                    
                    if(PinnedNotes == null) await LoadPinnedNotesAsync();

                    var pinnedNote = PinnedNotes.FirstOrDefault(n => n.Id == note.Id);

                    if(pinnedNote != null)
                    {
                        pinnedNote.Id = updatedStickyNote.Id;
                        pinnedNote.Title = updatedStickyNote.Title;
                        pinnedNote.Content = updatedStickyNote.Content;
                        pinnedNote.PreviewContent = updatedStickyNote.PreviewContent;
                        pinnedNote.Color = note.Color;
                        pinnedNote.ClientId = note.ClientId;
                        pinnedNote.LastEditedDateTime = updatedStickyNote.LastEditedDateTime;
                        pinnedNote.CreatedDate = updatedStickyNote.CreatedDate;
                        pinnedNote.DueDateTime = updatedStickyNote.DueDateTime;
                    }

                    await SavePinnedNotesAsync();
                }
                    
                NoteUpdated?.Invoke(this, updatedStickyNote);

                return (updatedStickyNote, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(T data, Exception ex)> GetCachedAsync<T>(string key, Func<Task<(T data, Exception ex)>> fetchFunc, bool forceRefresh = false, bool fetchData = true)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));
            if(await RequestAccessAsync() != PermissionState.Allowed) return (default, new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission + typeof(TWidget).Name));

            if(forceRefresh) await ResetCacheAsync(key);

            var cache = await _data.GetFromFileAsync<T>(key);

            if(cache.ex != null) return (default, cache.ex);
            if(cache.data != null && !forceRefresh) return (cache.data, null);
            if(!fetchData) return (default, null);

            var data = await fetchFunc();

            if(data.ex != null) return (default, data.ex);

            await SetCacheAsync(key, data.data);

            return (data.data, null);
        }

        public async Task SetCacheAsync<T>(string key, T data)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));
            if(data == null) throw new NullReferenceException(Errors.NullReference);
            if(await RequestAccessAsync() != PermissionState.Allowed) return;

            await _data.SetToFileAsync(key, data);
        }

        public async Task ResetCacheAsync(string key)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));
            if(await RequestAccessAsync() != PermissionState.Allowed) return;

            await _data.DeleteFileAsync(key);
        }

        public Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            try
            {
                var accessState = _permissions.TryCheckPermissionState(new Permission(Scopes.Notes, level));
                
                return Task.FromResult(accessState);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return Task.FromResult(PermissionState.Undefined);
            }
        }

        public async Task<(IEnumerable<StickyNote> notes, Exception ex)> GetPinnedNotesAsync()
        {
            try
            {
                var notesData = await _data.GetFromFileAsync<List<StickyNote>>(FileNames.pinnedNotes);

                if(notesData.ex != null) throw notesData.ex;

                return (notesData.data != null ? notesData.data : DefaultCollection().notes, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return DefaultCollection(ex);
            }
        }

        public async Task<(bool success, Exception ex)> PinNoteAsync(StickyNote note)
        {
            try
            {
                if(note == null) throw new ArgumentNullException(nameof(note));
                if(PinnedNotes == null) await LoadPinnedNotesAsync();

                var contains = await ContainsPinnedNoteIdAsync(note.ClientId);

                if(contains.ex != null) return (false, contains.ex);
                if(contains.contains) return (true, null);

                note.IsPinned = true;
                note.ClientId = Guid.NewGuid();

                PinnedNotes.Add(note);

                await SavePinnedNotesAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(bool success, Exception ex)> UnpinNoteAsync(Guid clientId)
        {
            try
            {
                if(clientId == default) throw new ArgumentException(string.Format(Errors.EntityGuidCannotBeDefault, nameof(clientId)));
                if(PinnedNotes == null) await LoadPinnedNotesAsync();

                var contains = await ContainsPinnedNoteIdAsync(clientId);

                if(contains.ex != null) return (false, contains.ex);
                if(!contains.contains) return (false, null);

                var note = PinnedNotes.First(n => n.ClientId == clientId);
                PinnedNotes.Remove(note);

                note.IsPinned = false;
                note.ClientId = default;

                await SavePinnedNotesAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(bool success, Exception ex)> ClearAllPinnedNotesAsync()
        {
            try
            {
                PinnedNotes = new List<StickyNote>();

                await SavePinnedNotesAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(bool contains, Exception ex)> ContainsPinnedNoteIdAsync(Guid clientId)
        {
            try
            {
                if(clientId == default) return (false, null);
                if(PinnedNotes == null) await LoadPinnedNotesAsync();

                return (PinnedNotes.Any(n => n.ClientId == clientId), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }
    }
}
