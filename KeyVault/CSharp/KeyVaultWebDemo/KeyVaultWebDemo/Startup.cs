using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KeyVaultWebDemo.Startup))]
namespace KeyVaultWebDemo
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
