using System.Globalization;
using System.Windows.Controls;

namespace Scanner.Infrastructure
{

    public class StringRangeValidationRule : ValidationRule
    {
        private int _minimumLength = -1;
        private int _maximumLength = -1;
        private string _errorMessage;

        public int MinimumLength
        {
            get { return _minimumLength; }
            set { _minimumLength = value; }
        }

        public int MaximumLength
        {
            get { return _maximumLength; }
            set { _maximumLength = value; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public override ValidationResult Validate(object value,
            CultureInfo cultureInfo)
        {
            ValidationResult result = new ValidationResult(true, null);
            int n;
            bool isNumeric = int.TryParse("123", out n);
            if (isNumeric && value?.ToString().Length>=MinimumLength && value.ToString().Length<=MaximumLength)
                return result;
            return new ValidationResult(false, this.ErrorMessage);

        }
    }

}
