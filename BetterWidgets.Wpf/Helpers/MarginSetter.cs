using System.Windows;
using System.Windows.Controls;

namespace BetterWidgets.Helpers
{
    public class MarginSetter
    {
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(MarginSetter),
                new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        public static readonly DependencyProperty LastItemMarginProperty =
            DependencyProperty.RegisterAttached("LastItemMargin", typeof(Thickness), typeof(MarginSetter),
                new UIPropertyMetadata(new Thickness(), MarginChangedCallback));

        private static Thickness GetLastItemMargin(Panel obj)
        {
            return (Thickness)obj.GetValue(LastItemMarginProperty);
        }

        public static Thickness GetMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(MarginProperty);
        }

        private static void MarginChangedCallback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = sender as Panel;

            if(panel == null) return;

            panel.Loaded -= OnPanelLoaded;
            panel.Loaded += OnPanelLoaded;

            if(panel.IsLoaded) OnPanelLoaded(panel, null);
        }

        private static void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            var panel = (Panel)sender;

            for(var i = 0; i < panel.Children.Count; i++)
            {
                if(panel.Children[i] is FrameworkElement element)
                {
                    bool isLastItem = i == panel.Children.Count - 1;

                    element.Margin = isLastItem ? GetLastItemMargin(panel) : GetMargin(panel);
                }
            }
        }

        public static void SetLastItemMargin(DependencyObject obj, Thickness value)
            => obj.SetValue(LastItemMarginProperty, value);

        public static void SetMargin(DependencyObject obj, Thickness value)
            => obj.SetValue(MarginProperty, value);
    }
}
