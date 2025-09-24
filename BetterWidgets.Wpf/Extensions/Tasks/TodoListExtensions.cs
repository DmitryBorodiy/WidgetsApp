using BetterWidgets.Model.Tasks;

namespace BetterWidgets.Extensions.Tasks
{
    public static class TodoListExtensions
    {
        public static TodoList ToTodoList(this Microsoft.Graph.Models.TodoTaskList todoTaskList) => new TodoList()
        {
            Id = todoTaskList.Id,
            IsDefault = todoTaskList.WellknownListName == Microsoft.Graph.Models.WellknownListName.DefaultList,
            Title = todoTaskList.DisplayName
        };

        public static Microsoft.Graph.Models.TodoTaskList ToGraphTodoList(this TodoList todoList) => new Microsoft.Graph.Models.TodoTaskList()
        {
            Id = todoList.Id,
            DisplayName = todoList.Title
        };

        public static Microsoft.Graph.Models.TodoTaskList ToGraphTodoListCreationRequest(this TodoList todoList) => new Microsoft.Graph.Models.TodoTaskList()
        {
            DisplayName = todoList.Title
        };
    }
}
