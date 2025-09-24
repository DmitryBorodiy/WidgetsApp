using BetterWidgets.Abstractions;
using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Exceptions;
using BetterWidgets.Extensions.Tasks;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Model.DTO;
using BetterWidgets.Model.Tasks;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.ViewModel.Dialogs;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class TodoViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly IMSGraphService _msGraph;
        private readonly Settings<TodoWidget> _settings;
        private readonly ITodoManager<TodoWidget> _todo;
        private readonly IPermissionManager<TodoWidget> _permissions;
        private readonly IShareService _share;

        private DispatcherTimer _updateTimer;
        #endregion

        public TodoViewModel()
        {
            _msGraph = App.Services?.GetService<IMSGraphService>();
            _logger = App.Services?.GetRequiredService<ILogger<TodoViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<TodoWidget>>();
            _todo = App.Services?.GetKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<IWidget>));
            _permissions = App.Services?.GetService<IPermissionManager<TodoWidget>>();
            _share = App.Services?.GetService<IShareService>();

            if(_settings != null) _settings.ValueChanged += OnSettingsChanged;
            if(_msGraph != null)
            {
                IsSignedIn = _msGraph.IsSignedIn;

                _msGraph.SignedIn += OnMSSignedIn;
                _msGraph.SignedOut += OnMSSignOut;
            }
        }

        #region Props

        private Guid Id { get; set; }
        private Widget Widget { get; set; }

        [ObservableProperty]
        public bool isHideTitleBar = false;

        [ObservableProperty]
        public Visibility iconVisibility = Visibility.Visible;

        [ObservableProperty]
        public Thickness rootMargin = new Thickness(11, 43, 11, 0);

        [ObservableProperty]
        public bool isLoading;

        [ObservableProperty]
        public bool isSignedIn;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasListIcon))]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        public TodoListViewModel todoList;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        public ObservableCollection<TodoTaskViewModel> todoTasks;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTaskSelected))]
        public TodoTaskViewModel selectedTodoTask;

        public bool IsEmpty => (TodoList == null || TodoTasks == null ||
                               TodoTasks.Count == 0) && IsSignedIn && !IsLoading;

        public bool HasListIcon => !string.IsNullOrEmpty(TodoList?.Icon);

        public bool IsUpdateEnabled => _settings?.GetSetting(nameof(IsUpdateEnabled), false) ?? false;
        public int UpdateInterval => _settings?.GetSetting(nameof(UpdateInterval), 35) ?? 35;
        public bool IsTaskSelected => TodoList != null && SelectedTodoTask != null;

        #endregion

        #region Settings

        private string SelectedTodoList
        {
            get => _settings?.GetSetting<string>(nameof(SelectedTodoList), null);
            set => _settings?.SetSetting(nameof(SelectedTodoList), value);
        }

        public bool ShowCompletedTasks => _settings?.GetSetting(nameof(ShowCompletedTasks), false) ?? false;
        public int MaxTasksCount => _settings?.GetSetting(nameof(MaxTasksCount), 10) ?? 10;
        public TasksSortBy TasksSortBy => (TasksSortBy)(_settings?.GetSetting(nameof(TasksSortBy), 2) ?? 2);

        #endregion

        #region Utils

        private void ShowSelectTaskMessage() => Widget?.ShowNotify(
            Resources.Resources.SelectTaskMessage,
            Resources.Resources.SelectTask,
            true,
            InfoBarSeverity.Informational,
            true,
            TimeSpan.FromSeconds(6));

        private void SetLayoutState(WidgetSizes size)
        {
            if(Widget?.Content is Panel rootLayout)
            {
                if(rootLayout.Resources.Contains(size.ToString()) &&
                   rootLayout.Resources[size.ToString()] is Style rootLayoutStyle)
                   rootLayout.Style = rootLayoutStyle;

                foreach(var element in rootLayout.Children)
                {
                    if(element is FrameworkElement control)
                    {
                        if(control.Resources.Contains(size.ToString()) &&
                           control.Resources[size.ToString()] is Style style)
                           control.Style = style;
                    }
                }
            }
        }

        private ObservableCollection<TodoTaskViewModel> SortTasks(IEnumerable<TodoTaskViewModel> tasks, TasksSortBy sortBy = TasksSortBy.Importance)
        {
            if(sortBy == TasksSortBy.Date)
               tasks = tasks.OrderBy(t => t.DueDate == DateTime.MinValue ? DateTime.MaxValue : t.DueDate);

            if(sortBy == TasksSortBy.Importance)
               tasks = tasks.OrderByDescending(t => t.IsImportant);

            if(sortBy == TasksSortBy.Name)
               tasks = tasks.OrderBy(t => t.Title);

            if(!ShowCompletedTasks)
               tasks = tasks.Where(t => !t.IsCompleted);

            return new ObservableCollection<TodoTaskViewModel>(tasks.Take(MaxTasksCount));
        }

        private async Task<TodoListViewModel> GetCurrentTodoListAsync(bool refresh = true)
        {
            try
            {
                TodoListViewModel listViewModel = null;
                var permissionState = _permissions.TryCheckPermissionState(new Permission(Scopes.Tasks));

                if(permissionState != PermissionState.Allowed)
                {
                    Widget?.ShowNotify(
                        title: Resources.Resources.NoPermission,
                        message: Resources.Resources.TasksPermissionSubtitle,
                        severity: InfoBarSeverity.Warning);

                    return null;
                }
                else Widget?.HideNotify();

                bool canRefresh = NetworkHelper.IsConnected && 
                                  refresh && 
                                  !Widget.IsPreview && 
                                  _msGraph.Client != null;
                
                var list = string.IsNullOrEmpty(SelectedTodoList) ?
                        await _todo.GetCachedAsync(
                            FileNames.todoList,
                            _todo.GetDefaultTodoListAsync,
                            canRefresh) :
                        await _todo.GetCachedAsync(
                            FileNames.todoList,
                            () => _todo.GetListByIdAsync(SelectedTodoList),
                            canRefresh);

                if(list.ex != null && list.ex.GetType() != typeof(NetworkUnavailableException)) throw list.ex;

                if(list.Item1 != null)
                   listViewModel = new TodoListViewModel(list.Item1);

                if(listViewModel != null &&
                   !string.IsNullOrEmpty(listViewModel.Icon))
                   IconVisibility = Visibility.Visible;

                return listViewModel;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private async Task<ObservableCollection<TodoTaskViewModel>> GetTodoTasksAsync(TodoListViewModel list, bool refresh = false, TasksSortBy sortBy = TasksSortBy.Importance)
        {
            try
            {
                if(list == null) return null;
                var permissionState = _permissions.TryCheckPermissionState(new Permission(Scopes.Tasks));

                if(permissionState != PermissionState.Allowed)
                {
                    Widget?.ShowNotify(
                        title: Resources.Resources.NoPermission,
                        message: Resources.Resources.TasksPermissionSubtitle,
                        severity: InfoBarSeverity.Warning);

                    return null;
                }
                else Widget?.HideNotify();

                var tasks = await _todo.GetCachedAsync(
                    FileNames.todoTasks,
                    () => _todo.GetTasksByListIdAsync(list.Id),
                    NetworkHelper.IsConnected && refresh && !Widget.IsPreview && _msGraph.Client != null && _msGraph.IsSignedIn);

                if(tasks.ex != null &&
                   tasks.ex.GetType() != typeof(NetworkUnavailableException)) throw tasks.ex;
                if(tasks.data == null) return new ObservableCollection<TodoTaskViewModel>();

                var taskViewModels = tasks.data.Select(t => new TodoTaskViewModel(t, list.Id, Widget, this));

                return SortTasks(taskViewModels, sortBy);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private void AttachUpdateTimer()
        {
            if(!IsUpdateEnabled) return;
            if(_updateTimer == null)
            {
                _updateTimer = new DispatcherTimer(); ;
                _updateTimer.Tick += OnUpdateTick;
            }

            _updateTimer.Interval = TimeSpan.FromMinutes(UpdateInterval);
            _updateTimer.Start();
        }

        private void DetachUpdateTimer()
        {
            if(_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer.Tick -= OnUpdateTick;

                _updateTimer = null;
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void OnAppeared(Widget widget)
        {
            if(widget != null)
            {
                Widget = widget;
                Id = widget.Id;

                widget.NetworkStateChanged += OnNetworkStateChanged;

                RootMargin = widget.IsPreview ?
                             new Thickness(12, 16, 12, 0) :
                             new Thickness(12, 43, 12, 0);

                if(!widget.IsPreview) AttachUpdateTimer();
                else RefreshCommand.Execute(default);
            }
        }

        [RelayCommand]
        private async Task OnPinnedAsync()
        {
            AttachUpdateTimer();

            var permission = new Permission(Scopes.Tasks);
            var permissionState = _permissions.TryCheckPermissionState(permission);

            if(permissionState != PermissionState.Allowed)
            {
                permissionState = await _permissions.RequestAccessAsync(permission);

                if(permissionState == PermissionState.Allowed)
                   RefreshCommand.Execute(Widget);
                else
                   Widget?.ShowNotify(
                       title: Resources.Resources.NoPermission,
                       message: Resources.Resources.TasksPermissionSubtitle,
                       severity: InfoBarSeverity.Warning);
            }
        }

        [RelayCommand]
        private void OnUnpined()
        {
            if(Widget != null)
               Widget.NetworkStateChanged -= OnNetworkStateChanged;

            DetachUpdateTimer();
        }

        [RelayCommand]
        private void OnExecutionStateChanged(WidgetState state)
        {
            if(state == WidgetState.Activated) _updateTimer?.Start();
            else _updateTimer?.Stop();
        }

        [RelayCommand]
        private void OnSizeChanged(Size size)
        {
            var widgetSize = WidgetSize.GetSize(size);

            IsHideTitleBar = widgetSize == WidgetSizes.Small;
            IconVisibility = widgetSize == WidgetSizes.Small ?
                             Visibility.Collapsed : Visibility.Visible;
            RootMargin =  widgetSize == WidgetSizes.Small ?
                          new Thickness(0) : new Thickness(13, 43, 13, 0);

            SetLayoutState(widgetSize);
        }

        [RelayCommand]
        private void LaunchSettings()
        {
            if(Id == default) return;

            ShellHelper.LaunchSettingsById(Id);
        }

        [RelayCommand]
        private void RequestSelectListDialog()
        {
            if(Widget == null) return;
            if(Widget.IsWidgetDialogOpen) return;

            if(Widget.IsPreview)
            {
                ShellHelper.LaunchSettingsById(Widget.Id);
                return;
            }

            var view = new ListDialogView();
            var viewModel = new SelectMSTodoListViewModel();

            view.DataContext = viewModel;

            var dialogParameters = new WidgetContentDialogParams()
            {
                Title = Resources.Resources.SelectList,
                Content = view,
                PrimaryButtonContent = Resources.Resources.SelectLabel,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonParameter = viewModel,
                PrimaryButtonCommand = SelectListCommand
            };

            viewModel.LoadedCommand.Execute(Widget);

            Widget.ShowContentDialog(dialogParameters);
        }

        [RelayCommand]
        private async Task SelectList(SelectMSTodoListViewModel viewModel)
        {
            if(viewModel == null) return;
            if(viewModel.SelectedItem == null) return;

            TodoList = viewModel.SelectedItem;
            SelectedTodoList = viewModel.SelectedItem.Id;

            await _todo.ResetCacheAsync(FileNames.todoList);
        }

        [RelayCommand]
        private void RequestMoveTask(TodoTaskViewModel task)
        {
            if(task == null)
            {
                ShowSelectTaskMessage();

                return;
            }

            SelectedTodoTask = task;

            var view = new ListDialogView();
            var viewModel = new SelectMSTodoListViewModel();

            view.DataContext = viewModel;

            var dialogParameters = new WidgetContentDialogParams()
            {
                Title = Resources.Resources.SelectList,
                Content = view,
                PrimaryButtonContent = Resources.Resources.SelectLabel,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonParameter = viewModel,
                PrimaryButtonCommand = MoveTaskCommand
            };

            viewModel.LoadedCommand.Execute(Widget);

            Widget.ShowContentDialog(dialogParameters);
        }

        [RelayCommand]
        private async Task MoveTaskAsync(SelectMSTodoListViewModel listVM)
        {
            if(listVM == null) return;
            if(listVM.SelectedItem == null)
            {
                ShowSelectTaskMessage();
                return;
            }

            string selectedListId = listVM.SelectedItem.Id;
            var moveRequest = SelectedTodoTask.TodoTask?.TodoTaskRequest(selectedListId);

            var moveResult = await _todo.MoveTaskAsync(moveRequest);

            if(moveResult.ex != null)
            {
                Widget?.ShowNotify(
                    moveResult.ex.Message,
                    Resources.Resources.CannotMoveTask,
                    true,
                    InfoBarSeverity.Error,
                    true,
                    TimeSpan.FromSeconds(10));

                return;
            }

            if(moveResult.todoTask != null)
            {
                var taskView = TodoTasks.FirstOrDefault(t => t.Id == moveRequest.Id);

                if(taskView != null)
                   TodoTasks.Remove(taskView);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsLoading = true;

            if(TodoList == null)
               TodoList = await GetCurrentTodoListAsync();

            if(TodoList != null)
               TodoTasks = await GetTodoTasksAsync(TodoList, true, TasksSortBy);

            IsLoading = false;
        }

        [RelayCommand]
        private void RequestDeleteSelectedTodoTask()
        {
            if(Widget == null) return;

            var dialogParameters = new WidgetContentDialogParams()
            {
                Title = Resources.Resources.DeleteTaskTitle,
                Content = Resources.Resources.DeleteTaskSubtitle,
                PrimaryButtonContent = Resources.Resources.DeleteLabel,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonParameter = SelectedTodoTask,
                PrimaryButtonCommand = DeleteSelectedTodoTaskCommand
            };

            if(!Widget?.IsWidgetDialogOpen ?? false)
               Widget?.ShowContentDialog(dialogParameters);
        }

        [RelayCommand]
        private async Task DeleteSelectedTodoTaskAsync(TodoTaskViewModel task)
        {
            try
            {
                var request = new TaskDeletionRequest()
                {
                    Id = task.Id,
                    ListId = task.ListId
                };

                var result = await _todo?.DeleteTaskAsync(request);

                if(result.ex != null) throw result.ex;

                if(result.success) TodoTasks?.Remove(task);
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    Resources.Resources.NoNetworkSubtitle,
                    Resources.Resources.NoNetworkTitle,
                    true,
                    InfoBarSeverity.Warning
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void ViewTodoTask()
        {
            if(SelectedTodoTask == null) return;

            var view = new EditTaskDialog();
            var viewModel = new EditTodoTaskViewModel(SelectedTodoTask, Widget, this, FormMode.Edit);

            view.DataContext = viewModel;

            var parameters = new WidgetContentDialogParams()
            {
                Content = view,
                PrimaryButtonContent = viewModel.PrimaryButtonText,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                TitleBarVisibility = Visibility.Collapsed,
                PrimaryButtonCommand = viewModel.SaveCommand
            };

            if(!Widget?.IsWidgetDialogOpen ?? false)
               Widget?.ShowContentDialog(parameters);
        }

        [RelayCommand]
        private void RequestCreateTask()
        {
            SelectedTodoTask = null;

            var todoTask = new TodoTask()
            {
                Title = string.Empty,
                StartDateTime = DateTime.Now,
                IsReminderOn = true,
                ReminderDateTime = DateTime.Now,
                DueDateTime = DateTime.Now,
                CreatedDateTime = DateTime.Now,
                LastModifiedDateTime = DateTime.Now,
                IsSubTask = false
            };

            var todoTaskViewModel = new TodoTaskViewModel(todoTask, SelectedTodoList, Widget, this);

            var view = new EditTaskDialog();
            var viewModel = new EditTodoTaskViewModel(todoTaskViewModel, Widget, this, FormMode.Create);

            view.DataContext = viewModel;

            var parameters = new WidgetContentDialogParams()
            {
                Content = view,
                PrimaryButtonContent = viewModel.PrimaryButtonText,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonAppearance = ControlAppearance.Primary,
                TitleBarVisibility = Visibility.Collapsed,
                PrimaryButtonCommand = viewModel.SaveCommand
            };

            if(!Widget?.IsWidgetDialogOpen ?? false) 
               Widget?.ShowContentDialog(parameters);
        }

        [RelayCommand]
        private void ChooseListIcon()
        {
            if(Widget == null) return;

            var picker = new EmojiPicker();

            var parameters = new WidgetContentDialogParams()
            {
                Content = picker,
                Title = Resources.Resources.ChooseListIcon,
                PrimaryButtonContent = Resources.Resources.SelectLabel,
                SecondaryButtonContent = Resources.Resources.CancelLabel,
                PrimaryButtonParameter = picker,
                PrimaryButtonCommand = SelectListIconCommand
            };

            Widget.ShowContentDialog(parameters);
        }

        [RelayCommand]
        private void SelectListIcon(EmojiPicker picker)
        {
            if(string.IsNullOrEmpty(picker.Selected)) return;
            if(TodoList == null) return;
            
            TodoList.Icon = picker.Selected;
        }

        [RelayCommand]
        private void ClearIcon()
        {
            if(TodoList != null)
               TodoList.Icon = null;
        }

        [RelayCommand]
        private async Task SortTasks(int parameter)
        {
            if(parameter < 0) return;
            if(parameter > 2) return;

            var sortBy = (TasksSortBy)parameter;

            TodoTasks = await GetTodoTasksAsync(TodoList, false, sortBy);
        }

        [RelayCommand]
        private void OnKeyDownTasksCollection(KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Space)
               ViewTodoTaskCommand.Execute(default);
        }

        [RelayCommand]
        private void Share()
        {
            if(!IsTaskSelected) return;

            string title = SelectedTodoTask.Title;
            string subtitle = string.Empty;

            StringBuilder builder = new StringBuilder();

            if(SelectedTodoTask.IsCompleted) title += $" ✔️";

            builder.AppendLine(title);
            builder.AppendLine("----------------------------------------");

            subtitle = SelectedTodoTask.IsReminderOn ? 
                       $"{SelectedTodoTask.ReminderDate?.ToString(DateFormatValidator.Default)} - {SelectedTodoTask.ReminderDate?.ToString(TimeFormatValidator.Default)}" :
                       SelectedTodoTask.DueDate?.ToString(DateFormatValidator.Default);

            builder.AppendLine(subtitle);
            builder.AppendLine("----------------------------------------");

            if(SelectedTodoTask.CheckListItems != null &&
               SelectedTodoTask.CheckListItems.Count > 0)
            {
                builder.AppendLine("----------------------------------------");

                foreach(var checkListTask in SelectedTodoTask.CheckListItems)
                {
                    builder.AppendLine(checkListTask.Title);

                    if(checkListTask.IsCompleted) builder.Append(" ✔️");
                }

                builder.AppendLine("----------------------------------------");
            }

            if(SelectedTodoTask.HasBody)
               builder.AppendLine(SelectedTodoTask.BodyContent);

            _share.RequestShare(builder.ToString(), Widget, title, subtitle);
        }

        #endregion

        #region Handlers

        async partial void OnTodoListChanged(TodoListViewModel value)
        {
            if(value == null) return;
            if(string.IsNullOrEmpty(value.Id)) return;

            IsLoading = true;

            TodoTasks = await GetTodoTasksAsync(value, true, TasksSortBy);

            IsLoading = false;
        }

        private void OnMSSignedIn(object sender, EventArgs e)
        {
            IsSignedIn = _msGraph.IsSignedIn;

            if(TodoList == null || TodoTasks == null)
               RefreshCommand.Execute(default);
        }

        private void OnMSSignOut(object sender, EventArgs e)
        {
            IsSignedIn = _msGraph.IsSignedIn;

            TodoList = null;
            TodoTasks = null;
            IconVisibility = Visibility.Collapsed;
        }

        private void OnNetworkStateChanged(object sender, bool e)
        {
            if(e && _msGraph.IsSignedIn)
               RefreshCommand.Execute(default);
        }

        private void OnSettingsChanged(object sender, string e)
        {
            switch(e)
            {
                case nameof(IsUpdateEnabled):

                    if(IsUpdateEnabled) _updateTimer?.Start();
                    else _updateTimer?.Stop();

                    break;
                case nameof(UpdateInterval):

                    if(_updateTimer != null)
                       _updateTimer.Interval = TimeSpan.FromMinutes(UpdateInterval);

                    break;
            }
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            RefreshCommand.Execute(default);
        }

        #endregion
    }
}
