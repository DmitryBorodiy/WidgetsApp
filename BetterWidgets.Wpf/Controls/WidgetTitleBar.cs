using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace BetterWidgets.Controls
{
    [TemplatePart(Name = UIPinCommand, Type = typeof(Wpf.Ui.Controls.Button))]
    [TemplatePart(Name = UICloseCommand, Type = typeof(Wpf.Ui.Controls.Button))]
    public sealed class WidgetTitleBar : Control
    {
        private const string UIPinCommand = nameof(UIPinCommand);
        private const string UICloseCommand = nameof(UICloseCommand);

        public WidgetTitleBar()
        {
            DefaultStyleKey = typeof(WidgetTitleBar);
        }

        #region PropsRegistrations

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(WidgetTitleBar), 
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon),
            typeof(ImageSource),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
            nameof(IconVisibility),
            typeof(Visibility),
            typeof(WidgetTitleBar),
            new PropertyMetadata(default));

        public static readonly DependencyProperty TitleVisibilityProperty = DependencyProperty.Register(
            nameof(TitleVisibility),
            typeof(Visibility),
            typeof(WidgetTitleBar),
            new PropertyMetadata(default));

        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.Register(
            nameof(CanClose),
            typeof(bool),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CanPinProperty = DependencyProperty.Register(
            nameof(CanPin),
            typeof(bool),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsPinnedProperty = DependencyProperty.Register(
            nameof(IsPinned),
            typeof(bool),
            typeof(WidgetTitleBar),
            new PropertyMetadata(false));

        public static readonly DependencyProperty CloseCommandProperty = DependencyProperty.Register(
            nameof(CloseCommand),
            typeof(ICommand),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty CloseCommandParameterProperty = DependencyProperty.Register(
            nameof(CloseCommandParameter),
            typeof(object),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content),
            typeof(object),
            typeof(WidgetTitleBar),
            new PropertyMetadata(null));

        #endregion

        #region EventRegistrations

        public static readonly RoutedEvent CloseRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(CloseRequested),
            RoutingStrategy.Bubble,
            typeof(TypedEventHandler<WidgetTitleBar, RoutedEventArgs>),
            typeof(WidgetTitleBar)
        );

        public static readonly RoutedEvent PinRequestedEvent = EventManager.RegisterRoutedEvent(
            nameof(PinRequested),
            RoutingStrategy.Bubble,
            typeof(TypedEventHandler<WidgetTitleBar, RoutedEventArgs>),
            typeof(WidgetTitleBar)
        );

        #endregion

        #region Events

        public event TypedEventHandler<WidgetTitleBar, RoutedEventArgs> CloseRequested
        {
            add => AddHandler(CloseRequestedEvent, value);
            remove => RemoveHandler(CloseRequestedEvent, value);
        }

        public event TypedEventHandler<WidgetTitleBar, RoutedEventArgs> PinRequested
        {
            add => AddHandler(PinRequestedEvent, value);
            remove => RemoveHandler(PinRequestedEvent, value);
        }

        #endregion

        #region Props

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Visibility IconVisibility
        {
            get => (Visibility)GetValue(IconVisibilityProperty);
            set => SetValue(IconVisibilityProperty, value);
        }

        public Visibility TitleVisibility
        {
            get => (Visibility)GetValue(TitleVisibilityProperty);
            set => SetValue(TitleVisibilityProperty, value);
        }

        public bool CanClose
        {
            get => (bool)GetValue(CanCloseProperty);
            set => SetValue(CanCloseProperty, value);
        }

        public bool CanPin
        {
            get => (bool)GetValue(CanPinProperty);
            set => SetValue(CanPinProperty, value);
        }

        public bool IsPinned
        {
            get => (bool)GetValue(IsPinnedProperty);
            set => SetValue(IsPinnedProperty, value);
        }

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public object CloseCommandParameter
        {
            get => GetValue(CloseCommandParameterProperty);
            set => SetValue(CloseCommandParameterProperty, value);
        }

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild(UIPinCommand) is ToggleButton pinCommand)
               pinCommand.Click += delegate 
               {
                   if(pinCommand.IsChecked.HasValue)
                      IsPinned = pinCommand.IsChecked.Value;

                   RaiseEvent(new RoutedEventArgs(PinRequestedEvent)); 
               };

            if(GetTemplateChild(UICloseCommand) is Button closeCommand)
               closeCommand.Click += delegate 
               {
                   CloseCommand?.Execute(CloseCommandParameter);
                   RaiseEvent(new RoutedEventArgs(CloseRequestedEvent)); 
               };
        }
    }
}
