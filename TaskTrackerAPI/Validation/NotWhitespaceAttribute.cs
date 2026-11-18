using System.ComponentModel.DataAnnotations;

namespace TaskTrackerAPI.Validation
{
    public class NotWhitespaceAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string text)
                return false;

            return !string.IsNullOrWhiteSpace(text);
        }
    }
}
