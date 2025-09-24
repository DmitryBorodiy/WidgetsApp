using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Exceptions;
using BetterWidgets.Extensions.Tasks;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Model.DTO;
using BetterWidgets.Model.Tasks;
using BetterWidgets.Properties;
using BetterWidgets.Widgets;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Permission = BetterWidgets.Model.Permission;
using TodoTask = BetterWidgets.Model.Tasks.TodoTask;

namespace BetterWidgets.Services
{
    public sealed partial class MSTodoManager<TWidget> : ITodoManager<TWidget> where TWidget : IWidget
    {
        #region Services
        private readonly ILogger _logger;
        private readonly DataService<TWidget> _data;
        private readonly IPermissionManager<TWidget> _permissions;
        private readonly IMSGraphService _graph;
        private readonly Settings _settings;
        #endregion

        public MSTodoManager(
            ILogger<MSTodoManager<TWidget>> logger,
            DataService<TWidget> data,
            IPermissionManager<TWidget> permissions,
            IMSGraphService graphService,
            Settings settings)
        {
            _logger = logger;
            _data = data;
            _permissions = permissions;
            _graph = graphService;
            _settings = settings;
        }

        private async Task EnsureGraphServiceIsReadyAsync()
        {
            if(!NetworkHelper.IsConnected) throw new NetworkUnavailableException();

            if(_graph == null)
               throw new InvalidOperationException(Errors.MSGraphServiceIsNotRegistered);

            if(!_graph.IsSignedIn)
               throw new InvalidOperationException(Errors.UserIsNotSignedIn);

            if(_graph.Client == null)
               throw new InvalidOperationException(Errors.MSGraphClientIsNotInitialized);

            if(await RequestAccessAsync(PermissionLevel.HighLevel) != PermissionState.Allowed)
               throw new UnauthorizedAccessException(Errors.WidgetHasNotAllowedPermission);
        }

        public async Task<(TodoTask task, Exception ex)> CreateAsync(TodoTaskRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(Errors.NullReference);
                if(string.IsNullOrEmpty(request.Title)) throw new FormatException(Errors.ValueNull + nameof(request.Title));
                if(string.IsNullOrEmpty(request.ListId)) throw new FormatException(Errors.ValueNull + nameof(request.ListId));

                await EnsureGraphServiceIsReadyAsync();

                var task = request.ToMSGraphTodoTask();
                var createdTask = await _graph.Client.Me.Todo.Lists[request.ListId].Tasks.PostAsync(task);
                var result = createdTask.ToTodoTask();

                return (result, null);
            }
            catch(Exception ex)
            {
                return (null, ex);
            }
        }

        public async Task<(TodoList todoList, Exception ex)> CreateListAsync(TodoList request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request), Errors.NullReference);
                if(string.IsNullOrEmpty(request.Title)) throw new FormatException(Errors.ValueNull);

                await EnsureGraphServiceIsReadyAsync();

                var list = request.ToGraphTodoListCreationRequest();
                var createdList = await _graph.Client.Me.Todo.Lists.PostAsync(list);

                return (createdList.ToTodoList(), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(bool success, Exception ex)> DeleteListAsync(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);

                await EnsureGraphServiceIsReadyAsync();

                await _graph.Client.Me.Todo.Lists[id].DeleteAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(bool success, Exception ex)> DeleteTaskAsync(TaskDeletionRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request));
                if(string.IsNullOrEmpty(request.Id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(request.ListId)) throw new ArgumentNullException(Errors.IdNullOrEmpty + nameof(request.ListId));

                await EnsureGraphServiceIsReadyAsync();

                await _graph.Client.Me.Todo.Lists[request.ListId].Tasks[request.Id].DeleteAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public async Task<(List<TodoList> todoLists, Exception ex)> GetAllListsAsync()
        {
            try
            {
                await EnsureGraphServiceIsReadyAsync();

                var lists = await _graph.Client.Me.Todo.Lists.Delta.GetAsDeltaGetResponseAsync();

                if(lists == null) return (new List<TodoList>(), null);

                var todoLists = lists.Value?.Select(l => l.ToTodoList())
                                            .ToList();

                return (todoLists, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (new List<TodoList>(), ex);
            }
        }

        public async Task<(T data, Exception ex)> GetCachedAsync<T>(string key, Func<Task<(T data, Exception ex)>> fetchFunc, bool forceRefresh = false, bool fetchData = true)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));

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

        public async Task<(TodoList todoList, Exception ex)> GetListByIdAsync(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);

                await EnsureGraphServiceIsReadyAsync();

                var list = await _graph.Client.Me.Todo.Lists[id].GetAsync();

                if(list == null) throw new NullReferenceException(nameof(list));

                var result = list.ToTodoList();

                return (result, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoTask task, Exception ex)> GetTaskAsync(TodoTaskRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(Errors.NullReference);
                if(string.IsNullOrEmpty(request.Id)) throw new FormatException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(request.ListId)) throw new FormatException(Errors.IdNullOrEmpty + nameof(request.ListId));

                await EnsureGraphServiceIsReadyAsync();

                var task = await _graph.Client.Me.Todo.Lists[request.ListId].Tasks[request.Id].GetAsync();
                
                if(task == null) throw new NullReferenceException(nameof(task));

                var result = task.ToTodoTask();

                return (result, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(List<TodoTask> tasks, Exception ex)> GetTasksByListIdAsync(string listId)
        {
            try
            {
                if(string.IsNullOrEmpty(listId)) throw new ArgumentNullException(Errors.IdNullOrEmpty);

                await EnsureGraphServiceIsReadyAsync();

                var tasks = await _graph.Client.Me.Todo.Lists[listId].Tasks.Delta.GetAsDeltaGetResponseAsync();
                var todoTasks = tasks?.Value?.Select(t => t.ToTodoTask()) ??
                                Enumerable.Empty<TodoTask>();

                return (todoTasks.ToList(), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (new List<TodoTask>(), ex);
            }
        }

        public async Task<PermissionState> RequestAccessAsync(PermissionLevel level = PermissionLevel.HighLevel)
        {
            var permission = new Permission(Scopes.Tasks, level);
            var state = _permissions.TryCheckPermissionState(permission);

            if(state == PermissionState.Undefined)
               state = await _permissions.RequestAccessAsync(permission);

            return state;
        }

        public async Task ResetCacheAsync(string key)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));

            await _data.DeleteFileAsync(key);
        }

        public async Task SetCacheAsync<T>(string key, T data)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(Errors.FileNameIsNullOrEmpty));
            if(data == null) throw new NullReferenceException(Errors.NullReference);

            await _data.SetToFileAsync(key, data);
        }

        public async Task<(TodoTask task, Exception ex)> UpdateAsync(TodoTaskRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(Errors.NullReference);
                if(string.IsNullOrEmpty(request.ListId)) throw new FormatException(nameof(request.ListId));
                if(string.IsNullOrEmpty(request.Title)) throw new FormatException(nameof(request.Title));

                await EnsureGraphServiceIsReadyAsync();

                var task = request.ToMSGraphTodoTask();
                var updatedTask = await _graph.Client.Me.Todo.Lists[request.ListId].Tasks[request.Id].PatchAsync(task);
                var result = updatedTask.ToTodoTask();

                return (result, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoList todoList, Exception ex)> UpdateListAsync(TodoList request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request), Errors.NullReference);
                if(string.IsNullOrEmpty(request.Id)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(request.Title)) throw new ArgumentNullException(nameof(request.Title), Errors.ValueNull);

                await EnsureGraphServiceIsReadyAsync();

                var list = request.ToGraphTodoListCreationRequest();
                var updatedList = await _graph.Client.Me.Todo.Lists[request.Id].PatchAsync(list);

                if(updatedList == null) throw new NullReferenceException(nameof(updatedList));

                return (updatedList.ToTodoList(), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoTask todoTask, Exception ex)> MoveTaskAsync(TodoTaskRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request));

                var deletionResult = await DeleteTaskAsync(new TaskDeletionRequest()
                {
                    Id = request.Id,
                    ListId = request.ListId
                });

                if(deletionResult.ex != null) throw deletionResult.ex;

                var taskCreationResult = await CreateAsync(request);

                return taskCreationResult;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoList list, Exception ex)> GetDefaultTodoListAsync()
        {
            try
            {
                await EnsureGraphServiceIsReadyAsync();

                var lists = await _graph.Client?.Me.Todo.Lists.Delta.GetAsDeltaGetResponseAsync();
                
                if(lists.Value == null) return (null, null);

                var defaultList = lists.Value.FirstOrDefault(l => l.WellknownListName == WellknownListName.DefaultList);

                return (defaultList.ToTodoList(), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoTask todoTask, Exception ex)> CreateChecklistItemAsync(ChecklistRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request));
                if(string.IsNullOrEmpty(request.ListId)) throw new ArgumentNullException(nameof(request.ListId));
                if(string.IsNullOrEmpty(request.TaskId)) throw new ArgumentNullException(nameof(request.TaskId));
                if(string.IsNullOrEmpty(request.Title)) throw new FormatException(nameof(request.Title));

                var checklistItem = request.ToMSCheckListItem();
                var createdChecklistItem = await _graph.Client?.Me.Todo
                                          .Lists[request.ListId]
                                          .Tasks[request.TaskId.ToString()]
                                          .ChecklistItems.PostAsync(checklistItem);

                return (createdChecklistItem.ToTodoTask(request.TaskId), null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(TodoTask checkListTask, Exception ex)> UpdateChecklistItemAsync(ChecklistRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request));
                
                if(string.IsNullOrEmpty(request.Id)) throw new ArgumentException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(request.ListId)) throw new ArgumentNullException(nameof(request.ListId));
                if(string.IsNullOrEmpty(request.TaskId)) throw new ArgumentNullException(nameof(request.TaskId));
                if(string.IsNullOrEmpty(request.Title)) throw new FormatException(nameof(request.Title));

                await EnsureGraphServiceIsReadyAsync();

                var checkListItem = request.ToMSCheckListItem();
                var updateResult = await _graph.Client?.Me.Todo
                                         .Lists[request.ListId]
                                         .Tasks[request.TaskId]
                                         .ChecklistItems[request.Id?.ToString()]
                                         .PatchAsync(checkListItem);
                var updatedListItem = updateResult.ToTodoTask(request.TaskId);

                return (updatedListItem, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public async Task<(bool success, Exception ex)> DeleteChecklistItemAsync(ChecklistRequest request)
        {
            try
            {
                if(request == null) throw new ArgumentNullException(nameof(request));

                if(string.IsNullOrEmpty(request.Id)) throw new ArgumentException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(request.ListId)) throw new ArgumentNullException(nameof(request.ListId));
                if(string.IsNullOrEmpty(request.TaskId)) throw new ArgumentNullException(nameof(request.TaskId));

                await EnsureGraphServiceIsReadyAsync();

                await _graph.Client?.Me.Todo
                      .Lists[request.ListId]
                      .Tasks[request.TaskId]
                      .ChecklistItems[request.Id]
                      .DeleteAsync();

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }

        public (string glyph, Exception ex) GetIconForList(string listId)
        {
            try
            {
                if(string.IsNullOrEmpty(listId)) throw new ArgumentNullException(Errors.IdNullOrEmpty);

                var glyph = _settings.GetValue<string>($"{listId}:icon", null);

                return (glyph, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (null, ex);
            }
        }

        public (bool success, Exception ex) SetIconForList(string listId, string glyph)
        {
            try
            {
                if(string.IsNullOrEmpty(listId)) throw new ArgumentNullException(Errors.IdNullOrEmpty);
                if(string.IsNullOrEmpty(glyph)) throw new ArgumentNullException(nameof(glyph));

                _settings.SetValue($"{listId}:icon", glyph);

                return (true, null);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return (false, ex);
            }
        }
    }
}
