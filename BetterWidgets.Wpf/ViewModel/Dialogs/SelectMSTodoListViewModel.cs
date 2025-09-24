using System.Windows;
using BetterWidgets.Abstractions;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using BetterWidgets.Controls;
using BetterWidgets.Services;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class SelectMSTodoListViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly ITodoManager<TodoWidget> _todo;
        #endregion

        public SelectMSTodoListViewModel()
        {
            _logger = App.Services?.GetRequiredService<ILogger<SelectMSTodoListViewModel>>();
            _todo = App.Services?.GetKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<IWidget>));
        }

        #region Props

        private Widget Widget { get; set; }

        [ObservableProperty]
        public ObservableCollection<TodoListViewModel> itemsSource;

        [ObservableProperty]
        public TodoListViewModel selectedItem;

        [ObservableProperty]
        public DataTemplate itemTemplate;

        [ObservableProperty]
        public bool isLoading;

        #endregion

        #region Utils

        private async Task<ObservableCollection<TodoListViewModel>> GetTodoListsAsync()
        {
            try
            {
                IsLoading = true;

                var lists = await _todo.GetAllListsAsync();

                if(lists.ex != null) throw lists.ex;
                if(lists.todoLists == null) return new ObservableCollection<TodoListViewModel>();

                IsLoading = false;

                return new ObservableCollection<TodoListViewModel>(
                    lists.todoLists.Select(i => new TodoListViewModel(i))
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return new ObservableCollection<TodoListViewModel>();
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private async Task OnLoadedAsync(Widget widget)
        {
            Widget = widget;
            ItemTemplate = (DataTemplate)Application.Current.Resources["TodoListItemTemplate"];
            ItemsSource = await GetTodoListsAsync();

            if(Widget != null) 
               Widget.IsDialogPrimaryButtonEnabled = false;
        }

        #endregion

        #region Handlers

        partial void OnSelectedItemChanged(TodoListViewModel value)
        {
            if(Widget != null)
               Widget.IsDialogPrimaryButtonEnabled = value != null;
        }

        #endregion
    }
}
