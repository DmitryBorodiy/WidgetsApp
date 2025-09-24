using BetterWidgets.Abstractions;
using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Controls;
using BetterWidgets.Extensions;
using BetterWidgets.Model.DTO;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using BetterWidgets.Services;
using Wpf.Ui.Controls;
using BetterWidgets.Exceptions;
using Microsoft.Extensions.Logging;
using BetterWidgets.ViewModel.Widgets;
using System.Windows.Input;
using BetterWidgets.Extensions.Tasks;
using System.Windows;
using BetterWidgets.Enums;
using Windows.System;
using BetterWidgets.Properties;

namespace BetterWidgets.ViewModel.Dialogs
{
    public partial class EditTodoTaskViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly ITodoManager<TodoWidget> _todo;
        private readonly Settings<TodoWidget> _settings;
        #endregion

        public EditTodoTaskViewModel() : this(null, null, null) { }
        public EditTodoTaskViewModel(TodoTaskViewModel todoTask, Widget widget, TodoViewModel todoViewModel, FormMode mode = FormMode.Edit)
        {
            _logger = App.Services?.GetRequiredService<ILogger<EditTodoTaskViewModel>>();
            _todo = App.Services?.GetRequiredKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<TodoWidget>));
            _settings = App.Services?.GetRequiredService<Settings<TodoWidget>>();

            _settings.ValueChanged += OnSettingsChanged;

            TodoTask = todoTask;
            Widget = widget;
            FormMode = mode;
            TodoTasksVM = todoViewModel;
            TaskTitle = todoTask.Title;
            TaskDescription = todoTask.BodyContent;
            IsReminderOn = todoTask.IsReminderOn && todoTask.ReminderDate != default;
            SubTasksItems = todoTask.CheckListItems;
            DateTimePickerVM = new DateTimePickerViewModel();
            PrimaryButtonText = mode == FormMode.Edit ?
                                Resources.Resources.Save : Resources.Resources.AddTask;

            ReminderDateSubtitle = todoTask.IsReminderOn ?
                                   todoTask.ReminderDate?.ToDateTimeLabel(DateFormatValidator.Default, TimeFormatValidator.Default, CultureInfo.CurrentCulture, true) :
                                   Resources.Resources.SetReminder;

            DueDateSubtitle = todoTask.DueDate != default ?
                              todoTask.DueDate?.ToDateTimeLabel(DateFormatValidator.Default, TimeFormatValidator.Default, CultureInfo.CurrentCulture) :
                              Resources.Resources.SetDueDate;
        }

        private Widget Widget { get; set; }  
        private TodoViewModel TodoTasksVM { get; set; }

        [ObservableProperty]
        public TodoTaskViewModel _todoTask;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsCheckListEnabled))]
        public FormMode formMode = FormMode.Edit;

        [ObservableProperty]
        public bool isReminderOn;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSaveTask))]
        public string taskTitle = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasDueDate))]
        public string dueDateSubtitle;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasReminderDate))]
        public string reminderDateSubtitle;

        [ObservableProperty]
        public string primaryButtonText = string.Empty;

        [ObservableProperty]
        public TodoTaskViewModel selectedSubTask;

        [ObservableProperty]
        public ObservableCollection<TodoTaskViewModel> subTasksItems;

        [ObservableProperty]
        public string taskDescription;

        public bool IsCheckListEnabled => FormMode == FormMode.Edit;

        #region SubTaskDialogProps

        [ObservableProperty]
        public bool isSubTaskDialogOpen;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanSaveSubTask))]
        public string subTaskTitle = string.Empty;

        public bool CanSaveTask => !string.IsNullOrEmpty(TaskTitle);
        public bool CanSaveSubTask => !string.IsNullOrEmpty(SubTaskTitle);

        #endregion

        #region Settings

        public bool EnableTextWrap => _settings.GetSetting(nameof(EnableTextWrap), false);

        #endregion

        #region PickDateDialog

        [ObservableProperty]
        public bool isDateTimePickerOpen;

        [ObservableProperty]
        public DateTimePickerViewModel dateTimePickerVM;

        #endregion

        public bool HasDueDate => !string.IsNullOrEmpty(DueDateSubtitle);
        public bool HasReminderDate => !string.IsNullOrEmpty(ReminderDateSubtitle);

        [RelayCommand]
        private void ShowSubTaskDialog()
        {
            if(SelectedSubTask == null) return;

            IsSubTaskDialogOpen = !IsSubTaskDialogOpen;
            SubTaskTitle = SelectedSubTask.Title;
        }

        [RelayCommand]
        private void RequestCreateSubTask()
        {
            SelectedSubTask = null;

            SubTaskTitle = string.Empty;
            IsSubTaskDialogOpen = true;
        }

        [RelayCommand]
        private void CancelSubTaskDialog()
        {
            SelectedSubTask = null;

            SubTaskTitle = string.Empty;
            IsSubTaskDialogOpen = false;
        }

        [RelayCommand]
        private async Task DeleteSubTaskAsync()
        {
            try
            {
                if(SelectedSubTask == null) return;

                var deletionRequest = new ChecklistRequest()
                {
                    Id = SelectedSubTask.SubTaskId,
                    TaskId = TodoTask.Id,
                    ListId = TodoTask.ListId
                };

                var deletionResult = await _todo.DeleteChecklistItemAsync(deletionRequest);

                if(deletionResult.ex != null) throw deletionResult.ex;

                if(deletionResult.success)
                   SubTasksItems.Remove(SelectedSubTask);
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotDeleteTaskError + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private async Task PatchSubTaskAsync()
        {
            try
            {
                var checkListRequest = new ChecklistRequest()
                {
                    Title = SubTaskTitle,
                    IsChecked = SelectedSubTask?.IsCompleted ?? false,
                    Id = SelectedSubTask?.SubTaskId ?? null,
                    TaskId = TodoTask.Id,
                    ListId = TodoTask.ListId
                };

                var result = SelectedSubTask != null ?
                             await _todo.UpdateChecklistItemAsync(checkListRequest) :
                             await _todo.CreateChecklistItemAsync(checkListRequest);

                if(result.ex != null) throw result.ex;

                IsSubTaskDialogOpen = false;

                if(SelectedSubTask != null)
                   SelectedSubTask.Title = SubTaskTitle;
                else
                   SubTasksItems.Add(new TodoTaskViewModel(result.Item1, TodoTask.ListId, Widget, TodoTasksVM));
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void OnSubTaskInputKeyDown(KeyEventArgs e)
        {
            if(!CanSaveSubTask) return;

            if(e.Key == Key.Enter)
               PatchSubTaskCommand.Execute(default);
        }

        [RelayCommand]
        private void RequestsReminderDate()
        {
            DateTimePickerVM = new DateTimePickerViewModel()
            {
                SelectedDate = TodoTask.ReminderDate ?? DateTime.Now,
                SelectedTime = TodoTask.ReminderDate ?? DateTime.Now,
                OnPickCommand = PickReminderDateCommand,
                IsOpen = true
            };
        }

        [RelayCommand]
        private async Task PickReminderDate(DateTimePickerViewModel viewModel)
        {
            try
            {
                if(viewModel == null) return;

                TodoTask.TodoTask.IsReminderOn = true;
                TodoTask.TodoTask.ReminderDateTime = viewModel.SelectedDate.Date + viewModel.SelectedTime.TimeOfDay;
                IsReminderOn = true;

                ReminderDateSubtitle = TodoTask.TodoTask.ReminderDateTime?.ToDateTimeLabel(DateFormatValidator.Default, TimeFormatValidator.Default, CultureInfo.CurrentCulture, true);

                if(FormMode == FormMode.Create) return;

                var todoRequest = TodoTask.TodoTask.TodoTaskRequest(TodoTask.ListId);
                var updateResult = await _todo.UpdateAsync(todoRequest);

                if(updateResult.ex != null) throw updateResult.ex;
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotSetReminderDate + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private void RequestsDueDate()
        {
            DateTimePickerVM = new DateTimePickerViewModel()
            {
                SelectedDate = TodoTask.DueDate ?? DateTime.Now,
                IsTimePickerEnabled = false,
                OnPickCommand = PickDueDateCommand,
                IsOpen = true
            };
        }

        [RelayCommand]
        private async Task PickDueDateAsync(DateTimePickerViewModel viewModel)
        {
            try
            {
                if(viewModel == null) return;

                TodoTask.TodoTask.DueDateTime = viewModel.SelectedDate.Date;

                DueDateSubtitle = viewModel.SelectedDate != default ?
                                  viewModel.SelectedDate.ToDateTimeLabel(DateFormatValidator.Default, TimeFormatValidator.Default, CultureInfo.CurrentCulture) :
                                  Resources.Resources.SetDueDate;

                if(FormMode == FormMode.Create) return;

                var todoRequest = TodoTask.TodoTask.TodoTaskRequest(TodoTask.ListId);

                var updateResult = await _todo.UpdateAsync(todoRequest);

                if(updateResult.ex != null) throw updateResult.ex;
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotSetReminderDate + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private void OnTitleChanged(RoutedEventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(TaskTitle)) return;

                TodoTask.TodoTask.Title = TaskTitle;
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotChangeTaskTitle + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private async Task OnIsReminderOnChangedAsync()
        {
            try
            {
                TodoTask.TodoTask.IsReminderOn = !IsReminderOn;
                TodoTask.TodoTask.ReminderDateTime = !TodoTask.TodoTask.IsReminderOn ? 
                                                     null : DateTime.Now.AddHours(3);

                if(FormMode == FormMode.Create) return;

                var request = TodoTask.TodoTask.TodoTaskRequest(TodoTask.ListId);

                var updateResult = await _todo.UpdateAsync(request);

                if(updateResult.ex != null) throw updateResult.ex;

                if(updateResult.task != null)
                   TodoTasksVM.RefreshCommand.Execute(default);
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotChangeTaskTitle + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var request = TodoTask.TodoTask.TodoTaskRequest(TodoTask.ListId);

                if(FormMode == FormMode.Create)
                {   
                    var creationResult = await _todo.CreateAsync(request);

                    if(creationResult.ex != null) throw creationResult.ex;
                }
                else if(FormMode == FormMode.Edit)
                {
                    var updateResult = await _todo.UpdateAsync(request);

                    if(updateResult.ex != null) throw updateResult.ex;
                }

                TodoTasksVM?.RefreshCommand.Execute(default);
            }
            catch(NetworkUnavailableException)
            {
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(10),
                    hasDelay: true
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                Widget?.ShowNotify
                (
                    message: Resources.Resources.CannotChangeTaskTitle + ex.Message,
                    severity: InfoBarSeverity.Error,
                    isClosable: true,
                    delay: TimeSpan.FromSeconds(6),
                    hasDelay: true
                );
            }
        }

        [RelayCommand]
        private async Task OnUITitleKeyDownAsync(KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                TitleChangedCommand.Execute(e);

                await SaveAsync();
            }
        }

        partial void OnTaskTitleChanged(string value)
        {
            if(TodoTask?.TodoTask != null)
               TodoTask.TodoTask.Title = value;

            if(Widget != null)
               Widget.IsDialogPrimaryButtonEnabled = CanSaveTask;
        }

        partial void OnTaskDescriptionChanged(string value)
        {
            if(TodoTask?.TodoTask != null)
               TodoTask.TodoTask.Body = value;
        }

        private void OnSettingsChanged(object sender, string e)
        {
            switch(e)
            {
                case nameof(EnableTextWrap):
                    OnPropertyChanged(nameof(EnableTextWrap));
                    break;
            }
        }
    }
}
