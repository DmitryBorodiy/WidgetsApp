using BetterWidgets.Consts;
using System.Globalization;
using System.Windows.Controls;

namespace BetterWidgets.Behaviours.Validators
{
    public class NotEmptyValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value is not string) throw new ArgumentException(Errors.UnexpectedValueType);

            return value is string input &&
                   !string.IsNullOrWhiteSpace(input) ?
                   ValidationResult.ValidResult : new ValidationResult(false, Resources.Resources.EmptyInputMessage);
        }
    }
}
