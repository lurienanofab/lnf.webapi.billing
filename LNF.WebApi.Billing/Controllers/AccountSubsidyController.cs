using LNF.Billing;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class AccountSubsidyController : BillingApiController
    {
        public AccountSubsidyController(IProvider provider) : base(provider) { }

        [Route("account-subsidy")]
        public IEnumerable<IAccountSubsidy> GetAccountSubsidy(int? accountId = null)
        {
            using (StartUnitOfWork())
                return Provider.Billing.AccountSubsidy.GetAccountSubsidy(accountId);
        }

        [Route("account-subsidy/active")]
        public IEnumerable<IAccountSubsidy> GetActiveAccountSubsidy(DateTime sd, DateTime ed)
        {
            using (StartUnitOfWork())
                return Provider.Billing.AccountSubsidy.GetActiveAccountSubsidy(sd, ed);
        }

        [HttpGet, Route("account-subsidy/disable/{accountSubsidyId}")]
        public bool DisableAccountSubsidy(int accountSubsidyId)
        {
            using (StartUnitOfWork())
                return Provider.Billing.AccountSubsidy.DisableAccountSubsidy(accountSubsidyId);
        }

        [HttpPost, Route("account-subsidy")]
        public int AddAccountSubsidy([FromBody] Impl.Repository.Billing.AccountSubsidy model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.AccountSubsidy.AddAccountSubsidy(model);
        }
    }
}
