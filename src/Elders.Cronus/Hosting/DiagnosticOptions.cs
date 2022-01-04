using Microsoft.Extensions.Configuration;

namespace Elders.Cronus
{
    public class DiagnosticOptions
    {
        public bool ActionDiagnosticEnabled { get; set; } = false;
    }

    public class DiagnosticOptionsProvider : CronusOptionsProviderBase<DiagnosticOptions>
    {
        public DiagnosticOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(DiagnosticOptions options)
        {
            configuration.GetSection("Diagnostic").Bind(options);
        }
    }
}
