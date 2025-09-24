using BetterWidgets.Abstractions;
using BetterWidgets.Consts;
using BetterWidgets.Enums;
using BetterWidgets.Extensions;
using BetterWidgets.Helpers;
using BetterWidgets.Model;
using BetterWidgets.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Windows.Networking.Connectivity;
using Wpf.Ui.Controls;
using Wpf.Ui.Designer;
using Button = Wpf.Ui.Controls.Button;
using TextBlock = Wpf.Ui.Controls.TextBlock;

namespace BetterWidgets.Controls
{
    public class Widget : Window, IWidget
    {
        #region Consts
        private const string UISecondaryWidgetDialogButton = nameof(UISecondaryWidgetDialogButton);
        private const string UIPrimaryWidgetDialogButton = nameof(UIPrimaryWidgetDialogButton);
        private const string UIDialogPopup = nameof(UIDialogPopup);
        private const string UIWidgetDialog = nameof(UIWidgetDialog);
        private const string UINotifyMessage = nameof(UINotifyMessage);
        private const string UINotifyMessageCloseCommand = nameof(UINotifyMessageCloseCommand);
        #endregion

        #region Services
        private readonly ILogger Logger;
        private readonly Settings Settings;
        #endregion

        public Widget() : this(default) { }
        public Widget(Guid? guid, bool isSecondary = false)
        {
            if(!DesignerHelper.IsInDesignMode)
            {
                Logger = App.Services?.GetRequiredService<ILogger<Widget>>();
                Settings = App.Services?.GetRequiredService<Settings>();
            }

            DefaultStyleKey = typeof(Widget);
            Id = guid ?? this.GetId();
            Permissions = this.GetPermissions();
            IsRequireNetwork = this.GetIsRequireNetwork();
            Title = this.GetWidgetTitle();
            Subtitle = this.GetWidgetSubtitle();
            Icon = this.GetWidgetIcon();
            DevMode = this.GetIsDevMode();
            IsSecondaryView = isSecondary;

            Style = (Style)Application.Current.Resources["DefaultWidgetStyle"];

            Loaded += Widget_Loaded;
            SizeChanged += Widget_SizeChanged;
            LocationChanged += Widget_LocationChanged;
            StateChanged += Widget_StateChanged;
            MouseEnter += Widget_MouseEnter;
            MouseLeave += Widget_MouseLeave;
        }

        #region PropRegistrations

        public static readonly DependencyProperty IsBlurEnabledProperty = DependencyProperty.Register(
            nameof(IsBlurEnabled),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false, OnIsBlurEnabledChanged));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CornerModeProperty = DependencyProperty.Register(
            nameof(CornerMode),
            typeof(WidgetCornerMode),
            typeof(Widget),
            new PropertyMetadata(WidgetCornerMode.Default, OnCornerModeChanged));

        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(Widget),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty BackdropOpacityProperty = DependencyProperty.Register(
            nameof(BackdropOpacity),
            typeof(double),
            typeof(Widget),
            new PropertyMetadata(0.5));

        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register(
            nameof(IsPreview),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyPropertyKey IsNetworkAvailablePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsNetworkAvailable), 
            typeof(bool), 
            typeof(Widget), 
            new PropertyMetadata(true));

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly DependencyProperty IsNetworkAvailableProperty = IsNetworkAvailablePropertyKey.DependencyProperty;

        internal static readonly DependencyProperty IsMessageBarOpenedProperty = DependencyProperty.Register(
            nameof(IsMessageBarOpened),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));

        internal static readonly DependencyProperty IsMessageBarClosableProperty = DependencyProperty.Register(
            nameof(IsMessageBarClosable),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(true));

        internal static readonly DependencyProperty MessageBarTextProperty = DependencyProperty.Register(
            nameof(MessageBarText),
            typeof(string),
            typeof(Widget),
            new PropertyMetadata(string.Empty));

        internal static readonly DependencyProperty MessageBarTitleProperty = DependencyProperty.Register(
            nameof(MessageBarTitle),
            typeof(string),
            typeof(Widget),
            new PropertyMetadata(string.Empty));

        internal static readonly DependencyProperty MessageBarSeverityProperty = DependencyProperty.Register(
            nameof(MessageBarSeverity),
            typeof(InfoBarSeverity),
            typeof(Widget),
            new PropertyMetadata(InfoBarSeverity.Informational));

        public static readonly DependencyProperty AppearedCommandProperty = DependencyProperty.Register(
            nameof(AppearedCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty UnpinedCommandProperty = DependencyProperty.Register(
            nameof(UnpinCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MouseEnterCommandProperty = DependencyProperty.Register(
            nameof(MouseEnterCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MouseLeaveCommandProperty = DependencyProperty.Register(
            nameof(MouseLeaveCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PinnedCommandProperty = DependencyProperty.Register(
            nameof(PinnedCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty SizeChangedCommandProperty = DependencyProperty.Register(
            nameof(SizeChangedCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ShareCommandProperty = DependencyProperty.Register(
            nameof(ShareCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty StateChangedCommandProperty = DependencyProperty.Register(
            nameof(StateChangedCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ShareCommandParameterProperty = DependencyProperty.Register(
            nameof(ShareCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MouseLeaveCommandParameterProperty = DependencyProperty.Register(
            nameof(MouseLeaveCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty MouseEnterCommandParameterProperty = DependencyProperty.Register(
            nameof(MouseEnterCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty AppearedCommandParameterProperty = DependencyProperty.Register(
            nameof(AppearedCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty UnpinedCommandParameterProperty = DependencyProperty.Register(
            nameof(UnpinCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PinnedCommandParameterProperty = DependencyProperty.Register(
            nameof(PinnedCommandParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(
            nameof(TitleBar),
            typeof(WidgetTitleBar),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsTitleBarEnabledProperty = DependencyProperty.Register(
            nameof(IsTitleBarEnabled),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(null));

        internal static readonly DependencyPropertyKey IsSecondaryViewPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsSecondaryView),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));
        internal static readonly DependencyProperty IsSecondaryViewProperty = IsSecondaryViewPropertyKey.DependencyProperty;

        public static readonly DependencyProperty WidgetDialogBackgroundProperty = DependencyProperty.Register(
            nameof(WidgetDialogBackground),
            typeof(Brush),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty WidgetDialogForegroundProperty = DependencyProperty.Register(
            nameof(WidgetDialogForeground),
            typeof(Brush),
            typeof(Widget),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register(
            nameof(IsPinned),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false, OnIsPinnedChanged));

        public static readonly DependencyProperty CanShareProperty = DependencyProperty.Register(
            nameof(CanShare),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsHideTitleBarProperty = DependencyProperty.Register(
            nameof(IsHideTitleBar),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false, OnIsHideTitleChanged));

        #region EventsRegistration

        public static readonly RoutedEvent ContentDialogOpenedEvent = EventManager.RegisterRoutedEvent(
            nameof(ContentDialogOpened),
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(Widget));

        public static readonly RoutedEvent ContentDialogClosedEvent = EventManager.RegisterRoutedEvent(
            nameof(ContentDialogClosed),
            RoutingStrategy.Tunnel,
            typeof(RoutedEventHandler),
            typeof(Widget));

        #endregion

        #region UIWidgetDialog

        internal static readonly DependencyPropertyKey IsWidgetDialogOpenPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsWidgetDialogOpen),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));
        public static readonly DependencyProperty IsWidgetDialogOpenProperty = IsWidgetDialogOpenPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetDialogContentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetDialogContent),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetDialogContentProperty = WidgetDialogContentPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetTitlePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetTitle),
            typeof(string),
            typeof(Widget),
            new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty WidgetTitleProperty = WidgetTitlePropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetPrimaryButtonContentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetPrimaryButtonContent),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetPrimaryButtonContentProperty = WidgetPrimaryButtonContentPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetSecondaryButtonContentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetSecondaryButtonContent),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetSecondaryButtonContentProperty = WidgetSecondaryButtonContentPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetPrimaryButtonParameterPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetPrimaryButtonParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetPrimaryButtonParameterProperty = WidgetPrimaryButtonParameterPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetSecondaryButtonParameterPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetSecondaryButtonParameter),
            typeof(object),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetSecondaryButtonParameterProperty = WidgetSecondaryButtonParameterPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetPrimaryButtonAppearancePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetPrimaryButtonAppearance),
            typeof(ControlAppearance),
            typeof(Widget),
            new PropertyMetadata(ControlAppearance.Primary));
        public static readonly DependencyProperty WidgetPrimaryButtonAppearanceProperty = WidgetPrimaryButtonAppearancePropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetSecondaryButtonAppearancePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetSecondaryButtonAppearance),
            typeof(ControlAppearance),
            typeof(Widget),
            new PropertyMetadata(ControlAppearance.Secondary));
        public static readonly DependencyProperty WidgetSecondaryButtonAppearanceProperty = WidgetSecondaryButtonAppearancePropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetDialogTitleVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetDialogTitleVisibility),
            typeof(Visibility),
            typeof(Widget),
            new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty WidgetDialogTitleVisibilityProperty = WidgetDialogTitleVisibilityPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetSecondaryButtonVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetSecondaryButtonVisibility),
            typeof(Visibility),
            typeof(Widget),
            new PropertyMetadata(Visibility.Collapsed));
        public static readonly DependencyProperty WidgetSecondaryButtonVisibilityProperty = WidgetSecondaryButtonVisibilityPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetPrimaryButtonVisibilityPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetPrimaryButtonVisibility),
            typeof(Visibility),
            typeof(Widget),
            new PropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty WidgetPrimaryButtonVisibilityProperty = WidgetPrimaryButtonVisibilityPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetPrimaryButtonCommandPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetPrimaryButtonCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetPrimaryButtonCommandProperty = WidgetPrimaryButtonCommandPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetSecondaryButtonCommandPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetSecondaryButtonCommand),
            typeof(ICommand),
            typeof(Widget),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WidgetSecondaryButtonCommandProperty = WidgetSecondaryButtonCommandPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey WidgetContentDialogStaysOpenPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(WidgetContentDialogStaysOpen),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(false));
        public static readonly DependencyProperty WidgetContentDialogStaysOpenProperty = WidgetContentDialogStaysOpenPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsDialogPrimaryButtonEnabledProperty = DependencyProperty.Register(
            nameof(IsDialogPrimaryButtonEnabled),
            typeof(bool),
            typeof(Widget),
            new PropertyMetadata(true));

        #endregion

        #endregion

        #region Fields
        private UIElement titleBarElement;
        private DispatcherTimer _messageBarDelay;
        private Border widgetDialog;

        private InfoBar NotifyMessage;
        private Button NotifyMessageCloseCommand;

        private Storyboard _hideDialog;
        private Storyboard _showDialog;
        #endregion

        #region Props

        public Guid Id { get; protected set; }
        public bool IsRequireNetwork { get; private set; }
        public string WidgetGroupName { get; set; }
        public string ProductId { get; private set; }
        public bool DevMode { get; private set; }

        public bool IsPreview
        {
            get => (bool)GetValue(IsPreviewProperty);
            set
            {
                SetValue(IsPreviewProperty, value);
                SetValue(IsTitleBarEnabledProperty, !value);
            }
        }

        public bool IsPinnedDesktop
        {
            get => Settings?.GetWidgetValue(Id, nameof(IsPinnedDesktop), false) ?? false;
            private set => Settings?.SetWidgetValue(Id, nameof(IsPinnedDesktop), value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public bool IsPinned
        {
            get => (bool)GetValue(IsPinnedProperty);
            set => SetValue(IsPinnedProperty, value);
        }

        public bool CanShare
        {
            get => (bool)GetValue(CanShareProperty);
            set => SetValue(CanShareProperty, value);
        }

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public double BackdropOpacity
        {
            get => (double)GetValue(BackdropOpacityProperty);
            set => SetValue(BackdropOpacityProperty, value);
        }

        public bool IsBlurEnabled
        {
            get => (bool)GetValue(IsBlurEnabledProperty);
            set => SetValue(IsBlurEnabledProperty, value);
        }

        public bool IsNetworkAvailable
        {
            get => (bool)GetValue(IsNetworkAvailableProperty);
            private set => SetValue(IsNetworkAvailableProperty, value);
        }

        public bool IsSecondaryView
        {
            get => (bool)GetValue(IsSecondaryViewProperty);
            private set => SetValue(IsSecondaryViewPropertyKey, value);
        }

        public Size Size
        {
            get => GetWidgetSize();
            set => SetWidgetSize(value);
        }

        public Point Position
        {
            get => GetWidgetLocation();
            set => SetWidgetLocationState(value);
        }

        public WidgetState State { get; private set; } = WidgetState.Suspended;

        public WidgetCornerMode CornerMode
        {
            get => (WidgetCornerMode)GetValue(CornerModeProperty);
            set => SetValue(CornerModeProperty, value);
        }

        public IEnumerable<Permission> Permissions { get; private set; }

        public SearchType SearchType => SearchType.Widget;

        public ICommand AppearedCommand
        {
            get => (ICommand)GetValue(AppearedCommandProperty);
            set => SetValue(AppearedCommandProperty, value);
        }

        public ICommand UnpinCommand
        {
            get => (ICommand)GetValue(UnpinedCommandProperty);
            set => SetValue(UnpinedCommandProperty, value);
        }

        public ICommand MouseEnterCommand
        {
            get => (ICommand)GetValue(MouseEnterCommandProperty);
            set => SetValue(MouseEnterCommandProperty, value);
        }

        public ICommand MouseLeaveCommand
        {
            get => (ICommand)GetValue(MouseLeaveCommandProperty);
            set => SetValue(MouseLeaveCommandProperty, value);
        }

        public ICommand PinnedCommand
        {
            get => (ICommand)GetValue(PinnedCommandProperty);
            set => SetValue(PinnedCommandProperty, value);
        }

        public ICommand SizeChangedCommand
        {
            get => (ICommand)GetValue(SizeChangedCommandProperty);
            set => SetValue(SizeChangedCommandProperty, value);
        }

        public ICommand ShareCommand
        {
            get => (ICommand)GetValue(ShareCommandProperty);
            set => SetValue(ShareCommandProperty, value);
        }

        public ICommand StateChangedCommand
        {
            get => (ICommand)GetValue(StateChangedCommandProperty);
            set => SetValue(StateChangedCommandProperty, value);
        }

        public object MouseLeaveCommandParameter
        {
            get => GetValue(MouseLeaveCommandParameterProperty);
            set => SetValue(MouseLeaveCommandParameterProperty, value);
        }

        public object MouseEnterCommandParameter
        {
            get => GetValue(MouseEnterCommandParameterProperty);
            set => SetValue(MouseEnterCommandParameterProperty, value);
        }

        public object AppearedCommandParameter
        {
            get => GetValue(AppearedCommandParameterProperty);
            set => SetValue(AppearedCommandParameterProperty, value);
        }

        public object PinnedCommandParameter
        {
            get => GetValue(PinnedCommandParameterProperty);
            set => SetValue(PinnedCommandParameterProperty, value);
        }

        public object UnpinCommandParameter
        {
            get => GetValue(UnpinedCommandParameterProperty);
            set => SetValue(UnpinedCommandParameterProperty, value);
        }

        public object ShareCommandParameter
        {
            get => GetValue(ShareCommandParameterProperty);
            set => SetValue(ShareCommandParameterProperty, value);
        }

        public WidgetTitleBar TitleBar
        {
            get => (WidgetTitleBar)GetValue(TitleBarProperty);
            set
            {
                SetValue(TitleBarProperty, value);

                if(titleBarElement == value) return;

                UnsetTitleBar();
                SetTitleBar(value);
            }
        }

        public bool IsTitleBarEnabled
        {
            get => (bool)GetValue(IsTitleBarEnabledProperty);
            set => SetValue(IsTitleBarEnabledProperty, value);
        }

        public bool IsHideTitleBar
        {
            get => (bool)GetValue(IsHideTitleBarProperty);
            set => SetValue(IsHideTitleBarProperty, value);
        }

        #region MessageBarProps

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool IsMessageBarOpened
        {
            get => (bool)GetValue(IsMessageBarOpenedProperty);
            set => SetValue(IsMessageBarOpenedProperty, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal bool IsMessageBarClosable
        {
            get => (bool)GetValue(IsMessageBarClosableProperty);
            set => SetValue(IsMessageBarClosableProperty, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal string MessageBarTitle
        {
            get => (string)GetValue(MessageBarTitleProperty);
            set => SetValue(MessageBarTitleProperty, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal string MessageBarText
        {
            get => (string)GetValue(MessageBarTextProperty); 
            set => SetValue(MessageBarTextProperty, value);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal InfoBarSeverity MessageBarSeverity
        {
            get => (InfoBarSeverity)GetValue(MessageBarSeverityProperty);
            set => SetValue(MessageBarSeverityProperty, value);
        }

        #endregion

        #region UIWidgetDialogProps

        public bool IsWidgetDialogOpen
        {
            get => (bool)GetValue(IsWidgetDialogOpenProperty);
            set => SetValue(IsWidgetDialogOpenPropertyKey, value);
        }

        public bool WidgetContentDialogStaysOpen
        {
            get => (bool)GetValue(WidgetContentDialogStaysOpenProperty);
            set => SetValue(WidgetContentDialogStaysOpenPropertyKey, value);
        }

        public object WidgetDialogContent
        {
            get => GetValue(WidgetDialogContentProperty);
            private set => SetValue(WidgetDialogContentPropertyKey, value);
        }

        public string WidgetTitle
        {
            get => (string)GetValue(WidgetTitleProperty);
            private set => SetValue(WidgetTitlePropertyKey, value);
        }

        public object WidgetPrimaryButtonContent
        {
            get => GetValue(WidgetPrimaryButtonContentProperty);
            private set => SetValue(WidgetPrimaryButtonContentPropertyKey, value);
        }

        public object WidgetSecondaryButtonContent
        {
            get => GetValue(WidgetSecondaryButtonContentProperty);
            private set => SetValue(WidgetSecondaryButtonContentPropertyKey, value);
        }

        public ControlAppearance WidgetPrimaryButtonAppearance
        {
            get => (ControlAppearance)GetValue(WidgetPrimaryButtonAppearanceProperty);
            private set => SetValue(WidgetPrimaryButtonAppearancePropertyKey, value);
        }

        public object WidgetPrimaryButtonParameter
        {
            get => GetValue(WidgetPrimaryButtonParameterProperty);
            private set => SetValue(WidgetPrimaryButtonParameterPropertyKey, value);
        }

        public ControlAppearance WidgetSecondaryButtonAppearance
        {
            get => (ControlAppearance)GetValue(WidgetSecondaryButtonAppearanceProperty);
            private set => SetValue(WidgetSecondaryButtonAppearancePropertyKey, value);
        }

        public object WidgetSecondaryButtonParameter
        {
            get => GetValue(WidgetSecondaryButtonParameterProperty);
            private set => SetValue(WidgetSecondaryButtonParameterPropertyKey, value);
        }

        public Visibility WidgetDialogTitleVisibility
        {
            get => (Visibility)GetValue(WidgetDialogTitleVisibilityProperty);
            set => SetValue(WidgetDialogTitleVisibilityPropertyKey, value);
        }

        public Visibility WidgetSecondaryButtonVisibility
        {
            get => (Visibility)GetValue(WidgetSecondaryButtonVisibilityProperty);
            private set => SetValue(WidgetSecondaryButtonVisibilityPropertyKey, value);
        }

        public Visibility WidgetPrimaryButtonVisibility
        {
            get => (Visibility)GetValue(WidgetPrimaryButtonVisibilityProperty);
            private set => SetValue(WidgetPrimaryButtonVisibilityPropertyKey, value);
        }

        public ICommand WidgetPrimaryButtonCommand
        {
            get => (ICommand)GetValue(WidgetPrimaryButtonCommandProperty);
            private set => SetValue(WidgetPrimaryButtonCommandPropertyKey, value);
        }

        public ICommand WidgetSecondaryButtonCommand
        {
            get => (ICommand)GetValue(WidgetSecondaryButtonCommandProperty);
            private set => SetValue(WidgetSecondaryButtonCommandPropertyKey, value);
        }

        public bool IsDialogPrimaryButtonEnabled
        {
            get => (bool)GetValue(IsDialogPrimaryButtonEnabledProperty);
            set => SetValue(IsDialogPrimaryButtonEnabledProperty, value);
        }

        public Brush WidgetDialogBackground
        {
            get => (Brush)GetValue(WidgetDialogBackgroundProperty);
            set => SetValue(WidgetDialogBackgroundProperty, value);
        }

        public Brush WidgetDialogForeground
        {
            get => (Brush)GetValue(WidgetDialogForegroundProperty);
            set => SetValue(WidgetDialogForegroundProperty, value);
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<bool> NetworkStateChanged;

        public event RoutedEventHandler ContentDialogOpened
        {
            add { AddHandler(ContentDialogOpenedEvent, value); }
            remove { RemoveHandler(ContentDialogOpenedEvent, value); }
        }

        public event RoutedEventHandler ContentDialogClosed
        {
            add { AddHandler(ContentDialogClosedEvent, value); }
            remove { RemoveHandler(ContentDialogClosedEvent, value); }
        }

        #endregion

        #region Utils

        private void InitializeWidgetComponent()
        {
            SetWidgetSize(Size, false);
            SetWidgetLocationState(Position, false);
            SetBlurEnabledState(IsBlurEnabled);
            SetRondedCornersState(CornerMode);
            SetAltTabVisibility(false);
            SetBackdropTransparency(Settings.WidgetTransparency);
            SetIsHideTitleBar(IsHideTitleBar);
            SetTitleBar(TitleBar);
            GetIsPinnedState();
            SetNetworkEvents();
            BeginAppear();
        }

        private void SetNetworkEvents()
        {
            SetValue(IsNetworkAvailablePropertyKey, NetworkHelper.IsConnected);
            NetworkInformation.NetworkStatusChanged += OnNetworkStatusChanged;
        }

        private void SetBlurEnabledState(bool enabled)
        {
            AccentState accent = enabled ? AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND :
                                           AccentState.ACCENT_DISABLED;

            WindowHelper.SetBlurBehind(this, accent);
        }

        private void SetRondedCornersState(WidgetCornerMode cornerMode)
        {
            WindowHelper.SetRoundedCorners(this, cornerMode);
        }

        private void SetAltTabVisibility(bool isVisible)
        {
            WindowHelper.SetAltTabVisibility(this, isVisible);
        }

        private void SetBackdropTransparency(double transparency)
        {
            if(transparency == BackdropOpacity) return;

            BackdropOpacity = transparency;
        }

        /// <summary>
        /// Throws AppearedCommand execution for all widget inheritors.
        /// </summary>
        private void BeginAppear()
        {
            if(IsPreview)
            {
                if(Content is FrameworkElement content)
                   content.Loaded += delegate
                   {
                       AppearedCommand?.Execute(AppearedCommandParameter);
                   };
            }
            else AppearedCommand?.Execute(AppearedCommandParameter);
        }

        /// <summary>
        /// Hides title bar when mouse leaves widget area.
        /// </summary>
        /// <param name="value"></param>
        public void SetIsHideTitleBar(bool value)
        {
            if(TitleBar == null) return;
               
            TitleBar.Visibility = value ?
                     Visibility.Hidden : Visibility.Visible;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild(UIPrimaryWidgetDialogButton) is Button primaryWidgetDialogButton)
               primaryWidgetDialogButton.Click += OnDialogButtonClick;

            if(GetTemplateChild(UISecondaryWidgetDialogButton) is Button secondaryWidgetDialogButton)
               secondaryWidgetDialogButton.Click += OnDialogButtonClick;

            if(GetTemplateChild(UIDialogPopup) is Popup dialogPopup)
            {
                dialogPopup.Closed += OnDialogPopupClosed;
                dialogPopup.KeyDown += OnDialogKeyDown;
            }

            if(GetTemplateChild(UINotifyMessage) is InfoBar notifyMessage &&
               GetTemplateChild(UINotifyMessageCloseCommand) is Button closeButton)
            {
                NotifyMessage = notifyMessage;
                NotifyMessageCloseCommand = closeButton;

                closeButton.Click += OnNotifyMessageClose;
            }

            if(GetTemplateChild(UIWidgetDialog) is Border dialog)
               widgetDialog = dialog;

            _showDialog = ((Storyboard)Application.Current.Resources["ShowAnimation"]).Clone();
            _hideDialog = ((Storyboard)Application.Current.Resources["HideAnimation"]).Clone();

            _hideDialog.Completed += (s, e) =>
            {
                IsWidgetDialogOpen = false;
            };
        }

        #endregion

        #region Methods

        [Obsolete(Errors.WidgetUnclosable)]
        public new void Close() => throw new InvalidOperationException(Errors.WidgetUnclosable);

        public void ActivateWidget(bool activateWidgetBase = true)
        {
            SetExecutionState(WidgetState.Activated);

            if(activateWidgetBase)
            {
                Show();
                Activate();
                Focus();
            }
        }

        /// <summary>
        /// Sets the widget's dragable zone element.
        /// </summary>
        public void SetTitleBar(UIElement element)
        {
            if(IsPreview) return;
            if(element == null) return;

            titleBarElement = element;
            titleBarElement.MouseLeftButtonDown += TitleBarElement_MouseLeftButtonDown;

            if(element is WidgetTitleBar titleBar)
               titleBar.CloseRequested += TitleBar_CloseRequested;
        }

        /// <summary>
        /// Resets the widget's gragable zone element.
        /// </summary>
        public void UnsetTitleBar()
        {
            if(IsPreview) return;
            if(titleBarElement == null) return;

            titleBarElement.MouseLeftButtonDown -= TitleBarElement_MouseLeftButtonDown;
            titleBarElement = null;
        }

        /// <summary>
        /// Called when the widget appears on the desktop.
        /// </summary>
        protected virtual void OnDekstopAppear()
        {
            if(IsPreview) return;

            ActivateWidget(true);
        }

        /// <summary>
        /// Pins widget on the desktop.
        /// </summary>
        public void PinOnDesktop()
        {
            if(Id == Guid.Empty) throw new InvalidOperationException(Errors.CannotPinUnregisteredWidget);

            IsPinnedDesktop = true;

            Show();
            SetExecutionState(WidgetState.Activated);
            PinnedCommand?.Execute(PinnedCommandParameter);
        }

        /// <summary>
        /// Unpins widget from the desktop. 
        /// </summary>
        public void UnpinDesktop()
        {
            if(Id == Guid.Empty) throw new InvalidOperationException(Errors.CannotPinUnregisteredWidget);

            IsPinnedDesktop = false;

            Hide();
            SetExecutionState(WidgetState.Stopped);
            UnpinCommand?.Execute(UnpinCommandParameter);
        }

        /// <summary>
        /// Sets widget execution state.
        /// </summary>
        /// <param name="state">Widget execution state</param>
        /// <exception cref="ArgumentException">Raises when widget state is unknown.</exception>
        public void SetExecutionState(WidgetState state)
        {
            if(state == WidgetState.Unknown) throw new ArgumentException(Errors.ExecutionStateIsUnknown);

            State = state;
            StateChangedCommand?.Execute(state);
        }

        /// <summary>
        /// Retrieves cached widget location from local settings.
        /// </summary>
        /// <returns>Widget position from local settings.</returns>
        public Point GetWidgetLocation()
        {
            if(Settings == null) throw new InvalidOperationException(Errors.SettingsStorageLoadFailed);

            var point = new Point();

            point.X = Settings.GetWidgetValue(Id, nameof(point.X), Left);
            point.Y = Settings.GetWidgetValue(Id, nameof(point.Y), Top);

            return point;
        }

        /// <summary>
        /// Sets current widget location and saves it to local settings.
        /// </summary>
        /// <param name="point">New window location</param>
        /// <param name="cache">If true, the widget position will be cached, otherwise caching is skipped.</param>
        public void SetWidgetLocationState(Point? point, bool cache = true)
        {
            if(IsPreview) return;

            if(point == null) throw new ArgumentNullException(nameof(point));

            Top = point.Value.Y; 
            Left = point.Value.X;

            if(cache) CacheWidgetLocation(point.Value);
        }

        /// <summary>
        /// Caches widget position from System.Windows.Point to local settings.
        /// </summary>
        /// <param name="point">Position struct</param>
        private void CacheWidgetLocation(Point point)
        {
            Settings?.SetWidgetValue<double>(Id, nameof(point.X), point.X);
            Settings?.SetWidgetValue<double>(Id, nameof(point.Y), point.Y);
        }

        /// <summary>
        /// Retrieves widget size from local settings.
        /// </summary>
        /// <returns>Widget size</returns>
        public Size GetWidgetSize()
        {
            if(Settings == null) throw new InvalidOperationException(Errors.SettingsStorageLoadFailed);

            var size = new Size();

            size.Height = Settings.GetWidgetValue<double>(Id, nameof(size.Height), Height);
            size.Width = Settings.GetWidgetValue<double>(Id, nameof(size.Width), Width);

            return size;
        }

        /// <summary>
        /// Sets widget height and width properties from Size struct and caches it into local settings if cache property is true.
        /// </summary>
        /// <param name="size">New widget size</param>
        /// <param name="cache">If true, size chaching is enabled, disabled if false.</param>
        public void SetWidgetSize(Size? size, bool cache = true)
        {
            if(IsPreview) return;

            if(size == null) throw new ArgumentNullException(nameof(size));

            Height = size.Value.Height;
            Width = size.Value.Width;

            CacheWidgetSize(size.Value);
        }

        /// <summary>
        /// Caches size struct into local settings.
        /// </summary>
        /// <param name="size">New widget size</param>
        private void CacheWidgetSize(Size size)
        {
            Settings.SetWidgetValue<double>(Id, nameof(size.Height), size.Height);
            Settings.SetWidgetValue<double>(Id, nameof(size.Width), size.Width);
        }

        /// <summary>
        /// Displays the message bar notification inside widget area.
        /// </summary>
        /// <param name="message">Message bar text</param>
        /// <param name="title">Message bar title</param>
        /// <param name="isClosable">Enables ability to close message bar.</param>
        /// <param name="severity">Setup message bar severity.</param>
        /// <param name="hasDelay">Enables message bar hide delay.</param>
        /// <param name="delay">Sets message bar hide delay.</param>
        public void ShowNotify(string message, string title = null, bool isClosable = false, InfoBarSeverity severity = InfoBarSeverity.Informational, bool hasDelay = false, TimeSpan delay = default)
        {
            if(IsPreview)
            {
                var shell = ShellHelper.GetAppShell();
                shell?.NotifyUser(message, title, severity);

                return;
            }

            MessageBarTitle = title;
            MessageBarText = message;
            IsMessageBarClosable = isClosable;
            MessageBarSeverity = severity;
            IsMessageBarOpened = true;

            if(hasDelay)
            {
                if(_messageBarDelay == null)
                {
                    _messageBarDelay = new DispatcherTimer();
                    _messageBarDelay.Tick += OnMessageBarDelayTick;
                }

                _messageBarDelay.Interval = delay;
                _messageBarDelay.Start();
            }
        }

        /// <summary>
        /// Hides the message bar notification.
        /// </summary>
        public void HideNotify()
        {
            IsMessageBarOpened = false;
            MessageBarTitle = string.Empty;
            MessageBarText = string.Empty;
            IsMessageBarClosable = true;
            MessageBarSeverity = InfoBarSeverity.Informational;
        }

        /// <summary>
        /// Gets widget IsPinned state from settigs.
        /// </summary>
        private void GetIsPinnedState()
        {
            SetCurrentValue
            (
                IsPinnedProperty, 
                Settings?.GetWidgetValue(Id, nameof(IsPinned), false) ?? false
            );
        }

        /// <summary>
        /// Shows content dialog with custom actions and content inside widget control.
        /// </summary>
        public void ShowContentDialog(WidgetContentDialogParams parameters)
        {
            if(IsPreview) return;
            if(IsWidgetDialogOpen) throw new InvalidOperationException(Errors.CannotOpenDialogWhenAnotherIsOpen);

            IsWidgetDialogOpen = true;
            WidgetTitle = parameters.Title;
            WidgetContentDialogStaysOpen = parameters.StaysOpen;
            WidgetDialogTitleVisibility = parameters.TitleBarVisibility;
            WidgetPrimaryButtonContent = parameters.PrimaryButtonContent;
            WidgetSecondaryButtonContent = parameters.SecondaryButtonContent;
            WidgetPrimaryButtonAppearance = parameters.PrimaryButtonAppearance;
            WidgetSecondaryButtonAppearance = parameters.SecondaryButtonAppearance;
            WidgetPrimaryButtonCommand = parameters.PrimaryButtonCommand;
            WidgetSecondaryButtonCommand = parameters.SecondaryButtonCommand;
            WidgetSecondaryButtonVisibility = parameters.SecondaryButtonVisibility;
            WidgetPrimaryButtonVisibility = parameters.PrimaryButtonVisibility;
            WidgetPrimaryButtonParameter = parameters.PrimaryButtonParameter;
            WidgetSecondaryButtonParameter = parameters.SecondaryButtonParameter;
            WidgetDialogContent = parameters.Content is string ?
            new TextBlock()
            {
                Width = ActualWidth,
                Text = parameters.Content as string,
                TextWrapping = TextWrapping.Wrap
            } : parameters.Content;

            WidgetDialogBackground = parameters.Background ??
                                     (SolidColorBrush)Application.Current.Resources["ContentDialogBackground"];
            WidgetDialogForeground = parameters.Foreground ??
                                     (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];

            widgetDialog.BeginStoryboard(_showDialog);

            ((Popup)GetTemplateChild(UIDialogPopup)).Focus();
            RaiseEvent(new(ContentDialogOpenedEvent));
        }

        /// <summary>
        /// Hides content dialog with cancel dialog event raising.
        /// </summary>
        /// <param name="raiseCancel">If true raises cancel event.</param>
        public void HideContentDialog(bool raiseCancel = false)
        {
            if(IsPreview) return;

            if(raiseCancel)
               WidgetSecondaryButtonCommand?.Execute(WidgetSecondaryButtonParameter);

            widgetDialog.BeginStoryboard(_hideDialog);
        }

        /// <summary>
        /// Shows core widget view and raises activated execution state event.
        /// </summary>
        public void ShowWidget()
        {
            Show();
            SetExecutionState(WidgetState.Activated);
        }

        /// <summary>
        /// Hides core widget view and raises suspended execution state event.
        /// </summary>
        public void HideWidget()
        {
            Hide();
            SetExecutionState(WidgetState.Suspended);
        }

        #endregion

        #region EventHandlers

        private void Widget_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWidgetComponent();
        }

        private void Widget_LocationChanged(object sender, EventArgs e)
        {
            CacheWidgetLocation(new Point(Left, Top));
        }

        private void Widget_StateChanged(object sender, EventArgs e)
        {
            SetExecutionState(WindowState == WindowState.Minimized ?
                              WidgetState.Suspended : WidgetState.Activated);
        }

        private void Widget_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(IsActive)
            {
                SizeChangedCommand?.Execute(e.NewSize);
                CacheWidgetSize(e.NewSize);
            }
        }

        private static void OnIsBlurEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            bool enabled = (bool)args.NewValue;
            Widget widget = d as Widget;

            widget.SetBlurEnabledState(enabled);
        }

        private static void OnCornerModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            WindowHelper.SetRoundedCorners((Widget)d, (WidgetCornerMode)args.NewValue);
        }

        private void TitleBarElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                   DragMove();
            }
            catch { }
        }

        private void OnMessageBarDelayTick(object sender, EventArgs e) => HideNotify();

        private static void OnIsPinnedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Widget widget)
            {
                bool newValue = (bool)e.NewValue;

                widget.Topmost = newValue;
                widget.Settings?.SetWidgetValue<bool>(widget.Id, nameof(IsPinned), newValue);
            }
        }

        private static void OnIsHideTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is Widget widget)
               widget.SetIsHideTitleBar((bool)e.NewValue);
        }

        private void Widget_MouseEnter(object sender, MouseEventArgs e)
        {
            if(IsHideTitleBar && TitleBar != null)
               TitleBar.Visibility = Visibility.Visible;

            MouseEnterCommand?.Execute(MouseEnterCommandParameter);
        }

        private void Widget_MouseLeave(object sender, MouseEventArgs e)
        {
            if(IsHideTitleBar && TitleBar != null)
               TitleBar.Visibility = Visibility.Hidden;

            MouseLeaveCommand?.Execute(MouseLeaveCommandParameter);
        }

        private void TitleBar_CloseRequested(WidgetTitleBar sender, RoutedEventArgs args)
        {
            UnpinDesktop();
        }

        private void OnNetworkStatusChanged(object sender)
        {
            Dispatcher?.Invoke(() =>
            {
                SetValue(IsNetworkAvailablePropertyKey, NetworkHelper.IsConnected);
                NetworkStateChanged?.Invoke(this, IsNetworkAvailable);
            });
        }

        private void OnDialogButtonClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string tag = button.Tag.ToString();
            ContentDialogButton dialogButton;
            
            if(Enum.TryParse(tag, out dialogButton))
            {
                if(dialogButton == ContentDialogButton.Primary)
                   WidgetPrimaryButtonCommand?.Execute(WidgetPrimaryButtonParameter);
                else if(dialogButton == ContentDialogButton.Secondary)
                   WidgetSecondaryButtonCommand?.Execute(WidgetSecondaryButtonParameter);

                HideContentDialog();
            }
        }

        private void OnDialogPopupClosed(object sender, EventArgs e)
        {
            if(IsWidgetDialogOpen) IsWidgetDialogOpen = false;

            RaiseEvent(new(ContentDialogClosedEvent));

            WidgetTitle = null;
            WidgetContentDialogStaysOpen = false;
            WidgetPrimaryButtonContent = null;
            WidgetSecondaryButtonContent = null;
            WidgetPrimaryButtonAppearance = ControlAppearance.Primary;
            WidgetSecondaryButtonAppearance = ControlAppearance.Secondary;
            WidgetPrimaryButtonCommand = null;
            WidgetSecondaryButtonCommand = null;
            WidgetSecondaryButtonVisibility = Visibility.Visible;
            WidgetPrimaryButtonVisibility = Visibility.Visible;
            WidgetPrimaryButtonParameter = null;
            WidgetSecondaryButtonParameter = null;
            WidgetDialogContent = null;
            IsDialogPrimaryButtonEnabled = true;
            WidgetDialogBackground = (SolidColorBrush)Application.Current.Resources["ContentDialogBackground"];
        }

        private void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape) IsWidgetDialogOpen = false;
        }

        private void OnNotifyMessageClose(object sender, RoutedEventArgs e)
        {
            IsMessageBarOpened = false;
        }

        #endregion
    }
}
