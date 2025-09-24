using BetterWidgets.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BetterWidgets.Controls
{
    public class PlaceholderAdorner : Adorner
    {
        public PlaceholderAdorner(TextBox textBox) : base(textBox) { }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextBox textBoxControl = (TextBox)AdornedElement;
            string placeholderValue = TextBoxHelper.GetPlaceholder(textBoxControl);

            if(string.IsNullOrEmpty(placeholderValue)) return;

            FormattedText text = new FormattedText(
                placeholderValue,
                System.Globalization.CultureInfo.CurrentCulture,
                textBoxControl.FlowDirection,
                new Typeface(textBoxControl.FontFamily,
                    textBoxControl.FontStyle,
                    textBoxControl.FontWeight,
                    textBoxControl.FontStretch),
                    textBoxControl.FontSize,
                    SystemColors.InactiveCaptionBrush,
                    VisualTreeHelper.GetDpi(textBoxControl).PixelsPerDip
            );

            text.SetForegroundBrush(textBoxControl.Foreground);
            text.MaxTextWidth = Math.Max(textBoxControl.ActualWidth - textBoxControl.Padding.Left - textBoxControl.Padding.Right, 10);
            text.MaxTextHeight = Math.Max(textBoxControl.ActualHeight, 10);

            Point renderingOffset = new Point(textBoxControl.Padding.Left, textBoxControl.Padding.Top);

            if(textBoxControl.Template.FindName("PART_ContentHost", textBoxControl) is FrameworkElement part)
            {
                Point partPosition = part.TransformToAncestor(textBoxControl).Transform(new Point(0, 0));
                renderingOffset.X += partPosition.X;
                renderingOffset.Y += partPosition.Y;

                text.MaxTextWidth = Math.Max(part.ActualWidth - renderingOffset.X, 10);
                text.MaxTextHeight = Math.Max(part.ActualHeight, 10);
            }

            drawingContext.DrawText(text, renderingOffset);
        }
    }
}
