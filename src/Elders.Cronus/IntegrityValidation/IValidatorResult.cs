using System.Collections.Generic;

namespace Elders.Cronus.IntegrityValidation
{
    public interface IValidatorResult
    {
        bool IsValid { get; }
        string ErrorType { get; }
        IEnumerable<string> Errors { get; }
    }
}