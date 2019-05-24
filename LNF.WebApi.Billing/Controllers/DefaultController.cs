using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class DefaultController : ApiController
    {
        [AllowAnonymous, Route("")]
        public string Get() => "billing-api";

        [HttpPost, Route("update")]
        public IEnumerable<string> UpdateBilling([FromBody] UpdateBillingArgs args)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Process.UpdateBilling(args);
            }
        }

        [HttpPost, Route("update-client")]
        public UpdateClientBillingResult UpdateClientBilling(UpdateClientBillingCommand model)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Process.UpdateClientBilling(model);
            }
        }
    }
}
