using LNF.Billing;
using LNF.CommonTools;
using LNF.Logging;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Diagnostics;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for billing processes
    /// </summary>
    public class ProcessController : ApiController
    {
        private Stopwatch sw;

        private BillingProcessResult InitProcess(IProcessCommand command)
        {
            sw = Stopwatch.StartNew();

            return new BillingProcessResult()
            {
                Command = command.BillingCategory.ToString().ToLower(),
                ClientID = command.ClientID,
                StartDate = DateTime.Now,
                Success = true,
                Description = string.Empty,
                ErrorMessage = string.Empty
            };
        }

        private void CompleteProcess(BillingProcessResult result)
        {
            result.EndDate = DateTime.Now;
            result.LogText = Log.GetText();
            sw.Stop();
            result.TimeTaken = sw.Elapsed.TotalSeconds;
        }

        /// <summary>
        /// The process that loads data from the Data table into the Billing table. Final data cleanup and compilation takes place
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step1")]
        public BillingProcessResult BillingProcessStep1([FromBody] BillingProcessStep1Command model)
        {
            BillingProcessResult result = InitProcess(model);

            int clientId = (model.ClientID == 0) ? -1 : model.ClientID;

            if (model.StartPeriod != default(DateTime))
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                switch (model.BillingCategory)
                {
                    case BillingCategory.Tool:
                        result.Description = "ToolBillingStep1";
                        BillingDataProcessStep1.PopulateToolBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        break;
                    case BillingCategory.Room:
                        result.Description = "RoomBillingStep1";
                        BillingDataProcessStep1.PopulateRoomBilling(model.StartPeriod, model.ClientID, model.IsTemp);
                        break;
                    case BillingCategory.Store:
                        result.Description = "StoreBillingStep1";
                        BillingDataProcessStep1.PopulateStoreBilling(model.StartPeriod, model.IsTemp);
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown billing category: " + model.BillingCategory.ToString();
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: StartPeriod";
            }

            CompleteProcess(result);

            return result;
        }

        [HttpPost, Route("process/step2")]
        public BillingProcessResult BillingProcessStep2([FromBody] BillingProcessStep2Command model)
        {
            BillingProcessResult result = InitProcess(model);

            if (model.Period != default(DateTime))
            {
                switch (model.BillingCategory)
                {
                    case BillingCategory.Tool:
                        result.Description = "ToolBillingStep2";
                        BillingDataProcessStep2.PopulateToolBillingByAccount(model.Period, model.ClientID);
                        BillingDataProcessStep2.PopulateToolBillingByToolOrg(model.Period, model.ClientID);
                        break;
                    case BillingCategory.Room:
                        result.Description = "RoomBillingStep2";
                        BillingDataProcessStep2.PopulateRoomBillingByAccount(model.Period, model.ClientID);
                        BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(model.Period, model.ClientID);
                        break;
                    case BillingCategory.Store:
                        result.Description = "StoreBillingStep2";
                        BillingDataProcessStep2.PopulateStoreBillingByAccount(model.Period, model.ClientID);
                        BillingDataProcessStep2.PopulateStoreBillingByItemOrg(model.Period, model.ClientID);
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown billing category: " + model.BillingCategory.ToString();
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: Period";
            }

            CompleteProcess(result);

            return result;
        }

        [HttpPost, Route("process/step3")]
        public BillingProcessResult BillingProcessStep3([FromBody] BillingProcessStep3Command model)
        {
            BillingProcessResult result = InitProcess(model);

            if (model.Period != default(DateTime))
            {
                switch (model.BillingCategory)
                {
                    case BillingCategory.Tool:
                        result.Description = "ToolBillingStep3";
                        BillingDataProcessStep3.PopulateToolBillingByOrg(model.Period, model.ClientID);
                        break;
                    case BillingCategory.Room:
                        result.Description = "RoomBillingStep3";
                        BillingDataProcessStep3.PopulateRoomBillingByOrg(model.Period, model.ClientID);
                        break;
                    case BillingCategory.Store:
                        result.Description = "StoreBillingStep3";
                        BillingDataProcessStep3.PopulateStoreBillingByOrg(model.Period);
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown billing category: " + model.BillingCategory.ToString();
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: Period";
            }

            CompleteProcess(result);

            return result;
        }

        /// <summary>
        /// Processes Billing data to determine correct subsidy amounts. [Note: Steps 2 and 3 are obsolete and should not be used.]
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step4")]
        public BillingProcessResult BillingProcessStep4([FromBody] BillingProcessStep4Command model)
        {
            sw = Stopwatch.StartNew();

            BillingProcessResult result = new BillingProcessResult()
            {
                Command = model.Command,
                ClientID = model.ClientID,
                StartDate = DateTime.Now,
                Success = true,
                Description = string.Empty,
                ErrorMessage = string.Empty
            };

            if (model.Period != default(DateTime))
            {
                switch (model.Command)
                {
                    case "subsidy":
                        result.Description = "SubsidyBillingStep4";
                        BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.Period, model.ClientID);
                        break;
                    case "distribution":
                        result.Description = "SubsidyDistribution";
                        BillingDataProcessStep4Subsidy.DistributeSubsidyMoneyEvenly(model.Period, model.ClientID);
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown command: " + model.Command;
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: Period";
            }

            CompleteProcess(result);

            return result;
        }

        /// <summary>
        /// Updates the BillingType to Remote for new remote processing entries
        /// </summary>
        /// <param name="model">An update command</param>
        /// <returns>A bool value indicating success</returns>
        [HttpPost, Route("process/remote-processing-update")]
        public bool RemoteProcessingUpdate([FromBody] RemoteProcessingUpdate model)
        {
            var client = DA.Current.Single<Client>(model.ClientID);

            if (client == null)
                return false;

            var acct = DA.Current.Single<Account>(model.AccountID);

            if (acct == null)
                return false;

            BillingType bt = BillingTypeUtility.GetBillingType(client, acct, model.Period);
            ToolBillingUtility.UpdateBillingType(client, acct, bt, model.Period);
            RoomBillingUtility.UpdateBillingType(client, acct, bt, model.Period);

            return true;
        }

        /// <summary>
        /// This is the entry point for billing data. Source data is retrieved from external systems (i.e. Prowatch, Scheduler, or Store) and loaded into the appropriate DataClean table. Some initial data scrubbing occurs to prepare the data for the remaining billing processes
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/clean")]
        public BillingProcessResult BillingProcessDataClean([FromBody] BillingProcessDataCommand model)
        {
            BillingProcessResult result = InitProcess(model);

            if (model.StartPeriod != default(DateTime))
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                switch (model.BillingCategory)
                {
                    case BillingCategory.Tool:
                        result.Description = "ToolDataClean";
                        WriteToolDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteToolDataClean();
                        break;
                    case BillingCategory.Room:
                        result.Description = "RoomDataClean";
                        WriteRoomDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteRoomDataClean();
                        break;
                    case BillingCategory.Store:
                        result.Description = "StoreDataClean";
                        WriteStoreDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteStoreDataClean();
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown billing category: " + model.BillingCategory.ToString();
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: StartPeriod";
            }

            CompleteProcess(result);

            return result;
        }

        /// <summary>
        /// The second staging process where data is selected from the DataClean table and further compiled before being inserted into the Data table. After this process is complete the data is ready to be used for billing.
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data")]
        public BillingProcessResult BillingProcessData([FromBody] BillingProcessDataCommand model)
        {

            BillingProcessResult result = InitProcess(model);

            if (model.StartPeriod != default(DateTime))
            {
                if (model.EndPeriod == DateTime.MinValue)
                    model.EndPeriod = model.StartPeriod.AddMonths(1);

                switch (model.BillingCategory)
                {
                    case BillingCategory.Tool:
                        result.Description = "ToolData";
                        WriteToolDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteToolData();
                        break;
                    case BillingCategory.Room:
                        result.Description = "RoomData";
                        WriteRoomDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteRoomData();
                        break;
                    case BillingCategory.Store:
                        result.Description = "StoreData";
                        WriteStoreDataManager.Create(model.StartPeriod, model.EndPeriod, model.ClientID, model.Record).WriteStoreData();
                        break;
                    default:
                        result.Success = false;
                        result.ErrorMessage = "Unknown billing category: " + model.BillingCategory.ToString();
                        break;
                }
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "Missing parameter: StartPeriod";
            }

            CompleteProcess(result);

            return result;
        }

        /// <summary>
        /// Execute finanlization steps on the Data tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/finalize")]
        public BillingProcessResult BillingProcessDataFinalize([FromBody] BillingProcessDataFinalizeCommand model)
        {
            sw = Stopwatch.StartNew();

            BillingProcessResult result = new BillingProcessResult()
            {
                Command = "finalize",
                ClientID = 0,
                StartDate = DateTime.Now,
                Success = true,
                Description = "FinalizeDataTables",
                ErrorMessage = string.Empty
            };

            LNF.CommonTools.DataTableManager.Finalize(model.StartPeriod, model.EndPeriod);

            CompleteProcess(result);

            return result;
        }

        /// <summary>
        /// Perfoms the daily load of data into the Data and DataClean tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/update")]
        public BillingProcessResult BillingProcessDataUpdate([FromBody] BillingProcessDataUpdateCommand model)
        {
            sw = Stopwatch.StartNew();

            BillingProcessResult result = new BillingProcessResult()
            {
                Command = "update",
                ClientID = 0,
                StartDate = DateTime.Now,
                Success = true,
                Description = "UpdateDataTables",
                ErrorMessage = string.Empty
            };

            LNF.CommonTools.DataTableManager.Update(new[] { model.BillingCategory.ToString() }, model.IsDailyImport);

            CompleteProcess(result);

            return result;
        }

        [HttpDelete, Route("process/data/{billingCategory}")]
        public int DeleteData(BillingCategory billingCategory, DateTime period, int? clientId = null, int? record = null)
        {
            string recordParam = string.Empty;

            switch (billingCategory)
            {
                case BillingCategory.Tool:
                    recordParam = "ResourceID";
                    break;
                case BillingCategory.Room:
                    recordParam = "RoomID";
                    break;
                case BillingCategory.Store:
                    recordParam = "ItemID";
                    break;
                default:
                    throw new NotImplementedException();
            }

            int result = DA.Current.SqlQuery(
                string.Format("DELETE FROM {0}Data WHERE Period = :period AND ClientID = ISNULL(:clientId, ClientID) AND {1} = ISNULL(:record, {1});SELECT @@ROWCOUNT;", billingCategory, recordParam),
                new { period, clientId, record }
            ).Result<int>();

            return result;
        }
    }
}
