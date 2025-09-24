using SkiaSharp;
using LiveChartsCore;
using SkiaSharp.Views.WPF;
using System.Windows.Media;
using BetterWidgets.Helpers;
using LiveChartsCore.SkiaSharpView;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using LiveChartsCore.SkiaSharpView.Painting;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BetterWidgets.ViewModel.Widgets.Components
{
    public partial class LineSeriesViewModel : ObservableObject
    {
        public LineSeriesViewModel(IEnumerable<double> values, Color color)
        {
            if(values == null) values = Enumerable.Empty<double>();

            Color = color;

            Values = new ObservableCollection<double>(values);
            Values.CollectionChanged += OnValuesCollectionChanged;

            Series = new ObservableCollection<ISeries>(GetSeries(values));
        }

        #region Props

        public int MaxValues { get; set; } = 30;
        public Color Color { get; set; } = AccentColorHelper.AccentColor;

        [ObservableProperty]
        public ObservableCollection<ISeries> series;

        [ObservableProperty]
        public ObservableCollection<double> values;

        #endregion

        #region Utils

        private LinearGradientPaint GetCurrentBrush()
        {
            var accentColor = Color.ToSKColor();
            var transparent = new SKColor(0, 0, 0, 0);

            var startPoint = new SKPoint(0, 0);
            var endPoint = new SKPoint(0, 1);

            return new LinearGradientPaint([accentColor, transparent], startPoint, endPoint);
        }

        private List<LineSeries<double>> GetSeries(IEnumerable<double> values) => new List<LineSeries<double>>()
        {
            new LineSeries<double>()
            {
                Values = new ObservableCollection<double>(values),
                Fill = GetCurrentBrush(),
                Stroke = new SolidColorPaint(Color.ToSKColor()),
                GeometrySize = 0.3,
                LineSmoothness = 0.1,
                GeometryFill = null,
                GeometryStroke = null
            }
        };

        #endregion

        #region Methods

        public void AddNext(double value)
        {
            if(Values == null) return;
            if(Values.Count > MaxValues) Values.RemoveAt(0);

            Values.Add(value);
        }

        public void RemoveAt(int index)
        {
            if(Values == null) return;
            if(index < 0 || index > Values.Count) return;

            Values.RemoveAt(index);
        }

        public void Remove(double value)
        {
            if(Values == null) return;

            int index = Values.IndexOf(value);

            RemoveAt(index);
        }

        #endregion

        #region Handlers

        private void OnValuesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var series = Series.First().Values as ObservableCollection<double>;

            if(series == null || series.Count == 0)
               Series = new ObservableCollection<ISeries>(GetSeries(Values));

            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(double item in e.NewItems)
                   series.Add(item);
            }
            else if(e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach(double item in e.OldItems)
                   series.Remove(item);
            }
            else Series = new ObservableCollection<ISeries>(GetSeries(Values));
        }

        #endregion
    }
}
