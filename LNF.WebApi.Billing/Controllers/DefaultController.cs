using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            // updates all billing

            DateTime startTime = DateTime.Now;

            var result = new List<string>
            {
                $"Started at {startTime:yyyy-MM-dd HH:mm:ss}"
            };

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
                    var toolDataClean = new WriteToolDataCleanProcess(sd, ed, args.ClientID);
                    var toolData = new WriteToolDataProcess(sd, args.ClientID, args.ResourceID);

                    sw.Restart();
                    toolDataClean.Start();
                    result.Add(string.Format("Completed ToolDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    toolData.Start();
                    result.Add(string.Format("Completed ToolData in {0}", sw.Elapsed));

                    sw.Restart();
                    BillingDataProcessStep1.PopulateToolBilling(sd, args.ClientID, isTemp);
                    result.Add(string.Format("Completed ToolBilling in {0}", sw.Elapsed));

                    populateSubsidy = true;
                }

                if (args.BillingCategory.HasFlag(BillingCategory.Room))
                {
                    var roomDataClean = new WriteRoomDataCleanProcess(sd, ed, args.ClientID);
                    var roomData = new WriteRoomDataProcess(sd, args.ClientID, args.RoomID);

                    sw.Restart();
                    roomDataClean.Start();
                    result.Add(string.Format("Completed RoomDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    roomData.Start();
                    result.Add(string.Format("Completed RoomData in {0}", sw.Elapsed));

                    sw.Restart();
                    BillingDataProcessStep1.PopulateRoomBilling(sd, args.ClientID, isTemp);
                    result.Add(string.Format("Completed RoomBilling in {0}", sw.Elapsed));

                    populateSubsidy = true;
                }

                if (args.BillingCategory.HasFlag(BillingCategory.Store))
                {
                    var storeDataClean = new WriteStoreDataCleanProcess(sd, ed, args.ClientID);
                    var storeData = new WriteStoreDataProcess(sd, args.ClientID, args.ItemID);

                    sw.Restart();
                    storeDataClean.Start();
                    result.Add(string.Format("Completed StoreDataClean in {0}", sw.Elapsed));

                    sw.Restart();
                    storeData.Start();
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
        public UpdateClientBillingResult UpdateClientBilling(UpdateClientBillingCommand model)
        {
            DateTime now = DateTime.Now;

            DateTime sd = model.Period;
            DateTime ed = model.Period.AddMonths(1);

            var toolDataClean = new WriteToolDataCleanProcess(sd, ed, model.ClientID);
            var toolData = new WriteToolDataProcess(sd, model.ClientID, 0);

            var roomDataClean = new WriteRoomDataCleanProcess(sd, ed, model.ClientID);
            var roomData = new WriteRoomDataProcess(sd, model.ClientID, 0);

            var pr1 = toolDataClean.Start();
            var pr2 = roomDataClean.Start();

            var pr3 = toolData.Start();
            var pr4 = roomData.Start();

            bool isTemp = DateTime.Now.FirstOfMonth() == model.Period;

            var pr5 = BillingDataProcessStep1.PopulateToolBilling(model.Period, model.ClientID, isTemp);
            var pr6 = BillingDataProcessStep1.PopulateRoomBilling(model.Period, model.ClientID, isTemp);

            PopulateSubsidyBillingProcessResult pr7 = null;

            if (!isTemp)
                pr7 = BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.Period, model.ClientID);

            return new UpdateClientBillingResult
            {
                WriteToolDataCleanProcessResult = pr1,
                WriteRoomDataCleanProcessResult = pr2,
                WriteToolDataProcessResult = pr3,
                WriteRoomDataProcessResult = pr4,
                PopulateToolBillingProcessResult = pr5,
                PopulateRoomBillingProcessResult = pr6,
                PopulateSubsidyBillingProcessResult = pr7
            };
        }
    }
}
