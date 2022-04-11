using LNF.Impl.DependencyInjection;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(LNF.WebApi.Billing.Startup))]

namespace LNF.WebApi.Billing
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            // setup up dependency injection container
            ContainerContextFactory.Current.NewAsyncScopedContext();
            var context = ContainerContextFactory.Current.GetContext();

            var wcc = new WebContainerConfiguration(context);
            wcc.RegisterAllTypes();

            // setup webapi dependency injection
            config.BootstrapWebApi(context.Container);

            // mvc
            AreaRegistration.RegisterAllAreas();
            RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            RouteTable.Routes.MapMvcAttributeRoutes();

            // webapi
            WebApiConfig.Register(config);
            app.UseWebApi(config);
        }
    }
}