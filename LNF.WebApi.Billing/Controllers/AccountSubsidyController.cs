using LNF.Billing;
using LNF.Models.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace LNF.WebApi.Billing.Controllers
{
    public class AccountSubsidyController : ApiController
    {
        protected IAccountSubsidyManager AccountSubsidyManager => ServiceProvider.Current.Use<IAccountSubsidyManager>();

        [Route("account-subsidy")]
        public IEnumerable<AccountSubsidyItem> GetAccountSubsidy(DateTime sd, DateTime ed)
        {
            using (DA.StartUnitOfWork())
                return AccountSubsidyManager.GetActive(sd, ed).AsQueryable().CreateAccountSubsidyItems();
        }

        [HttpGet, Route("account-subsidy/disable/{accountSubsidyId}")]
        public AccountSubsidyItem DisableAccountSubsidy(int accountSubsidyId)
        {
            using (DA.StartUnitOfWork())
            {
                var entity = DA.Current.Single<AccountSubsidy>(accountSubsidyId);
                entity.DisableDate = DateTime.Now.Date.AddDays(1);
                return entity.CreateAccountSubsidyItem();
            }
        }

        [HttpPost, Route("account-subsidy")]
        public AccountSubsidyItem AddAccountSubsidy([FromBody] AccountSubsidyItem model)
        {
            using (DA.StartUnitOfWork())
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

                return entity.CreateAccountSubsidyItem();
            }
        }
    }
}
