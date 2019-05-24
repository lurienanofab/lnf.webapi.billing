using LNF.Models.Billing;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class MiscController : ApiController
    {
        [Route("misc")]
        public IEnumerable<IMiscBillingCharge> GetMiscBillingCharges(DateTime period, int? clientId = null, bool? active = null)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Misc.GetMiscBillingCharges(period, clientId, active);
        }

        [Route("misc/{expId}")]
        public IMiscBillingCharge GetMiscBillingCharge(int expId)
        {
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Misc.GetMiscBillingCharge(expId);
        }

        [HttpPost, Route("misc/create")]
        public int CreateMiscBilling([FromBody] MiscBillingChargeCreateArgs args)
        {
            // Always recalculate subsidy after creating a new MiscBillingCharge.
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Misc.CreateMiscBillingCharge(args);
        }

        [HttpPost, Route("misc/update")]
        public int UpdateMiscBilling([FromBody] MiscBillingChargeUpdateArgs args)
        {
            // Always recalculate subsidy after updating a MiscBillingCharge.
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Misc.UpdateMiscBilling(args);
        }

        [Route("misc/delete/{expId}")]
        public int DeleteMiscBillingCharge(int expId)
        {
            // the stored proc sselData.dbo.MiscBillingCharge_Delete does a real delete

            // Always recalculate subsidy after deleting a MiscBillingCharge.
            using (DA.StartUnitOfWork())
                return ServiceProvider.Current.Billing.Misc.DeleteMiscBillingCharge(expId);
        }
    }
}
