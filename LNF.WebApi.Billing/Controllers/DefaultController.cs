using LNF.Billing;
using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class DefaultController : ApiController
    {
        [AllowAnonymous, Route("")]
        public string Get()
        {
            return "billing-api";
        }

        [Route("test")]
        public string GetTest()
        {
            return "test";
        }

        [HttpPost, Route("update-client")]
        public BillingProcessResult UpdateClientBilling(UpdateClientBillingCommand model)
        {
            DateTime now = DateTime.Now;

            DateTime sd = model.Period;
            DateTime ed = model.Period.AddMonths(1);

            var result = new BillingProcessResult()
            {
                StartDate = sd,
                EndDate = ed,
                ClientID = model.ClientID,
                Command = "UpdateClientBilling",
                Description = "Load all billing tables for a client and date range.",
                ErrorMessage = string.Empty
            };

            try
            {
                var toolManager = WriteToolDataManager.Create(sd, ed, model.ClientID, 0);
                var roomManager = WriteRoomDataManager.Create(sd, ed, model.ClientID, 0);
                //var storeManager = WriteStoreDataManager.Create(sd, ed, model.ClientID, 0);

                toolManager.WriteToolDataClean();
                roomManager.WriteRoomDataClean();
                //storeManager.WriteStoreDataClean();

                toolManager.WriteToolData();
                roomManager.WriteRoomData();
                //storeManager.WriteStoreData();

                bool isTemp = DateTime.Now.FirstOfMonth() == model.Period;

                BillingDataProcessStep1.PopulateToolBilling(model.Period, model.ClientID, isTemp);
                BillingDataProcessStep1.PopulateRoomBilling(model.Period, model.ClientID, isTemp);
                //BillingDataProcessStep1.PopulateStoreBilling(model.Period, isTemp);

                if (!isTemp)
                    BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.Period, model.ClientID);

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.ToString();   
                result.Success = false;
            }

            result.TimeTaken = (DateTime.Now - now).TotalSeconds;

            return result;
        }

        [HttpGet, Route("tool")]
        public IEnumerable<ToolBillingModel> GetToolBilling(DateTime period, int clientId)
        {
            var items = ToolBillingUtility.SelectToolBilling(period, clientId);
            return items.Model<ToolBillingModel>();
        }

        [HttpGet, Route("room")]
        public IEnumerable<RoomBillingModel> GetRoomBilling(DateTime period, int clientId)
        {
            var items = RoomBillingUtility.SelectRoomBilling(period, clientId);
            return items.Model<RoomBillingModel>();
        }
    }
}
