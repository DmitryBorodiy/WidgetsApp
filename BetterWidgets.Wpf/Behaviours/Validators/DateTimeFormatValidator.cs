using BetterWidgets.Consts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace BetterWidgets.Behaviours.Validators
{
    public class DateFormatValidator : ValidationRule
    {
        public const string Default = "dddd, MMMM dd";
        private const string DateTemplate = @"^(([dMy]{1,4}|ddd|dddd|MMM|MMMM)[\s.,/\-]*)+$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is not string) throw new ArgumentException(Errors.UnexpectedValueType);

            var regex = new Regex(DateTemplate);

            return new ValidationResult
            (
                regex.IsMatch((string)value), 
                Resources.Resources.InvalidDateMessage
            );
        }
    }
}
