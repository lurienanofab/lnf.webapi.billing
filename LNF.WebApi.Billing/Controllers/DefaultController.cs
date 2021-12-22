using LNF.Billing;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class DefaultController : BillingApiController
    {
        public DefaultController(IProvider provider) : base(provider) { }

        [AllowAnonymous, Route("")]
        public string Get() => "billing-api";

        [HttpPost, Route("update")]
        public IEnumerable<string> UpdateBilling([FromBody] UpdateBillingArgs args)
        {
            using (StartUnitOfWork())
            {
                return Provider.Billing.Process.UpdateBilling(args);
            }
        }

        [HttpPost, Route("update-client")]
        public UpdateClientBillingResult UpdateClientBilling(UpdateClientBillingCommand model)
        {
            using (StartUnitOfWork())
            {
                return Provider.Billing.Process.UpdateClientBilling(model);
            }
        }
    }
}
