using System.Globalization;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace BetterWidgets.Behaviours.Validators
{
    public class PlaceNameValidator : ValidationRule
    {
        private readonly string inputValidationPattern = @"^[\p{L}\s'-]+$";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var regex = new Regex(inputValidationPattern, RegexOptions.IgnoreCase);
            bool isValid = regex.IsMatch(value.ToString());

            return isValid
                ? ValidationResult.ValidResult
                : new ValidationResult(false, Resources.Resources.PlaceNameValidationError);
        }
    }
}
