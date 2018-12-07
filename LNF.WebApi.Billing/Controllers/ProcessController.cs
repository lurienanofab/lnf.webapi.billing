using LNF.Billing;
using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Data;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for billing processes
    /// </summary>
    public class ProcessController : ApiController
    {
        protected IBillingTypeManager BillingTypeManager => ServiceProvider.Current.Use<IBillingTypeManager>();
        protected IToolBillingManager ToolBillingManager => ServiceProvider.Current.Use<IToolBillingManager>();

        /// <summary>
        /// The process that loads data from the Data table into the Billing table. Final data cleanup and compilation takes place
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step1")]
        public ProcessResult BillingProcessStep1([FromBody] BillingProcessStep1Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            int clientId = (model.ClientID == 0) ? -1 : model.ClientID;

            ProcessResult result;

            switch (model.BillingCategory)
            {
                case BillingCategory.Tool:
                    result = BillingDataProcessStep1.PopulateToolBilling(model.Period, model.ClientID, model.IsTemp);
                    break;
                case BillingCategory.Room:
                    result = BillingDataProcessStep1.PopulateRoomBilling(model.Period, model.ClientID, model.IsTemp);
                    break;
                case BillingCategory.Store:
                    result = BillingDataProcessStep1.PopulateStoreBilling(model.Period, model.IsTemp);
                    break;
                default:
                    throw new Exception($"Unknown billing category: {model.BillingCategory}");
            }

            return result;
        }

        public T BillingProcessStep1<T>(BillingProcessStep1Command model) where T : ProcessResult, new()
        {
            var result = BillingProcessStep1(model);
            return result as T;
        }

        [HttpPost, Route("process/step2")]
        public ProcessResult BillingProcessStep2([FromBody] BillingProcessStep2Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            ProcessResult result = new ProcessResult("BillingProcessStep2");

            switch (model.BillingCategory)
            {
                case BillingCategory.Tool:
                    result.RowsLoaded += BillingDataProcessStep2.PopulateToolBillingByAccount(model.Period, model.ClientID);
                    result.RowsLoaded += BillingDataProcessStep2.PopulateToolBillingByToolOrg(model.Period, model.ClientID);
                    break;
                case BillingCategory.Room:
                    result.RowsLoaded += BillingDataProcessStep2.PopulateRoomBillingByAccount(model.Period, model.ClientID);
                    result.RowsLoaded += BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(model.Period, model.ClientID);
                    break;
                case BillingCategory.Store:
                    result.RowsLoaded += BillingDataProcessStep2.PopulateStoreBillingByAccount(model.Period, model.ClientID);
                    result.RowsLoaded += BillingDataProcessStep2.PopulateStoreBillingByItemOrg(model.Period, model.ClientID);
                    break;
                default:
                    throw new Exception($"Unknown billing category: {model.BillingCategory}");
            }

            return result;
        }

        [HttpPost, Route("process/step3")]
        public ProcessResult BillingProcessStep3([FromBody] BillingProcessStep3Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            ProcessResult result = new ProcessResult("BillingProcessStep3");

            switch (model.BillingCategory)
            {
                case BillingCategory.Tool:
                    result.RowsLoaded += BillingDataProcessStep3.PopulateToolBillingByOrg(model.Period, model.ClientID);
                    break;
                case BillingCategory.Room:
                    result.RowsLoaded += BillingDataProcessStep3.PopulateRoomBillingByOrg(model.Period, model.ClientID);
                    break;
                case BillingCategory.Store:
                    result.RowsLoaded += BillingDataProcessStep3.PopulateStoreBillingByOrg(model.Period);
                    break;
                default:
                    throw new Exception($"Unknown billing category: {model.BillingCategory}");
            }

            return result;
        }

        /// <summary>
        /// Processes Billing data to determine correct subsidy amounts. [Note: Steps 2 and 3 are obsolete and should not be used.]
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step4")]
        public PopulateSubsidyBillingProcessResult BillingProcessStep4([FromBody] BillingProcessStep4Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            PopulateSubsidyBillingProcessResult result;

            switch (model.Command)
            {
                case "subsidy":
                    result = BillingDataProcessStep4Subsidy.PopulateSubsidyBilling(model.Period, model.ClientID);
                    break;
                case "distribution":
                    result = new PopulateSubsidyBillingProcessResult { Command = "distribution" };
                    BillingDataProcessStep4Subsidy.DistributeSubsidyMoneyEvenly(model.Period, model.ClientID);
                    break;
                default:
                    throw new Exception($"Unknown command: {model.Command}");
            }

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

            BillingType bt = BillingTypeManager.GetBillingType(client, acct, model.Period);
            ToolBillingManager.UpdateBillingType(client.ClientID, acct.AccountID, bt.BillingTypeID, model.Period);
            RoomBillingUtility.UpdateBillingType(client, acct, bt, model.Period);

            return true;
        }

        /// <summary>
        /// This is the entry point for billing data. Source data is retrieved from external systems (i.e. Prowatch, Scheduler, or Store) and loaded into the appropriate DataClean table. Some initial data scrubbing occurs to prepare the data for the remaining billing processes
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/clean")]
        public ProcessResult BillingProcessDataClean([FromBody] BillingProcessDataCleanCommand model)
        {
            if (model.StartDate == default(DateTime))
                throw new Exception("Missing parameter: StartDate");

            if (model.EndDate == default(DateTime))
                throw new Exception("Missing parameter: EndDate");

            if (model.EndDate <= model.StartDate)
                throw new Exception("StartDate must come before EndDate.");

            ProcessResult result;

            switch (model.BillingCategory)
            {
                case BillingCategory.Tool:
                    result = new WriteToolDataCleanProcess(model.StartDate, model.EndDate, model.ClientID).Start();
                    break;
                case BillingCategory.Room:
                    result = new WriteRoomDataCleanProcess(model.StartDate, model.EndDate, model.ClientID).Start();
                    break;
                case BillingCategory.Store:
                    result = new WriteStoreDataCleanProcess(model.StartDate, model.EndDate, model.ClientID).Start();
                    break;
                default:
                    throw new Exception($"Unknown billing category: {model.BillingCategory}");
            }

            return result;
        }

        public T BillingProcessDataClean<T>(BillingProcessDataCleanCommand model) where T : ProcessResult, new()
        {
            var result = BillingProcessDataClean(model);
            return result as T;
        }

        /// <summary>
        /// The second staging process where data is selected from the DataClean table and further compiled before being inserted into the Data table. After this process is complete the data is ready to be used for billing.
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data")]
        public ProcessResult BillingProcessData([FromBody] BillingProcessDataCommand model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            ProcessResult result;

            switch (model.BillingCategory)
            {
                case BillingCategory.Tool:
                    result = new WriteToolDataProcess(model.Period, model.ClientID, model.Record).Start();
                    break;
                case BillingCategory.Room:
                    result = new WriteRoomDataProcess(model.Period, model.ClientID, model.Record).Start();
                    break;
                case BillingCategory.Store:
                    result = new WriteStoreDataProcess(model.Period, model.ClientID, model.Record).Start();
                    break;
                default:
                    throw new Exception($"Unknown billing category: {model.BillingCategory}");
            }

            return result;
        }

        public T BillingProcessData<T>(BillingProcessDataCommand model) where T : ProcessResult, new()
        {
            ProcessResult result = BillingProcessData(model);
            return result as T;
        }

        /// <summary>
        /// Execute finanlization steps on the Data tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/finalize")]
        public DataFinalizeProcessResult BillingProcessDataFinalize([FromBody] BillingProcessDataFinalizeCommand model)
        {
            var result = new DataFinalizeProcessResult();

            DataTableManager.Finalize(model.Period);

            return result;
        }

        /// <summary>
        /// Perfoms the daily load of data into the Data and DataClean tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/update")]
        public DataUpdateProcessResult BillingProcessDataUpdate([FromBody] BillingProcessDataUpdateCommand model)
        {
            var result = new DataUpdateProcessResult();

            DataTableManager.Update(new[] { model.BillingCategory.ToString() });

            return result;
        }

        [HttpDelete, Route("process/data/{billingCategory}")]
        public int DeleteData(BillingCategory billingCategory, DateTime period, int clientId = 0, int record = 0)
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

            string sql = $"DELETE FROM {billingCategory}Data WHERE Period = @Period AND ClientID = ISNULL(@ClientID, ClientID) AND {recordParam} = ISNULL(@Record, {1})";

            int result = DA.Command(CommandType.Text)
                .Param("Period", period)
                .Param("ClientID", clientId > 0, clientId, DBNull.Value)
                .Param("Record", record > 0, record, DBNull.Value)
                .ExecuteNonQuery(sql).Value;

            return result;
        }
    }
}
