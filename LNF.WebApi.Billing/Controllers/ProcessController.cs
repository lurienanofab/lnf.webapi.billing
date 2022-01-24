using LNF.Billing;
using LNF.Billing.Process;
using LNF.Impl.Billing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for billing processes
    /// </summary>
    public class ProcessController : BillingApiController
    {
        public ProcessController(IProvider provider) : base(provider) { }

        /// <summary>
        /// This is the entry point for billing data. Source data is retrieved from external systems (i.e. Prowatch, Scheduler, or Store) and loaded into the appropriate DataClean table. Some initial data scrubbing occurs to prepare the data for the remaining billing processes
        /// </summary>
        [HttpPost, Route("process/data/clean")]
        public DataCleanResult BillingProcessDataClean([FromBody] DataCleanCommand model)
        {
            var result = Provider.Billing.Process.DataClean(model);
            return result;
        }

        /// <summary>
        /// The second staging process where data is selected from the DataClean table and further compiled before being inserted into the Data table. After this process is complete the data is ready to be used for billing.
        /// </summary>
        [HttpPost, Route("process/data")]
        public DataResult BillingProcessData([FromBody] DataCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Data(model);
        }

        /// <summary>
        /// The process that loads data from the Data table into the Billing table. Final data cleanup and compilation takes place
        /// </summary>
        [HttpPost, Route("process/step1")]
        public Step1Result BillingProcessStep1([FromBody] Step1Command model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Step1(model);
        }

        [Obsolete, HttpPost, Route("process/step2")]
        public BillingProcessResult BillingProcessStep2([FromBody] Step2Command model)
        {
            if (model.Period == default)
                throw new Exception("Missing parameter: Period");

            var startedAt = DateTime.Now;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var step2 = new BillingDataProcessStep2(conn);

                var results = new List<ProcessResult>();
                DateTime start;
                int rowsLoaded;

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateToolBillingByAccount(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateToolBillingByAccount", start)
                    {
                        RowsLoaded = rowsLoaded
                    });

                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateToolBillingByToolOrg(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateToolBillingByToolOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateRoomBillingByAccount(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateRoomBillingByAccount", start)
                    {
                        RowsLoaded = rowsLoaded
                    });

                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateRoomBillingByRoomOrg(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateRoomBillingByRoomOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                if ((model.BillingCategory & BillingCategory.Store) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateStoreBillingByAccount(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateStoreBillingByAccount", start)
                    {
                        RowsLoaded = rowsLoaded
                    });

                    start = DateTime.Now;
                    rowsLoaded = step2.PopulateStoreBillingByItemOrg(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateStoreBillingByItemOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                conn.Close();

                var result = new BillingProcessResult("BillingProcessStep2", startedAt, results);
  
                return result;
            }
        }

        [Obsolete, HttpPost, Route("process/step3")]
        public BillingProcessResult BillingProcessStep3([FromBody] Step3Command model)
        {
            if (model.Period == default)
                throw new Exception("Missing parameter: Period");

            var startedAt = DateTime.Now;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var step3 = new BillingDataProcessStep3(conn);

                var results = new List<ProcessResult>();
                DateTime start;
                int rowsLoaded;

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step3.PopulateToolBillingByOrg(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateToolBillingByOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step3.PopulateRoomBillingByOrg(model.Period, model.ClientID);
                    results.Add(new BillingProcessResult("PopulateRoomBillingByOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    start = DateTime.Now;
                    rowsLoaded = step3.PopulateStoreBillingByOrg(model.Period);
                    results.Add(new BillingProcessResult("PopulateStoreBillingByOrg", start)
                    {
                        RowsLoaded = rowsLoaded
                    });
                }

                conn.Close();

                var result = new BillingProcessResult("BillingProcessStep3", startedAt, results);

                return result;
            }
        }

        /// <summary>
        /// Processes Billing data to determine correct subsidy amounts. [Note: Steps 2 and 3 are obsolete and should not be used.]
        /// </summary>
        [HttpPost, Route("process/step4")]
        public PopulateSubsidyBillingResult BillingProcessStep4([FromBody] Step4Command model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Step4(model);
        }

        /// <summary>
        /// Execute finanlization steps on the Data tables
        /// </summary>
        [HttpPost, Route("process/data/finalize")]
        public FinalizeResult BillingProcessDataFinalize([FromBody] FinalizeCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Finalize(model);
        }

        /// <summary>
        /// Perfoms the daily load of data into the Data and DataClean tables
        /// </summary>
        [HttpPost, Route("process/data/update")]
        public UpdateResult BillingProcessDataUpdate([FromBody] UpdateCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Update(model);
        }

        /// <summary>
        /// Updates the BillingType to Remote for new remote processing entries
        /// </summary>
        [HttpPost, Route("process/remote-processing-update")]
        public bool RemoteProcessingUpdate([FromBody] RemoteProcessingUpdate model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.RemoteProcessingUpdate(model);
        }

        /// <summary>
        /// Delete rows from Data tables (Tool, Room, Store).
        /// </summary>
        [HttpDelete, Route("process/data/{billingCategory}")]
        public int DeleteData(BillingCategory billingCategory, DateTime period, int clientId = 0, int record = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.DeleteData(billingCategory, period, clientId, record);
        }
    }
}
