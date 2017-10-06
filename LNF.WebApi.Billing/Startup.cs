using Microsoft.Owin;

[assembly: OwinStartup(typeof(LNF.WebApi.Billing.Startup))]

namespace LNF.WebApi.Billing
{
    /// <summary>
    /// This class must be local to the application or there is an issue with routing when IIS resets.
    /// </summary>
    public class Startup : ApiOwinStartup { }
}