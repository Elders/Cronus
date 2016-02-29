using System.Collections.Generic;

namespace Elders.Cronus.IntegrityValidation
{
    public class ValidatorResult : IValidatorResult
    {
        List<string> errors;

        public ValidatorResult(IEnumerable<string> errors, string errorType)
        {
            this.errors = new List<string>(errors);
            ErrorType = errorType;
        }

        public IEnumerable<string> Errors { get { return errors; } }

        public string ErrorType { get; private set; }

        public bool IsValid { get { return errors.Count == 0; } }
    }
}