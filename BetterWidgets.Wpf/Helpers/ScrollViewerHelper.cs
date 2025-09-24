using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BetterWidgets.Helpers
{
    public static class ScrollViewerHelper
    {
        #region PropertyRegistrations

        private const string EnableHorizontalScroll = nameof(EnableHorizontalScroll);
        public static readonly DependencyProperty EnableHorizontalScrollProperty = DependencyProperty.RegisterAttached(
            EnableHorizontalScroll,
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnEnableHorizontalScrollingChanged));

        private const string EnableVerticalScroll = nameof(EnableVerticalScroll);
        public static readonly DependencyProperty EnableVerticalScrollProperty = DependencyProperty.RegisterAttached(
            EnableVerticalScroll,
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnEnableVerticalScrollChanged));

        #endregion

        #region Handlers

        public static bool GetEnableHorizontalScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableHorizontalScrollProperty);
        }

        public static void SetEnableHorizontalScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableHorizontalScrollProperty, value);
        }

        public static bool GetEnableVerticalScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableVerticalScrollProperty);
        }

        public static void SetEnableVerticalScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableVerticalScrollProperty, value);
        }

        private static void OnEnableHorizontalScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if(d is ScrollViewer scrollViewer)
                {
                    if((bool)e.NewValue)
                       scrollViewer.PreviewMouseWheel += OnScroll;
                    else
                       scrollViewer.PreviewMouseWheel -= OnScroll;
                }
            }
            catch { }
        }

        private static void OnEnableVerticalScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if(d is ScrollViewer scrollViewer)
                {
                    if((bool)e.NewValue)
                       scrollViewer.PreviewMouseWheel += OnScroll;
                    else
                       scrollViewer.PreviewMouseWheel -= OnScroll;
                }
            }
            catch { }
        }

        private static void OnScroll(object sender, MouseWheelEventArgs e)
        {
            if(sender is ScrollViewer scrollViewer)
            {
                if(scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
                {
                    if(GetEnableHorizontalScroll(scrollViewer))
                       scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                    else if(GetEnableVerticalScroll(scrollViewer))
                       scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);

                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
