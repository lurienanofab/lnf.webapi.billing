using LNF.Impl.DependencyInjection.Default;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    public class TestBase
    {
        [TestInitialize]
        public void TestInit()
        {
            var ioc = new IOC();
            ServiceProvider.Configure(ioc.Resolver);
        }
    }
}
