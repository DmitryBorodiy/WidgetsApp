using BetterWidgets.Abstractions;
using BetterWidgets.Behaviours.Validators;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Exceptions;
using BetterWidgets.Extensions;
using BetterWidgets.Model.DTO;
using BetterWidgets.Model.Tasks;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Widgets;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Components
{
    public partial class TodoTaskViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly ITodoManager<TodoWidget> _todo;
        private readonly Settings<TodoWidget> _settings;
        private readonly IMediaPlayerService _player;
        #endregion

        public TodoTaskViewModel(TodoTask task, string listId, Widget widget, TodoViewModel todoViewModel)
        {
            _player = App.Services?.GetService<IMediaPlayerService>();
            _logger = App.Services?.GetRequiredService<ILogger<TodoTaskViewModel>>();
            _todo = App.Services?.GetKeyedService<ITodoManager<TodoWidget>>(nameof(MSTodoManager<IWidget>));
            _settings = App.Services?.GetRequiredService<Settings<TodoWidget>>();

            _settings.ValueChanged += OnSettingsChanged;

            Id = task.Id;
            ListId = listId;
            SubTaskId = task.SubTaskId;
            Widget = widget;
            MainVM = todoViewModel;
            TodoTask = task;

            Title = task.Title;
            IsCompleted = task.IsCompleted;
            IsImportant = task.IsImportant;
            IsReminderOn = task.IsReminderOn;
            CurrentTask = task;
            StartDate = task.StartDateTime;
            DueDate = task.DueDateTime;
            ReminderDate = task.ReminderDateTime;
            IsSubTask = task.IsSubTask;
            BodyContent = task.Body;

            if(task.DueDateTime != default &&
               task.DueDateTime != DateTime.MinValue)
               Subtitle = task.DueDateTime?.ToDateTimeLabel(DateFormatValidator.Default, TimeFormatValidator.Default, CultureInfo.CurrentCulture);

            SubtitleVisibility = string.IsNullOrEmpty(Subtitle) ? Visibility.Collapsed : Visibility.Visible;

            SetListItems(task.CheckListItems);
        }

        #region Props

        public string Id { get; set; }
        public string ListId { get; set; }
        public string SubTaskId { get; set; }
        public TodoTask TodoTask { get; set; }

        private TodoTask CurrentTask { get; set; }
        private Widget Widget { get; set; }
        private TodoViewModel MainVM { get; set; }

        [ObservableProperty]
        public string title = string.Empty;

        [ObservableProperty]
        public string subtitle = string.Empty;

        [ObservableProperty]
        public DateTime? dueDate;

        [ObservableProperty]
        public DateTime? startDate;

        [ObservableProperty]
        public DateTime? reminderDate;

        [ObservableProperty]
        public bool isReminderOn;

        [ObservableProperty]
        public ObservableCollection<TodoTaskViewModel> checkListItems = new ObservableCollection<TodoTaskViewModel>();

        [ObservableProperty]
        public bool isCompleted;
        
        [ObservableProperty]
        public bool isImportant;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImportanceCommandVisibility))]
        public bool isSubTask;

        [ObservableProperty]
        public string bodyContent = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasBody))]
        public Visibility subtitleVisibility = Visibility.Visible;

        public Visibility ImportanceCommandVisibility => IsSubTask ? Visibility.Collapsed : Visibility.Visible;

        public bool HasBody => !string.IsNullOrEmpty(BodyContent);

        #region Settings

        public bool IsTaskCheckSoundEnabled => _settings.GetSetting(nameof(IsTaskCheckSoundEnabled), true);
        public bool EnableTextWrap => _settings.GetSetting(nameof(EnableTextWrap), false);

        #endregion

        #endregion

        #region Utils

        private void SetListItems(List<TodoTask> tasks)
        {
            if(tasks == null) return;
            if(tasks.Count == 0) return;

            CheckListItems = new ObservableCollection<TodoTaskViewModel>(
                tasks.Select(i => new TodoTaskViewModel(i, ListId, Widget, MainVM))
            );
        }

        private TodoTaskRequest CreateRequest(bool isCompleted, bool isImportant) => new TodoTaskRequest()
        {
            ListId = ListId,
            IsCompleted = isCompleted,
            IsImportant = isImportant,
            Title = Title,
            Id = CurrentTask.Id,
            StartDate = CurrentTask.StartDateTime != default ?
                                CurrentTask.StartDateTime : null,
            DueDate = CurrentTask.DueDateTime != default ?
                              CurrentTask.DueDateTime : null,
            ReminderDate = CurrentTask.ReminderDateTime != default ?
                                   CurrentTask.ReminderDateTime : null
        };

        private ChecklistRequest CreateChecklistRequest(bool isCompleted) => new ChecklistRequest()
        {
            ListId = ListId,
            TaskId = Id,
            Id = SubTaskId,
            IsChecked = isCompleted,
            Title = Title
        };

        private async void PlaySound(string soundName)
        {
            if(!IsTaskCheckSoundEnabled) return;
            if(!Uri.IsWellFormedUriString(soundName, UriKind.RelativeOrAbsolute)) return;

            await _player?.PlayAsync(new Uri(soundName));
        }

        #endregion

        #region Command

        [RelayCommand]
        private void SwitchImportance() => IsImportant = !IsImportant;

        #endregion

        #region Handlers

        partial void OnSubtitleChanged(string value)
        {
            SubtitleVisibility = string.IsNullOrEmpty(value) ? 
                                 Visibility.Collapsed : Visibility.Visible;
        }

        async partial void OnIsCompletedChanged(bool value)
        {
            try
            {
                if(CurrentTask == null) return;
                if(CurrentTask.IsCompleted == value) return;

                TodoTaskRequest todoRequest = null;
                ChecklistRequest checklistRequest = null;

                CurrentTask.IsCompleted = value;

                if(!IsSubTask)
                   todoRequest = CreateRequest(value, CurrentTask.IsImportant);
                else
                   checklistRequest = CreateChecklistRequest(value);

                var result = IsSubTask ? 
                             await _todo.UpdateChecklistItemAsync(checklistRequest) :
                             await _todo?.UpdateAsync(todoRequest);

                if(result.ex != null) throw result.ex;

                PlaySound(FileNames.taskcheckSound);
                MainVM?.RefreshCommand.Execute(default);
            }
            catch(NetworkUnavailableException)
            {
                CurrentTask.IsCompleted = !value;
                IsCompleted = !value;

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

        async partial void OnIsImportantChanged(bool value)
        {
            try
            {
                if(IsSubTask) return;
                if(CurrentTask == null) return;
                if(CurrentTask.IsImportant == value) return;

                CurrentTask.IsImportant = value;

                var request = CreateRequest(CurrentTask.IsCompleted, value);
                var result = await _todo?.UpdateAsync(request);

                if(result.ex != null) throw result.ex;
            }
            catch(NetworkUnavailableException)
            {
                IsImportant = !value;
                CurrentTask.IsImportant = !value;

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

        private void OnSettingsChanged(object sender, string e)
        {
            switch(e)
            {
                case nameof(EnableTextWrap):
                    OnPropertyChanged(nameof(EnableTextWrap));
                    break;
            }
        }

        #endregion
    }
}
