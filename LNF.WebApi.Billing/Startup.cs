using LNF.Impl.Context;
using LNF.Impl.DependencyInjection.Web;
using Microsoft.Owin;
using Owin;
using System.Web.Http;

[assembly: OwinStartup(typeof(LNF.WebApi.Billing.Startup))]

namespace LNF.WebApi.Billing
{
    public class Startup : ApiOwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            var ctx = new WebContext(new WebContextFactory());
            var ioc = new IOC(ctx);
            ServiceProvider.Current = ioc.Resolver.GetInstance<ServiceProvider>();

            // ServiceProvider.Current.DataAccess.StartUnitOfWork() is not called here. It should be called in each controller action method.
            // This allows more control of when database transactions commit, which solves issues where different processes access the same
            // tables and cause transaction deadlock issues.

            // WebApi setup (includes adding the Authorization filter)
            config = new HttpConfiguration();
            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }
    }
}