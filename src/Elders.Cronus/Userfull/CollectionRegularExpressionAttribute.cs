using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Elders.Cronus;

/// <summary>
/// Regular expression validation attribute. Applies the regular expression to every element of the collecion
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
internal class CollectionRegularExpressionAttribute : RegularExpressionAttribute
{
    public CollectionRegularExpressionAttribute(string pattern) : base(pattern) { }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        List<string> errors = new List<string>();

        var collection = value as IEnumerable;
        if (collection != null)
        {
            bool isValid = true;

            foreach (var item in collection)
            {
                ValidationResult valiResult = base.IsValid(item, new ValidationContext(item));
                if (valiResult != ValidationResult.Success)
                {
                    isValid = false;
                    errors.Add(FormatErrorMessage(item as string));
                }
            }
            if (isValid == false)
            {
                errors.Add($"All values must match the regex pattern {Pattern}.");
                var err = string.Join(Environment.NewLine, errors);
                return new ValidationResult(err);
            }
        }
        return ValidationResult.Success;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The value `{name}` is not valid.";
    }
}
