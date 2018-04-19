using Microsoft.Owin;
using Owin;
using LNF;
using LNF.Impl.DependencyInjection.Web;

[assembly: OwinStartup(typeof(LNF.WebApi.Billing.Startup))]

namespace LNF.WebApi.Billing
{
    public class Startup : ApiOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
            base.Configuration(app);
        }
    }
}