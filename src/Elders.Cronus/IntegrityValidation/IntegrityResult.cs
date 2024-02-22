namespace Elders.Cronus.IntegrityValidation;

public sealed class IntegrityResult<T>
{
    public IntegrityResult(T output, bool isIntegrityViolated)
    {
        Output = output;
        IsIntegrityViolated = isIntegrityViolated;
    }

    public bool IsIntegrityViolated { get; private set; }

    public T Output { get; private set; }
}
