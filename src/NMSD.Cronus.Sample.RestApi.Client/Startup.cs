using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NMSD.Cronus.Sample.RestApi.Client.Startup))]
namespace NMSD.Cronus.Sample.RestApi.Client
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
