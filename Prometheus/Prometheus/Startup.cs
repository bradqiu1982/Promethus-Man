using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Prometheus.Startup))]
namespace Prometheus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
