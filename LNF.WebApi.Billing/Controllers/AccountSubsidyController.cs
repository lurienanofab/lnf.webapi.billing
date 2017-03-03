using LNF.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using System;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class AccountSubsidyController : ApiController
    {
        [Route("account-subsidy")]
        public AccountSubsidy[] GetAccountSubsidy(DateTime sd, DateTime ed)
        {
            return AccountSubsidyUtility.GetActive(sd, ed).ToArray();
        }

        [HttpGet, Route("account-subsidy/disable/{id}")]
        public AccountSubsidy DisableAccountSubsidy(int id)
        {
            var entity = DA.Current.Single<AccountSubsidy>(id);
            entity.DisableDate = DateTime.Now.Date.AddDays(1);
            return entity;
        }

        [HttpPost, Route("account-subsidy")]
        public AccountSubsidy AddAccountSubsidy([FromBody] AccountSubsidy model)
        {
            var existing = DA.Current.Query<AccountSubsidy>().Where(x => x.AccountID == model.AccountID && x.DisableDate == null);

            if (existing != null && existing.Count() > 0)
            {
                foreach (var item in existing)
                    item.DisableDate = model.EnableDate;
            }

            var entity = new AccountSubsidy()
            {
                AccountID = model.AccountID,
                UserPaymentPercentage = model.UserPaymentPercentage,
                CreatedDate = DateTime.Now,
                EnableDate = model.EnableDate,
            };

            DA.Current.Insert(entity);

            return entity;
        }
    }
}
