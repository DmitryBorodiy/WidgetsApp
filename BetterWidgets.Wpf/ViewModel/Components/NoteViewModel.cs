using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Controls;
using BetterWidgets.Exceptions;
using BetterWidgets.Extensions;
using BetterWidgets.Extensions.Xaml;
using BetterWidgets.Model;
using BetterWidgets.Model.Notes;
using BetterWidgets.Properties;
using BetterWidgets.Services;
using BetterWidgets.Views.Dialogs;
using BetterWidgets.Widgets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Wpf.Ui.Controls;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace BetterWidgets.ViewModel.Components
{
    public partial class NoteViewModel : ObservableObject
    {
        #region Consts
        private readonly string _dateFormat = "d/M/yyyy";
        #endregion

        #region Services
        private readonly ILogger _logger;
        private readonly IStickyNotes<StickyNotesWidget> _notes;
        private readonly Settings<StickyNotesWidget> _settings;
        private readonly CoreWidget _coreWidget;
        private readonly IMediaPlayerService _player;
        private readonly IMSGraphService _graph;
        #endregion

        public NoteViewModel()
        {
            _logger = App.Services?.GetRequiredService<ILogger<NoteViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<StickyNotesWidget>>();
            _notes = App.Services?.GetRequiredService<IStickyNotes<StickyNotesWidget>>();
            _graph = App.Services?.GetRequiredService<IMSGraphService>();
            _player = App.Services?.GetService<IMediaPlayerService>();
            _coreWidget = App.Services?.GetService<CoreWidget>();
        }

        public NoteViewModel(StickyNote stickyNote, bool isSeparateCoreView = false, Widget widget = null)
        {
            _logger = App.Services?.GetRequiredService<ILogger<NoteViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<StickyNotesWidget>>();
            _notes = App.Services?.GetRequiredService<IStickyNotes<StickyNotesWidget>>();
            _graph = App.Services?.GetRequiredService<IMSGraphService>();
            _player = App.Services?.GetService<IMediaPlayerService>();
            _coreWidget = App.Services?.GetService<CoreWidget>();

            if(_settings != null) _settings.ValueChanged += OnSettingsChanged;

            if(_graph != null)
            {
                _graph.SignedIn += OnSignedInChanged;
                _graph.SignedOut += OnSignedInChanged;
            }

            Id = stickyNote.Id;
            Title = stickyNote.Title;
            Content = stickyNote.Content;
            IsPinned = stickyNote.IsPinned;
            ClientId = stickyNote.ClientId;
            ContentPreview = stickyNote.PreviewContent;
            IsSeparateCoreView = isSeparateCoreView;
            WidgetView = widget;
            CurrentNote = stickyNote;

            DateLabel = stickyNote.LastEditedDateTime?.ToString(_dateFormat, CultureInfo.CurrentCulture) ?? null;
            ColorBrush = new SolidColorBrush(GetNoteColor(Id));

            if(!string.IsNullOrEmpty(Content))
               Document = Content.GetDocumentFromHtml(DefaultFontFamily, DefaultFontSize);
        }

        public NoteViewModel(NoteViewModel viewModel, bool isSeparateCoreView = false, Widget widgetView = null)
        {
            _logger = App.Services?.GetRequiredService<ILogger<NoteViewModel>>();
            _settings = App.Services?.GetRequiredService<Settings<StickyNotesWidget>>();
            _notes = App.Services?.GetRequiredService<IStickyNotes<StickyNotesWidget>>();
            _graph = App.Services?.GetRequiredService<IMSGraphService>();
            _player = App.Services?.GetRequiredService<IMediaPlayerService>();
            _coreWidget = App.Services?.GetService<CoreWidget>();

            if(_graph != null)
            {
                _graph.SignedIn += OnSignedInChanged;
                _graph.SignedOut += OnSignedInChanged;
            }

            WidgetView = widgetView;
            CurrentNote = viewModel.CurrentNote;

            Id = viewModel.Id;
            Title = viewModel.Title;
            Content = viewModel.Content;
            IsPinned = viewModel.IsPinned;
            ClientId = viewModel.ClientId;
            ContentPreview = viewModel.ContentPreview;
            IsSeparateCoreView = isSeparateCoreView;
            DateLabel = viewModel.DateLabel;
            ColorBrush = viewModel.ColorBrush;
            Document = viewModel.Document;
        }

        #region Props

        public string Id { get; set; }
        public Guid ClientId { get; set; }

        public Widget WidgetView { get; private set; }
        private StickyNote CurrentNote { get; set; }

        [ObservableProperty]
        public bool isPinned;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReadOnly))]
        [NotifyPropertyChangedFor(nameof(HasTitle))]
        public bool isEdit;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RootMargin))]
        public bool isSeparateCoreView;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasTitle))]
        public string title;

        [ObservableProperty]
        public string content;

        [ObservableProperty]
        public string contentPreview;

        [ObservableProperty]
        public FlowDocument document;

        [ObservableProperty]
        public SolidColorBrush colorBrush;

        [ObservableProperty]
        public string dateLabel;

        [ObservableProperty]
        public bool isColorPickerOpen;

        [ObservableProperty]
        public bool isFontPickerOpen;

        [ObservableProperty]
        public SolidColorBrush pickedColor;

        [ObservableProperty]
        public FontFamily selectedFont;

        public bool IsSignedIn => _graph?.IsSignedIn ?? false;

        public Thickness RootMargin => IsSeparateCoreView ? new Thickness(13, 15, 13, 15) : new Thickness(0);
        public bool IsTitleEmpty => string.IsNullOrEmpty(Title);
        public bool IsReadOnly => !IsEdit;
        public bool HasId => !string.IsNullOrEmpty(Id);
        public bool HasTitle => !string.IsNullOrEmpty(Title) &&
                                IsReadOnly;

        #region Settings

        public bool IsSoundsEnabled => _settings.GetSetting(nameof(IsSoundsEnabled), true);
        public FontFamily DefaultFontFamily => GetDefaultFontFamily();
        public int DefaultFontSize => _settings.GetSetting(nameof(DefaultFontSize), 15);

        #endregion

        #endregion

        #region Commands

        [RelayCommand]
        private void ClearColor() => ColorBrush = null;

        [RelayCommand]
        private void PickColor()
        {
            var picker = new ColorPickerDialog();
            picker.SelectedColor = ColorBrush?.Color ?? Colors.LightYellow;

            picker.ShowDialog();

            if(picker.SelectedColor != Colors.Transparent)
               ColorBrush = new SolidColorBrush(picker.SelectedColor);
        }

        [RelayCommand]
        private void CopyAll()
        {
            var textRange = new TextRange(Document.ContentStart, Document.ContentEnd);

            Clipboard.SetText(textRange.Text);
        }

        [RelayCommand]
        private async Task PinNote()
        {
            try
            {
                var pinResult = await _notes.PinNoteAsync(CurrentNote);

                if(pinResult.ex != null) throw pinResult.ex;

                IsPinned = pinResult.success;
                ClientId = CurrentNote.ClientId;

                var view = _coreWidget?.CreateNewView(CurrentNote.ClientId, nameof(StickyNotesWidget));

                if(!view.HasValue) return;
                if(view.Value.ex != null) throw view.Value.ex;

                if(view.Value.createdView != null)
                {
                    var widget = view.Value.createdView;

                    var vm = new NoteViewModel(this, true, widget);
                    var contentView = new NoteView(vm);

                    widget.MinHeight = 160;
                    widget.MinWidth = 160;
                    widget.Content = contentView;
                    widget.IsTitleBarEnabled = true;
                    widget.TitleBar.CanPin = false;
                    widget.IsHideTitleBar = true;

                    widget.UnpinCommand = NoteUnpinnedCommand;
                    widget.UnpinCommandParameter = CurrentNote.ClientId;

                    widget.Show();
                    widget.Activate();

                    if(IsSoundsEnabled)
                       await _player.PlayAsync(new Uri(FileNames.noteSound));
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private async Task OnNoteUnpinned(Guid clientId)
        {
            try
            {
                if(clientId == default) return;

                var unpinResult = await _notes.UnpinNoteAsync(clientId);

                if(unpinResult.ex != null) throw unpinResult.ex;

                IsPinned = false;

                var view = _coreWidget.GetViewById(clientId);

                view?.Hide();

                if(IsSoundsEnabled)
                   await _player.PlayAsync(new Uri(FileNames.noteSound));
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void RequestDelete()
        {
            if(WidgetView == null) return;

            var dialogParams = new WidgetContentDialogParams()
            {
                Title = Resources.Resources.DeleteNote,
                Content = Resources.Resources.DeleteNoteSubtitle,
                PrimaryButtonContent = Resources.Resources.Yes,
                SecondaryButtonContent = Resources.Resources.No,
                PrimaryButtonCommand = DeleteCommand,
                PrimaryButtonParameter = ClientId
            };

            if(WidgetView.IsWidgetDialogOpen) 
               WidgetView.IsWidgetDialogOpen = false;

            WidgetView.ShowContentDialog(dialogParams);
        }

        [RelayCommand]
        private async Task DeleteAsync(Guid clientId)
        {
            try
            {
                if(string.IsNullOrEmpty(Id)) return;

                var view = _coreWidget.GetViewById(clientId);

                if(view != null)
                {
                    await _notes.UnpinNoteAsync(clientId);
                    view.Hide();
                }

                await _notes.DeleteStickyNoteAsync(Id);

                if(CurrentNote != null)
                   CurrentNote.ClearColorCache();

                if(IsSoundsEnabled)
                   await _player.PlayAsync(new Uri(FileNames.noteSound));
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);
            }
        }

        [RelayCommand]
        private void ToggleEdit() => IsEdit = !IsEdit;

        [RelayCommand]
        private void ShowPickColor() => IsColorPickerOpen = true;

        [RelayCommand]
        private void ShowFontPicker() => IsFontPickerOpen = !IsFontPickerOpen;

        [RelayCommand]
        private void PickFont(RichTextBox editor)
        {
            if(editor == null) return;
            if(SelectedFont == null) return;
            if(!IsFontPickerOpen) return;

            editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, SelectedFont);

            SelectedFont = null;
            IsFontPickerOpen = false;
        }

        [RelayCommand]
        private async Task SaveAsync(RichTextBox editor)
        {
            try
            {
                var noteRequest = new StickyNote()
                {
                    Id = Id,
                    ClientId = ClientId,
                    Title = Title,
                    IsPinned = IsPinned,
                    Color = ColorBrush.Color,
                    Content = editor.Document.GetHtmlFromFlowDocument(),
                    CreatedDate = CurrentNote.CreatedDate
                };

                var result = HasId ?
                             await _notes.UpdateStickyNoteAsync(noteRequest) :
                             await _notes.CreateStickyNoteAsync(noteRequest);

                if(result.ex != null) throw result.ex;

                IsEdit = false;
            }
            catch(NetworkUnavailableException)
            {
                WidgetView?.HideContentDialog();
                WidgetView?.ShowNotify
                (
                    Resources.Resources.NoNetworkSubtitle,
                    Resources.Resources.NoNetworkTitle,
                    true,
                    InfoBarSeverity.Error,
                    true,
                    TimeSpan.FromSeconds(8)
                );
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                WidgetView?.HideContentDialog();
                WidgetView?.ShowNotify
                (
                    Resources.Resources.CannotSaveChangesSubtitle + ex.Message,
                    Resources.Resources.CannotSaveChanges,
                    true,
                    InfoBarSeverity.Error,
                    true,
                    TimeSpan.FromSeconds(10)
                );
            }
        }

        [RelayCommand]
        private void OnNoteTyping()
        {
            OnPropertyChanged(nameof(IsTitleEmpty));
        }

        [RelayCommand]
        private async Task SignInAsync()
        {
            if(_graph != null)
               await _graph.SignInAsync();
        }

        #endregion

        #region Utils

        private Color GetNoteColor(string id)
        {
            if(string.IsNullOrEmpty(id)) return Colors.LightYellow;

            string hex = _settings?.GetValue<string>($"{Id}:color");

            if(string.IsNullOrEmpty(hex)) return Colors.LightYellow;

            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private FontFamily GetDefaultFontFamily()
        {
            if(_settings == null) return null;

            string fontFamily = _settings.GetSetting(nameof(DefaultFontFamily), "Georgia");

            return new FontFamily(fontFamily);
        }

        #endregion

        #region Handlers

        partial void OnColorBrushChanged(SolidColorBrush value)
        {
            if(value == null) return;
            if(value.Color == default) return;
            if(string.IsNullOrEmpty(Id)) return;

            _settings?.SetValue($"{Id}:color", value.Color.ToHex());
        }

        partial void OnPickedColorChanged(SolidColorBrush value)
        {
            if(value == null) return;

            ColorBrush = value;

            if(WidgetView != null)
               WidgetView.WidgetDialogBackground = value;
        }

        private void OnSignedInChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsSignedIn));
        }

        private void OnSettingsChanged(object sender, string e)
        {
            switch(e)
            {
                case nameof(DefaultFontSize):
                    OnPropertyChanged(nameof(DefaultFontSize));
                    break;
                case nameof(DefaultFontFamily):
                    OnPropertyChanged(nameof(DefaultFontFamily));
                    break;
            }
        }

        #endregion
    }
}
