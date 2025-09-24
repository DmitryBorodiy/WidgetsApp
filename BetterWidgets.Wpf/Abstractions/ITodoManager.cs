using BetterWidgets.Model.DTO;
using BetterWidgets.Model.Tasks;

namespace BetterWidgets.Abstractions
{
    public interface ITodoManager<TWidget> : IPermissionable, ICachable where TWidget : IWidget
    {
        Task<(TodoTask task, Exception ex)> CreateAsync(TodoTaskRequest request);
        Task<(TodoTask task, Exception ex)> UpdateAsync(TodoTaskRequest request);

        Task<(TodoList todoList, Exception ex)> CreateListAsync(TodoList request);
        Task<(TodoList todoList, Exception ex)> UpdateListAsync(TodoList request);
        Task<(TodoTask todoTask, Exception ex)> MoveTaskAsync(TodoTaskRequest request);

        Task<(TodoTask task, Exception ex)> GetTaskAsync(TodoTaskRequest request);
        Task<(TodoList todoList, Exception ex)> GetListByIdAsync(string id);

        Task<(List<TodoList> todoLists, Exception ex)> GetAllListsAsync();
        Task<(List<TodoTask> tasks, Exception ex)> GetTasksByListIdAsync(string listId);
        Task<(TodoList list, Exception ex)> GetDefaultTodoListAsync();

        Task<(TodoTask todoTask, Exception ex)> CreateChecklistItemAsync(ChecklistRequest request);
        Task<(TodoTask checkListTask, Exception ex)> UpdateChecklistItemAsync(ChecklistRequest request);

        (string glyph, Exception ex) GetIconForList(string listId);
        (bool success, Exception ex) SetIconForList(string listId, string glyph);

        Task<(bool success, Exception ex)> DeleteChecklistItemAsync(ChecklistRequest request);
        Task<(bool success, Exception ex)> DeleteTaskAsync(TaskDeletionRequest request);
        Task<(bool success, Exception ex)> DeleteListAsync(string id);
    }
}
