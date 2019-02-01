using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class MiscController : ApiController
    {
        [Route("misc")]
        public IEnumerable<MiscBillingChargeItem> GetMiscBillingCharges(DateTime period, int? clientId = null, bool? active = null)
        {
            using (DA.StartUnitOfWork())
            {
                return DA.Current.Query<MiscBillingCharge>()
                    .Where(x => x.Period == period && x.Client.ClientID == clientId.GetValueOrDefault(x.Client.ClientID) && x.Active == active.GetValueOrDefault(x.Active))
                    .CreateMiscBillingChargeItems();
            }
        }

        [Route("misc/{expId}")]
        public MiscBillingChargeItem GetMiscBillingCharge(int expId)
        {
            using (DA.StartUnitOfWork())
                return DA.Current.Single<MiscBillingCharge>(expId).CreateMiscBillingChargeItem();
        }

        [HttpPost, Route("misc/create")]
        public int CreateMiscBilling([FromBody] MiscBillingChargeCreateArgs args)
        {
            int result;

            using (DA.StartUnitOfWork())
            {
                var mbc = new MiscBillingCharge
                {

                    Client = DA.Current.Single<Client>(args.ClientID),
                    Account = DA.Current.Single<Account>(args.AccountID),
                    SubType = args.SubType,
                    Period = args.Period,
                    ActDate = DateTime.Now,
                    Description = args.Description,
                    Quantity = args.Quantity,
                    UnitCost = args.UnitCost,
                    SubsidyDiscount = 0,
                    Active = true
                };

                DA.Current.Insert(mbc);

                result = mbc.ExpID;
            }

            // recalculate subsidy
            using (DA.StartUnitOfWork())
                BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(args.Period, args.ClientID);

            return result;
        }

        [HttpPost, Route("misc/update")]
        public int UpdateMiscBilling([FromBody] MiscBillingChargeUpdateArgs args)
        {
            DateTime newPeriod;
            DateTime oldPeriod;
            int clientId;

            using (DA.StartUnitOfWork())
            {
                var mbc = DA.Current.Single<MiscBillingCharge>(args.ExpID);

                if (mbc == null) return 0;

                newPeriod = args.Period.FirstOfMonth();
                oldPeriod = mbc.Period.FirstOfMonth();
                clientId = mbc.Client.ClientID;

                mbc.Period = newPeriod;
                mbc.Description = args.Description;
                mbc.Quantity = args.Quantity;
                mbc.UnitCost = args.UnitCost;

                DA.Current.SaveOrUpdate(mbc);
            }

            if (oldPeriod != newPeriod)
            {
                using (DA.StartUnitOfWork())
                    BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(oldPeriod, clientId);
            }

            using (DA.StartUnitOfWork())
                BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(newPeriod, clientId);

            return 1;
        }

        [Route("misc/delete/{expId}")]
        public int DeleteMiscBillingCharge(int expId)
        {
            // the stored proc sselData.dbo.MiscBillingCharge_Delete does a real delete

            DateTime period;
            int clientId;

            using (DA.StartUnitOfWork())
            {
                var mbc = DA.Current.Single<MiscBillingCharge>(expId);

                if (mbc == null) return 0;

                period = mbc.Period;
                clientId = mbc.Client.ClientID;

                DA.Current.Delete(mbc);
            }

            // recalculate subsidy
            using (DA.StartUnitOfWork())
                BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(period, clientId);

            return 1;
        }
    }
}
