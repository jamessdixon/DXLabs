using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ATMDashboard.UI.Startup))]
namespace ATMDashboard.UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
