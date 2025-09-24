using System.Windows;
using System.Windows.Data;
using BetterWidgets.Consts;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BetterWidgets.Behaviours.Converters
{
    public class BoolToVisibility : IValueConverter
    {
        private readonly ILogger _logger;

        public BoolToVisibility()
        {
            _logger = App.Services?.GetRequiredService<ILogger<BoolToVisibility>>();
        }

        public bool IsInvert { get; set; } = false;

        private bool IsValid(object value)
            => value.GetType() == typeof(bool) ||
               value.GetType() == typeof(Visibility);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if(!IsValid(value)) throw new ArgumentException(string.Format(Errors.UnexpectedValueType, value.GetType().FullName));

                if(!IsInvert)
                   return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                else
                   return (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if(!IsValid(value)) throw new ArgumentException(string.Format(Errors.UnexpectedValueType, value.GetType().FullName));

                if(!IsInvert)
                   return (Visibility)value == Visibility.Visible;
                else
                   return (Visibility)value == Visibility.Collapsed;
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, ex.Message, ex.StackTrace);

                return value;
            }
        }
    }
}
