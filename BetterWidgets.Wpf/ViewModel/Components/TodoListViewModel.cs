using System.Windows;
using BetterWidgets.Abstractions;
using BetterWidgets.Model.Tasks;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.ViewModel.Components
{
    public partial class TodoListViewModel : ObservableObject
    {
        #region Services
        private readonly ITodoManager<TodoWidget> _todo;
        #endregion

        public TodoListViewModel() : this(null) { }

        public TodoListViewModel(TodoList todoList)
        {
            _todo = App.Services?.GetKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<TodoWidget>));

            if(todoList == null) return;

            Id = todoList.Id;
            Title = todoList.Title;
            Icon = GetAssociatedIcon(todoList);
        }

        public string Id { get; set; }

        [ObservableProperty]
        public string title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IconVisibility))]
        public string icon;

        [ObservableProperty]
        public bool isDefault;

        public Visibility IconVisibility => !string.IsNullOrEmpty(Icon) ? 
                                            Visibility.Visible : Visibility.Collapsed;

        #region Utils

        private string GetAssociatedIcon(TodoList todoList)
        {
            if(todoList == null) throw new ArgumentNullException(nameof(todoList));

            var icon = _todo.GetIconForList(todoList.Id);

            if(icon.ex != null) throw icon.ex;

            return icon.glyph;
        }

        #endregion

        #region Handlers

        partial void OnIconChanged(string value)
        {
            _todo.SetIconForList(Id, value);
        }

        #endregion
    }
}
