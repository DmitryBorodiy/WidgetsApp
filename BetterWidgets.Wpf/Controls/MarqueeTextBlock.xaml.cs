using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BetterWidgets.Controls
{
    public partial class MarqueeTextBlock : UserControl
    {
        public MarqueeTextBlock()
        {
            InitializeComponent();

            SizeChanged += OnSizeChanged;
            LayoutUpdated += OnLayoutUpdated;

            UIText.SizeChanged += OnSizeChanged;
        }

        #region Fields
        private bool _marqueeStarted = false;
        #endregion

        #region PropsRegistration

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), 
            typeof(string), 
            typeof(MarqueeTextBlock),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty DurationSecondsProperty = DependencyProperty.Register(
            nameof(DurationSeconds),
            typeof(double),
            typeof(MarqueeTextBlock),
            new PropertyMetadata(15.0, OnDurationChanged));

        #endregion

        #region Props

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double DurationSeconds
        {
            get => (double)GetValue(DurationSecondsProperty);
            set => SetValue(DurationSecondsProperty, value);
        }

        #endregion

        #region Utils

        private void StartMarqueeIfNeeded(bool reset = true)
        {
            UIText.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            if(reset) UITransform.X = 5;

            double currentX = UITransform.X;
            double textWidth = UIText.DesiredSize.Width;
            double canvasWidth = UICanvas.ActualWidth;

            UITransform.BeginAnimation(TranslateTransform.XProperty, null);

            if(textWidth > canvasWidth && canvasWidth > 0)
            {
                if(currentX < -textWidth || currentX > canvasWidth) currentX = canvasWidth;

                var animation = new DoubleAnimation
                {
                    From = currentX,
                    To = canvasWidth - textWidth,
                    Duration = new Duration(TimeSpan.FromSeconds(
                        DurationSeconds * (currentX - (-textWidth)) / (canvasWidth + textWidth)
                    )),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                UITransform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            else UITransform.X = 0;
        }

        #endregion

        #region EventHandlers

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StartMarqueeIfNeeded();
        }

        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is MarqueeTextBlock marquee)
               marquee.StartMarqueeIfNeeded();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if(!_marqueeStarted && UICanvas.ActualWidth > 0)
            {
                _marqueeStarted = true;

                StartMarqueeIfNeeded();
                LayoutUpdated -= OnLayoutUpdated;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is MarqueeTextBlock marquee)
               marquee.StartMarqueeIfNeeded(true);
        }

        #endregion
    }
}
