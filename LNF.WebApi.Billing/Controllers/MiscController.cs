using LNF.Billing;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class MiscController : BillingApiController
    {
        public MiscController(IProvider provider) : base(provider) { }

        [Route("misc")]
        public IEnumerable<IMiscBillingCharge> GetMiscBillingCharges(DateTime period, int clientId = 0, bool? active = null, string types = "room,tool,store")
        {
            var typesArray = types.Split(',');

            using (StartUnitOfWork())
                return Provider.Billing.Misc.GetMiscBillingCharges(period, typesArray, clientId, active: active);
        }

        [Route("misc/{expId}")]
        public IMiscBillingCharge GetMiscBillingCharge(int expId)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Misc.GetMiscBillingCharge(expId);
        }

        [HttpPost, Route("misc/create")]
        public int CreateMiscBilling([FromBody] MiscBillingChargeCreateArgs args)
        {
            // Always recalculate subsidy after creating a new MiscBillingCharge.
            using (StartUnitOfWork())
                return Provider.Billing.Misc.CreateMiscBillingCharge(args);
        }

        [HttpPost, Route("misc/update")]
        public int UpdateMiscBilling([FromBody] MiscBillingChargeUpdateArgs args)
        {
            // Always recalculate subsidy after updating a MiscBillingCharge.
            using (StartUnitOfWork())
                return Provider.Billing.Misc.UpdateMiscBilling(args);
        }

        [Route("misc/delete/{expId}")]
        public int DeleteMiscBillingCharge(int expId)
        {
            // the stored proc sselData.dbo.MiscBillingCharge_Delete does a real delete

            // Always recalculate subsidy after deleting a MiscBillingCharge.
            using (StartUnitOfWork())
                return Provider.Billing.Misc.DeleteMiscBillingCharge(expId);
        }
    }
}
