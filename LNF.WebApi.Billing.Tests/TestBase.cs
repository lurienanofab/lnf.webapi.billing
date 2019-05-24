using LNF.Impl.Context;
using LNF.Impl.DependencyInjection.Default;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    public class TestBase
    {
        [TestInitialize]
        public void TestInit()
        {
            var ctx = new WebContext(new ContextFactory());
            var ioc = new IOC(ctx);
            ServiceProvider.Current = ioc.Resolver.GetInstance<ServiceProvider>();
        }
    }
}
