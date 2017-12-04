using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ElectronicsStore.Startup))]
namespace ElectronicsStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
