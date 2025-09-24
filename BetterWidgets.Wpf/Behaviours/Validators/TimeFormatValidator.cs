using BetterWidgets.Consts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace BetterWidgets.Behaviours.Validators
{
    public class TimeFormatValidator : ValidationRule
    {
        public const string Default = "hh:mm tt";
        private const string TimeTemplate = @"^[hH]{1,2}[:.\-][m]{1,2}([:.\-][s]{1,2})?\s*(tt)?$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is not string) throw new ArgumentException(Errors.UnexpectedValueType);

            string input = value as string;
            var regex = new Regex(TimeTemplate);

            return new ValidationResult(regex.IsMatch(input), Resources.Resources.InvalidTimeMessage);
        }
    }
}
