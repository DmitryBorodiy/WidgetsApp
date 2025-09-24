using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using BetterWidgets.Controls;

namespace BetterWidgets.Helpers
{
    public static class TextBoxHelper
    {
        public static string GetPlaceholder(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderProperty);

        public static void SetPlaceholder(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderProperty, value);

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(TextBoxHelper),
                new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: OnPlaceholderChanged)
                );

        #region Utils

        private static bool GetOrCreateAdorner(TextBox textBoxControl, out PlaceholderAdorner adorner)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(textBoxControl);

            if(layer == null)
            {
                adorner = null;
                return false;
            }

            adorner = layer.GetAdorners(textBoxControl)?.OfType<PlaceholderAdorner>().FirstOrDefault();

            if(adorner == null)
            {
                adorner = new PlaceholderAdorner(textBoxControl);
                layer.Add(adorner);

                adorner.Opacity = 0.7;
                adorner.IsHitTestVisible = false;
                adorner.Visibility = string.IsNullOrEmpty(textBoxControl.Text) ?
                                     Visibility.Visible : Visibility.Collapsed;
            }

            return true;
        }

        #endregion

        #region Handlers

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBox textBoxControl)
            {
                if(!textBoxControl.IsLoaded)
                {
                    textBoxControl.Loaded -= TextBoxControl_Loaded;
                    textBoxControl.Loaded += TextBoxControl_Loaded;
                }

                textBoxControl.TextChanged -= TextBoxControl_TextChanged;
                textBoxControl.TextChanged += TextBoxControl_TextChanged;

                if(GetOrCreateAdorner(textBoxControl, out PlaceholderAdorner adorner))
                   adorner.InvalidateVisual();
            }
        }

        private static void TextBoxControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox textBoxControl)
            {
                textBoxControl.Loaded -= TextBoxControl_Loaded;
                GetOrCreateAdorner(textBoxControl, out _);
            }
        }

        private static void TextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(sender is TextBox textBoxControl
               && GetOrCreateAdorner(textBoxControl, out PlaceholderAdorner adorner))
            {
                if(textBoxControl.Text.Length > 0)
                   adorner.Visibility = Visibility.Hidden;

                else
                   adorner.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}
