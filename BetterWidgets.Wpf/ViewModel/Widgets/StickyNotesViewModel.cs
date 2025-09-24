using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Enums;
using BetterWidgets.Exceptions;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Model.Notes;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.ViewModel.Components;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace BetterWidgets.ViewModel.Widgets
{
    public partial class StickyNotesViewModel : ObservableObject
    {
        #region Services
        private readonly ILogger _logger;
        private readonly Settings<StickyNotesWidget> _settings;
        private readonly IStickyNotes<StickyNotesWidget> _notes;
        private readonly IPermissionManager<StickyNotesWidget> _permissions;
        private readonly IMediaPlayerService _player;
        private readonly CoreWidget _coreWidget;
        private readonly IMSGraphService _graph;
        private readonly IShareService _share;

        private DispatcherTimer _updateTimer;
        #endregion

        public StickyNotesViewModel()
        {
            _logger = App.Services?.GetRequiredService<ILogger<StickyNotesViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<StickyNotesWidget>>();
            _notes = App.Services?.GetRequiredService<IStickyNotes<StickyNotesWidget>>();
            _permissions = App.Services?.GetRequiredService<IPermissionManager<StickyNotesWidget>>();
            _player = App.Services?.GetService<IMediaPlayerService>();
            _coreWidget = App.Services?.GetService<CoreWidget>();
            _graph = App.Services?.GetService<IMSGraphService>();
            _share = App.Services?.GetService<IShareService>();

            if(_graph != null)
            {
                IsSignedIn = _graph.IsSignedIn;

                _graph.SignedIn += OnGraphSignedIn;
                _graph.SignedOut += OnGraphSignedOut;
            }
            if(_settings != null) _settings.ValueChanged += OnSettingsChanged;
            if(_notes != null)
            {
                _notes.NoteUpdated += OnNoteUpdated;
                _notes.NoteCreated += OnNoteCreated;
                _notes.NoteDeleted += OnNoteDeleted;
            }
        }

        #region Props

        private Widget Widget { get; set; }

        [ObservableProperty]
        public bool isLoading;

        [ObservableProperty]
        public Thickness rootMargin = new Thickness(13, 42, 13, 0);

        public Brush Background => SelectedNote?.ColorBrush ??
                                   (SolidColorBrush)Application.Current.Resources["ContentDialogBackground"];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsEmpty))]
        public ObservableCollection<NoteViewModel> notes;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNoteSelected))]
        public NoteViewModel selectedNote;

        [ObservableProperty]
        public bool isSignedIn;

        #region UINoteProps

        [ObservableProperty]
        public bool isNoteViewOpen;

        #endregion

        public bool IsNoteSelected => SelectedNote != null;
        public bool IsEmpty => (Notes == null || Notes.Count == 0) && IsSignedIn;

        public bool IsUpdateEnabled => _settings.GetSetting(nameof(IsUpdateEnabled), false);
        public int UpdateInterval => _settings.GetSetting(nameof(UpdateInterval), 35);

        #region Settings

        public bool IsSoundsEnabled => _settings.GetSetting(nameof(IsSoundsEnabled), true);
        public bool IsNoteAutosaveEnabled => _settings.GetSetting(nameof(IsNoteAutosaveEnabled), false);

        #endregion

            #endregion

            #region Utils

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

        private async Task<ObservableCollection<NoteViewModel>> GetNotesAsync(bool forceRefresh = false)
        {
            try
            {
                if(_notes == null) return null;

                IsLoading = true;

                var notes = await _notes.GetCachedAsync(
                    FileNames.stickyNotes,
                    _notes.GetAllNotesAsync,
                    forceRefresh && NetworkHelper.IsConnected && _graph.Client != null && _graph.IsSignedIn);

                if(notes.ex != null) throw notes.ex;
                if(notes.data == null) return null;

                var noteViews = notes.data.Select(n => new NoteViewModel(n, false, Widget));
                IsLoading = false;

                return new ObservableCollection<NoteViewModel>(noteViews);
            }
            catch(UnauthorizedAccessException)
            {
                IsLoading = false;

                Widget?.ShowNotify(
                    message: Resources.Resources.NotesPermissionNeededMessage,
                    title: Resources.Resources.NotesPermissionNeeded,
                    severity: InfoBarSeverity.Warning);

                return null;
            }
            catch(NetworkUnavailableException)
            {
                IsLoading = false;
                Widget?.ShowNotify
                (
                    message: Resources.Resources.NoNetworkSubtitle,
                    severity: InfoBarSeverity.Warning,
                    isClosable: true
                );

                return null;
            }
            catch(Exception ex)
            {
                IsLoading = false;
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return null;
            }
        }

        private async Task RestorePinnedNotesAsync()
        {
            try
            {
                if(_coreWidget.HasViewsInGroupName(nameof(StickyNotesWidget))) return;

                var pinnedNotes = await _notes.GetPinnedNotesAsync();

                if(pinnedNotes.ex != null) throw pinnedNotes.ex;

                foreach(var note in pinnedNotes.notes)
                    OpenNoteWidgetView(note);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        private void OpenNoteWidgetView(StickyNote note)
        {
            var view = _coreWidget?.CreateNewView(note.ClientId, nameof(StickyNotesWidget));

            if(!view.HasValue) return;
            if(view.Value.ex != null) throw view.Value.ex;

            if(view.Value.createdView != null)
            {
                var widget = view.Value.createdView;

                var vm = new NoteViewModel(note, true, widget);
                var contentView = new NoteView(vm);

                widget.MinHeight = 160;
                widget.MinWidth = 160;
                widget.Content = contentView;
                widget.IsTitleBarEnabled = true;
                widget.TitleBar.CanPin = false;
                widget.IsHideTitleBar = true;

                widget.UnpinCommand = vm.NoteUnpinnedCommand;
                widget.UnpinCommandParameter = note.ClientId;

                widget.Show();
                widget.Activate();
            }
        }

        private void OpenNoteDialog(NoteViewModel note, bool isEdit = false)
        {
            if(note == null) throw new ArgumentNullException(nameof(note));

            note.IsEdit = isEdit;

            var noteView = new NoteView(note)
            {
                Width = 280,
                Height = 280
            };

            var dialogParams = new WidgetContentDialogParams()
            {
                Content = noteView,
                Background = note.ColorBrush,
                TitleBarVisibility = Visibility.Collapsed,
                PrimaryButtonVisibility = Visibility.Collapsed,
                SecondaryButtonVisibility = Visibility.Collapsed
            };

            Widget.ShowContentDialog(dialogParams);
            PlaySound(FileNames.noteSound);
        }

        private async void PlaySound(string path)
        {
            if(IsSoundsEnabled)
               await _player.PlayAsync(new Uri(path));
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

        private bool CheckPermission()
            => _permissions.TryCheckPermissionState(new Permission(Scopes.Notes, PermissionLevel.HighLevel)) == PermissionState.Allowed;

        #endregion

        #region Commands

        [RelayCommand]
        private async Task OnAppearedAsync(Widget widget)
        {
            Widget = widget;

            if(Widget != null)
            {
                if(CheckPermission())
                {
                    RefreshCommand.Execute(widget);
                    Widget.HideNotify();
                }
                else
                {
                    Widget?.ShowNotify(
                        message: Resources.Resources.NotesPermissionNeededMessage,
                        title: Resources.Resources.NotesPermissionNeeded,
                        severity: InfoBarSeverity.Warning);
                }

                if(Widget.IsPreview) RootMargin = new Thickness(15, 15, 15, 0);
                else
                {
                    Widget.NetworkStateChanged += OnNetworkStateChanged;
                    Widget.ContentDialogClosed += OnContentDialogClosing;

                    AttachUpdateTimer();
                    await RestorePinnedNotesAsync();
                }
            }
        }

        [RelayCommand]
        private async Task OnPinnedAsync()
        {
            if(!CheckPermission()) await RequestPermission();

            AttachUpdateTimer();

            RefreshCommand.Execute(Widget);
        }

        [RelayCommand]
        private void OnUnpinned()
        {
            try
            {
                Notes = null;

                if(Widget != null)
                   Widget.NetworkStateChanged -= OnNetworkStateChanged;

                DetachUpdateTimer();
            }
            catch { }
        }

        [RelayCommand]
        private void OnExecutionStateChanged(WidgetState state)
        {
            if(state == WidgetState.Activated) _updateTimer?.Start();
            else _updateTimer?.Stop();
        }

        [RelayCommand]
        private void OnResize(Size size)
        {
            var widgetSize = WidgetSize.GetSize(size);

            SetLayoutState(widgetSize);
        }

        [RelayCommand]
        private async Task RefreshAsync(Widget widget)
        {
            if(!CheckPermission()) return;

            Notes = await GetNotesAsync(!widget?.IsPreview ?? true);
        }

        [RelayCommand]
        private async Task RequestPermission()
        {
            var permission = new Permission(Scopes.Notes, PermissionLevel.HighLevel);
            var access = await _permissions.RequestAccessAsync(permission);

            if(access == PermissionState.Allowed)
            { 
                RefreshCommand.Execute(Widget);
                Widget?.HideNotify();
            }
            else
                Widget?.ShowNotify(
                    message: Resources.Resources.NotesPermissionNeededMessage,
                    title: Resources.Resources.NotesPermissionNeeded,
                    severity: InfoBarSeverity.Warning);
        }

        [RelayCommand]
        private void Share()
        {
            if(!IsNoteSelected) return;

            var textRange = new TextRange(SelectedNote.Document.ContentStart, 
                                          SelectedNote.Document.ContentEnd);

            if(string.IsNullOrEmpty(textRange.Text)) return;

            _share.RequestShare(textRange.Text, Widget, SelectedNote.Title, textRange.Text);
        }

        #endregion

        #region Handlers

        private void OnNetworkStateChanged(object sender, bool e)
        {
            if(!e) return;
            //if(_graph.Client == null && _graph.IsSignedIn)
               //await _graph.SignInAsync();
            if(_graph.IsSignedIn)
               RefreshCommand.Execute(Widget);
        }

        private void OnGraphSignedIn(object sender, EventArgs e)
        {
            IsSignedIn = _graph.IsSignedIn;

            RefreshCommand.Execute(Widget);
        }

        private void OnGraphSignedOut(object sender, EventArgs e)
        {
            IsSignedIn = false;
        }

        [RelayCommand]
        private void OnNotesCollectionMouseDown(RoutedEventArgs e)
        {
            if(Widget == null) return;
            if(SelectedNote == null) return;
            if(Widget.IsWidgetDialogOpen) return;

            OpenNoteDialog(SelectedNote);
        }

        [RelayCommand]
        private void OnNotesCollectionKeyDown(KeyEventArgs e)
        {
            if(e.Key == Key.Enter || e.Key == Key.Space)
               NotesCollectionMouseDownCommand.Execute(e);
        }

        [RelayCommand]
        private void CreateNote()
        {
            if(Widget == null) return;

            var note = new StickyNote() { Content = "<html><body></body></html>" };
            var noteView = new NoteViewModel(note, widget: Widget);

            OpenNoteDialog(noteView, true);
        }

        [RelayCommand]
        private void LaunchSettings()
        {
            if(Widget != null)
               ShellHelper.LaunchSettingsById(Widget.Id);
        }

        private void OnNoteDeleted(object sender, string e)
        {
            if(Notes == null) return;
            if(string.IsNullOrEmpty(e)) return;

            var noteView = Notes.FirstOrDefault(n => n.Id == e);

            if(noteView != null) Notes.Remove(noteView);
        }

        private void OnNoteUpdated(object sender, StickyNote e)
        {
            RefreshCommand.Execute(Widget);
        }

        private void OnNoteCreated(object sender, StickyNote e)
        {
            if(e == null) return;
            if(Notes == null) return;

            var vm = new NoteViewModel(e, false, Widget);
            Notes.Insert(0, vm);
        }

        private void OnSettingsChanged(object sender, string e)
        {
            switch(e)
            {
                case nameof(UpdateInterval):

                    if(_updateTimer != null)
                       _updateTimer.Interval = TimeSpan.FromMinutes(UpdateInterval);

                    break;
                case nameof(IsUpdateEnabled):

                    if(IsUpdateEnabled) _updateTimer?.Start();
                    else _updateTimer?.Stop();

                    break;
            }
        }

        private void OnUpdateTick(object sender, EventArgs e)
        {
            RefreshCommand.Execute(Widget);
        }

        private void OnContentDialogClosing(object sender, RoutedEventArgs e)
        {
            if(SelectedNote == null) return;

            if(SelectedNote.IsEdit && IsNoteAutosaveEnabled)
            {
                if(Widget.WidgetDialogContent is NoteView noteView)
                   SelectedNote.SaveCommand.Execute(noteView.UIRichTextBox);
            }
        }

        #endregion
    }
}
