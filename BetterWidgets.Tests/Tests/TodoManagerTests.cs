using BetterWidgets.Abstractions;
using BetterWidgets.Extensions.Tasks;
using BetterWidgets.Model.DTO;
using BetterWidgets.Model.Tasks;
using BetterWidgets.Services;
using BetterWidgets.Tests.Consts;
using BetterWidgets.Tests.Fixtures;
using BetterWidgets.Tests.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Tests
{
    public class TodoManagerTests : IClassFixture<TodoManagerFixture>
    {
        #region Services
        private readonly ITodoManager<ToDoWidget> _todo;
        #endregion

        public TodoManagerTests(TodoManagerFixture fixture)
        {
            _todo = fixture.Services.GetKeyedService<ITodoManager<ToDoWidget>>(nameof(MSTodoManager<IWidget>));
        }

        [Fact]
        public async Task Should_Get_All_Task_Lists()
        {
            var result = await _todo.GetCachedAsync
            (
                FileNames.todoCache,
                _todo.GetAllListsAsync,
                true
            );

            if(result.ex != null) throw result.ex;

            Assert.NotNull(result.data);
            Assert.NotEmpty(result.data);
        }

        [Fact]
        public async Task Should_Get_All_Tasks_In_List()
        {
            var lists = await _todo.GetCachedAsync
            (
                FileNames.todoCache,
                _todo.GetAllListsAsync,
                true
            );

            if (lists.ex != null) throw lists.ex;

            Assert.NotNull(lists.data);

            var list = lists.data.FirstOrDefault();

            Assert.NotNull(list);

            var tasks = await _todo.GetCachedAsync
            (
                FileNames.todoCache,
                () => _todo.GetTasksByListIdAsync(list.Id),
                true
            );

            if (tasks.ex != null) throw tasks.ex;

            Assert.NotNull(tasks.data);
        }

        [Fact]
        public async Task Should_Create_Task_List()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createResult = await _todo.CreateListAsync(todoList);

            if(createResult.ex != null) throw createResult.ex;

            Assert.NotNull(createResult.todoList);
            Assert.Equal(createResult.todoList.Title, createResult.todoList.Title);

            var deletionResult = await _todo.DeleteListAsync(createResult.todoList.Id);

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);
        }

        [Fact]
        public async Task Should_Create_Task_In_List()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            var todoTask = new TodoTaskRequest
            {
                ListId = createListResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var createTaskResult = await _todo.CreateAsync(todoTask);

            if(createTaskResult.ex != null) throw createTaskResult.ex;

            Assert.NotNull(createTaskResult.task);
            Assert.Equal(createTaskResult.task.Title, todoTask.Title);
            Assert.Equal(createTaskResult.task.DueDateTime?.Date, todoTask.DueDate.Value.Date);
            Assert.Equal(createTaskResult.task.StartDateTime?.Date, todoTask.StartDate.Value.Date);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Update_Task_In_List()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            var todoTask = new TodoTaskRequest
            {
                ListId = createListResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var createTaskResult = await _todo.CreateAsync(todoTask);

            if(createTaskResult.ex != null) throw createTaskResult.ex;

            Assert.NotNull(createTaskResult.task);
            Assert.Equal(createTaskResult.task.Title, todoTask.Title);
            Assert.Equal(createTaskResult.task.DueDateTime?.Date, todoTask.DueDate.Value.Date);
            Assert.Equal(createTaskResult.task.StartDateTime?.Date, todoTask.StartDate.Value.Date);

            todoTask.Id = createTaskResult.task.Id;
            todoTask.Title = Guid.NewGuid().ToString();
            todoTask.DueDate = todoTask.DueDate?.AddDays(5);
            todoTask.IsImportant = true;

            var updatedTask = await _todo.UpdateAsync(todoTask);

            if(updatedTask.ex != null) throw updatedTask.ex;

            Assert.NotNull(updatedTask.task);
            Assert.Equal(updatedTask.task.Title, todoTask.Title);
            Assert.Equal(updatedTask.task.DueDateTime?.Date, todoTask.DueDate.Value.Date);
            Assert.Equal(updatedTask.task.IsImportant, updatedTask.task.IsImportant);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Get_Task_By_Id()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            var todoTask = new TodoTaskRequest
            {
                ListId = createListResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var createTaskResult = await _todo.CreateAsync(todoTask);

            if(createTaskResult.ex != null) throw createTaskResult.ex;

            Assert.NotNull(createTaskResult.task);
            Assert.Equal(createTaskResult.task.Title, todoTask.Title);
            Assert.Equal(createTaskResult.task.DueDateTime?.Date, todoTask.DueDate.Value.Date);
            Assert.Equal(createTaskResult.task.StartDateTime?.Date, todoTask.StartDate.Value.Date);

            var request = new TodoTaskRequest
            {
                Id = createTaskResult.task.Id,
                ListId = createListResult.todoList.Id
            };

            var getTaskResult = await _todo.GetTaskAsync(request);

            if(getTaskResult.ex != null) throw getTaskResult.ex;

            Assert.NotNull(getTaskResult.task);
            Assert.Equal(getTaskResult.task.Id, createTaskResult.task.Id);
            Assert.Equal(getTaskResult.task.Title, createTaskResult.task.Title);
            Assert.Equal(getTaskResult.task.DueDateTime?.Date, createTaskResult.task.DueDateTime?.Date);
            Assert.Equal(getTaskResult.task.StartDateTime?.Date, createTaskResult.task.StartDateTime?.Date);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Get_List_By_Id()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            var getListResult = await _todo.GetListByIdAsync(createListResult.todoList.Id);

            if(getListResult.ex != null) throw getListResult.ex;

            Assert.NotNull(getListResult.todoList);
            Assert.Equal(getListResult.todoList.Id, createListResult.todoList.Id);
            Assert.Equal(getListResult.todoList.Title, createListResult.todoList.Title);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Update_List()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            todoList.Id = createListResult.todoList.Id;
            todoList.Title = Guid.NewGuid().ToString();

            var updateListResult = await _todo.UpdateListAsync(todoList);

            if(updateListResult.ex != null) throw updateListResult.ex;

            Assert.NotNull(updateListResult.todoList);
            Assert.Equal(updateListResult.todoList.Id, createListResult.todoList.Id);
            Assert.Equal(updateListResult.todoList.Title, todoList.Title);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Delete_Task()
        {
            var todoList = new TodoList
            {
                Title = Guid.NewGuid().ToString()
            };

            var createListResult = await _todo.CreateListAsync(todoList);

            if(createListResult.ex != null) throw createListResult.ex;

            Assert.NotNull(createListResult.todoList);
            Assert.Equal(createListResult.todoList.Title, createListResult.todoList.Title);

            var todoTask = new TodoTaskRequest
            {
                ListId = createListResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var createTaskResult = await _todo.CreateAsync(todoTask);

            if(createTaskResult.ex != null) throw createTaskResult.ex;

            Assert.NotNull(createTaskResult.task);
            Assert.Equal(createTaskResult.task.Title, todoTask.Title);
            Assert.Equal(createTaskResult.task.DueDateTime?.Date, todoTask.DueDate.Value.Date);
            Assert.Equal(createTaskResult.task.StartDateTime?.Date, todoTask.StartDate.Value.Date);

            var deletionResult = await _todo.DeleteTaskAsync(new TaskDeletionRequest
            {
                Id = createTaskResult.task.Id,
                ListId = createListResult.todoList.Id
            });

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);

            var deleteListResult = await _todo.DeleteListAsync(createListResult.todoList.Id);

            if(deleteListResult.ex != null) throw deleteListResult.ex;

            Assert.True(deleteListResult.success);
        }

        [Fact]
        public async Task Should_Move_Task()
        {
            #region CreateLists

            var firstList = new TodoList { Title = Guid.NewGuid().ToString() };
            var secondList = new TodoList { Title = Guid.NewGuid().ToString() };

            var createFirstListResult = await _todo.CreateListAsync(firstList);
            var createSecondListResult = await _todo.CreateListAsync(secondList);

            if(createFirstListResult.ex != null) throw createFirstListResult.ex;
            if(createSecondListResult.ex != null) throw createSecondListResult.ex;

            Assert.NotNull(createFirstListResult.todoList);
            Assert.Equal(createFirstListResult.todoList.Title, createFirstListResult.todoList.Title);
            Assert.NotNull(createSecondListResult.todoList);
            Assert.Equal(createSecondListResult.todoList.Title, createSecondListResult.todoList.Title);

            #endregion

            #region CreateTask

            var taskRequest = new TodoTaskRequest()
            {
                ListId = createFirstListResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var taskCreationResult = await _todo.CreateAsync(taskRequest);

            if(taskCreationResult.ex != null) throw taskCreationResult.ex;

            Assert.NotNull(taskCreationResult.task);
            Assert.Equal(taskCreationResult.task.Title, taskRequest.Title);
            Assert.Equal(taskCreationResult.task.DueDateTime?.Date, taskRequest.DueDate.Value.Date);
            Assert.Equal(taskCreationResult.task.StartDateTime?.Date, taskRequest.StartDate.Value.Date);

            #endregion

            #region MoveTask

            var moveTaskRequest = taskCreationResult.task.TodoTaskRequest(createSecondListResult.todoList.Id);

            var moveTaskResult = await _todo.MoveTaskAsync(moveTaskRequest);

            if(moveTaskResult.ex != null) throw moveTaskResult.ex;

            Assert.NotNull(moveTaskResult.todoTask);
            Assert.Equal(moveTaskResult.todoTask.Title, taskRequest.Title);
            Assert.Equal(moveTaskResult.todoTask.DueDateTime?.Date, taskRequest.DueDate.Value.Date);
            Assert.Equal(moveTaskResult.todoTask.StartDateTime?.Date, taskRequest.StartDate.Value.Date);

            #endregion

            #region AssertMovement

            var movedTaskRequest = moveTaskResult.todoTask.TodoTaskRequest(createSecondListResult.todoList.Id);

            var movedTask = await _todo.GetTaskAsync(movedTaskRequest);

            if(movedTask.ex != null) throw movedTask.ex;

            Assert.NotNull(movedTask.task);

            #endregion

            #region DeleteLists

            var firstListDeletionResult = await _todo.DeleteListAsync(createFirstListResult.todoList.Id);
            var secondListDeletionResult = await _todo.DeleteListAsync(createSecondListResult.todoList.Id);

            if(firstListDeletionResult.ex != null) throw firstListDeletionResult.ex;
            if(secondListDeletionResult.ex != null) throw secondListDeletionResult.ex;

            Assert.True(firstListDeletionResult.success);
            Assert.True(secondListDeletionResult.success);

            #endregion
        }

        [Fact]
        public async Task Should_Get_Default_Todo_List()
        {
            var todoList = await _todo.GetDefaultTodoListAsync();

            if(todoList.ex != null) throw todoList.ex;

            Assert.NotNull(todoList.list);
            Assert.True(todoList.list.IsDefault);
        }

        [Fact]
        public async Task Should_Complete_Task()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);
            Assert.Equal(todoTask.Title, todoTaskCreationResult.task.Title);
            Assert.Equal(todoTask.DueDate.Value.Date, todoTaskCreationResult.task.DueDateTime?.Date);
            Assert.Equal(todoTask.StartDate.Value.Date, todoTaskCreationResult.task.CreatedDateTime?.Date);

            var completedTaskRequest = new TodoTaskRequest()
            {
                Id = todoTaskCreationResult.task.Id,
                ListId = todoListCreationResult.todoList.Id,
                Title = todoTaskCreationResult.task.Title,
                IsCompleted = true
            };

            var updatedTaskResult = await _todo.UpdateAsync(completedTaskRequest);

            if(updatedTaskResult.ex != null) throw updatedTaskResult.ex;

            Assert.NotNull(updatedTaskResult.task);
            Assert.True(updatedTaskResult.task.IsCompleted);

            var deletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);
        }

        [Fact]
        public async Task Should_Complete_SubTask()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);

            var checkListItem = new ChecklistRequest
            {
                TaskId = todoTaskCreationResult.task.Id,
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString()
            };

            var checklistCreationResult = await _todo.CreateChecklistItemAsync(checkListItem);

            if(checklistCreationResult.ex != null) throw checklistCreationResult.ex;

            Assert.NotNull(checklistCreationResult.todoTask);
            Assert.Equal(checklistCreationResult.todoTask.Title, checkListItem.Title);
            Assert.Equal(checklistCreationResult.todoTask.CreatedDateTime?.Date, DateTime.Now.Date);

            checkListItem.IsChecked = true;
            checkListItem.Id = checklistCreationResult.todoTask.SubTaskId;

            var checklistUpdateResult = await _todo.UpdateChecklistItemAsync(checkListItem);

            if(checklistUpdateResult.ex != null) throw checklistUpdateResult.ex;

            Assert.True(checklistUpdateResult.checkListTask.IsCompleted);
            Assert.Equal(checklistUpdateResult.checkListTask.SubTaskId, checkListItem.Id);

            var todoListDeletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);

            if(todoListDeletionResult.ex != null) throw todoListCreationResult.ex;

            Assert.True(todoListDeletionResult.success);
        }

        [Fact]
        public async Task Should_Create_SubTask()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);

            var checkListItem = new ChecklistRequest
            {
                TaskId = todoTaskCreationResult.task.Id,
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString()
            };

            var checklistCreationResult = await _todo.CreateChecklistItemAsync(checkListItem);

            if(checklistCreationResult.ex != null) throw checklistCreationResult.ex;

            Assert.NotNull(checklistCreationResult.todoTask);
            Assert.Equal(checklistCreationResult.todoTask.Title, checkListItem.Title);
            Assert.Equal(checklistCreationResult.todoTask.CreatedDateTime?.Date, DateTime.Now.Date);

            var todoListDeletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);
            
            if(todoListDeletionResult.ex != null) throw todoListCreationResult.ex;

            Assert.True(todoListDeletionResult.success);
        }

        [Fact]
        public async Task Should_Delete_SubTask()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);

            var checkListItem = new ChecklistRequest
            {
                TaskId = todoTaskCreationResult.task.Id,
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString()
            };

            var checklistCreationResult = await _todo.CreateChecklistItemAsync(checkListItem);

            if(checklistCreationResult.ex != null) throw checklistCreationResult.ex;

            Assert.NotNull(checklistCreationResult.todoTask);
            Assert.Equal(checklistCreationResult.todoTask.Title, checkListItem.Title);
            Assert.Equal(checklistCreationResult.todoTask.CreatedDateTime?.Date, DateTime.Now.Date);

            checkListItem.Id = checklistCreationResult.todoTask.SubTaskId;

            var checkListItemDeletionResult = await _todo.DeleteChecklistItemAsync(checkListItem);

            if(checkListItemDeletionResult.ex != null) throw checkListItemDeletionResult.ex;

            Assert.True(checkListItemDeletionResult.success);

            var todoListDeletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);

            if(todoListDeletionResult.ex != null) throw todoListCreationResult.ex;

            Assert.True(todoListDeletionResult.success);
        }

        [Fact]
        public async Task Should_Rename_SubTask()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);

            var checkListItem = new ChecklistRequest
            {
                TaskId = todoTaskCreationResult.task.Id,
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString()
            };

            var checklistCreationResult = await _todo.CreateChecklistItemAsync(checkListItem);

            if(checklistCreationResult.ex != null) throw checklistCreationResult.ex;

            Assert.NotNull(checklistCreationResult.todoTask);
            Assert.Equal(checklistCreationResult.todoTask.Title, checkListItem.Title);
            Assert.Equal(checklistCreationResult.todoTask.CreatedDateTime?.Date, DateTime.Now.Date);

            checkListItem.Id = checklistCreationResult.todoTask.SubTaskId;
            checkListItem.Title = Guid.NewGuid().ToString();

            var checkListItemUpdateResult = await _todo.UpdateChecklistItemAsync(checkListItem);

            if(checkListItemUpdateResult.ex != null) throw checkListItemUpdateResult.ex;

            Assert.Equal(checkListItem.Id, checkListItemUpdateResult.checkListTask.SubTaskId);
            Assert.Equal(checkListItem.Title, checkListItemUpdateResult.checkListTask.Title);

            var deletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);
        }

        [Fact]
        public async Task Should_Set_Reminder_For_Task()
        {
            var todoList = new TodoList()
            {
                Title = Guid.NewGuid().ToString()
            };

            var todoListCreationResult = await _todo.CreateListAsync(todoList);

            if(todoListCreationResult.ex != null) throw todoListCreationResult.ex;

            Assert.NotNull(todoListCreationResult.todoList);

            var todoTask = new TodoTaskRequest()
            {
                ListId = todoListCreationResult.todoList.Id,
                Title = Guid.NewGuid().ToString(),
                DueDate = DateTime.Now.AddDays(1),
                StartDate = DateTime.Now
            };

            var todoTaskCreationResult = await _todo.CreateAsync(todoTask);

            if(todoTaskCreationResult.ex != null) throw todoTaskCreationResult.ex;

            Assert.NotNull(todoTaskCreationResult.task);
            Assert.Equal(todoTask.Title, todoTaskCreationResult.task.Title);
            Assert.Equal(todoTask.DueDate.Value.Date, todoTaskCreationResult.task.DueDateTime?.Date);
            Assert.Equal(todoTask.StartDate.Value.Date, todoTaskCreationResult.task.CreatedDateTime?.Date);

            todoTask.Id = todoTaskCreationResult.task.Id;
            todoTask.ListId = todoListCreationResult.todoList.Id;

            todoTask.IsReminderOn = true;
            todoTask.ReminderDate = DateTime.Now.AddDays(1).Date;

            var updateTaskResult = await _todo.UpdateAsync(todoTask);

            if(updateTaskResult.ex != null) throw updateTaskResult.ex;

            Assert.NotNull(updateTaskResult.task);
            Assert.True(updateTaskResult.task.IsReminderOn);
            Assert.Equal(updateTaskResult.task.ReminderDateTime?.Date, todoTask.ReminderDate.Value.Date);

            var deletionResult = await _todo.DeleteListAsync(todoListCreationResult.todoList.Id);

            if(deletionResult.ex != null) throw deletionResult.ex;

            Assert.True(deletionResult.success);
        }
    }
}
