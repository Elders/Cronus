using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Elders.Cronus.Hosting
{
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
            return newOptions;
        }

        public IChangeToken GetChangeToken() => configuration.GetReloadToken();

        public virtual void PostConfigure(string name, TOptions options) { }
    }
}
