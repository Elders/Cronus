using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Elders.Cronus;

public abstract class CronusOptionsProviderBase<TOptions> :
    IConfigureOptions<TOptions>,
    IOptionsChangeTokenSource<TOptions>,
    IOptionsFactory<TOptions>,
    IPostConfigureOptions<TOptions>
    where TOptions : class, new()
{
    protected IConfiguration configuration;

    public CronusOptionsProviderBase(IConfiguration configuration, string name)
    {
        this.configuration = configuration;
        this.Name = name;
    }

    public CronusOptionsProviderBase(IConfiguration configuration)
        : this(configuration, Options.DefaultName)
    { }

    public string Name { get; private set; }

    public abstract void Configure(TOptions options);

    public TOptions Create(string name)
    {
        var newOptions = new TOptions();
        Configure(newOptions);
        ValidateByDataAnnotation(newOptions);
        return newOptions;
    }

    public IChangeToken GetChangeToken() => configuration.GetReloadToken();

    public virtual void PostConfigure(string name, TOptions options) { }

    private void ValidateByDataAnnotation(TOptions instance)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(instance);
        var valid = TryValidateObjectRecursive(instance, validationResults);
        if (valid) return;

        var msg = string.Join("\n", validationResults.Select(r => $"{r.ErrorMessage} => {string.Join(',', r.MemberNames)}"));
        throw new Exception($"Invalid configuration!':\n{msg}");
    }

    private bool TryValidateObject(object obj, ICollection<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
    {
        return Validator.TryValidateObject(obj, new ValidationContext(obj, null, validationContextItems), results, true);
    }

    private bool TryValidateObjectRecursive(object obj, List<ValidationResult> results, IDictionary<object, object> validationContextItems = null)
    {
        return TryValidateObjectRecursive(obj, results, new List<object>(), validationContextItems);
    }

    private bool TryValidateObjectRecursive(object obj, List<ValidationResult> results, ICollection<object> validatedObjects, IDictionary<object, object> validationContextItems = null)
    {
        if (validatedObjects.Contains(obj))
            return true;

        validatedObjects.Add(obj);
        bool result = TryValidateObject(obj, results, validationContextItems);

        var properties = obj.GetType().GetProperties().Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
                continue;

            var value = GetPropertyValue(obj, property.Name);

            if (value is null)
                continue;

            var asEnumerable = value as IEnumerable;
            if (asEnumerable is not null)
            {
                foreach (var enumObj in asEnumerable)
                {
                    if (enumObj is not null)
                    {
                        var nestedResults = new List<ValidationResult>();
                        if (TryValidateObjectRecursive(enumObj, nestedResults, validatedObjects, validationContextItems) == false)
                        {
                            result = false;
                            foreach (var validationResult in nestedResults)
                            {
                                PropertyInfo property1 = property;
                                results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.PropertyType.Name)));
                            }
                        };
                    }
                }
            }
            else
            {
                var nestedResults = new List<ValidationResult>();
                if (TryValidateObjectRecursive(value, nestedResults, validatedObjects, validationContextItems) == false)
                {
                    result = false;
                    foreach (var validationResult in nestedResults)
                    {
                        PropertyInfo property1 = property;
                        results.Add(new ValidationResult(validationResult.ErrorMessage, validationResult.MemberNames.Select(x => property1.PropertyType.Name)));
                    }
                };
            }
        }

        return result;
    }

    private static object GetPropertyValue(object o, string propertyName)
    {
        var propertyInfo = o.GetType().GetProperty(propertyName);
        if (propertyInfo is not null)
            return propertyInfo.GetValue(o, null);

        return default;
    }
}
