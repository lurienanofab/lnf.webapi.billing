using LNF.Billing;
using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [HttpPost, Route("update")]
        public IEnumerable<string> UpdateBilling([FromBody] UpdateBillingArgs args)
        {
            // updates all billing

            DateTime startTime = DateTime.Now;

            var result = new List<string>();
            result.Add(string.Format("Started at {0:yyyy-MM-dd HH:mm:ss}", startTime));

            Stopwatch sw;

            DateTime sd = args.StartDate;

            while (sd < args.EndDate)
            {
                DateTime ed = sd.AddMonths(1);

                var isTemp = (sd == DateTime.Now.FirstOfMonth());

                var populateSubsidy = false;

                sw = new Stopwatch();

                if (args.BillingCategory.HasFlag(BillingCategory.Tool))
                {
                    var tool = WriteToolDataManager.Create(sd, ed, args.ClientID, args.ResourceID);

                    sw.Restart();    
                    tool.WriteToolDataClean();
                    result.Add(string.Format("Completed ToolDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    tool.WriteToolData();
                    result.Add(string.Format("Completed ToolData in {0}", sw.Elapsed));

                    sw.Restart();
                    BillingDataProcessStep1.PopulateToolBilling(sd, args.ClientID, isTemp);
                    result.Add(string.Format("Completed ToolBilling in {0}", sw.Elapsed));

                    populateSubsidy = true;
                }

                if (args.BillingCategory.HasFlag(BillingCategory.Room))
                {
                    var room = WriteRoomDataManager.Create(sd, ed, args.ClientID, args.RoomID);

                    sw.Restart();
                    room.WriteRoomDataClean();
                    result.Add(string.Format("Completed RoomDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    room.WriteRoomData();
                    result.Add(string.Format("Completed RoomData in {0}", sw.Elapsed));

                    sw.Restart();
                    BillingDataProcessStep1.PopulateRoomBilling(sd, args.ClientID, isTemp);
                    result.Add(string.Format("Completed RoomBilling in {0}", sw.Elapsed));

                    populateSubsidy = true;
                }

                if (args.BillingCategory.HasFlag(BillingCategory.Store))
                {
                    var store = WriteStoreDataManager.Create(sd, ed, args.ClientID, args.RoomID);

                    sw.Restart();
                    store.WriteStoreDataClean();
                    result.Add(string.Format("Completed StoreDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    store.WriteStoreData();
                    result.Add(string.Format("Completed StoreData in {0}", sw.Elapsed));

                    sw.Restart();
                    BillingDataProcessStep1.PopulateStoreBilling(sd, isTemp);
                    result.Add(string.Format("Completed StoreBilling in {0}", sw.Elapsed));
                }

                if (!isTemp && populateSubsidy)
                {
                    sw.Restart();
                    BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(sd, args.ClientID);
                    result.Add(string.Format("Completed SubsidyBilling in {0}", sw.Elapsed));
                }

                sd = sd.AddMonths(1);

                sw.Stop();
            }

            result.Add(string.Format("Completed at {0:yyyy-MM-dd HH:mm:ss}, time taken: {1}", DateTime.Now, DateTime.Now - startTime));

            return result;
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
                Description = "Load all billing tables for a client and period.",
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
