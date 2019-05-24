using LNF.Impl.Context;
using Moq;
using System.Web;

namespace LNF.WebApi.Billing.Tests
{
    internal class ContextFactory : IHttpContextFactory
    {
        public HttpContextBase CreateContext()
        {
            var mock = new Mock<HttpContextBase>();
            return mock.Object;
        }
    }
}
