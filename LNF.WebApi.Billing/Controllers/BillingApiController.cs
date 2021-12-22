using LNF.DataAccess;
using System;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public abstract class BillingApiController : ApiController
    {
        public IProvider Provider { get; }
        public ISession Session => Provider.DataAccess.Session;

        public BillingApiController(IProvider provider)
        {
            Provider = provider;
        }

        public IDisposable StartUnitOfWork()
        {
            return Provider.DataAccess.StartUnitOfWork();
        }
    }
}
