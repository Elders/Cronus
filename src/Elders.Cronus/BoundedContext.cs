using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Elders.Cronus;

public class BoundedContext
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "The configuration `Cronus:BoundedContext` is required. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md")]
    [RegularExpression(@"^\b([\w\d_]+$)", ErrorMessage = "Characters are not allowed for configuration `Cronus:BoundedContext`. For more information see here https://github.com/Elders/Cronus/blob/master/doc/Configuration.md")]
    public string Name { get; set; }

    public override string ToString() => Name;
}

public class BoundedContextProvider : CronusOptionsProviderBase<BoundedContext>
{
    public const string SettingKey = "cronus:boundedcontext";

    public BoundedContextProvider(IConfiguration configuration) : base(configuration) { }

    public override void Configure(BoundedContext options)
    {
        options.Name = configuration[SettingKey]?.ToLower()?.Trim();
    }
}
