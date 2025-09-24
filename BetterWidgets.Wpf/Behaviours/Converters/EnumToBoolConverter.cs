﻿using System.Globalization;
using System.Windows.Data;

namespace BetterWidgets.Behaviours.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || parameter == null) return false;

            var enumValue = Enum.GetName(value.GetType(), value);

            return enumValue.Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool isChecked && isChecked && parameter != null)
               return Enum.Parse(targetType, parameter.ToString());

            return Binding.DoNothing;
        }
    }
}
