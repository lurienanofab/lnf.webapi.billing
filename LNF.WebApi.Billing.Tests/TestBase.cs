using LNF.Impl.DependencyInjection.Default;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNF.WebApi.Billing.Tests
{
    public class TestBase
    {
        [TestInitialize]
        public void TestInit()
        {
            var ioc = new IOC(new ContextFactory());
            ServiceProvider.Current = ioc.Resolver.GetInstance<ServiceProvider>();
        }
    }
}
