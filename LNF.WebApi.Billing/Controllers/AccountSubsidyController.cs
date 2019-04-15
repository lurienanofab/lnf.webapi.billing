using LNF.Models.Billing;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class AccountSubsidyController : ApiController
    {
        [Route("account-subsidy")]
        public IEnumerable<IAccountSubsidy> GetAccountSubsidy(int? accountId = null)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.AccountSubsidy.GetAccountSubsidy(accountId);
        }

        [Route("account-subsidy/active")]
        public IEnumerable<IAccountSubsidy> GetActiveAccountSubsidy(DateTime sd, DateTime ed)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.AccountSubsidy.GetActiveAccountSubsidy(sd, ed);
        }

        [HttpGet, Route("account-subsidy/disable/{accountSubsidyId}")]
        public bool DisableAccountSubsidy(int accountSubsidyId)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.AccountSubsidy.DisableAccountSubsidy(accountSubsidyId);
        }

        [HttpPost, Route("account-subsidy")]
        public int AddAccountSubsidy([FromBody] AccountSubsidyItem model)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.AccountSubsidy.AddAccountSubsidy(model);
        }
    }
}
