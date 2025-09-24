using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Helpers;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace BetterWidgets.ViewModel.WidgetsSettingsViews
{
    public partial class ToDoSettingsViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<TodoWidget> _settings;
        private readonly ITodoManager<TodoWidget> _todo;
        private readonly IMSGraphService _graph;
        #endregion

        public ToDoSettingsViewModel()
        {
            _logger = App.Services?.GetRequiredService<ILogger<ToDoSettingsViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<TodoWidget>>();
            _todo = App.Services?.GetKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<IWidget>));
            _graph = App.Services?.GetRequiredService<IMSGraphService>();

            if(_graph != null)
            {
                _graph.SignedIn += OnMSSignedIn;
                _graph.SignedOut += OnMSSignedOut;
            }
        }

        #region Props

        public bool ShowCompletedTasks
        {
            get => _settings?.GetSetting(nameof(ShowCompletedTasks), false) ?? false;
            set
            {
                _settings?.SetSetting(nameof(ShowCompletedTasks), value);

                OnPropertyChanged(nameof(ShowCompletedTasks));
            }
        }

        [ObservableProperty]
        public ObservableCollection<TodoListViewModel> todoLists;

        [ObservableProperty]
        public int selectedTodoList;

        [ObservableProperty]
        public bool isSignedIn;

        public bool IsTaskCheckSoundEnabled
        {
            get => _settings.GetSetting(nameof(IsTaskCheckSoundEnabled), true);
            set
            {
                _settings.SetSetting(nameof(IsTaskCheckSoundEnabled), value);
                OnPropertyChanged(nameof(IsTaskCheckSoundEnabled));
            }
        }

        public int MaxTasksCount
        {
            get => _settings.GetSetting(nameof(MaxTasksCount), 10);
            set => _settings.SetSetting(nameof(MaxTasksCount), value);
        }

        public int TasksSortBy
        {
            get => _settings.GetSetting(nameof(TasksSortBy), 2);
            set => _settings.SetSetting(nameof(TasksSortBy), value);
        }

        public bool IsUpdateEnabled
        {
            get => _settings.GetSetting(nameof(IsUpdateEnabled), false);
            set
            {
                _settings.SetSetting(nameof(IsUpdateEnabled), value);
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public int UpdateInterval
        {
            get => _settings.GetSetting(nameof(UpdateInterval), 35);
            set => _settings.SetSetting(nameof(UpdateInterval), value);
        }

        public bool EnableTextWrap
        {
            get => _settings.GetSetting(nameof(EnableTextWrap), false);
            set => _settings.SetSetting(nameof(EnableTextWrap), value);
        }

        #endregion

        #region Utils

        private string GetSelectedTodoListId()
            => _settings?.GetSetting<string>(nameof(SelectedTodoList), null);

        private TodoListViewModel GetSelectedTodoList()
        {
            if(TodoLists == null) return null;

            string listId = GetSelectedTodoListId();

            return string.IsNullOrEmpty(listId) ?
                   TodoLists.FirstOrDefault(l => l.IsDefault) :
                   TodoLists.FirstOrDefault(l => l.Id == listId);
        }

        private async Task<ObservableCollection<TodoListViewModel>> GetTodoListsAsync()
        {
            try
            {
                var lists = await _todo.GetAllListsAsync();

                if(lists.ex != null) throw lists.ex;
                if(lists.todoLists == null) return new ObservableCollection<TodoListViewModel>();

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

        public ICommand GoBackCommand => new RelayCommand(ShellHelper.GoBack);

        [RelayCommand]
        private async Task OnLoadedAsync()
        {
            IsSignedIn = _graph?.IsSignedIn ?? false;
            
            TodoLists = await GetTodoListsAsync();
            SelectedTodoList = TodoLists.IndexOf(GetSelectedTodoList());
        }

        #endregion

        #region Handlers

        async partial void OnSelectedTodoListChanged(int value)
        {
            if(value < 0) return;
            if(TodoLists == null) return;
            if(TodoLists.Count == 0) return;

            var list = TodoLists[value];

            _settings?.SetSetting(nameof(SelectedTodoList), list.Id);

            await _todo.ResetCacheAsync(FileNames.todoList);
        }

        private void OnMSSignedOut(object sender, EventArgs e)
        {
            IsSignedIn = _graph.IsSignedIn;
        }

        private void OnMSSignedIn(object sender, EventArgs e)
        {
            IsSignedIn = _graph.IsSignedIn;
        }

        #endregion
    }
}
