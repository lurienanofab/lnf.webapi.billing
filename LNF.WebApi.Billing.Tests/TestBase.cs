using LNF.Impl.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.SqlClient;

namespace LNF.WebApi.Billing.Tests
{
    public class TestBase
    {
        public IProvider Provider { get; private set; }

        [TestInitialize]
        public void TestInit()
        {
            var context = ContainerContextFactory.Current.NewThreadScopedContext();
            var cfg = new ThreadStaticContainerConfiguration(context);
            cfg.RegisterAllTypes();
            context.Container.Verify();
            Provider = context.GetInstance<IProvider>();
            ServiceProvider.Setup(Provider);
        }

        protected SqlConnection NewConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
        }
    }
}
